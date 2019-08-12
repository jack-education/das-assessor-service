﻿using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AssessorService.Web.ViewModels
{
    public class SelectOrChangeContactNameViewModel : ChangeOrganisationDetailsViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a contact name")]
        public string PrimaryContact { get; set; }
        public string PrimaryContactName { get; set; }
    }
}
