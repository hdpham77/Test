using CERS.AddressServices;
using CERS.Model;
using CERS.Reports;
using CERS.ViewModels;
using CERS.Web.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using UPF;

namespace CERS.Web.UI.Organization.Controllers
{
	[Authorize]
	public class FacilityInformationController : SubmittalElementControllerBase
	{
        #region CreateHMBPSubmittalElements

        [EntityFilterAuthorization(Context.Organization, "organizationId", PermissionRole.OrgViewer)]
        public ActionResult CreateHMBPSubmittalElements(int organizationId, int CERSID, int fseID_FI, int fseID_HMI, int fseID_ERTP)
        {
            var result = false;

            var currentDraftSubmittals = Repository.DataModel.uspGetCurrentDraftSubmittals( CERSID ).ToList();
            var draftExists = currentDraftSubmittals.Where( w =>
                    w.SubmittalElementID.IfInRange( (int)SubmittalElementType.FacilityInformation, (int)SubmittalElementType.HazardousMaterialsInventory, (int)SubmittalElementType.EmergencyResponseandTrainingPlans ) 
                    && w.DraftFacilitySubmittalElementID != null 
                    ).Count() > 0;

            if ( !draftExists )
            {
                //Cloning each HMBP submittal element
                Handle_Cloning( organizationId, CERSID, fseID_FI, SubmittalElementType.FacilityInformation );
                Handle_Cloning( organizationId, CERSID, fseID_HMI, SubmittalElementType.HazardousMaterialsInventory );
                Handle_Cloning( organizationId, CERSID, fseID_ERTP, SubmittalElementType.EmergencyResponseandTrainingPlans );
                result = true;
            }

            return Json( new { success = result } );
        }

        #endregion CreateHMBPSubmittalElements

		#region FacilityInfoHome

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult FacilityInfoHome( int organizationId, int CERSID, int FSEID )
		{
			return View();
		}

		#endregion FacilityInfoHome

