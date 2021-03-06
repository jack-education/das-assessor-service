﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.AssessorService.Application.Handlers.Login;
using SFA.DAS.AssessorService.Application.Interfaces;
using SFA.DAS.AssessorService.Domain.Consts;
using SFA.DAS.AssessorService.Domain.Entities;
using SFA.DAS.AssessorService.Settings;

namespace SFA.DAS.AssessorService.Application.UnitTests.Handlers.Login
{

    public class LoginHandlerTestsBase
    {
        protected LoginHandler Handler;
        protected Mock<IOrganisationQueryRepository> OrgQueryRepository;
        protected Mock<IContactQueryRepository> ContactQueryRepository;
        protected Mock<IContactRepository> ContactRepository;
        protected Mock<IRegisterRepository> RegisterRepository;

        [SetUp]
        public void Arrange()
        {
            var config = new WebConfiguration() {Authentication = new AuthSettings() {Role = "EPA"}};


            OrgQueryRepository = new Mock<IOrganisationQueryRepository>();

            ContactQueryRepository = new Mock<IContactQueryRepository>();
            var roles = new List<ContactRole>
            {
                new ContactRole
                {
                    RoleName = "SuperUser"
                }
            };
            ContactQueryRepository.Setup(r => r.GetBySignInId(It.IsNotIn(Guid.Empty)))
                .Returns(Task.FromResult(new Contact() {Id = It.IsAny<Guid>(), Status = ContactStatus.Live,
                    OrganisationId = It.IsAny<Guid>(), Username = "Test", Email = "test@email.com" }));
            ContactQueryRepository.Setup(r => r.GetBySignInId(Guid.Empty))
                .Returns(Task.FromResult(new Contact() { Id = Guid.Empty, Status = ContactStatus.InvitePending,
                    OrganisationId = It.IsAny<Guid>(), Username = "Test", Email = "test@email.com" }));

            ContactQueryRepository.Setup(r => r.GetRolesFor(It.IsAny<Guid>())).ReturnsAsync(roles);
            OrgQueryRepository.Setup(r => r.Get(It.IsAny<Guid>())).ReturnsAsync(new Organisation
            {
                EndPointAssessorName = "SomeName",
                EndPointAssessorUkprn = 12345,
                Id = It.IsAny<Guid>(),
                Status = OrganisationStatus.New
            });

            ContactRepository = new Mock<IContactRepository>();
            ContactRepository.Setup(x => x.UpdateUserName(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(default(object)));

            RegisterRepository = new Mock<IRegisterRepository>();
            RegisterRepository.Setup(m => m.UpdateEpaOrganisationPrimaryContact(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(default(object)));

            Handler = new LoginHandler(new Mock<ILogger<LoginHandler>>().Object, config,
                OrgQueryRepository.Object, ContactQueryRepository.Object, ContactRepository.Object, RegisterRepository.Object);
        }
    }
}