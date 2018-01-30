using CERS;
using CERS.Model;
using CERS.ViewModels;
using CERS.ViewModels.Organizations;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using UPF;
using CERSOrg = CERS.Model.Organization;

namespace CERS.Web.UI.Organization.Controllers
{
	[Authorize]
	public class HomeController : AppControllerBase
	{
		public ActionResult GetStarted()
		{
			GetStartedViewModel viewModel = new GetStartedViewModel();
			var arNotifications = Services.Events.GetPendingAccessRequests( CurrentAccountID );
			viewModel.PendingAccessRequests = arNotifications;
			return View( viewModel );
		}

		public ActionResult Index()
		{
			return RedirectToOrganizationPortalSwitchboard();
		}

        public void OrganizationsListExportToExcel( string name, string headquarters, int? CERSID )
		{
            var organizationsGridView = Repository.Organizations.GridSearch( name: name.TrimPro(), headquarters: headquarters.TrimPro(), CERSID: CERSID, statusID: (int)OrganizationStatus.Active, applySecurityFilter: true, matrices: CurrentUserRoles );

			ExportToExcel( "OrganizationsList.xlsx", organizationsGridView, new ExcelColumnMapping( "OrganizationCode" )
																		 , new ExcelColumnMapping( "Name" )
																		 , new ExcelColumnMapping( "HeadQuarters" )
																		 , new ExcelColumnMapping( "FacilityCount" )
																		 , new ExcelColumnMapping( "UserCount" )
																		 , new ExcelColumnMapping( "CreatedOn" )
				);
		}

		public ActionResult SelectOrganization()
		{
			OrganizationViewModel viewModel = null;
			if ( CurrentUserRoles.IsSystemAdmin )
			{
				viewModel = new OrganizationViewModel
				{
					Entities = new List<CERSOrg>()
				};
			}
			else
			{
				//make sure to apply security filtering, and lets pass in the cached set of user roles.
				var orgs = Repository.Organizations.GridSearch( statusID: (int) OrganizationStatus.Active, applySecurityFilter: true, matrices: CurrentUserRoles );
				viewModel = new OrganizationViewModel
				{
					OrganizationGridView = orgs
				};
			}
			return View( viewModel );
		}

		public ActionResult Switchboard()
		{
			return RedirectToOrganizationPortalSwitchboard();
		}

		public ActionResult UA( Boolean? displayOnly = null )
		{
			if ( displayOnly != null )
			{
				ViewBag.DisplayOnly = displayOnly;
			}
			return View( "UserAgreementSplash" );
		}

		[HttpPost]
		public ActionResult UA( FormCollection form )
		{
			if ( form["agrees"] != "true" )
			{
				ModelState.AddModelError( "agrees", "You must agree to continue." );
			}

			if ( CurrentPortal.Environment == RuntimeEnvironment.Training )
			{
				if ( form["AgreeTraining"] != "true" )
				{
					ModelState.AddModelError( "AgreeTraining", "You must acknowledge you are using the training version of CERS." );
				}
			}

			if ( ModelState.IsValid )
			{
				Repository.ContactStatistics.Update( CurrentAccount, uaAgreed: true );
				return RedirectToOrganizationPortalSwitchboard();
			}
			return View( "UserAgreementSplash" );
		}

		public ActionResult UnAuthorized()
		{
			return View();
		}

		#region Ajax GridAction

		[GridAction]
		public ActionResult Search_GridBindingOrganization( string name, string headquarters, int? statusID = (int)OrganizationStatus.Active, int? CERSID = null )
		{
			var entities = Repository.Organizations.GridSearch( name: name.TrimPro(), headquarters: headquarters.TrimPro(), statusID: statusID, applySecurityFilter: true, matrices: CurrentUserRoles, CERSID: CERSID );
			return View( new GridModel( entities ) );
		}

		public ActionResult SelectOrganization_GridRead( [DataSourceRequest]DataSourceRequest request, string name, string headquarters, int? statusID = (int)OrganizationStatus.Active, int? CERSID = null )
		{
			var entities = Repository.Organizations.GridSearch( name: name.TrimPro(), headquarters: headquarters.TrimPro(), statusID: statusID, applySecurityFilter: true, matrices: CurrentUserRoles, CERSID: CERSID );
			DataSourceResult result = entities.ToDataSourceResult( request );
			return Json( result, JsonRequestBehavior.AllowGet );
		}

		#endregion Ajax GridAction
	}
}