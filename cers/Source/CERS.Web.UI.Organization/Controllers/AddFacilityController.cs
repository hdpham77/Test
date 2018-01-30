using CERS.ViewModels.FacilityManagement;
using CERS.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UPF;
using UPF.Core;

namespace CERS.Web.UI.Organization.Controllers
{
	[Authorize]
	public class AddFacilityController : AppControllerBase
	{
		public ActionResult PotentialDuplicates_Grid( [DataSourceRequest]DataSourceRequest request, Guid key )
		{
			var viewModel = Services.ViewModels.Facility.Management.Load( key );
			var potentialDuplicates = Services.Facilities.FindPotentialDuplicates( viewModel.State );

			DataSourceResult result = potentialDuplicates.ToDataSourceResult( request, r => new PotentialDuplicateFacilityGridViewModel
			{
				CERSID = r.CERSID,
				FacilityName = r.Name,
				LastSubmittalOn = r.LastFacilityInfoSubmittalSubmittedOn,
				OrganizationHeadquarters = r.OrganizationHeadquarters,
				OrganizationName = r.OrganizationName,
				OrganizationID = r.OrganizationID,
				Street = r.Street,
				City = r.City,
				Zipcode = r.ZipCode,
				State = "CA",
				WashedStreet = r.WashedStreet,
				WashedStreetWithoutSuite = r.WashedStreetWithoutSuite,
				WashedSuite = r.WashedSuite,
				WashedCity = r.WashedCity,
				WashedState = "CA",
				WashedZipcode = r.WashedZipCode
			} );

			return Json( result, JsonRequestBehavior.AllowGet );
		}

		public ActionResult Start( int? organizationID = null )
		{
			var viewModel = Services.ViewModels.Facility.Management.Start( organizationID );
			return RedirectToRoute( FacilityManagementRoute.Wizard, new { key = viewModel.State.Key } );
		}

		public ActionResult VerifyCityByZip_Async( string city, string zipCode, string state = "CA" )
		{
			var result = Services.Geo.VerifyCityByZip( city, state, zipCode );

			var jsonResult = new
			{
				Valid = string.IsNullOrWhiteSpace( result.Message ),
				Message = result.Message //,
				//	ServiceResult = result
			};

			return Json( jsonResult, JsonRequestBehavior.AllowGet );
		}

		public ActionResult Wizard( Guid key )
		{
			var viewModel = Services.ViewModels.Facility.Management.Load( key );
			if ( viewModel.State == null )
			{
				//if we don't have any wizard state for the specified key, probably due to the guid being empty or invalid,
				//in either case, lets start a new one. We hope this won't happen, but stuff happens.
				return RedirectToAction( "Start" );
			}

			viewModel.TargetStep = viewModel.State.GetCurrentStep();

			return View( viewModel );
		}

		[HttpPost]
		public ActionResult Wizard( Guid key, FormCollection form )
		{
			//lets load the ViewModel from state by the key
			var viewModel = Services.ViewModels.Facility.Management.Load( key );

			//verify our model is valid
			if ( ModelState.IsValid )
			{
				//now lets merge the view's post data with viewmodel state from the db.
				if ( TryUpdateModel( viewModel ) )
				{
					//decide what the current step is and call that steps processing method accordingly.
					if ( viewModel.TargetStep == AddFacilityWizardStep.ProvideAddress )
					{
						Handle_ProvideAddress( viewModel );
					}
					else if ( viewModel.TargetStep == AddFacilityWizardStep.ProvideFacilityName )
					{
						Handle_ProvideFacilityName( viewModel );
					}
					else if ( viewModel.TargetStep == AddFacilityWizardStep.ChooseExistingFacility )
					{
						Handle_ChooseExistingFacility( viewModel );
					}
					else if ( viewModel.TargetStep == AddFacilityWizardStep.FacilityExistsRequestAccessOrTransfer )
					{
						Handle_FacilityExistsRequestAccessOrTransfer( viewModel );
					}
					else if ( viewModel.TargetStep == AddFacilityWizardStep.NewAdditionalFacilitySharesAddressWithAnotherFacility )
					{
						Handle_NewAdditionalFacilitySharesAddressWithAnotherFacility( viewModel );
					}
					else if ( viewModel.TargetStep == AddFacilityWizardStep.NewFacilityNewOrganization )
					{
						Handle_NewFacilityNewOrganization( viewModel );
					}
				}
			}

			return PartialView( viewModel.TargetStepView, viewModel );
		}

