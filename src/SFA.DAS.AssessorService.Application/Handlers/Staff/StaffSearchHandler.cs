﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.AssessorService.Api.Types.Models;
using SFA.DAS.AssessorService.Application.Handlers.Search;
using SFA.DAS.AssessorService.Application.Interfaces;
using SFA.DAS.AssessorService.Application.Logging;
using SFA.DAS.AssessorService.Domain.Entities;
using SFA.DAS.AssessorService.Domain.Extensions;
using SFA.DAS.AssessorService.Domain.Paging;
using SFA.DAS.AssessorService.ExternalApis.AssessmentOrgs;
using SFA.DAS.AssessorService.ExternalApis.AssessmentOrgs.Types;

namespace SFA.DAS.AssessorService.Application.Handlers.Staff
{
    public class StaffSearchHandler : IRequestHandler<StaffSearchRequest, PaginatedList<StaffSearchResult>>
    {
        private readonly IAssessmentOrgsApiClient _assessmentOrgsApiClient;
        private readonly IIlrRepository _ilrRepository;
        private readonly IStaffCertificateRepository _staffCertificateRepository;
        private readonly ILogger<SearchHandler> _logger;
        private readonly IStaffIlrRepository _staffIlrRepository;

        public StaffSearchHandler(IAssessmentOrgsApiClient assessmentOrgsApiClient,
            IIlrRepository ilrRepository, IStaffCertificateRepository staffCertificateRepository, ILogger<SearchHandler> logger, IStaffIlrRepository staffIlrRepository)
        {
            _assessmentOrgsApiClient = assessmentOrgsApiClient;
            _ilrRepository = ilrRepository;
            _staffCertificateRepository = staffCertificateRepository;
            _logger = logger;
            _staffIlrRepository = staffIlrRepository;
        }

        public async Task<PaginatedList<StaffSearchResult>> Handle(StaffSearchRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.SearchQuery))
                return new PaginatedList<StaffSearchResult>(new List<StaffSearchResult>(), 0, request.Page, 10);

            var pageSize = 10;
            var searchResult = await Search(request);
            var totalRecordCount = searchResult.TotalCount;

            if (searchResult.TotalCount == 0)
            {
                totalRecordCount = await _staffIlrRepository.CountLearnersByName(request.SearchQuery);
                searchResult.PageOfResults = await _staffIlrRepository.SearchForLearnerByName(request.SearchQuery, request.Page, pageSize);   
            }

            _logger.LogInformation(searchResult.PageOfResults.Any() ? LoggingConstants.SearchSuccess : LoggingConstants.SearchFailure);

            var searchResults = Mapper.Map<List<StaffSearchResult>>(searchResult.PageOfResults);

            searchResults = MatchUpExistingCompletedStandards(searchResults);
            searchResults = PopulateStandards(searchResults, _assessmentOrgsApiClient, _logger);

            return new PaginatedList<StaffSearchResult>(searchResults, totalRecordCount, request.Page, pageSize);
        }

        private async Task<StaffReposSearchResult> Search(StaffSearchRequest request)
        {
            // Naive decision on what is being searched.

            var regex = new Regex(@"\b(EPA|epa)[0-9]{4}\b");
            if (regex.IsMatch(request.SearchQuery))
            {
                return await _staffIlrRepository.SearchForLearnerByEpaOrgId(request);
            }

            if (request.SearchQuery.Length == 10 && long.TryParse(request.SearchQuery, out var uln))
            {
                // Search string is a long of 10 length so must be a uln.
                var sr = new StaffReposSearchResult
                {
                    TotalCount = (await _ilrRepository.SearchForLearnerByUln(uln)).Count(),
                    PageOfResults = await _ilrRepository.SearchForLearnerByUln(uln)
                };
                return sr;
            }

            if (request.SearchQuery.Length == 8 && long.TryParse(request.SearchQuery, out var certRef))
            {
                var sr = new StaffReposSearchResult
                {
                    TotalCount = (await _staffIlrRepository.SearchForLearnerByCertificateReference(request.SearchQuery)).Count(),
                    PageOfResults = await _staffIlrRepository.SearchForLearnerByCertificateReference(request.SearchQuery)
                };
                return sr;
            }

            return new StaffReposSearchResult() {PageOfResults = new List<Ilr>(), TotalCount = 0};
        }


        private List<StaffSearchResult> MatchUpExistingCompletedStandards(List<StaffSearchResult> searchResults)
        {
            _logger.LogInformation("MatchUpExistingCompletedStandards Before Get Certificates for uln from db");
            var completedCertificates = _staffCertificateRepository.GetCertificatesFor(searchResults.Select(r => r.Uln).ToArray()).Result;
            _logger.LogInformation("MatchUpExistingCompletedStandards After Get Certificates for uln from db");
            
            foreach (var searchResult in searchResults)
            {
                var certificate = completedCertificates.SingleOrDefault(s => s.StandardCode == searchResult.StandardCode && s.Uln == searchResult.Uln);
                if (certificate == null) continue;

                searchResult.CertificateReference = certificate.CertificateReference;
                searchResult.CertificateStatus = certificate.Status;
                if (searchResult.LastUpdatedAt == null)
                {
                    searchResult.LastUpdatedAt = certificate.LastUpdatedAt?.UtcToTimeZoneTime();
                }
            }
            return searchResults;
        }

        private List<StaffSearchResult> PopulateStandards(List<StaffSearchResult> searchResults, IAssessmentOrgsApiClient assessmentOrgsApiClient, ILogger<SearchHandler> logger)
        {
            var allStandards = assessmentOrgsApiClient.GetAllStandards().Result;

            foreach (var searchResult in searchResults)
            {
                var standard = allStandards.SingleOrDefault(s => s.Id == searchResult.StandardCode);
                if (standard == null)
                {
                    standard = assessmentOrgsApiClient.GetStandard(searchResult.StandardCode).Result;
                }
                searchResult.Standard = standard.Title;
                searchResult.Standard = standard.Title;
            }

            return searchResults;
        }
    }
}