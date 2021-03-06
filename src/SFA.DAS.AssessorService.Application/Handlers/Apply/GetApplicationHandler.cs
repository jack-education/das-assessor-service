﻿using AutoMapper;
using MediatR;
using SFA.DAS.AssessorService.Api.Types.Models.Apply;
using SFA.DAS.AssessorService.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AssessorService.Application.Handlers.Apply
{
    public class GetApplicationHandler : IRequestHandler<GetApplicationRequest, ApplicationResponse>
    {
        private readonly IApplyRepository _applyRepository;

        public GetApplicationHandler(IApplyRepository applyRepository)
        {
            _applyRepository = applyRepository;
        }

        public async Task<ApplicationResponse> Handle(GetApplicationRequest request, CancellationToken cancellationToken)
        {
            var result =  await _applyRepository.GetApplication(request.ApplicationId);

            return Mapper.Map<Domain.Entities.Apply, ApplicationResponse>(result);
        }
    }
}
