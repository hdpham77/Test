using CERS.Model;
using CERS.ViewModels.Chemicals;
using CERS.ViewModels.Violations;
using CERS.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using UPF;
using Winnovative.ExcelLib;

namespace CERS.Web.UI.Organization.Controllers
{
	public class PublicController : AppControllerBase
	{
		//
		// GET: /Public/

		public ActionResult BrowserInfo( bool sendEmail = false )
		{
			if ( sendEmail && User.Identity.IsAuthenticated )
			{
				StringBuilder body = new StringBuilder();
				body.AppendLine( "On " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToShortTimeString() + ", " + CurrentContact.FullName + " (" + CurrentContact.Email + " ) sent Browser/Diagnostic information. " );
				body.AppendLine( "AccountID: " + CurrentContact.AccountID.Value );
				body.AppendLine( "" );
				body.AppendLine( "Browser Type: " + Request.Browser.Type );
				body.AppendLine( "Browser Name: " + Request.Browser.Browser );
				body.AppendLine( "Version: " + Request.Browser.Version );
				body.AppendLine( "Mobile Device: " + ( Request.Browser.IsMobileDevice ? "Yes" : "No" ) );
				body.AppendLine( "Major Version: " + Request.Browser.MajorVersion );
				body.AppendLine( "Platform: " + Request.Browser.Platform );
				body.AppendLine( "" );
				body.AppendLine( "Your Browsers' User Agent string as received from our servers:" );
				body.AppendLine( Request.UserAgent ).AppendLine( "" );
				body.AppendLine( "Your IP Address: " + Request.UserHostAddress );

				Services.Emails.Send( "cers@calepa.ca.gov", "CERS Diagnostic Information for " + CurrentContact.FullName + " (" + CurrentContact.Email + " )", body.ToString(), isHtml: false, contactID: CurrentContact.ID );
				return RedirectPermanent( Url.CERSRouteUrl( CommonRoute.Switchboard ) );
			}

			return View();
		}

		public ActionResult ConditionsOfUse()
		{
			return View();
		}

		public ActionResult Contact()
		{
			return View();
		}

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult PrivacyPolicy()
		{
			return View();
		}

		public ActionResult UploadPolicy()
		{
			return View();
		}

		#region Chemical Library

		public ActionResult ChemicalDetail( int CID )
		{
			ChemicalViewModel viewModel = new ChemicalViewModel();
			viewModel.Entity = Repository.Chemicals.GetByID( CID );
			return View( viewModel );
		}

		public ActionResult Chemicals( string name, string casNumber, string cclFQID, bool export = false, bool exportAll = false )
		{
			List<ChemicalViewModel.DropDownListViewModel> searchOptions = new List<ChemicalViewModel.DropDownListViewModel>();
			searchOptions.Add( new ChemicalViewModel.DropDownListViewModel { ID = (int) StringSearchOption.StartsWith, Name = "Starts with" } );
			searchOptions.Add( new ChemicalViewModel.DropDownListViewModel { ID = (int) StringSearchOption.ExactMatch, Name = "Exact Match" } );
			searchOptions.Add( new ChemicalViewModel.DropDownListViewModel { ID = (int) StringSearchOption.Contains, Name = "Contains" } );
			searchOptions.Add( new ChemicalViewModel.DropDownListViewModel { ID = (int) StringSearchOption.EndsWith, Name = "Ends with" } );

			ChemicalViewModel viewModel = new ChemicalViewModel();
			viewModel.Entity = new Chemical();
			viewModel.Entities = new List<Chemical>().ToList();
			viewModel.ChemicalLibraryEntries = new List<ChemicalLibraryEntry>().ToList();
			viewModel.ChemicalLibrarySearchResults = new List<ChemicalLibrarySearchResult>().ToList();
			viewModel.SearchOptions = searchOptions;
			viewModel.ChemicalNameSearchOption = StringSearchOption.StartsWith;
			//if (!string.IsNullOrWhiteSpace(name) ||
			//	!string.IsNullOrWhiteSpace(casNumber) ||
			//	!string.IsNullOrWhiteSpace(cclFQID))
			//{
			//	// Search based on user-input
			//	viewModel.ChemicalLibrarySearchResults = Repository.Chemicals.SearchLibrary( name: name, casNumber: casNumber, cclFQID: cclFQID);
			//}

			return View( viewModel );
		}

