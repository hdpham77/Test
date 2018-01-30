using CERS.Model;
using CERS.Repository;
using CERS.ViewModels;
using CERS.ViewModels.Regulators;
using CERS.Web.UI.Public.ViewModels;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using UPF;

namespace CERS.Web.UI.Public.Controllers
{
	public class DirectoryController : AppControllerBase
	{
		public ActionResult CUPAEvaluationDocuments()
		{
			RegulatorDocumentViewModel regulatorDocumentViewModel = SystemViewModelData.BuildRegulatorDocumentViewModel( true );

			regulatorDocumentViewModel.Regulators = Repository.Regulators.Search();
			regulatorDocumentViewModel.RegulatorDocumentTypes = Repository.SystemLookupTables.GetValues( SystemLookupTable.RegulatorDocumentType );

			return View( regulatorDocumentViewModel );
		}

		public JsonResult CUPAEvaluationDocuments_GridRead( [DataSourceRequest] DataSourceRequest request, int? regulatorID = null, string year = null )
		{
			var data = Repository.RegulatorDocuments.GridSearch( regulatorID, year );

			//limit the documents listed to be evaluation related.
			data = from d in data where d.TypeName.Contains( "evaluation" ) select d;

			return Json( data.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
		}

		public ActionResult EnforcementSummaries()
		{
			RegulatorDocumentViewModel regulatorDocumentViewModel = SystemViewModelData.BuildRegulatorDocumentViewModel( true );
			regulatorDocumentViewModel.Regulators = Repository.Regulators.Search();
			return View( regulatorDocumentViewModel );
		}

		public ActionResult EnforcementSummariesDocuments_GridRead( [DataSourceRequest] DataSourceRequest request, int? regulatorID, string year, string keyword )
		{
			var data = Repository.RegulatorDocuments.GridSearch( regulatorID, year, keyword, RegulatorDocumentType.Enforcement_Summary );

			return Json( data.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
		}

		public ActionResult Index()
		{
			RegulatorDirectorySearchViewModel regulatorDirectorySearchViewModel = new ViewModels.RegulatorDirectorySearchViewModel()
			{
				Cities = Repository.Places.Search().OrderBy( o => o.Name ).Select( p => p.Name ),
				ZipCodes = Repository.ZipCodes.Search().OrderBy( o => o.ZipCodeID ).Select( p => p.ZipCodeID.ToString() ),
				RegulatorTypes = Repository.SystemLookupTables.GetValues( SystemLookupTable.RegulatorType ).Where( p => p.ID != 4 && p.ID != 5 ),
				Counties = Repository.Counties.GetAll(),
			};

			return View( regulatorDirectorySearchViewModel );
		}

		[HttpGet]
		public ActionResult OpenDocument( int id, string title )
		{
			Document document = Repository.Documents.GetByID( id );
			var docBytes = DocumentStorage.GetBytes( document.Location );
			string contentType = IOHelper.GetContentType( document.Location );
			string ext = Path.GetExtension( document.Location ).ToLower();

			FileContentResult result = null;

			if ( docBytes != null )
			{
				result = File( docBytes, contentType, title + ext );
			}
			return result;
		}

        public ActionResult PublicRegulatorContacts( [DataSourceRequest] DataSourceRequest request, int regulatorID )
		{
			var contacts = Repository.RegulatorContacts.GridSearch( regulatorID: regulatorID, isPublic: true );
            return Json( contacts.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
		}

		public ActionResult RegulatorDetails( int regulatorID )
		{
			RegulatorViewModel viewModel = new RegulatorViewModel
			{
				Entity = Repository.Regulators.GetByID( regulatorID ),
				RegulatorContacts = Repository.RegulatorContacts.Search( regulatorID ),
				RelatedRegulators = Repository.Regulators.GetRelatedRegulatorsGridView( regulatorID )
			};

			return View( viewModel );
		}

		public ActionResult RegulatorDocuments( int regulatorID )
		{
			RegulatorViewModel viewModel = new RegulatorViewModel
			{
				Entity = Repository.Regulators.GetByID( regulatorID )
			};

			return View( viewModel );
		}

		public JsonResult RegulatorDocuments_GridRead( [DataSourceRequest] DataSourceRequest request, int regulatorID )
		{
			var data = Repository.RegulatorDocuments.GridSearch( regulatorID );

			return Json( data.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
		}

		public ActionResult Regulators_GridRead( [DataSourceRequest] DataSourceRequest request, string address = null, string city = null, string zipCode = null, int? countyID = null, int? typeID = null )
		{
			IQueryable<RegulatorGridViewModel> data = null;
			if ( string.IsNullOrWhiteSpace( address ) && !string.IsNullOrWhiteSpace( city ) && !string.IsNullOrWhiteSpace( zipCode ) )
			{
				var addressInformation = Services.Geo.GetAddressInformation( address, city, zipCode );

				var facility = Repository.Facilities.GetByExactAddress( street: addressInformation.Street, city: addressInformation.City, zipCode: addressInformation.ZipCode ).FirstOrDefault();
				if ( facility != null )
				{
					data = Repository.Regulators.GetGridViewForFacility( facility );
					return Json( data.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
				}
			}

			RegulatorType? regulatorType = null;
			if ( typeID != null )
			{
				regulatorType = (RegulatorType)typeID.Value;
			}

			data = Repository.Regulators.GridSearch( type: regulatorType, zipCode: zipCode, countyID: countyID, zipCodeSearchType: ZipCodeSearchType.ZipCode );

			data = from d in data where d.Type != "4" && d.Type != "5" select d;

			return Json( data.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
		}

		public JsonResult RelatedRegulators_GridRead( [DataSourceRequest]DataSourceRequest request, int regulatorID )
		{
			var data = Repository.Regulators.GetRelatedRegulatorsGridView( regulatorID );
			return Json( data.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
		}

		public ActionResult UPAListing()
		{
			RegulatorDocumentViewModel regulatorDocumentViewModel = SystemViewModelData.BuildRegulatorDocumentViewModel( true );
			regulatorDocumentViewModel.RegulatorTypes = regulatorDocumentViewModel.RegulatorTypes.Where( p => p.ID != 4 && p.ID != 5 );
			regulatorDocumentViewModel.Counties = Repository.Counties.GetAll();
			return View( regulatorDocumentViewModel );
		}
	}
}