		#region FacilityInfoStart

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult FacilityInfoStart( int organizationId, int CERSID, int? FSEID )
		{
			//Cloning Logic Start
			//check to see if we have a facility information in draft...
			var fseCurrentDraft = Services.Facilities.GetCurrentDraft( CERSID, SubmittalElementType.FacilityInformation );

			//If FSEID is passed in AND there isn't a current facility info in Draft start the cloning and redirect to crazy page.
			if ( FSEID.HasValue && fseCurrentDraft == null )
			{
				return Handle_Cloning( organizationId, CERSID, FSEID.Value, SubmittalElementType.FacilityInformation );
			}

			//Cloning Logic End

			// The first call was to check to see if there is already a draft. This call will persist a new
   // draft if one doesn't exists
			var fse = Services.BusinessLogic.SubmittalElements.GetFacilitySubmittalElement( CERSID, SubmittalElementType.FacilityInformation, SubmittalElementStatus.Draft, true );
			fse.ValidateAndCommitResults( Repository );

			//need to see if we already got a BPActivity form.
			string routeName = string.Empty;
			var resource = fse.Resources.SingleOrDefault( p => p.ResourceTypeID == (int)ResourceType.BusinessActivities && !p.Voided );
			if ( resource.ResourceEntityCount > 0 )
			{
				routeName = GetDraftEditRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessActivities );
			}
			else
			{
				routeName = GetDraftCreateRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessActivities );
			}
			return RedirectToRoute( routeName, new { organizationId = organizationId, CERSID = CERSID, FSEID = fse.ID } );
		}

		#endregion FacilityInfoStart

		#region Biz Activities

		#region Detail_BusinessActivities

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_BusinessActivities( int organizationId, int CERSID, int FSEID )
		{
			return Handle_BusinessActivitiesGet( organizationId, CERSID, FSEID );
		}

		public void Detail_BusinessActivities_Print( int organizationId, int CERSID, int FSEID )
		{
			Services.Reports.BusinessActivitiesReportBLL( organizationId, CERSID, FSEID );
		}

		#endregion Detail_BusinessActivities

		#region Create_BusinessActivities

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_BusinessActivities( int organizationId, int CERSID )
		{
			return Handle_BusinessActivitiesGet( organizationId, CERSID );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_BusinessActivities( int organizationId, int CERSID, FormCollection form )
		{
			return Handle_BusinessActivitiesPost( organizationId, CERSID, null );
		}

		#endregion Create_BusinessActivities

		#region Edit_BusinessActivities

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_BusinessActivities( int organizationId, int CERSID, int FSEID, int? FSERID )
		{
			return Handle_BusinessActivitiesGet( organizationId, CERSID, FSEID );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_BusinessActivities( int organizationId, int CERSID, int FSEID, FormCollection form )
		{
			return Handle_BusinessActivitiesPost( organizationId, CERSID, FSEID, form );
		}

		#endregion Edit_BusinessActivities

        #region FlagFacilityAsNonRegulated_Async

        [HttpPost]
        public JsonResult FlagFacilityAsNonRegulated_Async( int CERSID )
        {
            var frses = Repository.FacilityRegulatorSubmittalElements.GetForFacility( CERSID );
            var organizationID = frses.First().Facility.OrganizationID;
            var draftFI = frses.Where( p => (SubmittalElementType)p.SubmittalElementID == SubmittalElementType.FacilityInformation ).FirstOrDefault();
            if ( draftFI != null && draftFI.DraftFacilitySubmittalElement != null )
            {
                var evt = Services.Events.CreateFacilityOwnerOperatorReportingNoRegulatedActivitiesNotification( draftFI.DraftFacilitySubmittalElement );
            }

            foreach ( var frse in frses )
            {
                //discard current submittals (loop through all submittal elements) 
                if ( frse.DraftFacilitySubmittalElement != null )
                {
                    Repository.FacilitySubmittalElements.DiscardFacilitySubmittalElement( frse.DraftFacilitySubmittalElement );
                }

                //flag as non regulated
                if ( frse.ReportingRequirementID != (int)ReportingRequirement.NotApplicable )
                {
                    frse.ReportingRequirementID = (int)ReportingRequirement.NotApplicable;
                    Repository.FacilityRegulatorSubmittalElements.Save( frse );
                }
            }

            var link = RedirectToFacilityDraftSubmittals( organizationID: organizationID, CERSID: CERSID, extraInfo: "" );
            return Json( new { linkURL = link.Url } );
        }

        #endregion FlagFacilityAsNonRegulated_Async

        #endregion Biz Activities

        #region Owner Operator

        #region Detail_BusinessOwnerOperatorIdentification

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_BusinessOwnerOperatorIdentification( int organizationId, int CERSID, int FSEID )
		{
			return Handle_BizOwnerOperatorGet( organizationId, CERSID, FSEID );
		}

		public void Detail_BusinessOwnerOperatorIdentification_Print( int organizationId, int CERSID, int FSEID )
		{
			Services.Reports.BusinessOwnerOperatorReportBLL( organizationId, CERSID, FSEID );
		}

		#endregion Detail_BusinessOwnerOperatorIdentification

		#region Create_BusinessOwnerOperatorIdentification

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_BusinessOwnerOperatorIdentification( int organizationId, int CERSID, int FSEID )
		{
			return Handle_BizOwnerOperatorGet( organizationId, CERSID, FSEID );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_BusinessOwnerOperatorIdentification( int organizationId, int CERSID, int FSEID, FormCollection form )
		{
			return Handle_BizOwnerOperatorPost( organizationId, CERSID, FSEID );
		}

		#endregion Create_BusinessOwnerOperatorIdentification

		#region Edit_BusinessOwnerOperatorIdentification

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_BusinessOwnerOperatorIdentification( int organizationId, int CERSID, int FSEID, int? FSERID = null )
		{
			return Handle_BizOwnerOperatorGet( organizationId, CERSID, FSEID );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_BusinessOwnerOperatorIdentification( int organizationId, int CERSID, int FSEID, FormCollection form )
		{
			return Handle_BizOwnerOperatorPost( organizationId, CERSID, FSEID );
		}

		#endregion Edit_BusinessOwnerOperatorIdentification

		#endregion Owner Operator

		#region FacilityInformationLocallyRequired_Document

		#region Detail_FacilityInformationLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_FacilityInformationLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocallyRequired_Document, FSEID );
		}

		#endregion Detail_FacilityInformationLocallyRequired_Document

		#region Create_FacilityInformationLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_FacilityInformationLocallyRequired_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocallyRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_FacilityInformationLocallyRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocallyRequired_Document, viewModel );
		}

		#endregion Create_FacilityInformationLocallyRequired_Document

		#region Edit_FacilityInformationLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_FacilityInformationLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocallyRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_FacilityInformationLocallyRequired_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocallyRequired_Document, viewModel );
		}

		#endregion Edit_FacilityInformationLocallyRequired_Document

		#region Delete_LocallyRequiredDocument

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_FacilityInformationLocallyRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_LocallyRequiredDocument

		#endregion FacilityInformationLocallyRequired_Document

		#region FacilityInformationMiscellaneousStateRequired_Document

		#region Detail_FacilityInformationMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_FacilityInformationMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationMiscellaneousStateRequired_Document, FSEID );
		}

		#endregion Detail_FacilityInformationMiscellaneousStateRequired_Document

		#region Create_FacilityInformationMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_FacilityInformationMiscellaneousStateRequired_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationMiscellaneousStateRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_FacilityInformationMiscellaneousStateRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationMiscellaneousStateRequired_Document, viewModel );
		}

		#endregion Create_FacilityInformationMiscellaneousStateRequired_Document

		#region Edit_FacilityInformationMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_FacilityInformationMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationMiscellaneousStateRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_FacilityInformationMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationMiscellaneousStateRequired_Document, viewModel );
		}

		#endregion Edit_FacilityInformationMiscellaneousStateRequired_Document

		#region Delete_FacilityInformationMiscellaneousStateRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_FacilityInformationMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_FacilityInformationMiscellaneousStateRequired_Document

		#endregion FacilityInformationMiscellaneousStateRequired_Document

		#region Handler Methods

		#region BusinessActivities

		protected virtual ActionResult Handle_BusinessActivitiesGet( int organizationId, int CERSID, int? fseID = null )
		{
			var viewModel = GetSingleEntityViewModel<BPActivity>( organizationId, CERSID, SubmittalElementType.FacilityInformation, ResourceType.BusinessActivities, fseID );

			//See if BusinessName or address is null/empty.  If yes, get them from dbo.Facility.
			var bpActivities = viewModel.Entity;

			//Get the facility.
			var facility = Repository.Facilities.GetByID( CERSID );
			FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource );

			Services.BusinessLogic.SubmittalElements.FacilityInformation.InitializeDraftActivitiesForm( viewModel );

			//var siteAddress = (!string.IsNullOrWhiteSpace(bpActivities.SiteAddress)) ? bpActivities.SiteAddress : facility.WashedStreet;
			//var city = (!string.IsNullOrWhiteSpace(bpActivities.City)) ? bpActivities.City : facility.WashedCity;
			//var zipCode = (!string.IsNullOrWhiteSpace(bpActivities.ZipCode)) ? bpActivities.ZipCode : facility.WashedZipCode;

			viewModel.AddressInformation = new AddressInformation();
			viewModel.Facility = facility;

			CERSFacilityGeoPoint geoPoint = facility.CERSFacilityGeoPoint;

			//if geopoint is null create a generic one using the county centroid
			if ( geoPoint == null )
			{
				if ( facility.CountyID != null )
				{
					CountyGISCentroid countyCentroid = Repository.DataModel.CountyGISCentroids.SingleOrDefault( p => p.CountyID == facility.CountyID );

					if ( countyCentroid != null )
					{
						geoPoint = new CERSFacilityGeoPoint();
						geoPoint.LatitudeMeasure = countyCentroid.LatCentroid != null ? Convert.ToDecimal( countyCentroid.LatCentroid ) : 0;
						geoPoint.LongitudeMeasure = countyCentroid.LonCentroid != null ? Convert.ToDecimal( countyCentroid.LonCentroid ) : 0;
						geoPoint.HorizontalAccuracyMeasure = 100000;
						geoPoint.HorizontalCollectionMethodID = 104;
						geoPoint.HorizontalReferenceDatumID = 3;
						geoPoint.GeographicReferencePointID = 102;
						geoPoint.DataCollectionDate = DateTime.Now;
					}
				}
			}

			//if geopoint is still null setup a generic geopoint
			if ( geoPoint == null )
			{
				geoPoint = new CERSFacilityGeoPoint();
				geoPoint.LatitudeMeasure = 0;
				geoPoint.LongitudeMeasure = 0;
				geoPoint.HorizontalAccuracyMeasure = 100000;
				geoPoint.HorizontalCollectionMethodID = 104;
				geoPoint.HorizontalReferenceDatumID = 3;
				geoPoint.GeographicReferencePointID = 102;
				geoPoint.DataCollectionDate = DateTime.Now;
			}

			viewModel.CERSFacilityGeoPoint = geoPoint;

			return View( viewModel );
		}

		protected virtual ActionResult Handle_BusinessActivitiesPost( int organizationId, int CERSID, int? fseID = null, FormCollection fc = null )
		{
			var viewModel = GetSingleEntityViewModel<BPActivity>( organizationId, CERSID, SubmittalElementType.FacilityInformation, ResourceType.BusinessActivities, fseID );
			var facility = Repository.Facilities.GetByID( CERSID );

			viewModel.AddressInformation = new AddressInformation();
			viewModel.Facility = facility;

			CERSFacilityGeoPoint geoPoint = facility.CERSFacilityGeoPoint;
			//if geopoint is null create a generic one using the county centroid
			if ( geoPoint == null )
			{
				CountyGISCentroid countyCentroid = Repository.DataModel.CountyGISCentroids.SingleOrDefault( p => p.CountyID == facility.CountyID );

				if ( countyCentroid != null )
				{
					geoPoint = new CERSFacilityGeoPoint();
					geoPoint.LatitudeMeasure = countyCentroid.LatCentroid != null ? Convert.ToDecimal( countyCentroid.LatCentroid ) : 0;
					geoPoint.LongitudeMeasure = countyCentroid.LonCentroid != null ? Convert.ToDecimal( countyCentroid.LonCentroid ) : 0;
					geoPoint.HorizontalAccuracyMeasure = 100000;
					geoPoint.HorizontalCollectionMethodID = 104;
					geoPoint.HorizontalReferenceDatumID = 3;
					geoPoint.GeographicReferencePointID = 102;
					geoPoint.DataCollectionDate = DateTime.Now;
				}
			}

			//if geopoint is still null setup a generic geopoint
			if ( geoPoint == null )
			{
				geoPoint = new CERSFacilityGeoPoint();
				geoPoint.LatitudeMeasure = 0;
				geoPoint.LongitudeMeasure = 0;
				geoPoint.HorizontalAccuracyMeasure = 100000;
				geoPoint.HorizontalCollectionMethodID = 104;
				geoPoint.HorizontalReferenceDatumID = 3;
				geoPoint.GeographicReferencePointID = 102;
				geoPoint.DataCollectionDate = DateTime.Now;
			}

			viewModel.CERSFacilityGeoPoint = geoPoint;

			var entity = viewModel.Entity;
			if ( TryUpdateModel( entity, "Entity" ) )
			{
				if ( ModelState.IsValid )
				{
					//Call the Business Logic Layer that will handle the validation/flag setting
					//and all the other junk that needs to occur to persist a form.
					Services.BusinessLogic.SubmittalElements.FacilityInformation.SaveActivitiesForm( viewModel );

					string routeName = GetDraftEditRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessOwnerOperatorIdentification );
					return RedirectToRoute( routeName, new { organizationID = organizationId, CERSID = CERSID, FSEID = viewModel.FacilitySubmittalElement.ID } );
				}
			}
			return View( viewModel );
		}

		#region UpdateMapLocation_Async

		[HttpPost]
		public JsonResult UpdateLocationMap_Async( int? geoPointID, int cersID, decimal latitude, decimal longitude, decimal? accuracyMeasure, int referenceDatumID, int collectionMethodID, int referencePointID, DateTime collectionDate )
		{
			bool success = false;
			try
			{
				if ( geoPointID != null && geoPointID != 0 )
				{
					CERSFacilityGeoPoint geoPoint = Repository.CERSFacilityGeoPoints.GetByID( Convert.ToInt32( geoPointID ) );
					if ( geoPoint != null )
					{
						geoPoint.LatitudeMeasure = latitude;
						geoPoint.LongitudeMeasure = longitude;
						geoPoint.HorizontalAccuracyMeasure = accuracyMeasure != null ? accuracyMeasure.Value : 100;
						geoPoint.HorizontalReferenceDatumID = referenceDatumID;
						geoPoint.HorizontalCollectionMethodID = collectionMethodID;
						geoPoint.GeographicReferencePointID = referencePointID;
						//geoPoint.DataCollectionDate = DateTime.Now;
						geoPoint.DataCollectionDate = collectionDate;
						Repository.CERSFacilityGeoPoints.Update( geoPoint );
					}
				}
				else
				{
					CERSFacilityGeoPoint geoPoint = new CERSFacilityGeoPoint();
					geoPoint.CERSID = cersID;
					geoPoint.LatitudeMeasure = latitude;
					geoPoint.LongitudeMeasure = longitude;
					geoPoint.HorizontalAccuracyMeasure = accuracyMeasure != null ? accuracyMeasure.Value : 100;
					geoPoint.HorizontalReferenceDatumID = referenceDatumID;
					geoPoint.HorizontalCollectionMethodID = collectionMethodID;
					geoPoint.GeographicReferencePointID = referencePointID;
					//geoPoint.DataCollectionDate = DateTime.Now;
					geoPoint.DataCollectionDate = collectionDate;
					Repository.CERSFacilityGeoPoints.Create( geoPoint );
				}
				success = true;
			}
			catch ( Exception ex )
			{
				string message = ex.Message;
			}

			var result = new
			{
				Success = success
			};
			return Json( result );
		}

		#endregion UpdateMapLocation_Async

		#endregion BusinessActivities

		#region OwnerOperator

		protected virtual ActionResult Handle_BizOwnerOperatorGet( int organizationId, int CERSID, int? fseID = null )
		{
			ViewData.Add( "States", Repository.States.GetStateDistinctCodeList().AsEnumerable() );
			ViewData.Add( "DomesticCountries", Repository.Countries.GetDomesticCountryList().AsEnumerable() );
			var viewModel = GetSingleEntityViewModel<BPOwnerOperator>( organizationId, CERSID, SubmittalElementType.FacilityInformation, ResourceType.BusinessOwnerOperatorIdentification, fseID );
			viewModel.FacilitySubmittalElementResource.BPActivities.Add( GetSingleEntity<BPActivity>( viewModel.FacilitySubmittalElement, ResourceType.BusinessActivities ) );
			FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource );

			Services.BusinessLogic.SubmittalElements.FacilityInformation.InitializeDraftOwnerOperatorForm( viewModel );

			return View( viewModel );
		}

		protected virtual ActionResult Handle_BizOwnerOperatorPost( int organizationId, int CERSID, int? fseID = null )
		{
			var viewModel = GetSingleEntityViewModel<BPOwnerOperator>( organizationId, CERSID, SubmittalElementType.FacilityInformation, ResourceType.BusinessOwnerOperatorIdentification, fseID );
			var entity = viewModel.Entity;
			if ( TryUpdateModel( entity, "Entity" ) )
			{
				if ( ModelState.IsValid )
				{
					Services.BusinessLogic.SubmittalElements.FacilityInformation.SaveOwnerOperatorForm( viewModel );

					string anchor = viewModel.FacilitySubmittalElement.SubmittalElement.Acronym;
					return RedirectToFacilityDraftSubmittals( viewModel.OrganizationID, viewModel.CERSID, "#" + anchor );
				}
			}
			//if loop back to the page we need to repopulate the ViewData so it's not null
			ViewData.Add( "States", Repository.States.GetStateDistinctCodeList().AsEnumerable() );
			ViewData.Add( "DomesticCountries", Repository.Countries.GetDomesticCountryList().AsEnumerable() );

			return View( viewModel );
		}

		#endregion OwnerOperator

		#endregion Handler Methods

		#region Address Helpers

		[HttpPost]
		public ActionResult GetAllCountries()
		{
			var data = Repository.Countries.GetCountryNameList();
			return Json( data );
		}

		[HttpPost]
		public ActionResult GetDomesticCountries()
		{
			var data = Repository.Countries.GetDomesticCountryList();
			return Json( data );
		}

		[HttpPost]
		public ActionResult GetInternationalCountries()
		{
			var data = Repository.Countries.GetInternationalCountryList();
			return Json( data );
		}

		[HttpPost]
		public ActionResult GetStates()
		{
			var data = Repository.States.GetStateNameList();
			return Json( data );
		}

		private BPOwnerOperator FormatAddressHelper( BPOwnerOperator entity )
		{
			BPOwnerOperator result = entity;

			if ( result != null )
			{
				if ( result.BillingAddressCountry != "United States" || entity.BillingAddressCountry != "Canada" )
				{
					entity.BillingAddressState = null;
				}
				if ( string.IsNullOrEmpty( entity.BillingAddressCountry ) )
				{
					entity.BillingAddressCountry = "United States";
				}
				if ( result.EContactCountry != "United States" || entity.EContactCountry != "Canada" )
				{
					entity.EContactState = null;
				}
				if ( string.IsNullOrEmpty( entity.EContactCountry ) )
				{
					entity.EContactCountry = "United States";
				}
				if ( result.OwnerCountry != "United States" || entity.OwnerCountry != "Canada" )
				{
					entity.OwnerState = null;
				}
				if ( string.IsNullOrEmpty( entity.OwnerCountry ) )
				{
					entity.OwnerCountry = "United States";
				}
				if ( result.PropertyOwnerCountry != "United States" || entity.PropertyOwnerCountry != "Canada" )
				{
					entity.PropertyOwnerCountry = null;
				}
				if ( string.IsNullOrEmpty( entity.PropertyOwnerCountry ) )
				{
					entity.PropertyOwnerCountry = "United States";
				}
			}

			return result;
		}

		#endregion Address Helpers

		#region AjaxHelper

		[HttpPost]
		public JsonResult GetBizActivitySubmittalElementStatus_Async( int CERSID )
		{
			var currentSubmittalElements = Repository.DataModel.GetCurrentSubmittalElements( CERSID, null ).ToList();

			var hazMatInvStatusID = currentSubmittalElements.Where( s => s.SubmittalElementID == (int)SubmittalElementType.HazardousMaterialsInventory ).Select( f => f.SEStatusID ).Single();
			var ustInvStatusID = currentSubmittalElements.Where( s => s.SubmittalElementID == (int)SubmittalElementType.UndergroundStorageTanks ).Select( f => f.SEStatusID ).Single();
			var RMRStatusID = currentSubmittalElements.Where( s => s.SubmittalElementID == (int)SubmittalElementType.RecyclableMaterialsReport ).Select( f => f.SEStatusID ).Single();
			var tieredPermittingStatusID = currentSubmittalElements.Where( s => s.SubmittalElementID == (int)SubmittalElementType.OnsiteHazardousWasteTreatmentNotification ).Select( f => f.SEStatusID ).Single();
			var remoteWasteStatusID = currentSubmittalElements.Where( s => s.SubmittalElementID == (int)SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification ).Select( f => f.SEStatusID ).Single();
			var tankClosureStatusID = currentSubmittalElements.Where( s => s.SubmittalElementID == (int)SubmittalElementType.HazardousWasteTankClosureCertification ).Select( f => f.SEStatusID ).Single();
			var aboveGroundStatusID = currentSubmittalElements.Where( s => s.SubmittalElementID == (int)SubmittalElementType.AbovegroundPetroleumStorageTanks ).Select( f => f.SEStatusID ).Single();
			var emrpStatusID = currentSubmittalElements.Where( s => s.SubmittalElementID == (int)SubmittalElementType.EmergencyResponseandTrainingPlans ).Select( f => f.SEStatusID ).Single();

			var jsonData = new
			{
				isHMInvDraft = ( hazMatInvStatusID == 1 ),
				isUSTDraft = ( ustInvStatusID == 1 ),
				isRMRDraft = ( RMRStatusID == 1 ),
				istieredPermittingDraft = ( tieredPermittingStatusID == 1 ),
				isRemoteWasteDraft = ( remoteWasteStatusID == 1 ),
				isTankClosureDraft = ( tankClosureStatusID == 1 ),
				isASTDraft = ( aboveGroundStatusID == 1 ),
				isEMRPDraft = ( emrpStatusID == 1 )
			};
			return Json( jsonData );
		}

		#endregion AjaxHelper
	}
}