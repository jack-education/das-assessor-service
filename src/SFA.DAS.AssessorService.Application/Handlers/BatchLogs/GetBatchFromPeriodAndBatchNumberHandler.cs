﻿using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.AssessorService.Api.Types.Models;
using SFA.DAS.AssessorService.Application.Interfaces;

namespace SFA.DAS.AssessorService.Application.Handlers.BatchLogs
{
    public class GetBatchFromPeriodAndBatchNumberHandler : IRequestHandler<GetBatchFromPeriodAndBatchNumberRequest, BatchLogResponse>
    {

        private readonly IBatchLogRepository _batchLogRepository;

        public GetBatchFromPeriodAndBatchNumberHandler(IBatchLogRepository batchLogRepository)
        {
            _batchLogRepository = batchLogRepository;
        }

        public async Task<BatchLogResponse> Handle(GetBatchFromPeriodAndBatchNumberRequest request, CancellationToken cancellationToken)
        {
            var batchLog =
                await _batchLogRepository.GetBatchLogFromPeriodAndBatchNumber(request.Period, request.BatchNumber);  
            return batchLog;
        }
    }
}
