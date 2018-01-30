using CERS.ViewModels.Infrastructure.DataRegistry;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UPF;

namespace CERS.Web.UI.Public.Controllers
{
	public class DataRegistryController : AppControllerBase
	{
		//
		// GET: /DataRegistry/

		public ActionResult ElementDetail( int elementID )
		{
			DataRegistryElementViewModel viewModel = new DataRegistryElementViewModel();
			viewModel.Entity = Repository.DataRegistry.DataElements.GetByID( elementID );
			return View( viewModel );
		}

		public ActionResult ElementDetailByDataSourceKeyAndIdentifier( string dataSourceKey, string identifier )
		{
			DataRegistryElementViewModel viewModel = new DataRegistryElementViewModel();
			viewModel.Entity = Repository.DataRegistry.DataElements.FindByDataSourceKey( dataSourceKey, identifier );
			return View( "ElementDetail", viewModel );
		}

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Search( int? dataSourceID = null )
		{
			DataRegistrySearchViewModel viewModel = new DataRegistrySearchViewModel();
			viewModel.DataSourceID = dataSourceID;
			return View( viewModel );
		}

		[HttpPost]
		public ActionResult Search( DataRegistrySearchViewModel viewModel, FormCollection form, string commandParameter )
		{
			if ( viewModel == null )
			{
				viewModel = new DataRegistrySearchViewModel();
			}

			if ( !string.IsNullOrWhiteSpace( commandParameter ) && commandParameter.ToLower().Trim().Contains( "export" ) )
			{
				var data = Repository.DataRegistry.DataElements.GridSearch( viewModel.DataSourceID, viewModel.DataContainerID, viewModel.DataContainerTableID, viewModel.Keywords, viewModel.XMLTag, viewModel.Identifier, viewModel.RegistryFieldNumber );
				//lets make a filename based on the dictionary name and the date.
				DateTime fSuffix = DateTime.Now;
				string fileName = "";
				if ( viewModel.DataSourceID.HasValue )
				{
					var dataSource = Repository.DataRegistry.DataSources.GetByID( viewModel.DataSourceID.Value );
					if ( dataSource != null )
					{
						fileName += dataSource.Acronym + "_";
					}
				}
				fileName += fSuffix.Month + "-" + fSuffix.Day + "-" + fSuffix.Year;
				fileName = "CDR_" + fileName + ".xlsx";

				ExcelColumnMappingCollection mappings = new ExcelColumnMappingCollection();
				mappings.Add( "Identifier", "Identifier" );
				mappings.Add( "Section", "Section" );
				mappings.Add( "FieldName", "Field Name" );
				mappings.Add( "RegistryFieldNumber", "Registry Field Number" );
				mappings.Add( "CodeArray", "Codes/Criteria" );
				mappings.Add( "Length", "Length" );
				mappings.Add( "Type", "Type" );
				mappings.Add( "Description", "Description" );
				mappings.Add( "XMLTag", "XML Tag" );
				mappings.Add( "FieldFormat", "Field Format" );
                mappings.Add("Obsolete", "Obsolete");

                ExportToExcel( fileName, data, mappings );
			}

			return View( viewModel );
		}

		public JsonResult Search_GridRead( [DataSourceRequest] DataSourceRequest request, int? dataSourceID = null, int? dataContainerID = null, int? dataContainerTableID = null, string keywords = null, string xmlTag = null, string identifier = null, string registryFieldNumber = null )
		{
			var data = Repository.DataRegistry.DataElements.GridSearch( dataSourceID, dataContainerID, dataContainerTableID, keywords, xmlTag, identifier, registryFieldNumber );

			return Json( data.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
		}
	}
}