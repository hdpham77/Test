using CERS.AddressServices;
using CERS.Compositions;
using CERS.Guidance;
using CERS.Model;
using CERS.ViewModels.Chemicals;
using CERS.ViewModels.Enforcements;
using CERS.ViewModels.Facilities;
using CERS.ViewModels.GuidanceMessages;
using CERS.ViewModels.Inspections;
using CERS.ViewModels.Organizations;
using CERS.ViewModels.SubmittalElements;
using CERS.ViewModels.Violations;
using CERS.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using UPF;
using UPF.Web.Mvc;
using UPF.Web.Mvc.UI;
using Winnovative.ExcelLib;

namespace CERS.Web.UI.Organization.Controllers
{
	[Authorize]
	public class FacilityController : AppControllerBase
    {
        #region AddFacility

        [HttpPost]
		public ActionResult AddFacility( FacilityViewModel viewModel, FormCollection collection, int? organizationId = null )
		{
			if ( string.IsNullOrWhiteSpace( viewModel.BusinessName ) )
			{
				ModelState.AddModelError( "FacilityName", "Facility name not provided" );
			}
			if ( ModelState.IsValid )
			{
				//If the user is associated with at least 1 organization. Create facility for that Org.
				if ( !string.IsNullOrWhiteSpace( viewModel.SelectedOrganizationID ) && viewModel.SelectedOrganizationID != "0" )
				{
					//Double check: If the user has eligibility to add facility to this organization.
					if ( CheckUserOrgAccessPermissions( viewModel.SelectedOrganizationID.ToInt32() ) == false )
					{
						ModelState.AddModelError( "Unauthorized", "You do not have sufficient permissions to add facility to this organization" );
					}
					try
					{
						viewModel = Trimmer( viewModel ); //Trim the values, just in case.

						viewModel.Entity = Services.Facilities.Create( organizationID: Convert.ToInt32( viewModel.SelectedOrganizationID ), name: viewModel.BusinessName, street: viewModel.Street, city: viewModel.City, zipCode: viewModel.ZipCode, countyID: viewModel.CountyID );

						//Create the CERSFacilityGeoPoint record
						try
						{
							AddressInformation standardizedAddress = Services.Geo.GetAddressInformation( viewModel.Street, viewModel.City, viewModel.ZipCode, "CA" );

							if ( standardizedAddress.Longitude != 0 && standardizedAddress.Latitude != 0 )
							{
								CERSFacilityGeoPoint geoPoint = new CERSFacilityGeoPoint();
								geoPoint.CERSID = viewModel.Entity.CERSID;
								geoPoint.GeographicReferencePointID = standardizedAddress.GeographicReferencePointID;
								geoPoint.HorizontalAccuracyMeasure = standardizedAddress.HorizontalAccuracyMeasure;
								geoPoint.HorizontalCollectionMethodID = standardizedAddress.HorizontalCollectionMethodID;
								geoPoint.HorizontalReferenceDatumID = standardizedAddress.HorizontalReferenceDatumID;
								geoPoint.LatitudeMeasure = standardizedAddress.Latitude;
								geoPoint.LongitudeMeasure = standardizedAddress.Longitude;
								geoPoint.DataCollectionDate = DateTime.Now;
								Repository.CERSFacilityGeoPoints.Create( geoPoint );
							}
						}
						catch ( Exception ex )
						{
							string message = ex.Message;
						}

						//Set the permissions for the user as Admin and refresh the permissions in the session
						SetUserPermissions( viewModel.Entity.OrganizationID );

						//Write notification - facility created for existing Org.
						Contact contact = Repository.Contacts.GetByAccount( CurrentAccountID );
						Services.Events.CreateFacilityAdded( viewModel.Entity.Organization, viewModel.Entity, contact, CurrentAccount );

						return View( "FacilityAdded", viewModel );
					}
					catch
					{
						// Changed "catch (Exception ex)" to "catch" to suppress compilation warning; we do not do
						// anything meaningful with "ex" in this scenario. If we need to do something meaningful with
						// the Exception in the future, we can change it back.
						ModelState.AddModelError( "CreateFacilityFailed", "Error creating facility!" );
					}
				}
				else
				{
					//Create Org first so that facility can be added to it.
					viewModel.CreateOrgReason = (int)CreateOrganizationReason.AddFacility;
					return View( "AddOrganization", viewModel );
				}
			}

			viewModel.AssociatedOrganizations = Repository.Contacts.GetOrganizations( CurrentAccountID );
			return View( "FacilitySearchNoMatch", viewModel );
		}

		public ActionResult AddFacilitySearchResult( string street, string city, string zipCode, string washedStreet, string washedCity, string washedZipCode, ConfirmAddressType confirmAddressType, bool isAddressWashed, int? organizationId = null, string washedStreetWithoutSuite = "", string washedSuite = "" )
		{
			var viewModel = SystemViewModelData.BuildUpFacilityViewModel();
			viewModel.AssociatedOrganizations = Repository.Contacts.GetOrganizations( CurrentAccountID );
			viewModel.IsAddressWashed = isAddressWashed;
			viewModel.CurrentUserOrganizationID = CurrentOrganizationID;
			viewModel.ConfirmAddressType = confirmAddressType.ToString();
			viewModel.Street = street;
			viewModel.City = city;
			viewModel.ZipCode = zipCode;
			viewModel.WashedStreet = washedStreet;
			viewModel.WashedCity = washedCity;
			viewModel.WashedZipCode = washedZipCode;
            viewModel.WashedStreetWithoutSuite = String.IsNullOrWhiteSpace( washedStreetWithoutSuite ) ? street : washedStreetWithoutSuite;
			viewModel.WashedSuite = washedSuite;
			ZipCode zipcode = null;

			//search against CERS2 facility table's washed fields.
			if ( confirmAddressType == ConfirmAddressType.Washed )
			{
				AddressInformation standardizedAddress = Services.Geo.GetAddressInformation( washedStreet, washedCity, washedZipCode, "CA" );
				Address address = new Address();
				address.Street = street;
				address.City = city;
				address.ZipCode = zipCode;
				address.State = "CA";

				//viewModel.Entities = Repository.Facilities.GetByExactAddress(street: washedStreet, city: washedCity, zipCode: washedZipCode);
				viewModel.Entities = Repository.Facilities.FindPotentialDuplicatesAsFacilityCollection( address, standardizedAddress );
				zipcode = Repository.ZipCodes.Search( washedZipCode ).FirstOrDefault();
			}
			else
			{
				AddressInformation standardizedAddress = Services.Geo.GetAddressInformation( street, city, zipCode, "CA" );
				Address address = new Address();
				address.Street = street;
				address.City = city;
				address.ZipCode = zipCode;
				address.State = "CA";

				//viewModel.Entities = Repository.Facilities.GetByExactAddress(street: street, city: city, zipCode: zipCode);
				viewModel.Entities = Repository.Facilities.FindPotentialDuplicatesAsFacilityCollection( address, standardizedAddress );
				zipcode = Repository.ZipCodes.Search( washedZipCode ).FirstOrDefault();
			}

			if ( zipcode != null )
			{
				viewModel.CountyID = zipcode.PrimaryCountyID;
			}

			// MULTIPLE MATCHES
			if ( viewModel.Entities.Count() > 1 )
			{
				viewModel.ExistingFacilities = viewModel.Entities.Select( p => p.CERSID ).ToList();
				viewModel.View = "ExistingAddresses";
				viewModel.WashedAddresses = viewModel.Entities.GroupBy( x => new { WashedStreet = x.WashedStreet.Trim().ToUpperInvariant(), WashedCity = x.WashedCity.Trim().ToUpperInvariant(), WashedZipCode = x.WashedZipCode.Trim(), CERSID = x.CERSID, FacilityName = x.Name, OrganizationHeadquarters = x.Organization.Headquarters, LastSubmittalSubmittedOnDate = x.UpdatedOn, WashedSuite = x.WashedSuite } ).Select( result => new WashedAddress { WashedStreet = result.Key.WashedStreet, WashedCity = result.Key.WashedCity, WashedZipCode = result.Key.WashedZipCode, CERSID = result.Key.CERSID, FacilityName = result.Key.FacilityName, OrganizationHeadquarters = result.Key.OrganizationHeadquarters, LastSubmittalSubmittedOnDate = result.Key.LastSubmittalSubmittedOnDate, WashedSuite = result.Key.WashedSuite } );
				return View( "FacilitySearchMultipleMatches", viewModel );
			}

			// UNIQUE MATCH
			else if ( viewModel.Entities.Count() == 1 )
			{
				viewModel.Entity = viewModel.Entities.SingleOrDefault();
				var frseLastSubmitted = Repository.FacilityRegulatorSubmittalElements.GetLastSubmittalOn( viewModel.Entity.CERSID );
				if ( frseLastSubmitted != null )
				{
					viewModel.LastSubmittedDate = frseLastSubmitted.LastSubmittedFacilitySubmittalElementOn.Value.ToShortDateString();
				}

				return View( "FacilitySearchSingleMatch", viewModel );
			}

			//if NO MATCHES
			else
			{
				return View( "FacilitySearchNoMatch", viewModel );
			}
		}

		[HttpPost]
		public ActionResult AddFacilityToExistingOrganization( FacilityViewModel fvm, FormCollection collection, int? organizationId = null )
		{
			if ( string.IsNullOrWhiteSpace( fvm.BusinessName ) )
			{
				ModelState.AddModelError( "BusinessName", "Facility name is required" );
			}
			if ( ModelState.IsValid )
			{
				//If user has no Org or chose to add new org, then go to "Add Org" and then request "Add Facility"
				if ( string.IsNullOrWhiteSpace( fvm.SelectedOrganizationID ) || fvm.SelectedOrganizationID.ToInt32() == 0 )
				{
					fvm.CreateOrgReason = (int)CreateOrganizationReason.AddFacility;
					return View( "AddOrganization", fvm );
				}

				//If user has Org, then add facility to the Org (selected from drop down) and return to "Facility Added".
				else
				{
					//Check user has sufficient privileges to do this.
					if ( CheckUserOrgAccessPermissions( fvm.SelectedOrganizationID.ToInt32() ) == false )
					{
						ModelState.AddModelError( "Unauthorized", "You do not have sufficient permissions to add facility to this Organization" );
						return View( fvm );
					}
					try
					{
						int newOrgID = Convert.ToInt32( fvm.SelectedOrganizationID );
						Facility newFacility = Services.Facilities.Create( organizationID: newOrgID, name: fvm.BusinessName, street: fvm.Street, city: fvm.City, zipCode: fvm.ZipCode, countyID: fvm.CountyID );
						fvm.Entity = newFacility;

						try
						{
							AddressInformation standardizedAddress = Services.Geo.GetAddressInformation( fvm.Street, fvm.City, fvm.ZipCode, "CA" );

							if ( standardizedAddress.Longitude != 0 && standardizedAddress.Latitude != 0 )
							{
								CERSFacilityGeoPoint geoPoint = new CERSFacilityGeoPoint();
								geoPoint.CERSID = fvm.Entity.CERSID;
								geoPoint.GeographicReferencePointID = standardizedAddress.GeographicReferencePointID;
								geoPoint.HorizontalAccuracyMeasure = standardizedAddress.HorizontalAccuracyMeasure;
								geoPoint.HorizontalCollectionMethodID = standardizedAddress.HorizontalCollectionMethodID;
								geoPoint.HorizontalReferenceDatumID = standardizedAddress.HorizontalReferenceDatumID;
								geoPoint.LatitudeMeasure = standardizedAddress.Latitude;
								geoPoint.LongitudeMeasure = standardizedAddress.Longitude;
								geoPoint.DataCollectionDate = DateTime.Now;
								Repository.CERSFacilityGeoPoints.Create( geoPoint );
							}
						}
						catch ( Exception ex )
						{
							string message = ex.Message;
						}

						//Set the permissions for the user as Admin and refresh the permissions in the session
						SetUserPermissions( newOrgID );

						//Write notification - facility added to existing Org.
						Contact contact = Repository.Contacts.GetByAccount( CurrentAccountID );
						Services.Events.CreateFacilityAdded( fvm.Entity.Organization, newFacility, contact, CurrentAccount, fvm.NewFacilityNotes );

						return View( "FacilityAdded", fvm );
					}
					catch
					{
						// Changed "catch (Exception ex)" to "catch" to suppress compilation warning; we do not do
						// anything meaningful with "ex" in this scenario. If we need to do something meaningful with
						// the Exception in the future, we can change it back.
						ModelState.AddModelError( "FacilityCreationFail", "Unable to create facility" );
					}
				}
			}
			return View( fvm );
		}

