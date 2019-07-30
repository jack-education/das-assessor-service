﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.AssessorService.Api.Types.Models;
using SFA.DAS.AssessorService.Application.Api.Client.Clients;
using SFA.DAS.AssessorService.Application.Api.Client.Exceptions;
using SFA.DAS.AssessorService.ApplyTypes;
using SFA.DAS.AssessorService.Domain.Consts;
using SFA.DAS.AssessorService.Domain.Paging;
using SFA.DAS.AssessorService.Settings;
using SFA.DAS.AssessorService.Web.Infrastructure;
using SFA.DAS.AssessorService.Web.ViewModels.Organisation;
using FHADetails = SFA.DAS.AssessorService.ApplyTypes.FHADetails;

namespace SFA.DAS.AssessorService.Web.Controllers
{
    

    [Authorize]
    public class OrganisationSearchController : Controller
    {
        private const int PageSize = 10;
        private readonly IContactsApiClient _contactsApiClient;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IOrganisationsApiClient _organisationsApiClient;
        private readonly ILogger<OrganisationSearchController> _logger;
        private readonly IWebConfiguration _config;
        private readonly ISessionService _sessionService;

       

        public OrganisationSearchController(ILogger<OrganisationSearchController> logger,
            IHttpContextAccessor contextAccessor, IOrganisationsApiClient organisationsApiClient,
            IContactsApiClient contactApiClient,
            IWebConfiguration config,
            ISessionService sessionService)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
            _organisationsApiClient = organisationsApiClient;
            _contactsApiClient = contactApiClient;
            _config = config;
            _sessionService = sessionService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Results(OrganisationSearchViewModel viewModel, int? pageIndex)
        {
            var signinId = _contextAccessor.HttpContext.User.Claims.First(c => c.Type == "sub")?.Value;
            var user = await _contactsApiClient.GetContactBySignInId(signinId);

            if (!string.IsNullOrEmpty(user.EndPointAssessorOrganisationId) && user.OrganisationId != null &&
                user.Status == ContactStatus.Live)
                return RedirectToAction("Index", "Dashboard");

            if (string.IsNullOrEmpty(viewModel.SearchString) || viewModel.SearchString.Length < 2)
            {
                ModelState.AddModelError(nameof(viewModel.SearchString), "Enter a valid search string");
                TempData["ShowErrors"] = true;
                return View(nameof(Index));
            }

            viewModel.Organisations = await _organisationsApiClient.SearchForOrganisations(viewModel.SearchString,PageSize, SanitizePageIndex(pageIndex));

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> FromResults(OrganisationSearchViewModel viewModel, int? pageIndex)
        {
            var signinId = _contextAccessor.HttpContext.User.Claims.First(c => c.Type == "sub")?.Value;
            var user = await _contactsApiClient.GetContactBySignInId(signinId);

            if (!string.IsNullOrEmpty(user.EndPointAssessorOrganisationId) && user.OrganisationId != null &&
                user.Status == ContactStatus.Live)
                return RedirectToAction("Index", "Dashboard");

            if (string.IsNullOrEmpty(viewModel.SearchString) || viewModel.SearchString.Length < 2)
            {
                ModelState.AddModelError(nameof(viewModel.SearchString), "Enter a valid search string");
                TempData["ShowErrors"] = true;
                return View(nameof(Results), viewModel);
            }

            viewModel.Organisations = await _organisationsApiClient.SearchForOrganisations(viewModel.SearchString, PageSize, SanitizePageIndex(pageIndex));

            return View(nameof(Results), viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> NextResults(string searchString, int? pageIndex)
        {
            var viewModel = new OrganisationSearchViewModel
            {
                SearchString = searchString
            };

            if (string.IsNullOrEmpty(viewModel.SearchString) || viewModel.SearchString.Length < 2)
            {
                ModelState.AddModelError(nameof(viewModel.SearchString), "Enter a valid search string");
                TempData["ShowErrors"] = true;
                return View(nameof(Results), viewModel);
            }
            
            viewModel.Organisations = await _organisationsApiClient.SearchForOrganisations(viewModel.SearchString, PageSize, SanitizePageIndex(pageIndex));

            return View(nameof(Results), viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Confirm(OrganisationSearchViewModel viewModel)
        {

            var signinId = _contextAccessor.HttpContext.User.Claims.First(c => c.Type == "sub")?.Value;
            var user = await _contactsApiClient.GetContactBySignInId(signinId);

            if (!string.IsNullOrEmpty(user.EndPointAssessorOrganisationId) && user.OrganisationId != null &&
                user.Status == ContactStatus.Live)
                return RedirectToAction("Index", "Dashboard");

            if (string.IsNullOrEmpty(viewModel.Name) || viewModel.SearchString.Length < 2)
            {
                ModelState.AddModelError(nameof(viewModel.Name), "Enter a valid search string");
                TempData["ShowErrors"] = true;
                return RedirectToAction(nameof(Index));
            }

            if(string.IsNullOrEmpty(viewModel.OrganisationType))
            {
                ModelState.AddModelError(nameof(viewModel.OrganisationType), "Select an organisation type");
                TempData["ShowErrors"] = true;
                viewModel.OrganisationTypes = await _organisationsApiClient.GetOrganisationTypes();
                return View("Type", viewModel);
            }
           

            var organisationSearchResult = await GetOrganisation(viewModel.SearchString, viewModel.Name,
                viewModel.Ukprn, viewModel.OrganisationType, viewModel.Postcode, viewModel.PageIndex);

            if (organisationSearchResult != null)
            {
                if (organisationSearchResult.CompanyNumber != null)
                {
                    var isActivelyTrading = await _organisationsApiClient.IsCompanyActivelyTrading(organisationSearchResult.CompanyNumber);

                    if (!isActivelyTrading)
                    {
                        return View("~/Views/OrganisationSearch/CompanyNotActive.cshtml", viewModel);
                    }
                }

                viewModel.Organisations = new PaginatedList<OrganisationSearchResult>(new List<OrganisationSearchResult> { organisationSearchResult },1,1,1);
                viewModel.OrganisationTypes = await _organisationsApiClient.GetOrganisationTypes();
            }
            _sessionService.Set("OrganisationSearchViewModel", viewModel);
            return View(viewModel);

        }

        [HttpGet]
        public async Task<IActionResult> NoAccess(OrganisationSearchViewModel viewModel)
        {
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> OrganisationChosen()
        {
            var signinId = _contextAccessor.HttpContext.User.Claims.First(c => c.Type == "sub")?.Value;
            var user = await _contactsApiClient.GetContactBySignInId(signinId);
            var sessionString = _sessionService.Get("OrganisationSearchViewModel");
            if (sessionString == null)
            {
                _logger.LogInformation($"Session for OrganisationSearchViewModel requested by { user.DisplayName } has been lost. Redirecting to Search Index");
                return RedirectToAction("Index", "OrganisationSearch");
            }
            var viewModelFromSession = JsonConvert.DeserializeObject<OrganisationSearchViewModel>(sessionString);
            _sessionService.Remove("OrganisationSearchViewModel");

            var organisationSearchResult = await GetOrganisation(viewModelFromSession.SearchString, viewModelFromSession.Name,
                viewModelFromSession.Ukprn, viewModelFromSession.OrganisationType, viewModelFromSession.Postcode,null);
            viewModelFromSession.Organisations = new PaginatedList<OrganisationSearchResult>(new List<OrganisationSearchResult> { organisationSearchResult }, 1, 1, PageSize);
            viewModelFromSession.OrganisationTypes = await _organisationsApiClient.GetOrganisationTypes();

            return View("Type", viewModelFromSession);
        }

        [HttpPost]
        public async Task<IActionResult> OrganisationChosen(OrganisationSearchViewModel viewModel)
        {
            var organisationSearchResult = await GetOrganisation(viewModel.SearchString, viewModel.Name,
                viewModel.Ukprn, viewModel.OrganisationType, viewModel.Postcode, viewModel.PageIndex);
            if (organisationSearchResult != null)
            {
                if (organisationSearchResult.OrganisationReferenceType == "RoEPAO")
                {
                    return RequestAccess(viewModel, organisationSearchResult);
                }
                viewModel.Organisations = new PaginatedList<OrganisationSearchResult>(new List<OrganisationSearchResult> { organisationSearchResult }, 1, 1, PageSize);
                viewModel.OrganisationTypes = await _organisationsApiClient.GetOrganisationTypes();

                // ON-1818 do not pre-select OrganisationType
                // NOTE: ModelState overrides viewModel
                viewModel.OrganisationType = null;
                var orgTypeModelState = ModelState[nameof(viewModel.OrganisationType)];
                if (orgTypeModelState != null)
                {
                    orgTypeModelState.RawValue = viewModel.OrganisationType;
                    orgTypeModelState.Errors.Clear();
                }
                
                return View("Type", viewModel);
            }
            return View(nameof(Confirm),viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> DealingWithRequest(OrganisationSearchViewModel viewModel)
        {
            var signinId = _contextAccessor.HttpContext.User.Claims.First(c => c.Type == "sub")?.Value;
            var user = await _contactsApiClient.GetContactBySignInId(signinId);

            // Why would a new user searching for an Organisation have an EPAOrgId or an OrganisationId?
            if (!string.IsNullOrEmpty(user.EndPointAssessorOrganisationId) && user.OrganisationId != null &&
                user.Status == ContactStatus.Live)
                return RedirectToAction("Index", "Dashboard");

            var sessionString = _sessionService.Get("OrganisationSearchViewModel");
            if (sessionString != null)
                _sessionService.Remove("OrganisationSearchViewModel");

            var organisationSearchResult = await GetOrganisation(viewModel.SearchString, viewModel.Name,
                viewModel.Ukprn, viewModel.OrganisationType, viewModel.Postcode, viewModel.PageIndex);
            if (organisationSearchResult != null)
            {
                if (organisationSearchResult.CompanyNumber != null)
                {
                    var isActivelyTrading = await _organisationsApiClient.IsCompanyActivelyTrading(organisationSearchResult.CompanyNumber);

                    if (!isActivelyTrading)
                    {
                        return View("~/Views/OrganisationSearch/CompanyNotActive.cshtml", viewModel);
                    }
                }
                
                if (organisationSearchResult.OrganisationReferenceType == "RoEPAO")
                {
                    //Update assessor organisation status
                    await UpdateOrganisationStatus(organisationSearchResult, user);
                    await NotifyOrganisationUsers(organisationSearchResult, user);
                }
            }


            return View(viewModel);
        }  
     
        private async Task<OrganisationSearchResult> GetOrganisation(string searchString, string name, int? ukprn,
            string organisationType, string postcode, int? pageIndex)
        {
            var searchResultsReturned = await _organisationsApiClient.SearchForOrganisations(searchString,PageSize, SanitizePageIndex(pageIndex));
            var searchResults = searchResultsReturned.Items == null ? 
                new List<OrganisationSearchResult>().AsEnumerable() : searchResultsReturned.Items.AsEnumerable();

            // filter ukprn
            searchResults = searchResults.Where(sr =>
                !sr.Ukprn.HasValue || !ukprn.HasValue || sr.Ukprn == ukprn);

            // filter name (identical match)
            searchResults = searchResults.Where(sr =>
                sr.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            // filter organisation type
            searchResults = searchResults.Where(sr => sr.RoATPApproved || 
                (sr.OrganisationType?.Equals(organisationType, StringComparison.InvariantCultureIgnoreCase) ?? true));

            // filter postcode
            searchResults = searchResults.Where(sr =>
                string.IsNullOrEmpty(postcode) ||
                (sr.Address?.Postcode.Equals(postcode, StringComparison.InvariantCultureIgnoreCase) ?? true));

            var organisationSearchResult = searchResults.FirstOrDefault();

            if (organisationSearchResult != null)
            {
                if (organisationSearchResult.RoATPApproved  || organisationSearchResult.OrganisationType == null)
                    organisationSearchResult.OrganisationType = organisationType;
            }

            return organisationSearchResult;
        }

        private OrganisationDetails MapToOrganisationDetails(OrganisationSearchResult organisationSearchResult)
        {
           return new OrganisationDetails
            {
                OrganisationReferenceType = organisationSearchResult.OrganisationReferenceType,
                OrganisationReferenceId = organisationSearchResult.OrganisationReferenceId,
                LegalName = organisationSearchResult.LegalName,
                TradingName = organisationSearchResult.TradingName,
                ProviderName = organisationSearchResult.ProviderName,
                CompanyNumber = organisationSearchResult.CompanyNumber,
                CharityNumber = organisationSearchResult.CharityNumber,
                Address1 = organisationSearchResult.Address?.Address1,
                Address2 = organisationSearchResult.Address?.Address2,
                Address3 = organisationSearchResult.Address?.Address3,
                City = organisationSearchResult.Address?.City,
                Postcode = organisationSearchResult.Address?.Postcode,
                FHADetails = new FHADetails()
                {
                    FinancialDueDate = organisationSearchResult.FinancialDueDate,
                    FinancialExempt = organisationSearchResult.FinancialExempt
                }
            };
        }

        private async Task UpdateOrganisationStatus(OrganisationSearchResult organisationSearchResult, ContactResponse user)
        {
            OrganisationResponse registeredOrganisation;

            if (organisationSearchResult.Ukprn != null)
                registeredOrganisation =
                    await _organisationsApiClient.Get(organisationSearchResult.Ukprn.ToString());
            else
            {
                var result =
                     await _organisationsApiClient.GetEpaOrganisation(organisationSearchResult.Id);
                registeredOrganisation = new OrganisationResponse
                {
                    Id = result.Id
                };
            }

            await _contactsApiClient.UpdateOrgAndStatus(new UpdateContactWithOrgAndStausRequest(
                     user.Id.ToString(),
                     registeredOrganisation.Id.ToString(),
                     organisationSearchResult.Id,
                     ContactStatus.InvitePending));
        }

        private async Task NotifyOrganisationUsers(OrganisationSearchResult organisationSearchResult,
          ContactResponse user)
        {
            //ON-2020 Changed from reference Id to Id , since org reference Id can have multiple values 
            await _organisationsApiClient.SendEmailsToOrganisationUserManagementUsers(new NotifyUserManagementUsersRequest(
                user.DisplayName, organisationSearchResult
                    .Id, _config.ServiceLink));
        }

        private ViewResult RequestAccess(OrganisationSearchViewModel viewModel, OrganisationSearchResult organisationSearchResult)
        {
            var newViewModel = Mapper.Map<RequestAccessOrgSearchViewModel>(viewModel);
            var addressArray = new[] { organisationSearchResult.Address?.Address1, organisationSearchResult.Address?.City, organisationSearchResult.Address.Postcode };
            newViewModel.Address = string.Join(", ", addressArray.Where(s => !string.IsNullOrEmpty(s)));
            newViewModel.RoEPAOApproved = organisationSearchResult.RoEPAOApproved;
            newViewModel.OrganisationIsLive = organisationSearchResult.OrganisationIsLive;

            if (!string.IsNullOrEmpty(organisationSearchResult.CompanyNumber))
            {
                newViewModel.CompanyOrCharityDisplayText = "Company number";
                newViewModel.CompanyNumber = organisationSearchResult.CompanyNumber;
            }
            else if (!string.IsNullOrEmpty(organisationSearchResult.CharityNumber))
            {
                newViewModel.CompanyOrCharityDisplayText = "Charity number";
                newViewModel.CompanyNumber = organisationSearchResult.CharityNumber;
            }
            return View(nameof(NoAccess), newViewModel);
        }

        private int SanitizePageIndex(int? pageIndex)
        {
            return (pageIndex ?? 1) < 0 ? 1 : pageIndex ?? 1;
        }
    }
}