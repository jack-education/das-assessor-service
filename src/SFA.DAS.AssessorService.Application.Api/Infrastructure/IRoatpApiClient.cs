using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.AssessorService.Api.Types.Models;

namespace SFA.DAS.AssessorService.Application.Api.Infrastructure
{
    public interface IRoatpApiClient
    {
        Task<IEnumerable<OrganisationSearchResult>> SearchOrganisationByName(string searchTerm, bool exactMatch);
        Task<IEnumerable<OrganisationSearchResult>> SearchOrganisationByUkprn(int ukprn);
        Task<IEnumerable<OrganisationSearchResult>> SearchOrganisationInUkrlp(int ukprn);
    }
}