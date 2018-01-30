using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UPF.ViewModel;

namespace CERS.Web.UI.Regulator.ViewModels
{
    public class InactiveAccountGridViewModel : GridViewModel
	{
        public int? AccountID { get; set; }

        [Display( Name = "Activated" )]
        public bool? Approved { get; set; }

        public int? ContactID { get; set; }

        public int? ContextEntityID { get; set; }

        public int ContextID { get; set; }

        [Display( Name = "Email" )]
        public string Email { get; set; }

        [Display( Name = "First Name" )]
        public string FirstName { get; set; }

        [Display( Name = "ID" )]
        public int ID { get; set; }

        [Display( Name = "Last Name" )]
        public string LastName { get; set; }

        public int? PID { get; set; }

        [Display( Name = "Status" )]
        public string Status { get; set; }

        public int StatusID { get; set; }

        [Display( Name = "User Name" )]
        public string UserName { get; set; }

        public DateTime? LastSignInDate { get; set; }

        [Display( Name = "Last Sign-In" )]
        public string LastSignInDateDisplay
        {
            get
            {
                return LastSignInDate.HasValue ? LastSignInDate.Value.ToString() : string.Empty;
            }
        }

        [Display( Name = "Environment" )]
        public string EnvironmentName { get; set; }

        [Display( Name = "Portal Name" )]
        public string PortalName { get; set; }
    }
}