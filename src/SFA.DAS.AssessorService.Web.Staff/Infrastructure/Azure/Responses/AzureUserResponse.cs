﻿using Newtonsoft.Json;
using SFA.DAS.AssessorService.Web.Staff.Models.Azure;
using System.Collections.Generic;

namespace SFA.DAS.AssessorService.Web.Staff.Infrastructure.Azure.Responses
{
    public class AzureUserResponse
    {
        [JsonProperty("value")]
        public List<User> Users { get; set; }
        [JsonProperty("count")]
        public int TotalCount { get; set; }
        public string NextLink { get; set; }
    }
}
