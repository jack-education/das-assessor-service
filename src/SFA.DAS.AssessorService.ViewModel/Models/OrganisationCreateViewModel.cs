﻿namespace SFA.DAS.AssessorService.ViewModel.Models
{
    using MediatR;
    using System;

    public class OrganisationCreateViewModel : IRequest<OrganisationQueryViewModel>
    {
        public string EndPointAssessorOrganisationId { get; set; }
        public int EndPointAssessorUKPRN { get; set; }
        public string EndPointAssessorName { get; set; }
        public int? PrimaryContactId { get; set; }
    }
}