        #endregion AddFacility

        #region AddOrganization

        [HttpPost]
		public ActionResult AddOrganization( FacilityViewModel fvm, bool checkDuplicates = true, int? organizationId = null )
		{
			if ( string.IsNullOrWhiteSpace( fvm.OrganizationName ) )
			{
				ModelState.AddModelError( "OrganizationName", "Organization name is required." );
			}
			if ( string.IsNullOrWhiteSpace( fvm.OrganizationHeadquarters ) )
			{
				ModelState.AddModelError( "OrganizationHeadquarters", "Organization headquarters is required." );
			}

			if ( ModelState.IsValid )
			{
				fvm.OrgContactPhone.FormatPhoneNumber();

                #region if ( checkDuplicates == true )

                if ( checkDuplicates == true )
				{
					//Check for duplicate organization names.
					//(future): For now, we will just check for the first 20 characters of the Org name. We can change this check in future.
					var duplicateOrgs = Repository.Organizations.CheckDuplicates( fvm.OrganizationName, fvm.OrganizationHeadquarters );
					if ( duplicateOrgs.Count() > 0 )
					{
						var gvDuplicateOrgs = duplicateOrgs.ToGridView().ToList();
						foreach ( var o in gvDuplicateOrgs )
						{
							o.CanCurrentUserAccess = CheckUserOrgAccessPermissions( o.ID );
						}
						fvm.DuplicateOrganizationsGridView = gvDuplicateOrgs;

						//Go to Duplicate Organization Check
						return View( "DuplicateOrganizationCheck", fvm );
					}
                }

                #endregion if ( checkDuplicates == true )

                #region if ( fvm.CreateOrgReason == (int)CreateOrganizationReason.AddFacility )

                if ( fvm.CreateOrgReason == (int)CreateOrganizationReason.AddFacility )
				{
					fvm = Trimmer( fvm );

					//Create new Organization
					Facility facility = Services.Facilities.Create( organizationName: fvm.OrganizationName, organizationHQ: fvm.OrganizationHeadquarters, name: fvm.BusinessName, street: fvm.Street, city: fvm.City, zipCode: fvm.ZipCode, countyID: fvm.CountyID );
					fvm.Entity = facility;
					try
					{
						//Create Organization-Contact for this Org and user.
						Repository.OrganizationContacts.Create( CurrentAccount, facility.OrganizationID, fvm.OrgContactTitle, fvm.OrgContactPhone );
						//Set the permissions for the user as Admin and refresh the permissions in the session
						SetUserPermissions( facility.OrganizationID );

						Repository.Organizations.Save( facility.Organization );
						//Write notification- New org, facility created.
						Contact contact = Repository.Contacts.GetByAccount( CurrentAccountID );
						Services.Events.CreateNewFacilityAndNewOrganization( facility.Organization, facility, contact, CurrentAccount, fvm.NewFacilityNotes );
					}
					catch ( Exception ex )
					{
						ModelState.AddModelError( "FacilityCreationFailed", "Unable to create facility" );
						throw new Exception( "Unable to create Facility", ex );
					}

					try
					{
						AddressInformation standardizedAddress = Services.Geo.GetAddressInformation( fvm.Street, fvm.City, fvm.ZipCode, "CA" );

						if ( standardizedAddress.Longitude != 0 && standardizedAddress.Latitude != 0 )
						{
							CERSFacilityGeoPoint geoPoint = new CERSFacilityGeoPoint();
							geoPoint.CERSID = fvm.Entity.CERSID;
							geoPoint.GeographicReferencePointID = standardizedAddress.GeographicReferencePointID;
							geoPoint.HorizontalAccuracyMeasure = standardizedAddress.HorizontalAccuracyMeasure;
							geoPoint.HorizontalCollectionMethodID = standardizedAddress.HorizontalCollectionMethodID;
							geoPoint.HorizontalReferenceDatumID = standardizedAddress.HorizontalReferenceDatumID;
							geoPoint.LatitudeMeasure = standardizedAddress.Latitude;
							geoPoint.LongitudeMeasure = standardizedAddress.Longitude;
							geoPoint.DataCollectionDate = DateTime.Now;
							Repository.CERSFacilityGeoPoints.Create( geoPoint );
						}
					}
					catch ( Exception ex )
					{
						string message = ex.Message;
					}

					return View( "FacilityAdded", fvm );
                }

                #endregion if ( fvm.CreateOrgReason == (int)CreateOrganizationReason.AddFacility )

                #region if ( fvm.CreateOrgReason == (int)CreateOrganizationReason.TransferFacility )

                if ( fvm.CreateOrgReason == (int)CreateOrganizationReason.TransferFacility )
				{
					//Regulator authorized transfer facility request
					//TODO:
					CERS.Model.Organization targetOrganization = null;
					try
					{
						//Add the New Organization first and then try requesting the Transfer of Facility.
						targetOrganization = Repository.Organizations.Create( fvm.OrganizationName, fvm.OrganizationHeadquarters );

						//Set the user to be the Org's Admin
						Services.Security.AddContactToGroup( CurrentAccount, targetOrganization.ID, Context.Organization, fvm.OrgContactTitle, fvm.OrgContactPhone, BuiltInPermissionGroup.OrgAdmins );

						if ( targetOrganization != null )
						{
							var sourceOrganization = Repository.Organizations.GetByID( fvm.Entity.OrganizationID );
							Contact contact = Repository.OrganizationContacts.GetByAccount( CurrentAccountID );
							fvm.Entity = Repository.Facilities.GetByID( fvm.Entity.CERSID );

							Event evt = Services.Events.CreateRegulatorAuthorizedFacilityTransferRequest( sourceOrganization, targetOrganization, fvm.Entity, contact, CurrentAccount, fvm.NewFacilityNotes );
							fvm.EventTicketCode = evt.TicketCode;
							fvm.EventTypeCode = evt.GetEventTypeCode();
							fvm.Regulator = evt.Regulator;
							if ( evt.Notifications != null && evt.Notifications.Count > 0 )
							{
								List<Contact> notificationContacts = new List<Contact>();
								evt.Notifications.ToList().ForEach( n => notificationContacts.Add( n.Contact ) );
								fvm.NotificationContacts = notificationContacts;
							}

							return View( "RegulatorAuthorizedFacilityTransferRequest", fvm );
						}
					}
					catch ( Exception ex )
					{
						ModelState.AddModelError( "FacilityTransferFailed", "Facility Transfer failed" );
						throw new Exception( "Unable Transfer Facility", ex );
					}
                }

                #endregion if ( fvm.CreateOrgReason == (int)CreateOrganizationReason.TransferFacility )

            }

			return View( fvm );
		}

        #endregion AddOrganization

        #region CheckUserOrgAccessPermissions

        /// <summary>
		/// If the user has Ord Editor or higher permissions returns true else returns false for the
		/// provided Org ID.
		/// </summary>
		/// <param name="orgId"></param>
		/// <returns>true/false</returns>
		public bool CheckUserOrgAccessPermissions( int orgId )
		{
			//if(!CurrentUserRoles.IsSystemAdmin)
			return CurrentUserRoles.HasRoles( Convert.ToInt32( orgId ), Context.Organization, PermissionRole.OrgEditor );
		}

        #endregion CheckUserOrgAccessPermissions

        #region CityZipValidation_Async

        [HttpPost]
		public JsonResult CityZipValidation_Async( string city, string zipCode )
		{
			bool success = false;
			string message = "";
			if ( !( city == null && zipCode == null ) )
			{
				CityZipValidationResult cityZipValidationResult = Services.Geo.VerifyCityByZip( city, "CA", zipCode );
				message = cityZipValidationResult.Message;
				if ( cityZipValidationResult.Message == String.Empty )
				{
					success = true;
				}
			}
			return Json( new { Success = success, Message = message } );
		}

        #endregion CityZipValidation_Async

        #region Compliance

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Compliance( int organizationId, int CERSID )
		{
			FacilityViewModel viewModel = SystemViewModelData.BuildUpFacilityViewModel( CERSID );

			return View( viewModel );
		}

