using CERS.Guidance;
using CERS.Model;
using CERS.ViewModels;
using CERS.Web.Mvc;
using CERS.Web.UI.Organization.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CERS.Web.UI.Organization.Controllers
{
	[Authorize]
	public class EmergencyResponseAndTrainingPlansController : SubmittalElementControllerBase
	{
		//
  // GET: /EmergencyResponseAndTrainingPlans/

		public ActionResult Index()
		{
			return View();
		}

		#region EmergencyResponseAndTrainingPlansStart

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult EmergencyResponseAndTrainingPlansStart( int organizationId, int CERSID, int? FSEID )
		{
			//Cloning Logic Start
			//check to see if we have a facility information in draft...
			var fseCurrentDraft = Services.Facilities.GetCurrentDraft( CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans );

			//If FSEID is passed in AND there isn't a current facility info in Draft start the cloning and redirect to crazy page.
			if ( FSEID.HasValue && fseCurrentDraft == null )
			{
				return Handle_Cloning( organizationId, CERSID, FSEID.Value, SubmittalElementType.EmergencyResponseandTrainingPlans );
			}

			//Cloning Logic End

			// The first call was to check to see if there is already a draft. This call will persist a new
   // draft if one doesn't exists
			var fse = Services.BusinessLogic.SubmittalElements.GetFacilitySubmittalElement( CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, SubmittalElementStatus.Draft, true );

			//Validate this submittal element
			fse.ValidateAndCommitResults( Repository, CallerContext.UI );

			//need to see if we already got a BPActivity form.
			string routeName = string.Empty;
			var resource = fse.Resources.SingleOrDefault( p => p.ResourceTypeID == (int)ResourceType.EmergencyResponseContingencyPlan_Document && !p.Voided );
			if ( resource.ResourceEntityCount > 0 )
			{
				routeName = GetDraftEditRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseContingencyPlan_Document );
			}
			else
			{
				routeName = GetDraftCreateRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseContingencyPlan_Document );
			}
			return RedirectToRoute( routeName, new { organizationId = organizationId, CERSID = CERSID, FSEID = fse.ID } );
		}

		#endregion EmergencyResponseAndTrainingPlansStart

		#region Supplemental Document Actions

		#region EmergencyResponseContingencyPlan_Document

		#region Detail_EmergencyResponseContingencyPlan_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_EmergencyResponseContingencyPlan_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseContingencyPlan_Document, FSEID );
		}

		#endregion Detail_EmergencyResponseContingencyPlan_Document

		#region Create_EmergencyResponseContingencyPlan_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_EmergencyResponseContingencyPlan_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseContingencyPlan_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_EmergencyResponseContingencyPlan_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseContingencyPlan_Document, viewModel );
		}

		#endregion Create_EmergencyResponseContingencyPlan_Document

		#region Edit_EmergencyResponseContingencyPlan_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_EmergencyResponseContingencyPlan_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseContingencyPlan_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_EmergencyResponseContingencyPlan_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseContingencyPlan_Document, viewModel );
		}

		#endregion Edit_EmergencyResponseContingencyPlan_Document

		#region Delete_EmergencyResponseContingencyPlan_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_EmergencyResponseContingencyPlan_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_EmergencyResponseContingencyPlan_Document

		#endregion EmergencyResponseContingencyPlan_Document

		#region EmployeeTrainingPlan_Document

		#region Detail_EmployeeTrainingPlan_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_EmployeeTrainingPlan_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmployeeTrainingPlan_Document, FSEID );
		}

		#endregion Detail_EmployeeTrainingPlan_Document

		#region Create_EmployeeTrainingPlan_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_EmployeeTrainingPlan_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmployeeTrainingPlan_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_EmployeeTrainingPlan_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmployeeTrainingPlan_Document, viewModel );
		}

		#endregion Create_EmployeeTrainingPlan_Document

		#region Edit_EmployeeTrainingPlan_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_EmployeeTrainingPlan_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmployeeTrainingPlan_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_EmployeeTrainingPlan_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmployeeTrainingPlan_Document, viewModel );
		}

		#endregion Edit_EmployeeTrainingPlan_Document

		#region Delete_EmployeeTrainingPlan_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_EmployeeTrainingPlan_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_EmployeeTrainingPlan_Document

		#endregion EmployeeTrainingPlan_Document

		#region EmergencyResponseAndTrainingPlansLocallyRequired_Document

		#region Detail_EmergencyResponseAndTrainingPlansLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_EmergencyResponseAndTrainingPlansLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseAndTrainingPlansLocallyRequired_Document, FSEID );
		}

		#endregion Detail_EmergencyResponseAndTrainingPlansLocallyRequired_Document

		#region Create_EmergencyResponseAndTrainingPlansLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_EmergencyResponseAndTrainingPlansLocallyRequired_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseAndTrainingPlansLocallyRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_EmergencyResponseAndTrainingPlansLocallyRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseAndTrainingPlansLocallyRequired_Document, viewModel );
		}

		#endregion Create_EmergencyResponseAndTrainingPlansLocallyRequired_Document

		#region Edit_EmergencyResponseAndTrainingPlansLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_EmergencyResponseAndTrainingPlansLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseAndTrainingPlansLocallyRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_EmergencyResponseAndTrainingPlansLocallyRequired_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseAndTrainingPlansLocallyRequired_Document, viewModel );
		}

		#endregion Edit_EmergencyResponseAndTrainingPlansLocallyRequired_Document

		#region Delete_EmergencyResponseAndTrainingPlansLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_EmergencyResponseAndTrainingPlansLocallyRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_EmergencyResponseAndTrainingPlansLocallyRequired_Document

		#endregion EmergencyResponseAndTrainingPlansLocallyRequired_Document

		#region EmergencyResponseAndTrainingPlansMiscellaneousStateRequired_Document

		#region Detail_ERTPMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_ERTPMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.ERTPMiscellaneousStateRequired_Document, FSEID );
		}

		#endregion Detail_ERTPMiscellaneousStateRequired_Document

		#region Create_ERTPMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_ERTPMiscellaneousStateRequired_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.ERTPMiscellaneousStateRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_ERTPMiscellaneousStateRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.ERTPMiscellaneousStateRequired_Document, viewModel );
		}

		#endregion Create_ERTPMiscellaneousStateRequired_Document

		#region Edit_ERTPMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_ERTPMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.ERTPMiscellaneousStateRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_ERTPMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.ERTPMiscellaneousStateRequired_Document, viewModel );
		}

		#endregion Edit_ERTPMiscellaneousStateRequired_Document

		#region Delete_ERTPMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_ERTPMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_ERTPMiscellaneousStateRequired_Document

		#endregion EmergencyResponseAndTrainingPlansMiscellaneousStateRequired_Document

		#endregion Supplemental Document Actions
	}
}