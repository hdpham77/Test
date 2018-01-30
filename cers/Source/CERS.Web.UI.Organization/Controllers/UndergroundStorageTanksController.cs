using CERS.Model;
using CERS.Reports;
using CERS.ViewModels;
using CERS.ViewModels.UndergroundStorageTanks;
using CERS.Web.Mvc;
using System;
using System.Linq;
using System.Web.Mvc;
using UPF.Web.Mvc;

namespace CERS.Web.UI.Organization.Controllers
{
    [Authorize]
    public class UndergroundStorageTanksController :SubmittalElementControllerBase
    {
        //
        // GET: /UndergroundStorageTanks/

        public ActionResult Index()
        {
            return View();
        }

        #region USTStart

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
        public ActionResult USTStart( int organizationId, int CERSID, int? FSEID )
        {
            // Call the Facility Services' GetCurrentDraft method to see if a Draft UST Submittal Element
            // exists for this CERSID:
            var fse = Services.Facilities.GetCurrentDraft( CERSID, SubmittalElementType.UndergroundStorageTanks );

            // If no Draft Facility Submittal Element is found, call Facility Services method
            // GetFacilitySubmittalElement and persist the changes to the database:
            if ( fse == null )
            {
                fse = Services.BusinessLogic.SubmittalElements.GetFacilitySubmittalElement( CERSID, SubmittalElementType.UndergroundStorageTanks, SubmittalElementStatus.Draft, true );

                // If an FSE ID is provided, clone entities from that FSE to the newly-created FSE
                if ( FSEID.HasValue )
                {
                    Repository.FacilitySubmittalElements.CloneWithEntities( CERSID, FSEID.Value, fse );
                }
            }

            //Validate this submittal element
            fse.ValidateAndCommitResults( Repository, CallerContext.UI );

            // First, retrieve the UST Facility Info Resource for this FSE so we can check the Entity Count.
            // If we have cloned entities, it is very likely to have a UST Facility Info entity, but is not
            // guaranteed.
            var ustFacilityInfoFSER = fse.Resources.Where( fserModel => fserModel.ResourceTypeID == (int)ResourceType.USTOperatingPermitApplicationFacilityInformation && !fserModel.Voided ).SingleOrDefault();

            // ustFacilityInfoFSER *should* always be found, if the FSE was created properly. If it is null,
            // throw an exception
            if ( ustFacilityInfoFSER == null )
            {
                throw new Exception( "UST Facility Info Resource not found in UST Facility Submittal Element.  Cannot determine if UST Facility Info Entity Exists." );
            }

            // *****
            // * Default Type of Action for Cloned USTFacilityInfo and USTTankInfo Entities to
            //   "Confirmed/Updated Information"
            // *****

            // Retrieve USTFacilityInfo Entity (if it exists) and default to "Confirmed/Updated Information"
            // ("5" in CDR/UPDD)
            var ustFacilityInfo = ustFacilityInfoFSER.USTFacilityInfoes.FetchFirstUnVoided();
            if ( ustFacilityInfo != null )
            {
                // If previous value of Type of Action was "New Permit" ("1" in CDR/UPDD), update Type of Action
                // to "Confirmed/Updated Information" ("5" in CDR/UPDD), and Save:
                if ( ustFacilityInfo.TypeOfAction == null || ustFacilityInfo.TypeOfAction == "1" )
                {
                    ustFacilityInfo.TypeOfAction = "5";
                    Repository.USTFacilityInfos.Save( ustFacilityInfo );
                }
            }

            // Retrieve USTTankInfo FSERs
            var ustTankInfoFSERs = fse.Resources.Where( fserModel => fserModel.ResourceTypeID == (int)ResourceType.USTOperatingPermitApplicationTankInformation && !fserModel.Voided );
            if ( ustTankInfoFSERs != null )
            {
                foreach ( var ustTankInfoFSER in ustTankInfoFSERs )
                {
                    var ustTankInfo = ustTankInfoFSER.USTTankInfos.FetchFirstUnVoided();
                    if ( ustTankInfo != null )
                    {
                        // If previous value of Type of Action was "New Permit" ("1" in CDR/UPDD), update Type of
                        // Action to "Confirmed/Updated Information" ("5" in CDR/UPDD), and Save:
                        if ( ustTankInfo.TypeOfAction == null || ustTankInfo.TypeOfAction == "1" )
                        {
                            ustTankInfo.TypeOfAction = "5";
                            Repository.USTTankInfos.Save( ustTankInfo );
                        }
                    }
                }
            }

            // If Resource Entity Count is null or zero, redirect user to UST Facility Info Create Action
            if ( ustFacilityInfoFSER.ResourceEntityCount == null || ustFacilityInfoFSER.ResourceEntityCount == 0 )
            {
                string routeName = GetDraftCreateRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationFacilityInformation );
                return RedirectToRoute( routeName, new { organizationID = organizationId, CERSID = CERSID, FSEID = fse.ID } );
            }

