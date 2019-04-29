﻿using SFA.DAS.AssessorService.Api.Types.Commands;
using SFA.DAS.AssessorService.Web.Staff.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.AssessorService.Api.Types.Commands;
using SFA.DAS.AssessorService.Web.Staff.Infrastructure;

namespace SFA.DAS.AssessorService.Web.Staff.Services
{
    public class AnswerService : IAnswerService
    {
        private readonly IApplyApiClient _applyApiClient;
        private readonly IApiClient _assessorApiClient;


        public AnswerService(IApplyApiClient applyApiClient, IApiClient assessorApiClient)
        {
            _applyApiClient = applyApiClient;
            _assessorApiClient = assessorApiClient;
        }

        public async Task<CreateOrganisationContactCommand> GatherAnswersForOrganisationAndContactForApplication(Guid applicationId)
        {
            var tradingName = await GetAnswer(applicationId, "trading-name");
            var useTradingNameString = await GetAnswer(applicationId, "use-trading-name");
            var contactName = await GetAnswer(applicationId, "contact-name");
            var contactAddress1 = await GetAnswer(applicationId, "contact-address") ?? await GetAnswer(applicationId, "contact-address1");
            var contactAddress2 = await GetAnswer(applicationId, "contact-address2");
            var contactAddress3 = await GetAnswer(applicationId, "contact-address3");
            var contactAddress4 = await GetAnswer(applicationId, "contact-address4");
            var contactPostcode = await GetAnswer(applicationId, "contact-postcode");
            var contactEmail = await GetAnswer(applicationId, "contact-email");
            var contactPhoneNumber = await GetAnswer(applicationId, "contact-phone-number");
            var companyUkprn = await GetAnswer(applicationId, "company-ukprn");

            var companyNumber = await GetAnswer(applicationId, "company-number");
            if ("no".Equals(companyNumber, StringComparison.InvariantCultureIgnoreCase))
            {
                companyNumber = null;
            }

            var charityNumber = await GetAnswer(applicationId, "charity-number");
            if ("no".Equals(charityNumber, StringComparison.InvariantCultureIgnoreCase))
            {
                charityNumber = null;
            }

            var standardWebsite = await GetAnswer(applicationId, "standard-website");

            var organisation = await _applyApiClient.GetOrganisationForApplication(applicationId);
            var applyContact = await _applyApiClient.GetContact(Guid.Parse(organisation.CreatedBy??Guid.Empty.ToString()));
            var application = await _applyApiClient.GetApplication(applicationId);

            var createdBy = application?.CreatedBy ?? organisation?.CreatedBy;

            var organisationName = organisation?.Name;
            var organisationType = organisation?.OrganisationType;
            var organisationUkprn = organisation?.OrganisationUkprn;
            var organisationReferenceType = organisation?.OrganisationDetails?.OrganisationReferenceType;
            var isEpaoApproved = organisation?.RoEPAOApproved;
            var useTradingName = "yes".Equals(useTradingNameString, StringComparison.InvariantCultureIgnoreCase) || "true".Equals(useTradingNameString, StringComparison.InvariantCultureIgnoreCase) || "1".Equals(useTradingNameString, StringComparison.InvariantCultureIgnoreCase);

            var financialDueDate = organisation?.OrganisationDetails?.FHADetails?.FinancialDueDate;
            var isFinancialExempt = organisation?.OrganisationDetails?.FHADetails?.FinancialExempt;

            var command = new CreateOrganisationContactCommand
            (organisationName,
                organisationType,
                organisationUkprn?.ToString(),
                organisationReferenceType,
                isEpaoApproved,
                tradingName,
                useTradingName,
                contactName,
                contactAddress1,
                contactAddress2,
                contactAddress3,
                contactAddress4,
                contactPostcode,
                contactEmail,
                contactPhoneNumber,
                companyUkprn,
                companyNumber,
                charityNumber,
                standardWebsite,
                createdBy,
                applyContact.FamilyName,
                applyContact.GivenNames,
                applyContact.SigninId,
                applyContact.SigninType,
                applyContact.Email,
                financialDueDate,
                isFinancialExempt);


            return command;
        }

        public async Task<CreateOrganisationStandardCommand> GatherAnswersForOrganisationStandardForApplication(Guid applicationId)
        {
            var application = await _applyApiClient.GetApplication(applicationId);
            var organisation = await _applyApiClient.GetOrganisationForApplication(applicationId);

            if (application is null || organisation is null) return new CreateOrganisationStandardCommand();

            var assessorOrganisation = (await _assessorApiClient.SearchOrganisations(organisation.Name)).FirstOrDefault();

            var organisationId = assessorOrganisation?.Id;
            var createdBy = application.CreatedBy ?? organisation.CreatedBy;
            var standardCode = application.ApplicationData?.StandardCode;

            var effectiveFrom = DateTime.UtcNow.Date;
            if(DateTime.TryParse(await GetAnswer(applicationId, "effective-from"), out var effectiveFromDate))
            {
                effectiveFrom = effectiveFromDate.Date;
            }
                
            var deliveryAreas = await GetAnswer(applicationId, "delivery-areas");
            

            var command = new CreateOrganisationStandardCommand
            (createdBy,
                organisationId,
                standardCode ?? 0,
                effectiveFrom,
                deliveryAreas?.Split(',').ToList());

            return command;
        }

        public async Task<string> GetAnswer(Guid applicationId, string questionTag)
        {
           var response= await _applyApiClient.GetAnswer(applicationId, questionTag);
           return response.Answer;
        }
    }
}

