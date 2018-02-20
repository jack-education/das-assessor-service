﻿namespace SFA.DAS.AssessorService.Application.Api.UnitTests.WebAPI.ContactContoller
{
    using Machine.Specifications;
    using SFA.DAS.AssessorService.Application.Api.Controllers;
    using FizzWare.NBuilder;
    using SFA.DAS.AssessorService.ViewModel.Models;
    using System.Threading.Tasks;
    using Moq;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using FluentAssertions;

    [Subject("AssessorService")]
    public class WhenGetAssessmentUsersSucceeds : WhenGetAssessmentUsersTestBase
    {
        private static IEnumerable<ContactQueryViewModel> _organisationQueryViewModels;
      
        Establish context = () =>
        {
            Setup();

            _organisationQueryViewModels = Builder<ContactQueryViewModel>.CreateListOfSize(10).Build();

            ContactRepository.Setup(q => q.GetContacts(Moq.It.IsAny<Guid>()))
                .Returns(Task.FromResult((_organisationQueryViewModels)));

            ContactQueryController = new ContactQueryController(
                Mediator.Object,
                ContactRepository.Object, 
                StringLocalizer.Object,
                Logger.Object);
        };

        Because of = () =>
        {
            Result = ContactQueryController.GetAllContactsForAnOrganisation(Guid.NewGuid()).Result;
        };

        Machine.Specifications.It verify_succesfully = () =>
        {
            var result = Result as Microsoft.AspNetCore.Mvc.OkObjectResult;
            result.Should().NotBeNull();
        };
    }
}