		//obsolete
		//[HttpPost]
		//public ActionResult Chemicals(ChemicalViewModel viewModel, FormCollection formCollection)
		//{
		//	// Default Chemical Search to Empty IEnumerable

		//	viewModel.Entities = new List<Chemical>().ToList();
		//	viewModel.ChemicalLibraryEntries = new List<ChemicalLibraryEntry>().ToList();
		//	viewModel.ChemicalLibrarySearchResults = new List<ChemicalLibrarySearchResult>().ToList();

		//	// Only perform the search if they search by at least one parameter -
		//	if (!string.IsNullOrWhiteSpace(viewModel.Entity.ChemicalName) ||
		//		!string.IsNullOrWhiteSpace(viewModel.Entity.CAS) ||
		//		!string.IsNullOrWhiteSpace(viewModel.Entity.CCLFQID))
		//	{
		//		// Search based on user-input
		//		//viewModel.ChemicalLibraryEntries = Repository.Chemicals.SearchLibrary(name: viewModel.Entity.ChemicalName, casNumber: viewModel.Entity.CAS, cclFQID: viewModel.Entity.CCLFQID);
		//		viewModel.ChemicalLibrarySearchResults = Repository.Chemicals.SearchLibrary(name: viewModel.Entity.ChemicalName, casNumber: viewModel.Entity.CAS, cclFQID: viewModel.Entity.CCLFQID);
		//	}

		//	return View(viewModel);
		//}

		public void Download_Chemicals( string name = "", string casNumber = "", string cclFQID = "", bool excludeSynonyms = false, bool excludeMixtures = false )
		{
			var chemicalGridView = Repository.Chemicals.GridSearch( name: name, casNumber: casNumber, cclFQID: cclFQID );
			if ( excludeMixtures )
			{
				chemicalGridView = chemicalGridView.Where( e => ( e.HMType ?? string.Empty ) != "b" );	//b=mixture
			}

			IEnumerable<ChemicalSynonymGridViewModel> chemicalSynonymGridView = new List<ChemicalSynonymGridViewModel>();
			if ( !excludeSynonyms )
			{
				chemicalSynonymGridView = Repository.Chemicals.SynonymGridSearch( name: name, casNumber: casNumber, cclFQID: cclFQID );
			}

			string downloadFileName = "CERSChemicalLibrary_EntireLibrary.xlsx";
			if ( !string.IsNullOrWhiteSpace( name ) ||
				!string.IsNullOrWhiteSpace( casNumber ) ||
				!string.IsNullOrWhiteSpace( cclFQID ) )
			{
				downloadFileName = "CERSChemicalLibrary_SearchResults.xlsx";
			}

			GenerateChemicalLibraryExcel( chemicalGridView, chemicalSynonymGridView, downloadFileName );
		}

		[Authorize]
		public ActionResult RecommendChemicalChanges( int CID )
		{
			var account = SessionStorage.CurrentAccountInformation;
			var chemical = Repository.Chemicals.GetByID( CID );

			var materialName = "No Material Found";
			var CCLID = 0;
			if ( chemical != null )
			{
				materialName = string.IsNullOrWhiteSpace( chemical.ChemicalName ) ? chemical.CommonName : chemical.ChemicalName;
				CCLID = chemical.CCLID;
			}

			RecommendChemicalChangesViewModel viewModel = new RecommendChemicalChangesViewModel()
			{
				YourName = account.FullName,
				Email = account.Email,
				RecommendedDateTime = DateTime.Now,
				Material = materialName,
				CCLID = CCLID.ToString(),
				displayCCLID = CCLID.ToString(),
			};

			return View( viewModel );
		}

