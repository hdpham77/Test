using CERS.Compositions;
using CERS.Model;
using CERS.ViewModels.Facilities;
using CERS.ViewModels.SubmittalElements;
using CERS.Web.Mvc;
using CERS.Web.UI.Organization.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Telerik.Web.Mvc;
using UPF;

namespace CERS.Web.UI.Organization.Controllers
{
	[Authorize]
	public class SubmittalController : AppControllerBase
	{
		//public ActionResult OrganizationFacilitySubmittals

		[GridAction]
		public ActionResult GetGuidanceMessagesBySubmittalElementResource( int fserid, int? levelID = null )
		{
			return View( new GridModel( Repository.GuidanceMessages.GetGuidanceMessages( fserID: fserid, levelID: levelID ).ToGridView() ) );
		}

		[GridAction]
		public ActionResult GetGuidanceMessagesBySubmittalElementResourceEntity( int fserid, int entityid, int? levelID = null )
		{
			return View( new GridModel( Repository.GuidanceMessages.GetGuidanceMessages( fserID: fserid, levelID: levelID, entityID: entityid ).ToGridView() ) );
		}

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

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult OrganizationFacilityCancel( int organizationId, int CERSID, int FSERID )
		{
			var fser = Repository.FacilitySubmittalElementResources.GetByID( FSERID );

			if ( fser.FacilitySubmittalElement.StatusID == (int)SubmittalElementStatus.Draft )
			{
				return RedirectToRoute( OrganizationFacility.Home, new { organizationId = organizationId, CERSID = CERSID } );
			}
			else
			{
				return RedirectToRoute( OrganizationFacility.SubmittalEvent, new { organizationId = organizationId, FSID = fser.FacilitySubmittalElement.FacilitySubmittalID } );
			}
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult OrganizationFacilitySubmittalArchive( int organizationId )
		{
			return RedirectToRoute( OrganizationManage.Archives, organizationId );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult OrganizationFacilitySubmittalDetail( int organizationId, int CERSID, int FSEID )
		{
			var viewModel = SystemViewModelData.BuildUpFacilitySubmittalElementViewModel( CERSID, FSEID );
			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult OrganizationFacilitySubmittalDrafts( int organizationId, int? CERSID )
		{
			//check if CERSID = 0 then set is null
			if ( CERSID.HasValue && CERSID.Value == 0 )
			{
				CERSID = null;
			}

			FacilitySubmittalElementViewModel viewModel = new FacilitySubmittalElementViewModel()
			{
				Entities = Repository.FacilitySubmittalElements.Search( CERSID: CERSID, organizationID: organizationId, status: SubmittalElementStatus.Draft )
			};

			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult OrganizationFacilitySubmittalEvent( int organizationID, int FSID )
		{
			FacilitySubmittalViewModel viewModel = SystemViewModelData.BuildUpFacilitySubmittalViewModel( FSID, organizationID );

			viewModel.FacilitySubmittalElements = viewModel.FacilitySubmittalElements.OrderBy( f => f.SubmittalElementID );

			//plug in HMI report
			FacilitySubmittalElement HMIFse = viewModel.FacilitySubmittalElements.Where( p => p.SubmittalElementID == (int)( SubmittalElementType.HazardousMaterialsInventory ) ).FirstOrDefault();
			if ( HMIFse != null )
			{
				FacilitySubmittalElementResource HMIFser = HMIFse.Resources.Where( r => r.ResourceTypeID == (int)ResourceType.HazardousMaterialInventory && !r.Voided ).FirstOrDefault();
				if ( HMIFser != null )
				{
					HazardousMaterialsInventoryController hmiController = new HazardousMaterialsInventoryController();
					viewModel.HMISReport = hmiController.PopulateHMIMatrixViewModel( organizationID, HMIFse.CERSID, HMIFse.ID, HMIFser.ID );
				}
			}

			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult OrganizationFacilitySubmittalHistory( int organizationId, int? CERSID )
		{
			Facility facility = null;
			if ( CERSID != null )
			{
				facility = Repository.Facilities.GetByID( CERSID.Value );
			}

			FacilitySubmittalElementViewModel viewModel = new FacilitySubmittalElementViewModel()
			{
				Organization = Repository.Organizations.GetByID( organizationId ),
				Facility = facility,
				SubmittedEvents = Repository.FacilitySubmittalElements.Search( organizationID: organizationId, CERSID: CERSID )
				.Where( s => s.FacilitySubmittalID.HasValue && s.OwningOrganizationID == organizationId )
				.GetSubmittalEvent( Repository )
				.OrderByDescending( o => o.SubmittedDate )
					//converting it to list sped up the UI loading tremendously
				.ToList(),
			};

			return View( viewModel );
		}

        //no longer used??? [ak 08/21/2014]
        //[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
        //public ActionResult OrganizationFacilitySubmittalRecent( int organizationId, int? CERSID )
        //{
        //    Facility facility = null;
        //    if ( CERSID != null )
        //    {
        //        facility = Repository.Facilities.GetByID( CERSID.Value );
        //    }

        //    FacilitySubmittalElementViewModel viewModel = new FacilitySubmittalElementViewModel()
        //    {
        //        Organization = Repository.Organizations.GetByID( organizationId ),
        //        Facility = facility,
        //        Entities = Repository.FacilityRegulatorSubmittalElements.Search( CERSID: CERSID, organizationId: organizationId ).Where( f => f.ReportingRequirementID != (int)ReportingRequirement.NotApplicable ).GetRecentSubmittedOrApproved()
        //    };

        //    return View( viewModel );
        //}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult SubmittalEventExportToExcel( int organizationId, int? CERSID )
		{
			var submittalEvents = Repository.FacilitySubmittalElements.GetSubmittedEvents( organizationId, CERSID );

			ExportToExcel( "SubmittalHistory.xlsx", submittalEvents.ToExportViewModel() );

            return new EmptyResult();
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult SubmittalEventFacilityExportToExcel( int CERSID )
		{
			var facility = Repository.Facilities.GetByID( CERSID );
			var submittalEvents = Repository.FacilitySubmittalElements.GetSubmittedEvents( facility.OrganizationID, CERSID );

			ExportToExcel( "SubmittalHistory.xlsx", submittalEvents.ToExportViewModel() );

			return View();
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgApprover )]
		public ActionResult SubmittalFinished( int organizationId, int CERSID, int FSID )
		{
			Facility facility = Repository.Facilities.GetByID( CERSID );
			var submittedSubmittalElements = Repository.FacilitySubmittalElements.Search( facilitySubmittalID: FSID ).ToList();
			// queue up submittal deltas for submitted FSE's
			Services.SubmittalDelta.QueueDeltaFSEs( submittedSubmittalElements );

			LandingPageViewModel viewModel = new LandingPageViewModel()
			{
				Facility = facility,
				Instructions = BuildSubmittalLandingText( submittedSubmittalElements, showPrintButton: ( submittedSubmittalElements.Count == 0 ? false : true ) ),
				GuidanceMessages = new List<GuidanceMessage>(),
				FacilitySubmittalElement = submittedSubmittalElements.FirstOrDefault(),
				FacilitySubmittalElements = submittedSubmittalElements.ToList()
			};

			//Build Whats Next list items.
			List<WhatsNextItem> whatsNextItems = new List<WhatsNextItem>();

			// Get Route Values for the 'Submittal Summary'
			RouteValueDictionary routeValues = new RouteValueDictionary
			{
				{ "organizationId", organizationId },
				{ "CERSID", CERSID }
			};

			// Get Route Name for the 'Submittal Summary'
			string submittalSummaryRouteName = GetRouteName( OrganizationFacility.Home );
			whatsNextItems.Add( new WhatsNextItem( 1, "Return to the {0} page.", "Draft Submittal", submittalSummaryRouteName, routeValues ) );

			submittalSummaryRouteName = CommonRoute.OrganizationHome.ToString();
			routeValues = new RouteValueDictionary() { { "id", organizationId } };
			whatsNextItems.Add( new WhatsNextItem( 1, "Return to {0}.", "Facility Home", submittalSummaryRouteName, routeValues ) );

			viewModel.WhatsNext = whatsNextItems;

			//added 8/9/2013: Check to see whether this person was responded to the survey
			viewModel.Survey = Repository.Infrastructure.Surveys.Get( "BizUserSurveyAugust2013" );
			if ( viewModel.Survey != null )
			{
				viewModel.PreviouslyRespondedToSurvey = Repository.Infrastructure.SurveyResponses.HasResponded( CurrentAccount, viewModel.Survey );
			}

			return View( viewModel );
		}

		public ActionResult SubmittalStart( int organizationId, int CERSID )
		{
			var viewModel = SystemViewModelData.BuildUpSubmittalStartViewModel( CERSID );

			//Validate each submittal element before the official submit
			foreach ( var submittalElement in viewModel.CurrentSubmittalElementViewModelCollection.Where( f => f.FacilitySubmitalElement != null && f.FacilitySubmitalElement.StatusID == (int)SubmittalElementStatus.Draft ).Select( c => c.FacilitySubmitalElement ) )
			{
				var fse = Repository.FacilitySubmittalElements.GetByID( submittalElement.ID );
				fse.ValidateAndCommitResults( Repository, CallerContext.UI );
			}
			viewModel = SystemViewModelData.BuildUpSubmittalStartViewModel( CERSID );
			return View( viewModel );
		}

		[HttpPost]
		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgApprover )]
		public ActionResult SubmittalStart( int organizationId, int CERSID, FormCollection form )
		{
			var viewModel = SystemViewModelData.BuildUpSubmittalStartViewModel( CERSID );
			var isFacilityInfoChecked = false;
			var fseIDs = string.Empty;
			DateTime submittedDate = DateTime.Now;

			//Create FacilitySubmittalRecord
			FacilitySubmittal facilitySubmittal = new FacilitySubmittal();
			facilitySubmittal.ReceivedOn = DateTime.Now;
			facilitySubmittal.SubmittedOn = submittedDate;
			Repository.FacilitySubmittals.Create( facilitySubmittal );

			foreach ( var submittal in viewModel.ReadyToSubmitCollection )
			{
				var FSEIDValue = form[string.Format( "submittCheckBox_{0}", submittal.CurrentSubmittalElement.SubmittalElementID )];
				var comments = form[string.Format( "regulatorText_{0}", submittal.CurrentSubmittalElement.SubmittalElementID )];

				if ( !string.IsNullOrWhiteSpace( FSEIDValue ) )
				{
					FacilitySubmittalElement submittalElement = Repository.FacilitySubmittalElements.GetByID( submittal.CurrentSubmittalElement.FSEID.Value );

					//We do not want to submit unless the Faciilty Info Submittal Element has been chekced.
					if ( isFacilityInfoChecked || submittalElement.SubmittalElementID == (int)CERS.SubmittalElementType.FacilityInformation )
					{
						submittalElement.SubmittedByComments = comments;
						Repository.FacilitySubmittalElements.Submit( submittalElement, facilitySubmittal );
						isFacilityInfoChecked = true;

						fseIDs += "," + submittalElement.ID.ToString();
						Services.BusinessLogic.SubmittalElements.SetLastFacilityInfoSubmittal( submittalElement );
					}
				}
			}

			Facility facility = Repository.Facilities.GetByID( CERSID );
			Repository.Organizations.Save( facility.Organization );
			Services.Events.CreateFacilitySubmittalNotification( facility, facilitySubmittal );
			if ( fseIDs.Length > 1 )
			{
				fseIDs = fseIDs.Remove( 0, 1 );
			}

			return RedirectToRoute( GetRouteName( Part.Submittal, Part.Finish ), new { organizationID = organizationId, CERSID = CERSID, FSID = facilitySubmittal.ID } );
		}

		private string BuildSubmittalLandingText( IEnumerable<FacilitySubmittalElement> fseCollection, bool showPrintButton = false )
		{
			StringBuilder html = new StringBuilder();

			Regulator reg = null;

			//Gets a distinct collection of submittals by Regulator
			var fseByregulators = fseCollection.GroupBy( g => g.GetRegulator().ID ).Select( x => x.First() );

			foreach ( var fse in fseByregulators )
			{
				reg = fse.GetRegulator();
				html.Append( string.Format( "You have submitted the following elements on {0} to <b>{1}</b>", fse.UpdatedOn.ToShortDateString(), reg.Name ) );
				html.Append( "<br /><br />" );
				var submittalsForRegulator = fseCollection.Where( f => f.GetRegulator().ID == reg.ID );

				html.Append( "<ul>" );
				foreach ( var subByReg in submittalsForRegulator )
				{
					html.Append( "<li>" );
					html.Append( subByReg.SubmittalElement.Name );
					html.Append( "</li>" );
				}
				html.Append( "</ul>" );
				html.Append( "<br />" );
			}

			#region Print All Button

			if ( showPrintButton )
			{
				var organizationID = fseByregulators.First().Facility.OrganizationID;
				var CERSID = fseByregulators.First().Facility.CERSID;
				var fsID = fseByregulators.First().FacilitySubmittalID;

				StringBuilder printButtonHtml = new StringBuilder();
				var onClickEvent = string.Format( "onClick=\"OpenPrintSubmittalElementDocumentsWindow({0},{1},{2}); return false;\"", organizationID, CERSID, fsID );
				var buttonTitle = "Print Submittal";
				var printIcon = Url.Content( "~/Content/Bliss/Images/Icons/printer.png" );

				printButtonHtml.AppendFormat( "<div class=\"right\" style=\"padding-top:10px\">" );
				printButtonHtml.AppendFormat( "<a href=\"\" class=\"button\" id=\"printSEDocs\" {0}>", onClickEvent );
				printButtonHtml.AppendFormat( "<span class=\"color\"><span>{0}</span></span>", buttonTitle + " <img alt=\"\" src=\"" + printIcon + "\"" );
				printButtonHtml.AppendFormat( "</a>&nbsp;&nbsp;&nbsp;</div>" );
				html.Append( printButtonHtml );
			}

			#endregion Print All Button

			return html.ToString();
		}
	}
}