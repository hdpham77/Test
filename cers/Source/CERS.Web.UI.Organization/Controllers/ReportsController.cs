using CERS.Model;
using CERS.ViewModels;
using CERS.ViewModels.Organizations;
using CERS.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml.Linq;
using UPF.Core;

namespace CERS.Web.UI.Organization.Controllers
{
	[Authorize]
	public class ReportsController : AppControllerBase
	{
		//
  // GET: /Reports/{organizationId}

		public ActionResult Index( int? organizationId = null )
		{
			OrganizationReportViewModel viewModel = BuildOrganizationReportViewModel( accountID: CurrentAccountID );
			viewModel.OrganizationID = organizationId;
			return View( viewModel );
		}

		#region RegulatorLocalRequirements Methods

		public ActionResult RegulatorLocalRequirements() // bool export = false )
		{
			RegulatorLocalRequirementsReportViewModel viewModel = new RegulatorLocalRequirementsReportViewModel();
			viewModel.SubmittalElements = Repository.SubmittalElements.GetAll().ToDictionary( p => p.ID, p => p.Name );
			viewModel.Regulators = Repository.Regulators.Search( type: RegulatorType.CUPA ).ToDictionary( p => p.ID, p => p.NameShort );
            //if ( export )
            //{
            //    viewModel.GridView = Repository.RegulatorLocals.GridSearch().OrderBy( p => p.SubmittalElementID ).ThenBy( p => p.RegulatorName );
            //    ExportToExcel( "UP_LocalRequirements.xlsx", viewModel.GridView );
            //}

			return View( viewModel );
		}

		public ActionResult RegulatorLocalRequirements_Async( [DataSourceRequest]DataSourceRequest request,  int? regulatorID = null, int? submittalElementID = null, string keywords = null, DateTime? lastUpdatedOn = null, bool? isDocumentRequired = null )
		{
			var entities = Repository.RegulatorLocals.GridSearch( regulatorID, submittalElementID, isDocumentRequired, keywords, lastUpdatedOn ).OrderBy( p => p.SubmittalElementID ).ThenBy( p => p.RegulatorName );

            DataSourceResult result = entities.ToList().ToDataSourceResult( request );
            return Json( result, JsonRequestBehavior.AllowGet );
		}

        public void Download_LocalRequirements( int? regulatorID = null, int? submittalElementID = null, string keywords = null, DateTime? lastUpdatedOn = null, bool? isDocumentRequired = null )
        {
            var entities = Repository.RegulatorLocals.GridSearch( regulatorID, submittalElementID, isDocumentRequired, keywords, lastUpdatedOn ).OrderBy( p => p.SubmittalElementID ).ThenBy( p => p.RegulatorName );
            ExportToExcel( "UP_LocalRequirements.xlsx", entities );
        }

		#endregion RegulatorLocalRequirements Methods

		#region Hazardous Material Inventory Download

		// GET: /Reports/HazMatInventoryDownload/{organizationId}
        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult HazMatInventoryDownload( int organizationID )
		{
			string excelTemplateFilePath = System.IO.Path.Combine( Server.MapPath( "~" ), @"Content\TemplateFiles\HazMatInventoryBulkDownloadTemplate.xlsx" );
            var workbook = Services.Excel.GenerateHazardousMaterialInventory( excelTemplateFilePath, organizationID );

            OrganizationReportViewModel viewModel = BuildOrganizationReportViewModel( accountID: CurrentAccountID );
            CERS.Model.Organization currentOrganization = Repository.Organizations.GetByID( organizationID );
            string strippedOrganizationName = "OrganizationName";
			strippedOrganizationName = Regex.Replace( currentOrganization.Name, "[^a-zA-Z0-9]", "" );

			SetDownloadFileHeader( "Application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", strippedOrganizationName + "_HazMatInventoryBulkDownload_" + DateTime.Now.ToString( "yyyyMMdd" ) + ".xlsx" );
			workbook.Save( Response.OutputStream );
			Response.End();

			viewModel = BuildOrganizationReportViewModel( accountID: CurrentAccountID );
			return View( viewModel );
		}

