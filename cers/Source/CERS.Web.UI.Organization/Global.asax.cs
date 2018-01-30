using CERS.Web.Mvc;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CERS.Web.UI.Organization
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode,
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		#region Error Handling For Document Upload MaxRequestLength exceeded ( file too large)

		//The code below is not complete.  The exception is not thrown, but the request needs to be directed back to it's original location if possible.
		private void Application_Error( object sender, EventArgs e )
		{
			//Exception lastException = this.Server.GetLastError();

			//Session["LastException"] = lastException;

			//this.Server.ClearError();

			//if (IsMaxRequestExceededEexception(this.Server.GetLastError()))
			//{
			//    //  this.Server.ClearError();

			// //this.Server.Transfer("~/error/UploadTooLarge.aspx");
			//
			//
			//
			//}
		}

		private bool IsMaxRequestExceededEexception( Exception e )
		{
			// unhandeled errors = caught at global.ascx level http exception = caught at page level
			int TimedOutExceptionCode = -2147467259;
			Exception main;
			var unhandeled = e as HttpUnhandledException;

			if ( unhandeled != null && unhandeled.ErrorCode == TimedOutExceptionCode )
			{
				main = unhandeled.InnerException;
			}
			else
			{
				main = e;
			}

			var http = main as HttpException;

			if ( http != null && http.ErrorCode == TimedOutExceptionCode )
			{
				// hack: no real method of identifing if the error is max request exceeded as
				// it is treated as a timeout exception
				if ( http.StackTrace.Contains( "GetEntireRawContent" ) )
				{
					// MAX REQUEST HAS BEEN EXCEEDED
					return true;
				}
			}
			return false;
		}

		#endregion Error Handling For Document Upload MaxRequestLength exceeded ( file too large)

		#region MiniProfiler Begin/End

		#region Application_BeginRequest()

		protected void Application_BeginRequest()
		{
			if ( Request.IsLocal )
			{
				MiniProfiler.Start();
			}
		}

		#endregion Application_BeginRequest()

		#region Application_EndRequest()

		protected void Application_EndRequest()
		{
			MiniProfiler.Stop();
		}

		#endregion Application_EndRequest()

		#endregion MiniProfiler Begin/End

		#region RegisterRoutes Method

		public static void RegisterRoutes( RouteCollection routes )
		{
			routes.IgnoreRoute( "{resource}.axd/{*pathInfo}" );

			RegisterCommonRoutes( routes );
			RegisterAdminRoutes( routes );
			RegisterRevisionRoutes( routes );
			RegisterFacilityRoutes( routes );
			RegisterReportRoutes( routes );
			RegisterAccountManagementRoutes( routes );
			RegisterEventProcessingRoutes( routes );
			RegisterPublicRoutes( routes );
			RegisterEnhancementRoutes( routes );
			routes.RegisterServiceRoutes();

            routes.MapRoute( OrganizationFacility.SearchStartSubmittal, "{organizationId}/Facility/Search/StartSubmittal", new { controller = "Organization", action = "OrganizationFacilitySearch", organizationId = UrlParameter.Optional } );
            routes.MapRoute( OrganizationFacility.Search, "{organizationId}/Facility/Search", new { controller = "Facility", action = "Search", organizationId = UrlParameter.Optional } );
            routes.MapRoute( OrganizationFacility.Search_Async, "{organizationId}/Facility/Search_Async", new { controller = "Facility", action = "Search_Async", organizationId = UrlParameter.Optional } );
            routes.MapRoute( OrganizationFacility.SubmittalElementCommentToRegulator, "{organizationId}/Facility/SubmittalElementCommentToRegulator", new { controller = "Facility", action = "SubmittalElementCommentToRegulator", organizationId = UrlParameter.Optional } );
            routes.MapRoute( OrganizationFacility.HomeFacilities_Async, "{organizationId}/HomeFacilities_Async", new { controller = "Organization", action = "HomeFacilities_Async", organizationId = UrlParameter.Optional } );

			RegisterSubmittalRoutes( routes );

            routes.MapRoute( "OrganizationFacilityLandingPage", "{organizationId}/Facility/{CERSID}/", new { controller = "Facility", action = "Summary", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional } );
            routes.MapRoute( OrganizationFacility.Summary, "{organizationId}/Facility/{CERSID}/Summary", new { controller = "Facility", action = "Summary", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional } );
			routes.MapRoute( OrganizationFacility.Notifications, "{organizationId}/Facility/{CERSID}/Notifications", new { controller = "Facility", action = "Notifications", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional } );
			routes.MapRoute( OrganizationFacility.FacilityMap, "{organizationId}/Facility/{CERSID}/Map", new { controller = "Facility", action = "Map", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional } );
			routes.MapRoute( OrganizationFacility.Compliance, "{organizationId}/Facility/{CERSID}/Compliance", new { controller = "Facility", action = "Compliance", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional } );
			routes.MapRoute( OrganizationFacility.SubmittalElements, "{organizationId}/Facility/{CERSID}/SubmittalElements", new { controller = "Facility", action = "SubmittalElements", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional } );

            routes.MapRoute( OrganizationFacility.Submittals, "{organizationId}/Facility/{CERSID}/Submittals", new { controller = "Facility", action = "Submittals", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional } );
            routes.MapRoute( OrganizationFacility.Merge, "{organizationId}/Facility/{CERSID}/Merge", new { controller = "Facility", action = "Merge", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional } );
            routes.MapRoute( OrganizationFacility.Transfer, "{organizationId}/Facility/{CERSID}/Transfer", new { controller = "Facility", action = "Transfer", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional } );
            routes.MapRoute( OrganizationFacility.FacilitySubmittalPreparation, "{organizationId}/Facility/{CERSID}/DraftSubmittals", new { controller = "Facility", action = "FacilitySubmittalPreparation", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional } );

			//The following are the what we affectionately refer to as the CRAZY PAGE. The Old one can get commented out once the new one is ready.
			routes.MapRoute( OrganizationFacility.Home,
				"{organizationId}/Facility/{CERSID}/DraftSubmittalsHome",   //changed per Chris (bad URL naming) "{organizationId}/Facility/{CERSID}/DraftSubmittalsV1",
				new { controller = "Facility", action = "OrganizationFacilityHome", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional }
				);

			RegisterOrganizationComplianceRoutes( routes );
			RegisterOrganizationManagementRoutes( routes );

            routes.MapRoute( OrganizationFacility.RetrieveScheduledPrintJobDocument, "{organizationId}/RetrieveScheduledPrintJobDocument/{printJobID}", new { controller = "Facility", action = "RetrieveScheduledPrintJobDocument", printJobID = UrlParameter.Optional } );
            routes.MapRoute( CommonRoute.OrganizationHome, "{organizationId}", new { controller = "Organization", action = "Home", organizationId = UrlParameter.Optional } );

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);
		}

		#endregion RegisterRoutes Method

		public static void RegisterAdminRoutes( RouteCollection routes )
		{
			routes.RegisterCommonAdminRoutes();
		}

		public static void RegisterFacilityRoutes( RouteCollection routes )
		{
			routes.MapRoute( FacilityManagementRoute.New,
			  "New",
			  new { controller = "Facility", action = "New" }
			  );

			//routes.MapRoute( FacilityManagementRoute.StartWizardWithOrganizationID, "Facility/Add/Start/{organizationID}", new { controller = "AddFacility", action = "Start", organizationID = UrlParameter.Optional } );
			routes.MapRoute( FacilityManagementRoute.StartWizard, "Facility/Add/Start/{organizationID}", new { controller = "AddFacility", action = "Start", organizationID = UrlParameter.Optional } );
			routes.MapRoute( FacilityManagementRoute.Wizard, "Facility/Add/{key}", new { controller = "AddFacility", action = "Wizard", key = UrlParameter.Optional } );
		}

		#region RegisterCommonToolRoutes Method

		public static void RegisterCommonToolRoutes( RouteCollection routes )
		{
			routes.MapRoute( CommonRoute.Tools, "Tools", new { controller = "Tools", action = "Index" } );
			routes.MapRoute( CommonRoute.AddBusiness, "Tools/AddBusiness", new { controller = "Tools", action = "AddBusiness" } );
			routes.MapRoute( "ToolwithOrgId", "Tools/Organization/{organizationId}", new { controller = "Tools", action = "Index", organizationId = (int?)null } );
			routes.MapRoute( CommonRoute.OrganizationToolsRegulator, "Tools/Regulators", new { controller = "Tools", action = "Regulators" } );
			routes.MapRoute( CommonRoute.OrganizationToolsRegulatorDetail, "Tools/Regulators/{regulatorId}", new { controller = "Tools", action = "RegulatorDetail", regulatorId = UrlParameter.Optional } );
			routes.MapRoute( CommonRoute.CreateNewOrganization, "Tools/CreateNewOrganization", new { controller = "Tools", action = "CreateNewOrganization" } );
			routes.MapRoute( CommonRoute.OrganizationListing, "Tools/OrganizationListing", new { controller = "Tools", action = "OrganizationListing" } );
			routes.MapRoute( CommonRoute.OrganizationAccessRequest, "Tools/OrganizationAccessRequest/{organizationId}", new { controller = "Tools", action = "OrganizationAccessRequest", organizationId = (int?)null } );
			routes.MapRoute( CommonRoute.DraftsReplacedBySeeding, "Tools/DraftsReplacedBySeeding", new { controller = "Tools", action = "DraftsReplacedBySeeding" } );
			routes.MapRoute( CommonRoute.RestoreDraftReplacedBySeeding, "Tools/RestoreDraftReplacedBySeeding/{FSEID}", new { controller = "Tools", action = "RestoreDraftReplacedBySeeding", FSEID = UrlParameter.Optional } );
			routes.MapRoute( "Search_GridBindingDraftsReplacedBySeeding", "Tools/Search_GridBindingDraftsReplacedBySeeding", new { controller = "Tools", action = "Search_GridBindingDraftsReplacedBySeeding" } );
		}

		#endregion RegisterCommonToolRoutes Method

		#region RegisterPublicRoutes Method

		public static void RegisterPublicRoutes( RouteCollection routes )
		{
			routes.MapRoute( PublicRoute.Index, "Public", new { controller = "Public", action = "Index" } );
			routes.MapRoute( PublicRoute.ChemicalLibrary, "Public/ChemicalLibrary", new { controller = "Public", action = "Chemicals" } );
			routes.MapRoute( PublicRoute.ChemicalDetail, "Public/ChemicalDetail", new { controller = "Public", action = "ChemicalDetail" } );
			routes.MapRoute( PublicRoute.ChemicalDownload, "Public/Download_Chemicals", new { controller = "Public", action = "Download_Chemicals" } );
			routes.MapRoute( PublicRoute.RecommendChemicalChanges, "Public/RecommendChemicalChanges", new { controller = "Public", action = "RecommendChemicalChanges" } );
			routes.MapRoute( PublicRoute.ViolationLibrary, "Public/ViolationLibrary", new { controller = "Public", action = "Violations" } );
			routes.MapRoute( PublicRoute.ViolationDetail, "Public/ViolationDetail", new { controller = "Public", action = "ViolationDetail" } );
			routes.MapRoute( PublicRoute.ConditionsOfUse, "Public/ConditionsOfUse", new { controller = "Public", action = "ConditionsOfUse" } );
			routes.MapRoute( PublicRoute.Contact, "Public/Contact", new { controller = "Public", action = "Contact" } );
			routes.MapRoute( PublicRoute.Privacy, "Public/Privacy", new { controller = "Public", action = "PrivacyPolicy" } );
			routes.MapRoute( PublicRoute.BrowserInfo, "Public/BrowserInfo", new { controller = "Public", action = "BrowserInfo" } );
			routes.MapRoute( PublicRoute.UploadPolicy, "Public/UploadPolicy", new { controller = "Public", action = "UploadPolicy" } );
		}

		#endregion RegisterPublicRoutes Method

		#region RegisterAccountManagementRoutes Method

		public static void RegisterAccountManagementRoutes( RouteCollection routes )
		{
			routes.RegisterAccountManagementRoutes();
		}

		#endregion RegisterAccountManagementRoutes Method

		#region RegisterCommonRoutes Method

		public static void RegisterCommonRoutes( RouteCollection routes )
		{
			RegisterCommonToolRoutes( routes );

			routes.MapRoute( CommonRoute.Help, "Help", new { controller = "Help", action = "Index" } );
			routes.MapRoute( CommonRoute.FieldHelp, "FieldHelp", new { controller = "Help", action = "FieldHelp", dataRegistryID = UrlParameter.Optional } );
			routes.MapRoute( CommonRoute.Unauthorized, "Unauthorized", new { controller = "Home", action = "UnAuthorized" } );
			routes.MapRoute( CommonRoute.EventLinkForwarder, "Event/{ticketCode}", new { controller = "Home", action = "EventLinkForwarder", ticketCode = UrlParameter.Optional } );
			routes.MapRoute( CommonRoute.Error, "Error", new { controller = "Error", action = "Index" } );
			routes.MapRoute( CommonRoute.Error_CERSID_NotFound, "Error/CERSID/{CERSID}/NotFound", new { controller = "Error", action = "CERSIDNotFound", CERSID = UrlParameter.Optional } );
			routes.MapRoute( CommonRoute.Error_CERSID_Deleted, "Error/CERSID/{CERSID}/Deleted", new { controller = "Error", action = "CERSIDDeleted", CERSID = UrlParameter.Optional } );
			routes.MapRoute( CommonRoute.Error_CERSID_BusinessMismatch, "Error/CERSID/{CERSID}/OrganizationID/{organizationID}/Mismatch", new { controller = "Error", action = "CERSIDOrganizationIDMismatch", CERSID = UrlParameter.Optional, organizationID = UrlParameter.Optional } );
			routes.MapRoute( CommonRoute.TestError, "TestError", new { controller = "Error", action = "Test" } );
			routes.RegisterGlobalHelpRoutes();
			routes.MapRoute( CommonRoute.NotFound, "NotFound", new { controller = "Error", action = "NotFound" } );
			routes.MapRoute( CommonRoute.OrganizationDeferredProcessingExists, "Error/OrganizationID/{organizationID}/DeferredProcessingExists", new { controller = "Error", action = "OrganizationDeferredProcessingExists", organizationID = UrlParameter.Optional } );

            routes.MapRoute( CommonRoute.Switchboard, "Switchboard", new { controller = "Home", action = "Switchboard" } );
            routes.MapRoute( CommonRoute.GetStarted, "GetStarted", new { controller = "Home", action = "GetStarted" } );
            routes.MapRoute( CommonRoute.UserAgreement, "UserAgreement", new { controller = "Home", action = "UA" } );
            routes.MapRoute( CommonRoute.SelectOrganizationMenu, "SelectOrganization", new { controller = "Home", action = "SelectOrganization" } );

            //no longer relevant??? [ak 08/20/2014]
            //routes.MapRoute( GuidanceMessageRoute.FacilitySubmittalElement, "GuidanceMessage/FacilitySubmittalElement", new { controller = "Facility", action = "GuidanceMessageForSubmittalElement", fseid = UrlParameter.Optional } );
            //routes.MapRoute( GuidanceMessageRoute.FacilitySubmittalElementResource, "GuidanceMessage/FacilitySubmittalElementResource", new { controller = "Facility", action = "GuidanceMessageForSubmittalElementResource", fserid = UrlParameter.Optional } );
            //routes.MapRoute( GuidanceMessageRoute.FacilitySubmittalElementResourceEntity, "GuidanceMessage/FacilitySubmittalElementResourceEntity", new { controller = "Facility", action = "GuidanceMessageForSubmittalElementResourceEntity", fserid = UrlParameter.Optional, entityid = UrlParameter.Optional } );
		}

		#endregion RegisterCommonRoutes Method

		#region RegisterReportRoutes Method

		public static void RegisterReportRoutes( RouteCollection routes )
		{
			// NOTE: Base "/Reports" route is defined in the RegisterCommonRoutes Method
			routes.MapRoute( ReportRoute.OrganizationHazMatInventoryDownload, "Reports/HazMatInventoryDownload/{organizationId}", new { controller = "Reports", action = "HazMatInventoryDownload", organizationId = UrlParameter.Optional } );
            routes.MapRoute( ReportRoute.GetOrganizationHMICount_Async, "Reports/GetOrganizationHMICount_Async", new { controller = "Reports", action = "GetOrganizationHMICount_Async" } );
            routes.MapRoute( ReportRoute.OrganizationDeferredHazMatInventoryDownload, "Reports/DeferredOrganizationHMIDownload", new { controller = "Reports", action = "DeferredOrganizationHMIDownload" } );
            routes.MapRoute( ReportRoute.RegulatorSubmittalElementsLocalRequirements, "Reports/RegulatorLocalRequirements", new { controller = "Reports", action = "RegulatorLocalRequirements" } );
            routes.MapRoute( ReportRoute.RegulatorSubmittalElementsLocalRequirements_Aysnc, "Reports/RegulatorLocalRequirements_Async", new { controller = "Reports", action = "RegulatorLocalRequirements_Async" } );
            routes.MapRoute( ReportRoute.RegulatorSubmittalElementsLocalRequirements_Download, "Reports/Download_LocalRequirements", new { controller = "Reports", action = "Download_LocalRequirements" } );
			routes.MapRoute( CommonRoute.Reports, "Reports/{organizationId}", new { controller = "Reports", action = "Index", organizationId = UrlParameter.Optional } );
		}

		#endregion RegisterReportRoutes Method

		#region RegisterOrganizationComplianceRoutes

		public static void RegisterOrganizationComplianceRoutes( RouteCollection routes )
		{
            routes.MapRoute( OrganizationCompliance.Main, "{organizationId}/Compliance", new { controller = "OrganizationCompliance", action = "Index", organizationId = UrlParameter.Optional } );
            routes.MapRoute( OrganizationCompliance.Inspections, "{organizationId}/Compliance/Inspections", new { controller = "OrganizationCompliance", action = "Inspections", organizationId = UrlParameter.Optional } );
            routes.MapRoute( OrganizationCompliance.Violations, "{organizationId}/Compliance/Violations", new { controller = "OrganizationCompliance", action = "Violations", organizationId = UrlParameter.Optional } );
            routes.MapRoute( OrganizationCompliance.Enforcements, "{organizationId}/Compliance/Enforcements", new { controller = "OrganizationCompliance", action = "Enforcements", organizationId = UrlParameter.Optional } );
		}

		#endregion RegisterOrganizationComplianceRoutes

		#region RegisterOrganizationManagementRoutes Method

		public static void RegisterOrganizationManagementRoutes( RouteCollection routes )
		{
			routes.MapRoute( OrganizationManage.Summary, "{organizationId}/Manage", new { controller = "MyOrganization", action = "Index", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.Edit, "{organizationId}/Manage/Edit", new { controller = "MyOrganization", action = "Edit", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.Notifications, "{organizationId}/Manage/Notifications", new { controller = "MyOrganization", action = "Notifications", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.ActionRequired, "{organizationId}/Manage/ActionRequired", new { controller = "MyOrganization", action = "ActionRequired", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.EmailHistory, "{organizationId}/Manage/EmailHistory", new { controller = "MyOrganization", action = "EmailHistory", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.ArchivedFacilities, "{organizationId}/Manage/ArchivedFacilities", new { controller = "MyOrganization", action = "ArchivedFacilities", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.Archives, "{organizationId}/Manage/Archives", new { controller = "MyOrganization", action = "Archives", organizationId = UrlParameter.Optional } );

			#region People Related

			routes.MapRoute( OrganizationManage.People, "{organizationId}/Manage/People", new { controller = "MyOrganization", action = "People", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.PersonAdd, "{organizationId}/Manage/Person/Add", new { controller = "MyOrganization", action = "AddPerson", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.PersonEdit, "{organizationId}/Manage/Person/{pid}/Edit", new { controller = "MyOrganization", action = "EditPerson", organizationId = UrlParameter.Optional, pid = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.PersonDelete, "{organizationId}/Manage/Person/{pid}/Delete", new { controller = "MyOrganization", action = "DeletePerson", organizationId = UrlParameter.Optional, pid = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.PersonDetail, "{organizationId}/Manage/Person/{pid}", new { controller = "MyOrganization", action = "PersonDetails", organizationId = UrlParameter.Optional, pid = UrlParameter.Optional } );

			#endregion People Related

			routes.MapRoute( OrganizationManage.Facilities, "{organizationId}/Manage/Facilties/Search", new { controller = "MyOrganization", action = "Facilities", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.FacilityDeleteRequestTarget, "{organizationId}/Manage/FacilityDeleteRequestTarget", new { controller = "MyOrganization", action = "FacilityDeleteRequestTarget", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.FacilityDeleteRequestConfirm, "{organizationId}/Manage/FacilityDeleteRequestConfirm/{targetCERSID}", new { controller = "MyOrganization", action = "FacilityDeleteRequestConfirm", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.FacilityMergeRequestSource, "{organizationId}/Manage/FacilityMergeRequestSource", new { controller = "MyOrganization", action = "FacilityMergeRequestSource", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.FacilityMergeRequestTarget, "{organizationId}/Manage/FacilityMergeRequestTarget/{sourceCERSID}", new { controller = "MyOrganization", action = "FacilityMergeRequestTarget", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.FacilityMergeRequestConfirm, "{organizationId}/Manage/FacilityMergeRequestConfirm/{sourceCERSID}/{targetCERSID}", new { controller = "MyOrganization", action = "FacilityMergeRequestConfirm", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.FacilityTransferRequestSource, "{organizationId}/Manage/FacilityTransferRequestSource", new { controller = "MyOrganization", action = "FacilityTransferRequestSource", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.FacilityTransferRequestTarget, "{organizationId}/Manage/FacilityTransferRequestTarget/{sourceCERSID}", new { controller = "MyOrganization", action = "FacilityTransferRequestTarget", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.FacilityTransferRequestConfirm, "{organizationId}/Manage/FacilityTransferRequestConfirm/{sourceCERSID}/{targetOrganizationId}", new { controller = "MyOrganization", action = "FacilityTransferRequestConfirm", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.ManageFacilities, "{organizationId}/Manage/Facilities", new { controller = "MyOrganization", action = "ManageFacilities", organizationId = UrlParameter.Optional } );

			routes.MapRoute( OrganizationManage.Regulators, "{organizationId}/Manage/Regulators", new { controller = "MyOrganization", action = "Regulators", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.UploadInventory, "Manage/UploadBusinessInventory/{organizationId}", new { controller = "MyOrganization", action = "UploadBusinessInventory", organizationId = UrlParameter.Optional } );
			routes.MapRoute( OrganizationManage.UploadFacilityInfo, "Manage/UploadBusinessFacilityInfo/{organizationId}", new { controller = "MyOrganization", action = "UploadBusinessFacilityInfo", organizationId = UrlParameter.Optional } );

			routes.MapRoute( OrganizationManage.AccessRequestProcess, "{organizationId}/Manage/AccessRequest/{eventId}/Process", new { controller = "MyOrganization", action = "AccessRequestProcess", organizationId = UrlParameter.Optional, eventId = UrlParameter.Optional } );

			//API V2
			routes.MapRoute( OrganizationManage.AccessRequest, "{organizationId}/RequestAccess/{addFacilityWizardStateKey}", new { controller = "OrganizationManagement", action = "RequestAccess", organizationID = UrlParameter.Optional, addFacilityWizardStateKey = UrlParameter.Optional } );

			routes.MapRoute( OrganizationManage.FacilityReportingStatus, "{organizationId}/FacilityReportingStatus", new { controller = "Facility", action = "FacilityReportingStatus", organizationId = UrlParameter.Optional } );
            routes.MapRoute( OrganizationManage.FacilityInspectionStatus, "{organizationId}/FacilityInspectionStatus", new { controller = "Facility", action = "FacilityInspectionStatus", organizationId = UrlParameter.Optional } );

            routes.MapRoute( OrganizationManage.RetrieveScheduledDeferredJobDocument, "{organizationId}/RetrieveScheduledDeferredJobDocument/{deferredJobID}", new { controller = "Organization", action = "RetrieveScheduledDeferredJobDocument", deferredJobID = UrlParameter.Optional } );
        }

		#endregion RegisterOrganizationManagementRoutes Method

		#region RegisterSubmittalRoutes Method

		public static void RegisterSubmittalRoutes( RouteCollection routes )
		{
			RegisterSubmittalDraftRoutes( routes );

            routes.MapRoute( OrganizationFacility.Archive, "{organizationId}/Submittal/Archive", new { controller = "Submittal", action = "OrganizationFacilitySubmittalArchive", organizationId = UrlParameter.Optional } );
            routes.MapRoute( GetRouteName( Part.Submittal, Part.Start ), "{organizationId}/Facility/{CERSID}/Submittal/Start", new { controller = "Submittal", action = "SubmittalStart", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional } );
            routes.MapRoute( GetRouteName( Part.Submittal, Part.Finish ), "{organizationId}/Facility/{CERSID}/Submittal/{FSID}/Finish", new { controller = "Submittal", action = "SubmittalFinished", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSID = UrlParameter.Optional } );
            routes.MapRoute( OrganizationFacility.SubmittalHistory, "{organizationId}/Submittal/History", new { controller = "Submittal", action = "OrganizationFacilitySubmittalHistory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional } );
            routes.MapRoute( OrganizationFacility.SubmittalEvent, "{organizationId}/Submittal/Event/{FSID}", new { controller = "Submittal", action = "OrganizationFacilitySubmittalEvent", organizationId = UrlParameter.Optional, FSEID = UrlParameter.Optional } );
            routes.MapRoute( OrganizationFacility.Cancel, "{organizationId}/Facility/{CERSID}/SubmittalCancel/{FSERID}", new { controller = "Submittal", action = "OrganizationFacilityCancel", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSERID = UrlParameter.Optional } );
            routes.MapRoute( OrganizationFacility.SubmittalHistoryWithFacility, "{organizationId}/Submittal/History/Facility/{CERSID}", new { controller = "Submittal", action = "OrganizationFacilitySubmittalHistory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional } );
            routes.MapRoute( OrganizationFacility.SubmittalDetail, "{organizationId}/Facility/{CERSID}/Submittal/{FSEID}", new { controller = "Submittal", action = "OrganizationFacilitySubmittalDetail", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional } );

            //no longer relevant??? [ak 08/10/2014]
            //routes.MapRoute( OrganizationFacility.SubmittalRecentWithFacility, "{organizationId}/Facility/{CERSID}/Submittal/Recent", new { controller = "Submittal", action = "OrganizationFacilitySubmittalRecent", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional } );
            //routes.MapRoute( OrganizationFacility.SubmittalRecent, "{organizationId}/Submittal/Recent/", new { controller = "Submittal", action = "OrganizationFacilitySubmittalRecent", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional } );
        }

		#endregion RegisterSubmittalRoutes Method

		#region Submittal Element Routes

		/// <summary>
		/// Aboveground PEtroleum Storage Tanks related route rules
		/// </summary>
		/// <param name="routes"></param>
		public static void RegisterSubmittalDraftAPSTInfoRoutes( RouteCollection routes )
		{
			routes.MapRoute(
				GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, Part.DraftSubmittal, Part.Start ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/APST/Start/{FSEID}",
			new { controller = "AbovegroundPetroleumStorageTanks", action = "APSTStart", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			);

			#region Aboveground Petroleum Storage Tanks Locally-Required Document

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageTanksLocallyRequired_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/APST/{FSEID}/LRDoc/Detail",
			   new { controller = "AbovegroundPetroleumStorageTanks", action = "Detail_AbovegroundPetroleumStorageTanksLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageTanksLocallyRequired_Document, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/APST/{FSEID}/LRDoc/Detail",
			   new { controller = "AbovegroundPetroleumStorageTanks", action = "Detail_AbovegroundPetroleumStorageTanksLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageTanksLocallyRequired_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/APST/{FSEID}/LRDoc/New",
			   new { controller = "AbovegroundPetroleumStorageTanks", action = "Create_AbovegroundPetroleumStorageTanksLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageTanksLocallyRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/APST/{FSEID}/LRDoc/Edit/{FSERID}",
			new { controller = "AbovegroundPetroleumStorageTanks", action = "Edit_AbovegroundPetroleumStorageTanksLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageTanksLocallyRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/APST/{FSEID}/LRDoc/Delete/{FSERID}",
			new { controller = "AbovegroundPetroleumStorageTanks", action = "Delete_AbovegroundPetroleumStorageTanksLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageTanksLocallyRequired_Document, Part.DraftSubmittal, Part.Landing ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/APST/{FSEID}/LRDoc/Landing/{FSERID}",
			new { controller = "AbovegroundPetroleumStorageTanks", action = "Landing_AbovegroundPetroleumStorageTanksLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Aboveground Petroleum Storage Tanks Locally-Required Document

			#region Aboveground Petroleum Storage Tanks Miscellaneous State Required Document

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.APSAMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/APST/{FSEID}/MSRDoc/Detail",
			   new { controller = "AbovegroundPetroleumStorageTanks", action = "Detail_APSAMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.APSAMiscellaneousStateRequired_Document, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/APST/{FSEID}/MSRDoc/Detail",
			   new { controller = "AbovegroundPetroleumStorageTanks", action = "Detail_APSAMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.APSAMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/APST/{FSEID}/MSRDoc/New",
			   new { controller = "AbovegroundPetroleumStorageTanks", action = "Create_APSAMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.APSAMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/APST/{FSEID}/MSRDoc/Edit/{FSERID}",
			new { controller = "AbovegroundPetroleumStorageTanks", action = "Edit_APSAMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.APSAMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/APST/{FSEID}/MSRDoc/Delete/{FSERID}",
			new { controller = "AbovegroundPetroleumStorageTanks", action = "Delete_APSAMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.APSAMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Landing ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/APST/{FSEID}/MSRDoc/Landing/{FSERID}",
			new { controller = "AbovegroundPetroleumStorageTanks", action = "Landing_APSAMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Aboveground Petroleum Storage Tanks Miscellaneous State Required Document

			#region Aboveground Petroleum Storage Act Documentation Document

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageActDocumentation_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/APST/{FSEID}/APSADoc/Detail",
			   new { controller = "AbovegroundPetroleumStorageTanks", action = "Detail_AbovegroundPetroleumStorageActDocumentation_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageActDocumentation_Document, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/APST/{FSEID}/APSADoc/Detail",
			   new { controller = "AbovegroundPetroleumStorageTanks", action = "Detail_AbovegroundPetroleumStorageActDocumentation_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageActDocumentation_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/APST/{FSEID}/APSADoc/New",
			   new { controller = "AbovegroundPetroleumStorageTanks", action = "Create_AbovegroundPetroleumStorageActDocumentation_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageActDocumentation_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/APST/{FSEID}/APSADoc/Edit/{FSERID}",
			new { controller = "AbovegroundPetroleumStorageTanks", action = "Edit_AbovegroundPetroleumStorageActDocumentation_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageActDocumentation_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/APST/{FSEID}/APSADoc/Delete/{FSERID}",
			new { controller = "AbovegroundPetroleumStorageTanks", action = "Delete_AbovegroundPetroleumStorageActDocumentation_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageActDocumentation_Document, Part.DraftSubmittal, Part.Landing ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/APST/{FSEID}/APSADoc/Landing/{FSERID}",
			new { controller = "AbovegroundPetroleumStorageTanks", action = "Landing_AbovegroundPetroleumStorageActDocumentation_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Aboveground Petroleum Storage Act Documentation Document
		}

		/// <summary>
		/// California Accidental Release Program related route rules.
		/// </summary>
		/// <param name="routes"></param>
		public static void RegisterSubmittalDraftCARPInfoRoutes( RouteCollection routes )
		{
			routes.MapRoute(
				GetRouteName( SubmittalElementType.CaliforniaAccidentalReleaseProgram, Part.DraftSubmittal, Part.Start ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/CALARP/Start",
			new { controller = "CaliforniaAccidentalReleaseProgram", action = "CaliforniaAccidentalReleaseProgramStart", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional }
			);

			#region California Accidental Release Program - Locally Required Documents

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.CaliforniaAccidentalReleaseProgram, ResourceType.CaliforniaAccidentalReleaseProgramLocallyRequired_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/CALARP/{FSEID}/Cert/Detail",
			   new { controller = "CaliforniaAccidentalReleaseProgram", action = "Detail_CaliforniaAccidentalReleaseProgramLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.CaliforniaAccidentalReleaseProgram, ResourceType.CaliforniaAccidentalReleaseProgramLocallyRequired_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/CALARP/{FSEID}/Cert/New",
			   new { controller = "CaliforniaAccidentalReleaseProgram", action = "Create_CaliforniaAccidentalReleaseProgramLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.CaliforniaAccidentalReleaseProgram, ResourceType.CaliforniaAccidentalReleaseProgramLocallyRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/CALARP/{FSEID}/Cert/Edit/{FSERID}",
			new { controller = "CaliforniaAccidentalReleaseProgram", action = "Edit_CaliforniaAccidentalReleaseProgramLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.CaliforniaAccidentalReleaseProgram, ResourceType.CaliforniaAccidentalReleaseProgramLocallyRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/CALARP/{FSEID}/Cert/Delete/{FSERID}",
			new { controller = "CaliforniaAccidentalReleaseProgram", action = "Delete_CaliforniaAccidentalReleaseProgramLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion California Accidental Release Program - Locally Required Documents

			//#region California Accidental Release Program - Miscellaneous-State Required Documents

			//routes.MapRoute(
			//   GetRouteName(SubmittalElementType.CaliforniaAccidentalReleaseProgram, ResourceType.CalARPMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Detail),
			//   "{organizationId}/Facility/{CERSID}/Submittal/Draft/CALARP/{FSEID}/MSRDoc/Detail",
			//   new { controller = "CaliforniaAccidentalReleaseProgram", action = "Detail_CaliforniaAccidentalReleaseProgramMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			//   );

			//routes.MapRoute(
			//   GetRouteName(SubmittalElementType.CaliforniaAccidentalReleaseProgram, ResourceType.CalARPMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Create),
			//   "{organizationId}/Facility/{CERSID}/Submittal/Draft/CALARP/{FSEID}/MSRDoc/New",
			//   new { controller = "CaliforniaAccidentalReleaseProgram", action = "Create_CaliforniaAccidentalReleaseProgramMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			//   );

			//routes.MapRoute(
			//    GetRouteName(SubmittalElementType.CaliforniaAccidentalReleaseProgram, ResourceType.CalARPMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Edit),
			//"{organizationId}/Facility/{CERSID}/Submittal/Draft/CALARP/{FSEID}/MSRDoc/Edit/{FSERID}",
			//new { controller = "CaliforniaAccidentalReleaseProgram", action = "Edit_CaliforniaAccidentalReleaseProgramMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			//);

			//routes.MapRoute(
			//    GetRouteName(SubmittalElementType.CaliforniaAccidentalReleaseProgram, ResourceType.CalARPMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Delete),
			//"{organizationId}/Facility/{CERSID}/Submittal/Draft/CALARP/{FSEID}/MSRDoc/Delete/{FSERID}",
			//new { controller = "CaliforniaAccidentalReleaseProgram", action = "Delete_CaliforniaAccidentalReleaseProgramMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			//);

			//#endregion California Accidental Release Program - Miscellaneous-State Required Documents
		}

		/// <summary>
		/// Emergency response and training plans route rules.
		/// </summary>
		/// <param name="routes"></param>
		public static void RegisterSubmittalDraftERTPlansInfoRoutes( RouteCollection routes )
		{
			routes.MapRoute(
				GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, Part.DraftSubmittal, Part.Start ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/Start/{FSEID}",
			new { controller = "EmergencyResponseAndTrainingPlans", action = "EmergencyResponseAndTrainingPlansStart", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			);

			#region Emergency Response/Contigency Plan

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseContingencyPlan_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/ERCPDoc/Detail",
			   new { controller = "EmergencyResponseAndTrainingPlans", action = "Detail_EmergencyResponseContingencyPlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			 GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseContingencyPlan_Document, Part.Detail ),
			 "{organizationId}/Facility/{CERSID}/Submittal/ERTP/{FSEID}/ERCPDoc/Detail",
			 new { controller = "EmergencyResponseAndTrainingPlans", action = "Detail_EmergencyResponseContingencyPlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			 );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseContingencyPlan_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/ERCPDoc/New",
			   new { controller = "EmergencyResponseAndTrainingPlans", action = "Create_EmergencyResponseContingencyPlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseContingencyPlan_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/ERCPDoc/Edit/{FSERID}",
			new { controller = "EmergencyResponseAndTrainingPlans", action = "Edit_EmergencyResponseContingencyPlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseContingencyPlan_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/ERCPDoc/Delete/{FSERID}",
			new { controller = "EmergencyResponseAndTrainingPlans", action = "Delete_EmergencyResponseContingencyPlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseContingencyPlan_Document, Part.DraftSubmittal, Part.Landing ),
				"{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/ERCPDoc/Landing/{FSERID}",
				new { controller = "EmergencyResponseAndTrainingPlans", action = "Landing_EmergencyResponseContingencyPlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			#endregion Emergency Response/Contigency Plan

			#region Employee Training Plan

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmployeeTrainingPlan_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/ETPDoc/Detail",
			   new { controller = "EmergencyResponseAndTrainingPlans", action = "Detail_EmployeeTrainingPlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
	  GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmployeeTrainingPlan_Document, Part.Detail ),
	  "{organizationId}/Facility/{CERSID}/Submittal/ERTP/{FSEID}/ETPDoc/Detail",
	  new { controller = "EmergencyResponseAndTrainingPlans", action = "Detail_EmployeeTrainingPlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
	  );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmployeeTrainingPlan_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/ETPDoc/New",
			   new { controller = "EmergencyResponseAndTrainingPlans", action = "Create_EmployeeTrainingPlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmployeeTrainingPlan_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/ETPDoc/Edit/{FSERID}",
			new { controller = "EmergencyResponseAndTrainingPlans", action = "Edit_EmployeeTrainingPlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmployeeTrainingPlan_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/ETPDoc/Delete/{FSERID}",
			new { controller = "EmergencyResponseAndTrainingPlans", action = "Delete_EmployeeTrainingPlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmployeeTrainingPlan_Document, Part.DraftSubmittal, Part.Landing ),
				"{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/ETPDoc/Landing/{FSERID}",
				new { controller = "EmergencyResponseAndTrainingPlans", action = "Landing_EmployeeTrainingPlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			#endregion Employee Training Plan

			#region Emergency Response and Training Plans Locally-Required Document

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseAndTrainingPlansLocallyRequired_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/ERTPLRDoc/Detail",
			   new { controller = "EmergencyResponseAndTrainingPlans", action = "Detail_EmergencyResponseAndTrainingPlansLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
		  GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseAndTrainingPlansLocallyRequired_Document, Part.Detail ),
		  "{organizationId}/Facility/{CERSID}/Submittal/ERTP/{FSEID}/ERTPLRDoc/Detail",
		  new { controller = "EmergencyResponseAndTrainingPlans", action = "Detail_EmergencyResponseAndTrainingPlansLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
		  );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseAndTrainingPlansLocallyRequired_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/ERTPLRDoc/New",
			   new { controller = "EmergencyResponseAndTrainingPlans", action = "Create_EmergencyResponseAndTrainingPlansLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseAndTrainingPlansLocallyRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/ERTPLRDoc/Edit/{FSERID}",
			new { controller = "EmergencyResponseAndTrainingPlans", action = "Edit_EmergencyResponseAndTrainingPlansLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseAndTrainingPlansLocallyRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/ERTPLRDoc/Delete/{FSERID}",
			new { controller = "EmergencyResponseAndTrainingPlans", action = "Delete_EmergencyResponseAndTrainingPlansLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseAndTrainingPlansLocallyRequired_Document, Part.DraftSubmittal, Part.Landing ),
				"{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/ERTPLRDoc/Landing/{FSERID}",
				new { controller = "EmergencyResponseAndTrainingPlans", action = "Landing_EmergencyResponseAndTrainingPlansLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			#endregion Emergency Response and Training Plans Locally-Required Document

			#region Emergency Response and Training Plans Miscellaneous State-Required Document

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.ERTPMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/MSRDoc/Detail",
			   new { controller = "EmergencyResponseAndTrainingPlans", action = "Detail_ERTPMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			 GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.ERTPMiscellaneousStateRequired_Document, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/ERTP/{FSEID}/MSRDoc/Detail",
			new { controller = "EmergencyResponseAndTrainingPlans", action = "Detail_ERTPMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.ERTPMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/MSRDoc/New",
			   new { controller = "EmergencyResponseAndTrainingPlans", action = "Create_ERTPMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.ERTPMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/MSRDoc/Edit/{FSERID}",
			new { controller = "EmergencyResponseAndTrainingPlans", action = "Edit_ERTPMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.ERTPMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/ERTP/{FSEID}/MSRDoc/Delete/{FSERID}",
			new { controller = "EmergencyResponseAndTrainingPlans", action = "Delete_ERTPMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Emergency Response and Training Plans Miscellaneous State-Required Document
		}

		/// <summary>
		/// Facility Information related route rules.
		/// </summary>
		/// <param name="routes"></param>
		public static void RegisterSubmittalDraftFacilityInfoRoutes( RouteCollection routes )
		{
			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, Part.DraftSubmittal, Part.Start ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/Start/{FSEID}",
			new { controller = "FacilityInformation", action = "FacilityInfoStart", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			);

			#region Biz Activity

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessActivities, Part.DraftSubmittal, Part.Detail, Part.Print ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/BizActivity/Detail/{FSERID}/Print",
			new { controller = "FacilityInformation", action = "Detail_BusinessActivities_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessActivities, Part.DraftSubmittal, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/BizActivity/Detail/{FSERID}",
			new { controller = "FacilityInformation", action = "Detail_BusinessActivities", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessActivities, Part.Detail, Part.Print ),
			"{organizationId}/Facility/{CERSID}/Submittal/FI/{FSEID}/BizActivity/Detail/{FSERID}/Print",
			new { controller = "FacilityInformation", action = "Detail_BusinessActivities_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessActivities, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/FI/{FSEID}/BizActivity/Detail/{FSERID}",
			new { controller = "FacilityInformation", action = "Detail_BusinessActivities", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessActivities, Part.DraftSubmittal, Part.Create ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/BizActivity/New",
			new { controller = "FacilityInformation", action = "Create_BusinessActivities", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessActivities, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/BizActivity/Edit/{FSERID}",
			new { controller = "FacilityInformation", action = "Edit_BusinessActivities", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessActivities, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/BizActivity/Delete/{FSERID}",
			new { controller = "FacilityInformation", action = "Delete_BusinessActivities", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessActivities, Part.DraftSubmittal, Part.Landing ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/BizActivity/Landing/{FSERID}",
			new { controller = "FacilityInformation", action = "Landing_BusinessActivities", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			//routes.MapRoute(OrganizationFacility.UpdateFacilityLocation_Async, "FacilityInformation/UpdateLocationMap_Async", new { controller = "FacilityInformation", action = "UpdateLocationMap_Async" });

			#endregion Biz Activity

			#region Owner Operator

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessOwnerOperatorIdentification, Part.DraftSubmittal, Part.Detail, Part.Print ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/OwnerOperator/Detail/{FSERID}/Print",
			new { controller = "FacilityInformation", action = "Detail_BusinessOwnerOperatorIdentification_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessOwnerOperatorIdentification, Part.DraftSubmittal, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/OwnerOperator/Detail/{FSERID}",
			new { controller = "FacilityInformation", action = "Detail_BusinessOwnerOperatorIdentification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessOwnerOperatorIdentification, Part.Detail, Part.Print ),
			"{organizationId}/Facility/{CERSID}/Submittal/FI/{FSEID}/OwnerOperator/Detail/{FSERID}/Print",
			new { controller = "FacilityInformation", action = "Detail_BusinessOwnerOperatorIdentification_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessOwnerOperatorIdentification, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/FI/{FSEID}/OwnerOperator/Detail/{FSERID}",
			new { controller = "FacilityInformation", action = "Detail_BusinessOwnerOperatorIdentification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessOwnerOperatorIdentification, Part.DraftSubmittal, Part.Create ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/OwnerOperator/New",
			new { controller = "FacilityInformation", action = "Create_BusinessOwnerOperatorIdentification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessOwnerOperatorIdentification, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/OwnerOperator/Edit/{FSERID}",
			new { controller = "FacilityInformation", action = "Edit_BusinessOwnerOperatorIdentification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessOwnerOperatorIdentification, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/OwnerOperator/Delete/{FSERID}",
			new { controller = "FacilityInformation", action = "Delete_BusinessOwnerOperatorIdentification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.BusinessOwnerOperatorIdentification, Part.DraftSubmittal, Part.Landing ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/OwnerOperator/Landing/{FSERID}",
			new { controller = "FacilityInformation", action = "Landing_BusinessOwnerOperatorIdentification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Owner Operator

			#region Locally required Document

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocallyRequired_Document, Part.DraftSubmittal, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/LRDoc/Detail/{FSERID}",
			new { controller = "FacilityInformation", action = "Detail_FacilityInformationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				 GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocallyRequired_Document, Part.Detail ),
			 "{organizationId}/Facility/{CERSID}/Submittal/FI/{FSEID}/LRDoc/Detail/{FSERID}",
			 new { controller = "FacilityInformation", action = "Detail_FacilityInformationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			 );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocallyRequired_Document, Part.DraftSubmittal, Part.Create ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/LRDoc/New",
			new { controller = "FacilityInformation", action = "Create_FacilityInformationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocallyRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/LRDoc/Edit/{FSERID}",
			new { controller = "FacilityInformation", action = "Edit_FacilityInformationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocallyRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/LRDoc/Delete/{FSERID}",
			new { controller = "FacilityInformation", action = "Delete_FacilityInformationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocallyRequired_Document, Part.DraftSubmittal, Part.Landing ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/LRDoc/Landing/{FSERID}",
			new { controller = "FacilityInformation", action = "Landing_FacilityInformationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Locally required Document

			#region Miscellaneous State-Required Document

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/MSRDoc/Detail/{FSERID}",
			new { controller = "FacilityInformation", action = "Detail_FacilityInformationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				 GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationMiscellaneousStateRequired_Document, Part.Detail ),
			 "{organizationId}/Facility/{CERSID}/Submittal/FI/{FSEID}/MSRDoc/Detail/{FSERID}",
			 new { controller = "FacilityInformation", action = "Detail_FacilityInformationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			 );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Create ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/MSRDoc/New",
			new { controller = "FacilityInformation", action = "Create_FacilityInformationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/MSRDoc/Edit/{FSERID}",
			new { controller = "FacilityInformation", action = "Edit_FacilityInformationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/MSRDoc/Delete/{FSERID}",
			new { controller = "FacilityInformation", action = "Delete_FacilityInformationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Landing ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/MSRDoc/Landing/{FSERID}",
			new { controller = "FacilityInformation", action = "Landing_FacilityInformationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Miscellaneous State-Required Document

			#region Local Fields

			//routes.MapRoute(
			//      GetRouteName(SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocalFields, Part.DraftSubmittal, Part.Detail),
			//      "{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/LocalFields/Detail/{FSERID}",
			//      new { controller = "FacilityInformation", action = "Detail_FacilityInformationLocalFields", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			//      );

			//routes.MapRoute(
			//      GetRouteName(SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocalFields, Part.DraftSubmittal, Part.Create),
			//      "{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/LocalFields/New",
			//      new { controller = "FacilityInformation", action = "Create_FacilityInformationLocalFields", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			//      );

			//routes.MapRoute(
			//    GetRouteName(SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocalFields, Part.DraftSubmittal, Part.Edit),
			//    "{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/LocalFields/Edit/{FSERID}",
			//    new { controller = "FacilityInformation", action = "Edit_FacilityInformationLocalFields", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			//    );

			//routes.MapRoute(
			//    GetRouteName(SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocalFields, Part.DraftSubmittal, Part.Delete),
			//        "{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/LocalFields/Delete/{FSERID}",
			//        new { controller = "FacilityInformation", action = "Delete_FacilityInformationLocalFields", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			//        );

			//routes.MapRoute(
			//    GetRouteName(SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocalFields, Part.DraftSubmittal, Part.Landing),
			//"{organizationId}/Facility/{CERSID}/Submittal/Draft/FI/{FSEID}/LocalFields/Landing/{FSERID}",
			//new { controller = "FacilityInformation", action = "Landing_FacilityInformationLocalFields", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			//);

			#endregion Local Fields
		}

		/// <summary>
		/// Hazardous Material related route rules.
		/// </summary>
		/// <param name="routes"></param>
		public static void RegisterSubmittalDraftHMIInfoRoutes( RouteCollection routes )
		{
			routes.MapRoute(
					GetRouteName( SubmittalElementType.HazardousMaterialsInventory, Part.DraftSubmittal, Part.Start ),
				"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/Start/{FSEID}",
				new { controller = "HazardousMaterialsInventory", action = "HazMatInvStart", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			#region Hazardous Material Inventory

			routes.MapRoute( "Search_GridChemicals_Async", "HazardousMaterialsInventory/Search_GridChemicals_Async", new { controller = "HazardousMaterialsInventory", action = "Search_GridChemicals_Async" } );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Home ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HazMatInventory/{FSERID}",
			new { controller = "HazardousMaterialsInventory", action = "Home_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Home, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/HMI/{FSEID}/HazMatInventory/Detail/{FSERID}",
			new { controller = "HazardousMaterialsInventory", action = "Home_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.New ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HazMatInventory/{FSERID}/New",
			new { controller = "HazardousMaterialsInventory", action = "New_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Create ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HazMatInventory/{FSERID}/New/{CID}/{source}",
			new { controller = "HazardousMaterialsInventory", action = "Create_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional, CID = UrlParameter.Optional, source = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Detail, Part.Print ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HazMatInventory/{FSERID}/Detail/{BPFCID}/Print",
			new { controller = "HazardousMaterialsInventory", action = "Detail_HazardousMaterialInventory_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional, BPFCID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HazMatInventory/{FSERID}/Detail/{BPFCID}",
			new { controller = "HazardousMaterialsInventory", action = "Detail_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional, BPFCID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/HMI/{FSEID}/HazMatInventory/{FSERID}/Detail/{BPFCID}",
			new { controller = "HazardousMaterialsInventory", action = "Detail_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional, BPFCID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HazMatInventory/{FSERID}/Edit/{BPFCID}",
			new { controller = "HazardousMaterialsInventory", action = "Edit_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional, BPFCID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HazMatInventory/{FSERID}/Delete/{BPFCID}",
			new { controller = "HazardousMaterialsInventory", action = "Delete_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional, BPFCID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Upload ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HazMatInventory/{FSERID}/Upload",
			new { controller = "HazardousMaterialsInventory", action = "Upload_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Download ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HazMatInventory/{FSERID}/Download",
			new { controller = "HazardousMaterialsInventory", action = "Download_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Library ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HazMatInventory/{FSERID}/Library/{CCLID}",
			new { controller = "HazardousMaterialsInventory", action = "Library_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional, CCLID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Report ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HazMatInventory/{FSERID}/Report",
			new { controller = "HazardousMaterialsInventory", action = "Report_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.Report, Part.Matrix ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HazMatInventory/{FSERID}/Report_HazardousMaterialInventoryMatrix/{typeID}",
			new { controller = "HazardousMaterialsInventory", action = "Report_HazardousMaterialInventoryMatrix", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional, typeID = UrlParameter.Optional }
			);

			routes.MapRoute( "Report_HazardousMaterialInventoryMatrix_Async", "HazardousMaterialsInventory/Report_HazardousMaterialInventoryMatrix_Async", new { controller = "HazardousMaterialsInventory", action = "Report_HazardousMaterialInventoryMatrix_Async" } );
			routes.MapRoute( "Report_HazardousMaterialInventoryMatrix_Click", "HazardousMaterialsInventory/Report_HazardousMaterialInventoryMatrix_Click", new { controller = "HazardousMaterialsInventory", action = "Report_HazardousMaterialInventoryMatrix_Click" } );

			#region HMIS Reports

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.Report ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HazMatInventory/{FSERID}/Report/HMIS",
			new { controller = "HazardousMaterialsInventory", action = "GenerateHMISReport", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional, TypeID = UrlParameter.Optional, page = UrlParameter.Optional }
			);

			#endregion HMIS Reports

			#region HMIS ReportsHome

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.Report, Part.Home ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HazMatInventory/{FSERID}/Report/HMISHome",
			new { controller = "HazardousMaterialsInventory", action = "HMISReportHome", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion HMIS ReportsHome

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Template ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HazMatInventory/{FSERID}/Template",
			new { controller = "HazardousMaterialsInventory", action = "Template_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Validate ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HazMatInventory/{FSERID}/Validate",
			new { controller = "HazardousMaterialsInventory", action = "Validate_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Upload ) + "ExportGuidanceMessagesToExcel",
				"HazardousMaterialsInventory/ExportGuidanceMessagesToExcel",
				new { controller = "HazardousMaterialsInventory", action = "ExportGuidanceMessagesToExcel" } );

			#endregion Hazardous Material Inventory

			#region General Site Map

			//routes.MapRoute(
			//    GetRouteName(SubmittalElementType.HazardousMaterialsInventory, ResourceType.GeneralSiteMap_Document, Part.DraftSubmittal, Part.Detail),
			//"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/GeneralSiteMap/Detail/{FSERID}",
			//new { controller = "HazardousMaterialsInventory", action = "Detail_GeneralSiteMap_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			//);

			//routes.MapRoute(
			//    GetRouteName(SubmittalElementType.HazardousMaterialsInventory, ResourceType.GeneralSiteMap_Document, Part.DraftSubmittal, Part.Create),
			//"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/GeneralSiteMap/New",
			//new { controller = "HazardousMaterialsInventory", action = "Create_GeneralSiteMap_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			//);

			//routes.MapRoute(
			//    GetRouteName(SubmittalElementType.HazardousMaterialsInventory, ResourceType.GeneralSiteMap_Document, Part.DraftSubmittal, Part.Edit),
			//"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/GeneralSiteMap/Edit/{FSERID}",
			//new { controller = "HazardousMaterialsInventory", action = "Edit_GeneralSiteMap_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			//);

			//routes.MapRoute(
			//    GetRouteName(SubmittalElementType.HazardousMaterialsInventory, ResourceType.GeneralSiteMap_Document, Part.DraftSubmittal, Part.Delete),
			//"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/GeneralSiteMap/Delete/{FSERID}",
			//new { controller = "HazardousMaterialsInventory", action = "Delete_GeneralSiteMap_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			//);

			#endregion General Site Map

			#region Site Map

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.AnnotatedSiteMapOfficialUseOnly_Document, Part.DraftSubmittal, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/SiteMap/Detail/{FSERID}",
			new { controller = "HazardousMaterialsInventory", action = "Detail_AnnotatedSiteMapOfficialUseOnly_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.AnnotatedSiteMapOfficialUseOnly_Document, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/HMI/{FSEID}/SiteMap/Detail/{FSERID}",
			new { controller = "HazardousMaterialsInventory", action = "Detail_AnnotatedSiteMapOfficialUseOnly_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.AnnotatedSiteMapOfficialUseOnly_Document, Part.DraftSubmittal, Part.Create ),
		   "{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/SiteMap/New",
		   new { controller = "HazardousMaterialsInventory", action = "Create_AnnotatedSiteMapOfficialUseOnly_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
		   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.AnnotatedSiteMapOfficialUseOnly_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/SiteMap/Edit/{FSERID}",
			new { controller = "HazardousMaterialsInventory", action = "Edit_AnnotatedSiteMapOfficialUseOnly_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.AnnotatedSiteMapOfficialUseOnly_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/SiteMap/Delete/{FSERID}",
			new { controller = "HazardousMaterialsInventory", action = "Delete_AnnotatedSiteMapOfficialUseOnly_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Site Map

			#region Hazardous Material Inventory Locally Required Document

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventoryLocallyRequired_Document, Part.DraftSubmittal, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HMILRDoc/Detail/{FSERID}",
			new { controller = "HazardousMaterialsInventory", action = "Detail_HazardousMaterialInventoryLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventoryLocallyRequired_Document, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/HMI/{FSEID}/HMILRDoc/Detail/{FSERID}",
			new { controller = "HazardousMaterialsInventory", action = "Detail_HazardousMaterialInventoryLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventoryLocallyRequired_Document, Part.DraftSubmittal, Part.Create ),
		   "{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HMILRDoc/New",
		   new { controller = "HazardousMaterialsInventory", action = "Create_HazardousMaterialInventoryLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
		   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventoryLocallyRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HMILRDoc/Edit/{FSERID}",
			new { controller = "HazardousMaterialsInventory", action = "Edit_HazardousMaterialInventoryLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventoryLocallyRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/HMILRDoc/Delete/{FSERID}",
			new { controller = "HazardousMaterialsInventory", action = "Delete_HazardousMaterialInventoryLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Hazardous Material Inventory Locally Required Document

			#region Hazardous Material Inventory Miscellaneous State-Required Document

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HMIMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/MSRDoc/Detail/{FSERID}",
			new { controller = "HazardousMaterialsInventory", action = "Detail_HMIMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HMIMiscellaneousStateRequired_Document, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/HMI/{FSEID}/MSRDoc/Detail/{FSERID}",
			new { controller = "HazardousMaterialsInventory", action = "Detail_HMIMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HMIMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Create ),
		   "{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/MSRDoc/New",
		   new { controller = "HazardousMaterialsInventory", action = "Create_HMIMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
		   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HMIMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/MSRDoc/Edit/{FSERID}",
			new { controller = "HazardousMaterialsInventory", action = "Edit_HMIMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousMaterialsInventory, ResourceType.HMIMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HMI/{FSEID}/MSRDoc/Delete/{FSERID}",
			new { controller = "HazardousMaterialsInventory", action = "Delete_HMIMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Hazardous Material Inventory Miscellaneous State-Required Document
		}

		/// <summary>
		/// Hazardous Waste Tank Closure Certification related route rules
		/// </summary>
		/// <param name="routes"></param>
		public static void RegisterSubmittalDraftHWTCC( RouteCollection routes )
		{
			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, Part.DraftSubmittal, Part.Start ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HWTCC/Start/{FSEID}",
			new { controller = "HazardousWasteTankClosureCertification", action = "HazWasteTankClosureCertificationStart", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			);

			#region Hazardous Waste Tank Closure Certificate

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificate_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/HWTCC/{FSEID}/Cert/Detail/{FSERID}",
			   new { controller = "HazardousWasteTankClosureCertification", action = "Detail_HazardousWasteTankClosureCertificate_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificate_Document, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/HWTCC/{FSEID}/Cert/Detail/{FSERID}",
			   new { controller = "HazardousWasteTankClosureCertification", action = "Detail_HazardousWasteTankClosureCertificate_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificate_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/HWTCC/{FSEID}/Cert/New",
			   new { controller = "HazardousWasteTankClosureCertification", action = "Create_HazardousWasteTankClosureCertificate_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificate_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HWTCC/{FSEID}/Cert/Edit/{FSERID}",
			new { controller = "HazardousWasteTankClosureCertification", action = "Edit_HazardousWasteTankClosureCertificate_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificate_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HWTCC/{FSEID}/Cert/Delete/{FSERID}",
			new { controller = "HazardousWasteTankClosureCertification", action = "Delete_HazardousWasteTankClosureCertificate_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Hazardous Waste Tank Closure Certificate

			#region Hazardous Waste Tank Closure  Locally Required Doc

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationLocallyRequired_Document, Part.DraftSubmittal, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/Draft/HWTCC/{FSEID}/LRDoc/Detail",
				new { controller = "HazardousWasteTankClosureCertification", action = "Detail_HazardousWasteTankClosureCertificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationLocallyRequired_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/HWTCC/{FSEID}/LRDoc/Detail",
				new { controller = "HazardousWasteTankClosureCertification", action = "Detail_HazardousWasteTankClosureCertificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationLocallyRequired_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/HWTCC/{FSEID}/LRDoc/New",
			   new { controller = "HazardousWasteTankClosureCertification", action = "Create_HazardousWasteTankClosureCertificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationLocallyRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HWTCC/{FSEID}/LRDoc/Edit/{FSERID}",
			new { controller = "HazardousWasteTankClosureCertification", action = "Edit_HazardousWasteTankClosureCertificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationLocallyRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HWTCC/{FSEID}/LRDoc/Delete/{FSERID}",
			new { controller = "HazardousWasteTankClosureCertification", action = "Delete_HazardousWasteTankClosureCertificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Hazardous Waste Tank Closure  Locally Required Doc

			#region Hazardous Waste Tank Closure  Miscellaneous State Required Docs

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/Draft/HWTCC/{FSEID}/MSRDoc/Detail",
				new { controller = "HazardousWasteTankClosureCertification", action = "Detail_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/HWTCC/{FSEID}/MSRDoc/Detail",
				new { controller = "HazardousWasteTankClosureCertification", action = "Detail_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/HWTCC/{FSEID}/MSRDoc/New",
			   new { controller = "HazardousWasteTankClosureCertification", action = "Create_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HWTCC/{FSEID}/MSRDoc/Edit/{FSERID}",
			new { controller = "HazardousWasteTankClosureCertification", action = "Edit_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/HWTCC/{FSEID}/MSRDoc/Delete/{FSERID}",
			new { controller = "HazardousWasteTankClosureCertification", action = "Delete_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Hazardous Waste Tank Closure  Miscellaneous State Required Docs
		}

		/// <summary>
		/// Onsite Hazardous Waste Treatment Notification
		/// </summary>
		/// <param name="routes"></param>
		public static void RegisterSubmittalDraftOHWTNRoutes( RouteCollection routes )
		{
			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, Part.DraftSubmittal, Part.Unavailable ),
				"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/Unavailable",
				new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "OnsiteHazWasteTreatmentNotificationUnavailable", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, Part.DraftSubmittal, Part.Start ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/Start/{FSEID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "OnsiteHazWasteTreatmentNotificationStart", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			);

			#region Onsite Hazardous Waste Treatment Notification Facility

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationFacility, Part.DraftSubmittal, Part.Detail, Part.Print ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/Facility/Detail/{FSERID}/Print",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_OnsiteHazardousWasteTreatmentNotificationFacility_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationFacility, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/Facility/Detail/{FSERID}",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_OnsiteHazardousWasteTreatmentNotificationFacility", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			  GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationFacility, Part.Detail, Part.Print ),
			  "{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/Facility/Detail/{FSERID}/Print",
			  new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_OnsiteHazardousWasteTreatmentNotificationFacility_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			  GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationFacility, Part.Detail ),
			  "{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/Facility/Detail/{FSERID}",
			  new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_OnsiteHazardousWasteTreatmentNotificationFacility", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationFacility, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/Facility/New",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Create_OnsiteHazardousWasteTreatmentNotificationFacility", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationFacility, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/Facility/Edit/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Edit_OnsiteHazardousWasteTreatmentNotificationFacility", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationFacility, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/Facility/Delete/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Delete_OnsiteHazardousWasteTreatmentNotificationFacility", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Onsite Hazardous Waste Treatment Notification Facility

			#region Onsite Hazardous Waste Treatment Notification PlotPlanMap_Document

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentPlotPlanMap_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/PlotPlanMap/Detail",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_OnsiteHazardousWasteTreatmentPlotPlanMap_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			  GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentPlotPlanMap_Document, Part.Detail ),
			  "{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/PlotPlanMap/Detail/{FSERID}",
			  new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_OnsiteHazardousWasteTreatmentPlotPlanMap_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentPlotPlanMap_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/PlotPlanMap/New",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Create_OnsiteHazardousWasteTreatmentPlotPlanMap_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentPlotPlanMap_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/PlotPlanMap/Edit/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Edit_OnsiteHazardousWasteTreatmentPlotPlanMap_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentPlotPlanMap_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/PlotPlanMap/Delete/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Delete_OnsiteHazardousWasteTreatmentPlotPlanMap_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Onsite Hazardous Waste Treatment Notification PlotPlanMap_Document

			#region Onsite Hazardous Waste Treatment Notification Unit

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationUnit, Part.DraftSubmittal, Part.Detail, Part.Print ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/Unit/Detail/{FSERID}/Print",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_OnsiteHazardousWasteTreatmentNotificationUnit_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationUnit, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/Unit/Detail/{FSERID}",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_OnsiteHazardousWasteTreatmentNotificationUnit", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			  GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationUnit, Part.Detail, Part.Print ),
			  "{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/Unit/Detail/{FSERID}/Print",
			  new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_OnsiteHazardousWasteTreatmentNotificationUnit_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			  GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationUnit, Part.Detail ),
			  "{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/Unit/Detail/{FSERID}",
			  new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_OnsiteHazardousWasteTreatmentNotificationUnit", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationUnit, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/Unit/New",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Create_OnsiteHazardousWasteTreatmentNotificationUnit", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationUnit, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/Unit/Edit/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Edit_OnsiteHazardousWasteTreatmentNotificationUnit", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationUnit, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/Unit/Delete/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Delete_OnsiteHazardousWasteTreatmentNotificationUnit", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Onsite Hazardous Waste Treatment Notification Unit

			#region Tiered Permitting Prior Enforcement

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitPriorEnforcementHistory_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/PriorEnforcement/Detail",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_TieredPermittingUnitPriorEnforcementHistory_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitPriorEnforcementHistory_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/PriorEnforcement/Detail/{FSERID}",
				new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_TieredPermittingUnitPriorEnforcementHistory_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitPriorEnforcementHistory_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/PriorEnforcement/New",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Create_TieredPermittingUnitPriorEnforcementHistory_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitPriorEnforcementHistory_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/PriorEnforcement/Edit/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Edit_TieredPermittingUnitPriorEnforcementHistory_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitPriorEnforcementHistory_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/PriorEnforcement/Delete/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Delete_TieredPermittingUnitPriorEnforcementHistory_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Tiered Permitting Prior Enforcement

			#region Tiered Permitting Prior Tank and Container Certification

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitTankandContainerCertification_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/TankContainerCert/Detail",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_TieredPermittingUnitTankandContainerCertification_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitTankandContainerCertification_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/TankContainerCert/Detail/{FSERID}",
				new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_TieredPermittingUnitTankandContainerCertification_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitTankandContainerCertification_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/TankContainerCert/New",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Create_TieredPermittingUnitTankandContainerCertification_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitTankandContainerCertification_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/TankContainerCert/Edit/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Edit_TieredPermittingUnitTankandContainerCertification_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitTankandContainerCertification_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/TankContainerCert/Delete/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Delete_TieredPermittingUnitTankandContainerCertification_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Tiered Permitting Prior Tank and Container Certification

			#region Tiered Permitting Notification of Local Agencies

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/NotificationLocAgency/Detail",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/NotificationLocAgency/Detail/{FSERID}",
				new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/NotificationLocAgency/New",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Create_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/NotificationLocAgency/Edit/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Edit_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/NotificationLocAgency/Delete/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Delete_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Tiered Permitting Notification of Local Agencies

			#region Tiered Permitting Property Owner

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofPropertyOwner_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/PropertyOwner/Detail",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_TieredPermittingUnitNotificationofPropertyOwner_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofPropertyOwner_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/PropertyOwner/Detail/{FSERID}",
				new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_TieredPermittingUnitNotificationofPropertyOwner_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofPropertyOwner_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/PropertyOwner/New",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Create_TieredPermittingUnitNotificationofPropertyOwner_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofPropertyOwner_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/PropertyOwner/Edit/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Edit_TieredPermittingUnitNotificationofPropertyOwner_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofPropertyOwner_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/PropertyOwner/Delete/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Delete_TieredPermittingUnitNotificationofPropertyOwner_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Tiered Permitting Property Owner

			#region Tiered Permitting Property Certification of Financial Assurance

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitCertificationofFinancialAssurance, Part.DraftSubmittal, Part.Detail, Part.Print ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/CertFinancialAssurance/Detail/{FSERID}/Print",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_TieredPermittingUnitCertificationofFinancialAssurance_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitCertificationofFinancialAssurance, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/CertFinancialAssurance/Detail/{FSERID}",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_TieredPermittingUnitCertificationofFinancialAssurance", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitCertificationofFinancialAssurance, Part.Detail, Part.Print ),
				"{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/CertFinancialAssurance/Detail/{FSERID}/Print",
				new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_TieredPermittingUnitCertificationofFinancialAssurance_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitCertificationofFinancialAssurance, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/CertFinancialAssurance/Detail/{FSERID}",
				new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_TieredPermittingUnitCertificationofFinancialAssurance", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitCertificationofFinancialAssurance, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/CertFinancialAssurance/New",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Create_TieredPermittingUnitCertificationofFinancialAssurance", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitCertificationofFinancialAssurance, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/CertFinancialAssurance/Edit/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Edit_TieredPermittingUnitCertificationofFinancialAssurance", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitCertificationofFinancialAssurance, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/CertFinancialAssurance/Delete/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Delete_TieredPermittingUnitCertificationofFinancialAssurance", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Tiered Permitting Property Certification of Financial Assurance

			#region Tiered Permitting Property Certification of Financial Assurance: Written Estimate of Closure Costs

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/WrittenEstimate/Detail",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/WrittenEstimate/Detail/{FSERID}",
				new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/WrittenEstimate/New",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Create_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/WrittenEstimate/Edit/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Edit_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/WrittenEstimate/Delete/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Delete_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Tiered Permitting Property Certification of Financial Assurance: Written Estimate of Closure Costs

			#region Tiered Permitting Property Certification of Financial Assurance: Closure Mechanism

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/ClosureMechanism/Detail",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/ClosureMechanism/Detail/{FSERID}",
				new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/ClosureMechanism/New",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Create_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/ClosureMechanism/Edit/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Edit_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/ClosureMechanism/Delete/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Delete_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Tiered Permitting Property Certification of Financial Assurance: Closure Mechanism

			#region Onsite Hazardous Waste Treatment Locally Required Fields

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/LRDoc/Detail",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/LRDoc/Detail/{FSERID}",
				new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/LRDoc/New",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Create_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/LRDoc/Edit/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Edit_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/LRDoc/Delete/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Delete_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Onsite Hazardous Waste Treatment Locally Required Fields

			#region Onsite Hazardous Waste Treatment Miscellaneous State Required

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/MSRDoc/Detail",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_TieredPermittingMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingMiscellaneousStateRequired_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/MSRDoc/Detail/{FSERID}",
				new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Detail_TieredPermittingMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/MSRDoc/New",
			   new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Create_TieredPermittingMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/MSRDoc/Edit/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Edit_TieredPermittingMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/OHWTN/{FSEID}/MSRDoc/Delete/{FSERID}",
			new { controller = "OnsiteHazardousWasteTreatmentNotification", action = "Delete_TieredPermittingMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Onsite Hazardous Waste Treatment Miscellaneous State Required
		}

		/// <summary>
		/// Recyclable Materials Report related route rules
		/// </summary>
		/// <param name="routes"></param>
		public static void RegisterSubmittalDraftRMRInfoRoutes( RouteCollection routes )
		{
			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, Part.DraftSubmittal, Part.Unavailable ),
				"{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/Unavailable",
				new { controller = "RecyclableMaterialsReport", action = "RecyclableMaterialsReportUnavailable", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, Part.DraftSubmittal, Part.Start ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/Start/{FSEID}",
			new { controller = "RecyclableMaterialsReport", action = "RecyclableMaterialsReportStart", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			);

			#region Recyclable Materials Activities

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsActivities, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/Activities/Detail",
			   new { controller = "RecyclableMaterialsReport", action = "Detail_RecyclableMaterialsActivities", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsActivities, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/Activities/New",
			   new { controller = "RecyclableMaterialsReport", action = "Create_RecyclableMaterialsActivities", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsActivities, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/Activities/Edit/{FSERID}",
			new { controller = "RecyclableMaterialsReport", action = "Edit_RecyclableMaterialsActivities", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsActivities, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/Activities/Delete/{FSERID}",
			new { controller = "RecyclableMaterialsReport", action = "Delete_RecyclableMaterialsActivities", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Recyclable Materials Activities

			#region Recyclable Materials Offsite Generator Identification

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsOffsiteGeneratorIdentification, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/OffsiteGeneratorIdentification/Detail",
			   new { controller = "RecyclableMaterialsReport", action = "Detail_RecyclableMaterialsOffsiteGeneratorIdentification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsOffsiteGeneratorIdentification, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/OffsiteGeneratorIdentification/New",
			   new { controller = "RecyclableMaterialsReport", action = "Create_RecyclableMaterialsOffsiteGeneratorIdentification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsOffsiteGeneratorIdentification, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/OffstiteGeneratorIdentification/Edit/{FSERID}",
			new { controller = "RecyclableMaterialsReport", action = "Edit_RecyclableMaterialsOffsiteGeneratorIdentification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsOffsiteGeneratorIdentification, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/OffsiteGeneratorIdentification/Delete/{FSERID}",
			new { controller = "RecyclableMaterialsReport", action = "Delete_RecyclableMaterialsOffsiteGeneratorIdentification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Recyclable Materials Offsite Generator Identification

			#region Recyclable Materials Materials

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsMaterial, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/Materials/Detail",
			   new { controller = "RecyclableMaterialsReport", action = "Detail_RecyclableMaterialsMaterial", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsMaterial, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/Materials/New",
			   new { controller = "RecyclableMaterialsReport", action = "Create_RecyclableMaterialsMaterial", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsMaterial, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/Materials/Edit/{FSERID}",
			new { controller = "RecyclableMaterialsReport", action = "Edit_RecyclableMaterialsMaterial", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsMaterial, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/Materials/Delete/{FSERID}",
			new { controller = "RecyclableMaterialsReport", action = "Delete_RecyclableMaterialsMaterial", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Recyclable Materials Materials

			#region Recyclable Materials Documentation of Known Market

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsKnownMarket_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/KnownMarket/Detail",
			   new { controller = "RecyclableMaterialsReport", action = "Detail_RecyclableMaterialsKnownMarket_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsKnownMarket_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/KnownMarket/New",
			   new { controller = "RecyclableMaterialsReport", action = "Create_RecyclableMaterialsKnownMarket_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsKnownMarket_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/KnownMarket/Edit/{FSERID}",
			new { controller = "RecyclableMaterialsReport", action = "Edit_RecyclableMaterialsKnownMarket_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsKnownMarket_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/KnownMarket/Delete/{FSERID}",
			new { controller = "RecyclableMaterialsReport", action = "Delete_RecyclableMaterialsKnownMarket_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Recyclable Materials Documentation of Known Market

			#region Recyclable Material Locally Required Documents

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportLocallyRequired_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/LRDoc/Detail",
			   new { controller = "RecyclableMaterialsReport", action = "Detail_RecyclableMaterialsReportLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportLocallyRequired_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/RMR/{FSEID}/LRDoc/Detail",
				new { controller = "RecyclableMaterialsReport", action = "Detail_RecyclableMaterialsReportLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportLocallyRequired_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/LRDoc/New",
			   new { controller = "RecyclableMaterialsReport", action = "Create_RecyclableMaterialsReportLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportLocallyRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/LRDoc/Edit/{FSERID}",
			new { controller = "RecyclableMaterialsReport", action = "Edit_RecyclableMaterialsReportLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportLocallyRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/LRDoc/Delete/{FSERID}",
			new { controller = "RecyclableMaterialsReport", action = "Delete_RecyclableMaterialsReportLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Recyclable Material Locally Required Documents

			#region Recyclable Material Miscellaneous State Required

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/MSRDoc/Detail",
			   new { controller = "RecyclableMaterialsReport", action = "Detail_RecyclableMaterialsReportMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportMiscellaneousStateRequired_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/RMR/{FSEID}/MSRDoc/Detail",
				new { controller = "RecyclableMaterialsReport", action = "Detail_RecyclableMaterialsReportMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/MSRDoc/New",
			   new { controller = "RecyclableMaterialsReport", action = "Create_RecyclableMaterialsReportMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/MSRDoc/Edit/{FSERID}",
			new { controller = "RecyclableMaterialsReport", action = "Edit_RecyclableMaterialsReportMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/MSRDoc/Delete/{FSERID}",
			new { controller = "RecyclableMaterialsReport", action = "Delete_RecyclableMaterialsReportMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Recyclable Material Miscellaneous State Required

			#region Recyclable Material Documentation Documents

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportDocumentation_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/RMRDoc/Detail",
			   new { controller = "RecyclableMaterialsReport", action = "Detail_RecyclableMaterialsReportDocumentation_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportDocumentation_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/RMR/{FSEID}/RMRDoc/Detail",
				new { controller = "RecyclableMaterialsReport", action = "Detail_RecyclableMaterialsReportDocumentation_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportDocumentation_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/RMRDoc/New",
			   new { controller = "RecyclableMaterialsReport", action = "Create_RecyclableMaterialsReportDocumentation_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportDocumentation_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/RMRDoc/Edit/{FSERID}",
			new { controller = "RecyclableMaterialsReport", action = "Edit_RecyclableMaterialsReportDocumentation_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportDocumentation_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RMR/{FSEID}/RMRDoc/Delete/{FSERID}",
			new { controller = "RecyclableMaterialsReport", action = "Delete_RecyclableMaterialsReportDocumentation_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Recyclable Material Documentation Documents
		}

		public static void RegisterSubmittalDraftRoutes( RouteCollection routes )
		{
			RegisterSubmittalDraftFacilityInfoRoutes( routes );
			RegisterSubmittalDraftHMIInfoRoutes( routes );
			RegisterSubmittalDraftUSTInfoRoutes( routes );
			RegisterSubmittalDraftERTPlansInfoRoutes( routes );
			RegisterSubmittalDraftOHWTNRoutes( routes );
			RegisterSubmittalDraftRMRInfoRoutes( routes );
			RegisterSubmittalDraftRWCANInfoRoutes( routes );
			RegisterSubmittalDraftHWTCC( routes );
			RegisterSubmittalDraftAPSTInfoRoutes( routes );
			RegisterSubmittalDraftCARPInfoRoutes( routes );
		}

		/// <summary>
		/// Remote Waste Consolidation Annual Notification Report related route rules
		/// </summary>
		/// <param name="routes"></param>
		public static void RegisterSubmittalDraftRWCANInfoRoutes( RouteCollection routes )
		{
			routes.MapRoute(
				GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, Part.DraftSubmittal, Part.Start ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RWCAN/Start/{FSEID}",
			new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "RemoteWasteConsolidationSiteAnnualNotificationStart", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			);

			#region Remote Waste Consolidation Site Annual Notification

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationSiteAnnualNotification, Part.DraftSubmittal, Part.Detail, Part.Print ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RWCAN/{FSEID}/Notification/Detail/{FSERID}/Print",
			   new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Detail_RemoteWasteConsolidationSiteAnnualNotification_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationSiteAnnualNotification, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RWCAN/{FSEID}/Notification/Detail/{FSERID}",
			   new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Detail_RemoteWasteConsolidationSiteAnnualNotification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationSiteAnnualNotification, Part.Detail, Part.Print ),
			   "{organizationId}/Facility/{CERSID}/Submittal/RWCAN/{FSEID}/Notification/Detail/{FSERID}/Print",
			   new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Detail_RemoteWasteConsolidationSiteAnnualNotification_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationSiteAnnualNotification, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/RWCAN/{FSEID}/Notification/Detail/{FSERID}",
			   new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Detail_RemoteWasteConsolidationSiteAnnualNotification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationSiteAnnualNotification, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RWCAN/{FSEID}/Notification/New",
			   new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Create_RemoteWasteConsolidationSiteAnnualNotification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationSiteAnnualNotification, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RWCAN/{FSEID}/Notification/Edit/{FSERID}",
			new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Edit_RemoteWasteConsolidationSiteAnnualNotification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationSiteAnnualNotification, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RWCAN/{FSEID}/Notification/Delete/{FSERID}",
			new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Delete_RemoteWasteConsolidationSiteAnnualNotification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Remote Waste Consolidation Site Annual Notification

			#region Remote Waste Consolidation Site Annual Notification Locally Required Document

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RWCAN/{FSEID}/LRDoc/Detail",
			   new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Detail_RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/RWCAN/{FSEID}/LRDoc/Detail",
				new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Detail_RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RWCAN/{FSEID}/LRDoc/New",
			   new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Create_RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RWCAN/{FSEID}/LRDoc/Edit/{FSERID}",
			new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Edit_RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RWCAN/{FSEID}/LRDoc/Delete/{FSERID}",
			new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Delete_RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Remote Waste Consolidation Site Annual Notification Locally Required Document

			#region Remote Waste Consolidation Site Annual Notification Miscellaneous State Required Document

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RWCAN/{FSEID}/MSRDoc/Detail",
			   new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Detail_RemoteWasteConsolidationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationMiscellaneousStateRequired_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/RWCAN/{FSEID}/MSRDoc/Detail",
				new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Detail_RemoteWasteConsolidationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/RWCAN/{FSEID}/MSRDoc/New",
			   new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Create_RemoteWasteConsolidationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RWCAN/{FSEID}/MSRDoc/Edit/{FSERID}",
			new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Edit_RemoteWasteConsolidationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/RWCAN/{FSEID}/MSRDoc/Delete/{FSERID}",
			new { controller = "RemoteWasteConsolidationSiteAnnualNotification", action = "Delete_RemoteWasteConsolidationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Remote Waste Consolidation Site Annual Notification Miscellaneous State Required Document
		}

		/// <summary>
		/// Underground Storage Tanks related route rules.
		/// </summary>
		/// <param name="routes"></param>
		public static void RegisterSubmittalDraftUSTInfoRoutes( RouteCollection routes )
		{
			routes.MapRoute(
					GetRouteName( SubmittalElementType.UndergroundStorageTanks, Part.DraftSubmittal, Part.Start ),
				"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/Start/{FSEID}",
				new { controller = "UndergroundStorageTanks", action = "USTStart", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			#region UST Operating Permit Application: Facility Information

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationFacilityInformation, Part.DraftSubmittal, Part.Detail, Part.Print ),
				"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/FacilityInformation/Detail/{FSERID}/Print",
				new { controller = "UndergroundStorageTanks", action = "Detail_USTOperatingPermitApplicationFacilityInformation_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationFacilityInformation, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/FacilityInformation/Detail/{FSERID}",
			   new { controller = "UndergroundStorageTanks", action = "Detail_USTOperatingPermitApplicationFacilityInformation", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationFacilityInformation, Part.Detail, Part.Print ),
				"{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/FacilityInformation/Detail/{FSERID}/Print",
				new { controller = "UndergroundStorageTanks", action = "Detail_USTOperatingPermitApplicationFacilityInformation_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationFacilityInformation, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/FacilityInformation/Detail/{FSERID}",
				new { controller = "UndergroundStorageTanks", action = "Detail_USTOperatingPermitApplicationFacilityInformation", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationFacilityInformation, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/FacilityInformation/New",
			   new { controller = "UndergroundStorageTanks", action = "Create_USTOperatingPermitApplicationFacilityInformation", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationFacilityInformation, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/FacilityInformation/Edit/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Edit_USTOperatingPermitApplicationFacilityInformation", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationFacilityInformation, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/FacilityInformation/Delete/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Delete_USTOperatingPermitApplicationFacilityInformation", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion UST Operating Permit Application: Facility Information

			#region UST Operating Permit Application: Tank Information

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationTankInformation, Part.DraftSubmittal, Part.Detail, Part.Print ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/TankInformation/Detail/{FSERID}/Print",
			   new { controller = "UndergroundStorageTanks", action = "Detail_USTOperatingPermitApplicationTankInformation_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationTankInformation, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/TankInformation/Detail/{FSERID}",
			   new { controller = "UndergroundStorageTanks", action = "Detail_USTOperatingPermitApplicationTankInformation", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationTankInformation, Part.Detail, Part.Print ),
				"{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/TankInformation/Detail/{FSERID}/Print",
				new { controller = "UndergroundStorageTanks", action = "Detail_USTOperatingPermitApplicationTankInformation_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationTankInformation, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/TankInformation/Detail/{FSERID}",
				new { controller = "UndergroundStorageTanks", action = "Detail_USTOperatingPermitApplicationTankInformation", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationTankInformation, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/TankInformation/New",
			   new { controller = "UndergroundStorageTanks", action = "Create_USTOperatingPermitApplicationTankInformation", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationTankInformation, Part.DraftSubmittal, Part.Edit ),
				"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/TankInformation/Edit/{FSERID}",
				new { controller = "UndergroundStorageTanks", action = "Edit_USTOperatingPermitApplicationTankInformation", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationTankInformation, Part.DraftSubmittal, Part.Delete ),
				"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/TankInformation/Delete/{FSERID}",
				new { controller = "UndergroundStorageTanks", action = "Delete_USTOperatingPermitApplicationTankInformation", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			#endregion UST Operating Permit Application: Tank Information

			#region UST Monitoring Plan

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringPlan, Part.DraftSubmittal, Part.Detail, Part.Print ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/MonitoringPlan/Detail/{FSERID}/Print",
			   new { controller = "UndergroundStorageTanks", action = "Detail_USTMonitoringPlan_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringPlan, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/MonitoringPlan/Detail/{FSERID}",
			   new { controller = "UndergroundStorageTanks", action = "Detail_USTMonitoringPlan", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringPlan, Part.Detail, Part.Print ),
			   "{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/MonitoringPlan/Detail/{FSERID}/Print",
			   new { controller = "UndergroundStorageTanks", action = "Detail_USTMonitoringPlan_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringPlan, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/MonitoringPlan/Detail/{FSERID}",
			   new { controller = "UndergroundStorageTanks", action = "Detail_USTMonitoringPlan", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringPlan, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/MonitoringPlan/New/{USTTankInfoFSERID}",
			   new { controller = "UndergroundStorageTanks", action = "Create_USTMonitoringPlan", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, USTTankInfoFSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringPlan, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/MonitoringPlan/Edit/{FSERID}/{copyFromUSTMonitoringPlanID}",
			new { controller = "UndergroundStorageTanks", action = "Edit_USTMonitoringPlan", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional, copyFromUSTMonitoringPlanID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringPlan, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/MonitoringPlan/Delete/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Delete_USTMonitoringPlan", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute( UndergroundStorageTankRoute.MonitoringPlan_Ajax_SelectOneToCopy,
				"UndergroundStorageTanks/Ajax_USTMonitoringPlan_CopySelectOneGrid",
				new { controller = "UndergroundStorageTanks", action = "Ajax_USTMonitoringPlan_CopySelectOneGrid", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute( UndergroundStorageTankRoute.MonitoringPlan_Ajax_SelectManyToReplace,
				"UndergroundStorageTanks/Ajax_USTMonitoringPlan_ReplaceSelectManyGrid",
				new { controller = "UndergroundStorageTanks", action = "Ajax_USTMonitoringPlan_ReplaceSelectManyGrid", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute( UndergroundStorageTankRoute.TankInfo_Ajax_SelectOneToCopy,
				"UndergroundStorageTanks/Ajax_USTTankInfo_CopySelectOneGrid",
				new { controller = "UndergroundStorageTanks", action = "Ajax_USTTankInfo_CopySelectOneGrid", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			#endregion UST Monitoring Plan

			#region UST Certification of Installation/Modification

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofInstallationModification, Part.DraftSubmittal, Part.Detail, Part.Print ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/CertInstMod/Detail/{FSERID}/Print",
			   new { controller = "UndergroundStorageTanks", action = "Detail_USTCertificationofInstallationModification_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofInstallationModification, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/CertInstMod/Detail/{FSERID}",
			   new { controller = "UndergroundStorageTanks", action = "Detail_USTCertificationofInstallationModification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			  GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofInstallationModification, Part.Detail, Part.Print ),
			  "{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/CertInstMod/Detail/{FSERID}/Print",
			  new { controller = "UndergroundStorageTanks", action = "Detail_USTCertificationofInstallationModification_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			  GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofInstallationModification, Part.Detail ),
			  "{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/CertInstMod/Detail/{FSERID}",
			  new { controller = "UndergroundStorageTanks", action = "Detail_USTCertificationofInstallationModification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofInstallationModification, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/CertInstMod/New",
			   new { controller = "UndergroundStorageTanks", action = "Create_USTCertificationofInstallationModification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofInstallationModification, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/CertInstMod/Edit/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Edit_USTCertificationofInstallationModification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofInstallationModification, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/CertInstMod/Delete/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Delete_USTCertificationofInstallationModification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion UST Certification of Installation/Modification

			#region UST Monitoring Site Plan

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringSitePlan_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/MonitoringSitePlan/Detail",
			   new { controller = "UndergroundStorageTanks", action = "Detail_USTMonitoringSitePlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
		   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringSitePlan_Document, Part.Detail ),
		   "{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/MonitoringSitePlan/Detail",
		   new { controller = "UndergroundStorageTanks", action = "Detail_USTMonitoringSitePlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
		   );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringSitePlan_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/MonitoringSitePlan/New",
			   new { controller = "UndergroundStorageTanks", action = "Create_USTMonitoringSitePlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringSitePlan_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/MonitoringSitePlan/Edit/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Edit_USTMonitoringSitePlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringSitePlan_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/MonitoringSitePlan/Delete/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Delete_USTMonitoringSitePlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion UST Monitoring Site Plan

			#region UST Certificatin of Financial Responsibility

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofFinancialResponsibility_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/CertificationFinancialResponsibility/Detail",
			   new { controller = "UndergroundStorageTanks", action = "Detail_USTCertificationofFinancialResponsibility_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofFinancialResponsibility_Document, Part.Detail ),
			"{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/CertificationFinancialResponsibility/Detail",
			new { controller = "UndergroundStorageTanks", action = "Detail_USTCertificationofFinancialResponsibility_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofFinancialResponsibility_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/CertificationFinancialResponsibility/New",
			   new { controller = "UndergroundStorageTanks", action = "Create_USTCertificationofFinancialResponsibility_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofFinancialResponsibility_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/CertificationFinancialResponsibility/Edit/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Edit_USTCertificationofFinancialResponsibility_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofFinancialResponsibility_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/CertificationFinancialResponsibility/Delete/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Delete_USTCertificationofFinancialResponsibility_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion UST Certificatin of Financial Responsibility

			#region UST Response Plan

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTResponsePlan_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/ResponsePlan/Detail",
			   new { controller = "UndergroundStorageTanks", action = "Detail_USTResponsePlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			  GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTResponsePlan_Document, Part.Detail ),
			  "{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/ResponsePlan/Detail",
			  new { controller = "UndergroundStorageTanks", action = "Detail_USTResponsePlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTResponsePlan_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/ResponsePlan/New",
			   new { controller = "UndergroundStorageTanks", action = "Create_USTResponsePlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTResponsePlan_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/ResponsePlan/Edit/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Edit_USTResponsePlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTResponsePlan_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/ResponsePlan/Delete/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Delete_USTResponsePlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion UST Response Plan

			#region UST Owner and UST Operator: Written Agreement

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOwnerandUSTOperatorWrittenAgreement_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/WrittenAgreement/Detail",
			   new { controller = "UndergroundStorageTanks", action = "Detail_USTOwnerandUSTOperatorWrittenAgreement_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			  GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOwnerandUSTOperatorWrittenAgreement_Document, Part.Detail ),
			  "{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/WrittenAgreement/Detail",
			  new { controller = "UndergroundStorageTanks", action = "Detail_USTOwnerandUSTOperatorWrittenAgreement_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOwnerandUSTOperatorWrittenAgreement_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/WrittenAgreement/New",
			   new { controller = "UndergroundStorageTanks", action = "Create_USTOwnerandUSTOperatorWrittenAgreement_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOwnerandUSTOperatorWrittenAgreement_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/WrittenAgreement/Edit/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Edit_USTOwnerandUSTOperatorWrittenAgreement_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOwnerandUSTOperatorWrittenAgreement_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/WrittenAgreement/Delete/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Delete_USTOwnerandUSTOperatorWrittenAgreement_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion UST Owner and UST Operator: Written Agreement

			#region UST Letter from the Chief Financial Officer

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLetterfromtheChiefFinancialOfficer_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/LetterCFO/Detail",
			   new { controller = "UndergroundStorageTanks", action = "Detail_USTLetterfromtheChiefFinancialOfficer_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			  GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLetterfromtheChiefFinancialOfficer_Document, Part.Detail ),
			  "{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/LetterCFO/Detail",
			  new { controller = "UndergroundStorageTanks", action = "Detail_USTLetterfromtheChiefFinancialOfficer_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLetterfromtheChiefFinancialOfficer_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/LetterCFO/New",
			   new { controller = "UndergroundStorageTanks", action = "Create_USTLetterfromtheChiefFinancialOfficer_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLetterfromtheChiefFinancialOfficer_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/LetterCFO/Edit/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Edit_USTLetterfromtheChiefFinancialOfficer_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLetterfromtheChiefFinancialOfficer_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/LetterCFO/Delete/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Delete_USTLetterfromtheChiefFinancialOfficer_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion UST Letter from the Chief Financial Officer

			#region UST Owner Statement of Designated USTOperatorCompliance Document

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.OwnerStatementofDesignatedUSTOperatorCompliance_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/OperatorCompliance/Detail",
			   new { controller = "UndergroundStorageTanks", action = "Detail_OwnerStatementofDesignatedUSTOperatorCompliance_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			  GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.OwnerStatementofDesignatedUSTOperatorCompliance_Document, Part.Detail ),
			  "{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/OperatorCompliance/Detail",
			  new { controller = "UndergroundStorageTanks", action = "Detail_OwnerStatementofDesignatedUSTOperatorCompliance_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.OwnerStatementofDesignatedUSTOperatorCompliance_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/OperatorCompliance/New",
			   new { controller = "UndergroundStorageTanks", action = "Create_OwnerStatementofDesignatedUSTOperatorCompliance_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.OwnerStatementofDesignatedUSTOperatorCompliance_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/OperatorCompliance/Edit/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Edit_OwnerStatementofDesignatedUSTOperatorCompliance_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.OwnerStatementofDesignatedUSTOperatorCompliance_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/OperatorCompliance/Delete/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Delete_OwnerStatementofDesignatedUSTOperatorCompliance_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion UST Owner Statement of Designated USTOperatorCompliance Document

			#region UST LocallyRequired Document

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLocallyRequired_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/LRDoc/Detail",
			   new { controller = "UndergroundStorageTanks", action = "Detail_USTLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLocallyRequired_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/LRDoc/Detail",
				new { controller = "UndergroundStorageTanks", action = "Detail_USTLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLocallyRequired_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/LRDoc/New",
			   new { controller = "UndergroundStorageTanks", action = "Create_USTLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLocallyRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/LRDoc/Edit/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Edit_USTLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLocallyRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/LRDoc/Delete/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Delete_USTLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion UST LocallyRequired Document

			#region UST Miscellaneous State-Required Document

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Detail ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/MSRDoc/Detail",
			   new { controller = "UndergroundStorageTanks", action = "Detail_USTMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMiscellaneousStateRequired_Document, Part.Detail ),
				"{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/MSRDoc/Detail",
				new { controller = "UndergroundStorageTanks", action = "Detail_USTMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Create ),
			   "{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/MSRDoc/New",
			   new { controller = "UndergroundStorageTanks", action = "Create_USTMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Edit ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/MSRDoc/Edit/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Edit_USTMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName( SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMiscellaneousStateRequired_Document, Part.DraftSubmittal, Part.Delete ),
			"{organizationId}/Facility/{CERSID}/Submittal/Draft/UST/{FSEID}/MSRDoc/Delete/{FSERID}",
			new { controller = "UndergroundStorageTanks", action = "Delete_USTMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion UST Miscellaneous State-Required Document
		}

		#endregion Submittal Element Routes

		#region RegisterEventProcessingRoutes

		public static void RegisterEventProcessingRoutes( RouteCollection routes )
		{
			routes.MapRoute( EventProcessingRoute.OrganizationPortal_Detail,
				"{organizationId}/Event/{eventId}",
				new { controller = "EventProcessing", action = "Detail", organizationId = UrlParameter.Optional, eventId = UrlParameter.Optional }
				);

			routes.MapRoute( EventProcessingRoute.OrganizationPortal_ProcessOAR,
				"{organizationId}/Event/{eventId}/OAR",
				new { controller = "EventProcessing", action = "ProcessOrganizationAccessRequest", organizationId = UrlParameter.Optional, eventId = UrlParameter.Optional }
				);

			//routes.MapRoute(EventProcessingRoute.ProcessOAR,
			//"Event/{eventId}/RAR",
			//new { controller = "EventProcessing", action = "ProcessRegulatorAccessRequest", eventId = UrlParameter.Optional }
			//);
		}

		#endregion RegisterEventProcessingRoutes

		#region RegisterEnhancementRoutes

		public static void RegisterEnhancementRoutes( RouteCollection routes )
		{
            routes.MapRoute( EnhancementRoute.Index, "Enhancement", new { controller = "Enhancement", action = "Index" } );
            routes.MapRoute( EnhancementRoute.Index_Grid, "Enhancement/Index_Grid", new { controller = "Enhancement", action = "Index_Grid" } );
            routes.MapRoute( EnhancementRoute.MyIndex, "Enhancement/MyIndex", new { controller = "Enhancement", action = "MyIndex" } );
            routes.MapRoute( EnhancementRoute.MyIndex_Grid, "Enhancement/MyIndex_Grid", new { controller = "Enhancement", action = "MyIndex_Grid" } );
            routes.MapRoute( EnhancementRoute.Add, "Enhancement/Add", new { controller = "Enhancement", action = "Add" } );
            routes.MapRoute( EnhancementRoute.Submit, "Enhancement/Submit", new { controller = "Enhancement", action = "Submit" } );
            routes.MapRoute( EnhancementRoute.SubmitReceive, "Enhancement/Submit/Receive", new { controller = "Enhancement", action = "SubmitReceive" } );
            routes.MapRoute( EnhancementRoute.Detail, "Enhancement/{id}", new { controller = "Enhancement", action = "Detail" } );
            routes.MapRoute( EnhancementRoute.Detail_Grid, "Enhancement/Detail_Grid/{id}", new { controller = "Enhancement", action = "Detail_Grid" } );
            routes.MapRoute( EnhancementRoute.Edit, "Enhancement/{id}/Edit", new { controller = "Enhancement", action = "Edit" } );
            routes.MapRoute( EnhancementRoute.CommentDetail, "Enhancement/Comment/{commentID}", new { controller = "Enhancement", action = "CommentDetail" } );
            routes.MapRoute( EnhancementRoute.CommentEdit, "Enhancement/Comment/{commentID}/Edit", new { controller = "Enhancement", action = "CommentEdit" } );
            routes.MapRoute( EnhancementRoute.CommentDelete, "Enhancement/Comment/{commentID}/Delete", new { controller = "Enhancement", action = "CommentDelete" } );
            routes.MapRoute( EnhancementRoute.CommentAdd, "Enhancement/{enhancementID}/Comment/Add", new { controller = "Enhancement", action = "CommentAdd" } );
        }

		#endregion RegisterEnhancementRoutes

		#region RegisterRevisionRoutes

		public static void RegisterRevisionRoutes( RouteCollection routes )
		{
			routes.MapRoute( RevisionRoute.Index,
				"Revisions",
				new { controller = "Revision", action = "Index" }
			  );
		}

		#endregion RegisterRevisionRoutes

		#region GetRouteName Methods

		public static string GetRouteName( params Part[] parts )
		{
			return RouteHelper.GetRouteName( parts );
		}

		public static string GetRouteName( SubmittalElementType submittalElement, params Part[] parts )
		{
			return RouteHelper.GetRouteName( submittalElement, parts );
		}

		public static string GetRouteName( SubmittalElementType submittalElement, ResourceType resourceType, params Part[] parts )
		{
			return RouteHelper.GetRouteName( submittalElement, resourceType, parts );
		}

		#endregion GetRouteName Methods

		#region Application_Start Method

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters( GlobalFilters.Filters );
			RegisterRoutes( RouteTable.Routes );
			ModelMetadataProviders.Current = new CERSModelMetadataProvider();

			//cannot make it work properly, revisit later.
			//ModelBinders.Binders.DefaultBinder = new StringTrimmingBinder();
			DataRegistry.Initialize();
			LicenseKeys.Intialize();
			ApplicationOnStartShared.Initialize();

            //register the custom model binder
            //ModelBinders.Binders.Add(typeof(string), new CleanModelBinder());
		}

		#endregion Application_Start Method

		#region RegisterGlobalFilters Method

		public static void RegisterGlobalFilters( GlobalFilterCollection filters )
		{
			filters.Add( new CERSHandleErrorAttribute() );
			filters.Add( new VerifyCERSIDAttribute() );
			filters.Add( new BrowserCheckAttribute() );
		}

		#endregion RegisterGlobalFilters Method
	}
}