		[HttpPost]
		public ActionResult RecommendChemicalChanges( RecommendChemicalChangesViewModel recommendChemicalChangesViewModel )
		{
			RecommendChemicalChangesViewModel viewModel = new RecommendChemicalChangesViewModel();
			viewModel = recommendChemicalChangesViewModel;

			var message = "<html><span style='font-family: Arial,Verdana,Helvetica; font-size:.8em;'>";
			message += String.Format( "<b>Submitter Name:</b> {0}<br>", viewModel.YourName );
			message += String.Format( "<b>Submitter Email:</b> {0}<br>", viewModel.Email );
			message += String.Format( "<b>Date:</b> {0}<br>", viewModel.RecommendedDateTime );
			message += String.Format( "<b>Material:</b> {0}<br>", viewModel.Material );
			message += String.Format( "<b>CCLID:</b> {0}<br>", viewModel.CCLID );
			message += String.Format( "<br>" );
			message += String.Format( "<b>Comments:</b> {0}<br>", viewModel.RecommendationDescription );
			message += "</span></html>";

			var to = "cers@calepa.ca.gov";
			var subject = String.Format( "Recommended Changes for CERS Chemical Library Material {0}", viewModel.Material );

			Services.Emails.Send( to, subject, message );

			return Redirect( Url.CERSRouteUrl( PublicRoute.ChemicalLibrary ) );
		}

        public ActionResult Search_GridBindingChemicals( [DataSourceRequest]DataSourceRequest request, StringSearchOption chemicalNameSearchOption, string chemicalName, string casNumber, string cclFQID, bool excludeSynonyms, bool excludeMixtures )
        {
            // Default ChemicalLibraryEntry Search to Empty IEnumerable
            IEnumerable<ChemicalLibrarySearchResult> entities = new List<ChemicalLibrarySearchResult>();

            // Only perform the search if they search by at least one parameter -
            if ( !string.IsNullOrWhiteSpace( chemicalName ) ||
                !string.IsNullOrWhiteSpace( casNumber ) ||
                !string.IsNullOrWhiteSpace( cclFQID ) )
            {
                entities = Repository.Chemicals.SearchLibrary( chemicalNameSearchOption:chemicalNameSearchOption, name:WebUtility.UrlDecode( chemicalName ), casNumber:casNumber, cclFQID:WebUtility.UrlDecode( cclFQID ) );
                if ( excludeSynonyms )
                {
                    entities = entities.Where( e => e.ChemicalNameTypeID != (int)ChemicalNameType.Synonym );
                }
                if ( excludeMixtures )
                {
                    entities = entities.Where( e => ( e.HMType ?? string.Empty ) != "b" );	//b=mixture
                }
            }
            DataSourceResult result = entities.ToList().ToDataSourceResult( request );
            return Json( result, JsonRequestBehavior.AllowGet );
        }

