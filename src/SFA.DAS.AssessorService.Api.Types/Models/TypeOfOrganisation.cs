using System;

namespace SFA.DAS.AssessorService.Api.Types.Models
{
    public struct TypeOfOrganisation
    {
        public int Id { get; set; }
        public string OrganisationType { get; set; }
        public string Status { get; set; }
    }
}