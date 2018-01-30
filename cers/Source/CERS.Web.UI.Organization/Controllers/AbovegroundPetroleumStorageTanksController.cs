using CERS.ViewModels;
using CERS.Web.Mvc;
using CERS.Web.UI.Organization.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CERS.Web.UI.Organization.Controllers
{
	[Authorize]
	public class AbovegroundPetroleumStorageTanksController : SubmittalElementControllerBase
	{
		//
  // GET: /AbovegroundPetroleumStorageTanks/

		public ActionResult Index()
		{
			return View();
		}

		#region APSTStart

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult APSTStart( int organizationId, int CERSID, int? FSEID )
		{
			//Cloning Logic Start
			//check to see if we have a facility information in draft...
			var fseCurrentDraft = Services.Facilities.GetCurrentDraft( CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks );

			//If FSEID is passed in AND there isn't a current facility info in Draft start the cloning and redirect to crazy page.
			if ( FSEID.HasValue && fseCurrentDraft == null )
			{
				return Handle_Cloning( organizationId, CERSID, FSEID.Value, SubmittalElementType.AbovegroundPetroleumStorageTanks );
			}

			//Cloning Logic End

			// The first call was to check to see if there is already a draft. This call will persist a new
   // draft if one doesn't exists
			var fse = Services.BusinessLogic.SubmittalElements.GetFacilitySubmittalElement( CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks, SubmittalElementStatus.Draft, true );

			//Validate this submittal element
			fse.ValidateAndCommitResults( Repository, CallerContext.UI );

			//need to see if we already got a BPActivity form.
			string routeName = string.Empty;
			var resource = fse.Resources.SingleOrDefault( p => p.ResourceTypeID == (int)ResourceType.AbovegroundPetroleumStorageActDocumentation_Document && !p.Voided );
			if ( resource != null && resource.ResourceEntityCount > 0 )
			{
				routeName = GetDraftEditRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageActDocumentation_Document );
			}
			else
			{
				routeName = GetDraftCreateRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageActDocumentation_Document );
			}
			return RedirectToRoute( routeName, new { organizationId = organizationId, CERSID = CERSID, FSEID = fse.ID } );
		}

		#endregion APSTStart

		#region APSTLocallyRequired_Document

		#region Detail_APSTLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_AbovegroundPetroleumStorageTanksLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageTanksLocallyRequired_Document, FSEID );
		}

		#endregion Detail_APSTLocallyRequired_Document

		#region Create_APSTLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_AbovegroundPetroleumStorageTanksLocallyRequired_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageTanksLocallyRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_AbovegroundPetroleumStorageTanksLocallyRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageTanksLocallyRequired_Document, viewModel );
		}

		#endregion Create_APSTLocallyRequired_Document

		#region Edit_APSTLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_AbovegroundPetroleumStorageTanksLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageTanksLocallyRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_AbovegroundPetroleumStorageTanksLocallyRequired_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageTanksLocallyRequired_Document, viewModel );
		}

		#endregion Edit_APSTLocallyRequired_Document

		#region Delete_APSTLocallYRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_AbovegroundPetroleumStorageTanksLocallyRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_APSTLocallYRequired_Document

		#endregion APSTLocallyRequired_Document

		#region APSTMiscellaneousStateRequired_Document

		#region Detail_APSTMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_APSAMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.APSAMiscellaneousStateRequired_Document, FSEID );
		}

		#endregion Detail_APSTMiscellaneousStateRequired_Document

		#region Create_APSTLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_APSAMiscellaneousStateRequired_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.APSAMiscellaneousStateRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_APSAMiscellaneousStateRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.APSAMiscellaneousStateRequired_Document, viewModel );
		}

		#endregion Create_APSTLocallyRequired_Document

		#region Edit_APSTLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_APSAMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.APSAMiscellaneousStateRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_APSAMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.APSAMiscellaneousStateRequired_Document, viewModel );
		}

		#endregion Edit_APSTLocallyRequired_Document

		#region Delete_APSTLocallYRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_APSAMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_APSTLocallYRequired_Document

		#endregion APSTMiscellaneousStateRequired_Document

		#region AbovegroundPetroleumStorageActDocumentation_Document

		#region Detail_AbovegroundPetroleumStorageActDocumentation_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_AbovegroundPetroleumStorageActDocumentation_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageActDocumentation_Document, FSEID );
		}

		#endregion Detail_AbovegroundPetroleumStorageActDocumentation_Document

		#region Create_AbovegroundPetroleumStorageActDocumentation_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_AbovegroundPetroleumStorageActDocumentation_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageActDocumentation_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_AbovegroundPetroleumStorageActDocumentation_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageActDocumentation_Document, viewModel );
		}

		#endregion Create_AbovegroundPetroleumStorageActDocumentation_Document

		#region Edit_AbovegroundPetroleumStorageActDocumentation_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_AbovegroundPetroleumStorageActDocumentation_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageActDocumentation_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_AbovegroundPetroleumStorageActDocumentation_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageActDocumentation_Document, viewModel );
		}

		#endregion Edit_AbovegroundPetroleumStorageActDocumentation_Document

		#region Delete_AbovegroundPetroleumStorageActDocumentation_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_AbovegroundPetroleumStorageActDocumentation_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_AbovegroundPetroleumStorageActDocumentation_Document

		#endregion AbovegroundPetroleumStorageActDocumentation_Document

		#region Document_Cancel

		[HttpPost]
		public JsonResult DocumentCancel_Async( int fserID )
		{
			bool success = true;
            if ( fserID != 0 )
            {
                var fser = Repository.FacilitySubmittalElementResources.GetByID( fserID );

                //Validate FSE
                fser.FacilitySubmittalElement.ValidateAndCommitResults( Repository );
            }
			return Json( new { Success = success } );
		}

		#endregion Document_Cancel
	}
}