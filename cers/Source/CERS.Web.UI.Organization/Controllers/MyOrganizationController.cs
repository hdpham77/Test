using CERS.Model;
using CERS.ViewModels;
using CERS.ViewModels.Contacts;
using CERS.ViewModels.Organizations;
using CERS.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UPF;
using UPF.Core.Model;
using Winnovative.ExcelLib;
using CERSOrg = CERS.Model.Organization;

namespace CERS.Web.UI.Organization.Controllers
{
	[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
	public class MyOrganizationController : AppControllerBase
	{
		public ActionResult AccessRequestProcess( int organizationId, int eventId )
		{
			AccessRequestProcessViewModel viewModel = new AccessRequestProcessViewModel();
			viewModel.Event = Services.Events.GetActionRequiredEventItem( eventId, CurrentAccount, Context.Organization );
			viewModel.Organization = Repository.Organizations.GetByID( organizationId );

			if ( viewModel.Event.ContactID != null )
			{
				viewModel.Relationships = Repository.Contacts.GetContactRelationships( viewModel.Event.ContactID.Value );
			}

			return View( viewModel );
		}

		public ActionResult Facilities( int organizationId )
		{
			CurrentOrganizationID = organizationId;
			OrganizationViewModel viewModel = new OrganizationViewModel
			{
				Entity = Repository.Organizations.GetByID( organizationId )
			};
			return View( viewModel );
		}

		#region index/summary

		public ActionResult Index( int organizationId )
		{
			CurrentOrganizationID = organizationId;
			var org = Repository.Organizations.GetByID( organizationId );
			OrganizationViewModel viewModel = new OrganizationViewModel
			{
				Entity = org,
				People = this.SystemViewModelData.BuildEntityContactGridViewModel( org ),
				LeadRegulator = Repository.Organizations.GetLeadRegulator( organizationId ),
			};
			return View( viewModel );
		}

		#endregion index/summary

		#region EmailHistory

		public ActionResult EmailHistory( int organizationId )
		{
			CurrentOrganizationID = organizationId;
			EmailViewModel<CERS.Model.Organization> viewModel = new EmailViewModel<CERS.Model.Organization>();

			//per Chris if default should be today - 90
			DateTime begin = DateTime.Now.AddDays( -90 );
			viewModel.Begin = begin;
			viewModel.Entity = Repository.Organizations.GetByID( organizationId );
            viewModel.ControllerName = "MyOrganization";
            viewModel.ActionMethodName = "EmailHistory_Grid";
            viewModel.GridReadData = "OrganizationEmailHistoryGrid_ReadData";

			return View( viewModel );
		}

        public JsonResult EmailHistory_Grid( [DataSourceRequest]DataSourceRequest request, int organizationId, DateTime? begin, DateTime? end, string to = null, int? CERSID = null, string facilityName = null )
        {
            var emails = Repository.Emails.GridSearch( organizationID: organizationId, begin: begin, end: end, to: to, CERSID: CERSID, facilityName: facilityName );

            return Json( emails.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

		#endregion EmailHistory

		#region Edit

		public ActionResult Edit( int organizationId )
		{
			CurrentOrganizationID = organizationId;
			OrganizationViewModel viewModel = new OrganizationViewModel
			{
				Entity = Repository.Organizations.GetByID( organizationId ),
				LeadRegulator = Repository.Organizations.GetLeadRegulator( organizationId ),
			};
			return View( viewModel );
		}

		[HttpPost]
		public ActionResult Edit_Async( int organizationId, string name, string headquarters )
		{
			//Validate edits.
			bool isValid = true;
			if ( string.IsNullOrWhiteSpace( name ) )
			{
				isValid = false;
			}
			if ( string.IsNullOrWhiteSpace( headquarters ) )
			{
				isValid = false;
			}

			if ( isValid )
			{
				//Get the Organization
				CurrentOrganizationID = organizationId;
				var entity = Repository.Organizations.GetByID( organizationId );

				//retaing the original values for Event message.
				string originalName = entity.Name;
				string originalHQ = entity.Headquarters;

				//Save Entity
				entity.Name = name;
				entity.Headquarters = headquarters;
				Repository.Organizations.Save( entity );

				//Create events/notifications for changed name/headquarters
				if ( originalName != name )
				{
					string originalOrgMetadata = originalName + " (" + originalHQ + ")";
					Services.Events.CreateBusinessUserChangedBusinessName( CurrentOrganization, originalOrgMetadata );
				}
				if ( originalHQ != headquarters )
				{
					string originalOrgMetadata = originalName + " (" + originalHQ + ")";
					Services.Events.CreateBusinessUserChangedBusinessHeadquarters( CurrentOrganization, originalOrgMetadata );
				}
			}

			var result = new
			{
				Success = isValid,
				NewName = name,
				NewHeadquaters = headquarters,
				ErrorMessage = ""
			};

			return Json( result );
		}

		#endregion Edit

		#region ActionRequired

		public ActionResult ActionRequired( int organizationId )
		{
			CurrentOrganizationID = organizationId;
			OrganizationViewModel viewModel = new OrganizationViewModel
			{
				Entity = Repository.Organizations.GetByID( organizationId )
			};

			return View( viewModel );
		}

		#endregion ActionRequired

		#region Notifications

		public ActionResult Notifications( int organizationId )
		{
			CurrentOrganizationID = organizationId;
			OrganizationViewModel viewModel = new OrganizationViewModel
			{
				Entity = Repository.Organizations.GetByID( organizationId )
			};

			return View( viewModel );
		}

		#endregion Notifications

		#region Manage Facilities

		public ActionResult DeleteFacility( int organizationId, int CERSID )
		{
			CurrentOrganizationID = organizationId;
			OrganizationViewModel viewModel = new OrganizationViewModel
			{
				Entity = Repository.Organizations.GetByID( organizationId )
			};
			return View( viewModel );
		}

		[CheckDeferredProcessing]
		public ActionResult ManageFacilities( int organizationId )
		{
			CurrentOrganizationID = organizationId;
			OrganizationViewModel viewModel = new OrganizationViewModel
			{
				CurrentOrganizationID = organizationId,
				Entity = Repository.Organizations.GetByID( organizationId )
			};
			return View( viewModel );
		}

		#region Merge

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgAdmin )]
		public ActionResult FacilityMergeRequestConfirm( int organizationId, int sourceCERSID, int targetCERSID )
		{
			OrganizationViewModel viewModel = SystemViewModelData.BuildUpOrganizationViewModel( organizationId );
			viewModel.SourceCERSID = sourceCERSID;
			viewModel.TargetCERSID = targetCERSID;
			viewModel.SourceFacilityName = viewModel.Entity.Facilities.Where( p => p.ID == sourceCERSID ).Single().Name;
			viewModel.TargetFacilityName = viewModel.Entity.Facilities.Where( p => p.ID == targetCERSID ).Single().Name;

			//To whom the request will be sent to? [Org Admins and Regulators]
			var facilityMergeRequestTarget = Services.Events.GetFacilityMergeRequestTarget( targetCERSID );
			if ( facilityMergeRequestTarget != null )
			{
				viewModel.NotificationContacts = facilityMergeRequestTarget.Contacts;
				viewModel.Regulator = facilityMergeRequestTarget.Regulator;
			}

			return View( viewModel );
		}

		[HttpPost]
		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgAdmin )]
		[CheckDeferredProcessing]
		public ActionResult FacilityMergeRequestConfirm( int organizationId, OrganizationViewModel viewModel, FormCollection collection )
		{
			if ( string.IsNullOrWhiteSpace( viewModel.ManageFacilityComments ) )
			{
				ModelState.AddModelError( "CommentsEmpty", "Comments are required." );
			}

			if ( ModelState.IsValid )
			{
				//Raise the event - to save the request.
				var sourceFacility = Repository.Facilities.GetByID( viewModel.SourceCERSID );
				var targetFacility = Repository.Facilities.GetByID( viewModel.TargetCERSID );
				var organization = Repository.Organizations.GetByID( organizationId );
				Contact contact = Repository.OrganizationContacts.GetByAccount( CurrentAccount );

				var evt = Services.Events.CreateFacilityMergeRequest( sourceFacility, targetFacility, organization, contact, viewModel.ManageFacilityComments );
				if ( evt != null )
				{
					viewModel.EvtTicketCode = evt.TicketCode;
					viewModel.EvtTypeCode = evt.GetEventTypeCode();
				}

                viewModel.Name = organization.Name;
                viewModel.CurrentOrganizationID = organization.ID;

				//Return confirmation page.
				return View( "FacilityMergeRequestConfirmation", viewModel );
			}

			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgAdmin )]
		public ActionResult FacilityMergeRequestSource( int organizationId )
		{
			OrganizationViewModel viewModel = SystemViewModelData.BuildUpOrganizationViewModel( organizationId );
			//TODO: Need to check to see whether the source facility has CME related stuff, if so, it cannot be messed with.
			viewModel.FacilitiesGridView = Repository.Facilities.GridSearch( organizationID: organizationId );

			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgAdmin )]
		public ActionResult FacilityMergeRequestTarget( int organizationId, int sourceCERSID )
		{
			OrganizationViewModel viewModel = SystemViewModelData.BuildUpOrganizationViewModel( organizationId );
			if ( viewModel.Entity != null )
			{
				viewModel.FacilitiesGridView = Repository.Facilities.GridSearch( organizationID: viewModel.Entity.ID );
				var sourceFacility = viewModel.FacilitiesGridView.Where( fv => fv.CERSID == sourceCERSID ).SingleOrDefault();
				if ( sourceFacility != null )
				{
					viewModel.RegulatorID = sourceFacility.RegulatorID;
				}
			}
			viewModel.SourceCERSID = sourceCERSID;
			return View( viewModel );
		}

		#endregion Merge

		#region Transfer

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgAdmin )]
		public ActionResult FacilityTransferRequestConfirm( int organizationId, int sourceCERSID, int targetOrganizationID )
		{
			OrganizationViewModel viewModel = SystemViewModelData.BuildUpOrganizationViewModel( organizationId );

			//set the Organization to which the facility will be transferred.
			var targetOrg = Repository.Organizations.GetByID( targetOrganizationID );
			viewModel.TargetOrganizationID = targetOrg.ID;
			viewModel.TargetOrganizationName = targetOrg.Name;

			var sourceFacility = Repository.Facilities.GetByID( sourceCERSID );
			viewModel.SourceCERSID = sourceFacility.CERSID;
			viewModel.SourceFacilityName = sourceFacility.Name;

			//To whom the notifications will be sent to? [Org Admins ONLY]
			var facilityDeleteRequestTarget = Services.Events.GetFacilityTransferRequestTarget( viewModel.Entity, targetOrg );
			if ( facilityDeleteRequestTarget != null )
			{
				viewModel.NotificationContacts = facilityDeleteRequestTarget.Contacts;
				viewModel.Regulator = facilityDeleteRequestTarget.Regulator;
			}

			return View( viewModel );
		}

		[HttpPost]
		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgAdmin )]
		[CheckDeferredProcessing]
		public ActionResult FacilityTransferRequestConfirm( int organizationId, OrganizationViewModel viewModel, FormCollection collection )
		{
			if ( string.IsNullOrWhiteSpace( viewModel.ManageFacilityComments ) )
			{
				ModelState.AddModelError( "CommentsEmpty", "Comments are required." );
			}

			if ( ModelState.IsValid )
			{
				//Raise the event - to save the request.
				var facility = Repository.Facilities.GetByID( viewModel.SourceCERSID );
				var sourceOrganization = Repository.Organizations.GetByID( organizationId );
				var targetOrganization = Repository.Organizations.GetByID( viewModel.TargetOrganizationID );
				Contact contact = Repository.OrganizationContacts.GetByAccount( CurrentAccount );

				var evt = Services.Events.CreateFacilityTransferRequest( facility, sourceOrganization, targetOrganization, contact, viewModel.ManageFacilityComments );
				if ( evt != null )
				{
					viewModel.EvtTicketCode = evt.TicketCode;
					viewModel.EvtTypeCode = evt.GetEventTypeCode();
				}

				//Return confirmation page.
				return View( "FacilityTransferRequestConfirmation", viewModel );
			}

			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgAdmin )]
		public ActionResult FacilityTransferRequestSource( int organizationId )
		{
			CurrentOrganizationID = organizationId;
			OrganizationViewModel viewModel = SystemViewModelData.BuildUpOrganizationViewModel( organizationId );
			if ( viewModel.Entity != null )
			{
				viewModel.FacilitiesGridView = Repository.Facilities.GridSearch( organizationID: viewModel.Entity.ID );
			}
			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgAdmin )]
		public ActionResult FacilityTransferRequestTarget( int organizationId, int sourceCERSID )
		{
			OrganizationViewModel viewModel = SystemViewModelData.BuildUpOrganizationViewModel( organizationId );

			//Get the organizations with which this user is connected.
			viewModel.Entities = Repository.OrganizationContacts.GetOrganizations( CurrentAccountID );

			//set the facility to be transferred.
			viewModel.SourceCERSID = sourceCERSID;

			return View( viewModel );
		}

		#endregion Transfer

		#region Delete

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgAdmin )]
		public ActionResult FacilityDeleteRequestConfirm( int organizationId, int targetCERSID )
		{
			OrganizationViewModel viewModel = SystemViewModelData.BuildUpOrganizationViewModel( organizationId );
			viewModel.TargetCERSID = targetCERSID;
			viewModel.TargetFacilityName = viewModel.Entity.Facilities.Where( p => p.ID == targetCERSID ).Single().Name;

			//To whom the request will be sent to? [Org Admins and Regulators]
			var facilityDeleteRequestTarget = Services.Events.GetFacilityDeleteRequestTarget( targetCERSID );
			if ( facilityDeleteRequestTarget != null )
			{
				viewModel.NotificationContacts = facilityDeleteRequestTarget.Contacts;
				viewModel.Regulator = facilityDeleteRequestTarget.Regulator;
			}

			return View( viewModel );
		}

		[HttpPost]
		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgAdmin )]
		[CheckDeferredProcessing]
		public ActionResult FacilityDeleteRequestConfirm( int organizationId, OrganizationViewModel viewModel, FormCollection collection )
		{
			if ( string.IsNullOrWhiteSpace( viewModel.ManageFacilityComments ) )
			{
				ModelState.AddModelError( "CommentsEmpty", "Comments are required." );
			}

			if ( ModelState.IsValid )
			{
				//Raise the event - to save the request.
				var facility = Repository.Facilities.GetByID( viewModel.TargetCERSID );
				var organization = Repository.Organizations.GetByID( organizationId );
				Contact contact = Repository.OrganizationContacts.GetByAccount( CurrentAccount );

				var evt = Services.Events.CreateFacilityDeleteRequest( facility, organization, contact, viewModel.ManageFacilityComments );
				if ( evt != null )
				{
					viewModel.EvtTicketCode = evt.TicketCode;
					viewModel.EvtTypeCode = evt.GetEventTypeCode();
				}

				//Return confirmation page.
				return View( "FacilityDeleteRequestConfirmation", viewModel );
			}

			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgAdmin )]
		public ActionResult FacilityDeleteRequestTarget( int organizationId )
		{
			OrganizationViewModel viewModel = SystemViewModelData.BuildUpOrganizationViewModel( organizationId );
			viewModel.FacilitiesGridView = Repository.Facilities.GetFacilitiesWithoutAnySubmissionsEnforcementsInspections( organizationId ).ToGridView();

			return View( viewModel );
		}

		#endregion Delete

		#endregion Manage Facilities

		#region People

		public ActionResult People( int organizationId, bool export = false )
		{
			var org = Repository.Organizations.GetByID( organizationId );
			var people = this.SystemViewModelData.BuildEntityContactGridViewModel( org );

			//var people = AdvancedEntityContactGridView.GetForOrganization(org, Repository);

			if ( export )
			{
				ExportToExcel( org.Name + "_People.xlsx", people.GridView );
			}

			OrganizationViewModel viewModel = new OrganizationViewModel
			{
				Entity = org,
				People = people
			};
			return View( viewModel );
		}

		[HttpPost]
		public ActionResult People( int organizationId, EntityContactGridViewModel contactViewModel, FormCollection collection, bool export = false )
		{
			var org = Repository.Organizations.GetByID( organizationId );
			var people = this.SystemViewModelData.BuildEntityContactGridViewModel( org, contactViewModel );

			if ( export )
			{
				ExportToExcel( org.Name + "_People.xlsx", people.GridView );
			}

			OrganizationViewModel viewModel = new OrganizationViewModel
			{
				Entity = org,
				People = people
			};
			return View( viewModel );
		}

		#region AddPerson Methods

		public ActionResult AddPerson( int organizationId )
		{
			CurrentOrganizationID = organizationId;
			var viewModel = SystemViewModelData.BuildOrganizationContactViewModel( organizationId );
			return View( viewModel.ViewName, viewModel );
		}

		[HttpPost]
		public ActionResult AddPerson( int organizationId, EntityContactViewModel<CERSOrg, OrganizationContact> viewModel, FormCollection collection )
		{
			CurrentOrganizationID = organizationId;
			viewModel = SystemViewModelData.BuildOrganizationContactViewModel( organizationId, viewModel: viewModel );
			if ( viewModel.WizardStep == AddEntityContactWizardStep.EnterEmail )
			{
				if ( string.IsNullOrWhiteSpace( viewModel.Email ) )
				{
					ModelState.AddModelError( "Email", "Email is required." );
				}

				if ( string.IsNullOrWhiteSpace( viewModel.ConfirmEmail ) )
				{
					ModelState.AddModelError( "ConfirmEmail", "Confirm Email is required." );
				}

				if ( !string.IsNullOrWhiteSpace( viewModel.Email ) && !string.IsNullOrWhiteSpace( viewModel.ConfirmEmail ) )
				{
					if ( string.Compare( viewModel.Email, viewModel.ConfirmEmail, true ) != 0 )
					{
						ModelState.AddModelError( "ConfirmEmail", "The email addresses do not match!" );
					}
				}

				if ( ModelState.IsValid )
				{
					//now, lets figure out what view to go to next based on what is in the viewmodel.
					if ( viewModel.ContactID != null && viewModel.EntityContactID != null )
					{
						//we already have a contact and organization contact that is bound for this email
						return RedirectToRoute( OrganizationManage.PersonEdit, new { id = organizationId, pid = viewModel.EntityContactID } );
					}
					else if ( viewModel.ContactID != null && viewModel.EntityContactID == null )
					{
						//we already have a contact bound to this email, but still need to associate a organization contact.
						viewModel.WizardStep = AddEntityContactWizardStep.ExistingContact;
					}
					else if ( viewModel.ContactID == null && viewModel.EntityContactID == null )
					{
						//we can't find a Contact or a organization contact, so give the view that allows them to assign.
						viewModel.WizardStep = AddEntityContactWizardStep.NewContact;
					}
				}
			}
			else if ( viewModel.WizardStep == AddEntityContactWizardStep.ExistingContact )
			{
				//lets see if we have a new organization Contact stuffing.
				if ( ModelState.IsValid )
				{
					var contact = viewModel.Contact;
					if ( !viewModel.IsContactShared )
					{
						TryUpdateModel( contact, "Contact" );
					}

					var orgContact = viewModel.EntityContact;
					if ( orgContact == null )
					{
						orgContact = new OrganizationContact();
					}
					TryUpdateModel( orgContact, "EntityContact" );
					try
					{
						orgContact.Organization = viewModel.Entity;
						orgContact.Contact = viewModel.Contact;
						Repository.OrganizationContacts.Save( orgContact );
						return RedirectToRoute( OrganizationManage.PersonEdit, new { organizationId = organizationId, pid = orgContact.ID } );
					}
					catch ( Exception ex )
					{
						ModelState.AddModelError( "", ex.Message );
					}
				}
			}
			else if ( viewModel.WizardStep == AddEntityContactWizardStep.NewContact )
			{
				if ( ModelState.IsValid )
				{
					try
					{
						var contact = viewModel.Contact;
						var orgContact = viewModel.EntityContact;

						TryUpdateModel( contact, "Contact" );
						TryUpdateModel( orgContact, "EntityContact" );
						orgContact.OrganizationID = organizationId;
						orgContact.Contact = contact;
						contact.SetCommonFields( CurrentAccountID, creating: true );

						Repository.OrganizationContacts.Save( orgContact );
						return RedirectToRoute( OrganizationManage.PersonEdit, new { organizationId = organizationId, pid = orgContact.ID } );
					}
					catch ( Exception ex )
					{
						ModelState.AddModelError( "", ex.Message );
					}
				}
			}

			return View( viewModel.ViewName, viewModel );
		}

		#endregion AddPerson Methods

		#region EditContactInfo Method (AJAX)

		[HttpPost]
        public ActionResult EditContactInfo( int organizationId, int cid, string ChangeContact_FirstName, string ChangeContact_LastName, string ChangeContact_Email )
		{
            CurrentOrganizationID = organizationId;
			var result = new NameChangeResultViewModel();

			List<string> errors = new List<string>();
			if ( string.IsNullOrWhiteSpace( ChangeContact_FirstName ) )
			{
				errors.Add( "First Name must have a value." );
			}

			if ( string.IsNullOrWhiteSpace( ChangeContact_LastName ) )
			{
				errors.Add( "Last Name must have a value." );
			}

			if ( string.IsNullOrWhiteSpace( ChangeContact_Email ) )
			{
				errors.Add( "Email must have a value." );
			}

			//first thing we need to do is get the contact.
			Contact contact = Repository.Contacts.GetByID( cid );
			if ( contact == null )
			{
				errors.Add( "Contact could not be found." );
			}

			Account account = null;
			if ( contact.AccountID != null )
			{
				account = CoreRepository.Accounts.GetByID( contact.AccountID.Value );
			}

			if ( errors.Count == 0 )
			{
				//lets make sure this email is not already in use for another contact.
				Contact existingContact = Repository.Contacts.GetByEmail( ChangeContact_Email );
				if ( existingContact != null )
				{
					//a contact exists with this email, but it might be this contact, so lets see if they share the same ID.
					if ( existingContact.ID != contact.ID )
					{
						//email is already in use by another contact, cannot proceed.
						errors.Add( "This email address is already being used for another person." );
					}
				}

				if ( errors.Count == 0 )
				{
					//now lets make sure this email is not already in use for another account.
					var existingAccount = CoreRepository.Accounts.GetByEmail( ChangeContact_Email );
					if ( existingAccount != null )
					{
						//we have an existing account with this email...

						//if the current contact is associated with an account, lets see if the current account matches the one we found.
						if ( account != null )
						{
							//this contact is linked to an account, so lets make sure the existingAccount and account are the same.
							if ( account.ID != existingAccount.ID )
							{
								//they are not the same.
								errors.Add( "This email address is already in use for an existing account." );
							}
						}
						else
						{
							//so we found an account that shares the new target email, but it's not associated with this account, and so we cannot make these changes.
							errors.Add( "This email address is already in use for an existing account." );
						}
					}
				}

				if ( errors.Count == 0 )
				{
					// check if first and last name are the same
					if ( ChangeContact_LastName.Trim().Equals( ChangeContact_FirstName.Trim(), StringComparison.InvariantCultureIgnoreCase ) )
					{
						errors.Add( "First Name and Last Name cannot be identical." );
					}
				}

				if ( errors.Count == 0 )
				{
					//ok, looks like we can proceed!
					contact.FirstName = ChangeContact_FirstName.Trim();
					contact.LastName = ChangeContact_LastName.Trim();
					contact.Email = ChangeContact_Email.Trim();
					Repository.Contacts.Save( contact );

					if ( account != null )
					{
						account.FirstName = ChangeContact_FirstName.Trim();
						account.LastName = ChangeContact_LastName.Trim();
						account.Email = ChangeContact_Email.Trim();
						CoreRepository.Accounts.Save( account );
					}

					result.New_FullName = contact.FullName;
					result.New_Email = contact.Email;
					result.Message = "Person updated successfully!";
				}
			}

			if ( errors.Count > 0 )
			{
				result.Result = false;
				result.Message = errors.ToDelimitedString( "<br/>" );
			}
			else
			{
				result.Result = true;
			}
			return Json( result );
		}

		#endregion EditContactInfo Method (AJAX)

		#region EditPerson

		public ActionResult EditPerson( int organizationId, int pid )
		{
			CurrentOrganizationID = organizationId;
			var viewModel = SystemViewModelData.BuildOrganizationContactViewModel( organizationId, pid );
			return View( viewModel );
		}

		[HttpPost]
		public ActionResult EditPerson( int organizationId, int pid, EntityContactViewModel<CERSOrg, OrganizationContact> viewModel, FormCollection collection )
		{
			CurrentOrganizationID = organizationId;
			viewModel = SystemViewModelData.BuildOrganizationContactViewModel( organizationId, pid, viewModel: viewModel );
			var contact = viewModel.Contact;
			if ( !viewModel.IsContactShared )
			{
				TryUpdateModel( contact, "Contact" );
			}

			TryUpdateModel( viewModel.EntityContact, "EntityContact" );
			if ( ModelState.IsValid )
			{
				try
				{
                    string oldPermissions = "No Permission";
                    var contactRelationships = Repository.Contacts.GetContactRelationships( contact ).Where( cr => cr.Context == Context.Organization && cr.EntityID == organizationId );
                    if ( contactRelationships != null )
                    {
                        oldPermissions = contactRelationships.First().PermissionsDisplay;
                        if ( String.IsNullOrWhiteSpace( oldPermissions ) )
                        {
                            oldPermissions = "No Permission";
                        }
                    }

					string permissions = collection["PG"];
					Repository.OrganizationContacts.Save( viewModel.EntityContact );
					Repository.OrganizationContactPermissionGroups.Update( viewModel.EntityContact, permissions );
					Repository.Organizations.Save( viewModel.Entity );

                    string newPermissions = "No Permission";
                    var newContactRelationships = Repository.Contacts.GetContactRelationships( contact ).Where( cr => cr.Context == Context.Organization && cr.EntityID == organizationId );
                    if ( newContactRelationships != null )
                    {
                        newPermissions = newContactRelationships.First().PermissionsDisplay;
                        if ( String.IsNullOrWhiteSpace( newPermissions ) )
                        {
                            newPermissions = "No Permission";
                        }
                    }

                    if ( newPermissions != oldPermissions )
                    {
                        //if permission was changed create an Event for this
                        Services.Events.CreateUserPermissionChanged( contact, Context.Organization, viewModel.Entity.LeadRegulator, viewModel.Entity, CurrentAccount, oldPermissions, newPermissions );
                    }

					if ( !string.IsNullOrWhiteSpace( permissions ) && viewModel.AccountID == null )
					{
						//there is no account, but yet, some permissions were established.
						//so lets spin up an Event for this.
						Services.Events.CreateOrganizationInvitation( viewModel.Entity, viewModel.EntityContact, viewModel.ResendInvitation );
					}

					Messages.Add( "Person updated successfully!", UPF.Web.Mvc.MessageType.Success, "Person" );
					return RedirectToRoute( OrganizationManage.People, new { organizationId = organizationId } );
				}
				catch ( Exception ex )
				{
					ModelState.AddModelError( "", ex.Message );
				}
			}
			return View( viewModel );
		}

		#endregion EditPerson

		#region PersonDetails

		public ActionResult PersonDetails( int organizationId, int pid )
		{
			CurrentOrganizationID = organizationId;
			var viewModel = SystemViewModelData.BuildOrganizationContactViewModel( organizationId, pid );
			return View( viewModel );
		}

		#endregion PersonDetails

		#region Delete Person

		public ActionResult DeletePerson( int organizationId, int pid )
		{
			CurrentOrganizationID = organizationId;
			OrganizationContact orgContact = Repository.OrganizationContacts.GetByID( pid );

			try
			{
				Repository.OrganizationContacts.Delete( orgContact );
				Messages.Clear();
				Messages.Add( "The Contact has been successfully Deleted!", UPF.Web.Mvc.MessageType.Success );
			}
			catch ( Exception ex )
			{
				ModelState.AddModelError( "", ex.Format() );
			}

			return RedirectToRoute( OrganizationManage.People, new { organizationId = organizationId } );
		}

		#endregion Delete Person

		#endregion People

		#region ArchivedFacilities Method

		public ActionResult ArchivedFacilities( int organizationId )
		{
			return RedirectToRoute( OrganizationManage.Archives, organizationId );
		}

		public ActionResult Archives( int organizationId )
		{
			var org = Repository.Organizations.GetByID( organizationId );
			OrganizationViewModel viewModel = new OrganizationViewModel
			{
				Entity = org,
			};

			var archivedSubmittals = Repository.FacilitySubmittalElements.GetArchivedSubmittals( organizationId );

			//int count = archivedSubmittals.Count();
			var submittalEvents = archivedSubmittals.Where( p => p.StatusID != (int)SubmittalElementStatus.Draft ).GetSubmittalEvent( Repository ).OrderByDescending( o => o.SubmittedDate );
			viewModel.ArchivedSubmittalEvents = submittalEvents;
			viewModel.ArchivedFacilities = Repository.Organizations.GetArchivedFacilitiesGridView( organizationId );
			return View( viewModel );
		}

		#endregion ArchivedFacilities Method

		#region Upload Shared Methods

		[HttpPost]
		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult DeleteQueuedDeferredProcessing( int organizationID, string organizationName )
		{
			var message = string.Empty;
			var result = true;

			var deferredProcessingItem = Repository.DeferredProcessingUpload.GetUnprocessedItemByOrganizationID( organizationID );
			if ( deferredProcessingItem == null )
			{
				message = String.Format( "Cannot find any deferred processing for <strong>{0}</strong>", organizationName );
			}
			else
			{
				//check again one more time if the queued item already starts
				if ( deferredProcessingItem.StatusID == (int)DeferredProcessingItemStatus.InProgress )
				{
					message = String.Format( "Cannot delete, the queued item for <strong>{0}</strong><br />already in progress!", organizationName );
					result = false;
				}
				else
				{
					try
					{
                        deferredProcessingItem.StatusID = (int)DeferredProcessingItemStatus.Cancelled;
						deferredProcessingItem.Voided = true;
						Repository.DeferredProcessingUpload.Save( deferredProcessingItem );
						message = String.Format( "Successfully deleting deferred processing<br />{0} for <strong>{1}</strong>", deferredProcessingItem.SubmittalElement.Name, organizationName );
					}
					catch ( Exception ex )
					{
						message = "An error occured!<br /><small>" + ex.Message + "</small>";
						result = false;
					}
				}
			}

			return Json( new { success = result, message = message } );
		}

		public void ExportGuidanceMessagesToExcel()
		{
			List<GuidanceMessage> inventoryUploadGuidanceMessages = new List<GuidanceMessage>();
			if ( Session["GuidanceMessages"] != null )
			{
				inventoryUploadGuidanceMessages = (List<GuidanceMessage>)Session["GuidanceMessages"];
			}

			ExcelColumnMapping[] columns = new ExcelColumnMapping[] { new ExcelColumnMapping( "Message", "Error Message" ) };

			ExportToExcel( "GuidanceMessages.xlsx", inventoryUploadGuidanceMessages, columns );
		}

		private void CheckIsFileValid( HttpPostedFileBase file, List<GuidanceMessage> guidanceMessages, LUTGuidanceLevel lutGuidanceLevel )
		{
			if ( file == null || file.ContentLength == 0 )
			{
				Repository.GuidanceMessages.AddGuidanceMessage( guidanceMessages, lutGuidanceLevel, "Uploaded worksheet was not specified, or is an empty file." );
			}
            if ( file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" )
            {
                Repository.GuidanceMessages.AddGuidanceMessage( guidanceMessages, lutGuidanceLevel, "Uploaded worksheet is not a valid excel worksheet." );
            }
        }

		private void UploadFile( int organizationId, HttpPostedFileBase file, SubmittalElementType submittalElementType, List<GuidanceMessage> guidanceMessages, LUTGuidanceLevel lutGuidanceLevel, int guidanceMessageTruncationLimit = 25 )
		{
			// Track row of being processed (this is updated dynamically if the first row of data is not the
   // second row)
			int rowDataBegins = 2;
			int rowIndex = 0;

			// Perform a larger try/catch around the uploaded file processing. If any unforseen exception is
   // encountered, add a more generic error message to the guidanceMessages property
			try
			{
				CheckIsFileValid( file, guidanceMessages, lutGuidanceLevel );

				if ( guidanceMessages.Count == 0 )
				{
					#region XLS Clean Header

                    //Need to convert to Stream to handle both xls & xlsx format (2003 & 2007)
                    byte[] tempArray = new byte[file.InputStream.Length];
                    file.InputStream.Read( tempArray, 0, (int)file.InputStream.Length );
                    System.IO.Stream sourceXlsDataStream = new System.IO.MemoryStream( tempArray );
                    ExcelWorkbook uploadedWorkbook = new ExcelWorkbook( sourceXlsDataStream );

					// Load Uploaded Excel Workbook, and open first Worksheet
					uploadedWorkbook.LicenseKey = Excel.GetWinnovativeExcelLicenseKey();
					ExcelWorksheet uploadedWorksheet = uploadedWorkbook.Worksheets[0];

					// Load entire first worksheet into Data Table, to determine location of first real data row
					DataTable uploadedDataTable = uploadedWorksheet.GetDataTable( uploadedWorksheet.UsedRange, true );
					int columnHeaderRowIndex = 0;
					string cellValue = "";

					// If first column is not CERSID, then we do
					if ( !uploadedDataTable.Columns[0].ColumnName.Equals( "CERSID" ) )
					{
						// Determine location of CERSID Column
						foreach ( DataRow row in uploadedDataTable.Rows )
						{
							columnHeaderRowIndex++;
							cellValue = row[0].ToString();
							if ( cellValue != null && cellValue.ToUpper().Equals( "CERSID" ) )
							{
								break;
							}
						}

						// Delete Unnecessary rows, leaving only the header row and subsequent data
						for ( int i = 0; i < columnHeaderRowIndex; i++ )
						{
							uploadedWorksheet.DeleteRow( 1 );
							rowDataBegins++;
						}
					}

					#endregion XLS Clean Header

					// Reload the DataTable, this time without surplus header rows -
					uploadedDataTable = uploadedWorksheet.GetDataTable( uploadedWorksheet.UsedRange, true );
					var cols = uploadedDataTable.Columns;
					int numberOfDataRows = uploadedDataTable.Rows.Count - rowDataBegins + 2;

					if ( numberOfDataRows <= 0 )
					{
						Repository.GuidanceMessages.AddGuidanceMessage( guidanceMessages, lutGuidanceLevel, "Uploaded worksheet has no record." );
					}
					else
					{
						#region FacInfo

						if ( submittalElementType == SubmittalElementType.FacilityInformation )
						{
							// Validate that the uploaded spreadsheet contains the required columns
							FacInfoCheckIsFormatValid( cols, guidanceMessages, lutGuidanceLevel );

							// Only proceed if no Guidance Messages were thrown
							if ( guidanceMessages.Count == 0 )
							{
								CERS.ModelValidationExtensionMethods.FacInfoValidate( uploadedDataTable, rowDataBegins, guidanceMessages, lutGuidanceLevel, guidanceMessageTruncationLimit, organizationId );
							}
						}

						#endregion FacInfo

						#region HazardousMaterialsInventory

						if ( submittalElementType == SubmittalElementType.HazardousMaterialsInventory )
						{
							// Validate that the uploaded spreadsheet contains the required columns
							HMICheckIsFormatValid( cols, guidanceMessages, lutGuidanceLevel );

							// Only proceed if no Guidance Messages were thrown
							if ( guidanceMessages.Count == 0 )
							{
								CERS.ModelValidationExtensionMethods.HMIValidate( uploadedDataTable, rowDataBegins, guidanceMessages, lutGuidanceLevel, guidanceMessageTruncationLimit, organizationId, CERSID: null );
							}
						}

						#endregion HazardousMaterialsInventory
					}
					// Only save file if no guidance messages were produced
					if ( guidanceMessages.Count == 0 )
					{
						#region Save Uploaded File

						//save the file for deferred processing
						Document document = null;
						try
						{
							document = Repository.Documents.Save( file.InputStream, file.FileName, DocumentContext.Organization, organizationId );
							DeferredProcessingUpload deferredProcessingItem = new DeferredProcessingUpload
							{
								OrganizationID = organizationId,
								SubmittalElementID = (int)submittalElementType,
								DocumentID = document.ID,
								BatchTypeID = submittalElementType == SubmittalElementType.FacilityInformation ? (int)DeferredProcessingBatchType.OwnerOperator : (int)DeferredProcessingBatchType.MultiFacilityHMI,
								NumberOfDataRows = numberOfDataRows,
								UploadType = (int)DeferredProcessingUploadType.Replace,
							};

							Repository.DeferredProcessingUpload.SaveEntity( deferredProcessingItem );
						}
						catch ( Exception ex )
						{
							ModelState.AddModelError( "File", "Unable to save file. " + ex.Message );
						}

						#endregion Save Uploaded File
					}
				}
			}
			catch ( Exception e )
			{
				// If we end up inside this Catch block, an unforseen error was encountered (for example, users
	// providing alpha values in a numeric field, users exceeding a field size, etc.)
	// TODO: Provide more robust error checking for Inventory Upload (Check for invalid casts, size
	//       limitations, etc.)
				Repository.GuidanceMessages.AddGuidanceMessage( guidanceMessages, lutGuidanceLevel, String.Format( "Error(s) on row {0} of your spreadsheet.  The following machine-generated text may help you discern the cause of the error(s) - \"{1} {2}\".", rowIndex, e.Message.ToString(), ( e.TargetSite != null ? e.TargetSite.ToString() : "" ) ) );
			}
		}

		#endregion Upload Shared Methods

		#region UploadInventory

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult UploadBusinessInventory( int? organizationID )
		{
			if ( organizationID == null )
			{
				return RedirectToOrganizationPortalSwitchboard();
			}

			ICERSRepositoryManager repo = ServiceLocator.GetRepositoryManager();
			CurrentOrganizationID = organizationID;
			var org = repo.Organizations.GetByID( organizationID.Value );

			OrganizationInventoryUploadViewModel viewModel = new OrganizationInventoryUploadViewModel
			{
				OrganizationID = org.ID,
				OrganizationName = org.Name,
			};

			//check if there is any pending item on deferred processing queue
			var deferredProcessing = repo.DeferredProcessingUpload.GetUnprocessedItemByOrganizationID( organizationID.Value );
			if ( deferredProcessing != null
				&& ( deferredProcessing.StatusID.ContainedIn( (int)DeferredProcessingItemStatus.InProgress, (int)DeferredProcessingItemStatus.Queued ) ) )
			{
				viewModel.IsDeferredProcessingExists = true;
				viewModel.DeferredProcessing = deferredProcessing;
				viewModel.Submitter = repo.CoreData.Accounts.GetByID( deferredProcessing.CreatedByID );
			}

			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult UploadBusinessInventory( int organizationID, OrganizationInventoryUploadViewModel viewModel )
		{
			List<GuidanceMessage> guidanceMessages = new List<GuidanceMessage>();

			//int guidanceMessageTruncationLimit = 25;
			LUTGuidanceLevel lutGuidanceLevel = new LUTGuidanceLevel();
			lutGuidanceLevel.Name = "Required";

			UploadFile( organizationID, viewModel.File, SubmittalElementType.HazardousMaterialsInventory, guidanceMessages, lutGuidanceLevel );

			var org = Repository.Organizations.GetByID( organizationID );
			viewModel.OrganizationID = org.ID;
			viewModel.OrganizationName = org.Name;
			viewModel.InventoryUploadGuidanceMessages = guidanceMessages;
			viewModel.IsDeferredProcessingExists = guidanceMessages.Count == 0;

			return View( viewModel );
		}

		private void HMICheckIsFormatValid( DataColumnCollection cols, List<GuidanceMessage> guidanceMessages, LUTGuidanceLevel lutGuidanceLevel )
		{
			if ( !cols.Contains( "CERSID" ) ||
				!cols.Contains( "ChemicalLocation" ) ||
				!cols.Contains( "CLConfidential" ) ||
				!cols.Contains( "MapNumber" ) ||
				!cols.Contains( "GridNumber" ) ||
				!cols.Contains( "ChemicalName" ) ||
				!cols.Contains( "TradeSecret" ) ||
				!cols.Contains( "CommonName" ) ||
				!cols.Contains( "EHS" ) ||
				!cols.Contains( "CASNumber" ) ||
				!cols.Contains( "PFCodeHazardClass" ) ||
				!cols.Contains( "SFCodeHazardClass" ) ||
				!cols.Contains( "TFCodeHazardClass" ) ||
				!cols.Contains( "FFCodeHazardClass" ) ||
				!cols.Contains( "FifthFireCodeHazardClass" ) ||
				!cols.Contains( "SixthFireCodeHazardClass" ) ||
				!cols.Contains( "SeventhFireCodeHazardClass" ) ||
				!cols.Contains( "EighthFireCodeHazardClass" ) ||
				!cols.Contains( "HMType" ) ||
				!cols.Contains( "RadioActive" ) ||
				!cols.Contains( "Curies" ) ||
				!cols.Contains( "PhysicalState" ) ||
				!cols.Contains( "LargestContainer" ) ||
				!cols.Contains( "FHCFire" ) ||
				!cols.Contains( "FHCReactive" ) ||
				!cols.Contains( "FHCPressureRelease" ) ||
				!cols.Contains( "FHCAcuteHealth" ) ||
				!cols.Contains( "FHCChronicHealth" ) ||
                !cols.Contains( "AverageDailyAmount" ) ||
				!cols.Contains( "MaximumDailyAmount" ) ||
				!cols.Contains( "AnnualWasteAmount" ) ||
				!cols.Contains( "StateWasteCode" ) ||
				!cols.Contains( "Units" ) ||
				!cols.Contains( "DaysOnSite" ) ||
				!cols.Contains( "SCAboveGroundTank" ) ||
				!cols.Contains( "SCUnderGroundTank" ) ||
				!cols.Contains( "SCTankInsideBuilding" ) ||
				!cols.Contains( "SCSteelDrum" ) ||
				!cols.Contains( "SCPlasticNonMetallicDrum" ) ||
				!cols.Contains( "SCCan" ) ||
				!cols.Contains( "SCCarboy" ) ||
				!cols.Contains( "SCSilo" ) ||
				!cols.Contains( "SCFiberDrum" ) ||
				!cols.Contains( "SCBag" ) ||
				!cols.Contains( "SCBox" ) ||
				!cols.Contains( "SCCylinder" ) ||
				!cols.Contains( "SCGlassBottle" ) ||
				!cols.Contains( "SCPlasticBottle" ) ||
				!cols.Contains( "SCToteBin" ) ||
				!cols.Contains( "SCTankTruckTankWagon" ) ||
				!cols.Contains( "SCTankCarRailCar" ) ||
				!cols.Contains( "SCOther" ) ||
				!cols.Contains( "OtherStorageContainer" ) ||
				!cols.Contains( "StoragePressure" ) ||
				!cols.Contains( "StorageTemperature" ) ||
				!cols.Contains( "HC1PercentByWeight" ) ||
				!cols.Contains( "HC1Name" ) ||
				!cols.Contains( "HC1EHS" ) ||
				!cols.Contains( "HC1CAS" ) ||
				!cols.Contains( "HC2PercentByWeight" ) ||
				!cols.Contains( "HC2Name" ) ||
				!cols.Contains( "HC2EHS" ) ||
				!cols.Contains( "HC2CAS" ) ||
				!cols.Contains( "HC3PercentByWeight" ) ||
				!cols.Contains( "HC3Name" ) ||
				!cols.Contains( "HC3EHS" ) ||
				!cols.Contains( "HC3CAS" ) ||
				!cols.Contains( "HC4PercentByWeight" ) ||
				!cols.Contains( "HC4Name" ) ||
				!cols.Contains( "HC4EHS" ) ||
				!cols.Contains( "HC4CAS" ) ||
				!cols.Contains( "HC5PercentByWeight" ) ||
				!cols.Contains( "HC5Name" ) ||
				!cols.Contains( "HC5EHS" ) ||
				!cols.Contains( "HC5CAS" ) ||
				!cols.Contains( "ChemicalDescriptionComment" ) ||
				!cols.Contains( "AdditionalMixtureComponents" ) ||
				!cols.Contains( "CCLID" ) ||
				!cols.Contains( "USEPASRSNumber" ) ||
                !cols.Contains( "DOTHazardClassificationID" ) )
			{
				Repository.GuidanceMessages.AddGuidanceMessage( guidanceMessages, lutGuidanceLevel, "Columns in uploaded worksheet do not match template.  Please download the inventory upload template and confirm that the column headings match your uploaded worksheet." );
			}
		}

		#endregion UploadInventory

		#region UploadBusinessFacilityInfo

		public void DownloadOwnerOperatorData( int OrganizationID )
		{
			HttpContext.Server.ScriptTimeout = 1000;
            string excelTemplateFilePath = System.IO.Path.Combine( Server.MapPath( "~" ), @"Content\TemplateFiles\MultiFacilityFacInfoUploadTemplate.xlsx" );
			var workbook = Services.Excel.ExportToExcelOwnerOperatorData(
				excelTemplateFilePath,
				OrganizationID
				);

            SetDownloadFileHeader( "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OwnerOperatorData.xlsx" );

			workbook.Save( Response.OutputStream );
			Response.End();
		}

		// force processing on deferred queue - for debugging only
		public ActionResult ProcessDeferredQueue( int organizationId )
		{
			Services.BusinessLogic.SubmittalElements.FacilityInformation.ProcessDeferredProcessingQueue();
			return RedirectToAction( "Index", "Tools", new { organizationId = organizationId } );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult UploadBusinessFacilityInfo( int? organizationID )
		{
			if ( organizationID == null )
			{
				return RedirectToOrganizationPortalSwitchboard();
			}

			ICERSRepositoryManager repo = ServiceLocator.GetRepositoryManager();
			CurrentOrganizationID = organizationID;
			var org = repo.Organizations.GetByID( organizationID.Value );

			OrganizationFacInfoUploadViewModel viewModel = new OrganizationFacInfoUploadViewModel
			{
				OrganizationID = org.ID,
				OrganizationName = org.Name,
			};

			//check if there is any pending item on deferred processing queue
			var deferredProcessing = repo.DeferredProcessingUpload.GetUnprocessedItemByOrganizationID( organizationID.Value );
			if ( deferredProcessing != null
				&& ( deferredProcessing.StatusID.ContainedIn( (int)DeferredProcessingItemStatus.InProgress, (int)DeferredProcessingItemStatus.Queued ) ) )
			{
				viewModel.IsDeferredProcessingExists = true;
				viewModel.DeferredProcessing = deferredProcessing;
				viewModel.Submitter = repo.CoreData.Accounts.GetByID( deferredProcessing.CreatedByID );
			}

			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult UploadBusinessFacilityInfo( int organizationID, OrganizationFacInfoUploadViewModel viewModel )
		{
			List<GuidanceMessage> guidanceMessages = new List<GuidanceMessage>();

			//int guidanceMessageTruncationLimit = 25;
			LUTGuidanceLevel lutGuidanceLevel = new LUTGuidanceLevel();
			lutGuidanceLevel.Name = "Required";

			UploadFile( organizationID, viewModel.File, SubmittalElementType.FacilityInformation, guidanceMessages, lutGuidanceLevel );

			var org = Repository.Organizations.GetByID( organizationID );
			viewModel.OrganizationID = org.ID;
			viewModel.OrganizationName = org.Name;
			viewModel.FacInfoUploadGuidanceMessages = guidanceMessages;
			viewModel.IsDeferredProcessingExists = guidanceMessages.Count == 0;
			Session["GuidanceMessages"] = guidanceMessages;

			return View( viewModel );
		}

		private void FacInfoCheckIsFormatValid( DataColumnCollection cols, List<GuidanceMessage> guidanceMessages, LUTGuidanceLevel lutGuidanceLevel )
		{
			if ( !cols.Contains( "CERSID" ) ||
				!cols.Contains( "BeginningDate" ) ||
				!cols.Contains( "EndingDate" ) ||
				!cols.Contains( "Phone" ) ||
				!cols.Contains( "Fax" ) ||
				!cols.Contains( "DunAndBradstreet" ) ||
				!cols.Contains( "SICCode" ) ||
				!cols.Contains( "NAICSCode" ) ||
				!cols.Contains( "MailingAddress" ) ||
				!cols.Contains( "MailingAddressCity" ) ||
				!cols.Contains( "MailingAddressState" ) ||
				!cols.Contains( "MailingAddressZipCode" ) ||
				!cols.Contains( "OperatorName" ) ||
				!cols.Contains( "OperatorPhone" ) ||
				!cols.Contains( "OwnerName" ) ||
				!cols.Contains( "OwnerPhone" ) ||
				!cols.Contains( "OwnerMailAddress" ) ||
				!cols.Contains( "OwnerCity" ) ||
				!cols.Contains( "OwnerState" ) ||
				!cols.Contains( "OwnerZipCode" ) ||
				!cols.Contains( "OwnerCountry" ) ||
				!cols.Contains( "EContactName" ) ||
				!cols.Contains( "EContactPhone" ) ||
				!cols.Contains( "EContactMailingAddress" ) ||
				!cols.Contains( "EContactEmailAddress" ) ||
				!cols.Contains( "EContactCity" ) ||
				!cols.Contains( "EContactState" ) ||
				!cols.Contains( "EContactZipCode" ) ||
				!cols.Contains( "EContactCountry" ) ||
				!cols.Contains( "PECName" ) ||
				!cols.Contains( "PECTitle" ) ||
				!cols.Contains( "PECBusinessPhone" ) ||
				!cols.Contains( "PEC24HrPhone" ) ||
				!cols.Contains( "PECPager" ) ||
				!cols.Contains( "SECName" ) ||
				!cols.Contains( "SECTitle" ) ||
				!cols.Contains( "SECBusinessPhone" ) ||
				!cols.Contains( "SEC24HrPhone" ) ||
				!cols.Contains( "SECPager" ) ||
				!cols.Contains( "ALCollectedInformation" ) ||
				!cols.Contains( "IdentificationSignedDate" ) ||
				!cols.Contains( "DocumentPreparerName" ) ||
				!cols.Contains( "IdentificationSignerName" ) ||
				!cols.Contains( "IdentificationSignerTitle" ) ||
				!cols.Contains( "BillingContactName" ) ||
				!cols.Contains( "BillingContactPhone" ) ||
				!cols.Contains( "BillingContactEmail" ) ||
				!cols.Contains( "BillingAddress" ) ||
				!cols.Contains( "BillingAddressCity" ) ||
				!cols.Contains( "BillingAddressState" ) ||
				!cols.Contains( "BillingAddressZipCode" ) ||
				!cols.Contains( "BillingAddressCountry" ) ||
				!cols.Contains( "AssessorParcelNumber" ) ||
				!cols.Contains( "NumberOfEmployees" ) ||
				!cols.Contains( "FacilityID" ) ||
				!cols.Contains( "PropertyOwnerName" ) ||
				!cols.Contains( "PropertyOwnerPhone" ) ||
				!cols.Contains( "PropertyOwnerMailingAddress" ) ||
				!cols.Contains( "PropertyOwnerCity" ) ||
				!cols.Contains( "PropertyOwnerState" ) ||
				!cols.Contains( "PropertyOwnerZipCode" ) ||
				!cols.Contains( "PropertyOwnerCountry" ) )
			{
				Repository.GuidanceMessages.AddGuidanceMessage( guidanceMessages, lutGuidanceLevel, "Columns in uploaded worksheet do not match template.  Please download the facility information upload template and confirm that the column headings match your uploaded worksheet." );
			}
		}

		#endregion UploadBusinessFacilityInfo

		public ActionResult Regulators( int organizationId )
		{
			CurrentOrganizationID = organizationId;

			OrganizationViewModel viewModel = new OrganizationViewModel
			{
				Entity = Repository.Organizations.GetByID( organizationId ),
				LeadRegulator = Repository.Organizations.GetLeadRegulator( organizationId ),
				Regulators = Repository.Organizations.GetRegulatorsForOrganization( organizationId ).ToGridView()
			};

			if ( viewModel.Regulators.Count() > 4 )
			{
				return RedirectToRoute( CommonRoute.OrganizationToolsRegulator );
			}

			return View( viewModel );
		}
	}
}