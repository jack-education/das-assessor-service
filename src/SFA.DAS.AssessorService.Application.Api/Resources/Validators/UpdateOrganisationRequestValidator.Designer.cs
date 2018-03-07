﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SFA.DAS.AssessorService.Application.Api.Resources.Validators {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class UpdateOrganisationRequestValidator {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal UpdateOrganisationRequestValidator() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SFA.DAS.AssessorService.Application.Api.Resources.Validators.UpdateOrganisationRe" +
                            "questValidator", typeof(UpdateOrganisationRequestValidator).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot update a record that does not exists in the database.
        /// </summary>
        internal static string DoesNotExist {
            get {
                return ResourceManager.GetString("DoesNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Request must contain EndPointAssessorName.
        /// </summary>
        internal static string EndPointAssessorNameMustBeDefined {
            get {
                return ResourceManager.GetString("EndPointAssessorNameMustBeDefined", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Request must contain EndPointAssessorOrganisationId.
        /// </summary>
        internal static string EndPointAssessorOrganisationIdMustBeDefined {
            get {
                return ResourceManager.GetString("EndPointAssessorOrganisationIdMustBeDefined", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Request must contain an ID.
        /// </summary>
        internal static string IdMustExist {
            get {
                return ResourceManager.GetString("IdMustExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reqest must contain a valid UKPRN as defined in the UK Register of Learning Providers (UKRLP) is 8 digits in the format 10000000 – 99999999.
        /// </summary>
        internal static string InvalidUKPRN {
            get {
                return ResourceManager.GetString("InvalidUKPRN", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Data Error.
        /// </summary>
        internal static string PrimaryContactDoesNotExist {
            get {
                return ResourceManager.GetString("PrimaryContactDoesNotExist", resourceCulture);
            }
        }
    }
}