        public JsonResult Facility_InspectionGrid( [DataSourceRequest]DataSourceRequest request, int CERSID )
        {
            var inspections = ( from inspection in Repository.Inspections.Search( CERSID:CERSID )
                                select new InspectionGridViewModel
                                 {
                                     ID = inspection.ID,
                                     OccurredOn = inspection.OccurredOn,
                                     CMEProgramElement = inspection.CMEProgramElement.Acronym,
                                     Type = inspection.Type,
                                     ViolationCount = inspection.ClassIViolationCount + inspection.ClassIIViolationCount + inspection.MinorViolationCount,
                                     ViolationsRTCOn = inspection.ViolationsRTCOn,
                                     CMEDataStatus = inspection.Status.Name,
                                 } ).ToList();

            inspections.TranslateDataRegistryProperties();

            return Json( inspections.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public JsonResult Facility_EnforcementGrid( [DataSourceRequest]DataSourceRequest request, int CERSID )
        {
            var enforcements = ( from enforcement in Repository.Enforcements.Search( CERSID:CERSID )
                                select new EnforcementGridViewModel
                                {
                                    ID = enforcement.ID,
                                    OccurredOn = enforcement.OccurredOn,
                                    Type = enforcement.Type,
                                    FormalType = enforcement.Status.Name,
                                    ViolationCount = enforcement.EnforcementViolations.Where( ev => !ev.Voided ).Count(),
                                } ).ToList();

            enforcements.TranslateDataRegistryProperties();

            return Json( enforcements.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public JsonResult Facility_ViolationGrid( [DataSourceRequest]DataSourceRequest request, int CERSID )
        {
            var violations = ( from violation in Repository.Violations.Search( CERSID:CERSID )
                               select new ViolationGridViewModel
                                {
                                    ID = violation.ID,
                                    Program = violation.ViolationType.ViolationCategory.ViolationProgramElement.ProgramElement.Acronym,
                                    ViolationTypeNumber = violation.ViolationType.ViolationTypeNumber,
                                    OccurredOn = violation.OccurredOn,
                                    ActualRTCOn = violation.ActualRTCOn,
                                    Class = violation.Class,
                                } ).ToList();

            violations.TranslateDataRegistryProperties();

            return Json( violations.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        #endregion Compliance

        #region ConfirmAddress

        [HttpPost]
		public ActionResult ConfirmAddressStandardized( FacilityViewModel fvm, FormCollection collection )
		{
			if ( ModelState.IsValid )
			{
				fvm.ConfirmAddressType = ConfirmAddressType.Washed.ToString();

				//Set the regular fields to washed field values since user has chosen to go with Standardized values.
				fvm = StandardizeAddress( fvm, fvm.WashedStreet, fvm.WashedCity, fvm.WashedZipCode, fvm.WashedStreetWithoutSuite );

                return RedirectToActionPermanent( "AddFacilitySearchResult", new { street = fvm.Street, city = fvm.City, zipCode = fvm.ZipCode, washedStreet = fvm.WashedStreet, washedCity = fvm.WashedCity, washedZipCode = fvm.WashedZipCode, confirmAddressType = fvm.ConfirmAddressType, isAddressWashed = fvm.IsAddressWashed, organizationId = CurrentOrganizationID, washedStreetWithoutSuite = fvm.WashedStreetWithoutSuite, washedSuite = fvm.WashedSuite } );
			}

			return View( fvm );
		}

		[HttpPost]
		public ActionResult ConfirmAddressUserEntered( FacilityViewModel fvm, FormCollection collection )
		{
			if ( ModelState.IsValid )
			{
				fvm.ConfirmAddressType = ConfirmAddressType.UserEntered.ToString();

                return RedirectToActionPermanent( "AddFacilitySearchResult", new { street = fvm.Street, city = fvm.City, zipCode = fvm.ZipCode, washedStreet = fvm.WashedStreet, washedCity = fvm.WashedCity, washedZipCode = fvm.WashedZipCode, confirmAddressType = fvm.ConfirmAddressType, isAddressWashed = fvm.IsAddressWashed, organizationId = CurrentOrganizationID, washedStreetWithoutSuite = fvm.WashedStreetWithoutSuite, washedSuite = fvm.WashedSuite } );
			}

			return View( fvm );
		}

        #endregion ConfirmAddress

        #region CopyResource

        [HttpPost]
		public ActionResult CopyResource( int fseid, int fserid, int currentSubmittalStatusID )
		{
			FacilitySubmittalElement fse = null;

			var copySubmittelElementResource = Repository.FacilitySubmittalElementResources.GetByID( fserid );

			if ( fseid == 0 )
			{
				//Create new submittal Element
				fse = new FacilitySubmittalElement()
				{
					CERSID = copySubmittelElementResource.FacilitySubmittalElement.CERSID,
					SubmittalElementID = copySubmittelElementResource.FacilitySubmittalElement.SubmittalElementID,
					FacilitySubmittalID = copySubmittelElementResource.FacilitySubmittalElement.FacilitySubmittalID,
					StatusID = (int)CERS.SubmittalElementStatus.Draft,
					TemplateID = copySubmittelElementResource.FacilitySubmittalElement.TemplateID,
					OwningRegulatorID = copySubmittelElementResource.FacilitySubmittalElement.OwningRegulatorID,
					CERS1FacilityDataID = copySubmittelElementResource.FacilitySubmittalElement.CERS1FacilityDataID
				};

				Repository.FacilitySubmittalElements.Save( fse );
			}
			else
			{
				fse = Repository.FacilitySubmittalElements.GetByID( fseid );
			}

			//Clone FSER
			Repository.FacilitySubmittalElementResources.CloneWithEntities( copySubmittelElementResource, fse );

			//Make a copy of the entities

			//Update RegulatorSubmittalElement
			FacilityRegulatorSubmittalElement frse = Repository.FacilityRegulatorSubmittalElements.Search( CERSID: copySubmittelElementResource.FacilitySubmittalElement.CERSID, submittalElementID: copySubmittelElementResource.FacilitySubmittalElement.SubmittalElementID ).SingleOrDefault();
			frse.DraftFacilitySubmittalElementID = fse.ID;
			Repository.FacilityRegulatorSubmittalElements.Save( frse );

			return Json( new { success = true, message = "Success" } );
		}

        #endregion CopyResource

        #region DiscardFacilitySubmittalElement

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult DiscardFacilitySubmittalElement( int organizationId, int FSEID )
		{
			var facilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			Repository.FacilitySubmittalElements.DiscardFacilitySubmittalElement( facilitySubmittalElement );

			return RedirectToRoute( GetRouteName( OrganizationFacility.Home ), new { organizationId = organizationId, CERSID = facilitySubmittalElement.CERSID } );
		}

        #endregion DiscardFacilitySubmittalElement

        #region FacilityOwnershipChange

        [HttpPost]
		public ActionResult FacilityOwnershipChange( FacilityViewModel fvm, FormCollection collection, int? organizationId = null )
		{
			if ( string.IsNullOrWhiteSpace( fvm.BusinessName ) )
			{
				ModelState.AddModelError( "BusinessName", "Facility name is required" );
			}

			if ( ModelState.IsValid )
			{
				//If user has no Org, go to "Add Org" and then request "Transfer Facility"
				if ( string.IsNullOrWhiteSpace( fvm.SelectedOrganizationID ) )
				{
					fvm.CreateOrgReason = (int)CreateOrganizationReason.TransferFacility;
					return View( "AddOrganization", fvm );
				}

				//If user has Org,
				else
				{
					//Make sure ONLY Org Editors are doing this deed.
					if ( CheckUserOrgAccessPermissions( fvm.SelectedOrganizationID.ToInt32() ) == false )
					{
						ModelState.AddModelError( "Unauthorized", "You do not have sufficient permissions to transfer this facility to the selected Organization." );
						return View( fvm );
					}

					//DEPRECATED: We exclude the facility's Org from the dropdownlist. But let this check be in place just in case.
					//If selected Org (from dropdownlist) == Facility's Org then go to "Existing facility confirm"
					if ( Convert.ToInt32( fvm.SelectedOrganizationID ) == fvm.Entity.OrganizationID )
					{
						return View( "ExistingFacilityConfirmation", fvm );
					}

					//If current user Org != Facility's Org then
					else
					{
						//If for Facility's Org, the current user is a lead user then go to "User Authorized facility XFER"
						bool isProspectiveOrgLeadUser = CurrentUserRoles.HasRoles( Convert.ToInt32( fvm.Entity.OrganizationID ), Context.Organization, PermissionRole.OrgEditor );

						//Get the selected Org's headquarters
						var selectedOrg = Repository.Organizations.GetByID( Convert.ToInt32( fvm.SelectedOrganizationID ) );
						if ( selectedOrg != null )
						{
							fvm.OrganizationName = selectedOrg.Name;
							fvm.OrganizationHeadquarters = selectedOrg.Headquarters;
						}

						//If current user is Admin on the target Org then it is User Authorized Facility Transfer.
						if ( isProspectiveOrgLeadUser == true )
						{
							fvm.CurrentUserEmail = CurrentAccount.Email;
							fvm.CurrentUserName = CurrentAccount.UserName;

							return View( "UserAuthorizedFacilityTransferRequest", fvm );
						}

						//Else go to "Regulator Authorized Facility XFER"
						else
						{
							//Regulator Authorized transfer request
							var sourceOrganization = Repository.Organizations.GetByID( fvm.Entity.OrganizationID );
							Contact contact = Repository.OrganizationContacts.GetByAccount( CurrentAccountID );
							fvm.Entity = Repository.Facilities.GetByID( fvm.Entity.ID );
							Event evt = Services.Events.CreateRegulatorAuthorizedFacilityTransferRequest( sourceOrganization, selectedOrg, fvm.Entity, contact, CurrentAccount, fvm.NewFacilityNotes );
							fvm.EventTicketCode = evt.TicketCode;
							fvm.EventTypeCode = evt.GetEventTypeCode();
							fvm.Regulator = evt.Regulator;
							if ( evt.Notifications != null && evt.Notifications.Count > 0 )
							{
								List<Contact> notificationContacts = new List<Contact>();
								evt.Notifications.ToList().ForEach( n => notificationContacts.Add( n.Contact ) );
								fvm.NotificationContacts = notificationContacts;
							}
							return View( "RegulatorAuthorizedFacilityTransferRequest", fvm );
						}
					}
				}
			}
			return View( fvm );
		}

        #endregion FacilityOwnershipChange

        #region FacilityInspectionStatus

        public ActionResult FacilityInspectionStatus( int organizationId )
        {
            FacilityInspectionStatusViewModel viewModel = new FacilityInspectionStatusViewModel();
            viewModel.Regulators = Repository.Regulators.GetSimpleLookup().Where( p => p.Tag == RegulatorType.CUPA || p.Tag == RegulatorType.PA );
            viewModel.Organization = Repository.Organizations.GetByID( organizationId );
            return View( viewModel );
        }

        #endregion FacilityInspectionStatus

        #region FacilityReportingStatus

        public ActionResult FacilityReportingStatus( int organizationId )
		{
			FacilityReportingStatusViewModel viewModel = new FacilityReportingStatusViewModel();
			viewModel.Regulators = Repository.Regulators.GetSimpleLookup().Where( p => p.Tag == RegulatorType.CUPA || p.Tag == RegulatorType.PA );
			viewModel.NotReportedOnSince = DateTime.Now.AddDays( -365 );
			viewModel.Organization = Repository.Organizations.GetByID( organizationId );
			viewModel.SubmittalElements = Repository.SubmittalElements.SimpleList().ToList();
			return View( viewModel );
		}

        #endregion FacilityReportingStatus

        #region FacilitySearch

        [HttpPost]
		public ActionResult FacilitySearchMultipleMatches( FacilityViewModel fvm, FormCollection collection, int? organizationId = null )
		{
			fvm.CurrentUserOrganizationID = organizationId;

			//When the "My Facility Not Shown" button is clicked.
			if ( collection["NoMatch"] != null )
			{
				fvm.AssociatedOrganizations = Repository.Contacts.GetOrganizations( CurrentAccountID );
				return View( "FacilitySearchNoMatch", fvm );
			}

			//When Multiple Match address is selected.
			if ( collection["MultipleMatch"] != null )
			{
				//search against CERS2 facility table's washed fields.
				if ( fvm.ConfirmAddressType.Equals( ConfirmAddressType.Washed.ToString() ) )
				{
					fvm.Entities = Repository.Facilities.Search( washedStreetWithoutSuite: fvm.WashedStreetWithoutSuite, washedCity: fvm.WashedCity, washedZipCode: fvm.WashedZipCode, washedSuite: fvm.WashedSuite );
				}
				else
				{
					fvm.Entities = Repository.Facilities.Search( street: fvm.Street, washedCity: fvm.City, washedZipCode: fvm.WashedZipCode, washedSuite: fvm.WashedSuite );
				}

				//If multiple addresses with same address then show the same Multiple matches view but set the flag "ExistingFacilities"
				//so that it knows to show the second grid and hide the first one.
				if ( fvm.Entities.Count() > 1 )
				{
					fvm.View = "ExistingFacilities";
				}
				else if ( fvm.Entities.Count() == 1 )
				{
					fvm.Entity = fvm.Entities.FirstOrDefault();
					return View( "FacilitySearchSingleMatch", fvm );
				}
			}

			if ( !string.IsNullOrWhiteSpace( fvm.ActionValue ) )
			{
				int cersID = fvm.ActionValue.StripNonNumerics().ToInt32();
				fvm.Entity = SystemViewModelData.BuildUpFacilityViewModel( cersID ).Entity;
				return View( "FacilitySearchSingleMatch", fvm );
			}
			return View( fvm );
		}

        public JsonResult FacilitySearchMultipleMatches_Grid( [DataSourceRequest]DataSourceRequest request, string confirmAddressType, string street, string city, string zipCode, string washedStreetWithoutSuite, string washedCity, string washedZipCode, string washedSuite )
        {
            var facilityViewModel = SystemViewModelData.BuildUpFacilityViewModel();

            //search against CERS2 facility table's washed fields.
            if ( confirmAddressType.Equals( ConfirmAddressType.Washed.ToString() ) )
            {
                facilityViewModel.Entities = Repository.Facilities.Search( washedStreetWithoutSuite:washedStreetWithoutSuite, washedCity:washedCity, washedZipCode:washedZipCode, washedSuite:washedSuite );
            }
            else
            {
                facilityViewModel.Entities = Repository.Facilities.Search( street:street, washedCity:city, washedZipCode:zipCode, washedSuite:washedSuite );
            }

            return Json( facilityViewModel.GridView.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

		[HttpPost]
		public ActionResult FacilitySearchSingleMatchActions( FacilityViewModel fvm, FormCollection collection, int? organizationId = null )
		{
			if ( !CurrentOrganizationID.HasValue )
			{
				CurrentOrganizationID = fvm.CurrentUserOrganizationID;
			}

			if ( fvm.Entity.OrganizationID != 0 )
			{
				fvm.Entity.Organization = Repository.Organizations.GetByID( fvm.Entity.OrganizationID );
			}

			//Based on which button is clicked, redirect to that action method.
			if ( collection["AddFacilityToExistingOrg"] != null )
			{
				fvm.AssociatedOrganizations = Repository.Contacts.GetOrganizations( CurrentAccountID );
				return View( "AddFacilityToExistingOrganization", fvm );
			}

			if ( collection["FacilityOwnershipChange"] != null )
			{
				fvm.AssociatedOrganizations = Repository.Contacts.GetOrganizations( CurrentAccountID ).Where( p => p.ID != fvm.Entity.OrganizationID );
				return View( "FacilityOwnershipChange", fvm );
			}

			if ( collection["OrganizationAccessRequest"] != null )
			{
				//Check if the current user belongs to this Organization
				var orgUser = Repository.OrganizationContacts.Search( fvm.Entity.OrganizationID, CurrentAccountID );
				if ( orgUser.Count() > 0 )
				{
					//Existing facility/Org
					return View( "ExistingFacilityConfirmation", fvm );
				}
				else
				{
					//gets information about who this access request will be sent to.
					return RedirectToActionPermanent( "OrganizationAccessRequest", new { prospectiveOrgID = fvm.Entity.OrganizationID, facilityName = fvm.Entity.Name, street = fvm.Entity.WashedStreet, city = fvm.Entity.WashedCity, zipcode = fvm.Entity.WashedZipCode, washedSuite = fvm.Entity.WashedSuite } );
				}
			}
			return RedirectToAction( "New" );
		}

        #endregion FacilitySearch

        #region FacilitySubmittalPreparation

        /// <summary>
		/// This is the action method for the Version 2 implementation of the Crazy Page. It replaces the
		/// "OLD" OrganizationFaclityHome action method.
		/// </summary>
		/// <param name="organizationId"></param>
		/// <param name="CERSID"></param>
		/// <returns></returns>
		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult FacilitySubmittalPreparation( int organizationId, int CERSID )
		{
			FacilitySubmittalPreparationMenuViewModel viewModel = new FacilitySubmittalPreparationMenuViewModel();
			if ( organizationId != 0 && CERSID != 0 )
			{
				var facility = Repository.Facilities.GetByID( CERSID );

				viewModel.FacilityName = facility.Name;
				viewModel.OrganizationID = organizationId;
				viewModel.CERSID = CERSID;

				var currentDraftSubmittals = Repository.DataModel.uspGetCurrentDraftSubmittals( CERSID ).ToList();
				var validSubmittalElementIDs = currentDraftSubmittals.Select( p => p.SubmittalElementID ).ToList();
				viewModel.CurrentDraftSubmittalCollection = currentDraftSubmittals;
				viewModel.Regulators = Repository.FacilityRegulatorSubmittalElements.Search( CERSID: CERSID ).Select( s => s.Regulator ).Distinct();

				viewModel.SubmittalElements = ( from submittalElement in Repository.DataModel.SubmittalElements
												where submittalElement.Voided == false && validSubmittalElementIDs.Contains( submittalElement.ID )
												select submittalElement ).ToList();

				var regulator = Repository.Facilities.GetCUPA( CERSID );
				if ( regulator != null )
				{
					var localRequirements = Repository.RegulatorLocals.Search( regulatorID: regulator.ID );
					viewModel.LocalRequirements = ( from lr in localRequirements
													select new RegulatorSubmittalElementLocalRequirementData
													{
														IsLocalDocRequired = lr.IsLocalDocumentRequired,
														RegulatorID = lr.RegulatorID,
														RegulatorName = lr.Regulator.Name,
														RequirementText = lr.RequirementsText,
														SubmittalElementID = lr.SubmittalElementID,
														SubmittalElementName = lr.SubmittalElement.Name
													} ).ToList();
				}
				else
				{
					viewModel.LocalRequirements = new List<RegulatorSubmittalElementLocalRequirementData>();
				}

				//plug in HMI report
				uspGetCurrentDraftSubmittals_Result HMIFse = viewModel.CurrentDraftSubmittalCollection.Where( p => p.SubmittalElementID == (int)( SubmittalElementType.HazardousMaterialsInventory ) ).FirstOrDefault();
				if ( HMIFse != null )
				{
					HazardousMaterialsInventoryController hmiController = new HazardousMaterialsInventoryController();
					viewModel.HMISReport = hmiController.PopulateHMIMatrixViewModel( organizationId, CERSID, HMIFse.FacilitySubmittalElementID, HMIFse.FacilitySubmittalElementResourceID );
				}
			}

			return View( viewModel );
		}

        #endregion FacilitySubmittalPreparation

        #region GetChemicalBPFacilityChemicalGrid

        [Obsolete( "Just delete me anytime after 9/1/2014 if no error happen" )]
        [HttpPost]
		public ActionResult GetChemicalBPFacilityChemicalGrid( int fserid )
		{
			var chemicals = GetBPChemicals( fserid );
			var partialView = RenderPartialViewToString( "_BPFacilityChemicalGrid", chemicals );
			return Json( new { success = true, message = partialView } );
		}

        #endregion GetChemicalBPFacilityChemicalGrid

        #region GetOrgFacilities

        /// <summary>
		/// Returns facilities of an organization. [Used on the Duplicate Org check page]
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>

        public ActionResult GetOrgFacilities( [DataSourceRequest]DataSourceRequest request, int? id = null )
		{
            var facilities = new List<FacilityGridViewModel>(); 
            if ( id.HasValue )
            {
                facilities = Repository.Organizations.GetFacilitiesOfOrganization( id.Value ).ToList();
            }

            DataSourceResult result = facilities.ToDataSourceResult( request );
            return Json( result, JsonRequestBehavior.AllowGet );
        }

        #endregion GetOrgFacilities

        #region GetOrgUsers

        /// <summary>
		/// Returns users of an organization. [Used on the Duplicate org check page]
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>

        public ActionResult GetOrgUsers( [DataSourceRequest]DataSourceRequest request, int? id = null )
		{
            var contacts = new List<OrganizationContactGridViewModel>();
            if ( id.HasValue )
            {
                contacts = Repository.OrganizationContacts.GridSearch( id.Value ).ToList();
            }

            DataSourceResult result = contacts.ToDataSourceResult( request );
            return Json( result, JsonRequestBehavior.AllowGet );
		}

        #endregion GetOrgUsers

        #region GuidanceMessage

        public void GuidanceMessageExportToExcel( int CERSID, int? FSEID = null, int? FSERID = null, int? EntityID = null )
		{
			IEnumerable<GuidanceMessage> guidanceMessages = new List<GuidanceMessage>();
			if ( FSEID.HasValue && FSEID != 0 )
			{
				var fse = Repository.FacilitySubmittalElements.GetByID( FSEID.Value );
				guidanceMessages = fse.GetGuidanceMessagesWithResources();
			}
			if ( FSERID.HasValue && FSERID != 0 )
			{
				var fser = Repository.FacilitySubmittalElementResources.GetByID( FSERID.Value );
				if ( EntityID.HasValue && EntityID != 0 )
				{
					guidanceMessages = fser.GuidanceMessages.Where( g => g.ResourceEntityID == EntityID && !g.Voided );
				}
				else
				{
					guidanceMessages = fser.GuidanceMessages.Where( g => !g.Voided );
				}
			}

			ExportToExcel( "GuidanceMessages.xlsx", guidanceMessages.ToGuidanceMessageExportViewModel() );
		}

        //no longer relevant??? [ak 08/20/2014]

        //[HttpPost]
        //public ActionResult GuidanceMessageForSubmittalElement( int fseid )
        //{
        //    EntityGuidanceMessageViewModel viewModel = new EntityGuidanceMessageViewModel()
        //    {
        //        GuidanceMessages = Repository.GuidanceMessages.GetGuidanceMessages( fseID: fseid ),
        //        FSEID = fseid,
        //        AjaxController = "Facility",
        //        AjaxAction = "GetGuidanceMessagesBySubmittalElement",
        //        AjaxParameters = new { fseid = fseid }
        //    };

        //    var results = RenderPartialViewToString( "_GuidanceMessageGrid", viewModel );
        //    bool success = true;
        //    var fse = Repository.FacilitySubmittalElements.GetByID( fseid );

        //    if ( CurrentUserRoles.HasRoles( fse.Facility.OrganizationID, Context.Organization, PermissionRole.OrgViewer ) )
        //    {
        //        success = true;
        //    }
        //    else
        //    {
        //        success = false;
        //        results = "You are not authorized to view this content.";
        //    }

        //    return Json( new { success = success, message = results } );
        //}

        //[HttpPost]
        //public ActionResult GuidanceMessageForSubmittalElementResource( int fserid )
        //{
        //    EntityGuidanceMessageViewModel viewModel = new EntityGuidanceMessageViewModel()
        //    {
        //        GuidanceMessages = Repository.GuidanceMessages.GetGuidanceMessages( fserID: fserid ), //TODO: Investigate Further...
        //        FSERID = fserid,
        //        AjaxController = "Facility",
        //        AjaxAction = "GetGuidanceMessagesBySubmittalElementResource",
        //        AjaxParameters = new { fserid = fserid }
        //    };
        //    var results = RenderPartialViewToString( "_GuidanceMessageGrid", viewModel );

        //    var fser = Repository.FacilitySubmittalElementResources.GetByID( fserid );
        //    bool success = false;
        //    if ( CurrentUserRoles.HasRoles( fser.FacilitySubmittalElement.Facility.OrganizationID, Context.Organization, PermissionRole.OrgViewer ) )
        //    {
        //        success = true;
        //    }
        //    else
        //    {
        //        success = false;
        //        results = "You are not authorized to view this content.";
        //    }

        //    return Json( new { success = success, message = results } );
        //}

        //[HttpPost]
        //public ActionResult GuidanceMessageForSubmittalElementResourceEntity( int fserid, int entityid )
        //{
        //    EntityGuidanceMessageViewModel viewModel = new EntityGuidanceMessageViewModel()
        //    {
        //        GuidanceMessageGridViewModelCollection = Repository.GuidanceMessages.GetGuidanceMessages( fserID: fserid, entityID: entityid ).ToGridView(),
        //        GuidanceMessages = Repository.GuidanceMessages.GetGuidanceMessages( fserID: fserid, entityID: entityid ),
        //        FSERID = fserid,
        //        EntityID = entityid,
        //        AjaxController = "Facility",
        //        AjaxAction = "GetGuidanceMessagesBySubmittalElementResourceEntity",
        //        AjaxParameters = new { fserid = fserid, entityid = entityid }
        //    };
        //    var results = RenderPartialViewToString( "_GuidanceMessageGrid", viewModel );

        //    var fser = Repository.FacilitySubmittalElementResources.GetByID( fserid );
        //    bool success = false;
        //    if ( CurrentUserRoles.HasRoles( fser.FacilitySubmittalElement.Facility.OrganizationID, Context.Organization, PermissionRole.OrgViewer ) )
        //    {
        //        success = true;
        //    }
        //    else
        //    {
        //        success = false;
        //        results = "You are not authorized to view this content.";
        //    }

        //    return Json( new { success = success, message = results } );
        //}

        #endregion GuidanceMessage

        #region List

        public ActionResult List( int organizationId )
		{
			return RedirectToAction( "OrganizationFacilitySearch", new { organizationId = organizationId } );
		}

        #endregion List

        #region Map

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Map( int organizationId, int CERSID )
		{
			FacilityViewModel viewModel = SystemViewModelData.BuildUpFacilityViewModel( CERSID );

			return View( viewModel );
		}

        #endregion Map

        #region New Organization

        public ActionResult New( int? organizationId = null )
		{
#if FM2
			//if we are using the Facility Management v2.0 API, then redirect to that new controller section.
            return RedirectToRoute( FacilityManagementRoute.StartWizard, organizationId );
#endif

			var facilityViewModel = SystemViewModelData.BuildUpFacilityViewModel();
			facilityViewModel.Cities = Repository.Places.Search().OrderBy( o => o.Name ).Select( p => p.Name );
			facilityViewModel.ZipCodes = Repository.ZipCodes.Search().OrderBy( o => o.ZipCodeID ).Select( p => p.ZipCodeID.ToString() );
			facilityViewModel.AssociatedOrganizations = Repository.Contacts.GetOrganizations( CurrentAccountID );
			return View( facilityViewModel );
		}

        [HttpPost]
        public ActionResult New(FacilityViewModel viewModel, int? organizationID = null)
        {
            //Trim all the properties of spaces.
            viewModel = Trimmer(viewModel);

            // Validate if the search criteria is provided. Street, zip code and city are to be provided,
            // otherwise error message
            if (string.IsNullOrWhiteSpace(viewModel.Street))
            {
                ModelState.AddModelError("Street", "Street Address is blank.");
            }
            if (string.IsNullOrWhiteSpace(viewModel.City))
            {
                ModelState.AddModelError("City", "City is blank");
            }
            if (string.IsNullOrWhiteSpace(viewModel.ZipCode))
            {
                ModelState.AddModelError("ZipCode", "Zip Code is blank");
            }
            else
            {
                Regex regEx = new Regex(@"(^9\d{4}$)|(^9\d{4}-\d{4}$)");
                if (!regEx.Match(viewModel.ZipCode).Success)
                {
                    ModelState.AddModelError("ZipCode", "Zip Code must be in one of the following formats: 9#### or 9####-####");
                }
            }

            if (ModelState.IsValid)
            {
                //call the web svc to get the Washed address
                var washedAddress = Services.Geo.GetAddressInformation(viewModel.Street, viewModel.City, viewModel.ZipCode);

                //if (string.IsNullOrWhiteSpace(washedAddress.MelissaErrorCode)) //Washing was successful
                if (washedAddress.MelissaAddressWashSucceeded)
                {
                    //Keep in mind, the wash confidence could still be low, but melissa still gives us the parsed street values etc in that event
                    //Copy the washed address in to the ViewModel's Washed address fields. (This will be used for creating facility)

                    viewModel = CopyWashedAddress(viewModel, washedAddress.Street, washedAddress.City, washedAddress.ZipCode, washedAddress.StreetWithoutSuiteNumber, washedAddress.Suite);
                    viewModel.IsAddressWashed = washedAddress.MelissaAddressWashSucceeded;

                    //Check if the washed address is significantly different from the user entered address.
                    bool isSignificantChange = ValidateWashedAddressForSignificantChanges(viewModel.Street, viewModel.City, viewModel.ZipCode, viewModel.WashedStreet, viewModel.WashedCity, viewModel.WashedZipCode);
                    if (isSignificantChange == true)
                    {
                        //Get user's approval before applying standardized address.
                        return View("ConfirmAddress", viewModel);
                    }
                    else
                    {
                        //standardize the address since the changes are not significant.
                        viewModel = StandardizeAddress(viewModel, viewModel.WashedStreet, viewModel.WashedCity, viewModel.WashedZipCode, viewModel.WashedStreetWithoutSuite, viewModel.WashedSuite);
                    }
                    return RedirectToActionPermanent("AddFacilitySearchResult", new { street = viewModel.Street, city = viewModel.City, zipCode = viewModel.ZipCode, washedStreet = viewModel.WashedStreet, washedCity = viewModel.WashedCity, washedZipCode = viewModel.WashedZipCode, confirmAddressType = ConfirmAddressType.Washed.ToString(), isAddressWashed = viewModel.IsAddressWashed, organizationId = CurrentOrganizationID, washedStreetWithoutSuite = viewModel.WashedStreetWithoutSuite, washedSuite = viewModel.WashedSuite });
                }
                else //Washing wasn't successful.
                {
                    //So, at least wash the street address. [To convert Street to st, Boulevard to blvd, etc. We need to do this even when the address washing svc returned no results]
                    var parsedAddress = Services.Geo.ParseAddress(viewModel.Street);
                    viewModel.Street = parsedAddress.Range + " " + parsedAddress.PreDirection + " " + parsedAddress.StreetName + " " + parsedAddress.Suffix + " " + parsedAddress.PostDirection + " " + parsedAddress.Suite;

                    viewModel.IsAddressWashed = false;
                    viewModel = CopyWashedAddress(viewModel, viewModel.Street, viewModel.City, viewModel.ZipCode, viewModel.WashedStreetWithoutSuite);
                    return RedirectToActionPermanent("AddFacilitySearchResult", new { street = viewModel.Street, city = viewModel.City, zipCode = viewModel.ZipCode, washedStreet = viewModel.WashedStreet, washedCity = viewModel.WashedCity, washedZipCode = viewModel.WashedZipCode, confirmAddressType = ConfirmAddressType.UserEntered.ToString(), isAddressWashed = viewModel.IsAddressWashed, organizationId = CurrentOrganizationID, washedStreetWithoutSuite = viewModel.WashedStreetWithoutSuite, washedSuite = viewModel.WashedSuite });
                }
            }
            viewModel.AssociatedOrganizations = Repository.Contacts.GetOrganizations(CurrentAccountID);
            viewModel.Cities = Repository.Places.Search().Select(p => p.Name);
            viewModel.ZipCodes = Repository.ZipCodes.Search().OrderBy(o => o.ZipCodeID).Select(p => p.ZipCodeID.ToString());
            return View(viewModel);
        }

        #endregion New Organization

        #region Private Methods

        private FacilityViewModel CopyWashedAddress( FacilityViewModel viewModel, string washedStreet, string washedCity, string washedZipCode, string washedStreetWithoutSuite, string washedSuite = "" )
		{
			if ( !string.IsNullOrWhiteSpace( washedStreet ) )
			{
				viewModel.WashedStreet = washedStreet.Trim();
			}

			if ( !string.IsNullOrWhiteSpace( washedCity ) )
			{
				viewModel.WashedCity = washedCity.Trim();
			}

			if ( !string.IsNullOrWhiteSpace( washedZipCode ) )
			{
				viewModel.WashedZipCode = washedZipCode.Trim();
			}

			if ( !string.IsNullOrWhiteSpace( washedStreetWithoutSuite ) )
			{
				viewModel.WashedStreetWithoutSuite = washedStreetWithoutSuite.Trim();
			}

			if ( !string.IsNullOrWhiteSpace( washedSuite ) )
			{
				viewModel.WashedSuite = washedSuite.Trim();
			}

			return viewModel;
		}

		private void SetUserPermissions( int organizationID )
		{
			//Get the contact of this Account(user)
			Contact contact = Repository.OrganizationContacts.GetByAccount( CurrentAccountID );

			//Set the permissions for this user as -Admin
			Services.Security.AddContactToGroup( contact, organizationID, Context.Organization, permissionGroups: BuiltInPermissionGroup.OrgAdmins );

			//Refresh the session with the new role(s).
			CERSSecurityManager.ReloadCurrentUserRoles();
		}

		private FacilityViewModel StandardizeAddress( FacilityViewModel viewModel, string washedStreet, string washedCity, string washedZipCode, string washedStreetWithoutSuite, string washedSuite = "" )
		{
			//Set the regular fields to washed field values since user has chosen to go with Standardized values.
			viewModel.Street = washedStreet;
			viewModel.City = washedCity;
			viewModel.ZipCode = washedZipCode;
			viewModel.WashedStreetWithoutSuite = washedStreetWithoutSuite;
			viewModel.WashedSuite = washedSuite;

			return viewModel;
		}

		private FacilityViewModel Trimmer( FacilityViewModel viewModel )
		{
			viewModel.Street = !string.IsNullOrWhiteSpace( viewModel.Street ) ? viewModel.Street.Trim() : null;
			viewModel.City = !string.IsNullOrWhiteSpace( viewModel.City ) ? viewModel.City.Trim() : null;
			viewModel.ZipCode = !string.IsNullOrWhiteSpace( viewModel.ZipCode ) ? viewModel.ZipCode.Trim() : null;
			viewModel.FacilityID = !string.IsNullOrWhiteSpace( viewModel.FacilityID ) ? viewModel.FacilityID.Trim() : null;
			viewModel.BusinessName = !string.IsNullOrWhiteSpace( viewModel.BusinessName ) ? viewModel.BusinessName.Trim() : null;
			viewModel.OrgContactPhone = !string.IsNullOrWhiteSpace( viewModel.OrgContactPhone ) ? viewModel.OrgContactPhone.Trim() : null;
			viewModel.OrgContactTitle = !string.IsNullOrWhiteSpace( viewModel.OrgContactTitle ) ? viewModel.OrgContactTitle.Trim() : null;
			viewModel.OrganizationName = !string.IsNullOrWhiteSpace( viewModel.OrganizationName ) ? viewModel.OrganizationName.Trim() : null;
			return viewModel;
		}

		private bool ValidateWashedAddressForSignificantChanges( string street, string city, string zipCode, string washedStreet, string washedCity, string washedZipCode )
		{
			bool isSignificantChange = false;

			//If city is different, the change is significant
			if ( !washedCity.Equals( city, StringComparison.InvariantCultureIgnoreCase ) )
			{
				isSignificantChange = true;
			}

			//If zipcode is different then the change is significant
			if ( !washedZipCode.Equals( zipCode ) )
			{
				isSignificantChange = true;
			}

			//If street address has been modified by 6 or more characters, then let's take it as a significant change
			if ( ( washedStreet.Length - street.Length >= 6 ) || ( street.Length - washedStreet.Length >= 6 ) )
			{
				isSignificantChange = true;
			}

			return isSignificantChange;
		}

		#endregion Private Methods

        #region Notifications

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Notifications( int organizationId, int CERSID )
		{
			FacilityViewModel viewModel = SystemViewModelData.BuildUpFacilityViewModel( CERSID );

			var currentAccount = CurrentAccount;
			viewModel.Events = null;// Services.Events.GetEventItemsForAccount(CurrentAccount,
			// viewModel.Entity.Organization, Context.Organization).Where(p =>
			// p.F.ToGridView());

			return View( viewModel );
		}

        #endregion Notifications

        #region OpenFacilitySubmittalElementResourceDocument

        [Authorize]
		[HttpGet]
		public ActionResult OpenFacilitySubmittalElementResourceDocument( int FSERID, string title )
		{
			var fserd = Repository.FacilitySubmittalElementResources.GetByID( FSERID );
			var fsed = Repository.FacilitySubmittalElementResourceDocuments.Search( FSERID: FSERID ).First().Documents.FirstOrDefault( d => !d.Voided );
			var document = fsed.Document;

			var docBytes = DocumentStorage.GetBytes( document.Location );
			string contentType = IOHelper.GetContentType( document.Location );
			string ext = System.IO.Path.GetExtension( document.Location ).ToLower();

			if ( docBytes == null )
			{
				return RedirectToAction( "DocumentNotFound", "Error" );
			}

			return File( docBytes, contentType, fsed.Name + ext );
		}

        #endregion OpenFacilitySubmittalElementResourceDocument

        #region OrganizationAccessRequest

        public ActionResult OrganizationAccessRequest( int prospectiveOrgID, string facilityName, string street, string city, string zipcode )
		{
			var fvm = SystemViewModelData.BuildUpFacilityViewModel();
			fvm.Entity.Name = facilityName;
			fvm.Entity.Street = street;
			fvm.Entity.City = city;
			fvm.Entity.ZipCode = zipcode;
			fvm.Entity.Organization = Repository.Organizations.GetByID( prospectiveOrgID );
			var oarTarget = Services.Events.GetOrganizationAccessRequestTarget( fvm.Entity.Organization );
			fvm.EventTypeCode = oarTarget.Type;
			fvm.Regulator = oarTarget.Regulator;
			fvm.NotificationContacts = oarTarget.Contacts;

			return View( "OrganizationAccessRequest", fvm );
		}

		[HttpPost]
		public ActionResult OrganizationAccessRequest( FacilityViewModel fvm, string phoneNumber, string jobTitle )
		{
			var org = Repository.Organizations.GetByID( fvm.Entity.OrganizationID );
			var contact = Repository.Contacts.EnsureExists( CurrentAccountID );

			//Figures out who to send the "Request" to and delivers the emails too.
			var evnt = Services.Events.CreateOrganizationAccessRequest( org, contact, phoneNumber, jobTitle );

			if ( evnt != null )
			{
				//There can be 3 possibilities. They are:
				//1. the ball is in the organizations regulator contacts (OrgUserARNotificiation) court to approve/deny request.
				//2. the ball is in the Organization Contact's (lead users) court to approve/deny request.
				//3. the ball is in the CERS Help Center's court to approve/deny request.
				fvm.EventTicketCode = evnt.TicketCode;
				fvm.EventTypeCode = evnt.GetEventTypeCode();
				fvm.Regulator = evnt.Regulator;
				if ( evnt.Notifications.Count > 0 )
				{
					List<Contact> contacts = new List<Contact>();
					evnt.Notifications.ToList().ForEach( n => contacts.Add( n.Contact ) );
					fvm.NotificationContacts = contacts;
				}
			}

			//Go to Org Access Request Confirm
			return View( "OrganizationAccessRequestConfirmation", fvm );
		}

        #endregion OrganizationAccessRequest

        #region OrganizationFacilityHome

        /// <summary>
		/// This is the OLD CRAZY Page (new name for it is Facility Submittal Preparation)
		/// </summary>
		/// <param name="organizationId"></param>
		/// <param name="CERSID"></param>
		/// <returns></returns>
		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		[CheckDeferredProcessing( ignoreProcessing: true )]
		public ActionResult OrganizationFacilityHome( int organizationId, int CERSID )
		{
			/* Redirect to the "new" crazy page */
			return RedirectToAction( "FacilitySubmittalPreparation", new { organizationId = organizationId, CERSID = CERSID } );
		}

        #endregion OrganizationFacilityHome

        #region RetrieveScheduledPrintJobDocument

        [Authorize]
		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult RetrieveScheduledPrintJobDocument( int printJobID )
		{
			CERS.ViewModels.PrintJobViewModel viewModel = SystemViewModelData.BuildUpPrintJobDocumentRetrievalViewModel( printJobID );
			if ( viewModel.Entity != null )
			{
				viewModel.DocumentLink = Url.Action( "OpenDocument", "Tools", new { documentID = viewModel.Entity.DocumentID, title = String.Format( "{0}_Documents", viewModel.Entity.CERSID ) } );
			}
			return View( viewModel );
		}

        #endregion RetrieveScheduledPrintJobDocument

        #region ReturnCurrentSubmittalElement

        [HttpPost]
		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult ReturnCurrentSubmittalElement( int organizationID, int CERSID, int submittalElementTemplateID )
		{
			CurrentFacilitySubmittalViewModel viewModel = new CurrentFacilitySubmittalViewModel()
			{
				OrganizationID = organizationID,
				isFirstElement = ( submittalElementTemplateID == 1 ) ? true : false,
				CurrentSubmittalElement = Repository.FacilityRegulatorSubmittalElements.GetCurrentFacilitySubmittalElements( CERSID, submittalElementTemplateID ).FirstOrDefault()
			};

			var results = string.Empty;

			results = RenderPartialViewToString( "_CurrentSubmittalElement", viewModel );

			return Json( new { success = true, message = results } );
		}

        #endregion ReturnCurrentSubmittalElement

        #region Search

        public ActionResult Search( int organizationId )
		{
			var org = Repository.Organizations.GetGridViewByID( organizationId );

			//If the URL does not contain a valid organizationId, take user to the home page
			if ( org == null )
			{
				return RedirectToRoute( CommonRoute.OrganizationHome.ToString(), new { organizationId = organizationId } );
			}

			List<FacilityViewModel.DropDownListViewModel> reportingRequirementCollection = new List<FacilityViewModel.DropDownListViewModel>();
			reportingRequirementCollection.Add( new FacilityViewModel.DropDownListViewModel { ID = 4, Name = "Regulated" } );
			reportingRequirementCollection.Add( new FacilityViewModel.DropDownListViewModel { ID = 1, Name = "Non-Regulated" } );

			Setting setting = Repository.Settings.GetByKey( "FacilityDetailLastUpdatedOn" );
			FacilityViewModel viewModel = new FacilityViewModel()
			{
				OrganizationName = org.Name,
				SourceOrganizationID = organizationId,
				FacilityCount = org.FacilityCount,
				Regulators = Repository.Regulators.Search( applySecurityFilter: false, matrices: CurrentUserRoles ).Where( r => r.TypeID == (int)RegulatorType.CUPA || r.TypeID == (int)RegulatorType.PA || r.TypeID == (int)RegulatorType.UPOA ).OrderBy( r => r.NameShort ),
				CountyCollection = Repository.Counties.GetAll(),
				FacilityDetailLastUpdated = Convert.ToDateTime( setting.Value ).ToShortDateString() + " " + Convert.ToDateTime( setting.Value ).ToShortTimeString(),
				ReportingRequirementCollection = reportingRequirementCollection,
			};

			return View( viewModel );
		}

        public JsonResult Search_Async( [DataSourceRequest]DataSourceRequest request, int organizationId, string facilityName = null, int? CERSID = null, string street = null, string city = null, string zipCode = null, DateTime? lastSubmittedFrom = null, DateTime? lastSubmittedTo = null, int? regulatorID = null, int? countyID = null, string facilityID = null, int reportingRequirement = 0 )
        {
            var facilities = Repository.Facilities.GridSearch( name: HttpUtility.UrlDecode(facilityName).Trim(),
                                                                CERSID:CERSID,
                                                                street: HttpUtility.UrlDecode(street).Trim(),
                                                                city: HttpUtility.UrlDecode(city).Trim(),
                                                                zipCode: HttpUtility.UrlDecode(zipCode).Trim(),
                                                                organizationID:organizationId,
                                                                applySecurityFilter:true,
                                                                securityFilterContext:Context.Organization,
                                                                matrices:CurrentUserRoles,
                                                                lastSubmittalFrom:lastSubmittedFrom,
                                                                lastSubmittalTo:lastSubmittedTo,
                                                                regulatorID:regulatorID,
                                                                countyID:countyID,
                                                                facilityID: HttpUtility.UrlDecode(facilityID).Trim(),
                                                                reportingRequirement:reportingRequirement
                                                             );

            if ( reportingRequirement != (int)ReportingRequirement.NotApplicable )
            {
                var tempResults = facilities.ToList();
                foreach ( var f in tempResults )
                {
                    var rr = Repository.FacilityRegulatorSubmittalElements.GetForFacility( f.CERSID ).FirstOrDefault();

                    if ( rr != null )
                    {
                        f.IsRegulated = ( rr.ReportingRequirementID != (int)ReportingRequirement.NotApplicable );
                    }
                }
                facilities = tempResults;
            }

            return Json( facilities.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        #endregion Search

        #region SelectOrganization

        /// <summary>
		/// Called from the Org select of "Duplicate Org check".
		/// </summary>
		/// <param name="id"></param>
		/// <param name="createType"></param>
		/// <param name="facilityName"></param>
		/// <param name="street"></param>
		/// <param name="city"></param>
		/// <param name="zipcode"></param>
		/// <returns></returns>
		public ActionResult SelectOrganization( int id, int? sourceOrgID, int createType, string facilityName, string street, string city, string zipcode, string washedStreet, string washedCity, string washedZipcode, bool isAddressWashed, int? countyID, string newFacilityNotes )
		{
			//Check to see if user belongs to the Selected Org
			var orgUsers = Repository.OrganizationContacts.Search( id, CurrentAccountID );
			if ( orgUsers != null && orgUsers.Count() > 0 )
			{
				var viewModel = SystemViewModelData.BuildUpFacilityViewModel();

				//If user does belong to ORG, go to: Facility Added / Facility transfer request based on the request
				//check if we need to add a facility or transfer a facility.
				if ( createType == (int)CreateOrganizationReason.AddFacility )
				{
					//Create facility for this organization.
					viewModel.Entity = Services.Facilities.Create( id, facilityName, street, city, zipcode, countyID );

					try
					{
						AddressInformation standardizedAddress = Services.Geo.GetAddressInformation( viewModel.Street, viewModel.City, viewModel.ZipCode, "CA" );

						if ( standardizedAddress.Longitude != 0 && standardizedAddress.Latitude != 0 )
						{
							CERSFacilityGeoPoint geoPoint = new CERSFacilityGeoPoint();
							geoPoint.CERSID = viewModel.Entity.CERSID;
							geoPoint.GeographicReferencePointID = standardizedAddress.GeographicReferencePointID;
							geoPoint.HorizontalAccuracyMeasure = standardizedAddress.HorizontalAccuracyMeasure;
							geoPoint.HorizontalCollectionMethodID = standardizedAddress.HorizontalCollectionMethodID;
							geoPoint.HorizontalReferenceDatumID = standardizedAddress.HorizontalReferenceDatumID;
							geoPoint.LatitudeMeasure = standardizedAddress.Latitude;
							geoPoint.LongitudeMeasure = standardizedAddress.Longitude;
							geoPoint.DataCollectionDate = DateTime.Now;
							Repository.CERSFacilityGeoPoints.Create( geoPoint );
						}
					}
					catch ( Exception ex )
					{
						string message = ex.Message;
					}

					//set user permissions as Admin user, refresh roles in the session.
					SetUserPermissions( id );

					//Write notification - Facility added to existing Org.
					Contact contact = Repository.Contacts.GetByAccount( CurrentAccountID );
					Services.Events.CreateFacilityAdded( viewModel.Entity.Organization, viewModel.Entity, contact, CurrentAccount, newFacilityNotes );

					return View( "FacilityAdded", viewModel );
				}
				else
				{
					//Regulator authorized transfer facility request
					var sourceOrganization = Repository.Organizations.GetByID( sourceOrgID.Value );
					var targetOrganization = Repository.Organizations.GetByID( id );
					Contact contact = Repository.OrganizationContacts.GetByAccount( CurrentAccountID );
					Event evt = Services.Events.CreateRegulatorAuthorizedFacilityTransferRequest( sourceOrganization, targetOrganization, viewModel.Entity, contact, CurrentAccount, newFacilityNotes );
					viewModel.EventTicketCode = evt.TicketCode;
					viewModel.EventTypeCode = evt.GetEventTypeCode();
					viewModel.Regulator = evt.Regulator;
					if ( evt.Notifications != null && evt.Notifications.Count > 0 )
					{
						List<Contact> notificationContacts = new List<Contact>();
						evt.Notifications.ToList().ForEach( n => notificationContacts.Add( n.Contact ) );
						viewModel.NotificationContacts = notificationContacts;
					}
					return View( "RegulatorAuthorizedFacilityTransferRequest", viewModel );
				}
			}

			//If user does not belong to ORG, go to "Org Access Request"
			else
			{
				return RedirectToActionPermanent( "OrganizationAccessRequest", new { prospectiveOrgID = id, facilityName = facilityName, street = street, city = city, zipcode = zipcode } );
			}
		}

        #endregion SelectOrganization

        #region SetFacilityReportingRequirement

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult SetFacilityReportingRequirement( int FRSEID, int reportingRequirementID, int organizationId, int CERSID )
		{
			//Update Facility Regulator Submittal Element
			var facilityRegulatorSubmittalElement = Repository.FacilityRegulatorSubmittalElements.GetByID( FRSEID );
			facilityRegulatorSubmittalElement.ReportingRequirementID = reportingRequirementID;
			Repository.FacilityRegulatorSubmittalElements.Save( facilityRegulatorSubmittalElement );
			return RedirectToRoute( GetRouteName( OrganizationFacility.Home ), new { organizationId = organizationId, CERSID = CERSID } );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult SetReportingRequirement( int FRSEID, int reportingRequirementID, int organizationId, int CERSID )
		{
			//Update Facility Regulator Submittal Element
			var facilityRegulatorSubmittalElement = Repository.FacilityRegulatorSubmittalElements.GetByID( FRSEID );
			facilityRegulatorSubmittalElement.ReportingRequirementID = reportingRequirementID;
			Repository.FacilityRegulatorSubmittalElements.Save( facilityRegulatorSubmittalElement );

			//If FSE is Haz Inventory, then also set same reporting requiremet for Emergency Response Plan
			if ( facilityRegulatorSubmittalElement.SubmittalElementID == (int)SubmittalElementType.HazardousMaterialsInventory )
			{
				var fserERPL = Repository.FacilityRegulatorSubmittalElements.GetByID( CERSID, (int)SubmittalElementType.EmergencyResponseandTrainingPlans );
				fserERPL.ReportingRequirementID = reportingRequirementID;
				Repository.FacilityRegulatorSubmittalElements.Save( fserERPL );
				Services.Events.CreateFacilitySubmittalElementNotApplicable( fserERPL.Regulator, fserERPL.Facility.Organization, fserERPL.Facility, fserERPL.SubmittalElement, CurrentAccount );
			}

			//If FSE is EmergencyResponsePlan, then also set the same reoprting requirement to Haz Inventory
			if ( facilityRegulatorSubmittalElement.SubmittalElementID == (int)SubmittalElementType.EmergencyResponseandTrainingPlans )
			{
				var fserHWInv = Repository.FacilityRegulatorSubmittalElements.GetByID( CERSID, (int)SubmittalElementType.HazardousMaterialsInventory );
				fserHWInv.ReportingRequirementID = reportingRequirementID;
				Repository.FacilityRegulatorSubmittalElements.Save( fserHWInv );
				Services.Events.CreateFacilitySubmittalElementNotApplicable( fserHWInv.Regulator, fserHWInv.Facility.Organization, fserHWInv.Facility, fserHWInv.SubmittalElement, CurrentAccount );
			}

			UpdateBizActivitiesReportingRequirements( facilityRegulatorSubmittalElement );

			Services.Events.CreateFacilitySubmittalElementNotApplicable( facilityRegulatorSubmittalElement.Regulator, facilityRegulatorSubmittalElement.Facility.Organization, facilityRegulatorSubmittalElement.Facility, facilityRegulatorSubmittalElement.SubmittalElement, CurrentAccount );

			return RedirectToRoute( GetRouteName( OrganizationFacility.Home ), new { organizationId = organizationId, CERSID = CERSID } );
		}

        #endregion SetFacilityReportingRequirement

        #region StartSubmittalHistory_Async

        public JsonResult StartSubmittalHistory_Async( int CERSID, int SEID )
		{
			SubmittalElementType type = (SubmittalElementType)SEID;
			var rawItems = Repository.FacilitySubmittalElements.Search( CERSID: CERSID, type: type ).Where( p => p.StatusID != (int)SubmittalElementStatus.Draft ).OrderByDescending( p => p.SubmittedDateTime );
			var formattedItems = ( from r in rawItems
								   select new
								   {
									   FSEID = r.ID,
									   DisplayText = r.SubmittedDateTime.Value.ToShortDateString() + " (" + r.Status.Name + ( r.SubmittalActionDateTime != null ? " " + r.SubmittalActionDateTime.Value.ToShortDateString() : "" ) + ")"
								   } ).Take( 15 );

			var result = new
			{
				History = formattedItems
			};

			return Json( result, JsonRequestBehavior.AllowGet );
		}

        #endregion StartSubmittalHistory_Async

        #region SubmittalElementCommentToRegulator

        [HttpPost]
        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgApprover, PermissionRole.OrgEditor )]
		public void SubmittalElementCommentToRegulator( int fseID, string comment )
		{
			FacilitySubmittalElement submittalElement = Repository.FacilitySubmittalElements.GetByID( fseID );
			submittalElement.SubmittedByComments = comment;
			Repository.FacilitySubmittalElements.Save( submittalElement );
		}

        #endregion SubmittalElementCommentToRegulator

        #region SubmittalElementGetCommentToRegulator

        [HttpPost]
		public JsonResult SubmittalElementGetCommentToRegulator( int fseID )
		{
			bool success = false;
			string message = "";
			FacilitySubmittalElement submittalElement = Repository.FacilitySubmittalElements.GetByID( fseID );
			message = submittalElement.SubmittedByComments;
			success = true;
			return Json( new { Success = success, Message = message } );
		}

        #endregion SubmittalElementGetCommentToRegulator

        #region SubmittalElements

        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult SubmittalElements( int organizationId, int CERSID )
		{
			FacilityViewModel viewModel = new FacilityViewModel()
			{
				Entity = Repository.Facilities.GetByID( CERSID ),
				FacilityRegulatorSubmittalElementCollection = Repository.FacilityRegulatorSubmittalElements.Search( CERSID: CERSID )
			};

			return View( viewModel );
		}

        #endregion SubmittalElements

        #region Submittals

        [HttpGet]
		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Submittals( int organizationId, int CERSID )
		{
			FacilitySubmittalElementViewModel viewModel = new FacilitySubmittalElementViewModel()
			{
				Facility = Repository.Facilities.GetByID( CERSID ),
                SubmittedEvents = Repository.FacilitySubmittalElements.SearchByRegulator( organizationId: organizationId, CERSID: CERSID ).Where( s => s.FacilitySubmittalID.HasValue && !s.FacilitySubmittal.Voided ).GetSubmittalEvent( Repository ).OrderByDescending( o => o.SubmittedDate )
			};

			return View( viewModel );
		}

        #endregion Submittals

        #region Summary

        [HttpGet]
		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Summary( int organizationId, int CERSID )
		{
			//FacilityViewModel viewModel = SystemViewModelData.BuildUpFacilityViewModel(CERSID);

			var viewModel = new FacilityViewModel() { Entity = Repository.Facilities.GetByID( CERSID, true ) };

			//need to catch non existance direct hyperlink (from old email, etc.)
			if ( viewModel.Entity == null )
			{
				viewModel.Entity = new Facility() { CERSID = CERSID, Name = string.Format( "There has never been a valid facility record for CERS ID {0}", CERSID ), Street = "Facility Not Found" };
			}
			else if ( viewModel.Entity.Voided )
			{
				viewModel.Entity = new Facility() { CERSID = CERSID, Name = string.Format( "The facility record for CERS ID {0} has been deleted", CERSID ), Street = "Facility Not Found" };
			}

			return View( viewModel );
		}

        #endregion Summary

        #region UserAuthorizedFacilityTransfer

        [HttpPost]
		//When the user is Admin on both the Organizations, he can transfer a facility from One Org to the other without Regulator's approval.
		public ActionResult UserAuthorizedFacilityTransfer( FacilityViewModel fvm, FormCollection collection )
		{
			if ( string.IsNullOrWhiteSpace( fvm.NewFacilityNotes ) )
			{
				ModelState.AddModelError( "TransferReason", "Invalid transfer reason" );
			}

			if ( ModelState.IsValid )
			{
				try
				{
					var facilityToBeTransferred = Repository.Facilities.GetByID( fvm.Entity.ID );
					var sourceOrganization = facilityToBeTransferred.Organization;
					if ( TryUpdateModel( facilityToBeTransferred, "Entity" ) )
					{
						//reparent the facility to new org.
						facilityToBeTransferred.OrganizationID = Convert.ToInt32( fvm.SelectedOrganizationID );
						Repository.Facilities.Update( facilityToBeTransferred );

						//update the old orgs metadata.
						Repository.Organizations.Save( sourceOrganization );

						Contact contact = Repository.Contacts.GetByAccount( CurrentAccountID );

						//get the target org
						var targetOrganization = Repository.Organizations.GetByID( fvm.SelectedOrganizationID.ToInt32() );

						Services.Events.UserAuthorizedFacilityTransferred( sourceOrganization, targetOrganization, facilityToBeTransferred, contact, CurrentAccount, fvm.NewFacilityNotes );

						//update the target orgs metadata to reflect addition.
						Repository.Organizations.Save( targetOrganization );
						return View( "UserAuthorizedFacilityTransferConfirmation", fvm );
					}
				}
				catch
				{
					ModelState.AddModelError( "FacilityTransferFail", "Unable to complete facility transfer" );
				}
			}

			return View( "UserAuthorizedFacilityTransferRequest", fvm );
		}

        #endregion UserAuthorizedFacilityTransfer

        #region ExportToExcel

        public void ExportToExcelFacilityListingBrief( int? CERSID, string businessName, string street, string city, string zipCode, int? organizationId, DateTime? lastSubmittedFrom, DateTime? lastSubmittedTo, int reportingRequirement = 0, int? regulatorID = null, int? countyID = null, string facilityID = null )
		{
			FacilityViewModel viewModel = new FacilityViewModel()
			{
				FacilityViewGridView = Repository.Facilities.GridSearch( CERSID: CERSID,
																		name: businessName,
																		street: street,
																		city: city,
																		zipCode: zipCode,
																		organizationID: organizationId,
																		applySecurityFilter: true,
																		matrices: CurrentUserRoles,
																		lastSubmittalFrom: lastSubmittedFrom,
																		lastSubmittalTo: lastSubmittedTo,
																		reportingRequirement: reportingRequirement,
																		regulatorID: regulatorID,
																		countyID: countyID,
																		facilityID: facilityID
																		)
			};

			ExcelColumnMapping[] columns = new ExcelColumnMapping[] { new ExcelColumnMapping("CERSID"),
																	  new ExcelColumnMapping("Name"),
																	  new ExcelColumnMapping("Street"),
																	  new ExcelColumnMapping("City"),
																	  new ExcelColumnMapping("ZipCode"),
																	  new ExcelColumnMapping("FacilityID"),
																	  new ExcelColumnMapping("LastSubmittalSubmittedOnDate", "Last Submittal On"),
																	  new ExcelColumnMapping("CUPAAbbreviatedName","CUPA Abbreviated Name"),
																	};

			ExportToExcel( "FacilityListing.xlsx", viewModel.FacilityViewGridView, columns );
		}

		public void ExportToExcelFacilityListingWithDetails(
			int? CERSID,
			string businessName,
			string street,
			string city,
			string zipCode,
			int? organizationId,
			DateTime? lastSubmittedFrom,
			DateTime? lastSubmittedTo,
			int reportingRequirement = 0,
			StringSearchOption nameSearchOption = StringSearchOption.Contains,
			StringSearchOption streetSearchOption = StringSearchOption.Contains,
			int? regulatorID = null,
			int? countyID = null,
			string facilityID = null
			)
		{
			HttpContext.Server.ScriptTimeout = 1000;

			string excelTemplateFilePath = System.IO.Path.Combine( Server.MapPath( "~" ), @"Content\TemplateFiles\FacilityListingTemplate.xlsx" );

			var workbook = Services.Excel.ExportToExcelFacilityListingWithDetails(
				excelTemplateFilePath,
				CERSID,
				facilityID,
				null,
				businessName.ToUrlDecoded(),
				street.ToUrlDecoded(),
				city,
				zipCode.ToUrlDecoded(),
				regulatorID,
				null,
				lastSubmittedFrom,
				lastSubmittedTo,
				nameSearchOption,
				streetSearchOption,
				countyID,
				reportingRequirement,
				true,
				CurrentUserRoles,
				 organizationID: organizationId
				);

			SetDownloadFileHeader( "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FacilityListing(Details).xlsx" );

			workbook.Save( Response.OutputStream );
			Response.End();
		}

		#endregion ExportToExcel

		#region GridActions

		[GridAction]
		public ActionResult GetBPFacilityChemicals( int fserid )
		{
			return View( new GridModel( GetBPChemicals( fserid ).BPFacilityChemicals.ToGridView() ) );
		}

		[GridAction]
		public ActionResult GetFacilities( int CERSID, string street, string city, string zipCode, int organizationID )
		{
			var facilities = Repository.Facilities.GridSearch( CERSID: CERSID, street: street, city: city, zipCode: zipCode, organizationID: organizationID );
			if ( facilities == null )
			{
				facilities = new List<FacilityGridViewModel>();
			}
			return View( new GridModel( facilities ) );
		}

        //no longer relevant??? [ak 09/10/2014]

        //[GridAction]
        //public ActionResult GetGuidanceMessagesBySubmittalElement( int fseid, int? levelID = null )
        //{
        //    return View( new GridModel( Repository.GuidanceMessages.GetGuidanceMessages( fseID: fseid, levelID: levelID ).ToGridView() ) );
        //}

        //[GridAction]
        //public ActionResult GetGuidanceMessagesBySubmittalElementResource( int fserid, int? levelID = null )
        //{
        //    return View( new GridModel( Repository.GuidanceMessages.GetGuidanceMessages( fserID: fserid, levelID: levelID ).ToGridView() ) );
        //}

        //[GridAction]
        //public ActionResult GetGuidanceMessagesBySubmittalElementResourceEntity( int fserid, int entityid, int? levelID = null )
        //{
        //    return View( new GridModel( Repository.GuidanceMessages.GetGuidanceMessages( fserID: fserid, levelID: levelID, entityID: entityid ).ToGridView() ) );
        //}

		#endregion GridActions

		#region Private

		private FSERBPFacilityChemicalViewModel GetBPChemicals( int fserid )
		{
			var facilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( fserid );

			var viewModel = new FSERBPFacilityChemicalViewModel()
			{
				FacilitySubmittalElementResource = facilitySubmittalElementResource,
				BPFacilityChemicals = Repository.BPFacilityChemicals.Search( FSERID: fserid )
			};

			return viewModel;
		}

		private RedirectToRouteResult OrganizationFacilityHomeCERSCheck( string CERSIDstring )
		{
			RedirectToRouteResult route = null;
			//Check to see if the CERSID Has a value
			int CERSID = 0;
			try
			{
				if ( !int.TryParse( CERSIDstring, out CERSID ) )
				{
					route = RedirectToRoute( GetRouteName( OrganizationFacility.DraftSelect ), new { organizationId = CurrentOrganizationID } );
				}
			}
			catch
			{
				var facilities = Repository.Facilities.Search( organizationID: CurrentOrganizationID );

				//If orgainzation contains one facilitiy, set CERSID and continue
				if ( facilities.Count() == 1 )
				{
					route = RedirectToRoute( GetRouteName( OrganizationFacility.Home ), new { organizationId = CurrentOrganizationID, CERSID = facilities.First().CERSID } );
				}
				else
				{
					route = RedirectToRoute( GetRouteName( OrganizationFacility.DraftSelect ), new { organizationId = CurrentOrganizationID } );
				}
			}

			return route;
		}

		/// <summary>
		/// Updates the Business Activitiy form and set the appropriate question to "N"
		/// </summary>
		/// <param name="facilityRegulatorSubmittalElement"></param>
		private void UpdateBizActivitiesReportingRequirements( FacilityRegulatorSubmittalElement facilityRegulatorSubmittalElement )
		{
			//Get FacilitySubmittalElement for Facility Information.
			var fse = Repository.FacilitySubmittalElements.GetLatestFacInfoSubmittalElement( facilityRegulatorSubmittalElement.CERSID );

            //If There was no Accepted/UnderReview/Submitted submittal and no Draft, then there is nothing to update (It means ALL submitted submittals were Not Accepted or no submited submittal EVER)
            if ( fse == null )
            {
                return;
            }

			var resources = fse.Resources.Where( resource => resource.ResourceTypeID == (int)ResourceType.BusinessActivities && !resource.Voided );
			var bizActivityResource = resources.FirstOrDefault();

			//Get BPActivity
			var bpActivity = bizActivityResource.BPActivities.FirstOrDefault();
			if ( bpActivity != null )
			{
				switch ( (SubmittalElementType)facilityRegulatorSubmittalElement.SubmittalElementID )
				{
					case SubmittalElementType.HazardousMaterialsInventory:
						bpActivity.HMOnSite = "N";
						break;

					case SubmittalElementType.EmergencyResponseandTrainingPlans:
						bpActivity.HMOnSite = "N";
						break;

					case SubmittalElementType.UndergroundStorageTanks:
						bpActivity.OwnOrOperateUST = "N";
						break;

					case SubmittalElementType.OnsiteHazardousWasteTreatmentNotification:
						bpActivity.HWGenerator = "N";
						break;

					case SubmittalElementType.RecyclableMaterialsReport:
						bpActivity.Recycle = "N";
						break;

					case SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification:
						bpActivity.RWConsolidationSite = "N";
						break;

					case SubmittalElementType.HazardousWasteTankClosureCertification:
						bpActivity.HWTankClosure = "N";
						break;

					case SubmittalElementType.AbovegroundPetroleumStorageTanks:
						bpActivity.OwnOrOperateAPST = "N";
						break;

					case SubmittalElementType.CaliforniaAccidentalReleaseProgram:
						bpActivity.CalARPRegulatedSubstances = "N";
						break;

					default:
						break;
				}

                Repository.BPActivities.Save( bpActivity );
            }
		}

		#endregion Private

		protected override void OnActionExecuting( ActionExecutingContext ctx )
		{
			if ( ctx.ActionDescriptor.ActionName == "OrganizationFacilityHome" )
			{
				string CERSIDValue = ( ctx.ActionParameters["CERSID"] == null ) ? string.Empty : ctx.ActionParameters["CERSID"].ToString();
				ctx.Result = OrganizationFacilityHomeCERSCheck( CERSIDValue );
			}

			base.OnActionExecuting( ctx );
		}

		#region Ajax GridAction

		public ActionResult SelectFacility_GridRead( [DataSourceRequest]DataSourceRequest request, int organizationID, int? CERSID = null, string city = "", string street = "", string zipCode = "" )
		{
			var entities = Repository.Facilities.GridSearch( applySecurityFilter: true, securityFilterContext: Context.Organization, matrices: CurrentUserRoles, organizationID: organizationID, CERSID: CERSID, city: city, street: street, zipCode: zipCode );
			DataSourceResult result = entities.ToDataSourceResult( request );
			return Json( result, JsonRequestBehavior.AllowGet );
		}

		#endregion Ajax GridAction
	}
}