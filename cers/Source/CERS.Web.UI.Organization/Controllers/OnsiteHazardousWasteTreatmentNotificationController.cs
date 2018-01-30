using CERS.Model;
using CERS.Reports;
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
	public class OnsiteHazardousWasteTreatmentNotificationController : SubmittalElementControllerBase
	{
		//
  // GET: /OnsiteHazardousWasteTreatmentNotification/

		#region Index

		public ActionResult Index()
		{
			return View();
		}

		#endregion Index

		#region OnsiteHazWasteTreatmentNotificationStart

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult OnsiteHazWasteTreatmentNotificationStart( int organizationId, int CERSID, int? FSEID )
		{
			if ( ConfigurationManager.AppSettings.GetValue( "IsTPEnabled", false ) )
			{
				//throw new NotImplementedException("OnsiteHazWasteTreatmentNotificationStart");

				//Cloning Logic Start
				//check to see if we have a TP in draft...
				var fseCurrentDraft = Services.Facilities.GetCurrentDraft( CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification );

				//If FSEID is passed in AND there isn't a current RMReport in Draft start the cloning and redirect to crazy page.
				if ( FSEID.HasValue && fseCurrentDraft == null )
				{
					return Handle_Cloning( organizationId, CERSID, FSEID.Value, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification );
				}

				//Cloning Logic End

				// The first call was to check to see if there is already a draft. This call will persist a new
	// draft if one doesn't exist
				var fse = Services.BusinessLogic.SubmittalElements.GetFacilitySubmittalElement( CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, SubmittalElementStatus.Draft, true );
				fse.ValidateAndCommitResults( Repository );

				//need to see if we already got a RMReport form.
				string routeName = string.Empty;
				var resource = fse.Resources.FirstOrDefault( p => p.ResourceTypeID == (int)ResourceType.OnsiteHazardousWasteTreatmentNotificationFacility && !p.Voided );
				if ( resource.ResourceEntityCount > 0 )
				{
					routeName = GetDraftEditRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationFacility );
					return RedirectToRoute( routeName, new { organizationId = organizationId, CERSID = CERSID, FSEID = fse.ID } );
				}
				else
				{
					routeName = GetDraftCreateRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationFacility );
					return RedirectToRoute( routeName, new { organizationId = organizationId, CERSID = CERSID, FSEID = fse.ID } );
				}
			}
			else
			{
				return RedirectToRoute( GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, Part.DraftSubmittal, Part.Unavailable ), new { organizationId = organizationId, CERSID = CERSID } );
			}
		}

		#endregion OnsiteHazWasteTreatmentNotificationStart

		#region OnsiteHazWasteTreatmentNotificationUnavailable

		public ActionResult OnsiteHazWasteTreatmentNotificationUnavailable( int organizationId, int CERSID )
		{
			var viewModel = SystemViewModelData.BuildUpFacilityViewModel( CERSID );
			return View( viewModel );
		}

		#endregion OnsiteHazWasteTreatmentNotificationUnavailable

		#region OnsiteHazardousWasteTreatmentNotificationFacility

		#region Detail_OnsiteHazardousWasteTreatmentNotificationFacility

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_OnsiteHazardousWasteTreatmentNotificationFacility( int organizationId, int CERSID, int FSEID )
		{
			return Handle_OnsiteHazardousWasteTreatmentNotificationFacilityGet( organizationId, CERSID, FSEID );
		}

		public void Detail_OnsiteHazardousWasteTreatmentNotificationFacility_Print( int organizationId, int CERSID, int FSEID )
		{
			Services.Reports.TieredPermittingFacilityReportBLL( organizationId, CERSID, FSEID );
		}

		#endregion Detail_OnsiteHazardousWasteTreatmentNotificationFacility

		#region Create_OnsiteHazardousWasteTreatmentNotificationFacility

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_OnsiteHazardousWasteTreatmentNotificationFacility( int organizationId, int CERSID, int FSEID )
		{
			return Handle_OnsiteHazardousWasteTreatmentNotificationFacilityGet( organizationId, CERSID, FSEID );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_OnsiteHazardousWasteTreatmentNotificationFacility( int organizationId, int CERSID, int FSEID, FormCollection form )
		{
			return Handle_OnsiteHazardousWasteTreatmentNotificationFacilityPost( organizationId, CERSID, FSEID );
		}

		#endregion Create_OnsiteHazardousWasteTreatmentNotificationFacility

		#region Edit_OnsiteHazardousWasteTreatmentNotificationFacility

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_OnsiteHazardousWasteTreatmentNotificationFacility( int organizationId, int CERSID, int FSEID, int? FSERID )
		{
			return Handle_OnsiteHazardousWasteTreatmentNotificationFacilityGet( organizationId, CERSID, FSEID );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_OnsiteHazardousWasteTreatmentNotificationFacility( int organizationId, int CERSID, int FSEID, FormCollection form )
		{
			return Handle_OnsiteHazardousWasteTreatmentNotificationFacilityPost( organizationId, CERSID, FSEID, form );
		}

		#endregion Edit_OnsiteHazardousWasteTreatmentNotificationFacility

		#region Delete_OnsiteHazardousWasteTreatmentNotificationFacility

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_OnsiteHazardousWasteTreatmentNotificationFacility( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_OnsiteHazardousWasteTreatmentNotificationFacility

		#endregion OnsiteHazardousWasteTreatmentNotificationFacility

		#region OnsiteHazardousWasteTreatmentNotificationUnit

		#region Detail_OnsiteHazardousWasteTreatmentNotificationUnit

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_OnsiteHazardousWasteTreatmentNotificationUnit( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_OnsiteHazardousWasteTreatmentNotificationUnitGet( organizationId, CERSID, FSEID, FSERID );
		}

		public void Detail_OnsiteHazardousWasteTreatmentNotificationUnit_Print( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			Services.Reports.TieredPermittingReportBLL( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Detail_OnsiteHazardousWasteTreatmentNotificationUnit

		#region Create_OnsiteHazardousWasteTreatmentNotificationUnit

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_OnsiteHazardousWasteTreatmentNotificationUnit( int organizationId, int CERSID, int FSEID )
		{
			return Handle_OnsiteHazardousWasteTreatmentNotificationUnitGet( organizationId, CERSID, FSEID );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_OnsiteHazardousWasteTreatmentNotificationUnit( int organizationId, int CERSID, int FSEID, FormCollection form )
		{
			return Handle_OnsiteHazardousWasteTreatmentNotificationUnitPost( organizationId, CERSID, FSEID );
		}

		#endregion Create_OnsiteHazardousWasteTreatmentNotificationUnit

		#region Edit_OnsiteHazardousWasteTreatmentNotificationUnit

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_OnsiteHazardousWasteTreatmentNotificationUnit( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_OnsiteHazardousWasteTreatmentNotificationUnitGet( organizationId, CERSID, FSEID, FSERID );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_OnsiteHazardousWasteTreatmentNotificationUnit( int organizationId, int CERSID, int FSEID, int FSERID, FormCollection form )
		{
			return Handle_OnsiteHazardousWasteTreatmentNotificationUnitPost( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Edit_OnsiteHazardousWasteTreatmentNotificationUnit

		#region Delete_OnsiteHazardousWasteTreatmentNotificationUnit

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_OnsiteHazardousWasteTreatmentNotificationUnit( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_OnsiteHazardousWasteTreatmentNotificationUnit

		#endregion OnsiteHazardousWasteTreatmentNotificationUnit

		#region OnsiteHazardousWasteTreatmentNotificationFinancialAssurance

		#region Detail_TieredPermittingUnitCertificationofFinancialAssurance

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_TieredPermittingUnitCertificationofFinancialAssurance( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_TieredPermittingUnitCertificationofFinancialAssuranceGet( organizationId, CERSID, FSEID, FSERID );
		}

		public void Detail_TieredPermittingUnitCertificationofFinancialAssurance_Print( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			Services.Reports.TieredPermittingFinancialAssuranceReportBLL( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Detail_TieredPermittingUnitCertificationofFinancialAssurance

		#region Create_TieredPermittingUnitCertificationofFinancialAssurance

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_TieredPermittingUnitCertificationofFinancialAssurance( int organizationId, int CERSID, int FSEID )
		{
			return Handle_TieredPermittingUnitCertificationofFinancialAssuranceGet( organizationId, CERSID, FSEID );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_TieredPermittingUnitCertificationofFinancialAssurance( int organizationId, int CERSID, int FSEID, FormCollection form )
		{
			return Handle_TieredPermittingUnitCertificationofFinancialAssurancePost( organizationId, CERSID, FSEID );
		}

		#endregion Create_TieredPermittingUnitCertificationofFinancialAssurance

		#region Edit_TieredPermittingUnitCertificationofFinancialAssurance

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_TieredPermittingUnitCertificationofFinancialAssurance( int organizationId, int CERSID, int FSEID, int? FSERID )
		{
			return Handle_TieredPermittingUnitCertificationofFinancialAssuranceGet( organizationId, CERSID, FSEID, FSERID );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_TieredPermittingUnitCertificationofFinancialAssurance( int organizationId, int CERSID, int FSEID, int FSERID, FormCollection form )
		{
			return Handle_TieredPermittingUnitCertificationofFinancialAssurancePost( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Edit_TieredPermittingUnitCertificationofFinancialAssurance

		#region Delete_TieredPermittingUnitCertificationofFinancialAssurance

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_TieredPermittingUnitCertificationofFinancialAssurance( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_TieredPermittingUnitCertificationofFinancialAssurance

		#endregion OnsiteHazardousWasteTreatmentNotificationFinancialAssurance

		#region OnsiteHazardousWasteTreatmentPlotPlanMap_Document

		#region Detail_OnsiteHazardousWasteTreatmentPlotPlanMap_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_OnsiteHazardousWasteTreatmentPlotPlanMap_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentPlotPlanMap_Document, FSEID );
		}

		#endregion Detail_OnsiteHazardousWasteTreatmentPlotPlanMap_Document

		#region Create_OnsiteHazardousWasteTreatmentPlotPlanMap_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_OnsiteHazardousWasteTreatmentPlotPlanMap_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentPlotPlanMap_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_OnsiteHazardousWasteTreatmentPlotPlanMap_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentPlotPlanMap_Document, viewModel );
		}

		#endregion Create_OnsiteHazardousWasteTreatmentPlotPlanMap_Document

		#region Edit_OnsiteHazardousWasteTreatmentPlotPlanMap_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_OnsiteHazardousWasteTreatmentPlotPlanMap_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentPlotPlanMap_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_OnsiteHazardousWasteTreatmentPlotPlanMap_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentPlotPlanMap_Document, viewModel );
		}

		#endregion Edit_OnsiteHazardousWasteTreatmentPlotPlanMap_Document

		#region Delete_OnsiteHazardousWasteTreatmentPlotPlanMap_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_OnsiteHazardousWasteTreatmentPlotPlanMap_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_OnsiteHazardousWasteTreatmentPlotPlanMap_Document

		#endregion OnsiteHazardousWasteTreatmentPlotPlanMap_Document

		#region TieredPermittingUnitPriorEnforcementHistory_Document

		#region Detail_TieredPermittingUnitPriorEnforcementHistory_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_TieredPermittingUnitPriorEnforcementHistory_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitPriorEnforcementHistory_Document, FSEID );
		}

		#endregion Detail_TieredPermittingUnitPriorEnforcementHistory_Document

		#region Create_TieredPermittingUnitPriorEnforcementHistory_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_TieredPermittingUnitPriorEnforcementHistory_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitPriorEnforcementHistory_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_TieredPermittingUnitPriorEnforcementHistory_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitPriorEnforcementHistory_Document, viewModel );
		}

		#endregion Create_TieredPermittingUnitPriorEnforcementHistory_Document

		#region Edit_TieredPermittingUnitPriorEnforcementHistory_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_TieredPermittingUnitPriorEnforcementHistory_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitPriorEnforcementHistory_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_TieredPermittingUnitPriorEnforcementHistory_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitPriorEnforcementHistory_Document, viewModel );
		}

		#endregion Edit_TieredPermittingUnitPriorEnforcementHistory_Document

		#region Delete_TieredPermittingUnitPriorEnforcementHistory_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_TieredPermittingUnitPriorEnforcementHistory_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_TieredPermittingUnitPriorEnforcementHistory_Document

		#endregion TieredPermittingUnitPriorEnforcementHistory_Document

		#region TieredPermittingUnitTankandContainerCertification_Document

		#region Detail_TieredPermittingUnitTankandContainerCertification_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_TieredPermittingUnitTankandContainerCertification_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitTankandContainerCertification_Document, FSEID );
		}

		#endregion Detail_TieredPermittingUnitTankandContainerCertification_Document

		#region Create_TieredPermittingUnitTankandContainerCertification_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_TieredPermittingUnitTankandContainerCertification_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitTankandContainerCertification_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_TieredPermittingUnitTankandContainerCertification_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitTankandContainerCertification_Document, viewModel );
		}

		#endregion Create_TieredPermittingUnitTankandContainerCertification_Document

		#region Edit_TieredPermittingUnitTankandContainerCertification_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_TieredPermittingUnitTankandContainerCertification_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitTankandContainerCertification_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_TieredPermittingUnitTankandContainerCertification_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitTankandContainerCertification_Document, viewModel );
		}

		#endregion Edit_TieredPermittingUnitTankandContainerCertification_Document

		#region Delete_TieredPermittingUnitTankandContainerCertification_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_TieredPermittingUnitTankandContainerCertification_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_TieredPermittingUnitTankandContainerCertification_Document

		#endregion TieredPermittingUnitTankandContainerCertification_Document

		#region TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document

		#region Detail_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document, FSEID );
		}

		#endregion Detail_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document

		#region Create_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document, viewModel );
		}

		#endregion Create_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document

		#region Edit_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document, viewModel );
		}

		#endregion Edit_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document

		#region Delete_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document

		#endregion TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document

		#region TieredPermittingUnitNotificationofPropertyOwner_Document

		#region Detail_TieredPermittingUnitNotificationofPropertyOwner_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_TieredPermittingUnitNotificationofPropertyOwner_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofPropertyOwner_Document, FSEID );
		}

		#endregion Detail_TieredPermittingUnitNotificationofPropertyOwner_Document

		#region Create_TieredPermittingUnitNotificationofPropertyOwner_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_TieredPermittingUnitNotificationofPropertyOwner_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofPropertyOwner_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_TieredPermittingUnitNotificationofPropertyOwner_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofPropertyOwner_Document, viewModel );
		}

		#endregion Create_TieredPermittingUnitNotificationofPropertyOwner_Document

		#region Edit_TieredPermittingUnitNotificationofPropertyOwner_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_TieredPermittingUnitNotificationofPropertyOwner_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofPropertyOwner_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_TieredPermittingUnitNotificationofPropertyOwner_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofPropertyOwner_Document, viewModel );
		}

		#endregion Edit_TieredPermittingUnitNotificationofPropertyOwner_Document

		#region Delete_TieredPermittingUnitNotificationofPropertyOwner_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_TieredPermittingUnitNotificationofPropertyOwner_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_TieredPermittingUnitNotificationofPropertyOwner_Document

		#endregion TieredPermittingUnitNotificationofPropertyOwner_Document

		#region OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document

		#region Detail_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document, FSEID );
		}

		#endregion Detail_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document

		#region Create_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document, viewModel );
		}

		#endregion Create_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document

		#region Edit_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document, viewModel );
		}

		#endregion Edit_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document

		#region Delete_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document

		#endregion OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document

		#region OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document

		#region Detail_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document, FSEID );
		}

		#endregion Detail_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document

		#region Create_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document, viewModel );
		}

		#endregion Create_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document

		#region Edit_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document, viewModel );
		}

		#endregion Edit_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document

		#region Delete_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document

		#endregion OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document

		#region OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		#region Detail_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document, FSEID );
		}

		#endregion Detail_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		#region Create_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document, viewModel );
		}

		#endregion Create_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		#region Edit_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document, viewModel );
		}

		#endregion Edit_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		#region Delete_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		#endregion OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		#region OnsiteHazardousWasteTreatmentNotificationMiscellaneousStateRequired_Document

		#region Detail_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_TieredPermittingMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingMiscellaneousStateRequired_Document, FSEID );
		}

		#endregion Detail_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		#region Create_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_TieredPermittingMiscellaneousStateRequired_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingMiscellaneousStateRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_TieredPermittingMiscellaneousStateRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingMiscellaneousStateRequired_Document, viewModel );
		}

		#endregion Create_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		#region Edit_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_TieredPermittingMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingMiscellaneousStateRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_TieredPermittingMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingMiscellaneousStateRequired_Document, viewModel );
		}

		#endregion Edit_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		#region Delete_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_TieredPermittingMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document

		#endregion OnsiteHazardousWasteTreatmentNotificationMiscellaneousStateRequired_Document

		#region Handler Methods

		#region OnsiteHazWasteTreatmentNotificationFacility

		protected virtual ActionResult Handle_OnsiteHazardousWasteTreatmentNotificationFacilityGet( int organizationId, int CERSID, int? fseID = null )
		{
			var viewModel = GetSingleEntityViewModel<HWTPFacility>( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationFacility, fseID );
			FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource );

			if ( viewModel.Entity.ID == 0 )
			{
				viewModel.Entity.OOName = CurrentAccount.DisplayName;
				var businessContact = Repository.OrganizationContacts.Search( organizationId, CurrentAccountID ).FirstOrDefault();
				viewModel.Entity.OOTitle = businessContact != null ? businessContact.Title : string.Empty;
				viewModel.Entity.DateCertified = DateTime.Now;
				viewModel.Entity.ShortenedReviewPeriod = "N";
			}

			return View( viewModel );
		}

		protected virtual ActionResult Handle_OnsiteHazardousWasteTreatmentNotificationFacilityPost( int organizationId, int CERSID, int? fseID = null, FormCollection fc = null )
		{
			var viewModel = GetSingleEntityViewModel<HWTPFacility>( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationFacility, fseID );
			var entity = viewModel.Entity;
			if ( TryUpdateModel( entity, "Entity" ) )
			{
				if ( ModelState.IsValid )
				{
					entity.FacilitySubmittalElementResource = viewModel.FacilitySubmittalElementResource;
					Repository.HWTPFacilities.Save( entity );

					//Set LastSubmittalDeltaId
					entity.FacilitySubmittalElementResource.SetLastSubmittalDelta( entity, CERSID, Repository );

					if ( ( entity.FacilitySubmittalElementResource != null ) && ( !entity.FacilitySubmittalElementResource.IsStarted ) )
					{
						entity.FacilitySubmittalElementResource.SetIsStarted( Repository );
					}

					//Set LastSubmittalDeltaId
					entity.FacilitySubmittalElementResource.SetLastSubmittalDelta( entity, CERSID, Repository );

					viewModel.ValidateAndCommitResults( Repository, CallerContext.UI );

					//If no Unit exists and Number of CECL is not > 0 send to Unit page, else back to crazy page.
					if ( ( !entity.NumberOfUnitsCECL.HasValue || entity.NumberOfUnitsCECL.Value <= 0 ) && ( entity.HWTPUnits == null || entity.HWTPUnits.Count == 0 ) )
					{
						string routeName = GetDraftCreateRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationUnit );
						return RedirectToRoute( routeName, new { organizationID = organizationId, CERSID = CERSID, FSEID = viewModel.FacilitySubmittalElement.ID } );
					}
					else
					{
						string anchor = viewModel.FacilitySubmittalElement.SubmittalElement.Acronym;
						return RedirectToFacilityDraftSubmittals( viewModel.OrganizationID, viewModel.CERSID, "#" + anchor );
					}
				}
			}
			return View( viewModel );
		}

		#endregion OnsiteHazWasteTreatmentNotificationFacility

		#region OnsiteHazWasteTreatmentNotificationUnit

		protected virtual ActionResult Handle_OnsiteHazardousWasteTreatmentNotificationUnitGet( int organizationId, int CERSID, int FSEID, int? FSERID = null )
		{
			var viewModel = GetSpecificEntityViewModel<HWTPUnit>( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationUnit, FSEID, FSERID );
			FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource );
			return View( viewModel );
		}

		protected virtual ActionResult Handle_OnsiteHazardousWasteTreatmentNotificationUnitPost( int organizationId, int CERSID, int? fseID = null, int? fserID = null, FormCollection fc = null )
		{
			var viewModel = GetSpecificEntityViewModel<HWTPUnit>( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationUnit, fseID, fserID );
			var entity = viewModel.Entity;
			if ( TryUpdateModel( entity, "Entity" ) )
			{
				if ( ModelState.IsValid )
				{
					entity.FacilitySubmittalElementResource = viewModel.FacilitySubmittalElementResource;

					if ( entity.HWTPFacility == null )
					{
						FacilitySubmittalElementResource hwtpFacilityFSER = viewModel.FacilitySubmittalElement.Resources.Where( fser => fser.GetResourceType() == ResourceType.OnsiteHazardousWasteTreatmentNotificationFacility && !fser.Voided ).SingleOrDefault();
						if ( hwtpFacilityFSER != null )
						{
							entity.FacilitySubmittalElementResource.ParentResourceID = hwtpFacilityFSER.ID;
							entity.HWTPFacility = hwtpFacilityFSER.HWTPFacilities.FetchSingleUnVoided();
						}
						else
						{
							throw new ArgumentNullException( "hwtpFacilityFSER", "No HWTP Facility FSER record found.  Cannot create HWTP Unit." );
						}
					}
					Repository.HWTPUnits.Save( entity );

					//Set LastSubmittalDeltaId
					entity.FacilitySubmittalElementResource.SetLastSubmittalDelta( entity, CERSID, Repository, "FacilitySubmittalElementResourceID" );

					if ( ( entity.FacilitySubmittalElementResource != null ) && ( !entity.FacilitySubmittalElementResource.IsStarted ) )
					{
						entity.FacilitySubmittalElementResource.SetIsStarted( Repository );
					}
					viewModel.ValidateAndCommitResults( Repository, CallerContext.UI );

					//if Unit type is PBR or CA and there is no existing Financial Assurance redirect to Financial Assurance, else go to crazy page.
					if ( ( entity.UnitType == "c" || entity.UnitType == "d" ) && ( viewModel.FacilitySubmittalElement.Resources.Count( fser => fser.ResourceTypeID == (int)ResourceType.TieredPermittingUnitCertificationofFinancialAssurance && fser.ResourceEntityCount > 0 && !fser.Voided ) == 0 ) )
					{
						string routeName = GetDraftCreateRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitCertificationofFinancialAssurance );
						return RedirectToRoute( routeName, new { organizationID = organizationId, CERSID = CERSID, FSEID = viewModel.FacilitySubmittalElement.ID } );
					}
					else
					{
						string anchor = viewModel.FacilitySubmittalElement.SubmittalElement.Acronym;
						return RedirectToFacilityDraftSubmittals( viewModel.OrganizationID, viewModel.CERSID, "#" + anchor );
					}
				}
			}

			return View( viewModel );
		}

		#endregion OnsiteHazWasteTreatmentNotificationUnit

		#region TieredPermittingUnitCertificationofFinancialAssurance

		public virtual ActionResult Handle_TieredPermittingUnitCertificationofFinancialAssuranceGet( int organizationId, int CERSID, int FSEID, int? FSERID = null )
		{
			var viewModel = GetSpecificEntityViewModel<HWTPFinancialAssurance>( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitCertificationofFinancialAssurance, FSEID, FSERID );
			FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource );
			return View( viewModel );
		}

		public virtual ActionResult Handle_TieredPermittingUnitCertificationofFinancialAssurancePost( int organizationId, int CERSID, int FSEID, int? FSERID = null, FormCollection fc = null )
		{
			var viewModel = GetSpecificEntityViewModel<HWTPFinancialAssurance>( organizationId, CERSID, SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitCertificationofFinancialAssurance, FSEID, FSERID );
			var entity = viewModel.Entity;
			if ( TryUpdateModel( entity, "Entity" ) )
			{
				//get ParentID from resource type OnsiteHazardousWasteTreatmentNotificationFacility
				var parentResource = viewModel.FacilitySubmittalElement.Resources.Where( r => r.ResourceTypeID == (int)ResourceType.OnsiteHazardousWasteTreatmentNotificationFacility ).FirstOrDefault();
				if ( parentResource != null )
				{
					viewModel.FacilitySubmittalElementResource.ParentResourceID = parentResource.ID;
				}

				entity.FacilitySubmittalElementResource = viewModel.FacilitySubmittalElementResource;
				Repository.HWTPFinancialAssurances.Save( entity );

				entity.FacilitySubmittalElementResource.SetLastSubmittalDelta( entity, CERSID, Repository, "FacilitySubmittalElementResourceID" );
				if ( ( entity.FacilitySubmittalElementResource != null ) && ( !entity.FacilitySubmittalElementResource.IsStarted ) )
				{
					entity.FacilitySubmittalElementResource.SetIsStarted( Repository );
				}
				// null out state if international address
				if ( !( entity.FinancialInstitutionCountry == "United States" || entity.FinancialInstitutionCountry == "Canada" ) )
				{
					entity.FinancialInstitutionState = null;
				}
				viewModel.ValidateAndCommitResults( Repository, CallerContext.UI );
				string anchor = viewModel.FacilitySubmittalElement.SubmittalElement.Acronym;
				return RedirectToFacilityDraftSubmittals( viewModel.OrganizationID, viewModel.CERSID, "#" + anchor );
			}
			return View( viewModel );
		}

		#endregion TieredPermittingUnitCertificationofFinancialAssurance

		#endregion Handler Methods
	}
}