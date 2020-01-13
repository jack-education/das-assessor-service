﻿using MediatR;
using SFA.DAS.AssessorService.Api.Types.Models.AO;
using SFA.DAS.AssessorService.ApplyTypes.CharityCommission;
using SFA.DAS.AssessorService.ApplyTypes.CompaniesHouse;
using System;

namespace SFA.DAS.AssessorService.Api.Types.Models.Register
{
    public class CreateEpaOrganisationRequest: IRequest<string>
    {
        public string Name { get; set; }   
        public long? Ukprn { get; set; }
        public int? OrganisationTypeId { get; set; }
        public string LegalName { get; set; }
        public string TradingName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string WebsiteLink { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Address4 { get; set; }
        public string Postcode { get; set; }
        public string CompanyNumber { get; set; }
        public CompaniesHouseSummary CompanySummary { get; set; } // Summary of Company info from Companies House. Similar format to that used in RoATP.
        public string CharityNumber { get; set; }
        public CharityCommissionSummary CharitySummary { get; set; } // Summary of Charity info from Charity Commission. Similar format to that used in RoATP.
        public string ProviderName { get; set; }
        public string City { get; set; }
        public string Status { get; set; }
        public string OrganisationReferenceType { get; set; } // "RoEPAO", "RoATP", "UKRLP" or "EASAPI"
        public string OrganisationReferenceId { get; set; } // CSV list of known id's
        public bool RoATPApproved { get; set; }
        public bool RoEPAOApproved { get; set; }
        public string EndPointAssessmentOrgId { get; set; }
        public FHADetails FHADetails { get; set; }
        public DateTime? FinancialDueDate { get; set; }
        public bool? FinancialExempt { get; set; }
    }
}
