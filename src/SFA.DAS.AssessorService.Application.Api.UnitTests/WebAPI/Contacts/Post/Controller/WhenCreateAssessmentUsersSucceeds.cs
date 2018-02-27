﻿namespace SFA.DAS.AssessorService.Application.Api.UnitTests.WebAPI.ContactContoller
{
    using FizzWare.NBuilder;
    using FluentAssertions;
    using Machine.Specifications;
    using SFA.DAS.AssessorService.Application.Api.Controllers;
    using SFA.DAS.AssessorService.ViewModel.Models;
    using System.Threading;
    using System.Threading.Tasks;

    [Subject("AssessorService")]
    public class WhenCreateAssessmentUsersSucceeds : WhenCreateAssessmentUsersTestBase
    {
        private static CreateContactRequest _ContactCreateViewModel;
        private static Contactl _ContactQueryViewModel;

        Establish context = () =>
        {
            Setup();

            _ContactQueryViewModel = Builder<Contactl>.CreateNew().Build();

            Mediator.Setup(q => q.Send(Moq.It.IsAny<CreateContactRequest>(), Moq.It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((_ContactQueryViewModel)));

            _ContactCreateViewModel = Builder<CreateContactRequest>.CreateNew()
                    //.With(x => x.EndPointAssessorUKPRN = 10000000)
                    .Build();

            ContactContoller = new ContactController(
                  Mediator.Object,
                  StringLocalizer.Object,                 
                  Logger.Object);
        };

        Because of = () =>
        {
            Result = ContactContoller.CreateContact(_ContactCreateViewModel).Result;
        };

        Machine.Specifications.It verify_succesfully = () =>
        {
            var result = Result as Microsoft.AspNetCore.Mvc.CreatedAtRouteResult;
            result.Should().NotBeNull();
        };
    }
}
