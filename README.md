# ![crest](https://assets.publishing.service.gov.uk/government/assets/crests/org_crest_27px-916806dcf065e7273830577de490d5c7c42f36ddec83e907efe62086785f24fb.png) Digital Apprenticeships Service

##  EPAO Onboarding and Certification

![Build Status](https://sfa-gov-uk.visualstudio.com/_apis/public/build/definitions/c39e0c0b-7aff-4606-b160-3566f3bbce23/831/badge)

### Developer Setup

#### Requirements

- Install [Visual Studio 2017 Enterprise](https://www.visualstudio.com/downloads/) with these workloads:
    - ASP.NET and web development
    - Azure development
- Install [SQL Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- Install [Azure Storage Emulator](https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409) (Make sure you are on v5.3)
- Install [Azure Storage Explorer](http://storageexplorer.com/)
- Install [Specflow](http://specflow.org/documentation/Installation/)
- Administrator Access

#### Setup

- Create a Configuration table in your (Development) local storage account.
- Add a row to the Configuration table with fields: PartitionKey: LOCAL, RowKey: SFA.DAS.AssessorService_1.0, Data: {The contents of the local config json file}.

##### Open the solution

- Open Visual studio as an administrator
- Open the solution
- Set SFA.DAS.AssessorService.Web and SFA.DAS.AssessorService.Application.Api as startup projects
- Running the solution will launch the site and API in your browser
- JSON configuration was created to work with dotnet run

-or-

- Navigate to src/SFA.DAS.AssessorService.Web/
- run `dotnet restore`
- run `dotnet run`
- Open https://localhost:5015

- Navigate to src/SFA.DAS.AssessorService.Application.Api/
- run `dotnet restore`
- run `dotnet run`

Running Specflow

Specflow is currently used for integrations testing the Internal API.
It is configured to run using the NUnit Test runner. 

As such it requires

1). To be ran using the NUnit Test Runner.

2). The SFA.DAS.AssessorService.Application.Api project to be running in advance.

3). The BaseAddress in the app.config to be set to the base address of the running 
SFA.DAS.AssessorService.Application.Api project.

#### To run local copy you will also require 

- https://github.com/SkillsFundingAgency/das-login-service
- https://github.com/SkillsFundingAgency/das-apply-service

     
	 
	 
    


