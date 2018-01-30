using CERS.Model;
using CERS.Reports;
using CERS.ViewModels;
using CERS.ViewModels.Organizations;
using CERS.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UPF;
using UPF.Web.Mvc;
using UPF.Web.Mvc.UI;

namespace CERS.Web.UI.Organization.Controllers
{
	[Authorize]
	public class RemoteWasteConsolidationSiteAnnualNotificationController : SubmittalElementControllerBase
	{
		#region Remote Waste Consolidation Site Annual Notification Start

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult RemoteWasteConsolidationSiteAnnualNotificationStart( int organizationId, int CERSID, int? FSEID )
		{
			//Cloning Logic Start
			//check to see if we have a remote waste consolidation site annual notification in draft...
			var fseCurrentDraft = Services.Facilities.GetCurrentDraft( CERSID, SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification );

			//If FSEID is passed in AND there isn't a current RWConsolidationSite in Draft start the cloning and redirect to crazy page.
			if ( FSEID.HasValue && fseCurrentDraft == null )
			{
				return Handle_Cloning( organizationId, CERSID, FSEID.Value, SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification );
			}

			//Cloning Logic End

			// The first call was to check to see if there is already a draft. This call will persist a new
   // draft if one doesn't exists
			var fse = Services.BusinessLogic.SubmittalElements.GetFacilitySubmittalElement( CERSID, SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, SubmittalElementStatus.Draft, true );
			fse.ValidateAndCommitResults( Repository );

			//need to see if we already got a RWConsolidationSite form.
			string routeName = string.Empty;
			var resource = fse.Resources.FirstOrDefault( p => p.ResourceTypeID == (int)ResourceType.RemoteWasteConsolidationSiteAnnualNotification && !p.Voided );
			if ( resource.ResourceEntityCount > 0 )
			{
				routeName = GetDraftEditRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationSiteAnnualNotification );
			}
			else
			{
				routeName = GetDraftCreateRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationSiteAnnualNotification );
			}
			return RedirectToRoute( routeName, new { organizationId = organizationId, CERSID = CERSID, FSEID = fse.ID } );
		}

		#endregion Remote Waste Consolidation Site Annual Notification Start

		#region Remote Waste Consolidation Site Annual Notification

		#region Detail_RemoteWasteColidationAnnualNotification

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_RemoteWasteConsolidationSiteAnnualNotification( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			var viewModel = GetSpecificEntityViewModel<RWConsolidationSite>( organizationId, CERSID, SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationSiteAnnualNotification, FSEID, FSERID );
			FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource );
			return View( viewModel );
		}

		public void Detail_RemoteWasteConsolidationSiteAnnualNotification_Print( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			Services.Reports.RemoteWasteCSANotificationReportBLL( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Detail_RemoteWasteColidationAnnualNotification

		#region Create_RemoteWasteColidationAnnualNotification

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_RemoteWasteConsolidationSiteAnnualNotification( int organizationId, int CERSID, int FSEID )
		{
			return Handle_RemoteWasteConsolidationSiteAnnualNotificationGet( organizationId, CERSID, FSEID );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_RemoteWasteConsolidationSiteAnnualNotification( int organizationId, int CERSID, int FSEID, FormCollection form )
		{
			return Handle_RemoteWasteConsolidationSiteAnnualNotificationPost( organizationId, CERSID, FSEID );
		}

		#endregion Create_RemoteWasteColidationAnnualNotification

		#region Edit_RemoteWasteColidationAnnualNotification

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_RemoteWasteConsolidationSiteAnnualNotification( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_RemoteWasteConsolidationSiteAnnualNotificationGet( organizationId, CERSID, FSEID, FSERID );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_RemoteWasteConsolidationSiteAnnualNotification( int organizationId, int CERSID, int FSEID, int FSERID, FormCollection form )
		{
			return Handle_RemoteWasteConsolidationSiteAnnualNotificationPost( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Edit_RemoteWasteColidationAnnualNotification

		#region Delete_RemoteWasteColidationAnnualNotification

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_RemoteWasteConsolidationSiteAnnualNotification( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_RemoteWasteColidationAnnualNotification

		#endregion Remote Waste Consolidation Site Annual Notification

		#region RWConsolidationSiteLocallyRequired_Document

		#region Detail_RWConsolidationSiteLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document, FSEID );
		}

		#endregion Detail_RWConsolidationSiteLocallyRequired_Document

		#region Create_RWConsolidationSiteLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document, viewModel );
		}

		#endregion Create_RWConsolidationSiteLocallyRequired_Document

		#region Edit_RWConsolidationSiteLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document, viewModel );
		}

		#endregion Edit_RWConsolidationSiteLocallyRequired_Document

		#region Delete_RWConsolidationSiteLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_RWConsolidationSiteLocallyRequired_Document

		#endregion RWConsolidationSiteLocallyRequired_Document

		#region RWConsolidationSiteMiscellaneousStateRequired_Document

		#region Detail_RWConsolidationSiteMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_RemoteWasteConsolidationMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationMiscellaneousStateRequired_Document, FSEID );
		}

		#endregion Detail_RWConsolidationSiteMiscellaneousStateRequired_Document

		#region Create_RWConsolidationSiteMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_RemoteWasteConsolidationMiscellaneousStateRequired_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationMiscellaneousStateRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_RemoteWasteConsolidationMiscellaneousStateRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationMiscellaneousStateRequired_Document, viewModel );
		}

		#endregion Create_RWConsolidationSiteMiscellaneousStateRequired_Document

		#region Edit_RWConsolidationSiteMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_RemoteWasteConsolidationMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationMiscellaneousStateRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_RemoteWasteConsolidationMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationMiscellaneousStateRequired_Document, viewModel );
		}

		#endregion Edit_RWConsolidationSiteMiscellaneousStateRequired_Document

		#region Delete_RWConsolidationSiteMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_RemoteWasteConsolidationMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_RWConsolidationSiteMiscellaneousStateRequired_Document

		#endregion RWConsolidationSiteMiscellaneousStateRequired_Document

		#region Handler Methods

		#region RemoteWasteConsolidationSiteAnnualNotification (RWConsolidationSite)

		protected virtual ActionResult Handle_RemoteWasteConsolidationSiteAnnualNotificationGet( int organizationId, int CERSID, int? fseID = null, int? fserID = null )
		{
			var viewModel = GetSpecificEntityViewModel<RWConsolidationSite>( organizationId, CERSID, SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationSiteAnnualNotification, fseID, fserID );
			FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource );

			if ( viewModel.Entity.ID == 0 )
			{
				viewModel.Entity.OOName = CurrentAccount.DisplayName;
				viewModel.Entity.CertificationDate = DateTime.Now;
			}

			return View( viewModel );
		}

		protected virtual ActionResult Handle_RemoteWasteConsolidationSiteAnnualNotificationPost( int organizationId, int CERSID, int? fseID = null, int? fserID = null, FormCollection fc = null )
		{
			var viewModel = GetSpecificEntityViewModel<RWConsolidationSite>( organizationId, CERSID, SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationSiteAnnualNotification, fseID, fserID );
			var entity = viewModel.Entity;
			if ( TryUpdateModel( entity, "Entity" ) )
			{
				if ( ModelState.IsValid )
				{
					entity.FacilitySubmittalElementResource = viewModel.FacilitySubmittalElementResource;

					Repository.RWConsolidationSite.Save( entity );

					//Set LastSubmittalDeltaId
					entity.FacilitySubmittalElementResource.SetLastSubmittalDelta( entity, CERSID, Repository, "FacilitySubmittalelementResourceID" );

					//Set IsStarted Flag on FSER Record to true, if it is not already
					if ( ( entity.FacilitySubmittalElementResource != null ) && ( !entity.FacilitySubmittalElementResource.IsStarted ) )
					{
						entity.FacilitySubmittalElementResource.SetIsStarted( Repository );
					}
					viewModel.ValidateAndCommitResults( Repository, CallerContext.UI );

					string anchor = viewModel.FacilitySubmittalElement.SubmittalElement.Acronym;
					return RedirectToFacilityDraftSubmittals( viewModel.OrganizationID, viewModel.CERSID, "#" + anchor );
				}
			}
			return View( viewModel );
		}

		#endregion RemoteWasteConsolidationSiteAnnualNotification (RWConsolidationSite)

		#endregion Handler Methods
	}
}