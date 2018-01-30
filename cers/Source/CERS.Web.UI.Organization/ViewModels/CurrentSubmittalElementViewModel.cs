using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace CERS.Web.UI.Organization.ViewModels
{
    public class CurrentSubmittalElementViewModel
    {
        public CERS.Model.CurrentSubmittalElement CurrentSubmittalElement { get; set; }
        public bool isFirstElement { get; set; }
        public int OrganizationID { get; set; }
    }
}