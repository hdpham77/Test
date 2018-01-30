using CERS.Model;
using CERS.ViewModels;
using CERS.ViewModels.Facilities;
using CERS.ViewModels.Organizations;
using CERS.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using UPF;
using UPF.Core;
using UPF.Web.Mvc;
using UPF.Web.Mvc.UI;

namespace CERS.Web.UI.Organization.Controllers
{
	[Authorize]
	public class OrganizationController : AppControllerBase
	{
		//this method is special, doesn't use the standard entity filter authorization because it be a main "sink" to catch all requests...
		//the method body defines code to handle security.
		public ActionResult Home( int? organizationId )
		{
			//We need to refresh the Roles because if a new Org was created by user using Admin portal, and he jumps to org portal he sees the org but the authorization below will fail if we dont refresh roles.
			CERSSecurityManager.ReloadCurrentUserRoles();
			if ( organizationId == null )
			{
				return RedirectToOrganizationPortalSwitchboard();
			}

			if ( !IsAuthorized( Context.Organization, "organizationId", PermissionRole.OrgViewer ) )
			{
				return this.HandleUnauthorizedRequest();
			}

			var orgGridView = Repository.Organizations.GetGridViewByID( organizationId.Value );
			int facilityCount = 0;
			if ( orgGridView != null )
			{
				facilityCount = orgGridView.FacilityCount;
			}

			var org = Repository.Organizations.GetByID( organizationId.Value, true );
			OrganizationViewModel viewModel = new OrganizationViewModel
			{
				//Entity = Repository.Organizations.GetByID(organizationId.Value),
				//FacilityCount = facilityCount
			};

			//need to catch non existance direct hyperlink (from old email, etc.)
			if ( org == null )
			{
				viewModel.Entity = new CERS.Model.Organization() { ID = organizationId.Value, Name = string.Format( "There has never been a valid organization record for Organization ID {0}", organizationId ), Headquarters = "Organization Not Found" };
			}
			else if ( org.Voided )
			{
				viewModel.Entity = new CERS.Model.Organization() { ID = organizationId.Value, Name = string.Format( "The organization record for organization ID {0} has been deleted", organizationId ), Headquarters = "Organization Not Found" };
			}
			else
			{
				viewModel.Entity = org;
				viewModel.FacilityCount = facilityCount;

				var facilities = Repository.Facilities.BusinessHomeFacilityListing( organizationID: organizationId.Value, includeNonRegulatedFacilities: true );
				viewModel.HasNonRegulatedFacility = facilities.Where( f => !( f.IsRegulated ?? false ) ).Any();

				if ( facilityCount > 1 )
				{
					viewModel.StartFacilitySubmittalURL = Url.RouteUrl( OrganizationFacility.SearchStartSubmittal, new { organizationId = organizationId } );
				}
				else
				{
					var facility = Repository.Facilities.GridSearch( organizationID: organizationId ).FirstOrDefault();
					if ( facility != null )
					{
						viewModel.StartFacilitySubmittalURL = Url.RouteUrl( OrganizationFacility.Home, new { organizationId = organizationId, CERSID = facility.CERSID } );
					}
				}

				var currentAccount = CurrentAccount;
			}

			return View( viewModel );
		}

		[GridAction]
		public ActionResult HomeFacilities_Async( int organizationId, int reportingRequirement = 0 )
		{
			bool includeNonRegulatedFacilities = ( reportingRequirement == 4 ) ? false : true;
			var facilities = Repository.Facilities.BusinessHomeFacilityListing( organizationId, includeNonRegulatedFacilities );
			return View( new GridModel( facilities ) );
		}

		public JsonResult HomeFacilities_GridRead( [DataSourceRequest]DataSourceRequest request, int organizationId, int reportingRequirement = 0 )
		{
			bool includeNonRegulatedFacilities = ( reportingRequirement == 4 ) ? false : true;
			var facilities = Repository.Facilities.BusinessHomeFacilityListing( organizationId, includeNonRegulatedFacilities );
			return Json( facilities.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult OrganizationFacilitySearch( int? organizationId )
		{
			if ( organizationId == null )
			{
				return RedirectToRoute( CommonRoute.SelectOrganizationMenu );
			}
			var organization = Repository.Organizations.GetByID( organizationId.Value );
			FacilityViewModel viewModel = new FacilityViewModel
			{
				OrganizationName = organization.Name,
				SourceOrganizationID = organization.ID,
				Entities = organization.Facilities.Where( f => !f.Voided )
			};

			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		[HttpPost]
		public ActionResult OrganizationFacilitySearch( int? organizationId, FacilityViewModel viewModel )
		{
			viewModel = new FacilityViewModel
			{
				Entities = Repository.Facilities.Search( CERSID: viewModel.CERSID, street: viewModel.Street, city: viewModel.City, zipCode: viewModel.ZipCode, organizationID: viewModel.SourceOrganizationID )
			};

			return View( viewModel );
		}

        #region Retrieve Scheduled Deferred Job Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
        public ActionResult RetrieveScheduledDeferredJobDocument( int deferredJobID )
        {
            DeferredJobViewModel viewModel = SystemViewModelData.BuildUpDeferredJobDocumentRetrievalViewModel( deferredJobID );
            if ( viewModel.Entity != null )
            {
                viewModel.DocumentLink = Url.Action( "OpenDocument", "Tools", new { documentID = viewModel.Entity.DocumentID, title = String.Format( "{0}_Documents", viewModel.Entity.DocumentID ) } );
                if ( (DeferredJobType)viewModel.Entity.DeferredJobTypeID == DeferredJobType.OrganizationHMIDownload )
                {
                    viewModel.JobTypeDescription = "a Hazardous Material Inventory Download";
                }
            }
            return View( viewModel );
        }

        #endregion Retrieve Scheduled Deferred Job Document
    
    }
}