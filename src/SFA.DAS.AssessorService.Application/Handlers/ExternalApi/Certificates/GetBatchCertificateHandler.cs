﻿using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.AssessorService.Api.Types.Models.ExternalApi.Certificates;
using SFA.DAS.AssessorService.Application.Interfaces;
using SFA.DAS.AssessorService.Domain.Consts;
using SFA.DAS.AssessorService.Domain.Entities;
using SFA.DAS.AssessorService.Domain.Extensions;
using SFA.DAS.AssessorService.Domain.JsonData;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AssessorService.Application.Handlers.ExternalApi.Certificates
{
    public class GetBatchCertificateHandler : IRequestHandler<GetBatchCertificateRequest, Certificate>
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly IContactQueryRepository _contactQueryRepository;
        private readonly IIlrRepository _ilrRepository;
        private readonly ILogger<GetBatchCertificateHandler> _logger;
        private readonly IStandardRepository _standardRepository;

        public GetBatchCertificateHandler(ICertificateRepository certificateRepository, IContactQueryRepository contactQueryRepository, IIlrRepository ilrRepository, IStandardRepository standardRepository, ILogger<GetBatchCertificateHandler> logger)
        {
            _certificateRepository = certificateRepository;
            _contactQueryRepository = contactQueryRepository;
            _ilrRepository = ilrRepository;
            _standardRepository = standardRepository;
            _logger = logger;
        }

        public async Task<Certificate> Handle(GetBatchCertificateRequest request, CancellationToken cancellationToken)
        {
            return await GetCertificate(request);
        }

        private async Task<Certificate> GetCertificate(GetBatchCertificateRequest request)
        {
            _logger.LogInformation("GetCertificate Before Get Certificate from db");
            Certificate certificate = await _certificateRepository.GetCertificate(request.Uln, request.StandardCode);
            var allowedCertificateStatus = new string[] { CertificateStatus.Draft, CertificateStatus.Submitted, CertificateStatus.Printed, CertificateStatus.Reprint };

            if (certificate != null && allowedCertificateStatus.Contains(certificate.Status))
            {
                var certData = JsonConvert.DeserializeObject<CertificateData>(certificate.CertificateData);

                if(string.Equals(certData.LearnerFamilyName, request.FamilyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    certificate = await ApplyStatusInformation(certificate);
                    
                    if(!EpaOutcome.Pass.Equals(certData.EpaDetails?.LatestEpaOutcome, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // As EPA has not passed, only give access to basic information & EPA Details
                        certificate = RedactCertificateInformation(certificate, true);
                    }

                    var searchingContact = await _contactQueryRepository.GetContactFromEmailAddress(request.Email);
                    var certificateContact = await GetContactFromCertificateLogs(certificate.Id, certificate.UpdatedBy, certificate.CreatedBy);

                    if (certificateContact is null || certificateContact.OrganisationId != searchingContact.OrganisationId)
                    {
                        var providedStandards = await _standardRepository.GetEpaoRegisteredStandards(searchingContact.EndPointAssessorOrganisationId, short.MaxValue, null);

                        if (providedStandards.PageOfResults.Any(s => s.StandardCode == certificate.StandardCode))
                        {
                            // Shared standard but not the EPAO who created the certificate
                            certificate = RedactCertificateInformation(certificate, false);
                        }
                        else
                        {
                            certificate = null;
                        }
                    }
                }
                else
                {
                    certificate = null;
                }
            }
            else
            {
                certificate = null;
            }

            return certificate;
        }

        private async Task<Contact> GetContactFromCertificateLogs(Guid certificateId, string fallbackUpdatedUsername, string fallbackCreatedUsername)
        {
            Contact contact = null;

            _logger.LogInformation("GetContactFromCertificateLogs Before GetCertificateLogsFor");
            var certificateLogs = await _certificateRepository.GetCertificateLogsFor(certificateId);
            certificateLogs = certificateLogs?.Where(l => l.ReasonForChange is null).ToList(); // this removes any admin changes done within staff app

            var submittedLogEntry = certificateLogs?.FirstOrDefault(l => l.Status == CertificateStatus.Submitted);
            var createdLogEntry = certificateLogs?.FirstOrDefault(l => l.Status == CertificateStatus.Draft);

            if (submittedLogEntry != null)
            {
                _logger.LogInformation("GetContactFromCertificateLogs Before Submitted LogEntry GetContact");
                contact = await _contactQueryRepository.GetContact(submittedLogEntry.Username);
            }

            if (contact is null && !string.IsNullOrEmpty(fallbackUpdatedUsername))
            {
                _logger.LogInformation("GetContactFromCertificateLogs Before Submitted Fallback GetContact");
                contact = await _contactQueryRepository.GetContact(fallbackUpdatedUsername);
            }

            if (contact is null && createdLogEntry != null)
            {
                _logger.LogInformation("GetContactFromCertificateLogs Before Created LogEntry GetContact");
                contact = await _contactQueryRepository.GetContact(createdLogEntry.Username);
            }

            if(contact is null && !string.IsNullOrEmpty(fallbackCreatedUsername))
            {
                _logger.LogInformation("GetContactFromCertificateLogs Before Created Fallback GetContact");
                contact = await _contactQueryRepository.GetContact(fallbackCreatedUsername);
            }

            return contact;
        }

        private async Task<Certificate> ApplyStatusInformation(Certificate certificate)
        {
            // certificate is track-able entity. So we have to do this in order to stop it from updating in the database
            var json = JsonConvert.SerializeObject(certificate);
            var cert = JsonConvert.DeserializeObject<Certificate>(json);

            var certificateLogs = await _certificateRepository.GetCertificateLogsFor(cert.Id);
            certificateLogs = certificateLogs?.Where(l => l.ReasonForChange is null).ToList(); // this removes any admin changes done within staff app

            var createdLogEntry = certificateLogs?.FirstOrDefault(l => l.Status == CertificateStatus.Draft);
            if (createdLogEntry != null)
            {
                var createdContact = await _contactQueryRepository.GetContact(createdLogEntry.Username);
                cert.CreatedAt = createdLogEntry.EventTime.UtcToTimeZoneTime();
                cert.CreatedBy = createdContact != null ? createdContact.DisplayName : createdLogEntry.Username;
            }

            var submittedLogEntry = certificateLogs?.FirstOrDefault(l => l.Status == CertificateStatus.Submitted);

            // NOTE: THIS IS A DATA FRIG FOR EXTERNAL API AS WE NEED SUBMITTED INFORMATION!
            if (submittedLogEntry != null)
            {
                var submittedContact = await _contactQueryRepository.GetContact(submittedLogEntry.Username);
                cert.UpdatedAt = submittedLogEntry.EventTime.UtcToTimeZoneTime();
                cert.UpdatedBy = submittedContact != null ? submittedContact.DisplayName : submittedLogEntry.Username;
            }
            else
            {
                cert.UpdatedAt = null;
                cert.UpdatedBy = null;
            }

            return cert;
        }

        private Certificate RedactCertificateInformation(Certificate certificate, bool showEpaDetails)
        {
            // certificate is track-able entity. So we have to do this in order to stop it from updating in the database
            var json = JsonConvert.SerializeObject(certificate);
            var cert = JsonConvert.DeserializeObject<Certificate>(json);
            var certData = JsonConvert.DeserializeObject<CertificateData>(certificate.CertificateData);

            CertificateData redactedData = new CertificateData
            {
                LearnerGivenNames = certData.LearnerGivenNames,
                LearnerFamilyName = certData.LearnerFamilyName,
                StandardReference = certData.StandardReference,
                StandardName = certData.StandardName,
                StandardLevel = certData.StandardLevel,
                StandardPublicationDate = certData.StandardPublicationDate,
                EpaDetails = showEpaDetails ? certData.EpaDetails : null
            };

            cert.CertificateData = JsonConvert.SerializeObject(redactedData);
            cert.CertificateReference = "";
            cert.CertificateReferenceId = null;
            cert.CreateDay = DateTime.MinValue;
            cert.CreatedAt = DateTime.MinValue;
            cert.CreatedBy = null;
            cert.UpdatedAt = null;
            cert.UpdatedBy = null;
            cert.DeletedBy = null;
            cert.DeletedAt = null;

            return cert;
        }
    }
}
