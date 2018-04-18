﻿using System.Collections.Generic;
using SFA.DAS.AssessorService.Domain.Entities;

namespace SFA.DAS.AssessorService.Api.Types.Models
{
    using MediatR;

    public class GetEMailTemplatesRequest : IRequest<List<EMailTemplateResponse>>
    {
     
    }
}
