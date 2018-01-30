using CERS.Compositions;
using CERS.Model;
using CERS.ViewModels;
using CERS.ViewModels.Infrastructure;
using CERS.Web.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UPF;

namespace CERS.Web.UI.Organization.Controllers
{
	public class HelpController : HelpControllerBase
	{
		//
		// GET: /Help/

		public ActionResult DemoHMIBPFacilityChemicalGridJSON()
		{
			var entities = Repository.BPFacilityChemicals.GridSearch( FSERID: 1588460 );
			entities = entities.Where( p => p.ID == 2208505 );

			return Json( entities, JsonRequestBehavior.AllowGet );
		}

		public ActionResult FieldHelp( decimal dataRegistryID )
		{
			FieldHelpViewModel viewModel = Services.ViewModels.LoadFieldHelp( dataRegistryID );

			return PartialView( viewModel );
		}

		public ActionResult Index()
		{
			CERSHelpItemViewModel viewModel = new CERSHelpItemViewModel();

			viewModel.CERSHelpItemGridView = Repository.HelpItems.GridSearch(/*helpPortalTypeID: 1*/ fromCrudPage: false );
			return View( viewModel );
		}

		[HttpGet]
		public ActionResult OpenDocument( int id )
		{
			Document document = Repository.Documents.GetByID( id );
			var docBytes = DocumentStorage.GetBytes( document.Location );
			string contentType = IOHelper.GetContentType( document.Location );
			string ext = Path.GetExtension( document.Location ).ToLower();

			string title = ( from hi in document.HelpItems where hi.DocumentID == id select hi.Title ).FirstOrDefault();
			if ( String.IsNullOrEmpty( title ) )
			{
				title = "HelpItem";
			}

			if ( docBytes == null )
			{
				return RedirectToAction( "DocumentNotFound", "Error" );
			}

			return File( docBytes, contentType, title + ext );
		}

		public ActionResult PrintingHelp()
		{
			return View();
		}
	}
}