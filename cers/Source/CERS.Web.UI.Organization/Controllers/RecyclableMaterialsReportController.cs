using CERS.Model;
using CERS.ViewModels;
using CERS.ViewModels.Organizations;
using CERS.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UPF;
using UPF.Web.Mvc;
using UPF.Web.Mvc.UI;

namespace CERS.Web.UI.Organization.Controllers
{
	[Authorize]
	public class RecyclableMaterialsReportController : SubmittalElementControllerBase
	{
		#region RecyclableMaterialsReportStart

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult RecyclableMaterialsReportStart( int organizationId, int CERSID, int? FSEID )
		{
			if ( ConfigurationManager.AppSettings.GetValue( "IsRMREnabled", false ) )
			{
				//Cloning Logic Start
				//check to see if we have a RMReport in draft...
				var fseCurrentDraft = Services.Facilities.GetCurrentDraft( CERSID, SubmittalElementType.RecyclableMaterialsReport );

				//If FSEID is passed in AND there isn't a current RMReport in Draft start the cloning and redirect to crazy page.
				if ( FSEID.HasValue && fseCurrentDraft == null )
				{
					return Handle_Cloning( organizationId, CERSID, FSEID.Value, SubmittalElementType.RecyclableMaterialsReport );
				}

				//Cloning Logic End

				// The first call was to check to see if there is already a draft. This call will persist a new
	// draft if one doesn't exist
				var fse = Services.BusinessLogic.SubmittalElements.GetFacilitySubmittalElement( CERSID, SubmittalElementType.RecyclableMaterialsReport, SubmittalElementStatus.Draft, true );
				fse.ValidateAndCommitResults( Repository );

				//need to see if we already got a RMReport form.
				string routeName = string.Empty;
				var resource = fse.Resources.FirstOrDefault( p => p.ResourceTypeID == (int)ResourceType.RecyclableMaterialsReportDocumentation_Document && !p.Voided );
				if ( resource.ResourceEntityCount > 0 )
				{
					routeName = GetDraftEditRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportDocumentation_Document );
				}
				else
				{
					routeName = GetDraftCreateRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportDocumentation_Document );
				}
				return RedirectToRoute( routeName, new { organizationId = organizationId, CERSID = CERSID, FSEID = fse.ID } );
			}
			else
			{
				return RedirectToRoute( GetRouteName( SubmittalElementType.RecyclableMaterialsReport, Part.DraftSubmittal, Part.Unavailable ), new { organizationId = organizationId, CERSID = CERSID } );
			}
		}

		#endregion RecyclableMaterialsReportStart

		#region RecyclableMaterialsReportUnavailable

		public ActionResult RecyclableMaterialsReportUnavailable( int organizationId, int CERSID )
		{
			var viewModel = SystemViewModelData.BuildUpFacilityViewModel( CERSID );
			return View( viewModel );
		}

		#endregion RecyclableMaterialsReportUnavailable

		#region RecyclableMaterialsReportLocallyRequired_Document

		#region Detail_RecyclableMaterialsReportLocallyRequired_Document

		public ActionResult Detail_RecyclableMaterialsReportLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportLocallyRequired_Document, FSEID );
		}

		#endregion Detail_RecyclableMaterialsReportLocallyRequired_Document

		#region Create_RecyclableMaterialsReportLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_RecyclableMaterialsReportLocallyRequired_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportLocallyRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_RecyclableMaterialsReportLocallyRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportLocallyRequired_Document, viewModel );
		}

		#endregion Create_RecyclableMaterialsReportLocallyRequired_Document

		#region Edit_RecyclableMaterialsReportLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_RecyclableMaterialsReportLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportLocallyRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_RecyclableMaterialsReportLocallyRequired_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportLocallyRequired_Document, viewModel );
		}

		#endregion Edit_RecyclableMaterialsReportLocallyRequired_Document

		#region Delete_RecyclableMaterialsReportLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_RecyclableMaterialsReportLocallyRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_RecyclableMaterialsReportLocallyRequired_Document

		#endregion RecyclableMaterialsReportLocallyRequired_Document

		#region RecyclableMaterialsReportMiscellaneousStateRequired_Document

		#region Detail_RecyclableMaterialsReportMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_RecyclableMaterialsReportMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportMiscellaneousStateRequired_Document, FSEID );
		}

		#endregion Detail_RecyclableMaterialsReportMiscellaneousStateRequired_Document

		#region Create_RecyclableMaterialsReportLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_RecyclableMaterialsReportMiscellaneousStateRequired_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportMiscellaneousStateRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_RecyclableMaterialsReportMiscellaneousStateRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportMiscellaneousStateRequired_Document, viewModel );
		}

		#endregion Create_RecyclableMaterialsReportLocallyRequired_Document

		#region Edit_RecyclableMaterialsReportMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_RecyclableMaterialsReportMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportMiscellaneousStateRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_RecyclableMaterialsReportMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportMiscellaneousStateRequired_Document, viewModel );
		}

		#endregion Edit_RecyclableMaterialsReportMiscellaneousStateRequired_Document

		#region Delete_RecyclableMaterialsReportMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_RecyclableMaterialsReportMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_RecyclableMaterialsReportMiscellaneousStateRequired_Document

		#endregion RecyclableMaterialsReportMiscellaneousStateRequired_Document

		#region RecyclableMaterialsReportDocumentation_Document

		#region Detail_RecyclableMaterialsReportDocumentation_Document

		public ActionResult Detail_RecyclableMaterialsReportDocumentation_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportDocumentation_Document, FSEID );
		}

		#endregion Detail_RecyclableMaterialsReportDocumentation_Document

		#region Create_RecyclableMaterialsReportDocumentation_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_RecyclableMaterialsReportDocumentation_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportDocumentation_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_RecyclableMaterialsReportDocumentation_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportDocumentation_Document, viewModel );
		}

		#endregion Create_RecyclableMaterialsReportDocumentation_Document

		#region Edit_RecyclableMaterialsReportDocumentation_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_RecyclableMaterialsReportDocumentation_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportDocumentation_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_RecyclableMaterialsReportDocumentation_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportDocumentation_Document, viewModel );
		}

		#endregion Edit_RecyclableMaterialsReportDocumentation_Document

		#region Delete_RecyclableMaterialsReportDocumentation_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_RecyclableMaterialsReportDocumentation_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_RecyclableMaterialsReportDocumentation_Document

		#endregion RecyclableMaterialsReportDocumentation_Document
	}
}