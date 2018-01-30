using CERS.ViewModels.OrganizationManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CERS.Web.UI.Organization.Controllers
{
	[Authorize]
	public class OrganizationManagementController : AppControllerBase
	{
		//
		// GET: /OrganizationManagement/

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult RequestAccess( int organizationID, Guid? addFacilityWizardStateKey )
		{
			var viewModel = Services.ViewModels.Organization.Management.LoadCreateAccessRequest( organizationID, addFacilityWizardStateKey );

			return View( viewModel );
		}

		[HttpPost]
		public ActionResult RequestAccess( int organizationID, Guid? addFacilityWizardStateKey, FormCollection form )
		{
			OrganizationAccessRequestViewModel viewModel = Services.ViewModels.Organization.Management.LoadCreateAccessRequest( organizationID, addFacilityWizardStateKey );

			//verify our model is valid
			if ( ModelState.IsValid )
			{
				//now lets merge the view's post data with viewmodel state from the db.
				if ( TryUpdateModel( viewModel ) )
				{
					//return partial
				}
			}

			return View( viewModel );
		}
	}
}