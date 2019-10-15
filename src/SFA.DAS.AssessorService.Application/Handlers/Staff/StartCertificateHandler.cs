﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Apprenticeships.Api.Types.Providers;
using SFA.DAS.AssessorService.Api.Types.Models.Certificates;
using SFA.DAS.AssessorService.Application.Interfaces;
using SFA.DAS.AssessorService.Application.Logging;
using SFA.DAS.AssessorService.Domain.Entities;
using SFA.DAS.AssessorService.Domain.JsonData;
using SFA.DAS.AssessorService.ExternalApis.AssessmentOrgs;
using SFA.DAS.AssessorService.ExternalApis.Services;

namespace SFA.DAS.AssessorService.Application.Handlers.Staff
{
    public class StartCertificateHandler : IRequestHandler<StartCertificateRequest, Certificate>
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly IIlrRepository _ilrRepository;
        private readonly IAssessmentOrgsApiClient _assessmentOrgsApiClient;
        private readonly IOrganisationQueryRepository _organisationQueryRepository;
        private readonly ILogger<StartCertificateHandler> _logger;
        private readonly IStandardService _standardService;

        public StartCertificateHandler(ICertificateRepository certificateRepository, IIlrRepository ilrRepository, IAssessmentOrgsApiClient assessmentOrgsApiClient, 
            IOrganisationQueryRepository organisationQueryRepository, ILogger<StartCertificateHandler> logger, IStandardService standardService)
        {
            _certificateRepository = certificateRepository;
            _ilrRepository = ilrRepository;
            _assessmentOrgsApiClient = assessmentOrgsApiClient;
            _organisationQueryRepository = organisationQueryRepository;
            _logger = logger;
            _standardService = standardService;
        }

        public async Task<Certificate> Handle(StartCertificateRequest request, CancellationToken cancellationToken)
        {
            var certificate = await _certificateRepository.GetCertificate(request.Uln, request.StandardCode);
            if (certificate == null)
                certificate = await CreateNewCertificate(request);
            else if(certificate.Status == Domain.Consts.CertificateStatus.Deleted)
            {
                _logger.LogInformation("CreateNewCertificate Before Get Ilr from db");
                var ilr = await _ilrRepository.Get(request.Uln, request.StandardCode);
                if (ilr != null)
                {
                    var certData = JsonConvert.DeserializeObject<CertificateData>(certificate.CertificateData);
                    certData.LearnerGivenNames = ilr.GivenNames;
                    certData.LearnerFamilyName = ilr.FamilyName;
                    certData.LearningStartDate = ilr.LearnStartDate;
                    certificate.CertificateData = JsonConvert.SerializeObject(certData);
                    certificate.IsPrivatelyFunded = false;
                    await _certificateRepository.Update(certificate, request.Username, null);
                }
            }
            return certificate;
        }

        private async Task<Certificate> CreateNewCertificate(StartCertificateRequest request)
        {
            _logger.LogInformation("CreateNewCertificate Before Get Ilr from db");
            var ilr = await _ilrRepository.Get(request.Uln, request.StandardCode);
            _logger.LogInformation("CreateNewCertificate Before Get Organisation from db");
            var organisation = await _organisationQueryRepository.GetByUkPrn(request.UkPrn);
            _logger.LogInformation("CreateNewCertificate Before Get Standard from API");
            var standard = await _standardService.GetStandard(ilr.StdCode);
            _logger.LogInformation("CreateNewCertificate Before Get Provider from API");
            var provider = await GetProviderFromUkprn(ilr.UkPrn);

            var certData = new CertificateData()
            {
                LearnerGivenNames = ilr.GivenNames,
                LearnerFamilyName = ilr.FamilyName,
                LearningStartDate = ilr.LearnStartDate,
                StandardReference = standard.ReferenceNumber,
                StandardName = standard.Title,
                StandardLevel = standard.StandardData.Level.GetValueOrDefault(),
                StandardPublicationDate = standard.StandardData.EffectiveFrom.GetValueOrDefault(),
                FullName = $"{ilr.GivenNames} {ilr.FamilyName}",
                ProviderName = provider.ProviderName,
                EpaDetails = new EpaDetails { Epas = new List<EpaRecord>() }
            };

            _logger.LogInformation("CreateNewCertificate Before create new Certificate");
            var newCertificate = await _certificateRepository.New(
                new Certificate()
                {
                    Uln = request.Uln,
                    StandardCode = request.StandardCode,
                    ProviderUkPrn = ilr.UkPrn,
                    OrganisationId = organisation.Id,
                    CreatedBy = request.Username,
                    CertificateData = JsonConvert.SerializeObject(certData),
                    Status = Domain.Consts.CertificateStatus.Draft,
                    CertificateReference = "",
                    LearnRefNumber = ilr.LearnRefNumber,
                    CreateDay = DateTime.UtcNow.Date
                });

            newCertificate.CertificateReference = newCertificate.CertificateReferenceId.ToString().PadLeft(8,'0');

            // need to update EPA Reference too
            certData.EpaDetails.EpaReference = newCertificate.CertificateReference;
            newCertificate.CertificateData = JsonConvert.SerializeObject(certData);

            _logger.LogInformation("CreateNewCertificate Before Update Cert in db");
            await _certificateRepository.Update(newCertificate, request.Username, null);

            _logger.LogInformation(LoggingConstants.CertificateStarted);
            _logger.LogInformation($"Certificate with ID: {newCertificate.Id} Started with reference of {newCertificate.CertificateReference}");

            return newCertificate;
        }

        private async Task<Provider> GetProviderFromUkprn(int ukprn)
        {
            Provider provider = null;
            try
            {
                provider = await _assessmentOrgsApiClient.GetProvider(ukprn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to get Provider from AssessmentOrgsApi. Ukprn: {ukprn}");
            }

            if (provider is null)
            {
                // see if we can get it from Organisation Table
                var org = await _organisationQueryRepository.GetByUkPrn(ukprn);

                if (org != null)
                {
                    provider = new Provider { ProviderName = org.EndPointAssessorName, Ukprn = ukprn };
                }
                else
                {
                    // see whether there are any previous certificates with this ukrpn and a ProviderName....
                    var previousProviderName = await _certificateRepository.GetPreviousProviderName(ukprn);
                    provider = new Provider { ProviderName = previousProviderName ?? "Unknown", Ukprn = ukprn };
                }
            }

            return provider;
        }
    }

    public interface ICommitmentsApi
    {
        CommitmentEmployerDetails GetCommitmentEmployerDetails(long providerId, long commitmentId);
    }

    public class CommitmentEmployerDetails
    {
        public string LegalEntityName { get; set; }
        public string LegalEntityAddress { get; set; }
    }
}