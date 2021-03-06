﻿using MediatR;
using SFA.DAS.AssessorService.Api.Types.Consts;
using SFA.DAS.AssessorService.Api.Types.Models;
using SFA.DAS.AssessorService.Api.Types.Models.Apply.Review;
using SFA.DAS.AssessorService.Application.Interfaces;
using SFA.DAS.AssessorService.ApplyTypes;
using SFA.DAS.AssessorService.Settings;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AssessorService.Application.Handlers.Apply.Review
{
    public class ReturnApplicationSequenceHandler : IRequestHandler<ReturnApplicationSequenceRequest>
    {
        private readonly IApplyRepository _applyRepository;
        private readonly IMediator _mediator;
        private readonly IEMailTemplateQueryRepository _eMailTemplateQueryRepository;
        private readonly IContactQueryRepository _contactQueryRepository;
        private readonly IWebConfiguration _config;

        private const string SERVICE_NAME = "Apprenticeship assessment service";
        private const string SERVICE_TEAM = "Apprenticeship assessment service team";

        public ReturnApplicationSequenceHandler(IApplyRepository applyRepository, IEMailTemplateQueryRepository eMailTemplateQueryRepository, IContactQueryRepository contactQueryRepository, IWebConfiguration config, IMediator mediator)
        {
            _applyRepository = applyRepository;
            _mediator = mediator;
            _eMailTemplateQueryRepository = eMailTemplateQueryRepository;
            _contactQueryRepository = contactQueryRepository;
            _config = config;
        }

        public async Task<Unit> Handle(ReturnApplicationSequenceRequest request, CancellationToken cancellationToken)
        {
            switch (request.ReturnType)
            {
                case "ReturnWithFeedback":
                    await ReturnSequenceWithFeedback(request);
                    break;
                case "Approve":
                case "ApproveWithFeedback":
                    await ApproveSequence(request);
                    break;
                default:
                    await DeclineSequence(request);
                    break;
            }

            await NotifyContact(request.ApplicationId, request.SequenceId, cancellationToken);

            return Unit.Value;
        }

        private async Task ReturnSequenceWithFeedback(ReturnApplicationSequenceRequest request)
        {
            await _applyRepository.ReturnApplicationSequence(request.ApplicationId, request.SequenceId, ApplicationSequenceStatus.FeedbackAdded, request.ReturnedBy);
        }

        private async Task ApproveSequence(ReturnApplicationSequenceRequest request)
        {
            await _applyRepository.ReturnApplicationSequence(request.ApplicationId, request.SequenceId, ApplicationSequenceStatus.Approved, request.ReturnedBy);
        }

        private async Task DeclineSequence(ReturnApplicationSequenceRequest request)
        {
            await _applyRepository.ReturnApplicationSequence(request.ApplicationId, request.SequenceId, ApplicationSequenceStatus.Declined, request.ReturnedBy);
        }

        private async Task NotifyContact(Guid applicationId, int sequenceNo, CancellationToken cancellationToken)
        {
            var application = await _applyRepository.GetApplication(applicationId);

            if (application != null)
            {
                var loginLink = $"{_config.ServiceLink}/Account/SignIn";

                if (sequenceNo == 1)
                {
                    var lastInitSubmission = application.ApplyData?.Apply.InitSubmissions.OrderByDescending(sub => sub.SubmittedAt).FirstOrDefault();

                    if (lastInitSubmission != null)
                    {
                        var contactToNotify = await _contactQueryRepository.GetContactById(lastInitSubmission.SubmittedBy);

                        var emailTemplate = await _eMailTemplateQueryRepository.GetEmailTemplate(EmailTemplateNames.APPLY_EPAO_UPDATE);
                        await _mediator.Send(new SendEmailRequest(contactToNotify.Email, emailTemplate,
                            new { ServiceName = SERVICE_NAME, ServiceTeam = SERVICE_TEAM, Contact = contactToNotify.DisplayName, LoginLink = loginLink }), cancellationToken);
                    }
                }
                else if (sequenceNo == 2)
                {
                    var standardName = application.ApplyData?.Apply.StandardName ?? string.Empty;
                    var lastStandardSubmission = application.ApplyData?.Apply.StandardSubmissions.OrderByDescending(sub => sub.SubmittedAt).FirstOrDefault();

                    if (lastStandardSubmission != null)
                    {
                        var contactToNotify = await _contactQueryRepository.GetContactById(lastStandardSubmission.SubmittedBy);

                        var emailTemplate = await _eMailTemplateQueryRepository.GetEmailTemplate(EmailTemplateNames.APPLY_EPAO_RESPONSE);
                        await _mediator.Send(new SendEmailRequest(contactToNotify.Email, emailTemplate,
                            new { ServiceName = SERVICE_NAME, ServiceTeam = SERVICE_TEAM, Contact = contactToNotify.DisplayName, standard = standardName, LoginLink = loginLink }), cancellationToken);
                    }
                }
            }
        }
    }
}
