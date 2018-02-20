﻿namespace SFA.DAS.AssessorService.Domain.Entities
{
    using System;

    public class Contact : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid OrganisationId { get; set; }
        public virtual Organisation Organisation { get; set; }

        public int EndPointAssessorContactId { get; set; }
        public string EndPointAssessorOrganisationId{ get; set; }
        public int EndPointAssessorUKPRN { get; set; }

        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string Status { get; set; }
    }
}