		protected void GenerateChemicalLibraryExcel( IEnumerable<ChemicalLibrarySearchResultGridViewModel> items, string fileName )
		{
			string excelTemplateFilePath = System.IO.Path.Combine( Server.MapPath( "~" ), @"Content\TemplateFiles\ChemicalLibraryTemplate.xlsx" );

			using ( FileStream sourceXlsDataStream = new FileStream( excelTemplateFilePath, FileMode.Open, FileAccess.Read ) )
			{
				ExcelWorkbook workbook = new ExcelWorkbook( sourceXlsDataStream );
				workbook.LicenseKey = Excel.GetWinnovativeExcelLicenseKey();

				// Retrieve the first Worksheet
				ExcelWorksheet worksheet = workbook.Worksheets[0];

				// Loop through Chemical GridView and Add All to the Worksheet
				string chemicalName = "";
				string commonSynonymName = "";
				int rowIndex = 2;
				foreach ( var item in items )
				{
					// Strip out any invalid XML characters from ChemicalName
					// and CommonSynonymName before populating the cell:
					chemicalName = item.ChemicalName != null ? Regex.Replace( item.ChemicalName, @"[^\u0009\u000a\u000d\u0020-\uD7FF\uE000-\uFFFD]", "" ) : "";
					commonSynonymName = item.CommonSynonymName != null ? Regex.Replace( item.CommonSynonymName, @"[^\u0009\u000a\u000d\u0020-\uD7FF\uE000-\uFFFD]", "" ) : "";

					worksheet[rowIndex, 1].Text = chemicalName != null ? chemicalName : "";
					worksheet[rowIndex, 2].Text = commonSynonymName != null ? commonSynonymName : "";
					worksheet[rowIndex, 3].Text = item.CASNumber != null ? item.CASNumber : "";
					worksheet[rowIndex, 4].Text = item.IsSynonym != null ? item.IsSynonym : "";
					worksheet[rowIndex, 5].Text = item.CCLFQID != null ? item.CCLFQID : "";
					worksheet[rowIndex, 6].Value = item.USEPASRSNumber != null ? item.USEPASRSNumber : "";

					rowIndex++;
				}

				SetDownloadFileHeader( "Application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName );
				workbook.Save( Response.OutputStream );
				Response.End();
			}
		}

		protected void GenerateChemicalLibraryExcel( IEnumerable<ChemicalGridViewModel> chemicals, IEnumerable<ChemicalSynonymGridViewModel> chemicalSynonyms, string fileName )
		{
			string excelTemplateFilePath = System.IO.Path.Combine( Server.MapPath( "~" ), @"Content\TemplateFiles\ChemicalLibraryExportTemplate.xlsx" );

			using ( FileStream sourceXlsDataStream = new FileStream( excelTemplateFilePath, FileMode.Open, FileAccess.Read ) )
			{
				ExcelWorkbook workbook = new ExcelWorkbook( sourceXlsDataStream );
				workbook.LicenseKey = Excel.GetWinnovativeExcelLicenseKey();

				// Retrieve the first two Worksheets
				ExcelWorksheet chemicalWorksheet = workbook.Worksheets[0];
				ExcelWorksheet synonymWorksheet = workbook.Worksheets[1];

				// Loop through Chemical GridView and Add All to the Worksheet
				string chemicalName = "";
				string commonName = "";
				string synonymName = "";
				int colIndex = 1;
				int rowIndex = 3;
				foreach ( var chemical in chemicals )
				{
					// Strip out any invalid XML characters from Chemical Name and Common Name before populating the cell:
					chemicalName = chemical.ChemicalName != null ? Regex.Replace( chemical.ChemicalName, @"[^\u0009\u000a\u000d\u0020-\uD7FF\uE000-\uFFFD]", "" ) : "";
					commonName = chemical.CommonName != null ? Regex.Replace( chemical.CommonName, @"[^\u0009\u000a\u000d\u0020-\uD7FF\uE000-\uFFFD]", "" ) : "";

					colIndex = 1;
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.CCLFQID != null ? chemical.CCLFQID : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.CASNumber != null ? chemical.CASNumber : "";
					chemicalWorksheet[rowIndex, colIndex++].Value = chemical.USEPASRSNumber != null ? chemical.USEPASRSNumber : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemicalName != null ? chemicalName : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = commonName != null ? commonName : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.EHS != null ? chemical.EHS : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.HMType != null ? chemical.HMType : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.RadioActive != null ? chemical.RadioActive : "";

					//chemicalWorksheet[rowIndex, colIndex++].Text = chemical. != null ? chemical : "";
					// colIndex++;  // This represents the Curies Column
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.PhysState != null ? chemical.PhysState : "";
					chemicalWorksheet[rowIndex, colIndex++].Value = chemical.Firecode1;
					chemicalWorksheet[rowIndex, colIndex++].Value = chemical.Firecode2;
					chemicalWorksheet[rowIndex, colIndex++].Value = chemical.Firecode3;
					chemicalWorksheet[rowIndex, colIndex++].Value = chemical.Firecode4;
					chemicalWorksheet[rowIndex, colIndex++].Value = chemical.Firecode5;
					chemicalWorksheet[rowIndex, colIndex++].Value = chemical.Firecode6;
					chemicalWorksheet[rowIndex, colIndex++].Value = chemical.Firecode7;
					chemicalWorksheet[rowIndex, colIndex++].Value = chemical.Firecode8;
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.FHCFire != null ? chemical.FHCFire : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.FHCReactive != null ? chemical.FHCReactive : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.FHCPressureRelease != null ? chemical.FHCPressureRelease : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.FHCAcuteHealth != null ? chemical.FHCAcuteHealth : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.FHCChronicHealth != null ? chemical.FHCChronicHealth : "";
                    chemicalWorksheet[rowIndex, colIndex++].Text = chemical.STACode != null ? chemical.STACode : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.DOTHazClassID.IfNullOrEmpty( "" );
					//chemicalWorksheet[rowIndex, colIndex++].Text = chemical.Prop65Types != null ? string.Join( ", ", chemical.Prop65Types.ToArray() ) : "";
					chemicalWorksheet[rowIndex, colIndex++].Value = chemical.Component1Percent;
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.Component1Name != null ? chemical.Component1Name : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.Component1EHS != null ? chemical.Component1EHS : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.Component1CAS != null ? chemical.Component1CAS : "";
					chemicalWorksheet[rowIndex, colIndex++].Value = chemical.Component2Percent;
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.Component2Name != null ? chemical.Component2Name : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.Component2EHS != null ? chemical.Component2EHS : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.Component2CAS != null ? chemical.Component2CAS : "";
					chemicalWorksheet[rowIndex, colIndex++].Value = chemical.Component3Percent;
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.Component3Name != null ? chemical.Component3Name : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.Component3EHS != null ? chemical.Component3EHS : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.Component3CAS != null ? chemical.Component3CAS : "";
					chemicalWorksheet[rowIndex, colIndex++].Value = chemical.Component4Percent;
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.Component4Name != null ? chemical.Component4Name : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.Component4EHS != null ? chemical.Component4EHS : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.Component4CAS != null ? chemical.Component4CAS : "";
					chemicalWorksheet[rowIndex, colIndex++].Value = chemical.Component5Percent;
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.Component5Name != null ? chemical.Component5Name : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.Component5EHS != null ? chemical.Component5EHS : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.Component5CAS != null ? chemical.Component5CAS : "";
					chemicalWorksheet[rowIndex, colIndex++].Text = chemical.AdditionalMixtureComponent != null ? chemical.AdditionalMixtureComponent : "";
					chemicalWorksheet[rowIndex, colIndex++].Value = chemical.UpdatedOn;
					rowIndex++;
				}

				colIndex = 1;
				rowIndex = 3;
				foreach ( var chemicalSynonym in chemicalSynonyms )
				{
					// Strip out any invalid XML characters from Chemical Name
					// and Synonym Name before populating the cell:
					chemicalName = "";
					if ( chemicalSynonym.ChemicalName != null )
					{
						chemicalName = chemicalSynonym.ChemicalName != null ? Regex.Replace( chemicalSynonym.ChemicalName, @"[^\u0009\u000a\u000d\u0020-\uD7FF\uE000-\uFFFD]", "" ) : "";
					}
					synonymName = "";
					synonymName = chemicalSynonym.SynonymName != null ? Regex.Replace( chemicalSynonym.SynonymName, @"[^\u0009\u000a\u000d\u0020-\uD7FF\uE000-\uFFFD]", "" ) : "";

					colIndex = 1;
					synonymWorksheet[rowIndex, colIndex++].Text = chemicalSynonym.CCLSFQID != null ? chemicalSynonym.CCLSFQID : "";
					synonymWorksheet[rowIndex, colIndex++].Text = chemicalSynonym.CCLFQID != null ? chemicalSynonym.CCLFQID : "";
					synonymWorksheet[rowIndex, colIndex++].Text = chemicalSynonym.CAS != null ? chemicalSynonym.CAS : "";
					synonymWorksheet[rowIndex, colIndex++].Text = chemicalSynonym.USEPASRSNumber != null ? chemicalSynonym.USEPASRSNumber : "";
					synonymWorksheet[rowIndex, colIndex++].Text = chemicalName;
					synonymWorksheet[rowIndex, colIndex++].Text = synonymName;
					synonymWorksheet[rowIndex, colIndex++].Value = chemicalSynonym.UpdatedOn;
					rowIndex++;
				}

				SetDownloadFileHeader( "Application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName );
				workbook.Save( Response.OutputStream );
				Response.End();
			}
		}

		#endregion Chemical Library

		#region Violations Library

		[HttpPost]
		public JsonResult LoadViolationCategoryByProgramID( int id )
		{
			var categoryList = Repository.ViolationCategories.Search( violationProgramElementID: id ).OrderBy( vc => vc.Name );

			var categoryData = categoryList.Select( c => new SelectListItem()
			{
				Text = c.Name,
				Value = c.ID.ToString()
			} );

			return Json( categoryData, JsonRequestBehavior.AllowGet );
		}

        public ActionResult SearchViolations( [DataSourceRequest]DataSourceRequest request, int? violationProgramElementID = null, int? violationCategoryID = null, int? violationSourceID = null, string violationTypeNumber = "", string name = "", string description = "", string violationCitation = "", DateTime? beginDate = null, DateTime? endDate = null )
		{
			ViolationTypeViewModel viewModel = SystemViewModelData.BuildUpViolationTypeViewModel( null, true );

			// Perform search using user-entered criteria:
            var ViolationTypes = Repository.ViolationTypes.GridSearch( violationProgramElementID:violationProgramElementID,
                                                                        violationCategoryID:violationCategoryID,
                                                                        violationSourceID:violationSourceID,
                                                                        typeNumber:violationTypeNumber,
                                                                        name:name,
                                                                        description:description,
                                                                        citation:violationCitation,
                                                                        beginDate:beginDate,
                                                                        endDate:endDate );

            DataSourceResult result = ViolationTypes.ToList().ToDataSourceResult( request );
            return Json( result, JsonRequestBehavior.AllowGet );
		}

		public ActionResult ViolationDetail( int violationTypeID )
		{
			ViolationTypeViewModel viewModel = SystemViewModelData.BuildUpViolationTypeViewModel( violationTypeID );
			return View( viewModel );
		}

		public DataTable ViolationLibraryDataTable( IEnumerable<ViolationType> violations )
		{
			DataTable table = new DataTable();
			table.Columns.Add( "Violation Program", typeof( string ) );
			table.Columns.Add( "Violation Category", typeof( string ) );
			table.Columns.Add( "Violation Type Number", typeof( string ) );
			table.Columns.Add( "RCRA Violation Type", typeof( string ) );
			table.Columns.Add( "Name", typeof( string ) );
			table.Columns.Add( "Description", typeof( string ) );
			table.Columns.Add( "Citations", typeof( string ) );
			table.Columns.Add( "Begin Date", typeof( DateTime ) );
			table.Columns.Add( "End Date", typeof( DateTime ) );
			table.Columns.Add( "Comments", typeof( string ) );
			table.Columns.Add( "Updated On", typeof( DateTime ) );

			foreach ( var violation in violations )
			{
				table.Rows.Add( violation.ViolationCategory.ViolationProgramElement.Name,
							   violation.ViolationCategory.Name,
							   violation.ViolationTypeNumber,
							   violation.RCRAViolationType,
							   violation.Name,
							   violation.Description,
							   violation.CompiledCitations,
							   violation.BeginDate,
							   violation.EndDate,
							   violation.Comments,
							   violation.UpdatedOn );
			}

			return table;
		}

		public ActionResult Violations()
		{
			ViolationTypeViewModel viewModel = SystemViewModelData.BuildUpViolationTypeViewModel( null, true );

			// Perform default search for all Violation Library Entries:
			viewModel.ViolationTypeGridView = Repository.ViolationTypes.GridSearch();

			return View( viewModel );
		}

		[HttpPost]
		public ActionResult Violations( ViolationTypeViewModel violationTypeViewModel, FormCollection formCollection )
		{
			ViolationTypeViewModel viewModel = SystemViewModelData.BuildUpViolationTypeViewModel( null, true );

			// Set Populated viewModel search parameters to user-supplied values
			viewModel.ViolationProgramElementID = violationTypeViewModel.ViolationProgramElementID;
			viewModel.ViolationCategoryID = violationTypeViewModel.ViolationCategoryID;
			viewModel.ViolationSourceID = violationTypeViewModel.ViolationSourceID;
			viewModel.ViolationTypeNumber = violationTypeViewModel.ViolationTypeNumber;
			viewModel.Name = violationTypeViewModel.Name;
			viewModel.Description = violationTypeViewModel.Description;
			viewModel.ViolationCitation = violationTypeViewModel.ViolationCitation;
            viewModel.BeginDate = violationTypeViewModel.BeginDate;
            viewModel.EndDate = violationTypeViewModel.EndDate;

			// Perform search using user-entered criteria, return Grid View:
            viewModel.ViolationTypeGridView = Repository.ViolationTypes.GridSearch( violationProgramElementID:violationTypeViewModel.ViolationProgramElementID,
                                                                                   violationCategoryID:violationTypeViewModel.ViolationCategoryID,
                                                                                   violationSourceID:violationTypeViewModel.ViolationSourceID,
                                                                                   typeNumber:violationTypeViewModel.ViolationTypeNumber,
                                                                                   name:violationTypeViewModel.Name,
                                                                                   description:violationTypeViewModel.Description,
                                                                                   citation:violationTypeViewModel.ViolationCitation,
                                                                                   beginDate:violationTypeViewModel.BeginDate,
                                                                                   endDate:violationTypeViewModel.EndDate
                                                                                   );

			//Check for Export
			if ( !string.IsNullOrWhiteSpace( formCollection["export"] ) )
			{
				string excelTemplateFilePath = System.IO.Path.Combine( Server.MapPath( "~" ), ConfigurationManager.AppSettings["ViolationLibraryExcelTemplate"] );
				FileStream sourceXlsDataStream = new System.IO.FileStream( excelTemplateFilePath, FileMode.Open, FileAccess.Read );

				ExcelWorkbook workbook = new ExcelWorkbook( sourceXlsDataStream );
				workbook.LicenseKey = Excel.GetWinnovativeExcelLicenseKey();

				// Obtain First Worksheet (this is where our Inventory will be inserted)
				ExcelWorksheet excelWorksheet = workbook.Worksheets[0];

				if ( formCollection["export"] == "Export to Excel" )
				{
					if ( excelWorksheet != null )
					{
						// Retrieve the real entities for export (not only the Grid View from above)
						// based on the provided search criteria:
						viewModel.Entities = Repository.ViolationTypes.Search( violationProgramElementID: violationTypeViewModel.ViolationProgramElementID,
																			  violationCategoryID: violationTypeViewModel.ViolationCategoryID,
																			  violationSourceID: violationTypeViewModel.ViolationSourceID,
																			  typeNumber: violationTypeViewModel.ViolationTypeNumber,
																			  name: violationTypeViewModel.Name,
																			  description: violationTypeViewModel.Description,
																			  citation: violationTypeViewModel.ViolationCitation );

						DataTable table = ViolationLibraryDataTable( viewModel.Entities );
						excelWorksheet.LoadDataTable( table, 4, 1, false );
					}
                    SetDownloadFileHeader( "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CERSViolationLibrary_SearchResults.xlsx" );
				}
				else if ( formCollection["export"] == "Export Entire Violation Library" )
				{
					if ( excelWorksheet != null )
					{
						// Retrieve the entire set of Violation Types (the real entities, not Grid View)
						viewModel.Entities = Repository.ViolationTypes.Search();

						DataTable table = ViolationLibraryDataTable( viewModel.Entities );
						excelWorksheet.LoadDataTable( table, 4, 1, false );
					}
                    SetDownloadFileHeader( "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CERSViolationLibrary_EntireExport.xlsx" );
				}

				workbook.Save( Response.OutputStream );
				Response.End();

                return new EmptyResult();
			}

			return View( viewModel );
		}

		#endregion Violations Library
	}
}