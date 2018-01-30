using CERS.Model;
using CERS.ViewModels;
using CERS.ViewModels.Chemicals;
using CERS.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using UPF;
using UPF.Web.Mvc;
using Winnovative.ExcelLib;

namespace CERS.Web.UI.Organization.Controllers
{
	[Authorize]
	public class HazardousMaterialsInventoryController : SubmittalElementControllerBase
	{
		//
		// GET: /HazardousMaterialsInventory/

		public ActionResult BPFacilityChemicalDetail()
		{
			CERSEntities entities = new CERSEntities();

			var entity = ( from t in entities.BPFacilityChemicals
						   where t.ID == 1002
						   select t ).FirstOrDefault();

			return View( entity );
		}

		public ActionResult BPFacilityChemicalEdit()
		{
			CERSEntities entities = new CERSEntities();

			var entity = ( from t in entities.BPFacilityChemicals
						   where t.ID == 1002
						   select t ).FirstOrDefault();

			return View( entity );
		}

		[HttpPost]
		public ActionResult BPFacilityChemicalEdit( FormCollection collection )
		{
			return RedirectToAction( "BPFacilityChemicalLandingPage" );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult HazMatInvStart( int organizationId, int CERSID, int? FSEID )
		{
			//Cloning Logic Start
			//check to see if we have a facility information in draft...
			var fseCurrentDraft = Services.Facilities.GetCurrentDraft( CERSID, SubmittalElementType.HazardousMaterialsInventory );

			//If FSEID is passed in AND there isn't a current facility info in Draft start the cloning and redirect to crazy page.
			if ( FSEID.HasValue && fseCurrentDraft == null )
			{
				return Handle_Cloning( organizationId, CERSID, FSEID.Value, SubmittalElementType.HazardousMaterialsInventory );
			}

			//Cloning Logic End

			// The first call was to check to see if there is already a draft. This call will persist a new
			// draft if one doesn't exists
			var fse = Services.BusinessLogic.SubmittalElements.GetFacilitySubmittalElement( CERSID, SubmittalElementType.HazardousMaterialsInventory, SubmittalElementStatus.Draft, true );

			//Validate this submittal element
			fse.ValidateAndCommitResults( Repository, CallerContext.UI );

			//need to see if we already got a BPActivity form.
			string routeName = string.Empty;
			var resource = fse.Resources.SingleOrDefault( p => p.ResourceTypeID == (int)ResourceType.HazardousMaterialInventory && !p.Voided );
			routeName = GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Home );

			return RedirectToRoute( routeName, new { organizationId = organizationId, CERSID = CERSID, FSEID = fse.ID, FSERID = resource.ID } );
		}

		public ActionResult Index()
		{
			return View();
		}

		#region Hazardous Material Inventory

		#region Home_HazardousMaterialInventory

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Home_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			HazardousMaterialInventoryViewModel<BPFacilityChemical> viewModel = new HazardousMaterialInventoryViewModel<BPFacilityChemical>();

			var currentSubmittalElement = Repository.FacilityRegulatorSubmittalElements.GetCurrentFacilitySubmittalElements( CERSID, (int)SubmittalElementType.HazardousMaterialsInventory ).ToList().FirstOrDefault();
			var facilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			var facilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID );
			var deferredProcessingInventory = Repository.DeferredProcessingUpload.GetUnprocessedItemByCERSID( CERSID, organizationId, SubmittalElementType.HazardousMaterialsInventory );

			viewModel.Entity = new BPFacilityChemical();
			var fserBPFacilityChemicalViewModel = new FSERBPFacilityChemicalViewModel()
			{
				FacilitySubmittalElementResource = facilitySubmittalElementResource,

				BPFacilityChemicals = Repository.BPFacilityChemicals.GetByFserID( FSERID ),
				IsDeferredProcessingExists = ( deferredProcessingInventory != null && deferredProcessingInventory.CERSID == CERSID && ( deferredProcessingInventory.StatusID.ContainedIn( (int)DeferredProcessingItemStatus.InProgress, (int)DeferredProcessingItemStatus.Queued ) ) ),
				IsDeferredProcessingInProgress = ( deferredProcessingInventory != null && deferredProcessingInventory.StatusID == (int)DeferredProcessingItemStatus.InProgress ),
				IsOrganizationWideDeferredProcessing = ( deferredProcessingInventory != null && deferredProcessingInventory.CERSID == null ),
			};
			viewModel.EntityCount = fserBPFacilityChemicalViewModel.BPFacilityChemicals.Count();	//
			//
			// Repository.BPFacilityChemicals.GetCount(
			// FSERID );
			viewModel.FSERBPFacilityChemicalViewModel = fserBPFacilityChemicalViewModel;

			viewModel.CurrentSubmittalElement = currentSubmittalElement;
			viewModel.FacilitySubmittalElement = facilitySubmittalElement;
			viewModel.FacilitySubmittalElementResource = facilitySubmittalElementResource;

