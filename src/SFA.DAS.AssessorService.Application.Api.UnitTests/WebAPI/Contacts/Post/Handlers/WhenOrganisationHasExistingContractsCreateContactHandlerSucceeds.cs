﻿namespace SFA.DAS.AssessorService.Application.Api.UnitTests.WebAPI.ContactContoller.Post.Handlers
{
    using FizzWare.NBuilder;
    using FluentAssertions;
    using Machine.Specifications;
    using Moq;
    using SFA.DAS.AssessorService.Application.ContactHandlers;
    using SFA.DAS.AssessorService.Application.Interfaces;
    using SFA.DAS.AssessorService.ViewModel.Models;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    [Subject("AssessorService")]
    public class WhenOrganisationHasExistingContractsCreateContactHandlerSucceeds
    {
        private static CreateContactHandler CreateContactHandler;
        protected static Mock<IContactRepository> ContactRepositoryMock;
        protected static Mock<IOrganisationRepository> OrganisationRepositoryMock;
        protected static Mock<IOrganisationQueryRepository> OrganisationQueryRepositoryMock;
        protected static ContactCreateDomainModel ContactCreateDomainModel;
        protected static Contactl ContactQueryViewModel;
        protected static CreateContactRequest ContactCreateViewModel;
        protected static Contactl _result;

        Establish context = () =>
        {
            Bootstrapper.Initialize();

            ContactRepositoryMock = new Mock<IContactRepository>();
            OrganisationRepositoryMock = new Mock<IOrganisationRepository>();
            OrganisationQueryRepositoryMock = new Mock<IOrganisationQueryRepository>();

            OrganisationQueryRepositoryMock.Setup(q => q.CheckIfOrganisationHasContacts(Moq.It.IsAny<Guid>()))
                 .Returns(Task.FromResult((true)));

            ContactCreateDomainModel = Builder<ContactCreateDomainModel>.CreateNew().Build();
            ContactQueryViewModel = Builder<Contactl>.CreateNew().Build();
            ContactCreateViewModel = Builder<CreateContactRequest>.CreateNew().Build();

            ContactRepositoryMock.Setup(q => q.CreateNewContact(Moq.It.IsAny<ContactCreateDomainModel>()))
                        .Returns(Task.FromResult((ContactQueryViewModel)));

            ContactRepositoryMock.Setup(q => q.CreateNewContact(Moq.It.IsAny<ContactCreateDomainModel>()))
                .Returns(Task.FromResult(ContactQueryViewModel));

            CreateContactHandler = new CreateContactHandler(OrganisationRepositoryMock.Object, OrganisationQueryRepositoryMock.Object, ContactRepositoryMock.Object);
        };

        Because of = () =>
        {
            _result = CreateContactHandler.Handle(ContactCreateViewModel, new CancellationToken()).Result;
        };

        Machine.Specifications.It verify_succesfully = () =>
        {
            var result = _result as Contactl;
            result.Should().NotBeNull();
        };
    }
}

