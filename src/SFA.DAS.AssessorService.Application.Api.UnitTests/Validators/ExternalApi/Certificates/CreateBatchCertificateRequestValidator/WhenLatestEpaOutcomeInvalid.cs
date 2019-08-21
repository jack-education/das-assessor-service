﻿using FizzWare.NBuilder;
using FluentAssertions;
using FluentValidation.Results;
using NUnit.Framework;
using SFA.DAS.AssessorService.Api.Types.Models.ExternalApi.Certificates;
using SFA.DAS.AssessorService.Domain.Consts;
using SFA.DAS.AssessorService.Domain.JsonData;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.AssessorService.Application.Api.UnitTests.Validators.ExternalApi.Certificates.CreateBatchCertificateRequestValidator
{
    public class WhenLatestEpaOutcomeInvalid : CreateBatchCertificateRequestValidatorTestBase
    {
        private ValidationResult _validationResult;

        [SetUp]
        public async Task Arrange()
        {
            CreateBatchCertificateRequest request = Builder<CreateBatchCertificateRequest>.CreateNew()
                .With(i => i.Uln = 9999999999)
                .With(i => i.StandardCode = 101)
                .With(i => i.StandardReference = null)
                .With(i => i.UkPrn = 99999999)
                .With(i => i.FamilyName = "Test")
                .With(i => i.CertificateReference = null)
                .With(i => i.CertificateData = Builder<CertificateData>.CreateNew()
                                .With(cd => cd.ContactPostCode = "AA11AA")
                                .With(cd => cd.AchievementDate = DateTime.UtcNow)
                                .With(cd => cd.OverallGrade = CertificateGrade.Pass)
                                .With(cd => cd.CourseOption = null)
                                .Build())
                .Build();

            _validationResult = await Validator.ValidateAsync(request);
        }

        [Test]
        public void ThenValidationResultShouldBeFalse()
        {
            _validationResult.IsValid.Should().BeFalse();
            _validationResult.Errors.Count.Should().Be(1);
        }
    }


}
