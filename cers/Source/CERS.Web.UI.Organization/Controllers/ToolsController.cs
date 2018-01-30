using CERS;
using CERS.Model;
using CERS.ViewModels.Chemicals;
using CERS.ViewModels.Facilities;
using CERS.ViewModels.Infrastructure;
using CERS.ViewModels.Organizations;
using CERS.ViewModels.Regulators;
using CERS.Web.Mvc;
using CERS.Web.UI.Organization.ViewModels;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using UPF;
using UPF.Web.Mvc;

namespace CERS.Web.UI.Organization.Controllers
{
	public class ToolsController : AppControllerBase
	{
		[Authorize]
		public ActionResult AddBusiness()
		{
			OrganizationViewModel viewModel = SystemViewModelData.BuildUpOrganizationViewModel();
			viewModel.Origins = viewModel.Origins.Where( p => p.ID == 6 );
			viewModel.Entity.OriginID = 6;
			viewModel.RegulatorsApprovingFacilityTransfer = Repository.Regulators.Search( applySecurityFilter: true, matrices: CurrentUserRoles );
			return View( viewModel );
		}

		[Authorize]
		[HttpPost]
		public ActionResult AddBusiness( FormCollection collection )
		{
			var orgVM = SystemViewModelData.BuildUpOrganizationViewModel();

			string orgName = collection["Entity.Name"].ToString();
			string headquarters = collection["Entity.Headquarters"].ToString();
			int? regulatorID = null;

			//Not setting the LeadRegulatorID, this is just the name of the Regulator Drop Down - using this regulatorID to create the event.
			if ( !String.IsNullOrEmpty( collection["Entity.LeadRegulatorID"].ToString() ) )
			{
				regulatorID = Convert.ToInt32( collection["Entity.LeadRegulatorID"].ToString() );
			}

			if ( ModelState.IsValid )
			{
				//Look for dupes
				List<Model.Organization> matches = ( from organization in Repository.DataModel.Organizations where organization.Name == orgName select organization ).ToList();

				if ( matches != null )
				{
					if ( matches.Count == 0 )
					{
						if ( TryUpdateModel( orgVM.Entity, "Entity" ) )
						{
							try
							{
								int orgID = CreateNewBusiness( orgVM.Entity.Name, orgVM.Entity.Headquarters, (OrganizationOrigin)orgVM.Entity.OriginID );

								Messages.Clear();
								Messages.Add( "Organization created successfully!", MessageType.Success, "Organizations" );
								return RedirectToAction( "Index", "MyOrganization", new { organizationId = orgID } );
							}
							catch ( Exception ex )
							{
								ModelState.AddModelError( "", ex.Message );
							}
						}
					}
					else
					{
						return RedirectToAction( "ExistingOrganizations", new { organizationName = orgName, headquarters = headquarters, regulatorID = regulatorID } );
					}
				}
			}
			return View( orgVM );
		}

		[Authorize]
		public ActionResult AddBusinessAnyway( string organizationName, string headquarters, string regulatorID )
		{
			var orgVM = SystemViewModelData.BuildUpOrganizationViewModel();

			try
			{
				int orgID = CreateNewBusiness( organizationName, headquarters, OrganizationOrigin.CUPA );
				Messages.Clear();
				Messages.Add( "Organization created successfully!", MessageType.Success, "Organizations" );
				return RedirectToAction( "Index", "MyOrganization", new { organizationId = orgID } );
			}
			catch ( Exception ex )
			{
				ModelState.AddModelError( "", ex.Message );
			}
			return View( orgVM );
		}

		[Authorize]
		public int CreateNewBusiness( string organizationName, string headquarters, OrganizationOrigin origin )
		{
			int result = 0;

			var org = Repository.Organizations.Create( organizationName, headquarters, origin );

			OrganizationContact orgContact = new OrganizationContact();

			Contact contact = Repository.DataModel.Contacts.SingleOrDefault( p => p.AccountID == CurrentAccount.ID );

			orgContact.Organization = org;
			orgContact.Contact = contact;
			Repository.OrganizationContacts.Save( orgContact );

			Repository.OrganizationContactPermissionGroups.Update( orgContact, "14" ); //OrgAdmin

			Services.Events.CreateOrganizationAdded( org, contact, CurrentAccount );

			CERSSecurityManager.ReloadCurrentUserRoles();

			result = org.ID;
			return result;
		}

