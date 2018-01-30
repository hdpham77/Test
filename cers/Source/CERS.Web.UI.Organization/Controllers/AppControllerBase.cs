using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CERS.Security;
using CERS.Web.Mvc;
using UPF.Core.Model;

namespace CERS.Web.UI.Organization.Controllers
{
    public class AppControllerBase : OrganizationControllerBase
    {
        private const string OrganizationIdActionParameterKey = "organizationId";
        private const string CERSIDActionParameterKey = "CERSID";

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionParameters.ContainsKey(OrganizationIdActionParameterKey))
            {
                var orgId = filterContext.ActionParameters[OrganizationIdActionParameterKey];
                if (orgId != null)
                {
                    CurrentOrganizationID = Convert.ToInt32(orgId);
                }
            }

            if (filterContext.ActionParameters.ContainsKey(CERSIDActionParameterKey))
            {
                var CERSID = filterContext.ActionParameters[CERSIDActionParameterKey];
                if (CERSID != null)
                {
                    CurrentCERSID = Convert.ToInt32(CERSID);
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}