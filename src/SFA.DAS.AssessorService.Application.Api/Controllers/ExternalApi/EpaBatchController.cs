﻿using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AssessorService.Api.Types.Models;
using SFA.DAS.AssessorService.Api.Types.Models.ExternalApi.Epas;
using SFA.DAS.AssessorService.Api.Types.Models.Standards;
using SFA.DAS.AssessorService.Application.Api.Middleware;
using SFA.DAS.AssessorService.Application.Api.Properties.Attributes;
using SFA.DAS.AssessorService.Application.Api.Validators.ExternalApi.Epas;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NotFound = SFA.DAS.AssessorService.Domain.Exceptions.NotFound;

namespace SFA.DAS.AssessorService.Application.Api.Controllers.ExternalApi
{
    [Authorize(Roles = "AssessorServiceInternalAPI")]
    [ValidateBadRequest]
    [Route("api/v1/epas/batch/")]
    public class EpaBatchController : Controller
    {
        private readonly IMediator _mediator;
        private readonly CreateBatchEpaRequestValidator _createValidator;
        private readonly UpdateBatchEpaRequestValidator _updateValidator;
        private readonly DeleteBatchEpaRequestValidator _deleteValidator;

        public EpaBatchController(IMediator mediator, CreateBatchEpaRequestValidator createValidator, UpdateBatchEpaRequestValidator updateValidator, DeleteBatchEpaRequestValidator deleteValidator)
        {
            _mediator = mediator;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _deleteValidator = deleteValidator;
        }

        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<BatchEpaResponse>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, typeof(IDictionary<string, string>))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> Create([FromBody] IEnumerable<CreateBatchEpaRequest> batchRequest)
        {
            var bag = new List<BatchEpaResponse>();

            foreach (var request in batchRequest)
            {
                var validationErrors = new List<string>();
                var isRequestValid = false;

                var collatedStandard = request.StandardCode > 0 ? await GetCollatedStandard(request.StandardCode) : await GetCollatedStandard(request.StandardReference);

                if (collatedStandard != null)
                {
                    // Only fill in the missing bits...
                    if (request.StandardCode < 1)
                    {
                        request.StandardCode = collatedStandard.StandardId ?? int.MinValue;
                    }
                    else if (string.IsNullOrEmpty(request.StandardReference))
                    {
                        request.StandardReference = collatedStandard.ReferenceNumber;
                    }
                }

                var validationResult = await _createValidator.ValidateAsync(request);
                isRequestValid = validationResult.IsValid;
                validationErrors = validationResult.Errors.Select(error => error.ErrorMessage).ToList();

                var epaResponse = new BatchEpaResponse
                {
                    RequestId = request.RequestId,
                    Uln = request.Uln,
                    StandardCode = request.StandardCode,
                    StandardReference = request.StandardReference,
                    FamilyName = request.FamilyName,
                    ValidationErrors = validationErrors
                };

                if (!validationErrors.Any() && isRequestValid)
                {
                    epaResponse.EpaDetails = await _mediator.Send(request);
                }

                bag.Add(epaResponse);
            }

            return Ok(bag.ToList());
        }

        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<BatchEpaResponse>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, typeof(IDictionary<string, string>))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> Update([FromBody] IEnumerable<UpdateBatchEpaRequest> batchRequest)
        {
            var bag = new List<BatchEpaResponse>();

            foreach (var request in batchRequest)
            {
                var collatedStandard = request.StandardCode > 0 ? await GetCollatedStandard(request.StandardCode) : await GetCollatedStandard(request.StandardReference);

                if (collatedStandard != null)
                {
                    // Only fill in the missing bits...
                    if (request.StandardCode < 1)
                    {
                        request.StandardCode = collatedStandard.StandardId ?? int.MinValue;
                    }
                    else if (string.IsNullOrEmpty(request.StandardReference))
                    {
                        request.StandardReference = collatedStandard.ReferenceNumber;
                    }
                }

                var validationResult = await _updateValidator.ValidateAsync(request);
                var isRequestValid = validationResult.IsValid;
                var validationErrors = validationResult.Errors.Select(error => error.ErrorMessage).ToList();

                var epaResponse = new BatchEpaResponse
                {
                    RequestId = request.RequestId,
                    Uln = request.Uln,
                    StandardCode = request.StandardCode,
                    StandardReference = request.StandardReference,
                    FamilyName = request.FamilyName,
                    ValidationErrors = validationErrors
                };

                if (!validationErrors.Any() && isRequestValid)
                {
                    epaResponse.EpaDetails = await _mediator.Send(request);
                }

                bag.Add(epaResponse);
            }

            return Ok(bag.ToList());
        }

        [HttpDelete("{uln}/{lastname}/{standard}/{epaReference}/{ukPrn}")]
        [SwaggerResponse((int)HttpStatusCode.NoContent)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, Type = typeof(ApiResponse))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, typeof(IDictionary<string, string>))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> Delete(long uln, string lastname, string standard, string epaReference, int ukPrn)
        {
            var request = new DeleteBatchEpaRequest
            {
                Uln = uln,
                FamilyName = lastname,
                EpaReference = epaReference,
                UkPrn = ukPrn
            };

            var collatedStandard = int.TryParse(standard, out int standardCode) ? await GetCollatedStandard(standardCode) : await GetCollatedStandard(standard);

            if (collatedStandard != null)
            {
                request.StandardCode = collatedStandard.StandardId ?? int.MinValue;
                request.StandardReference = collatedStandard.ReferenceNumber;
            }

            var validationResult = await _deleteValidator.ValidateAsync(request);
            var isRequestValid = validationResult.IsValid;
            var validationErrors = validationResult.Errors.Select(error => error.ErrorMessage).ToList();

            if (!validationErrors.Any() && isRequestValid)
            {
                try
                {
                    await _mediator.Send(request);
                    return NoContent();
                }
                catch (NotFound)
                {
                    return NotFound();
                }
            }
            else
            {
                ApiResponse response = new ApiResponse((int)HttpStatusCode.Forbidden, string.Join("; ", validationErrors));
                return StatusCode(response.StatusCode, response);
            }
        }

        private async Task<StandardCollation> GetCollatedStandard(string referenceNumber)
        {
            return await _mediator.Send(new GetCollatedStandardRequest { ReferenceNumber = referenceNumber });
        }

        private async Task<StandardCollation> GetCollatedStandard(int standardId)
        {
            return await _mediator.Send(new GetCollatedStandardRequest { StandardId = standardId });
        }
    }
}