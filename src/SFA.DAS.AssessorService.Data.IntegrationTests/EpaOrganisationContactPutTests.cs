using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.AssessorService.Api.Types.Models.AO;
using SFA.DAS.AssessorService.Data.IntegrationTests.Handlers;
using SFA.DAS.AssessorService.Data.IntegrationTests.Models;
using SFA.DAS.AssessorService.Data.IntegrationTests.Services;
using SFA.DAS.AssessorService.Domain.Consts;

namespace SFA.DAS.AssessorService.Data.IntegrationTests
{
    public class EpaOrganisationContactPutTests: TestBase
    {
        private readonly DatabaseService _databaseService = new DatabaseService();
        private RegisterRepository _repository;
        private RegisterValidationRepository _validationRepository;
        private string _organisationIdCreated;
        private OrganisationModel _organisation;
        private int _organisationTypeId;
        private Guid _id;
        private Guid _contactId;
        private EpaContact _contactUpdated;
        private OrganisationContactModel _contactModel;
        private string _username;
        private EpaContact _contactBeforeChange;


        [OneTimeSetUp]
        public void SetUpOrganisationTests()
        {
            _repository = new RegisterRepository(_databaseService.WebConfiguration, new Mock<ILogger<RegisterRepository>>().Object);
            _validationRepository = new RegisterValidationRepository(_databaseService.WebConfiguration);
            _organisationIdCreated = "EPA0987";
            _organisationTypeId = 5;
            OrganisationTypeHandler.InsertRecord(new OrganisationTypeModel { Id = _organisationTypeId, Status = "new", Type = "organisation type 1" });
            _id = Guid.NewGuid();

            _organisation = new OrganisationModel
            {
                Id = _id,
                CreatedAt = DateTime.Now,
                EndPointAssessorName = "name 2",
                EndPointAssessorOrganisationId = _organisationIdCreated,
                PrimaryContact = null,
                OrganisationTypeId = _organisationTypeId,
                OrganisationData = null,
                Status = OrganisationStatus.New
            };

            _username = "username-9999";
            OrganisationHandler.InsertRecord(_organisation);
            _contactId = Guid.NewGuid();
            _contactModel = new OrganisationContactModel
            {
                Id = _contactId,
                EndPointAssessorOrganisationId = _organisationIdCreated,
                OrganisationId = _id,
                Username = _username,
                DisplayName = "Joe Cool",
                Email = "tester@test.com",
                PhoneNumber = "555 55555",
                Status = OrganisationStatus.Live
            };
            
            _contactBeforeChange = new EpaContact
            {
                Id = _contactId,
                EndPointAssessorOrganisationId = _organisationIdCreated,
                Username = _username,
                DisplayName = "Joe Cool",
                Email = "tester@test.com",
                PhoneNumber = "555 55555",
                Status = OrganisationStatus.Live   
            };
            
            _contactUpdated = new EpaContact
            {
                Id = _contactId,
                EndPointAssessorOrganisationId = _organisationIdCreated,
                Username = _username,
                DisplayName = "Joe Cool changes",
                Email = "tester@testChanged.com",
                PhoneNumber = "555 44444",
                Status = OrganisationStatus.Live                   
            };
            OrganisationContactHandler.InsertRecord(_contactModel);
        }

        [Test]
        public async Task UpdateOrganisationContactCheckNewDetails()
        {
            var contactBeforeChange = OrganisationContactHandler.GetContactById(_contactId.ToString());
            await _repository.UpdateEpaOrganisationContact(_contactUpdated,false);
            var contactAfterChange = OrganisationContactHandler.GetContactById(_contactId.ToString());
   
            _contactBeforeChange.Should().BeEquivalentTo(contactBeforeChange);
            _contactUpdated.Should().BeEquivalentTo(contactAfterChange);      
        }
        [OneTimeTearDown]
        public void TearDownOrganisationTests()
        {
            OrganisationContactHandler.DeleteRecordByUserName(_username);
            OrganisationHandler.DeleteRecordByOrganisationId(_organisationIdCreated);
            OrganisationTypeHandler.DeleteRecord(_organisationTypeId);
        }
    }
}