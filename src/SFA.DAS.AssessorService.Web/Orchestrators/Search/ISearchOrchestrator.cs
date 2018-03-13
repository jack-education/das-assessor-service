﻿using System.Threading.Tasks;
using SFA.DAS.AssessorService.Web.ViewModels.Search;

namespace SFA.DAS.AssessorService.Web.Orchestrators.Search
{
    public interface ISearchOrchestrator
    {
        Task<SearchViewModel> Search(SearchViewModel vm);
    }
}