        [HttpPost]
        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
        public ActionResult GetOrganizationHMICount_Async( int organizationID )
        {
            int MAX_REALTIME_HMI_ITEM = Repository.Settings.GetMaxRealTimeHMIRecords();
            int count = Repository.BPFacilityChemicals.GetOrganizationInventoryCount( organizationID );
            var result = new
            {
                DoDeferredProcessing = ( count > MAX_REALTIME_HMI_ITEM ),
                HMICount = count,
            };
            return Json( result );
        }

        [HttpPost]
        [EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
        public ActionResult DeferredOrganizationHMIDownload( int organizationID )
        {
            string excelTemplateFilePath = System.IO.Path.Combine( Server.MapPath( "~" ), @"Content\TemplateFiles\HazMatInventoryBulkDownloadTemplate.xlsx" );
            XElement xmlElement = new XElement( "Parameters" );
            xmlElement.Add( new XElement( "OrganizationID", organizationID ) );

            DeferredJob deferredJob = new DeferredJob()
            {
                PortalTypeID = CurrentPortal.SystemPortalID,
                DeferredJobTypeID = (int)DeferredJobType.OrganizationHMIDownload,
                Parameters = xmlElement.ToString(),
                JobSubmittedDate = DateTime.Now,
                JobStatusID = (int)DeferredJobStatus.Queued,
            };

            var foundJob = Repository.DeferredJobs.SearchMatchingDeferredJob( (SystemPortalType)CurrentPortal.SystemPortalID, DeferredJobType.OrganizationHMIDownload, xmlElement.ToString(), purgeDate: DateTime.Now, requesterID: CurrentAccountID );

            string message = string.Empty;
            if ( foundJob != null )
            {
                message = String.Format( "You already submitted a similiar request on {0}. It is on the queue waiting to be processed.", foundJob.JobSubmittedDate );
                if ( (DeferredJobStatus)foundJob.JobStatusID == DeferredJobStatus.InProgress )
                {
                    message = String.Format( "You already submitted a similiar request on {0} that is now in process. Your should receive an email with the result very soon.", foundJob.JobSubmittedDate );
                }
                else if ( (DeferredJobStatus)foundJob.JobStatusID == DeferredJobStatus.Success )
                {
                    message = String.Format( "Your request has been processed on {0}. Please check your email.", foundJob.JobEndTime );
                    //if ( foundJob.DocumentPurgeAfter != null || foundJob.DocumentPurgeAfter.Value < DateTime.Now )
                    //{
                    //    foundJob = null;
                    //}
                }
            }

            if ( foundJob == null )
            {
                message = "Your request has been submitted. You will receive an email when the file is ready for download.";

                FileStream xlsFileStream = new System.IO.FileStream( excelTemplateFilePath, FileMode.Open, FileAccess.Read );
                var fileName = String.Format( "BusinessName" + "_HazMatInventoryBulkDownload_" + DateTime.Now.ToString( "yyyyMMdd" ) + ".xlsx" );
                var document = Repository.Documents.Save( document: xlsFileStream, originalFileName: fileName, context: DocumentContext.Organization, contextID: organizationID );

                deferredJob.DocumentID = document.ID;
                Repository.DeferredJobs.Save( deferredJob );
            }

            var result = new
            {
                Message = message,
            };
            return Json( result );
        }

		#endregion Hazardous Material Inventory Download

		#region BuildOrganizationReportViewModel Method

		private OrganizationReportViewModel BuildOrganizationReportViewModel( int? accountID = null )
		{
			var viewModel = new OrganizationReportViewModel() { };

			if ( accountID.HasValue )
			{
				viewModel.OrganizationCollection = Repository.Organizations.GetByAccount( accountID.Value );
			}

			return viewModel;
		}

		#endregion BuildOrganizationReportViewModel Method
	}
}