		[Authorize]
		public ActionResult CreateNewOrganization()
		{
			OrganizationViewModel viewModel = SystemViewModelData.BuildUpOrganizationViewModel();
			viewModel.CheckDuplicates = true;
			return View( viewModel );
		}

		[Authorize]
		[HttpPost]
		public ActionResult CreateNewOrganization( OrganizationViewModel viewModel, FormCollection collection )
		{
			if ( string.IsNullOrWhiteSpace( viewModel.Entity.Name ) )
			{
				ModelState.AddModelError( "InvalidOrgName", "Organization name is required" );
			}

			if ( string.IsNullOrWhiteSpace( viewModel.Entity.Headquarters ) )
			{
				ModelState.AddModelError( "InvalidOrgHQ", "Organization headquarters is required" );
			}

			if ( string.IsNullOrWhiteSpace( viewModel.OrgContactPhone ) )
			{
				ModelState.AddModelError( "InvalidOrgContactPhone", "Organization contact phone is required" );
			}

			if ( string.IsNullOrWhiteSpace( viewModel.OrgContactTitle ) )
			{
				ModelState.AddModelError( "InvalidOrgContactTitle", "Organization contact title is required" );
			}

			if ( ModelState.IsValid )
			{
				try
				{
					viewModel.OrgContactPhone.FormatPhoneNumber();

					if ( viewModel.CheckDuplicates == true )
					{
						//Check for duplicate organization names.
						//(future): For now, we will just check for the first 20 characters of the Org name. We can change this check in future.
						var duplicateOrgs = Repository.Organizations.CheckDuplicates( viewModel.Entity.Name.TrimPro(), viewModel.Entity.Headquarters.TrimPro() );
						if ( duplicateOrgs.Count() > 0 )
						{
							var ogvList = duplicateOrgs.ToGridView().ToList();
							viewModel.OrganizationsGridView = SetOrganizationAccessFlag( ogvList );

							//Go to Duplicate Organization Check
							return View( "DuplicateOrganizationCheck", viewModel );
						}
					}

					//create the org.
					viewModel.Entity = Repository.Organizations.Create( viewModel.Entity.Name.TrimPro(), viewModel.Entity.Headquarters.Trim() );
					//Create Organization-Contact for this Org and user.
					Repository.OrganizationContacts.Create( CurrentAccount, viewModel.Entity.ID, viewModel.OrgContactTitle, viewModel.OrgContactPhone );
					//Set the permissions for the user as Admin and refresh the permissions in the session
					SetUserPermissions( viewModel.Entity.ID );

					//everything was successful. So, route to confirmation page.
					return View( "OrganizationAdded", viewModel );
				}
				catch
				{
					// Changed "catch (Exception ex)" to "catch" to suppress compilation warning; we do not do
	 // anything meaningful with "ex" in this scenario. If we need to do something meaningful with
	 // the Exception in the future, we can change it back.
					ModelState.AddModelError( "OrgCreationFailed", "Create Organization failed!" );
				}
			}
			return View( viewModel );
		}

		[Authorize]
		public ActionResult DraftsReplacedBySeeding()
		{
			FacilitySubmittalElementViewModel viewModel = new FacilitySubmittalElementViewModel();

			viewModel.FacilitySubmittalElementDraftsReplacedBySeeding = Repository.FacilitySubmittalElements.GetFSEWithDraftsReplacedBySeedingGridSearch( true, CurrentUserRoles );

			return View( viewModel );
		}

		[Authorize]
		public ActionResult ExistingOrganizations( string organizationName )
		{
			OrganizationViewModel viewModel = SystemViewModelData.BuildUpOrganizationViewModel();
			viewModel.Entity.OriginID = 6;
			viewModel.Entity.Name = organizationName;
			return View( viewModel );
		}

