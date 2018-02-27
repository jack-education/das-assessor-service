﻿namespace SFA.DAS.AssessorService.Application.Api.Specflow.Tests.Organisations
{
    using FluentAssertions;
    using SFA.DAS.AssessorService.ViewModel.Models;
    using TechTalk.SpecFlow;
    using SFA.DAS.AssessorService.Application.Api.Specflow.Tests.Extensions;
    using System.Data;
    using Dapper;
    using System.Linq;
    using System.Collections.Generic;
    using System;
    using SFA.DAS.AssessorService.Domain.Enums;
    using System.Net.Http;
    using Newtonsoft.Json;

    [Binding]
    public sealed class WhenDeleteContact
    {
        private RestClient _restClient;
        private readonly IDbConnection _dbconnection;
        private Organisation _organisationRetrieved;
        private dynamic _contactArguments;

        public WhenDeleteContact(RestClient restClient,
            IDbConnection dbconnection)
        {
            _restClient = restClient;
            _dbconnection = dbconnection;
        }


        [When(@"I Delete a Contact")]
        public void WhenIDeleteAContact(IEnumerable<dynamic> contactArguments)
        {
            //_contactArguments = contactArguments.First();

            //var organisationCreateViewModel = new OrganisationCreateViewModel
            //{
            //    EndPointAssessorName = "Test User",
            //    EndPointAssessorOrganisationId = "9999999994432",
            //    EndPointAssessorUKPRN = 99953456,
            //    PrimaryContactId = null
            //};

            //CreateOrganisation(organisationCreateViewModel);

            //_restClient.HttpResponseMessage = _restClient.HttpClient.PostAsJsonAsync(
            //     "api/v1/organisations", organisation).Result;
            //_restClient.Result = _restClient.HttpResponseMessage.Content.ReadAsStringAsync().Result;

            //var organisationCreated = JsonConvert.DeserializeObject<OrganisationQueryViewModel>(_restClient.Result);

            //_restClient.HttpResponseMessage = _restClient.HttpClient.DeleteAsJsonAsync($"api/v1/organisations?id={organisationCreated.Id}").Result;
        }

        [Then(@"the Contact should be deleted")]
        public void ThenTheContactShouldBeDeleted()
        {
            //var organisationsCreated = _dbconnection.Query<OrganisationQueryViewModel>
            //($"Select EndPointAssessorOrganisationId, EndPointAssessorUKPRN, EndPointAssessorName, OrganisationStatus From Organisations where EndPointAssessorOrganisationId = {_contactArguments.EndPointAssessorOrganisationId}").ToList();
            //_organisationRetrieved = organisationsCreated.First();

            //_organisationRetrieved.OrganisationStatus.Should().Be(OrganisationStatus.Deleted);
        }



       

        //[When(@"I Delete an Organisation Twice")]
        //public void WhenIDeleteAnOrganisationTwice(IEnumerable<dynamic> organisations)
        //{
        //    _organisationArguments = organisations.First();

        //    var organisation = new OrganisationCreateViewModel
        //    {
        //        EndPointAssessorName = _organisationArguments.EndPointAssessorName,
        //        EndPointAssessorOrganisationId = _organisationArguments.EndPointAssessorOrganisationId.ToString(),
        //        EndPointAssessorUKPRN = Convert.ToInt32(_organisationArguments.EndPointAssessorUKPRN),
        //        PrimaryContactId = null
        //    };

        //    _restClient.HttpResponseMessage = _restClient.HttpClient.PostAsJsonAsync(
        //         "api/v1/organisations", organisation).Result;
        //    _restClient.Result = _restClient.HttpResponseMessage.Content.ReadAsStringAsync().Result;

        //    var organisationCreated = JsonConvert.DeserializeObject<OrganisationQueryViewModel>(_restClient.Result);

        //    _restClient.HttpResponseMessage = _restClient.HttpClient.DeleteAsJsonAsync($"api/v1/organisations?id={organisationCreated.Id}").Result;
        //    _restClient.HttpResponseMessage = _restClient.HttpClient.DeleteAsJsonAsync($"api/v1/organisations?id={organisationCreated.Id}").Result;
        //}

        //[Then(@"the Organisation should be deleted")]
        //public void ThenTheOrganisationShouldBeDeleted()
        //{
        //    var organisationsCreated = _dbconnection.Query<OrganisationQueryViewModel>
        //    ($"Select EndPointAssessorOrganisationId, EndPointAssessorUKPRN, EndPointAssessorName, OrganisationStatus From Organisations where EndPointAssessorOrganisationId = {_organisationArguments.EndPointAssessorOrganisationId}").ToList();
        //    _organisationRetrieved = organisationsCreated.First();

        //    _organisationRetrieved.OrganisationStatus.Should().Be(OrganisationStatus.Deleted);
        //}
    }
}