            // Otherwise, if Resource Entity Count greater than zero, redirect user to UST Facility Info Edit
            // Action
            else
            {
                string routeName = GetDraftEditRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationFacilityInformation );
                return RedirectToRoute( routeName, new { organizationID = organizationId, CERSID = CERSID, FSEID = fse.ID } );
            }
        }

        #endregion USTStart

        #region UST Operating Permit Application: Facility Information

        #region Detail_USTOperatingPermitApplicationFacilityInformation

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
        public ActionResult Detail_USTOperatingPermitApplicationFacilityInformation( int organizationId, int CERSID, int FSEID )
        {
            var viewModel = GetSingleEntityViewModel<USTFacilityInfo>( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationFacilityInformation, FSEID );
            FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource );
            return View( viewModel );
        }

        public void Detail_USTOperatingPermitApplicationFacilityInformation_Print( int organizationId, int CERSID, int FSEID )
        {
            Services.Reports.USTFacilityInformationReportBLL( organizationId, CERSID, FSEID );
        }

        #endregion Detail_USTOperatingPermitApplicationFacilityInformation

        #region Create_USTOperatingPermitApplicationFacilityInformation

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Create_USTOperatingPermitApplicationFacilityInformation( int organizationId, int CERSID, int FSEID )
        {
            return Handle_USTOperatingPermitApplicationFacilityInformationGet( organizationId, CERSID, FSEID );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Create_USTOperatingPermitApplicationFacilityInformation( int organizationId, int CERSID, int FSEID, FormCollection form )
        {
            return Handle_USTOperatingPermitApplicationFacilityInformationPost( organizationId, CERSID, FSEID );
        }

        #endregion Create_USTOperatingPermitApplicationFacilityInformation

        #region Edit_USTOperatingPermitApplicationFacilityInformation

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Edit_USTOperatingPermitApplicationFacilityInformation( int organizationId, int CERSID, int FSEID )
        {
            return Handle_USTOperatingPermitApplicationFacilityInformationGet( organizationId, CERSID, FSEID );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Edit_USTOperatingPermitApplicationFacilityInformation( int organizationId, int CERSID, int FSEID, FormCollection form )
        {
            return Handle_USTOperatingPermitApplicationFacilityInformationPost( organizationId, CERSID, FSEID );
        }

        #endregion Edit_USTOperatingPermitApplicationFacilityInformation

        #region Delete_USTOperatingPermitApplicationFacilityInformation

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Delete_USTOperatingPermitApplicationFacilityInformation( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
        }

        #endregion Delete_USTOperatingPermitApplicationFacilityInformation

        #endregion UST Operating Permit Application: Facility Information

        #region UST Operating Permit Application: Tank Information

        #region Detail_USTOperatingPermitApplicationTankInformation

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
        public ActionResult Detail_USTOperatingPermitApplicationTankInformation( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            var viewModel = GetSpecificEntityViewModel<USTTankInfo>( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationTankInformation, FSEID, FSERID );
            FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource );
            return View( viewModel );
        }

        public void Detail_USTOperatingPermitApplicationTankInformation_Print( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            Services.Reports.USTTankInformationReportBLL( organizationId, CERSID, FSEID, FSERID );
        }

        #endregion Detail_USTOperatingPermitApplicationTankInformation

        #region Create_USTOperatingPermitApplicationTankInformation

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Create_USTOperatingPermitApplicationTankInformation( int organizationId, int CERSID, int FSEID, int? copyFromUSTTankID )
        {
            return Handle_USTOperatingPermitApplicationTankInformationGet( organizationId, CERSID, FSEID, copyFromUSTTankID:copyFromUSTTankID );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Create_USTOperatingPermitApplicationTankInformation( int organizationId, int CERSID, int FSEID, FormCollection form )
        {
            return Handle_USTOperatingPermitApplicationTankInformationPost( organizationId, CERSID, FSEID );
        }

        #endregion Create_USTOperatingPermitApplicationTankInformation

        #region Edit_USTOperatingPermitApplicationTankInformation

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Edit_USTOperatingPermitApplicationTankInformation( int organizationId, int CERSID, int FSEID, int FSERID, int? copyFromUSTTankID )
        {
            return Handle_USTOperatingPermitApplicationTankInformationGet( organizationId, CERSID, FSEID, FSERID, copyFromUSTTankID );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Edit_USTOperatingPermitApplicationTankInformation( int organizationId, int CERSID, int FSEID, int FSERID, FormCollection form )
        {
            return Handle_USTOperatingPermitApplicationTankInformationPost( organizationId, CERSID, FSEID, FSERID );
        }

        #endregion Edit_USTOperatingPermitApplicationTankInformation

        #region Delete_USTOperatingPermitApplicationTankInformation

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Delete_USTOperatingPermitApplicationTankInformation( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
        }

        #endregion Delete_USTOperatingPermitApplicationTankInformation

        #region Ajax_USTTankInfo_SelectOneToCopy

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Ajax_USTTankInfo_CopySelectOneGrid( int organizationId, int CERSID, int FSEID, bool isCreateAction, int? FSERID = null, int? ustTankInfoID = null )
        {
            var viewModel = new USTTankInfoViewModel()
            {
                IsCreateAction = isCreateAction,
                OrganizationID = organizationId,
                CERSID = CERSID,
                FSEID = FSEID,
                FSERID = FSERID,
                OtherUSTTankInfos = Repository.USTTankInfos.Search( string.Empty, null, FSEID ).DefaultIfEmpty().Where( t => ustTankInfoID != null ? t.ID != ustTankInfoID : true ),
            };

            // Retrieve current UST Tank Info (if FSERID is provided)
            if ( FSERID.HasValue && FSERID.Value > 0 )
            {
                //viewModel.Entity = Repository.USTMonitoringPlan.GetByFSERID(FSERID.Value);
            }

            // Convert list of other UST Tank Infos to Grid View
            viewModel.OtherUSTTankInfosGridView = viewModel.OtherUSTTankInfos.ToGridView();

            var results = RenderPartialViewToString( "_USTTankInfoCopySelectOneGrid", viewModel );
            bool success = true;
            return Json( new { success = success, message = results } );
        }

        #endregion Ajax_USTTankInfo_SelectOneToCopy

        #endregion UST Operating Permit Application: Tank Information

        #region UST Monitoring Plan

        #region Detail_USTMonitoringPlan

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
        public ActionResult Detail_USTMonitoringPlan( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            var viewModel = GetSpecificEntityViewModel<USTMonitoringPlan>( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringPlan, FSEID, FSERID );
            FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource );
            return View( viewModel );
        }

        public void Detail_USTMonitoringPlan_Print( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            Services.Reports.USTMonitoringPlanReportBLL( organizationId, CERSID, FSEID, FSERID );
        }

        #endregion Detail_USTMonitoringPlan

        #region Create_USTMonitoringPlan

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Create_USTMonitoringPlan( int organizationId, int CERSID, int FSEID, int USTTankInfoFSERID, int? copyFromUSTMonitoringPlanID )
        {
            // Specify FSERID for UST Tank Info to link new UST Monitoring Plan to:
            return Handle_USTMonitoringPlanGet( organizationId, CERSID, FSEID, USTTankInfoFSERID:USTTankInfoFSERID, copyFromUSTMonitoringPlanID:copyFromUSTMonitoringPlanID );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Create_USTMonitoringPlan( int organizationId, int CERSID, int FSEID, int USTTankInfoFSERID, FormCollection form )
        {
            // Specify FSERID for UST Tank Info to link new UST Monitoring Plan to:
            return Handle_USTMonitoringPlanPost( organizationId, CERSID, form, FSEID, USTTankInfoFSERID:USTTankInfoFSERID );
        }

        #endregion Create_USTMonitoringPlan

        #region Edit_USTMonitoringPlan

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Edit_USTMonitoringPlan( int organizationId, int CERSID, int FSEID, int FSERID, int? copyFromUSTMonitoringPlanID )
        {
            return Handle_USTMonitoringPlanGet( organizationId, CERSID, FSEID, FSERID, copyFromUSTMonitoringPlanID:copyFromUSTMonitoringPlanID );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Edit_USTMonitoringPlan( int organizationId, int CERSID, int FSEID, int FSERID, FormCollection form )
        {
            return Handle_USTMonitoringPlanPost( organizationId, CERSID, form, FSEID, FSERID );
        }

        #endregion Edit_USTMonitoringPlan

        #region Delete_USTMonitoringPlan

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Delete_USTMonitoringPlan( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
        }

        #endregion Delete_USTMonitoringPlan

        #region Ajax_USTMonitoringPlan_SelectOneToCopy

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Ajax_USTMonitoringPlan_CopySelectOneGrid( int organizationId, int CERSID, int FSEID, bool isCreateAction, int? FSERID = null, int? USTTankInfoFSERID = null )
        {
            var viewModel = new USTMonitoringPlanViewModel()
            {
                IsCreateAction = isCreateAction,
                OrganizationID = organizationId,
                CERSID = CERSID,
                FSEID = FSEID,
                USTTankInfoFSERID = USTTankInfoFSERID,
                FSERID = FSERID,
                OtherUSTMonitoringPlans = Repository.USTMonitoringPlans.GetByFSEID( FSEID, excludeFSERID:FSERID ).DefaultIfEmpty()
            };

            // Retrieve current Monitoring Plan (if FSERID is provided)
            if ( FSERID.HasValue && FSERID.Value > 0 )
            {
                viewModel.Entity = Repository.USTMonitoringPlans.GetByFSERID( FSERID.Value );
            }

            // Convert list of other UST Montioring Plans to Grid View
            viewModel.OtherUSTMonitoringPlansGridView = viewModel.OtherUSTMonitoringPlans.ToGridView();

            var results = RenderPartialViewToString( "_USTMonitoringPlanCopySelectOneGrid", viewModel );
            bool success = true;
            return Json( new { success = success, message = results } );
        }

        #endregion Ajax_USTMonitoringPlan_SelectOneToCopy

        #region Ajax_USTMonitoringPlan_SelectManyToReplace

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Ajax_USTMonitoringPlan_ReplaceSelectManyGrid( int organizationId, int CERSID, int FSEID, bool isCreateAction, int? FSERID = null, int? USTTankInfoFSERID = null )
        {
            var viewModel = new USTMonitoringPlanViewModel()
            {
                IsCreateAction = isCreateAction,
                OrganizationID = organizationId,
                CERSID = CERSID,
                FSEID = FSEID,
                USTTankInfoFSERID = USTTankInfoFSERID,
                FSERID = FSERID,
                OtherUSTMonitoringPlans = Repository.USTMonitoringPlans.GetByFSEID( FSEID, excludeFSERID:FSERID ).DefaultIfEmpty()
            };

            // Retrieve current Monitoring Plan (if FSERID is provided)
            if ( FSERID.HasValue && FSERID.Value > 0 )
            {
                viewModel.Entity = Repository.USTMonitoringPlans.GetByFSERID( FSERID.Value );
            }

            // Convert list of other UST Monitoring Plans to Grid View
            viewModel.OtherUSTMonitoringPlansGridView = viewModel.OtherUSTMonitoringPlans.ToGridView();

            var results = RenderPartialViewToString( "_USTMonitoringPlanReplaceSelectManyGrid", viewModel );
            bool success = true;
            return Json( new { success = success, message = results } );
        }

        #endregion Ajax_USTMonitoringPlan_SelectManyToReplace

        #endregion UST Monitoring Plan

        #region UST Certification of Installation / Modification

        #region Detail_USTCertificationofInstallationModification

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public void Detail_USTCertificationofInstallationModification_Print( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            Services.Reports.USTCertificationofInstallationModificationReportBLL( organizationId, CERSID, FSEID, FSERID );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Detail_USTCertificationofInstallationModification( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            var viewModel = GetSpecificEntityViewModel<USTInstallModCert>( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofInstallationModification, FSEID, FSERID );
            FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource );
            return View( viewModel );
        }

        #endregion Detail_USTCertificationofInstallationModification

        #region Create_USTCertificationofInstallationModification

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Create_USTCertificationofInstallationModification( int organizationId, int CERSID, int FSEID )
        {
            return Handle_USTCertificationofInstallationModificationGet( organizationId, CERSID, FSEID );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Create_USTCertificationofInstallationModification( int organizationId, int CERSID, int FSEID, FormCollection form )
        {
            return Handle_USTCertificationofInstallationModificationPost( organizationId, CERSID, FSEID );
        }

        #endregion Create_USTCertificationofInstallationModification

        #region Edit_USTCertificationofInstallationModification

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Edit_USTCertificationofInstallationModification( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            return Handle_USTCertificationofInstallationModificationGet( organizationId, CERSID, FSEID, FSERID );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Edit_USTCertificationofInstallationModification( int organizationId, int CERSID, int FSEID, int FSERID, FormCollection form )
        {
            return Handle_USTCertificationofInstallationModificationPost( organizationId, CERSID, FSEID, FSERID );
        }

        #endregion Edit_USTCertificationofInstallationModification

        #region Delete_USTCertificationofInstallationModification

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Delete_USTCertificationofInstallationModification( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
        }

        #endregion Delete_USTCertificationofInstallationModification

        #endregion UST Certification of Installation / Modification

        #region Supplemental Document Actions

        #region USTMonitoringSitePlan_Document

        #region Detail_USTMonitoringSitePlan_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
        public ActionResult Detail_USTMonitoringSitePlan_Document( int organizationId, int CERSID, int FSEID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringSitePlan_Document, FSEID );
        }

        #endregion Detail_USTMonitoringSitePlan_Document

        #region Create_USTMonitoringSitePlan_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Create_USTMonitoringSitePlan_Document( int organizationId, int CERSID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringSitePlan_Document );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Create_USTMonitoringSitePlan_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
        {
            return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringSitePlan_Document, viewModel );
        }

        #endregion Create_USTMonitoringSitePlan_Document

        #region Edit_USTMonitoringSitePlan_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Edit_USTMonitoringSitePlan_Document( int organizationId, int CERSID, int FSEID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringSitePlan_Document );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Edit_USTMonitoringSitePlan_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
        {
            return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringSitePlan_Document, viewModel );
        }

        #endregion Edit_USTMonitoringSitePlan_Document

        #region Delete_USTMonitoringSitePlan_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Delete_USTMonitoringSitePlan_Document( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
        }

        #endregion Delete_USTMonitoringSitePlan_Document

        #endregion USTMonitoringSitePlan_Document

        #region USTCertificationofFinancialResponsibility_Document

        #region Detail_USTCertificationofFinancialResponsibility_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
        public ActionResult Detail_USTCertificationofFinancialResponsibility_Document( int organizationId, int CERSID, int FSEID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofFinancialResponsibility_Document, FSEID );
        }

        #endregion Detail_USTCertificationofFinancialResponsibility_Document

        #region Create_USTCertificationofFinancialResponsibility_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Create_USTCertificationofFinancialResponsibility_Document( int organizationId, int CERSID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofFinancialResponsibility_Document );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Create_USTCertificationofFinancialResponsibility_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
        {
            return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofFinancialResponsibility_Document, viewModel );
        }

        #endregion Create_USTCertificationofFinancialResponsibility_Document

        #region Edit_USTCertificationofFinancialResponsibility_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Edit_USTCertificationofFinancialResponsibility_Document( int organizationId, int CERSID, int FSEID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofFinancialResponsibility_Document );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Edit_USTCertificationofFinancialResponsibility_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
        {
            return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofFinancialResponsibility_Document, viewModel );
        }

        #endregion Edit_USTCertificationofFinancialResponsibility_Document

        #region Delete_USTCertificationofFinancialResponsibility_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Delete_USTCertificationofFinancialResponsibility_Document( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
        }

        #endregion Delete_USTCertificationofFinancialResponsibility_Document

        #endregion USTCertificationofFinancialResponsibility_Document

        #region USTResponsePlan_Document

        #region Detail_USTResponsePlan_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
        public ActionResult Detail_USTResponsePlan_Document( int organizationId, int CERSID, int FSEID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTResponsePlan_Document, FSEID );
        }

        #endregion Detail_USTResponsePlan_Document

        #region Create_USTResponsePlan_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Create_USTResponsePlan_Document( int organizationId, int CERSID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTResponsePlan_Document );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Create_USTResponsePlan_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
        {
            return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTResponsePlan_Document, viewModel );
        }

        #endregion Create_USTResponsePlan_Document

        #region Edit_USTResponsePlan_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Edit_USTResponsePlan_Document( int organizationId, int CERSID, int FSEID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTResponsePlan_Document );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Edit_USTResponsePlan_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
        {
            return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTResponsePlan_Document, viewModel );
        }

        #endregion Edit_USTResponsePlan_Document

        #region Delete_USTResponsePlan_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Delete_USTResponsePlan_Document( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
        }

        #endregion Delete_USTResponsePlan_Document

        #endregion USTResponsePlan_Document

        #region USTOwnerandUSTOperatorWrittenAgreement_Document

        #region Detail_USTOwnerandUSTOperatorWrittenAgreement_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
        public ActionResult Detail_USTOwnerandUSTOperatorWrittenAgreement_Document( int organizationId, int CERSID, int FSEID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOwnerandUSTOperatorWrittenAgreement_Document, FSEID );
        }

        #endregion Detail_USTOwnerandUSTOperatorWrittenAgreement_Document

        #region Create_USTOwnerandUSTOperatorWrittenAgreement_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Create_USTOwnerandUSTOperatorWrittenAgreement_Document( int organizationId, int CERSID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOwnerandUSTOperatorWrittenAgreement_Document );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Create_USTOwnerandUSTOperatorWrittenAgreement_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
        {
            return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOwnerandUSTOperatorWrittenAgreement_Document, viewModel );
        }

        #endregion Create_USTOwnerandUSTOperatorWrittenAgreement_Document

        #region Edit_USTOwnerandUSTOperatorWrittenAgreement_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Edit_USTOwnerandUSTOperatorWrittenAgreement_Document( int organizationId, int CERSID, int FSEID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOwnerandUSTOperatorWrittenAgreement_Document );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Edit_USTOwnerandUSTOperatorWrittenAgreement_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
        {
            return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOwnerandUSTOperatorWrittenAgreement_Document, viewModel );
        }

        #endregion Edit_USTOwnerandUSTOperatorWrittenAgreement_Document

        #region Delete_USTOwnerandUSTOperatorWrittenAgreement_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Delete_USTOwnerandUSTOperatorWrittenAgreement_Document( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
        }

        #endregion Delete_USTOwnerandUSTOperatorWrittenAgreement_Document

        #endregion USTOwnerandUSTOperatorWrittenAgreement_Document

        #region USTLetterfromtheChiefFinancialOfficer_Document

        #region Detail_USTLetterfromtheChiefFinancialOfficer_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
        public ActionResult Detail_USTLetterfromtheChiefFinancialOfficer_Document( int organizationId, int CERSID, int FSEID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLetterfromtheChiefFinancialOfficer_Document, FSEID );
        }

        #endregion Detail_USTLetterfromtheChiefFinancialOfficer_Document

        #region Create_USTLetterfromtheChiefFinancialOfficer_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Create_USTLetterfromtheChiefFinancialOfficer_Document( int organizationId, int CERSID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLetterfromtheChiefFinancialOfficer_Document );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Create_USTLetterfromtheChiefFinancialOfficer_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
        {
            return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLetterfromtheChiefFinancialOfficer_Document, viewModel );
        }

        #endregion Create_USTLetterfromtheChiefFinancialOfficer_Document

        #region Edit_USTLetterfromtheChiefFinancialOfficer_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Edit_USTLetterfromtheChiefFinancialOfficer_Document( int organizationId, int CERSID, int FSEID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLetterfromtheChiefFinancialOfficer_Document );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Edit_USTLetterfromtheChiefFinancialOfficer_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
        {
            return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLetterfromtheChiefFinancialOfficer_Document, viewModel );
        }

        #endregion Edit_USTLetterfromtheChiefFinancialOfficer_Document

        #region Delete_USTLetterfromtheChiefFinancialOfficer_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Delete_USTLetterfromtheChiefFinancialOfficer_Document( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
        }

        #endregion Delete_USTLetterfromtheChiefFinancialOfficer_Document

        #endregion USTLetterfromtheChiefFinancialOfficer_Document

        #region OwnerStatementofDesignatedUSTOperatorCompliance_Document

        #region Detail_OwnerStatementofDesignatedUSTOperatorCompliance_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
        public ActionResult Detail_OwnerStatementofDesignatedUSTOperatorCompliance_Document( int organizationId, int CERSID, int FSEID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.OwnerStatementofDesignatedUSTOperatorCompliance_Document, FSEID );
        }

        #endregion Detail_OwnerStatementofDesignatedUSTOperatorCompliance_Document

        #region Create_OwnerStatementofDesignatedUSTOperatorCompliance_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Create_OwnerStatementofDesignatedUSTOperatorCompliance_Document( int organizationId, int CERSID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.OwnerStatementofDesignatedUSTOperatorCompliance_Document );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Create_OwnerStatementofDesignatedUSTOperatorCompliance_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
        {
            return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.OwnerStatementofDesignatedUSTOperatorCompliance_Document, viewModel );
        }

        #endregion Create_OwnerStatementofDesignatedUSTOperatorCompliance_Document

        #region Edit_OwnerStatementofDesignatedUSTOperatorCompliance_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Edit_OwnerStatementofDesignatedUSTOperatorCompliance_Document( int organizationId, int CERSID, int FSEID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.OwnerStatementofDesignatedUSTOperatorCompliance_Document );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Edit_OwnerStatementofDesignatedUSTOperatorCompliance_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
        {
            return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.OwnerStatementofDesignatedUSTOperatorCompliance_Document, viewModel );
        }

        #endregion Edit_OwnerStatementofDesignatedUSTOperatorCompliance_Document

        #region Delete_OwnerStatementofDesignatedUSTOperatorCompliance_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Delete_OwnerStatementofDesignatedUSTOperatorCompliance_Document( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
        }

        #endregion Delete_OwnerStatementofDesignatedUSTOperatorCompliance_Document

        #endregion OwnerStatementofDesignatedUSTOperatorCompliance_Document

        #region USTLocallyRequired_Document

        #region Detail_USTLocallyRequired_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
        public ActionResult Detail_USTLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLocallyRequired_Document, FSEID );
        }

        #endregion Detail_USTLocallyRequired_Document

        #region Create_USTLocallyRequired_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Create_USTLocallyRequired_Document( int organizationId, int CERSID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLocallyRequired_Document );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Create_USTLocallyRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
        {
            return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLocallyRequired_Document, viewModel );
        }

        #endregion Create_USTLocallyRequired_Document

        #region Edit_USTLocallyRequired_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Edit_USTLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLocallyRequired_Document );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Edit_USTLocallyRequired_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
        {
            return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLocallyRequired_Document, viewModel );
        }

        #endregion Edit_USTLocallyRequired_Document

        #region Delete_USTLocallyRequired_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Delete_USTLocallyRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
        }

        #endregion Delete_USTLocallyRequired_Document

        #endregion USTLocallyRequired_Document

        #region USTMiscellaneousStateRequired_Document

        #region Detail_USTLocallyRequired_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
        public ActionResult Detail_USTMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMiscellaneousStateRequired_Document, FSEID );
        }

        #endregion Detail_USTLocallyRequired_Document

        #region Create_USTLocallyRequired_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Create_USTMiscellaneousStateRequired_Document( int organizationId, int CERSID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMiscellaneousStateRequired_Document );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Create_USTMiscellaneousStateRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
        {
            return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMiscellaneousStateRequired_Document, viewModel );
        }

        #endregion Create_USTLocallyRequired_Document

        #region Edit_USTLocallyRequired_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Edit_USTMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
        {
            return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMiscellaneousStateRequired_Document );
        }

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        [HttpPost]
        public ActionResult Edit_USTMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
        {
            return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMiscellaneousStateRequired_Document, viewModel );
        }

        #endregion Edit_USTLocallyRequired_Document

        #region Delete_USTLocallyRequired_Document

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
        public ActionResult Delete_USTMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
        {
            return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
        }

        #endregion Delete_USTLocallyRequired_Document

        #endregion USTMiscellaneousStateRequired_Document

        #endregion Supplemental Document Actions

        #region Handler Methods

        #region USTFacilityInfo

        protected virtual ActionResult Handle_USTOperatingPermitApplicationFacilityInformationGet( int organizationId, int CERSID, int? fseID = null )
        {
            // Retrieve the two standard Exempt BOE Numbers for State and Federal, and populate ViewBag to
            // make available in the FSERView:
            try
            {
                ViewBag.BOENumberStateExempt = Repository.Settings.GetByKey( "BOENumberStateExempt" ).Value;
                ViewBag.BOENumberFederalExempt = Repository.Settings.GetByKey( "BOENumberFederalExempt" ).Value;
            }
            catch
            {
                // Changed "catch (Exception ex)" to "catch" to suppress compilation warning; we do not do
                // anything meaningful with "ex" in this scenario. If we need to do something meaningful with
                // the Exception in the future, we can change it back.
                ViewBag.BOENumberStateExempt = "";
                ViewBag.BOENumberFederalExempt = "";
            }

            var viewModel = GetSingleEntityViewModel<USTFacilityInfo>( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationFacilityInformation, fseID );
            FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource );

            // If this is a brand new UST Facility Info form, default action type to 'New Permit'
            if ( viewModel.Entity.ID == 0 )
            {
                // In UPDD/CDR, Type of Action "1" = New Permit
                viewModel.Entity.TypeOfAction = "1";
            }
            return View( viewModel );
        }

        protected virtual ActionResult Handle_USTOperatingPermitApplicationFacilityInformationPost( int organizationId, int CERSID, int? fseID = null )
        {
            // Retrieve the two standard Exempt BOE Numbers for State and Federal, and populate ViewBag to
            // make available in the FSERView:
            try
            {
                ViewBag.BOENumberStateExempt = Repository.Settings.GetByKey( "BOENumberStateExempt" ).Value;
                ViewBag.BOENumberFederalExempt = Repository.Settings.GetByKey( "BOENumberFederalExempt" ).Value;
            }
            catch
            {
                // Changed "catch (Exception ex)" to "catch" to suppress compilation warning; we do not do
                // anything meaningful with "ex" in this scenario. If we need to do something meaningful with
                // the Exception in the future, we can change it back.
                ViewBag.BOENumberStateExempt = "";
                ViewBag.BOENumberFederalExempt = "";
            }

            var viewModel = GetSingleEntityViewModel<USTFacilityInfo>( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationFacilityInformation, fseID );
            var entity = viewModel.Entity;
            if ( TryUpdateModel( entity, "Entity" ) )
            {
                // null out state if international address
                if ( !( entity.POCountry == "United States" || entity.POCountry == "Canada" ) )
                {
                    entity.POState = null;
                }
                if ( !( entity.TankOperatorCountry == "United States" || entity.TankOperatorCountry == "Canada" ) )
                {
                    entity.TankOperatorState = null;
                }
                if ( !( entity.TOwnerCountry == "United States" || entity.TOwnerCountry == "Canada" ) )
                {
                    entity.TOwnerState = null;
                }

                if ( ModelState.IsValid )
                {
                    entity.FacilitySubmittalElementResource = viewModel.FacilitySubmittalElementResource;
                    Repository.USTFacilityInfos.Save( entity );

                    // Set IsStarted Flag on FSER Record to True, if it is not already set to True:
                    if ( ( entity.FacilitySubmittalElementResource != null ) && !entity.FacilitySubmittalElementResource.IsStarted )
                    {
                        entity.FacilitySubmittalElementResource.SetIsStarted( Repository );
                    }

                    //Set LastSubmittalDeltaId
                    entity.FacilitySubmittalElementResource.SetLastSubmittalDelta( entity, CERSID, Repository );

                    viewModel.ValidateAndCommitResults( Repository, CallerContext.UI );

                    // If no UST Tanks exist, send the user directly to the UST Tank Create Route
                    if ( viewModel.FacilitySubmittalElement != null )
                    {
                        if ( viewModel.FacilitySubmittalElement.Resources != null )
                        {
                            // Only go to UST Tank Info Create Route if no UST Tank FSERs exist with Resource Entity
                            // Counts
                            if ( viewModel.FacilitySubmittalElement.Resources.Count( fserModel => fserModel.ResourceTypeID == (int)ResourceType.USTOperatingPermitApplicationTankInformation &&
                                                                                                fserModel.ResourceEntityCount > 0 &&
                                                                                               !fserModel.Voided ) == 0 )
                            {
                                string routeName = GetDraftCreateRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationTankInformation );
                                return RedirectToRoute( routeName, new { organizationID = organizationId, CERSID = CERSID, FSEID = viewModel.FacilitySubmittalElement.ID } );
                            }

                            // Otherwise, UST Tank Info records already exist; send user to Facility Draft Submittals
                            // Page
                            else
                            {
                                string anchor = viewModel.FacilitySubmittalElement.SubmittalElement.Acronym;
                                return RedirectToFacilityDraftSubmittals( viewModel.OrganizationID, viewModel.CERSID, "#" + anchor );
                            }
                        }
                    }
                }
            }

            return View( viewModel );
        }

        #endregion USTFacilityInfo

        #region USTTankInfo

        protected virtual ActionResult Handle_USTOperatingPermitApplicationTankInformationGet( int organizationId, int CERSID, int? fseID = null, int? fserID = null, int? copyFromUSTTankID = null )
        {
            var viewModel = GetSpecificEntityViewModel<USTTankInfo>( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationTankInformation, fseID, fserID );

            if ( copyFromUSTTankID.HasValue )
            {
                var copyFromUSTTankInfo = Repository.USTTankInfos.GetByID( copyFromUSTTankID.Value );
                Repository.USTTankInfos.Copy( viewModel.Entity, copyFromUSTTankInfo );

                Messages.Add( "The form contents below have been replaced with a copy of the chosen Tank ID.  Please make any necessary changes (if any) and click \"Save\".", MessageType.Success, "UST Tank Information" );
            }
            FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource );

            // If this is a brand new UST Tank Info form, default action type to 'New Permit'
            if ( viewModel.Entity.ID == 0 )
            {
                // In UPDD/CDR, Type of Action "1" = New Permit
                viewModel.Entity.TypeOfAction = "1";
            }
            return View( viewModel );
        }

        protected virtual ActionResult Handle_USTOperatingPermitApplicationTankInformationPost( int organizationId, int CERSID, int? fseID = null, int? fserID = null )
        {
            var viewModel = GetSpecificEntityViewModel<USTTankInfo>( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationTankInformation, fseID, fserID );
            var entity = viewModel.Entity;
            if ( TryUpdateModel( entity, "Entity" ) )
            {
                if ( ModelState.IsValid )
                {
                    entity.FacilitySubmittalElementResource = viewModel.FacilitySubmittalElementResource;

                    // Because USTTankInfo is a child of USTFacilityInfo, we must find the appropriate
                    // USTFacilityInfo record and map the entity to it (if it is new):
                    if ( entity.USTFacilityInfo == null )
                    {
                        FacilitySubmittalElementResource ustFacilityInfoFSER = viewModel.FacilitySubmittalElement.Resources.Where( fser => fser.ResourceTypeID == (int)ResourceType.USTOperatingPermitApplicationFacilityInformation ).SingleOrDefault();

                        // If a USTFacilityInfo resource has been found, link to the entity before saving:
                        if ( ustFacilityInfoFSER != null )
                        {
                            entity.FacilitySubmittalElementResource.ParentResourceID = ustFacilityInfoFSER.ID;
                            entity.USTFacilityInfo = ustFacilityInfoFSER.USTFacilityInfoes.FetchSingleUnVoided();
                        }
                    }
                    Repository.USTTankInfos.Save( entity );

                    //Set LastSubmittalDeltaId
                    entity.FacilitySubmittalElementResource.SetLastSubmittalDelta( entity, CERSID, Repository, "USTFacilityInfoID" );

                    // Set IsStarted Flag on FSER Record to True, if it is not already set to True:
                    if ( ( entity.FacilitySubmittalElementResource != null ) && !entity.FacilitySubmittalElementResource.IsStarted )
                    {
                        entity.FacilitySubmittalElementResource.SetIsStarted( Repository );
                    }

                    viewModel.ValidateAndCommitResults( Repository, CallerContext.UI );

                    // At this point, determine if a UST Monitoring Plan already exists for this UST Tank To
                    // accommodate issue found on 5/2/2012, fetch First UnVoided, although there should only be
                    // zero or one:
                    USTMonitoringPlan ustMonitoringPlan = entity.USTMonitoringPlans.FetchFirstUnVoided();
                    if ( ustMonitoringPlan != null )
                    {
                        // If exists, redirect to Edit for UST Monitoring Plan for the Monitoring Plan's FSERID
                        string routeName = GetDraftEditRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringPlan );
                        return RedirectToRoute( routeName, new { organizationID = organizationId, CERSID = CERSID, FSEID = viewModel.FacilitySubmittalElement.ID, FSERID = ustMonitoringPlan.FacilitySubmittalElementResourceID } );
                    }
                    else
                    {
                        // If no UST Monitoring Plan exists, redirect to Create for UST Monitoring Plan, and pass in
                        // TankInformationFSERID to help link the UST Monitoring Plan to the correct tank:
                        string routeName = GetDraftCreateRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringPlan );
                        return RedirectToRoute( routeName, new { organizationID = organizationId, CERSID = CERSID, FSEID = viewModel.FacilitySubmittalElement.ID, USTTankInfoFSERID = entity.FacilitySubmittalElementResourceID } );
                    }
                }
            }

            return View( viewModel );
        }

        #endregion USTTankInfo

        #region USTMonitoringPlan

        protected virtual ActionResult Handle_USTMonitoringPlanGet( int organizationId, int CERSID, int? fseID = null, int? fserID = null, int? USTTankInfoFSERID = null, int? copyFromUSTMonitoringPlanID = null )
        {
            var viewModel = GetSpecificEntityViewModel<USTMonitoringPlan>( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringPlan, fseID, fserID, USTTankInfoFSERID );

            if ( copyFromUSTMonitoringPlanID.HasValue )
            {
                var copyFromUSTMonitoringPlan = Repository.USTMonitoringPlans.GetByID( copyFromUSTMonitoringPlanID.Value );

                Repository.USTMonitoringPlans.Copy( viewModel.Entity, copyFromUSTMonitoringPlan, USTTankInfoFSERID );

                //Set LastSubmittalDeltaId
                viewModel.Entity.FacilitySubmittalElementResource.SetLastSubmittalDelta( viewModel.Entity, CERSID, Repository, "USTTankInfoID" );

                // Set IsStarted Flag on FSER Record to True, if it is not already set to True:
                if ( ( viewModel.Entity.FacilitySubmittalElementResource != null ) && !viewModel.Entity.FacilitySubmittalElementResource.IsStarted )
                {
                    viewModel.Entity.FacilitySubmittalElementResource.SetIsStarted( Repository );
                }

                viewModel.ValidateAndCommitResults( Repository, CallerContext.UI );

                // Retrieve from Repository a second time to obtain all updated values
                //viewModel = GetSpecificEntityViewModel<USTMonitoringPlan>(organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringPlan, fseID, fserID);

                Messages.Add( "The form contents below have been replaced with a copy of the chosen Monitoring Plan.  Please make any necessary changes (if any) and click \"Save\".", MessageType.Success, "UST Monitoring Plan" );

                if ( USTTankInfoFSERID.HasValue )
                {
                    string routeName = GetDraftEditRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringPlan );
                    return RedirectToRoute( routeName, new { organizationID = organizationId, CERSID = CERSID, FSEID = fseID, FSERID = viewModel.Entity.FacilitySubmittalElementResourceID } );
                }
            }

            FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource );

            // For new Monitoring Plans, attach to previously-created USTTankInfo record
            if ( USTTankInfoFSERID != null && viewModel.Entity.USTTankInfo == null )
            {
                FacilitySubmittalElementResource ustTankInfoFSER = viewModel.FacilitySubmittalElement.Resources.Where( fser => fser.ResourceTypeID == (int)ResourceType.USTOperatingPermitApplicationTankInformation && fser.ID == USTTankInfoFSERID.Value ).SingleOrDefault();

                // If a USTTankInfo resource has been found, link to the entity:
                if ( ustTankInfoFSER != null )
                {
                    viewModel.Entity.FacilitySubmittalElementResource.ParentResourceID = ustTankInfoFSER.ID;
                    viewModel.Entity.USTTankInfo = ustTankInfoFSER.USTTankInfos.FetchSingleUnVoided();
                }
            }
            return View( viewModel );
        }

        protected virtual ActionResult Handle_USTMonitoringPlanPost( int organizationId, int CERSID, FormCollection form, int? fseID = null, int? fserID = null, int? USTTankInfoFSERID = null )
        {
            var viewModel = GetSpecificEntityViewModel<USTMonitoringPlan>( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringPlan, fseID, fserID, USTTankInfoFSERID );
            var entity = viewModel.Entity;
            if ( TryUpdateModel( entity, "Entity" ) )
            {
                if ( ModelState.IsValid )
                {
                    entity.FacilitySubmittalElementResource = viewModel.FacilitySubmittalElementResource;

                    if ( entity.USTTankInfo == null )
                    {
                        FacilitySubmittalElementResource ustTankInfoFSER = viewModel.FacilitySubmittalElement.Resources.Where( fser => fser.ResourceTypeID == (int)ResourceType.USTOperatingPermitApplicationTankInformation && fser.ID == USTTankInfoFSERID.Value ).SingleOrDefault();

                        // If a USTTankInfo resource has been found, link to the entity before saving:
                        if ( ustTankInfoFSER != null )
                        {
                            entity.FacilitySubmittalElementResource.ParentResourceID = ustTankInfoFSER.ID;
                            entity.USTTankInfo = ustTankInfoFSER.USTTankInfos.FetchSingleUnVoided();
                        }
                    }

                    Repository.USTMonitoringPlans.Save( entity );

                    // If CopyToMonitoringPlanIDs are specified, copy the contents into the other Monitoring Plans
                    if ( form["CopyToMonitoringPlanIDs"] != null && form["CopyToMonitoringPlanIDs"].Length > 0 )
                    {
                        var copyToMonitoringPlanIDs = (string)form["CopyToMonitoringPlanIDs"];
                        foreach ( string copyToMonitoringPlanIDStr in copyToMonitoringPlanIDs.Split( ',' ) )
                        {
                            var copyToMonitoringPlanID = int.Parse( copyToMonitoringPlanIDStr );
                            var copyToUSTMonitoringPlan = Repository.USTMonitoringPlans.GetByID( copyToMonitoringPlanID );
                            Repository.USTMonitoringPlans.Copy( copyToUSTMonitoringPlan, entity, USTTankInfoFSERID );
                        }
                    }

                    //Set LastSubmittalDeltaId
                    entity.FacilitySubmittalElementResource.SetLastSubmittalDelta( entity, CERSID, Repository, "USTTankInfoID" );

                    // Set IsStarted Flag on FSER Record to True, if it is not already set to True:
                    if ( ( entity.FacilitySubmittalElementResource != null ) && !entity.FacilitySubmittalElementResource.IsStarted )
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

        #endregion USTMonitoringPlan

        #region USTInstallModCert

        protected virtual ActionResult Handle_USTCertificationofInstallationModificationGet( int organizationId, int CERSID, int? fseID = null, int? fserID = null )
        {
            var viewModel = GetSpecificEntityViewModel<USTInstallModCert>( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofInstallationModification, fseID, fserID );
            FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource );
            return View( viewModel );
        }

        protected virtual ActionResult Handle_USTCertificationofInstallationModificationPost( int organizationId, int CERSID, int? fseID = null, int? fserID = null )
        {
            var viewModel = GetSpecificEntityViewModel<USTInstallModCert>( organizationId, CERSID, SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofInstallationModification, fseID, fserID );
            var entity = viewModel.Entity;
            if ( TryUpdateModel( entity, "Entity" ) )
            {
                if ( ModelState.IsValid )
                {
                    entity.FacilitySubmittalElementResource = viewModel.FacilitySubmittalElementResource;

                    // Because USTInstallModCert is a child of USTFacilityInfo, we must find the appropriate
                    // USTFacilityInfo record and map the entity to it (if it is new):
                    if ( entity.USTFacilityInfo == null )
                    {
                        FacilitySubmittalElementResource ustFacilityInfoFSER = viewModel.FacilitySubmittalElement.Resources.Where( fser => fser.ResourceTypeID == (int)ResourceType.USTOperatingPermitApplicationFacilityInformation ).SingleOrDefault();

                        // If a USTFacilityInfo resource has been found, link to the entity before saving:
                        if ( ustFacilityInfoFSER != null )
                        {
                            entity.FacilitySubmittalElementResource.ParentResourceID = ustFacilityInfoFSER.ID;
                            entity.USTFacilityInfo = ustFacilityInfoFSER.USTFacilityInfoes.FetchSingleUnVoided();
                        }
                    }
                    Repository.USTInstallModCerts.Save( entity );

                    //Set LastSubmittalDeltaId
                    entity.FacilitySubmittalElementResource.SetLastSubmittalDelta( entity, CERSID, Repository, "USTFacilityInfoID" );

                    // Set IsStarted Flag on FSER Record to True, if it is not already set to True:
                    if ( ( entity.FacilitySubmittalElementResource != null ) && !entity.FacilitySubmittalElementResource.IsStarted )
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

        #endregion USTInstallModCert

        #endregion Handler Methods
    }
}