		[Authorize]
		public IEnumerable<RegulatorGridViewModel> GetRelatedRegulators( int regulatorID )
		{
			IEnumerable<RegulatorGridViewModel> relatedRegulators = ( from r in Repository.RegulatorRelationships.GetMappingsByCUPA( regulatorID )
																	  select new RegulatorGridViewModel
																	  {
																		  ID = r.Second.ID,
																		  Name = r.Second.Name,
																		  Type = r.Second.Type.TypeCode,
																		  RelationshipType = r.SecondType.TypeCode,
																		  City = r.Second.City,
																		  State = r.Second.State,
																		  Phone = r.Second.Phone,
																		  ZipCode = r.Second.ZipCode,
																		  PublicContactEmail = r.Second.PublicContactEmail,
																		  PublicContactUrl = r.Second.PublicContactUrl
																	  } ).Union
	 ( from r in Repository.RegulatorRelationships.GetMappingsByPA( regulatorID )
	   select new RegulatorGridViewModel
	   {
		   ID = r.First.ID,
		   Name = r.First.Name,
		   Type = r.First.Type.TypeCode,
		   RelationshipType = r.FirstType.TypeCode,
		   City = r.First.City,
		   State = r.First.State,
		   Phone = r.First.Phone,
		   ZipCode = r.First.ZipCode,
		   PublicContactEmail = r.First.PublicContactEmail,
		   PublicContactUrl = r.First.PublicContactUrl
	   } );
			return relatedRegulators;
		}

		[Authorize]
		public ActionResult Index( int? organizationId = null )
		{
			ViewBag.CurrentOrganizationID = organizationId.HasValue ? organizationId.Value : (int?)null;
			return View();
		}

		[Authorize]
		[HttpGet]
		public ActionResult OpenDocument( int documentID, string title )
		{
			Document document = Repository.Documents.GetByID( documentID );
			var docBytes = DocumentStorage.GetBytes( document.Location );
			string contentType = IOHelper.GetContentType( document.Location );
			string ext = System.IO.Path.GetExtension( document.Location ).ToLower();

			if ( docBytes == null )
			{
				return RedirectToAction( "DocumentNotFound", "Error" );
			}

			return File( docBytes, contentType, title + ext );
		}

		[Authorize]
		public ActionResult OrganizationAccessRequest( int id )
		{
			OrganizationViewModel viewModel = SystemViewModelData.BuildUpOrganizationViewModel( id );

			var oarTarget = Services.Events.GetOrganizationAccessRequestTarget( viewModel.Entity );
			viewModel.EvtTypeCode = oarTarget.Type;
			viewModel.Regulator = oarTarget.Regulator;
			viewModel.NotificationContacts = oarTarget.Contacts;

			return View( viewModel );
		}

		[HttpPost]
		public ActionResult OrganizationAccessRequestConfirmation( OrganizationViewModel viewModel, FormCollection collection, string phoneNumber, string jobTitle )
		{
			viewModel = SystemViewModelData.BuildUpOrganizationViewModel( viewModel.Entity.ID );

			//Get the contact for the current user
			Contact contact = Repository.OrganizationContacts.GetByAccount( CurrentAccountID );

			//Figures out who to send the "Request" to and delivers the emails too.
			var evnt = Services.Events.CreateOrganizationAccessRequest( viewModel.Entity, contact, phoneNumber, jobTitle );

			if ( evnt != null )
			{
				//There can be 3 possibilities. They are:
				//1. the ball is in the organizations regulator contacts (OrgUserARNotificiation) court to approve/deny request.
				//2. the ball is in the Organization Contact's (lead users) court to approve/deny request.
				//3. the ball is in the CERS Help Center's court to approve/deny request.
				viewModel.EvtTicketCode = evnt.TicketCode;
				viewModel.EvtTypeCode = evnt.GetEventTypeCode();
				viewModel.Regulator = evnt.Regulator;
				if ( evnt.Notifications.Count > 0 )
				{
					List<Contact> contacts = new List<Contact>();
					evnt.Notifications.ToList().ForEach( n => contacts.Add( n.Contact ) );
					viewModel.NotificationContacts = contacts;
				}
			}

			return View( viewModel );
		}

		[Authorize]
		public ActionResult OrganizationListing()
		{
            OrganizationViewModel viewModel = new OrganizationViewModel(); //SystemViewModelData.BuildUpOrganizationViewModel();
			//var ogvList = viewModel.Entities.ToGridView().ToList();
			//viewModel.OrganizationsGridView = SetOrganizationAccessFlag(ogvList);
			return View( viewModel );
		}

		[Authorize]
		public ActionResult RegulatorDetail( int regulatorId )
		{
			RegulatorViewModel viewModel = SystemViewModelData.BuildRegulatorViewModel( regulatorId );

			viewModel.RelatedRegulators = GetRelatedRegulators( regulatorId );
			return View( viewModel );
		}

