﻿namespace SFA.DAS.AssessorService.Application.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AssessorService.Api.Types.Models;
    using Domain;

    public interface IContactRepository
    {       
        Task<Contact> CreateNewContact(ContactCreateDomainModel newContact);
        Task Update(UpdateContactRequest organisationUpdateViewModel);
        Task Delete(Guid id);
    }
}