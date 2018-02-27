﻿namespace SFA.DAS.AssessorService.Application.Api.Validators
{
    using System;
    using AssessorService.Api.Types.Models;
    using Consts;
    using FluentValidation;
    using Interfaces;
    using Microsoft.Extensions.Localization;

    public class OrganisationUpdateViewModelValidator : AbstractValidator<UpdateOrganisationRequest>
    {
        private readonly IContactQueryRepository _contactQueryRepository;
        private readonly IStringLocalizer<OrganisationUpdateViewModelValidator> _localizer;
        private readonly IOrganisationQueryRepository _organisationQueryRepository;

        public OrganisationUpdateViewModelValidator(IStringLocalizer<OrganisationUpdateViewModelValidator> localizer,
            IContactQueryRepository contactQueryRepository,
            IOrganisationQueryRepository organisationQueryRepository
        )
        {
            _localizer = localizer;
            _contactQueryRepository = contactQueryRepository;
            _organisationQueryRepository = organisationQueryRepository;

            var organisationUpdateViewModel = new UpdateOrganisationRequest();

            RuleFor(organisation => organisation.EndPointAssessorName).NotEmpty().WithMessage(
                _localizer[ResourceMessageName.EndPointAssessorNameMustBeDefined,
                    nameof(organisationUpdateViewModel.EndPointAssessorName)].Value);
            RuleFor(organisation => organisation.PrimaryContactId).Must(HaveAssociatedPrimaryContactInContacts)
                .WithMessage(_localizer[ResourceMessageName.PrimaryContactDoesNotExist,
                    nameof(organisationUpdateViewModel.PrimaryContactId)].Value);
            RuleFor(organisation => organisation.Id).Must(AlreadyExist).WithMessage(
                _localizer[ResourceMessageName.DoesNotExist, nameof(organisationUpdateViewModel.Id)].Value);
        }

        private bool AlreadyExist(Guid id)
        {
            var result = _organisationQueryRepository.CheckIfAlreadyExists(id).Result;
            return result;
        }

        private bool HaveAssociatedPrimaryContactInContacts(Guid? primaryContactId)
        {
            if (!primaryContactId.HasValue || primaryContactId == Guid.Empty)
                return true;

            var result = _contactQueryRepository.CheckContactExists(primaryContactId.Value).Result;
            return result;
        }
    }
}