		[Authorize]
		public ActionResult Regulators( bool export = false )
		{
			OrganizationToolsRegulatorsViewModel viewModel = new OrganizationToolsRegulatorsViewModel();
			viewModel.RegulatorGridView = Repository.Regulators.Search().ToGridView();

			if ( export )
			{
				ExportToExcel( "Regulators.xlsx", viewModel.RegulatorGridView );
			}

			return View( viewModel );
		}

		[Authorize]
		//[EntityFilterAuthorization(Context.Organization, "organizationId", PermissionRole.OrgEditor)]
		[HttpPost]
		public JsonResult RestoreDraftReplacedBySeeding( int FSEID )
		{
			bool result = false;

			var fse = Repository.FacilitySubmittalElements.GetByID( FSEID );
			if ( fse.FSEIDReplacedDuringSeeding.HasValue )
			{
				var frse = Repository.FacilityRegulatorSubmittalElements.GetByID( fse.CERSID, fse.SubmittalElementID );
				if ( frse != null )
				{
					var replacedFse = Repository.FacilitySubmittalElements.GetByID( fse.FSEIDReplacedDuringSeeding.Value );
					replacedFse.Voided = false;
					fse.Voided = true;
					frse.DraftFacilitySubmittalElementID = replacedFse.ID;
					Repository.FacilityRegulatorSubmittalElements.Update( frse );
					Repository.FacilitySubmittalElements.Update( fse );
					Repository.FacilitySubmittalElements.Update( replacedFse );

					result = true;
				}
			}

			return Json( result );
		}

        public ActionResult Search_Async( [DataSourceRequest]DataSourceRequest request, string name = "", string headquarters = "", int? statusID = null, int? regulatorID = null, int? CERSID = null, string facilityName = null, string facilityStreet = null, string facilityCity = null, string facilityZipCode = null, int? edtIdentityKey = null, OrganizationOrigin? origin = null, bool custSearch = false, StringSearchOption nameSearchOption = StringSearchOption.Contains, string nameEquals = null )
        {
            var organizations = Repository.Organizations.GridSearch(
                name:name,
                headquarters:headquarters,
                statusID:statusID,
                regulatorID:regulatorID,
                applySecurityFilter:false,
                CERSID:CERSID,
                facilityName:facilityName,
                facilityStreet:facilityStreet,
                facilityCity:facilityCity,
                facilityZipCode:facilityZipCode,
                edtIdentityKey:edtIdentityKey,
                origin:origin,
                nameSearchOption:nameSearchOption,
                nameEquals:nameEquals
                );

            DataSourceResult result = organizations.ToDataSourceResult( request );
            return Json( result, JsonRequestBehavior.AllowGet );

        }

		public ActionResult Search_GridBindingDraftsReplacedBySeeding([DataSourceRequest]DataSourceRequest request )
		{
            var facilitySubmittalElements = Repository.FacilitySubmittalElements.GetFSEWithDraftsReplacedBySeedingGridSearch( true, CurrentUserRoles );

            DataSourceResult result = facilitySubmittalElements.ToDataSourceResult( request );
            return Json( result, JsonRequestBehavior.AllowGet );
        }

        public ActionResult Search_GridBindingRelatedRegulators( [DataSourceRequest]DataSourceRequest request, int regulatorID )
		{
			var relatedRegulators = GetRelatedRegulators( regulatorID );

            DataSourceResult result = relatedRegulators.ToDataSourceResult( request );
            return Json( result, JsonRequestBehavior.AllowGet );
        }

        public ActionResult SearchOrganizations( [DataSourceRequest]DataSourceRequest request, int? CERSID, int? edtIdentityKey, string name = "", string businessName = "", string street = "", string city = "", string zipcode = "" )
		{

            var organizations = Repository.Organizations.SearchByFacility( CERSID, name, street, city, zipcode, businessName, statusID:(int)OrganizationStatus.Active, edtIdentityKey:edtIdentityKey, organizationType:(int)OrganizationType.Normal );
            var ogvList = SetOrganizationAccessFlag( organizations.ToList() );

            DataSourceResult result = ogvList.ToDataSourceResult( request );
            return Json( result, JsonRequestBehavior.AllowGet );
		}