			viewModel.HMISReport = PopulateHMIMatrixViewModel( organizationId, CERSID, FSERID, FSERID );

			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		[HttpPost]
		public ActionResult Home_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID, HazardousMaterialInventoryViewModel<BPFacilityChemical> viewModel )
		{
			var currentSubmittalElement = Repository.FacilityRegulatorSubmittalElements.GetCurrentFacilitySubmittalElements( CERSID, (int)SubmittalElementType.HazardousMaterialsInventory ).ToList().FirstOrDefault();
			var facilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			var facilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID );

			viewModel.Entity = new BPFacilityChemical();

			var fserBPFacilityChemicalViewModel = new FSERBPFacilityChemicalViewModel()
			{
				FacilitySubmittalElementResource = facilitySubmittalElementResource,
				BPFacilityChemicals = Repository.BPFacilityChemicals.Search( FSERID: FSERID, Name: viewModel.FSERBPFacilityChemicalViewModel.Name, ChemicalLocation: viewModel.FSERBPFacilityChemicalViewModel.Location, CASNumber: viewModel.FSERBPFacilityChemicalViewModel.CASNumber ),
				Name = viewModel.FSERBPFacilityChemicalViewModel.Name,
				Location = viewModel.FSERBPFacilityChemicalViewModel.Location,
				CASNumber = viewModel.FSERBPFacilityChemicalViewModel.CASNumber
			};
			viewModel.FSERBPFacilityChemicalViewModel = fserBPFacilityChemicalViewModel;

			viewModel.CurrentSubmittalElement = currentSubmittalElement;
			viewModel.FacilitySubmittalElement = facilitySubmittalElement;
			viewModel.FacilitySubmittalElementResource = facilitySubmittalElementResource;

			viewModel.HMISReport = PopulateHMIMatrixViewModel( organizationId, CERSID, FSERID, FSERID );

			return View( viewModel );
		}

		#endregion Home_HazardousMaterialInventory

		#region Detail_HazardousMaterialInventory

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID, int BPFCID )
		{
			HazardousMaterialInventoryViewModel<BPFacilityChemical> viewModel = new HazardousMaterialInventoryViewModel<BPFacilityChemical>();
			viewModel.FacilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			viewModel.FacilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID );
			viewModel.Entity = Repository.BPFacilityChemicals.GetByID( BPFCID );
			FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource, resourceEntityID: BPFCID );
			return View( viewModel );
		}

		public void Detail_HazardousMaterialInventory_Print( int organizationId, int CERSID, int FSEID, int FSERID, int BPFCID )
		{
			Services.Reports.HazardousMaterialInventoryBLL( organizationId, CERSID, FSEID, FSERID, BPFCID );
		}

		#endregion Detail_HazardousMaterialInventory

		#region New_HazardousMaterialInventory

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult New_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			HazardousMaterialInventoryViewModel<BPFacilityChemical> viewModel = new HazardousMaterialInventoryViewModel<BPFacilityChemical>();
			viewModel.FacilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			viewModel.FacilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID );

			// Default Chemical Entity
			viewModel.Chemical = new Chemical();

			// Default Chemical Search to Empty IEnumerable
			viewModel.Chemicals = new List<Chemical>().ToList();
			viewModel.ChemicalLibraryEntries = new List<ChemicalLibraryEntry>().ToList();
			viewModel.ChemicalLibrarySearchResults = new List<ChemicalLibrarySearchResult>().ToList();

			List<HazardousMaterialInventoryViewModel<BPFacilityChemical>.DropDownListViewModel> searchOptions = new List<HazardousMaterialInventoryViewModel<BPFacilityChemical>.DropDownListViewModel>();
			searchOptions.Add( new HazardousMaterialInventoryViewModel<BPFacilityChemical>.DropDownListViewModel { ID = (int)StringSearchOption.StartsWith, Name = "Starts with" } );
			searchOptions.Add( new HazardousMaterialInventoryViewModel<BPFacilityChemical>.DropDownListViewModel { ID = (int)StringSearchOption.ExactMatch, Name = "Exact Match" } );
			searchOptions.Add( new HazardousMaterialInventoryViewModel<BPFacilityChemical>.DropDownListViewModel { ID = (int)StringSearchOption.Contains, Name = "Contains" } );
			searchOptions.Add( new HazardousMaterialInventoryViewModel<BPFacilityChemical>.DropDownListViewModel { ID = (int)StringSearchOption.EndsWith, Name = "Ends with" } );

			viewModel.SearchOptions = searchOptions;
			viewModel.ChemicalNameSearchOption = StringSearchOption.StartsWith;

			return View( viewModel );
		}

		#endregion New_HazardousMaterialInventory

		#region Create_HazardousMaterialInventory

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID, int CID, string source )
		{
			HazardousMaterialInventoryViewModel<BPFacilityChemical> viewModel = new HazardousMaterialInventoryViewModel<BPFacilityChemical>();
			viewModel.FacilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			viewModel.FacilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID );
			viewModel.Entity = new BPFacilityChemical();

			// Need to link empty instance of Chemical to allow display of CERS Chemical Library ID
			viewModel.Entity.Chemical = new Chemical();

			if ( !string.IsNullOrWhiteSpace( source ) )
			{
				if ( source == "ChemicalLibrary" )
				{
					// TODO: Add Ajax Auto-Complete for Chemical Inventory Locations Drop-down (Create View)
					Services.BusinessLogic.SubmittalElements.HazardousMaterialsInventory.PopulateFacilityChemicalFromLibrary( viewModel.Entity, CID );
				}
				else
				{
					viewModel.Entity = Repository.BPFacilityChemicals.GetByID( CID );
				}
			}

			//replacest the inefficient code above.
			var chemicalLocation = Repository.BPFacilityChemicals.GetFacilityChemicalLocationsList( CERSID );

			ViewData.Add( "CERSChemicalLocation", chemicalLocation );

			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID, int CID, FormCollection formCollection )
		{
			HazardousMaterialInventoryViewModel<BPFacilityChemical> viewModel = new HazardousMaterialInventoryViewModel<BPFacilityChemical>();
			viewModel.FacilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			viewModel.FacilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID );

			var bpFacilityChemical = new BPFacilityChemical();
			if ( TryUpdateModel( bpFacilityChemical, "Entity" ) )
			{
				if ( ModelState.IsValid )
				{
					viewModel.Entity = bpFacilityChemical;
					Services.BusinessLogic.SubmittalElements.HazardousMaterialsInventory.SaveFacilityChemicalForm( viewModel, CID );
				}
			}

			string routeName = GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Home );
			if ( formCollection["action"] != null && formCollection["action"].Contains( "Add Another Material" ) )
			{
				routeName = GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.New );
			}

			return RedirectToRoute( routeName, new { organizationId = organizationId, CERSID = CERSID, FSEID = FSEID, FSERID = FSERID } );
		}

		#endregion Create_HazardousMaterialInventory

		#region Edit_HazardousMaterialInventory

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID, int BPFCID )
		{
			HazardousMaterialInventoryViewModel<BPFacilityChemical> viewModel = new HazardousMaterialInventoryViewModel<BPFacilityChemical>();
			viewModel.FacilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			viewModel.FacilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID );
			viewModel.Entity = Repository.BPFacilityChemicals.GetByID( BPFCID );
			FeedGuidanceMessagesIntoModelState( viewModel.FacilitySubmittalElementResource, resourceEntityID: BPFCID );

			//Add Auto-Complete for Chemical Inventory Locations Drop-down (Edit View)
			//commenting out as its rediculously inefficient.
			//var chemicalLocation = Repository.BPFacilityChemicals
			//	.Search(CERSID: CERSID)
			//	.Select(c => String.IsNullOrWhiteSpace(c.ChemicalLocation) ? String.Empty : c.ChemicalLocation)
			//	.Distinct()
			//	.OrderBy(o => o.ToUpper())
			//	.AsEnumerable();

			//replacest the inefficient code above.
			var chemicalLocation = Repository.BPFacilityChemicals.GetFacilityChemicalLocationsList( CERSID );

			ViewData.Add( "CERSChemicalLocation", chemicalLocation );

			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost, ValidateInput( false )]
		public ActionResult Edit_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID, int BPFCID, FormCollection formCollection )
		{
			HazardousMaterialInventoryViewModel<BPFacilityChemical> viewModel = new HazardousMaterialInventoryViewModel<BPFacilityChemical>();
			viewModel.FacilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			viewModel.FacilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID );

			var bpFacilityChemical = Repository.BPFacilityChemicals.GetByID( BPFCID );
			if ( TryUpdateModel( bpFacilityChemical, "Entity" ) )
			{
				if ( ModelState.IsValid )
				{
					viewModel.Entity = bpFacilityChemical;
					Services.BusinessLogic.SubmittalElements.HazardousMaterialsInventory.SaveFacilityChemicalForm( viewModel );

					//The Code below was commented out because the logic was moved into the Services Business Logic API for business logic consolidation and reuse
					//if (bpFacilityChemical.ChemicalID.HasValue)
					//{
					//    Chemical chemical = Repository.Chemicals.GetByID(bpFacilityChemical.ChemicalID.Value);
					//    // Remove the link between the BPFacilityChemical record and Chemical if the
					//    // ChemicalName, CommonName, and CAS Number do not match between the two records:
					//    if (
					//        !(
					//           ((chemical.ChemicalName == null && bpFacilityChemical.ChemicalName == null) || (chemical.ChemicalName != null && bpFacilityChemical.ChemicalName != null && chemical.ChemicalName.Trim().Equals(bpFacilityChemical.ChemicalName.Trim(), StringComparison.CurrentCultureIgnoreCase))) &&
					//           ((chemical.CommonName == null && bpFacilityChemical.CommonName == null) || (chemical.CommonName != null && bpFacilityChemical.CommonName != null && chemical.CommonName.Trim().Equals(bpFacilityChemical.CommonName.Trim(), StringComparison.CurrentCultureIgnoreCase))) &&
					//           ((chemical.CAS == null && bpFacilityChemical.CASNumber == null) || (chemical.CAS != null && bpFacilityChemical.CASNumber != null && chemical.CAS.Trim().Equals(bpFacilityChemical.CASNumber.Trim(), StringComparison.CurrentCultureIgnoreCase)))
					//         )
					//       )
					//    {
					//        bpFacilityChemical.ChemicalID = null;
					//    }
					//}

					//Repository.BPFacilityChemicals.Save(bpFacilityChemical);

					// Validate this Submittal Element
					//viewModel.FacilitySubmittalElement.ValidateAndCommitResults(Repository, CallerContext.UI);
				}
			}

			string routeName = GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Home );
			return RedirectToRoute( routeName, new { organizationId = organizationId, CERSID = CERSID, FSEID = FSEID, FSERID = FSERID } );
		}

		#endregion Edit_HazardousMaterialInventory

		#region Delete_HazardousMaterialInventory

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID, int? BPFCID )
		{
			var fser = Repository.FacilitySubmittalElementResources.GetByID( FSERID );

			if ( BPFCID.HasValue )
			{
				Services.BusinessLogic.SubmittalElements.HazardousMaterialsInventory.DeleteFacilityChemicalForm( fser, BPFCID.Value, CallerContext.UI );
			}
			else
			{
				return Handle_DiscardResource( organizationId, CERSID, FSEID, FSERID );
			}

			return RedirectToAction( "Home_HazardousMaterialInventory" );
		}

		#endregion Delete_HazardousMaterialInventory

		#region Copy_HazardousMaterialInventory

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Copy_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			HazardousMaterialInventoryViewModel<BPFacilityChemical> viewModel = new HazardousMaterialInventoryViewModel<BPFacilityChemical>();
			viewModel.FacilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			viewModel.FacilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID );
			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Copy_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID, FormCollection formCollection )
		{
			return RedirectToFacilityDraftSubmittals( organizationId, CERSID, "#INV" );
		}

		#endregion Copy_HazardousMaterialInventory

		#region Upload_HazardousMaterialInventory

		[HttpPost]
		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult DeleteQueuedDeferredProcessing( int CERSID, int facilitySubmittalElementResourceID )
		{
			var message = string.Empty;
			var result = true;

			var deferredProcessingInventory = Repository.DeferredProcessingUpload.GetUnprocessedItemByCERSID( CERSID, SubmittalElementType.HazardousMaterialsInventory );
			if ( deferredProcessingInventory == null )
			{
				message = "Cannot find any deferred processing for <strong>CERSID# " + CERSID.ToString() + "</strong>";
			}
			else
			{
				//check again one more time if the queued item already starts
				if ( deferredProcessingInventory.StatusID == (int)DeferredProcessingItemStatus.InProgress )
				{
					message = "Cannot delete, the queued item for <strong>CERSID# " + CERSID.ToString() + "</strong><br />already in progress!";
					result = false;
				}
				else
				{
					try
					{
						deferredProcessingInventory.StatusID = (int)DeferredProcessingItemStatus.Cancelled;
						deferredProcessingInventory.Voided = true;
						Repository.DeferredProcessingUpload.Save( deferredProcessingInventory );
						message = "Successfully deleting deferred processing<br>Hazardous Material Inventory for <strong>CERSID# " + CERSID.ToString() + "</strong>";
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

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Upload_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			HazardousMaterialInventoryViewModel<BPFacilityChemical> viewModel = new HazardousMaterialInventoryViewModel<BPFacilityChemical>();
			viewModel.FacilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			viewModel.FacilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID );
			viewModel.InventoryUploadGuidanceMessages = new List<GuidanceMessage>();
			viewModel.IsDeferredProcessingExists = Repository.DeferredProcessingUpload.IsThereUnprocessedOrganizationDeferredProcessing( organizationId, SubmittalElementType.HazardousMaterialsInventory, CERSID );
			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Upload_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID, HazardousMaterialInventoryViewModel<BPFacilityChemical> viewModel )
		{
			var fse = Repository.FacilitySubmittalElements.GetByID( FSEID );
			viewModel.FacilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			viewModel.FacilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID );

			List<GuidanceMessage> guidanceMessages = new List<GuidanceMessage>();
			int guidanceMessageTruncationLimit = 25;
			LUTGuidanceLevel lutGuidanceLevel = new LUTGuidanceLevel();
			lutGuidanceLevel.Name = "Required";

			// Track row of being processed (this is updated dynamically if the first row of data is not the
			// second row)
			int rowDataBegins = 2;
			int rowIndex = 0;

			BPFacilityChemical bpFacilityChemical = null;

			// Perform a larger try/catch around the uploaded file processing. If any unforseen exception is
			// encountered, add a more generic error message to the guidanceMessages property
			try
			{
				CheckIsFileValid( viewModel.File, guidanceMessages, lutGuidanceLevel );

				if ( guidanceMessages.Count == 0 )
				{
					//Need to convert to Stream to handle both xls & xlsx format (2003 & 2007)
					byte[] tempArray = new byte[viewModel.File.InputStream.Length];
					viewModel.File.InputStream.Read( tempArray, 0, (int)viewModel.File.InputStream.Length );
					System.IO.Stream hmiExcelInputStream = new System.IO.MemoryStream( tempArray );

					// Load Uploaded Excel Workbook, and open first Worksheet
					ExcelWorkbook uploadedWorkbook = new ExcelWorkbook( hmiExcelInputStream );
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

					// Reload the DataTable, this time without surplus header rows -
					uploadedDataTable = uploadedWorksheet.GetDataTable( uploadedWorksheet.UsedRange, true );
					var cols = uploadedDataTable.Columns;

					// Validate that the uploaded spreadsheet contains the required columns
					CheckIsFormatValid( cols, guidanceMessages, lutGuidanceLevel );

					int numberOfDataRows = 0;
					// Only proceed if no Guidance Messages were thrown
					if ( guidanceMessages.Count == 0 )
					{
						numberOfDataRows = CERS.ModelValidationExtensionMethods.HMIValidate( uploadedDataTable, rowDataBegins, guidanceMessages, lutGuidanceLevel, guidanceMessageTruncationLimit, organizationId, CERSID );
					}

					// Only import inventory if no guidance messages were produced
					if ( guidanceMessages.Count == 0 )
					{
						// Update the "uploadedDataTable" property to contain the new set of data, minus the deleted rows
						uploadedDataTable = uploadedWorksheet.GetDataTable( uploadedWorksheet.UsedRange, true );

						//int numberOfDataRows = uploadedDataTable.Rows.Count - rowDataBegins + 2;

						///TO DO: create an entry and get this value from settings table
						int maxThreshold = Repository.Settings.GetMaxHMIFileThreshold();
						if ( numberOfDataRows > maxThreshold )
						{
							Repository.GuidanceMessages.AddGuidanceMessage( guidanceMessages, lutGuidanceLevel, String.Format( "Inventory spreadsheet contain more than {0} materials. It will be queued for the background processing.", maxThreshold ) );

							//save the file for deferred processing
							Document document = null;
							try
							{
								document = Repository.Documents.Save( viewModel.File.InputStream, viewModel.File.FileName, DocumentContext.Organization, organizationId );
								DeferredProcessingUpload DeferredProcessing = new DeferredProcessingUpload
								{
									OrganizationID = organizationId,
									CERSID = CERSID,
									FacilitySubmittalElementID = FSEID,
									FacilitySubmittalElementResourceID = FSERID,
									SubmittalElementID = (int)SubmittalElementType.HazardousMaterialsInventory,
									DocumentID = document.ID,
									NumberOfDataRows = numberOfDataRows,
									UploadType = (int)viewModel.DeferredProcessingUploadType,
									BatchTypeID = (int)DeferredProcessingBatchType.SingleFacilityHMI,
								};

								Repository.DeferredProcessingUpload.SaveEntity( DeferredProcessing );

								//flag the viewModel for the refresh
								viewModel.IsDeferredProcessingExists = true;
							}
							catch ( Exception ex )
							{
								ModelState.AddModelError( "File", "Unable to save file. " + ex.Message );
							}
							if ( document != null )
							{
								//do something to flag the Submittal that it has HazMat Inventory on queue
							}
						}
						else
						{
							// If Replace/Append option was "Replace", delete existing Haz Mat Inventory
							if ( viewModel.DeferredProcessingUploadType == DeferredProcessingUploadType.Replace )
							{
								viewModel.FacilitySubmittalElementResource.Discard( Repository );
							}

							// Reset rowIndex to the first row of data
							rowIndex = rowDataBegins;

							foreach ( DataRow row in uploadedDataTable.Rows )
							{
								// Only process this row if the CERSID is populated and matches the current CERSID
								if ( row["CERSID"].ToString().Trim().Equals( CERSID.ToString().Trim() ) )
								{
									// Process this row
									bpFacilityChemical = new BPFacilityChemical();
									bpFacilityChemical.FacilitySubmittalElementResourceID = viewModel.FacilitySubmittalElementResource.ID;

									// TODO: Link uploaded BPFacilityChemical record to CERS Chemical Library entry if CCLID was specified
									//TODO: Fix CCLID
									//bpFacilityChemical.CCLID = row["CCLID"].ToString().Trim().Equals("") ? (int?)null : Int32.Parse(row["CCLID"].ToString().Trim());
									bpFacilityChemical.ChemicalLocation = row["ChemicalLocation"].ToString().Trim();
									bpFacilityChemical.CLConfidential = row["CLConfidential"].ToString().Trim();
									bpFacilityChemical.MapNumber = row["MapNumber"].ToString().Trim();
									bpFacilityChemical.GridNumber = row["GridNumber"].ToString().Trim();
									bpFacilityChemical.ChemicalName = row["ChemicalName"].ToString().Trim();
									bpFacilityChemical.TradeSecret = row["TradeSecret"].ToString().Trim();
									bpFacilityChemical.CommonName = row["CommonName"].ToString().Trim();
									bpFacilityChemical.EHS = row["EHS"].ToString().Trim();
									bpFacilityChemical.CASNumber = row["CASNumber"].ToString().Trim();
									bpFacilityChemical.PFCodeHazardClass = row["PFCodeHazardClass"].ToString().Trim().Equals( "" ) ? (int?)null : Int32.Parse( row["PFCodeHazardClass"].ToString().Trim() );
									bpFacilityChemical.SFCodeHazardClass = row["SFCodeHazardClass"].ToString().Trim().Equals( "" ) ? (int?)null : Int32.Parse( row["SFCodeHazardClass"].ToString().Trim() );
									bpFacilityChemical.TFCodeHazardClass = row["TFCodeHazardClass"].ToString().Trim().Equals( "" ) ? (int?)null : Int32.Parse( row["TFCodeHazardClass"].ToString().Trim() );
									bpFacilityChemical.FFCodeHazardClass = row["FFCodeHazardClass"].ToString().Trim().Equals( "" ) ? (int?)null : Int32.Parse( row["FFCodeHazardClass"].ToString().Trim() );
									bpFacilityChemical.FifthFireCodeHazardClass = row["FifthFireCodeHazardClass"].ToString().Trim().Equals( "" ) ? (int?)null : Int32.Parse( row["FifthFireCodeHazardClass"].ToString().Trim() );
									bpFacilityChemical.SixthFireCodeHazardClass = row["SixthFireCodeHazardClass"].ToString().Trim().Equals( "" ) ? (int?)null : Int32.Parse( row["SixthFireCodeHazardClass"].ToString().Trim() );
									bpFacilityChemical.SeventhFireCodeHazardClass = row["SeventhFireCodeHazardClass"].ToString().Trim().Equals( "" ) ? (int?)null : Int32.Parse( row["SeventhFireCodeHazardClass"].ToString().Trim() );
									bpFacilityChemical.EighthFireCodeHazardClass = row["EighthFireCodeHazardClass"].ToString().Trim().Equals( "" ) ? (int?)null : Int32.Parse( row["EighthFireCodeHazardClass"].ToString().Trim() );
									bpFacilityChemical.HMType = row["HMType"].ToString().Trim();
									bpFacilityChemical.RadioActive = row["RadioActive"].ToString().Trim();
									bpFacilityChemical.Curies = row["Curies"].ToString().Trim().Equals( "" ) ? (double?)null : Double.Parse( row["Curies"].ToString().Trim() );
									bpFacilityChemical.PhysicalState = row["PhysicalState"].ToString().Trim();
									bpFacilityChemical.LargestContainer = row["LargestContainer"].ToString().Trim().Equals( "" ) ? (double?)null : Double.Parse( row["LargestContainer"].ToString().Trim() );
									bpFacilityChemical.FHCFire = row["FHCFire"].ToString().Trim();
									bpFacilityChemical.FHCReactive = row["FHCReactive"].ToString().Trim();
									bpFacilityChemical.FHCPressureRelease = row["FHCPressureRelease"].ToString().Trim();
									bpFacilityChemical.FHCAcuteHealth = row["FHCAcuteHealth"].ToString().Trim();
									bpFacilityChemical.FHCChronicHealth = row["FHCChronicHealth"].ToString().Trim();
                                    bpFacilityChemical.AverageDailyAmount = row["AverageDailyAmount"].ToString().Trim().Equals( "" ) ? (double?)null : Double.Parse( row["AverageDailyAmount"].ToString().Trim() );
									bpFacilityChemical.MaximumDailyAmount = row["MaximumDailyAmount"].ToString().Trim().Equals( "" ) ? (double?)null : Double.Parse( row["MaximumDailyAmount"].ToString().Trim() );
									bpFacilityChemical.AnnualWasteAmount = row["AnnualWasteAmount"].ToString().Trim().Equals( "" ) ? (double?)null : Double.Parse( row["AnnualWasteAmount"].ToString().Trim() );
									bpFacilityChemical.StateWasteCode = row["StateWasteCode"].ToString().Trim();
									bpFacilityChemical.Units = row["Units"].ToString().Trim();
									bpFacilityChemical.DaysOnSite = row["DaysOnSite"].ToString().Trim().Equals( "" ) ? (int?)null : Int32.Parse( row["DaysOnSite"].ToString().Trim() );
									bpFacilityChemical.SCAboveGroundTank = row["SCAboveGroundTank"].ToString().Trim();
									bpFacilityChemical.SCUnderGroundTank = row["SCUnderGroundTank"].ToString().Trim();
									bpFacilityChemical.SCTankInsideBuilding = row["SCTankInsideBuilding"].ToString().Trim();
									bpFacilityChemical.SCSteelDrum = row["SCSteelDrum"].ToString().Trim();
									bpFacilityChemical.SCPlasticNonMetallicDrum = row["SCPlasticNonMetallicDrum"].ToString().Trim();
									bpFacilityChemical.SCCan = row["SCCan"].ToString().Trim();
									bpFacilityChemical.SCCarboy = row["SCCarboy"].ToString().Trim();
									bpFacilityChemical.SCSilo = row["SCSilo"].ToString().Trim();
									bpFacilityChemical.SCFiberDrum = row["SCFiberDrum"].ToString().Trim();
									bpFacilityChemical.SCBag = row["SCBag"].ToString().Trim();
									bpFacilityChemical.SCBox = row["SCBox"].ToString().Trim();
									bpFacilityChemical.SCCylinder = row["SCCylinder"].ToString().Trim();
									bpFacilityChemical.SCGlassBottle = row["SCGlassBottle"].ToString().Trim();
									bpFacilityChemical.SCPlasticBottle = row["SCPlasticBottle"].ToString().Trim();
									bpFacilityChemical.SCToteBin = row["SCToteBin"].ToString().Trim();
									bpFacilityChemical.SCTankTruckTankWagon = row["SCTankTruckTankWagon"].ToString().Trim();
									bpFacilityChemical.SCTankCarRailCar = row["SCTankCarRailCar"].ToString().Trim();
									bpFacilityChemical.SCOther = row["SCOther"].ToString().Trim();
									bpFacilityChemical.OtherStorageContainer = row["OtherStorageContainer"].ToString().Trim();
									bpFacilityChemical.StoragePressure = row["StoragePressure"].ToString().Trim();
									bpFacilityChemical.StorageTemperature = row["StorageTemperature"].ToString().Trim();
									bpFacilityChemical.HC1PercentByWeight = row["HC1PercentByWeight"].ToString().Trim().Equals( "" ) ? (decimal?)null : Decimal.Parse( row["HC1PercentByWeight"].ToString().Trim() );
									bpFacilityChemical.HC1Name = row["HC1Name"].ToString().Trim();
									bpFacilityChemical.HC1EHS = row["HC1EHS"].ToString().Trim();
									bpFacilityChemical.HC1CAS = row["HC1CAS"].ToString().Trim();
									bpFacilityChemical.HC2PercentByWeight = row["HC2PercentByWeight"].ToString().Trim().Equals( "" ) ? (decimal?)null : Decimal.Parse( row["HC2PercentByWeight"].ToString().Trim() );
									bpFacilityChemical.HC2Name = row["HC2Name"].ToString().Trim();
									bpFacilityChemical.HC2EHS = row["HC2EHS"].ToString().Trim();
									bpFacilityChemical.HC2CAS = row["HC2CAS"].ToString().Trim();
									bpFacilityChemical.HC3PercentByWeight = row["HC3PercentByWeight"].ToString().Trim().Equals( "" ) ? (decimal?)null : Decimal.Parse( row["HC3PercentByWeight"].ToString().Trim() );
									bpFacilityChemical.HC3Name = row["HC3Name"].ToString().Trim();
									bpFacilityChemical.HC3EHS = row["HC3EHS"].ToString().Trim();
									bpFacilityChemical.HC3CAS = row["HC3CAS"].ToString().Trim();
									bpFacilityChemical.HC4PercentByWeight = row["HC4PercentByWeight"].ToString().Trim().Equals( "" ) ? (decimal?)null : Decimal.Parse( row["HC4PercentByWeight"].ToString().Trim() );
									bpFacilityChemical.HC4Name = row["HC4Name"].ToString().Trim();
									bpFacilityChemical.HC4EHS = row["HC4EHS"].ToString().Trim();
									bpFacilityChemical.HC4CAS = row["HC4CAS"].ToString().Trim();
									bpFacilityChemical.HC5PercentByWeight = row["HC5PercentByWeight"].ToString().Trim().Equals( "" ) ? (decimal?)null : Decimal.Parse( row["HC5PercentByWeight"].ToString().Trim() );
									bpFacilityChemical.HC5Name = row["HC5Name"].ToString().Trim();
									bpFacilityChemical.HC5EHS = row["HC5EHS"].ToString().Trim();
									bpFacilityChemical.HC5CAS = row["HC5CAS"].ToString().Trim();
									bpFacilityChemical.ChemicalDescriptionComment = row["ChemicalDescriptionComment"].ToString().Trim();
									bpFacilityChemical.AdditionalMixtureComponents = row["AdditionalMixtureComponents"].ToString().Trim();
									bpFacilityChemical.USEPASRSNumber = row["USEPASRSNumber"].ToString().Trim();
									bpFacilityChemical.DOTHazClassID = row["DOTHazardClassificationID"].ToString().Trim().Equals( "" ) ? null : row["DOTHazardClassificationID"].ToString().Trim();
									bpFacilityChemical.SetDDCommonFields( creating: true );

									Repository.BPFacilityChemicals.Validate( bpFacilityChemical );
									Repository.BPFacilityChemicals.Save( bpFacilityChemical );
								}

								rowIndex++;
							}

							// Close the Uploaded Workbook
							uploadedWorkbook.Close();

							// Validate this Submittal Element to apply all other CDR Rules, and generate real
							// GuidanceMessage records
							fse.ValidateAndCommitResults( Repository, CallerContext.UI );

							// Send user back to the Hazardous Material Inventory Home View after upload
							string routeName = GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Home );
							return RedirectToRoute( routeName, new { organizationId = organizationId, CERSID = CERSID, FSEID = FSEID, FSERID = FSERID } );
						}
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

			// User will only get to this point if local Guidance Messages (errors that abort the processing
			// of the upload) were found. In this case, add those messages to the view and return them to the
			// current view (Upload). display only up to 25 error messages
			viewModel.InventoryUploadGuidanceMessages = guidanceMessages.Take( guidanceMessageTruncationLimit );
			Session["GuidanceMessages"] = guidanceMessages;
			return View( viewModel );
		}

		private void CheckIsFileValid( HttpPostedFileBase file, List<GuidanceMessage> guidanceMessages, LUTGuidanceLevel lutGuidanceLevel )
		{
			if ( file == null || file.ContentLength == 0 )
			{
				Repository.GuidanceMessages.AddGuidanceMessage( guidanceMessages, lutGuidanceLevel, "Uploaded inventory worksheet was not specified, or is an empty file." );
			}
			if ( file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" && !Request.Browser.Type.Contains( "IE" ) )
			{
				Repository.GuidanceMessages.AddGuidanceMessage( guidanceMessages, lutGuidanceLevel, "Uploaded inventory worksheet is not a valid excel worksheet." );
			}
		}

		private void CheckIsFormatValid( DataColumnCollection cols, List<GuidanceMessage> guidanceMessages, LUTGuidanceLevel lutGuidanceLevel )
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

		#endregion Upload_HazardousMaterialInventory

		#region Download_HazardousMaterialInventory

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Download_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			HazardousMaterialInventoryViewModel<BPFacilityChemical> viewModel = new HazardousMaterialInventoryViewModel<BPFacilityChemical>();
			viewModel.FacilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			viewModel.FacilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID );
			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		[HttpPost]
		public void Download_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID, FormCollection formCollection )
		{
			Download_HazardousMaterialInventory_ToXls( organizationId, CERSID, FSERID );
		}

		public void Download_HazardousMaterialInventory_ToXls( int organizationId, int CERSID, int FSERID )
		{
			string excelTemplateFilePath = System.IO.Path.Combine( Server.MapPath( "~" ), @"Content\TemplateFiles\HazMatInventoryTemplate.xlsx" );
			var workbook = Services.Excel.GenerateHazardousMaterialInventory( excelTemplateFilePath, CERSID, FSERID );

			SetDownloadFileHeader( "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CERSID_" + CERSID + "_HazMatInventory.xlsx" );
			workbook.Save( Response.OutputStream );
			Response.End();
		}

		#endregion Download_HazardousMaterialInventory

		#region Report_HazardousMaterialInventory

		public void GenerateHMISMatrixReport( IEnumerable<BPFacilityChemical> dataSource, int FSERID, int? TypeID = (int)HMISReportType.Waste, string chemicalLocation = "All Locations" )
		{
			Services.Reports.HMIMatrixReportBLL( dataSource, FSERID, TypeID, chemicalLocation );
		}

		public HMISReportViewModel PopulateHMIMatrixViewModel( int organizationId, int CERSID, int FSEID, int FSERID, int? typeID = null )
		{
			List<HMISReportViewModel.DropDownListViewModel> materialTypes = new List<HMISReportViewModel.DropDownListViewModel>();
			materialTypes.Add( new HMISReportViewModel.DropDownListViewModel { ID = 0, Name = "All" } );
			materialTypes.Add( new HMISReportViewModel.DropDownListViewModel { ID = (int)HMISReportType.Waste, Name = "Waste" } );
			materialTypes.Add( new HMISReportViewModel.DropDownListViewModel { ID = (int)HMISReportType.NonWaste, Name = "Non-Waste" } );

			List<HMISReportViewModel.DropDownListViewModel> reportTemplates = new List<HMISReportViewModel.DropDownListViewModel>();
			reportTemplates.Add( new HMISReportViewModel.DropDownListViewModel { ID = 0, Name = "CERS 2 Inventory Matrix Report" } );

			var locationList = Repository.BPFacilityChemicals.GetFacilityChemicalLocationsList( CERSID, fserID: FSERID );
			List<HMISReportViewModel.DropDownStringListViewModel> chemicalLocations = new List<HMISReportViewModel.DropDownStringListViewModel>();
			chemicalLocations.Add( new HMISReportViewModel.DropDownStringListViewModel { Value = "All Locations", Name = "All Locations" } );
			chemicalLocations.AddRange( from location in locationList
										orderby location
										select new HMISReportViewModel.DropDownStringListViewModel
										{
											Value = location ?? String.Empty,
											Name = String.IsNullOrWhiteSpace( location ) ? "[Not Specified]" : ( location.Length > 60 ? location.Substring( 0, 56 ) + " ..." : location ),
										} );

			List<HMISReportViewModel.DropDownListViewModel> sortOptions = new List<HMISReportViewModel.DropDownListViewModel>();
			sortOptions.Add( new HMISReportViewModel.DropDownListViewModel { ID = 1, Name = "Common Name" } );
			sortOptions.Add( new HMISReportViewModel.DropDownListViewModel { ID = 2, Name = "DOT Code" } );

			HMISReportViewModel viewModel = new HMISReportViewModel()
			{
				OrganizationID = organizationId,
				CERSID = CERSID,
				FSEID = FSEID,
				FSERID = FSERID,
				TypeID = typeID ?? 0,

				MaterialTypes = materialTypes,
				ChemicalLocations = chemicalLocations,
				ReportTemplates = reportTemplates,
				SortOptions = sortOptions,
				IncludeTradeSecret = true,

				FacilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID ),
				FacilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID ),
				ItemGroups = new List<HMISReportViewModel.DropDownListViewModel>(),
			};

			return viewModel;
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Report_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID, int? TypeID = null )
		{
			HazardousMaterialInventoryViewModel<BPFacilityChemical> viewModel = new HazardousMaterialInventoryViewModel<BPFacilityChemical>();
			viewModel.FacilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			viewModel.FacilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID );

			viewModel.HMISReport = PopulateHMIMatrixViewModel( organizationId, CERSID, FSEID, FSERID, TypeID );

			return View( viewModel );
		}

		[Obsolete]
		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		[HttpPost]
		public ActionResult Report_HazardousMaterialInventory( FormCollection form )
		{
			int organizationID = int.Parse( form["organizationID"] );
			int CERSID = int.Parse( form["CERSID"] );
			int FSEID = int.Parse( form["FSEID"] );
			int FSERID = int.Parse( form["FSERID"] );
			int TypeID = int.Parse( form["TypeID"] );

			HazardousMaterialInventoryViewModel<BPFacilityChemical> viewModel = new HazardousMaterialInventoryViewModel<BPFacilityChemical>();
			viewModel.FacilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			viewModel.FacilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID );

			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Report_HazardousMaterialInventoryMatrix( int organizationId, int CERSID, int FSEID, int FSERID, int? typeID = null )
		{
			HMISReportViewModel viewModel = PopulateHMIMatrixViewModel( organizationId, CERSID, FSEID, FSERID, typeID );

			return View( viewModel );
		}

		[HttpPost]
		public ActionResult Report_HazardousMaterialInventoryMatrix_Async( int fserID, int typeID, string chemicalLocation, string DOTHazClassID, bool includeTradeSecret, int sortOption )
		{
			int materialCount = 0;
			if ( fserID != 0 )
			{
				#region retrieve and filter report data

				var bpChemicals = Repository.BPFacilityChemicals.GetByFserID( fserID );
				if ( typeID == (int)HMISReportType.Waste )
				{
					bpChemicals = from c in bpChemicals where c.HMType == "c" select c;
				}
				else if ( typeID == (int)HMISReportType.NonWaste )
				{
					bpChemicals = from c in bpChemicals where c.HMType != "c" select c;
				}

				if ( chemicalLocation != "All Locations" )
				{
					bpChemicals = from c in bpChemicals where ( c.ChemicalLocation ?? String.Empty ) == chemicalLocation select c;
				}

				if ( !String.IsNullOrWhiteSpace( DOTHazClassID ) )
				{
					bpChemicals = from c in bpChemicals where c.DOTHazClassID == DOTHazClassID select c;
				}

				if ( !includeTradeSecret )
				{
					bpChemicals = from c in bpChemicals where c.TradeSecret != "Y" select c;
				}
				materialCount = bpChemicals.Count();

				#endregion retrieve and filter report data
			}

			#region generate html injection

			string returnHtml = "<tr>";

			if ( materialCount == 0 )
			{
				returnHtml += "<td class=\"error\">No Material Found!</td>";
			}
			else
			{
				int LINKS_PER_ROW = 5;
				int maxThreshold = Repository.Settings.GetMaxHMIFileThreshold();
				int numberOfLinks = (int)Math.Ceiling( (decimal)materialCount / maxThreshold );
				for ( int link = 1; link <= numberOfLinks; link++ )
				{
					string hrefID = String.Format( "Matrix_{0}", link );
					string hrefDisplay = String.Format( "Materials {0} - {1}", ( ( link - 1 ) * maxThreshold ) + 1, ( link * maxThreshold ) > materialCount ? materialCount : link * maxThreshold );
					string href = String.Format( Url.Action( "Report_HazardousMaterialInventoryMatrix_Click" )
						+ "?fserID={0}&typeID={1}&chemicalLocation={2}&DOTHazClassID={3}&includeTradeSecret={4}&sortOption={5}&reportTemplate={6}&materialLink={7}"
                        , fserID, typeID, HttpUtility.UrlEncode( chemicalLocation ), HttpUtility.UrlEncode( DOTHazClassID ), ( includeTradeSecret ? "true" : "false" ), sortOption, 0, link );

					returnHtml += "<td style=\"width:165px\">";
					returnHtml += String.Format( "<a class=\"smallButton smallButtonActive\" href=\"{0}\" id=\"{1}\"><span><span style=\"width:135px; text-align:center\">{2}</span></span></a>", href, hrefID, hrefDisplay );
					returnHtml += "</td>";

					if ( link % LINKS_PER_ROW == 0 )
					{
						returnHtml += "</tr><tr style=\"height:20px\">";
					}
				}
			}
			returnHtml += "</tr>";

			#endregion generate html injection

			return Json( returnHtml );
		}

		public void Report_HazardousMaterialInventoryMatrix_Click( int fserID, int typeID, string chemicalLocation, string DOTHazClassID, bool includeTradeSecret, int sortOption, int materialLink = 1 )
		{
			if ( fserID != 0 )
			{
				#region retrieve and filter report data

				var bpChemicals = Repository.BPFacilityChemicals.GetByFserID( fserID );
				if ( typeID == (int)HMISReportType.Waste )
				{
					bpChemicals = from c in bpChemicals where c.HMType == "c" select c;
				}
				else if ( typeID == (int)HMISReportType.NonWaste )
				{
					bpChemicals = from c in bpChemicals where c.HMType != "c" select c;
				}

				if ( chemicalLocation != "All Locations" )
				{
					bpChemicals = from c in bpChemicals where ( c.ChemicalLocation ?? String.Empty ) == chemicalLocation select c;
				}

				if ( !String.IsNullOrWhiteSpace( DOTHazClassID ) )
				{
					bpChemicals = from c in bpChemicals where c.DOTHazClassID == DOTHazClassID select c;
				}

				if ( !includeTradeSecret )
				{
					bpChemicals = from c in bpChemicals where c.TradeSecret != "Y" select c;
				}

				var bpChemList = bpChemicals.ToList();
				if ( sortOption == 1 ) //Chemical Location, Common Name
				{
					bpChemicals = from c in bpChemicals orderby c.ChemicalLocation, c.CommonName select c;
				}
				else //Chemical Location, DOT Hazard Code
				{
					bpChemicals = from c in bpChemicals orderby c.ChemicalLocation, c.DOTHazClassID select c;
				}

				if ( materialLink != 0 )
				{
					int maxThreshold = Repository.Settings.GetMaxHMIFileThreshold();
					int recordToSkip = ( materialLink - 1 ) * maxThreshold;
					bpChemicals = bpChemicals.Skip( recordToSkip ).Take( maxThreshold );
				}

				#endregion retrieve and filter report data

				GenerateHMISMatrixReport( bpChemicals, fserID, typeID );
			}
		}

		#region HMIS Reports

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult HMISReportHome( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return RedirectToRoute( GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.Report ), new { organizationId = organizationId, CERSID = CERSID, FSEID = FSEID, FSERID = FSERID } );
		}

		#endregion HMIS Reports

		#endregion Report_HazardousMaterialInventory

		#region Validate_HazardousMaterialInventory

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Validate_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			var fser = Repository.FacilitySubmittalElementResources.GetByID( FSERID );
			fser.FacilitySubmittalElement.ValidateAndCommitResults( Repository, CallerContext.UI );
			Messages.Clear();
			Messages.Add( "Your inventory was validated. Please review any guidance message icons.", MessageType.Success, "HazMatInventory" );
			return RedirectToAction( "Home_HazardousMaterialInventory" );
		}

		#endregion Validate_HazardousMaterialInventory

		#region Template_HazardousMaterialInventory

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Template_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			HazardousMaterialInventoryViewModel<BPFacilityChemical> viewModel = new HazardousMaterialInventoryViewModel<BPFacilityChemical>();
			viewModel.FacilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			viewModel.FacilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID );
			return View( viewModel );
		}

		#endregion Template_HazardousMaterialInventory

		#region Library_HazardousMaterialInventory

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Library_HazardousMaterialInventory( int organizationId, int CERSID, int FSEID, int FSERID, int? CCLID )
		{
			HazardousMaterialInventoryViewModel<BPFacilityChemical> viewModel = new HazardousMaterialInventoryViewModel<BPFacilityChemical>();
			viewModel.FacilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );
			viewModel.FacilitySubmittalElementResource = Repository.FacilitySubmittalElementResources.GetByID( FSERID );
			return View( viewModel );
		}

		#endregion Library_HazardousMaterialInventory

		#endregion Hazardous Material Inventory

		#region Supplemental Document Actions

		//#region General Site Map

		//#region Detail_GeneralSiteMap_Document

		//[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		//public ActionResult Detail_GeneralSiteMap_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		//{
		//	return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.GeneralSiteMap_Document, FSEID );
		//}

		//#endregion Detail_GeneralSiteMap_Document

		//#region Create_GeneralSiteMap_Document

		//[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		//public ActionResult Create_GeneralSiteMap_Document( int organizationId, int CERSID )
		//{
		//	return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.GeneralSiteMap_Document );
		//}

		//[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		//[HttpPost]
		//public ActionResult Create_GeneralSiteMap_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		//{
		//	return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.GeneralSiteMap_Document, viewModel );
		//}

		//#endregion Create_GeneralSiteMap_Document

		//#region Edit_GeneralSiteMap_Document

		//[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		//public ActionResult Edit_GeneralSiteMap_Document( int organizationId, int CERSID, int FSEID )
		//{
		//	return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.GeneralSiteMap_Document );
		//}

		//[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		//[HttpPost]
		//public ActionResult Edit_GeneralSiteMap_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		//{
		//	return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.GeneralSiteMap_Document, viewModel );
		//}

		//#endregion Edit_GeneralSiteMap_Document

		//#region Delete_GeneralSiteMap_Document

		//[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		//public ActionResult Delete_GeneralSiteMap_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		//{
		//	return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
		//}

		//#endregion Delete_GeneralSiteMap_Document

		//#endregion Supplemental Document Actions

		#region Annotated Site Map

		#region Detail_AnnotatedSiteMapOfficialUseOnly_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_AnnotatedSiteMapOfficialUseOnly_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.AnnotatedSiteMapOfficialUseOnly_Document, FSEID );
		}

		#endregion Detail_AnnotatedSiteMapOfficialUseOnly_Document

		#region Create_AnnotatedSiteMapOfficialUseOnly_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_AnnotatedSiteMapOfficialUseOnly_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.AnnotatedSiteMapOfficialUseOnly_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_AnnotatedSiteMapOfficialUseOnly_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.AnnotatedSiteMapOfficialUseOnly_Document, viewModel );
		}

		#endregion Create_AnnotatedSiteMapOfficialUseOnly_Document

		#region Edit_AnnotatedSiteMapOfficialUseOnly_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_AnnotatedSiteMapOfficialUseOnly_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.AnnotatedSiteMapOfficialUseOnly_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_AnnotatedSiteMapOfficialUseOnly_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.AnnotatedSiteMapOfficialUseOnly_Document, viewModel );
		}

		#endregion Edit_AnnotatedSiteMapOfficialUseOnly_Document

		#region Delete_AnnotatedSiteMapOfficialUseOnly_Document

		public ActionResult Delete_AnnotatedSiteMapOfficialUseOnly_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_AnnotatedSiteMapOfficialUseOnly_Document

		#endregion Annotated Site Map

		#region Hazardous Material MaterialInventory Locally-Required Document

		#region Detail_HazardousMaterialInventoryLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_HazardousMaterialInventoryLocallyRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventoryLocallyRequired_Document, FSEID );
		}

		#endregion Detail_HazardousMaterialInventoryLocallyRequired_Document

		#region Create_HazardousMaterialInventoryLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_HazardousMaterialInventoryLocallyRequired_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventoryLocallyRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_HazardousMaterialInventoryLocallyRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventoryLocallyRequired_Document, viewModel );
		}

		#endregion Create_HazardousMaterialInventoryLocallyRequired_Document

		#region Edit_HazardousMaterialInventoryLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_HazardousMaterialInventoryLocallyRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventoryLocallyRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_HazardousMaterialInventoryLocallyRequired_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventoryLocallyRequired_Document, viewModel );
		}

		#endregion Edit_HazardousMaterialInventoryLocallyRequired_Document

		#region Delete_HazardousMaterialInventoryLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_HazardousMaterialInventoryLocallyRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_HazardousMaterialInventoryLocallyRequired_Document

		#endregion Hazardous Material MaterialInventory Locally-Required Document

		#region Hazardous Material MaterialInventory Misc-State Required Document

		#region Detail_HazardousMaterialInventoryLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Detail_HMIMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.HMIMiscellaneousStateRequired_Document, FSEID );
		}

		#endregion Detail_HazardousMaterialInventoryLocallyRequired_Document

		#region Create_HazardousMaterialInventoryLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Create_HMIMiscellaneousStateRequired_Document( int organizationId, int CERSID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.HMIMiscellaneousStateRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Create_HMIMiscellaneousStateRequired_Document( int organizationId, int CERSID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.HMIMiscellaneousStateRequired_Document, viewModel );
		}

		#endregion Create_HazardousMaterialInventoryLocallyRequired_Document

		#region Edit_HazardousMaterialInventoryLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Edit_HMIMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID )
		{
			return Handle_DocumentUploadGet( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.HMIMiscellaneousStateRequired_Document );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		[HttpPost]
		public ActionResult Edit_HMIMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, DocumentUploadViewModel viewModel )
		{
			return Handle_DocumentUploadPost( organizationId, CERSID, SubmittalElementType.HazardousMaterialsInventory, ResourceType.HMIMiscellaneousStateRequired_Document, viewModel );
		}

		#endregion Edit_HazardousMaterialInventoryLocallyRequired_Document

		#region Delete_HazardousMaterialInventoryLocallyRequired_Document

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgEditor )]
		public ActionResult Delete_HMIMiscellaneousStateRequired_Document( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			return Handle_DocumentDelete( organizationId, CERSID, FSEID, FSERID );
		}

		#endregion Delete_HazardousMaterialInventoryLocallyRequired_Document

		#endregion Hazardous Material MaterialInventory Misc-State Required Document

		#endregion Supplemental Document Actions

		#region Controllers for UI Layout/Formatting

		#endregion Controllers for UI Layout/Formatting

		#region Ajax GridAction

        public JsonResult Search_GridBindingBPFacilityChemicals( [DataSourceRequest]DataSourceRequest request, int CERSID, int FSEID, int FSERID, string name, string location, string casNumber, bool? showRecordsWithGuidanceOnly = null )
		{
            var entities = Repository.BPFacilityChemicals.GridSearch( CERSID:CERSID, FSEID:FSEID, FSERID:FSERID, materialName:name, location:location, CASNumber:casNumber, baseContentImagePath:Url.Content( "~/Content/Bliss/Images/Icons" ), showRecordsWithGuidanceOnly:showRecordsWithGuidanceOnly );

            return Json( entities.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
		}

        public ActionResult Search_GridChemicals_Async( [DataSourceRequest]DataSourceRequest request, string chemicalName, string casNumber, string cclFQID, int? CERSID, string source, StringSearchOption chemicalNameSearchOption = StringSearchOption.StartsWith, bool excludeSynonyms = false, bool excludeMixtures = false )
		{
			// Default Chemical Search to Empty IEnumerable
			IEnumerable<ChemicalLibrarySearchResult> entities = new List<ChemicalLibrarySearchResult>();

			// Only perform the search if they search by at least one parameter -
			if ( !string.IsNullOrWhiteSpace( chemicalName ) || !string.IsNullOrWhiteSpace( casNumber ) || !string.IsNullOrWhiteSpace( cclFQID ) )
			{
				if ( source == "ChemicalLibrary" )
				{
					entities = Repository.Chemicals.ChemicalGridGenericSearch( chemicalNameSearchOption: chemicalNameSearchOption, chemicalName: chemicalName, casNumber: casNumber, CCLFQID: cclFQID, source: source );
					if ( excludeSynonyms )
					{
						entities = entities.Where( e => e.ChemicalNameTypeID != (int)ChemicalNameType.Synonym );
					}
					if ( excludeMixtures )
					{
						entities = entities.Where( e => ( e.HMType ?? string.Empty ) != "b" );	//b=mixture
					}
				}
				if ( source == "ThisFacility" )
				{
					entities = Repository.Chemicals.ChemicalGridGenericSearch( chemicalNameSearchOption: chemicalNameSearchOption, chemicalName: chemicalName, casNumber: casNumber, CCLFQID: cclFQID, CERSID: CERSID ?? CurrentCERSID, source: source );
				}
				if ( source == "AnyFacility" )
				{
					entities = Repository.Chemicals.ChemicalGridGenericSearch( chemicalNameSearchOption: chemicalNameSearchOption, chemicalName: chemicalName, casNumber: casNumber, CCLFQID: cclFQID, CERSID: CERSID, organizationID: CurrentOrganizationID, source: source );
				}
			}
            DataSourceResult result = entities.ToList().ToDataSourceResult( request );
            return Json( result, JsonRequestBehavior.AllowGet );
		}

		#endregion Ajax GridAction
	}
}