		protected virtual void Handle_ChooseExistingFacility( AddFacilityWizardViewModel viewModel )
		{
			//determine whether or not we have a selected CERSID
			if ( viewModel.TargetCERSID.HasValue )
			{
				viewModel.State.CERSID = viewModel.TargetCERSID;

				//get the selected Facility
				var facility = Repository.Facilities.GetByID( viewModel.TargetCERSID.Value );
				if ( facility != null )
				{
					//lets check to see if the current user already has access to this Facility via its associated organization.
					if ( CurrentUserRoles.Contains( facility.CERSID, Context.Organization ) )
					{
						viewModel.SetStep( AddFacilityWizardStep.FacilityExistsAlreadyBelongsToUsersOrganization );
					}
					else
					{
						viewModel.SetStep( AddFacilityWizardStep.FacilityExistsRequestAccessOrTransfer );
					}
				}
				else
				{
					//not sure yet what to do about this....it shouldn't happen, but you never know...
				}
				Services.ViewModels.Facility.Management.SaveState( viewModel );
			}
		}

		protected virtual void Handle_FacilityExistsRequestAccessOrTransfer( AddFacilityWizardViewModel viewModel )
		{
		}

		protected virtual void Handle_NewAdditionalFacilitySharesAddressWithAnotherFacility( AddFacilityWizardViewModel viewModel )
		{
		}

		protected virtual void Handle_NewFacilityNewOrganization( AddFacilityWizardViewModel viewModel )
		{
		}

		/// <summary>
		/// This method is responsible for handling the POST request of the step "Provide Address". In this method, we analyze the
		/// address and figure out whether we should be creating a new facility because the address doesn't yet exist in CERS.
		/// This method decides what view we progress to next...
		/// </summary>
		/// <param name="viewModel"></param>
		protected virtual void Handle_ProvideAddress( AddFacilityWizardViewModel viewModel )
		{
			//Step 1: Geocode/Wash address for the specified address.
			Services.ViewModels.Facility.Management.Geocode( viewModel );

			//Step 2: Find an existing Facility based on both washed and non-washed addresses.
			var potentialDuplicates = Services.Facilities.FindPotentialDuplicates( viewModel.State );

			//get the count of the potential duplicates.
			int potentialDuplicateCount = potentialDuplicates.Count();

			//lets record the potential duplicate address count on the state bag for pick up in the UI and
			//for general recording of what we are doing.
			viewModel.State.PotentialDuplicateAddressCount = potentialDuplicateCount;

			if ( potentialDuplicateCount > 0 )
			{
				//show the choose existing facility view since we have potential duplicates for the address
				//provided by the user.
				viewModel.SetStep( AddFacilityWizardStep.ChooseExistingFacility );
			}
			else
			{
				//this looks like a unique address that does not already exist in CERS, so lets let the user proceed with
				//naming the facility...
				viewModel.SetStep( AddFacilityWizardStep.ProvideFacilityName );
			}

			//save our state as we move along through the wizard.
			Services.ViewModels.Facility.Management.SaveState( viewModel );
		}

		protected virtual void Handle_ProvideFacilityName( AddFacilityWizardViewModel viewModel )
		{
			if ( viewModel.State.OrganizationID.HasValue )
			{
				//In this case, we should create the Facility, because we know everything we need to
				//in order to do so. (The new facility should be added to the OrganizationID associated with
				//the state bag.
				//TODO: Create Facility
				viewModel.SetStep( AddFacilityWizardStep.NewFacilityAddedConfirmation );
			}
			else
			{
				//The user should be presented with a screen that les them see what the new facility is they are creating.
				//and allow them to verify the new Organization name/headquarter information.
				viewModel.SetStep( AddFacilityWizardStep.NewFacilityNewOrganization );
			}
			//save our state as we move along through the wizard.
			Services.ViewModels.Facility.Management.SaveState( viewModel );
		}
	}
}