		[Authorize]
		[HttpPost]
		public JsonResult WashAddress( Address address )
		{
			if ( Request.IsAjaxRequest() )
			{
				var washed = Services.Geo.GetAddressInformation( address.Street, address.City, address.ZipCode );
				if ( string.IsNullOrEmpty( washed.ErrorMessage ) )
				{
					address.IsWashed = true;
					address.WashedStreet = washed.Street;
					address.WashedCity = washed.City;
					address.WashedZipCode = washed.ZipCode;
					address.WashedCountyID = washed.CountyID.ToString();
				}
				else
				{
					address.IsWashed = false;
				}
				return Json( address );
			}
			else
			{
				throw new InvalidOperationException( "WashAddress should only be accessed via an AjaxRequest." );
			}
		}

		private List<OrganizationGridViewModel> SetOrganizationAccessFlag( List<OrganizationGridViewModel> orgs )
		{
			if ( orgs != null && orgs.Count > 0 )
			{
				foreach ( var o in orgs )
				{
					if ( CERS.Web.Mvc.CERSSecurityManager.CurrentUserRoles.HasRoles( o.ID, Context.Organization, PermissionRole.OrgViewer ) == true )
					{
						o.CanCurrentUserAccess = true;
					}
					else
					{
						o.CanCurrentUserAccess = false;
					}
				}
			}
			return orgs;
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

        #region ZipCode Default Mapping Search Tool

        public void ExportRegulatorZipCodeSubmittalElementMappingSummaryListing( int? zipCodeID = null, int? regulatorID = null, int? countyID = null )
        {
            RegulatorZipCodeSubmittalElementMappingSummaryViewModel viewModel = new RegulatorZipCodeSubmittalElementMappingSummaryViewModel();
            viewModel.Regulators = Repository.Regulators.Search();
            viewModel.Counties = Repository.Counties.Search();
            viewModel.RegulatorZipCodeSubmittalElementMappingSummaryGridView = Repository.RegulatorZipCodeSubmittalElements.SummaryGridSearch( zipCodeID: zipCodeID, regulatorID: regulatorID, countyID: countyID );

            ExportToExcel( "RegulatorZipCodeSubmittalElementMapping.xlsx", viewModel.RegulatorZipCodeSubmittalElementMappingSummaryGridView );
        }

        public ActionResult Search_GridBindingRegulatorZipCodeSubmittalElementMappingSummaries( [DataSourceRequest]DataSourceRequest request, int? zipCodeID = null, int? regulatorID = null, int? countyID = null )
        {
            var zipCodeSummaries = Repository.RegulatorZipCodeSubmittalElements.SummaryGridSearch( zipCodeID:zipCodeID, regulatorID:regulatorID, countyID:countyID );

            DataSourceResult result = zipCodeSummaries.ToDataSourceResult( request );
            return Json( result, JsonRequestBehavior.AllowGet );
        }

        public ActionResult ZipCodeMapping( int? zipCodeID = null, int? regulatorID = null, int? countyID = null, string searchFormPosted = "" )
        {
            RegulatorZipCodeSubmittalElementMappingSummaryViewModel viewModel = new RegulatorZipCodeSubmittalElementMappingSummaryViewModel();
            viewModel.Regulators = Repository.Regulators.Search();
            viewModel.Counties = Repository.Counties.Search();
            viewModel.ZipCodeID = zipCodeID;
            viewModel.RegulatorID = regulatorID;
            viewModel.CountyID = countyID;
            viewModel.SearchFormPosted = searchFormPosted;
            viewModel.RegulatorZipCodeSubmittalElementMappingSummaryGridView = new List<RegulatorZipCodeSubmittalElementMappingSummaryGridViewModel>().ToList();

            return View( viewModel );
        }

        [HttpPost]
        public ActionResult ZipCodeMapping( RegulatorZipCodeSubmittalElementMappingSummaryViewModel viewModel )
        {
            if ( viewModel == null )
            {
                viewModel = new RegulatorZipCodeSubmittalElementMappingSummaryViewModel();
            }
            viewModel.Regulators = Repository.Regulators.Search();
            viewModel.Counties = Repository.Counties.Search();
            viewModel.SearchFormPosted = "Y";
            viewModel.RegulatorZipCodeSubmittalElementMappingSummaryGridView = Repository.RegulatorZipCodeSubmittalElements.SummaryGridSearch( zipCodeID: viewModel.ZipCodeID, regulatorID: viewModel.RegulatorID, countyID: viewModel.CountyID );

            return View( viewModel );
        }

        #endregion ZipCode Default Mapping Search Tool
    }
}