﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.3.0.0
//      SpecFlow Generator Version:2.3.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SFA.DAS.AssessorService.Application.Api.Specflow.Tests.Contacts.Maintenance
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.3.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Maintain Contacts through the SFA.DAS.AssessorService.Application.Api")]
    [NUnit.Framework.CategoryAttribute("maintainContacts")]
    public partial class MaintainContactsThroughTheSFA_DAS_AssessorService_Application_ApiFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "ContactMaintenance.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Maintain Contacts through the SFA.DAS.AssessorService.Application.Api", "\tIn order to be able to Modify Contacts\r\n\tAs a System\r\n\tI want to be be able to m" +
                    "aintain Contacts", ProgrammingLanguage.CSharp, new string[] {
                        "maintainContacts"});
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Create a Contact as First User for Organisation")]
        public virtual void CreateAContactAsFirstUserForOrganisation()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create a Contact as First User for Organisation", ((string[])(null)));
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
 testRunner.Given("System Has access to the SFA.DAS.AssessmentOrgs.Api", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "UserName",
                        "DisplayName",
                        "Email",
                        "EndPointAssessorOrganisationId",
                        "EndPointAssessorUKPRN"});
            table1.AddRow(new string[] {
                        "testuser",
                        "Test",
                        "jane128@gmail.com",
                        "99998888",
                        "10038887"});
#line 9
 testRunner.When("I Create a Contact as First User for Organisation", ((string)(null)), table1, "When ");
#line 12
 testRunner.Then("the response http status should be Created", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 13
 testRunner.And("the Location Header should be set", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 14
 testRunner.And("the Contact should be created", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 15
 testRunner.And("the Contact Status should be set to Live", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 16
 testRunner.And("the Contact Organisation Status should be set to Live", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Create a Contact when other Contact Exist for Organisation")]
        public virtual void CreateAContactWhenOtherContactExistForOrganisation()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create a Contact when other Contact Exist for Organisation", ((string[])(null)));
#line 18
this.ScenarioSetup(scenarioInfo);
#line 19
 testRunner.Given("System Has access to the SFA.DAS.AssessmentOrgs.Api", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "UserName",
                        "DisplayName",
                        "Email",
                        "EndPointAssessorOrganisationId",
                        "EndPointAssessorUKPRN"});
            table2.AddRow(new string[] {
                        "testuser140",
                        "Test",
                        "jane1289@gmail.com",
                        "9999887",
                        "10038887"});
#line 20
 testRunner.When("I Create a Contact as another User for Organisation", ((string)(null)), table2, "When ");
#line 23
 testRunner.Then("the response http status should be Created", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 24
 testRunner.And("the Location Header should be set", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 25
 testRunner.And("the Contact should be created", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 26
 testRunner.And("the Contact Status should be set to Live", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 27
 testRunner.And("the Contact Organisation Status should be set to Live", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Create a Contact When already Exists")]
        public virtual void CreateAContactWhenAlreadyExists()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create a Contact When already Exists", ((string[])(null)));
#line 29
this.ScenarioSetup(scenarioInfo);
#line 30
 testRunner.Given("System Has access to the SFA.DAS.AssessmentOrgs.Api", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "UserName",
                        "DisplayName",
                        "Email",
                        "EndPointAssessorOrganisationId",
                        "EndPointAssessorUKPRN"});
            table3.AddRow(new string[] {
                        "testuser",
                        "Jean Brodie",
                        "jbrodie@gmail.com",
                        "99998899",
                        "10038887"});
#line 31
 testRunner.When("I Create a Contact That already exists", ((string)(null)), table3, "When ");
#line 34
 testRunner.Then("the response http status should be Bad Request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Update a Contact succesfully")]
        public virtual void UpdateAContactSuccesfully()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Update a Contact succesfully", ((string[])(null)));
#line 36
this.ScenarioSetup(scenarioInfo);
#line 37
 testRunner.Given("System Has access to the SFA.DAS.AssessmentOrgs.Api", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "UserName",
                        "DisplayName",
                        "Email"});
            table4.AddRow(new string[] {
                        "testuser",
                        "Test",
                        "jane1289@gmail.com"});
#line 38
 testRunner.When("I Update a Contact succesfully", ((string)(null)), table4, "When ");
#line 41
 testRunner.Then("the response http status should be No Content", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 42
 testRunner.And("the Contact Update should have occured", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Delete a Contact")]
        public virtual void DeleteAContact()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Delete a Contact", ((string[])(null)));
#line 44
this.ScenarioSetup(scenarioInfo);
#line 45
 testRunner.Given("System Has access to the SFA.DAS.AssessmentOrgs.Api", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "UserName",
                        "DisplayName",
                        "Email",
                        "EndPointAssessorOrganisationId",
                        "EndPointAssessorUKPRN"});
            table5.AddRow(new string[] {
                        "testuser123",
                        "Jean Brodie",
                        "jbrodie@gmail.com",
                        "99998899",
                        "10038887"});
#line 46
 testRunner.When("I Delete a Contact", ((string)(null)), table5, "When ");
#line 49
 testRunner.Then("the response http status should be No Content", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 50
 testRunner.And("the Contact should be deleted", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Repeat Deleting a Contact")]
        public virtual void RepeatDeletingAContact()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Repeat Deleting a Contact", ((string[])(null)));
#line 52
this.ScenarioSetup(scenarioInfo);
#line 53
 testRunner.Given("System Has access to the SFA.DAS.AssessmentOrgs.Api", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "UserName",
                        "DisplayName",
                        "Email",
                        "EndPointAssessorOrganisationId",
                        "EndPointAssessorUKPRN"});
            table6.AddRow(new string[] {
                        "testuser127",
                        "Jean Brodie",
                        "jbrodie@gmail.com",
                        "99998899",
                        "10038887"});
#line 54
 testRunner.When("I Delete a Contact Twice", ((string)(null)), table6, "When ");
#line 57
 testRunner.Then("the response http status should be Not Found", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 58
 testRunner.And("the Contact should be deleted", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion