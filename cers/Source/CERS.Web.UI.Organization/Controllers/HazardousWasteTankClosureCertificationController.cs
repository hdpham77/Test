using CERS.ViewModels;
using CERS.Web.Mvc;
using CERS.Web.UI.Organization.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CERS.Web.UI.Organization.Controllers
{
	[Authorize]
	public class HazardousWasteTankClosureCertificationController : SubmittalElementControllerBase
	{
		//
  // GET: /HazardousWasteTankClosureCertification/

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult HazWasteTankClosureCertificationStart( int organizationId, int CERSID, int? FSEID )
		{
			//Cloning Logic Start
			//check to see if we have a facility information in draft...
			var fseCurrentDraft = Services.Facilities.GetCurrentDraft( CERSID, SubmittalElementType.HazardousWasteTankClosureCertification );

			//If FSEID is passed in AND there isn't a current facility info in Draft start the cloning and redirect to crazy page.
			if ( FSEID.HasValue && fseCurrentDraft == null )
			{
				return Handle_Cloning( organizationId, CERSID, FSEID.Value, SubmittalElementType.HazardousWasteTankClosureCertification );
			}

			//Cloning Logic End

			// The first call was to check to see if there is already a draft. This call will persist a new
   // draft if one doesn't exists
			var fse = Services.BusinessLogic.SubmittalElements.GetFacilitySubmittalElement( CERSID, SubmittalElementType.HazardousWasteTankClosureCertification, SubmittalElementStatus.Draft, true );

			//need to see if we already got a BPActivity form.

			//Initial Validation
			fse.ValidateAndCommitResults( Repository, CallerContext.UI );

			string routeName = string.Empty;
			var resource = fse.Resources.SingleOrDefault( p => p.ResourceTypeID == (int)ResourceType.HazardousWasteTankClosureCertificate_Document && !p.Voided );
			if ( resource != null && resource.ResourceEntityCount > 0 )
			{
				routeName = GetDraftEditRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificate_Document );
			}
			else
			{
				routeName = GetDraftCreateRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificate_Document );
			}
			return RedirectToRoute( routeName, new { organizationId = organizationId, CERSID = CERSID, FSEID = fse.ID } );
		}

		public ActionResult Index()
		{
			return View();
		}

		#region HazardousWasteTankClosureCertificationLocallyRequired_Document

		#region Detail_HazardousWasteTankClosureCertificationLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_HazardousWasteTankClosureCertificationLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationLocallyRequired_Document, FSEID );
		}

		#endregion Detail_HazardousWasteTankClosureCertificationLocallyRequired_Document

		#region Create_HazardousWasteTankClosureCertificationLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_HazardousWasteTankClosureCertificationLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationLocallyRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_HazardousWasteTankClosureCertificationLocallyRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationLocallyRequired_Document, viewModel );
		}

		#endregion Create_HazardousWasteTankClosureCertificationLocallyRequired_Document

		#region Edit_HazardousWasteTankClosureCertificationLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_HazardousWasteTankClosureCertificationLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationLocallyRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_HazardousWasteTankClosureCertificationLocallyRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationLocallyRequired_Document, viewModel );
		}

		#endregion Edit_HazardousWasteTankClosureCertificationLocallyRequired_Document

		#region Delete_HazardousWasteTankClosureCertificationLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_HazardousWasteTankClosureCertificationLocallyRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_HazardousWasteTankClosureCertificationLocallyRequired_Document

		#endregion HazardousWasteTankClosureCertificationLocallyRequired_Document

		#region HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document

		#region Detail_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document, FSEID );
		}

		#endregion Detail_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document

		#region Create_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document, viewModel );
		}

		#endregion Create_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document

		#region Edit_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document, viewModel );
		}

		#endregion Edit_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document

		#region Delete_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document

		#endregion HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document

		#region HazardousWasteTankClosureCertificate

		#region CREATE Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_HazardousWasteTankClosureCertificate_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificate_Document, FSEID, fserID: 0 );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_HazardousWasteTankClosureCertificate_Document( int organizationId, int CERSID, int fseID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificate_Document, viewModel, fseID, fserID: 0 );
		}

		#endregion CREATE Document

		#region EDIT Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_HazardousWasteTankClosureCertificate_Document( int organizationId, int CERSID, int FSEID, int fserID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificate_Document, FSEID, fserID );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_HazardousWasteTankClosureCertificate_Document( int organizationId, int CERSID, int fseID, int fserID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificate_Document, viewModel, fseID, fserID );
		}

		#endregion EDIT Document

		#region DETAIL Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_HazardousWasteTankClosureCertificate_Document( int organizationId, int CERSID, int FSEID, int fserID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificate_Document, FSEID, fserID );
		}

		#endregion DETAIL Document

		#region DELETE Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_HazardousWasteTankClosureCertificate_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion DELETE Document

		#endregion HazardousWasteTankClosureCertificate

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