using CERS.ViewModels;
using CERS.ViewModels.Reports;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CERS.Web.UI.Public.Controllers
{
	public class StatisticsController : AppControllerBase
	{

        public ActionResult Index( DateTime? endDate = null )
		{
            var data = GetStatisticsDataGeneric(Convert.ToDateTime("1/1/2013"));
            PublicStatisticsViewModel viewModel = new PublicStatisticsViewModel()
            {
                AverageUploadedDocumentSize = (int)data.Sum(d => d.DocumentUploadedSizeInKB) / data.Sum(d => d.DocumentUploadedCount),
                EndDate = endDate,
            };
			return View(viewModel);
		}

        public ActionResult CERSStatistics_GridRead([DataSourceRequest]DataSourceRequest request, DateTime? beginDate = null, DateTime? endDate = null )
        {
            var data = GetStatisticsDataGeneric(beginDate, endDate);

            var formattedData = from p in data
                                orderby p.Year descending, p.Month descending
                                select new CERSStatisticsViewModel
                                {
                                    MonthYear = Convert.ToDateTime(String.Format("{0}/1/{1}", p.Month, p.Year)).ToString("MMM-yyyy"),
                                    BusinessUserCount = p.BusinessUserCount,
                                    SubmittedSubmittalElements = p.SubmittedFacInfoCount + p.SubmittedHMICount + p.SubmittedERTPCount + p.SubmittedUSTCount + p.SubmittedTPCount + p.SubmittedHWRecycleCount + p.SubmittedSiteConsolidationCount + p.SubmittedTankClosureCount + p.SubmittedAPSACount,
                                    FacilitySubmitting = p.FacilitiesSubmittingCount,
                                    OrganizationCount = p.OrganizationCount,
                                    FacilityCount = p.FacilityCount,

                                    RegulatorUserCount = p.RegulatorUserCount,
                                    ReviewedSubmittalElements = p.ReviewedFacInfoCount + p.ReviewedHMICount + p.ReviewedERTPCount + p.ReviewedUSTCount + p.ReviewedTPCount + p.ReviewedHWRecycleCount + p.ReviewedSiteConsolidationCount + p.ReviewedTankClosureCount + p.ReviewedAPSACount,
                                    OtherRegulatorActions = p.OtherRegulatorActionCount,
                                    EDTTransactions = p.EDTTransactionCount,
                                };

            DataSourceResult result = formattedData.ToDataSourceResult(request);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CERSSignInCounts_ChartRead(DateTime? beginDate = null, DateTime? endDate = null)
        {
            var data = GetStatisticsDataGeneric(beginDate, endDate);

            var formattedData = from p in data
                                orderby p.Year, p.Month
                                select new CERSStatisticsViewModel
                                {
                                    MonthYear = Convert.ToDateTime(String.Format("{0}/1/{1}", p.Month, p.Year)).ToString("MMM-yyyy"),
                                    BusinessUserSignInCount = p.BusinessUserSignInCount,
                                    RegulatorUserSignInCount = p.RegulatorUserSignInCount,
                                    TrainingPortalSignInCount = p.TrainingPortalSignInCount,
                                    EDTTransactions = p.EDTTransactionCount,
                                };

            return Json(formattedData);
        }

        public ActionResult CERSMonthlySubmittalElementCounts_ChartRead(DateTime? beginDate = null, DateTime? endDate = null)
        {
            var data = GetStatisticsDataGeneric(beginDate, endDate);

            var formattedData = from p in data
                                orderby p.Year, p.Month
                                select new CERSStatisticsViewModel
                                {
                                    MonthYear = Convert.ToDateTime(String.Format("{0}/1/{1}", p.Month, p.Year)).ToString("MMM-yyyy"),

                                    SubmittedFacInfoCount = p.SubmittedFacInfoCount,
                                    SubmittedHMICount = p.SubmittedHMICount,
                                    SubmittedERTPCount = p.SubmittedERTPCount,
                                    SubmittedUSTCount = p.SubmittedUSTCount,
                                    SubmittedTPCount = p.SubmittedTPCount,
                                    SubmittedHWRecycleCount = p.SubmittedHWRecycleCount,
                                    SubmittedSiteConsolidationCount = p.SubmittedSiteConsolidationCount,
                                    SubmittedTankClosureCount = p.SubmittedTankClosureCount,
                                    SubmittedAPSACount = p.SubmittedAPSACount,
                                };

            return Json(formattedData);
        }

        public ActionResult CERSMonthlyRegulatorReviewed_ChartRead(DateTime? beginDate = null, DateTime? endDate = null)
        {
            var data = GetStatisticsDataGeneric(beginDate, endDate);

            var formattedData = from p in data
                                orderby p.Year, p.Month
                                select new CERSStatisticsViewModel
                                {
                                    MonthYear = Convert.ToDateTime(String.Format("{0}/1/{1}", p.Month, p.Year)).ToString("MMM-yyyy"),

                                    ReviewedFacInfoCount = p.ReviewedFacInfoCount,
                                    ReviewedHMICount = p.ReviewedHMICount,
                                    ReviewedERTPCount = p.ReviewedERTPCount,
                                    ReviewedUSTCount = p.ReviewedUSTCount,
                                    ReviewedTPCount = p.ReviewedTPCount,
                                    ReviewedHWRecycleCount = p.ReviewedHWRecycleCount,
                                    ReviewedSiteConsolidationCount = p.ReviewedSiteConsolidationCount,
                                    ReviewedTankClosureCount = p.ReviewedTankClosureCount,
                                    ReviewedAPSACount = p.ReviewedAPSACount,
                                };

            return Json(formattedData);
        }

        public ActionResult CERSMonthlyDocumentUploads_ChartRead(DateTime? beginDate = null, DateTime? endDate = null)
        {
            var data = GetStatisticsDataGeneric(beginDate, endDate);

            var formattedData = from p in data
                                orderby p.Year, p.Month
                                select new CERSStatisticsViewModel
                                {
                                    MonthYear = Convert.ToDateTime(String.Format("{0}/1/{1}", p.Month, p.Year)).ToString("MMM-yyyy"),

                                    DocumentUploadedCount = p.DocumentUploadedCount,
                                    DocumentAverageSizeInKB = p.DocumentUploadedCount == 0 ? 0 : (p.DocumentUploadedSizeInKB / p.DocumentUploadedCount),
                                };

            return Json(formattedData);
        }

        #region GetStatisticsDataGeneric

        private IEnumerable<Model.CERSStatistic> GetStatisticsDataGeneric(DateTime? beginDate = null, DateTime? endDate = null)
        {
            var data = Repository.CERSStatistics.GetAll();

            var result = from p in data
                         where 1 == 1
                            && (beginDate == null ? true : (string.Compare(Convert.ToDateTime(String.Format("{0}/1/{1}", p.Month, p.Year)).ToString("yyyyMM"), beginDate.Value.ToString("yyyyMM")) >= 0))
                            && (endDate == null ? true : (string.Compare(Convert.ToDateTime(String.Format("{0}/1/{1}", p.Month, p.Year)).ToString("yyyyMM"), endDate.Value.ToString("yyyyMM")) <= 0))
                         select p;
            return result;
        }

        #endregion xxx

        #region CUPAElectronicReportingStatus

        public ActionResult CUPAElectronicReportingStatus( DateTime? beginDate = null, DateTime? endDate = null, bool export = false )
        {
            if ( export )
            {
                var data = Repository.DataModel.uspCUPAElectronicReportingStatusReport( beginDate, endDate );

                var workbook = Services.Excel.CUPAElectronicReportingStatus( data, System.IO.Path.Combine( Server.MapPath( "~" ), @"Content\TemplateFiles\CUPAElectronicReportingStatsTemplate.xlsx" ) );

                SetDownloadFileHeader( "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CUPAElectronicReportingStatus.xlsx" );
                workbook.Save( Response.OutputStream );
                Response.End();
            }

            CUPAElectronicReportingStatusReportViewModel viewModel = new CUPAElectronicReportingStatusReportViewModel();
            var startDate = new DateTime( DateTime.Now.Year, 1, 1 );     //just do from beginning of this year for default   // DateTime.Parse( "1/1/2013" );
            viewModel.BeginDate = beginDate ?? startDate;
            viewModel.EndDate = DateTime.Now;
            return View( viewModel );
        }

        public ActionResult CUPAElectronicReportingStatus_GridRead( [DataSourceRequest]DataSourceRequest request, DateTime? beginDate = null, DateTime? endDate = null )
        {
            var data = Repository.DataModel.uspCUPAElectronicReportingStatusReport( beginDate, endDate ).ToList();

            var formattedData = from p in data
                                select new CUPAElectronicReportingStatusReportGridViewModel
                                {
                                    CUPA = p.NameShort,
                                    APSA = p.FacCountWithAPSASubmittals,
                                    ERTP = p.FacCountWithERTPSubmittals,
                                    FacInfo = p.FacCountWithFacInfoSubmittals,
                                    Inventory = p.FacCountWithInventorySubmittals,
                                    NumberOfFacilitiesIn2012 = p.FacCountIn2012,
                                    NumberOfFacilitiesNow = p.CurrentFacilityCount,
                                    TP = p.FacCountWithTPSubmittals,
                                    UST = p.FacCountWithUSTSubmittals,
                                    PercentFacilitiesInCERS = p.ApproxPercentOfRegulatedFacilitiesInCERS,
                                    PercentFacilitiesWithBizPlan = p.CurrentPercentOfCERSFacilitiesWith2013BizPlan
                                };

            DataSourceResult result = formattedData.ToDataSourceResult( request );

            return Json( result, JsonRequestBehavior.AllowGet );
        }

        #endregion CUPAElectronicReportingStatus

    }
}