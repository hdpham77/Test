using CERS.Web.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CERS.Web.UI.Regulator
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode,
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		#region RegisterGlobalFilters Methods

		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new CERSHandleErrorAttribute());
			filters.Add(new BrowserCheckAttribute());

			//filters.Add(new VerifyCERSIDAttribute());
		}

		#endregion RegisterGlobalFilters Methods

		#region RegisterAccountRoutes Method

		public static void RegisterAccountRoutes(RouteCollection routes)
		{
			routes.RegisterAccountManagementRoutes();

			routes.MapRoute(AccountManagement.Search, "Account/Search", new { controller = "Account", action = "Search" });
			routes.MapRoute(AccountManagement.Summary, "Account/Summary/{ID}", new { controller = "Account", action = "Summary", ID = UrlParameter.Optional });
			routes.MapRoute(AccountManagement.AdminDetails, "Account/Details/{ID}", new { controller = "Account", action = "AdminDetail", ID = UrlParameter.Optional });
			routes.MapRoute(AccountManagement.Associations, "Account/Associations/{ID}", new { controller = "Account", action = "Associations", ID = UrlParameter.Optional });
			routes.MapRoute(AccountManagement.AdminEdit, "Account/{ID}/Admin/Edit", new { controller = "Account", action = "AdminEdit", ID = UrlParameter.Optional });
			routes.MapRoute(AccountManagement.AdminDelete, "Account/{ID}/Admin/Delete", new { controller = "Account", action = "AdminDelete", ID = UrlParameter.Optional });
			routes.MapRoute(AccountManagement.AdminDeleteCompleted, "Account/{ID}/Admin/Deleted", new { controller = "Account", action = "AdminDeleteCompleted", ID = UrlParameter.Optional });
			routes.MapRoute(AccountManagement.AdminChangePassword, "Account/{ID}/Admin/ChangePassword", new { controller = "Account", action = "AdminChangePassword", ID = UrlParameter.Optional });
			routes.MapRoute(AccountManagement.AdminChangePasswordCompleted, "Account/{ID}/Admin/ChangePasswordCompleted", new { controller = "Account", action = "AdminChangePasswordCompleted", ID = UrlParameter.Optional });
			routes.MapRoute(AccountManagement.AdminSignInHistory, "Account/{ID}/Admin/SignInHistory", new { controller = "Account", action = "AdminSignInHistory", ID = UrlParameter.Optional });
			routes.MapRoute(AccountManagement.AdminEmailHistory, "Account/{ID}/Admin/EmailHistory", new { controller = "Account", action = "AdminEmailHistory", ID = UrlParameter.Optional });
		}

		#endregion RegisterAccountRoutes Method

		#region RegisterOrganizationManagementRoutes Method

		public static void RegisterOrganizationManagementRoutes(RouteCollection routes)
		{
			routes.MapRoute(OrganizationManage.Search, "Business/Search", new { controller = "Organization", action = "Search" });
			routes.MapRoute(OrganizationManage.Summary, "Business/{organizationId}", new { controller = "Organization", action = "Index", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.Edit, "Business/{organizationId}/Edit", new { controller = "Organization", action = "Edit", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.Notifications, "Business/{organizationId}/Notifications", new { controller = "Organization", action = "Notifications", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.ActionRequired, "Business/{organizationId}/ActionRequired", new { controller = "Organization", action = "ActionRequired", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.EmailHistory, "Business/{organizationId}/EmailHistory", new { controller = "Organization", action = "EmailHistory", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.ArchivedFacilities, "Business/{organizationId}/ArchivedFacilities", new { controller = "Organization", action = "ArchivedFacilities", organizationId = UrlParameter.Optional });

			#region People Related

			routes.MapRoute(OrganizationManage.People, "Business/{organizationId}/People", new { controller = "Organization", action = "People", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.PersonAdd, "Business/{organizationId}/Person/Add", new { controller = "Organization", action = "AddPerson", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.PersonEdit, "Business/{organizationId}/Person/{pid}/Edit", new { controller = "Organization", action = "EditPerson", organizationId = UrlParameter.Optional, pid = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.PersonDelete, "Business/{organizationId}/Person/{pid}/Delete", new { controller = "Organization", action = "DeletePerson", organizationId = UrlParameter.Optional, pid = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.PersonDetail, "Business/{organizationId}/Person/{pid}", new { controller = "Organization", action = "PersonDetails", organizationId = UrlParameter.Optional, pid = UrlParameter.Optional });

			#endregion People Related

			#region Transfer/Merge/Delete

			routes.MapRoute(OrganizationManage.FacilityDeleteRequestTarget, "Business/{organizationId}/FacilityDeleteRequestTarget", new { controller = "Organization", action = "FacilityDeleteRequestTarget", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.FacilityDeleteRequestConfirm, "Business/{organizationId}/FacilityDeleteRequestConfirm/{targetCERSID}", new { controller = "Organization", action = "FacilityDeleteRequestConfirm", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.FacilityMergeRequestSource, "Business/{organizationId}/FacilityMergeRequestSource", new { controller = "Organization", action = "FacilityMergeRequestSource", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.FacilityMergeRequestTarget, "Business/{organizationId}/FacilityMergeRequestTarget/{sourceCERSID}", new { controller = "Organization", action = "FacilityMergeRequestTarget", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.FacilityMergeRequestConfirm, "Business/{organizationId}/FacilityMergeRequestConfirm/{sourceCERSID}/{targetCERSID}", new { controller = "Organization", action = "FacilityMergeRequestConfirm", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.FacilityTransferRequestSource, "Business/{organizationId}/FacilityTransferRequestSource", new { controller = "Organization", action = "FacilityTransferRequestSource", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.FacilityTransferRequestTarget, "Business/{organizationId}/FacilityTransferRequestTarget/{sourceCERSID}", new { controller = "Organization", action = "FacilityTransferRequestTarget", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.FacilityTransferRequestConfirm, "Business/{organizationId}/FacilityTransferRequestConfirm/{sourceCERSID}/{targetOrganizationId}", new { controller = "Organization", action = "FacilityTransferRequestConfirm", organizationId = UrlParameter.Optional });

			#endregion Transfer/Merge/Delete

			routes.MapRoute(OrganizationManage.AccessRequestProcess, "Business/{organizationId}/AccessRequest/{eventId}/Process", new { controller = "Organization", action = "AccessRequestProcess", organizationId = UrlParameter.Optional, eventId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.Facilities, "Business/{organizationId}/Facilities", new { controller = "Organization", action = "Facilities", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.Submittals, "Business/{organizationId}/Submittals", new { controller = "Organization", action = "Submittals", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.Compliance, "Business/{organizationId}/Compliance", new { controller = "Organization", action = "Compliance", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.Documents, "Business/{organizationId}/Documents", new { controller = "Organization", action = "Documents", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.AddDocument, "Business/{organizationId}/Documents/Add", new { controller = "Organization", action = "AddDocument", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.DeleteDocument, "Business/{organizationId}/Documents/Delete/{documentId}", new { controller = "Organization", action = "DeleteDocument", organizationId = UrlParameter.Optional, documentId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.EditDocument, "Business/{organizationId}/Documents/Edit/{documentId}", new { controller = "Organization", action = "EditDocument", organizationId = UrlParameter.Optional, documentId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.Inspections, "Business/{organizationId}/Inspections", new { controller = "Organization", action = "Inspections", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.Enforcements, "Business/{organizationId}/Enforcements", new { controller = "Organization", action = "Enforcements", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.Violations, "Business/{organizationId}/Violations", new { controller = "Organization", action = "Violations", organizationId = UrlParameter.Optional });
			routes.MapRoute(OrganizationManage.SubmittalHistoryGrid_Async, "Organization/SubmittalHistory_GridAction", new { controller = "Organization", action = "SubmittalHistory_GridAction", organizationId = UrlParameter.Optional });
		}

		#endregion RegisterOrganizationManagementRoutes Method

		#region RegisterRegulatorManagementRoutes

		public static void RegisterRegulatorManagementRoutes(RouteCollection routes)
		{
			routes.MapRoute(RegulatorManage.Notifications, "{regulatorId}/Notifications", new { controller = "Regulator", action = "Notifications", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.ActionRequired, "{regulatorId}/ActionRequired", new { controller = "Regulator", action = "ActionRequired", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.EmailHistory, "{regulatorId}/EmailHistory", new { controller = "Regulator", action = "EmailHistory", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.LocalRequirements, "{regulatorId}/LocalRequirements", new { controller = "Regulator", action = "LocalRequirements", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.EditLocalRequirements, "{regulatorId}/EditLocalRequirements/{regulatorLocalID}", new { controller = "Regulator", action = "EditLocalRequirements", regulatorId = UrlParameter.Optional, regulatorLocalID = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.Compliance, "{regulatorId}/Compliance", new { controller = "Regulator", action = "Compliance", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.GeographicData, "{regulatorId}/GeographicData", new { controller = "Regulator", action = "GeographicData", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.Documents, "{regulatorId}/Documents", new { controller = "Regulator", action = "Documents", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.AddDocument, "{regulatorId}/Documents/Add", new { controller = "Regulator", action = "AddDocument", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.EditDocument, "{regulatorId}/Documents/Edit/{regulatorDocumentID}", new { controller = "Regulator", action = "EditDocument", regulatorId = UrlParameter.Optional, regulatorDocumentID = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.DeleteDocument, "{regulatorId}/Documents/Delete/{regulatorDocumentID}", new { controller = "Regulator", action = "DeleteDocument", regulatorId = UrlParameter.Optional, regulatorDocumentID = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.Inspections, "{regulatorId}/Inspections", new { controller = "Regulator", action = "Inspections", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.Enforcements, "{regulatorId}/Enforcements", new { controller = "Regulator", action = "Enforcements", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.Violations, "{regulatorId}/Violations", new { controller = "Regulator", action = "Violations", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.AccessRequestProcess, "{regulatorId}/AccessRequest/{eventId}/Process", new { controller = "Regulator", action = "AccessRequestProcess", regulatorId = UrlParameter.Optional, eventId = UrlParameter.Optional });
            routes.MapRoute(RegulatorManage.FacilityInspectionStatus, "Facility/FacilityInspectionStatus", new { controller = "Facility", action = "FacilityInspectionStatus" } );
            routes.MapRoute(RegulatorManage.ManageFacilities, "Facility/{CERSID}/Manage", new { controller = "Facility", action = "Manage", CERSID = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.Facilities, "Facility", new { controller = "Facility", action = "Index" });
			routes.MapRoute(RegulatorManage.DirectoryInfo, "{regulatorId}/DirectoryInfo", new { controller = "Regulator", action = "DirectoryInfo", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.EditDirectoryInfo, "{regulatorId}/EditDirectoryInfo", new { controller = "Regulator", action = "EditDirectoryInfo", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.RemoveRegulatorContact, "{regulatorId}/RemoveRegulatorContact/{regulatorContactID}", new { controller = "Regulator", action = "RemoveRegulatorContact", regulatorId = UrlParameter.Optional, regulatorContactID = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.EDTDashboard, "{regulatorId}/EDT", new { controller = "Regulator", action = "EDT", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.EDTAdmin, "{regulatorId}/EDT/Admin", new { controller = "Regulator", action = "EDTAdmin", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.EDTTransactionXml, "{regulatorId}/EDT/Transaction/{key}/Xml/{transactionXmlId}", new { controller = "Regulator", action = "DownloadTransactionXml", regulatorId = UrlParameter.Optional, key = UrlParameter.Optional, transactionXmlId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.EDTTransactionDetail, "{regulatorId}/EDT/Transaction/{key}", new { controller = "Regulator", action = "EDTTransactionDetails", regulatorId = UrlParameter.Optional, key = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.EDTTransactionExport, "{regulatorId}/EDT/Transactions/Export", new { controller = "Regulator", action = "EDTTransactionsExport", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.EDTTransactionSearch, "{regulatorId}/EDT/Transactions", new { controller = "Regulator", action = "EDTTransactions", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.NonRegulatedFacilities, "{regulatorId}/NonRegulatedFacilities", new { controller = "Regulator", action = "NonRegulatedFacilities", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.RetrieveScheduledDeferredJobDocument, "{regulatorId}/RetrieveScheduledDeferredJobDocument/{deferredJobID}", new { controller = "Regulator", action = "RetrieveScheduledDeferredJobDocument", deferredJobID = UrlParameter.Optional });

			#region People Related

			routes.MapRoute(RegulatorManage.Search, "Search", new { controller = "Regulator", action = "Index" });
			routes.MapRoute(RegulatorManage.People, "{regulatorId}/People", new { controller = "Regulator", action = "People", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.PersonAdd, "{regulatorId}/Person/Add", new { controller = "Regulator", action = "AddPerson", regulatorId = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.PersonEdit, "{regulatorId}/Person/{pid}/Edit", new { controller = "Regulator", action = "EditPerson", regulatorId = UrlParameter.Optional, pid = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.PersonDelete, "{regulatorId}/Person/{pid}/Delete", new { controller = "Regulator", action = "DeletePerson", regulatorId = UrlParameter.Optional, pid = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.PersonDetail, "{regulatorId}/Person/{pid}", new { controller = "Regulator", action = "PersonDetails", regulatorId = UrlParameter.Optional, pid = UrlParameter.Optional });

			#endregion People Related

			#region Transfer/Merge/Delete

			routes.MapRoute("ExportToExcelFacilityListingBrief", "Facility/ExportToExcelFacilityListingBrief", new { controller = "Facility", action = "ExportToExcelFacilityListingBrief" });
			routes.MapRoute("ExportToExcelFacilityListingWithDetails", "Facility/ExportToExcelFacilityListingWithDetails", new { controller = "Facility", action = "ExportToExcelFacilityListingWithDetails" });
			routes.MapRoute("OrganizationSearchGrid", "Facility/Search_GridBindingOrganization", new { controller = "Facility", action = "Search_GridBindingOrganization" });
			routes.MapRoute("FacilityMergeSearchGrid", "Facility/Search_GridBindingFacilityMerge", new { controller = "Facility", action = "Search_GridBindingFacilityMerge" });
			routes.MapRoute("SharedFacilitiesGrid", "Facility/GetOrgFacilities", new { controller = "Facility", action = "GetOrgFacilities" });
			routes.MapRoute("SharedOrgUsersGrid", "Facility/GetOrgUsers", new { controller = "Facility", action = "GetOrgUsers" });
			routes.MapRoute("GetRegulatorEdtIdentityKey", "Facility/GetRegulatorIdentityKey", new { controller = "Facility", action = "GetRegulatorIdentityKey" });
			routes.MapRoute(RegulatorManage.DeactivateOrganization, "Facility/{CERSID}/DeactivateOrganization/{organizationID}", new { controller = "Facility", action = "DeactivateOrganization", CERSID = UrlParameter.Optional, organizationID = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.FacilityDeleteTarget, "Facility/{CERSID}/FacilityDeleteTarget", new { controller = "Facility", action = "FacilityDeleteTarget", CERSID = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.FacilityDeleteConfirm, "Facility/{CERSID}/FacilityDeleteConfirm", new { controller = "Facility", action = "FacilityDeleteConfirm", CERSID = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.FacilityMergeSource, "Facility/{CERSID}/FacilityMergeSource", new { controller = "Facility", action = "FacilityMergeSource", CERSID = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.FacilityMergeTarget, "Facility/{CERSID}/FacilityMergeTarget", new { controller = "Facility", action = "FacilityMergeTarget", CERSID = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.FacilityMergeConfirm, "Facility/{CERSID}/FacilityMergeConfirm/{targetCERSID}", new { controller = "Facility", action = "FacilityMergeConfirm", CERSID = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.FacilityTransferSource, "Facility/{CERSID}/FacilityTransferSource", new { controller = "Facility", action = "FacilityTransferSource", CERSID = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.FacilityTransferTarget, "Facility/{CERSID}/FacilityTransferTarget", new { controller = "Facility", action = "FacilityTransferTarget", CERSID = UrlParameter.Optional });
			routes.MapRoute(RegulatorManage.FacilityTransferConfirm, "Facility/{CERSID}/FacilityTransferConfirm/{targetOrganizationId}", new { controller = "Facility", action = "FacilityTransferConfirm", CERSID = UrlParameter.Optional });

			#endregion Transfer/Merge/Delete
		}

		#endregion RegisterRegulatorManagementRoutes

		#region RegisterCommonRoutes Method

		public static void RegisterCommonRoutes(RouteCollection routes)
		{
			routes.MapRoute(CommonRoute.Unauthorized, "Unauthorized", new { controller = "Home", action = "UnAuthorized" });
			routes.MapRoute(CommonRoute.EventLinkForwarder, "Event/{ticketCode}", new { controller = "Home", action = "EventLinkForwarder", ticketCode = UrlParameter.Optional });
			routes.MapRoute(CommonRoute.Reports, "Reports", new { controller = "Reports", action = "Index" });
			routes.MapRoute(CommonRoute.RegisterRegulator, "Register", new { controller = "Home", action = "Register" });
			routes.MapRoute(CommonRoute.Tools, "Tools", new { controller = "Tools", action = "Index" });
			routes.MapRoute(CommonRoute.Help, "Help", new { controller = "Help", action = "Index" });
			routes.MapRoute(CommonRoute.FieldHelp, "FieldHelp", new { controller = "Help", action = "FieldHelp", dataRegistryID = UrlParameter.Optional });
			routes.MapRoute(CommonRoute.Switchboard, "Switchboard", new { controller = "Home", action = "Switchboard" });
			routes.MapRoute(CommonRoute.GetStarted, "GetStarted", new { controller = "Home", action = "GetStarted" });
			routes.MapRoute(CommonRoute.UserAgreement, "UserAgreement", new { controller = "Home", action = "UA" });
			routes.MapRoute(CommonRoute.Compliance, "Compliance", new { controller = "Compliance", action = "Index" });
			routes.MapRoute(CommonRoute.Error, "Error", new { controller = "Error", action = "Index" });
			routes.MapRoute(CommonRoute.Error_CERSID_NotFound, "Error/CERSID/{CERSID}/NotFound", new { controller = "Error", action = "CERSIDNotFound", CERSID = UrlParameter.Optional });
			routes.MapRoute(CommonRoute.Error_CERSID_Deleted, "Error/CERSID/{CERSID}/Deleted", new { controller = "Error", action = "CERSIDDeleted", CERSID = UrlParameter.Optional });
			routes.MapRoute(CommonRoute.TestError, "TestError", new { controller = "Error", action = "Test" });
			routes.MapRoute(CommonRoute.Notifications, "Notifications", new { controller = "Home", action = "Notifications" });
			routes.MapRoute(CommonRoute.ActionRequired, "ActionRequired", new { controller = "Home", action = "ActionRequired" });
			routes.MapRoute(CommonRoute.NotFound, "NotFound", new { controller = "Error", action = "NotFound" });
			routes.MapRoute(CommonRoute.OrganizationDeferredProcessingExists, "Error/OrganizationID/{organizationID}/DeferredProcessingExists", new { controller = "Error", action = "OrganizationDeferredProcessingExists", organizationID = UrlParameter.Optional });
			routes.MapRoute(CommonRoute.Accounts, "Account/Search", new { controller = "Account", action = "Search" });

			routes.RegisterGlobalHelpRoutes();
		}

		#endregion RegisterCommonRoutes Method

		#region RegisterPublicRoutes Method

		public static void RegisterPublicRoutes(RouteCollection routes)
		{
			routes.MapRoute(PublicRoute.Index, "Public", new { controller = "Public", action = "Index" });
			routes.MapRoute(PublicRoute.ConditionsOfUse, "Public/ConditionsOfUse", new { controller = "Public", action = "ConditionsOfUse" });
			routes.MapRoute(PublicRoute.Contact, "Public/Contact", new { controller = "Public", action = "Contact" });
			routes.MapRoute(PublicRoute.Privacy, "Public/Privacy", new { controller = "Public", action = "PrivacyPolicy" });
			routes.MapRoute(PublicRoute.BrowserInfo, "Public/BrowserInfo", new { controller = "Public", action = "BrowserInfo" });
			routes.MapRoute(PublicRoute.GuidanceMessageTemplates, "Public/GuidanceMessageTemplates", new { controller = "Public", action = "GuidanceMessageTemplates" });
			routes.MapRoute(PublicRoute.GuidanceMessageTemplateDetail, "Public/GuidanceMessageTemplateDetail/{guidanceMessageTemplateID}", new { controller = "Public", action = "GuidanceMessageTemplateDetail" });
			routes.MapRoute(PublicRoute.EDTEndpointDetail, "Public/EDTEndpoint/{acronym}", new { controller = "Public", action = "EDTEndpointDetail", acronym = UrlParameter.Optional });
			routes.MapRoute(PublicRoute.EDTEndpoints, "Public/EDTEndpoints", new { controller = "Public", action = "EDTEndpoints" });
			routes.MapRoute(PublicRoute.UploadPolicy, "Public/UploadPolicy", new { controller = "Public", action = "UploadPolicy" });
		}

		#endregion RegisterPublicRoutes Method

		#region RegisterToolsRoutes Method

		public static void RegisterToolsRoutes(RouteCollection routes)
		{
			routes.MapRoute("ToolsSearch_GridBindingRegulatorZipCodeSubmittalElementMappingSummaries", "Tools/Search_GridBindingRegulatorZipCodeSubmittalElementMappingSummaries", new { controller = "Tools", action = "Search_GridBindingRegulatorZipCodeSubmittalElementMappingSummaries" });
			routes.MapRoute("EventTypes", "Tools/EventTypes", new { controller = "Tools", action = "EventTypes" });
			routes.MapRoute("EventTypeDetails", "Tools/EventTypeDetails/{id}", new { controller = "Tools", action = "EventTypeDetails" });
			routes.MapRoute("UploadFacilityMetadata", "Tools/UploadFacilityMetadata", new { controller = "Tools", action = "UploadFacilityMetadata" });
		}

		#endregion RegisterToolsRoutes Method

		public static void RegisterAdminRoutes(RouteCollection routes)
		{
			routes.RegisterCommonAdminRoutes();

			routes.MapRoute(AdminRoute.HomeEmailHistory, "Admin/Emails", new { controller = "AdminHome", action = "Emails" });
			routes.MapRoute(AdminRoute.HomeActivityLog, "Admin/ActivityLog", new { controller = "AdminHome", action = "ActivityLog" });

			routes.MapRoute(AdminRoute.SystemPasswordRecovery, "Admin/System/PasswordRecovery", new { controller = "AdminSystem", action = "PasswordRecovery" });
			routes.MapRoute(AdminRoute.SystemTestAuthentication, "Admin/System/TestAuthentication", new { controller = "AdminSystem", action = "TestAuthentication" });
			routes.MapRoute(AdminRoute.SystemCERSSettings, "Admin/System/CERSSettings", new { controller = "AdminSystem", action = "SystemSettings" });
			routes.MapRoute(AdminRoute.SystemCERSSettingDetail, "Admin/System/CERSSetting/{id}", new { controller = "AdminSystem", action = "SystemSettingDetail", id = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.SystemGlobalSettings, "Admin/System/GlobalSettings", new { controller = "AdminSystem", action = "GlobalSettings" });
			routes.MapRoute(AdminRoute.SystemGlobalSettingDetail, "Admin/System/GlobalSetting/{id}", new { controller = "AdminSystem", action = "GlobalSettingDetail", id = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.SystemPortalManagement, "Admin/System/PortalManagement", new { controller = "AdminSystem", action = "PortalManagement" });
			routes.MapRoute(AdminRoute.SystemEDTEndpoints, "Admin/System/EDT/Endpoints", new { controller = "AdminSystem", action = "EDTEndpoints" });
			routes.MapRoute(AdminRoute.SystemEDTAuthRequests, "Admin/System/EDT/AuthRequests", new { controller = "AdminSystem", action = "EDTAuthenticationRequests" });
			routes.MapRoute(AdminRoute.SystemSearchEDTTransactions, "Admin/System/EDT/SearchTransactions", new { controller = "AdminSystem", action = "SearchEDTTransactions" });
			routes.MapRoute(AdminRoute.SystemEDTTransactionDetail, "Admin/System/EDT/TransactionDetail", new { controller = "AdminSystem", action = "EDTTransactionDetail" });
			routes.MapRoute(AdminRoute.SystemEDTTransactionExport, "Admin/System/EDT/Transactions/Export", new { controller = "AdminSystem", action = "EDTTransactionsExport"});
			routes.MapRoute(AdminRoute.SystemEDTAuthRequestsGridRead, "Admin/System/EDT/AuthRequestsGridRead", new { controller = "AdminSystem", action = "EDTAuthenticationRequests_GridRead" });
			routes.MapRoute(AdminRoute.SystemDeferredProcessing, "Admin/System/DeferredProcessing", new { controller = "AdminSystem", action = "DeferredProcessing" });
			routes.MapRoute(AdminRoute.SystemEmailQueueReports, "Admin/System/EmailQueueReports", new { controller = "AdminSystem", action = "EmailQueueReports" });

			routes.MapRoute(AdminRoute.FacilityHome, "Admin/Facility", new { controller = "AdminFacility", action = "Index" });
			routes.MapRoute(AdminRoute.FacilityZipCodeMapping, "Admin/Facility/ZipCodeMappings", new { controller = "AdminFacility", action = "ZipCodeMapping" });
			routes.MapRoute(AdminRoute.FacilitySaveZipCodeInfo, "Admin/Facility/SaveZipCodeInfo", new { controller = "AdminFacility", action = "SaveZipCodeInfo" });
			routes.MapRoute(AdminRoute.FacilityChildRegulators, "Admin/Facility/ChildRegulators/{id}", new { controller = "AdminFacility", action = "ChildRegulators", id = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.FacilityZipCodeInfo, "Admin/Facility/ZipCodeInfo/{id}", new { controller = "AdminFacility", action = "ZipCodeInfo", id = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.FacilityExportRegulatorZipCodeSubmittalElementMappingSummaryListing, "Admin/Facility/ExportRegulatorZipCodeSubmittalElementMappingSummaryListing", new { controller = "AdminFacility", action = "ExportRegulatorZipCodeSubmittalElementMappingSummaryListing" });
			routes.MapRoute(AdminRoute.FacilityGridSearchRegulatorZipCodeSubmittalElementMappingSummaries, "Admin/Facility/GridSearch_RegulatorZipCodeSubmittalElementMappingSummaries", new { controller = "AdminFacility", action = "GridSearch_RegulatorZipCodeSubmittalElementMappingSummaries" });

			routes.MapRoute(AdminRoute.HelpHome, "Admin/Help", new { controller = "AdminHelp", action = "Index" });
			routes.MapRoute(AdminRoute.HelpEdit, "Admin/Edit/{id}", new { controller = "AdminHelp", action = "Edit", id = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.HelpCreate, "Admin/Create", new { controller = "AdminHelp", action = "Create" });

			routes.MapRoute(AdminRoute.DataRegistryHome, "Admin/DataRegistry", new { controller = "AdminDataRegistry", action = "Index" });
			routes.MapRoute(AdminRoute.DataRegistrySearch, "Admin/DataRegistry/Search", new { controller = "AdminDataRegistry", action = "Search" });
			routes.MapRoute(AdminRoute.DataRegistrySearchGridRead, "Admin/DataRegistry/Search/Query", new { controller = "AdminDataRegistry", action = "Search_GridRead" });
			routes.MapRoute(AdminRoute.DataRegistryElementCreate, "Admin/DataRegistry/Element/Create", new { controller = "AdminDataRegistry", action = "ElementCreate" });
			routes.MapRoute(AdminRoute.DataRegistryElementDetail, "Admin/DataRegistry/Element/Detail/{elementID}", new { controller = "AdminDataRegistry", action = "ElementDetail", elementID = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.DataRegistryElementEdit, "Admin/DataRegistry/Element/Edit/{elementID}", new { controller = "AdminDataRegistry", action = "ElementEdit", elementID = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.DataRegistryElementDelete, "Admin/DataRegistry/Element/Delete/{elementID}", new { controller = "AdminDataRegistry", action = "ElementDelete", elementID = UrlParameter.Optional });

			routes.MapRoute(AdminRoute.EventHome, "Admin/Events", new { controller = "AdminEvent", action = "Index" });

			routes.MapRoute(AdminRoute.EventTypes, "Admin/Events/Types", new { controller = "AdminEvent", action = "EventTypes" });
			routes.MapRoute(AdminRoute.EventTypeDetail, "Admin/Events/Types/{id}", new { controller = "AdminEvent", action = "EventTypeDetail", id = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.EventTypeEdit, "Admin/Events/Types/{id}/Edit", new { controller = "AdminEvent", action = "EventTypeEdit", id = UrlParameter.Optional });

			routes.MapRoute(AdminRoute.EventNotificationTemplates, "Admin/Events/NotificationTemplates", new { controller = "AdminEvent", action = "NotificationTemplates" });
			routes.MapRoute(AdminRoute.EventNotificationTemplatesGridRead, "Admin/Events/NotificationTemplates/GridRead", new { controller = "AdminEvent", action = "NotificationTemplates_GridRead" });
			routes.MapRoute(AdminRoute.EventNotificationTemplateCreate, "Admin/Events/NotificationTemplates/Create", new { controller = "AdminEvent", action = "NotificationTemplateCreate" });
			routes.MapRoute(AdminRoute.EventNotificationTemplateDetail, "Admin/Events/NotificationTemplates/{id}", new { controller = "AdminEvent", action = "NotificationTemplateDetail", id = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.EventNotificationTemplateEdit, "Admin/Events/NotificationTemplates/{id}/Edit", new { controller = "AdminEvent", action = "NotificationTemplateEdit", id = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.EventNotificationTemplateClone, "Admin/Events/NotificationTemplates/Clone", new { controller = "AdminEvent", action = "NotificationTemplateClone" });

			routes.MapRoute(AdminRoute.EventEmailTemplates, "Admin/Events/EmailTemplates", new { controller = "AdminEvent", action = "EmailTemplates" });
			routes.MapRoute(AdminRoute.EventEmailTemplateDetail, "Admin/Events/EmailTemplates/{id}", new { controller = "AdminEvent", action = "EmailTemplateDetail", id = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.EventEmailTemplateEdit, "Admin/Events/EmailTemplate/{id}/Edit", new { controller = "AdminEvent", action = "EmailTemplateEdit", id = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.EventEmailTemplateDelete, "Admin/Events/EmailTemplate/{id}/Delete", new { controller = "AdminEvent", action = "EmailTemplateDelete", id = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.EventEmailTemplateCreate, "Admin/Events/EmailTemplate/Create", new { controller = "AdminEvent", action = "EmailTemplateCreate" });

			routes.MapRoute(AdminRoute.SecurityConfigHome, "Admin/SecurityConfig", new { controller = "AdminSecurityConfig", action = "Index" });
			routes.MapRoute(AdminRoute.SecurityConfigPermissionGroupEdit, "Admin/SecurityConfig/PermissionGroup/{id}/Edit", new { controller = "AdminSecurityConfig", action = "PermissionGroupEdit", id = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.SecurityConfigPermissionGroupDelete, "Admin/SecurityConfig/PermissionGroup/{id}/Delete", new { controller = "AdminSecurityConfig", action = "PermissionGroupDelete", id = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.SecurityConfigPermissionGroupCreate, "Admin/SecurityConfig/PermissionGroup/Create", new { controller = "AdminSecurityConfig", action = "PermissionGroupCreate" });
			routes.MapRoute(AdminRoute.SecurityConfigPermissionGroupDetail, "Admin/SecurityConfig/PermissionGroup/{id}", new { controller = "AdminSecurityConfig", action = "PermissionGroupDetail", id = UrlParameter.Optional });

			routes.MapRoute(AdminRoute.ViolationLibraryHome, "Admin/Libraries/Violation", new { controller = "AdminViolationLibrary", action = "Index" });
			routes.MapRoute(AdminRoute.ViolationLibraryViolationTypeSummary, "Admin/Libraries/Violation/Type/{violationTypeID}", new { controller = "AdminViolationLibrary", action = "Summary", violationTypeID = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.ViolationLibraryViolationTypeEdit, "Admin/Libraries/Violation/Type/Edit/{violationTypeID}", new { controller = "AdminViolationLibrary", action = "Edit", violationTypeID = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.ViolationLibraryViolationCitationSummary, "Admin/Libraries/Violation/Type/{violationTypeID}/CitationSummary/{violationCitationID}", new { controller = "AdminViolationLibrary", action = "CitationSummary", violationTypeID = UrlParameter.Optional, violationCitationID = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.ViolationLibraryViolationAddCitation, "Admin/Libraries/Violation/Type/AddCitation/{violationTypeID}", new { controller = "AdminViolationLibrary", action = "AddCitation", violationTypeID = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.ViolationLibraryViolationEditCitation, "Admin/Libraries/Violation/Type/{violationTypeID}/EditCitation/{violationCitationID}", new { controller = "AdminViolationLibrary", action = "EditCitation", violationTypeID = UrlParameter.Optional, violationCitationID = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.ViolationLibraryViolationDeleteCitation, "Admin/Libraries/Violation/Type/{violationTypeID}/DeleteCitation/{violationCitationID}", new { controller = "AdminViolationLibrary", action = "DeleteCitation", violationTypeID = UrlParameter.Optional, violationCitationID = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.ViolationLibraryCategoryList_Async, "Admin/Libraries/Violation/CategoryList_Async/{violationProgramElementID}", new { controller = "AdminViolationLibrary", action = "CategoryList_Async", violationProgramElementID = UrlParameter.Optional });

			routes.MapRoute(AdminRoute.ChemicalLibraryHome, "Admin/Libraries/Chemical", new { controller = "AdminChemicalLibrary", action = "Index" });
			routes.MapRoute(AdminRoute.ChemicalLibrarySummary, "Admin/Libraries/Chemical/{chemicalID}", new { controller = "AdminChemicalLibrary", action = "Summary", chemicalID = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.ChemicalLibraryEdit, "Admin/Libraries/Chemical/Edit/{chemicalID}", new { controller = "AdminChemicalLibrary", action = "Edit", chemicalID = UrlParameter.Optional });

			routes.MapRoute(AdminRoute.EnhancementHome, "Admin/Enhancements", new { controller = "AdminEnhancement", action = "Index" });
			routes.MapRoute(AdminRoute.EnhancementAdd, "Admin/Enhancements/Add", new { controller = "AdminEnhancement", action = "Add" });
			routes.MapRoute(AdminRoute.EnhancementEdit, "Admin/Enhancements/Edit/{enhancementID}", new { controller = "AdminEnhancement", action = "Edit", enhancementID = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.EnhancementDetail, "Admin/Enhancements/Detail/{enhancementID}", new { controller = "AdminEnhancement", action = "Detail", enhancementID = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.EnhancementCommentAdd, "Admin/Enhancements/Detail/{enhancementID}/AddComment", new { controller = "AdminEnhancement", action = "CommentAdd", enhancementID = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.EnhancementCommentEdit, "Admin/Enhancements/Detail/{enhancementID}/EditComment/{commentID}", new { controller = "AdminEnhancement", action = "CommentEdit", commentID = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.EnhancementCommentDetail, "Admin/Enhancements/Detail/{enhancementID}/DetailComment/{commentID}", new { controller = "AdminEnhancement", action = "CommentDetail", commentID = UrlParameter.Optional });
			routes.MapRoute(AdminRoute.EnhancementCommentDelete, "Admin/Enhancements/Detail/{enhancementID}/DeleteComment/{commentID}", new { controller = "AdminEnhancement", action = "CommentDelete", commentID = UrlParameter.Optional });
		}

		#region RegisterRoutes Method

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.MapRoute("TestNotifications", "Test", new { controller = "Test", action = "Index" });
			routes.MapRoute("TestNotifications_Async", "Test/Notifications_Async", new { controller = "Test", action = "Notifications_Async" });
			routes.MapRoute("ChangeCUPA_PotentialMappings_Async", "Facility/ChangeCUPA_PotentialMappings_Async", new { controller = "Facility", action = "ChangeCUPA_PotentialMappings_Async" });
			RegisterCommonRoutes(routes);
			RegisterAccountRoutes(routes);
			RegisterPublicRoutes(routes);
			RegisterToolsRoutes(routes);
			RegisterOrganizationManagementRoutes(routes);
			routes.RegisterServiceRoutes();
			RegisterAdminRoutes(routes);

			routes.MapRoute("EditPopupForSubmittalElement", "SubmittalElementEdit", new { controller = "Submittal", action = "EditPopupForSubmittalElement", CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional });
			routes.MapRoute("EditPopupForSubmittalElementForProcessing", "EditPopupForSubmittalElementForProcessing", new { controller = "Submittal", action = "EditPopupForSubmittalElementForProcessing", CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional });

			RegisterGuidanceMessageRoutes(routes);
			RegisterRegulatorManagementRoutes(routes);
			RegisterRegulatorDocRoutes(routes);

			// Set up Routes for each Tab:
			RegisterSubmittalRoutes(routes);
			RegisterFacilitySubmittalElementRoutes(routes);
			RegisterFacilityRoutes(routes);
			RegisterInspectionRoutes(routes);
			RegisterViolationRoutes(routes);
			RegisterEnforcementRoutes(routes);
			RegisterComplianceRoutes(routes);
			RegisterResponderRoutes(routes);
			RegisterReportRoutes(routes);

			routes.MapRoute(CommonRoute.RegulatorHome, "", new { controller = "Home", action = "Index" });

			routes.MapRoute(RegulatorManage.Summary,
				"{regulatorId}",
				new { controller = "Regulator", action = "Summary", regulatorId = UrlParameter.Optional }
				);

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);
		}

		#endregion RegisterRoutes Method

		#region RegisterGuidanceMessageRoutes

		private static void RegisterGuidanceMessageRoutes(RouteCollection routes)
		{
			routes.MapRoute(GuidanceMessageRoute.FacilitySubmittalElement,
				"GuidanceMessageFacilitySubmittalElement",
				new { controller = "Facility", action = "GuidanceMessageForSubmittalElement", fseid = UrlParameter.Optional }
				);

			routes.MapRoute(GuidanceMessageRoute.FacilitySubmittalElementGrid,
				"GuidanceMessageFacilitySubmittalElementGrid/{FSEID}",
				new { controller = "Facility", action = "GetGuidanceMessagesBySubmittalElement", FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(GuidanceMessageRoute.FacilitySubmittalElementResource,
				"GuidanceMessageFacilitySubmittalElementResource",
				new { controller = "Facility", action = "GuidanceMessageForSubmittalElementResource", FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(GuidanceMessageRoute.FacilitySubmittalElementResourceGrid,
				"GuidanceMessageFacilitySubmittalElementResourceGrid/{FSERID}",
				new { controller = "Facility", action = "GetGuidanceMessagesBySubmittalElementResource", FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(GuidanceMessageRoute.FacilitySubmittalElementResourceEntity,
				"GuidanceMessageFacilitySubmittalElementResourceEntity",
				new { controller = "Facility", action = "GuidanceMessageForSubmittalElementResourceEntity", FSERID = UrlParameter.Optional, entityid = UrlParameter.Optional }
				);
		}

		#endregion RegisterGuidanceMessageRoutes

		#region RegisterSubmittalRoutes

		public static void RegisterSubmittalRoutes(RouteCollection routes)
		{
			routes.MapRoute("SubmittalIndex", "Submittal", new { controller = "Submittal", action = "Index" });
			routes.MapRoute("SubmittalProcessing", "SubmittalProcessing", new { controller = "Submittal", action = "SubmittalProcessing" });
			routes.MapRoute("SubmittalSearchGrid", "Submittal/Search_GridBindingSubmittals", new { controller = "Submittal", action = "Search_GridBindingSubmittals" });
			routes.MapRoute("SubmittalHistory_GridAction", "Submittal/SubmittalHistory_GridAction", new { controller = "Submittal", action = "SubmittalHistory_GridAction" });
			routes.MapRoute("SubmittalEdit", "Submittal/{CERSID}/Edit/{FSEID}", new { controller = "Submittal", action = "Edit", CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional });
			routes.MapRoute("HideFacilitySubmittalElement", "HideFacilitySubmittalElement/{FSEID}", new { controller = "Submittal", action = "HideFacilitySubmittalElement", FSEID = UrlParameter.Optional });
			routes.MapRoute("UnHideFacilitySubmittalElement", "UnHideFacilitySubmittalElement/{FSEID}", new { controller = "Submittal", action = "UnHideFacilitySubmittalElement", FSEID = UrlParameter.Optional });
			routes.MapRoute("FacilitySubmittalUpdate_Async", "FacilitySubmittalUpdate_Async", new { controller = "Submittal", action = "FacilitySubmittalUpdate_Async" });
			routes.MapRoute("ArchivedFacilitySubmittalTransfer", "Submittal/ArchivedFacilitySubmittalTransfer", new { controller = "Submittal", action = "ArchivedFacilitySubmittalTransfer", FSID = UrlParameter.Optional, currentOrganizationID = UrlParameter.Optional });

            routes.MapRoute("CheckIfFseChronological", "CheckIfFseChronological/{FSEID}", new { controller = "Submittal", action = "CheckIfFseChronological", FSEID = UrlParameter.Optional });

			routes.MapRoute( "ExportToExcelSubmittalListing", "Submittal/ExportToExcelSubmittalListing", new { controller = "Submittal", action = "ExportToExcelSubmittalListing" } );

			routes.MapRoute(OrganizationFacility.SubmittalEvent, "Submittal/Details/{FSID}", new { controller = "Submittal", action = "OrganizationFacilitySubmittalEvent", FSID = UrlParameter.Optional });
			routes.MapRoute(OrganizationFacility.SubmittalUpdate, "Submittal/Update", new { controller = "Submittal", action = "OrganizationFacilitySubmittalUpdate" });
		}

		#endregion RegisterSubmittalRoutes

		#region RegisterFacilityRoutes

		public static void RegisterFacilityRoutes(RouteCollection routes)
		{
			routes.MapRoute(OrganizationFacility.Search, "Facility", new { controller = "Facility", action = "Index" });
			routes.MapRoute(OrganizationFacility.Search_GridRead, "Facility/Search_GridRead", new { controller = "Facility", action = "SearchFacilities_GridRead" });
			routes.MapRoute("FacilitySearch_GridBindingFacilities", "Facility/Search_GridBindingFacilities", new { controller = "Facility", action = "Search_GridBindingFacilities" });
			routes.MapRoute(OrganizationFacility.Summary, "Facility/{CERSID}", new { controller = "Facility", action = "Summary", CERSID = UrlParameter.Optional });
			routes.MapRoute(OrganizationFacility.Submittals, "Facility/{CERSID}/Submittals", new { controller = "Facility", action = "Submittals", CERSID = UrlParameter.Optional });
			routes.MapRoute(OrganizationFacility.ReportingRequirements, "Facility/{CERSID}/ReportingRequirements", new { controller = "Facility", action = "ReportingRequirements", CERSID = UrlParameter.Optional });
			routes.MapRoute(OrganizationFacility.ReportingRequirementsSave, "Facility/{CERSID}/ReportingRequirementsSave_Async", new { controller = "Facility", action = "ReportingRequirementsSave_Async", CERSID = UrlParameter.Optional });
			routes.MapRoute(OrganizationFacility.ReportingRequirements_GridBinding, "Facility/{CERSID}/ReportingRequirements_GridBinding", new { controller = "Facility", action = "ReportingRequirements_GridBinding", CERSID = UrlParameter.Optional });
			routes.MapRoute(OrganizationFacility.Compliance, "Facility/{CERSID}/Compliance", new { controller = "Facility", action = "Compliance", CERSID = UrlParameter.Optional });
			routes.MapRoute(OrganizationFacility.FacilityMap, "Facility/{CERSID}/Map", new { controller = "Facility", action = "Map", CERSID = UrlParameter.Optional });
			routes.MapRoute(OrganizationFacility.Notifications, "Facility/{CERSID}/Notifications", new { controller = "Facility", action = "Notifications", CERSID = UrlParameter.Optional });
			routes.MapRoute(OrganizationFacility.ManageFacility, "Facility/{CERSID}/Manage", new { controller = "Facility", action = "Manage", CERSID = UrlParameter.Optional });
			routes.MapRoute(OrganizationFacility.Inspections, "Facility/{CERSID}/Inspections", new { controller = "Facility", action = "Inspections", CERSID = UrlParameter.Optional });
			routes.MapRoute(OrganizationFacility.Enforcements, "Facility/{CERSID}/Enforcements", new { controller = "Facility", action = "Enforcements", CERSID = UrlParameter.Optional });
			routes.MapRoute(OrganizationFacility.Violations, "Facility/{CERSID}/Violations", new { controller = "Facility", action = "Violations", CERSID = UrlParameter.Optional });
			routes.MapRoute(OrganizationFacility.ChangeCUPA, "Facility/{CERSID}/ChangeCUPA", new { controller = "Facility", action = "ChangeCUPA", CERSID = UrlParameter.Optional });
			routes.MapRoute(EventProcessingRoute.RegulatorPortal_ProcessDeleteRequest, "Facility/{CERSID}/DeleteRequest/{eventID}/Process", new { controller = "Facility", action = "ProcessFacilityDeleteRequest", CERSID = UrlParameter.Optional, eventID = UrlParameter.Optional });
			routes.MapRoute(EventProcessingRoute.RegulatorPortal_ProcessMergeRequest, "Facility/{CERSID}/MergeRequest/{eventID}/Process", new { controller = "Facility", action = "ProcessFacilityMergeRequest", CERSID = UrlParameter.Optional, eventID = UrlParameter.Optional });
			routes.MapRoute(EventProcessingRoute.RegulatorPortal_ProcessTransferRequest, "Facility/{CERSID}/TransferRequest/{eventID}/Process", new { controller = "Facility", action = "ProcessFacilityTransferRequest", CERSID = UrlParameter.Optional, eventID = UrlParameter.Optional });

			routes.MapRoute("ChangeCUPA_Commit_Async", "ChangeCUPA_Commit_Async", new { controller = "Facility", action = "ChangeCUPA_Commit_Async" });
			routes.MapRoute("EditFacilityID_Async", "EditFacilityID_Async", new { controller = "Facility", action = "EditFacilityID_Async" });
			routes.MapRoute("EditFacilityRegulatorKey_Async", "EditFacilityRegulatorKey_Async", new { controller = "Facility", action = "EditFacilityRegulatorKey_Async" });
			routes.MapRoute("UpdateRemoteSQGFacility_Async", "UpdateRemoteSQGFacility_Async", new { controller = "Facility", action = "UpdateRemoteSQGFacility_Async" });

			routes.MapRoute(OrganizationFacility.UpdateLocationMap_Async, "Facility/{CERSID}/UpdateLocationMap_Async", new { controller = "Facility", action = "UpdateLocationMap_Async", CERSID = UrlParameter.Optional });
			routes.MapRoute(OrganizationFacility.RetrieveScheduledPrintJobDocument, "{organizationId}/RetrieveScheduledPrintJobDocument/{printJobID}", new { controller = "Facility", action = "RetrieveScheduledPrintJobDocument", printJobID = UrlParameter.Optional });

			routes.MapRoute("FacilitySubmittalSummary", "Facility/{CERSID}/Submittal/Summary", new { controller = "Facility", action = "Summary", CERSID = UrlParameter.Optional });
			routes.MapRoute("FacilitySubmittalHistory", "Facility/{CERSID}/Submittal/History", new { controller = "Facility", action = "History", CERSID = UrlParameter.Optional });
			routes.MapRoute("FacilitySubmittal", "Facility/{CERSID}/Submittal/{SDate}", new { controller = "Facility", action = "Submittal", CERSID = UrlParameter.Optional, SDate = UrlParameter.Optional });
			routes.MapRoute("FacilityInspectionList", "Facility/{CERSID}/Inspection", new { controller = "Facility", action = "InspectionList", CERSID = UrlParameter.Optional });
			routes.MapRoute("FacilityEnforcementList", "Facility/{CERSID}/Enforcement", new { controller = "Facility", action = "EnforcementList", CERSID = UrlParameter.Optional });
			routes.MapRoute("GetFacilities", "Facility/GetFacilities", new { controller = "Facility", action = "GetFacilities" });

			routes.MapRoute(OrganizationFacility.SubmittalHistoryGrid_Async, "Facility/{CERSID}/SubmittalHistory_GridAction", new { controller = "Facility", action = "SubmittalHistory_GridAction", CERSID = UrlParameter.Optional });
			routes.MapRoute(OrganizationFacility.SubmittalHistoryGridArchived_Async, "Facility/{CERSID}/ArchivedSubmittalHistory_GridAction", new { controller = "Facility", action = "ArchivedSubmittalHistory_GridAction", CERSID = UrlParameter.Optional });
		}

		#endregion RegisterFacilityRoutes

		#region RegisterFacilitySubmittalElementRoutes

		public static void RegisterFacilitySubmittalElementRoutes(RouteCollection routes)
		{
			#region FacilityInfo

			routes.MapRoute(
				GetRouteName(SubmittalElementType.FacilityInformation, ResourceType.BusinessActivities, Part.Detail, Part.Print),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/FI/{FSEID}/BizActivity/Detail/{FSERID}/Print",
				new { controller = "FacilitySubmittalElement", action = "Detail_BusinessActivities_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.FacilityInformation, ResourceType.BusinessActivities, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/FI/{FSEID}/BizActivity/Detail/{FSERID}",
				new { controller = "FacilitySubmittalElement", action = "Detail_BusinessActivities", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.FacilityInformation, ResourceType.BusinessOwnerOperatorIdentification, Part.Detail, Part.Print),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/FI/{FSEID}/OwnerOperator/Detail/{FSERID}/Print",
				new { controller = "FacilitySubmittalElement", action = "Detail_BusinessOwnerOperatorIdentification_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.FacilityInformation, ResourceType.BusinessOwnerOperatorIdentification, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/FI/{FSEID}/OwnerOperator/Detail/{FSERID}",
				new { controller = "FacilitySubmittalElement", action = "Detail_BusinessOwnerOperatorIdentification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
				 GetRouteName(SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationLocallyRequired_Document, Part.Detail),
			 "Business/{organizationId}/Facility/{CERSID}/Submittal/FI/{FSEID}/LRDoc/Detail/{FSERID}",
			 new { controller = "FacilitySubmittalElement", action = "Detail_FacilityInformationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			 );

			routes.MapRoute(
				GetRouteName(SubmittalElementType.FacilityInformation, ResourceType.FacilityInformationMiscellaneousStateRequired_Document, Part.Detail),
			"Business/{organizationId}/Facility/{CERSID}/Submittal/FI/{FSEID}/MSRDoc/Detail/{FSERID}",
			new { controller = "FacilitySubmittalElement", action = "Detail_FacilityInformationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion FacilityInfo

			#region Haz Inv

			routes.MapRoute(
			   GetRouteName(SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Home),
			   "Business/{organizationId}/Facility/{CERSID}/Submittal/HMI/{FSEID}/HazMatInventory/{FSERID}",
			   new { controller = "FacilitySubmittalElement", action = "Home_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName(SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Download),
			"Business/{organizationId}/Facility/{CERSID}/Submittal/HMI/{FSEID}/HazMatInventory/{FSERID}/Download",
			new { controller = "FacilitySubmittalElement", action = "Download_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute("Report_HazardousMaterialInventoryMatrix_Async", "HazardousMaterialsInventory/Report_HazardousMaterialInventoryMatrix_Async", new { controller = "FacilitySubmittalElement", action = "Report_HazardousMaterialInventoryMatrix_Async" });
			routes.MapRoute("Report_HazardousMaterialInventoryMatrix_Click", "HazardousMaterialsInventory/Report_HazardousMaterialInventoryMatrix_Click", new { controller = "FacilitySubmittalElement", action = "Report_HazardousMaterialInventoryMatrix_Click" });

			routes.MapRoute(
				GetRouteName(SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.DraftSubmittal, Part.Home, Part.Detail),
			"Business/{organizationId}/Facility/{CERSID}/Submittal/HMI/{FSEID}/HazMatInventory/Detail/{FSERID}",
			new { controller = "FacilitySubmittalElement", action = "Home_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.Detail, Part.Print),
			"Business/{organizationId}/Facility/{CERSID}/Submittal/HMI/{FSEID}/HazMatInventory/{FSERID}/Detail/{BPFCID}/Print",
			new { controller = "FacilitySubmittalElement", action = "Detail_HazardousMaterialInventory_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional, BPFCID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventory, Part.Detail),
			"Business/{organizationId}/Facility/{CERSID}/Submittal/HMI/{FSEID}/HazMatInventory/{FSERID}/Detail/{BPFCID}",
			new { controller = "FacilitySubmittalElement", action = "Detail_HazardousMaterialInventory", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional, BPFCID = UrlParameter.Optional }
			);

			routes.MapRoute(
			   GetRouteName(SubmittalElementType.HazardousMaterialsInventory, ResourceType.AnnotatedSiteMapOfficialUseOnly_Document, Part.Detail),
			"Business/{organizationId}/Facility/{CERSID}/Submittal/HMI/{FSEID}/SiteMap/Detail/{FSERID}",
			new { controller = "FacilitySubmittalElement", action = "Detail_AnnotatedSiteMapOfficialUseOnly_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
			   GetRouteName(SubmittalElementType.HazardousMaterialsInventory, ResourceType.HazardousMaterialInventoryLocallyRequired_Document, Part.Detail),
			"Business/{organizationId}/Facility/{CERSID}/Submittal/HMI/{FSEID}/HMILRDoc/Detail/{FSERID}",
			new { controller = "FacilitySubmittalElement", action = "Detail_HazardousMaterialInventoryLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.HazardousMaterialsInventory, ResourceType.HMIMiscellaneousStateRequired_Document, Part.Detail),
			"Business/{organizationId}/Facility/{CERSID}/Submittal/HMI/{FSEID}/MSRDoc/Detail/{FSERID}",
			new { controller = "FacilitySubmittalElement", action = "Detail_HazardousMaterialInventoryMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			#endregion Haz Inv

			#region ERP

			routes.MapRoute(
			 GetRouteName(SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseContingencyPlan_Document, Part.Detail),
			 "Business/{organizationId}/Facility/{CERSID}/Submittal/ERTP/{FSEID}/ERCPDoc/Detail",
			 new { controller = "FacilitySubmittalElement", action = "Detail_EmergencyResponseContingencyPlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			 );

			routes.MapRoute(
			GetRouteName(SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmployeeTrainingPlan_Document, Part.Detail),
			"Business/{organizationId}/Facility/{CERSID}/Submittal/ERTP/{FSEID}/ETPDoc/Detail",
			new { controller = "FacilitySubmittalElement", action = "Detail_EmployeeTrainingPlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			);

			routes.MapRoute(
			GetRouteName(SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.EmergencyResponseAndTrainingPlansLocallyRequired_Document, Part.Detail),
			"Business/{organizationId}/Facility/{CERSID}/Submittal/ERTP/{FSEID}/ERTPLRDoc/Detail",
			new { controller = "FacilitySubmittalElement", action = "Detail_EmergencyResponseAndTrainingPlansLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			);

			routes.MapRoute(
			   GetRouteName(SubmittalElementType.EmergencyResponseandTrainingPlans, ResourceType.ERTPMiscellaneousStateRequired_Document, Part.Detail),
			   "Business/{organizationId}/Facility/{CERSID}/Submittal/ERTP/{FSEID}/MSRDoc/Detail",
			   new { controller = "FacilitySubmittalElement", action = "Detail_EmergencyResponseAndTrainingPlansMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			#endregion ERP

			#region UST

			routes.MapRoute(
			  GetRouteName(SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationFacilityInformation, Part.Detail, Part.Print),
			  "Business/{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/FacilityInformation/Detail/{FSERID}/Print",
			  new { controller = "FacilitySubmittalElement", action = "Detail_USTOperatingPermitApplicationFacilityInformation_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			  GetRouteName(SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationFacilityInformation, Part.Detail),
			  "Business/{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/FacilityInformation/Detail/{FSERID}",
			  new { controller = "FacilitySubmittalElement", action = "Detail_USTOperatingPermitApplicationFacilityInformation", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
				GetRouteName(SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationTankInformation, Part.Detail, Part.Print),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/TankInformation/Detail/{FSERID}/Print",
				new { controller = "FacilitySubmittalElement", action = "Detail_USTOperatingPermitApplicationTankInformation_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOperatingPermitApplicationTankInformation, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/TankInformation/Detail/{FSERID}",
				new { controller = "FacilitySubmittalElement", action = "Detail_USTOperatingPermitApplicationTankInformation", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName(SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringPlan, Part.Detail, Part.Print),
			   "Business/{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/MonitoringPlan/Detail/{FSERID}/Print",
			   new { controller = "FacilitySubmittalElement", action = "Detail_USTMonitoringPlan_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName(SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringPlan, Part.Detail),
			   "Business/{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/MonitoringPlan/Detail/{FSERID}",
			   new { controller = "FacilitySubmittalElement", action = "Detail_USTMonitoringPlan", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName(SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofInstallationModification, Part.Detail, Part.Print),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/CertInstMod/Detail/{FSERID}/Print",
				new { controller = "FacilitySubmittalElement", action = "Detail_USTCertificationofInstallationModification_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofInstallationModification, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/CertInstMod/Detail/{FSERID}",
				new { controller = "FacilitySubmittalElement", action = "Detail_USTCertificationofInstallationModification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
				);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMonitoringSitePlan_Document, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/MonitoringSitePlan/Detail",
				new { controller = "FacilitySubmittalElement", action = "Detail_USTMonitoringSitePlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.UndergroundStorageTanks, ResourceType.USTCertificationofFinancialResponsibility_Document, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/CertificationFinancialResponsibility/Detail",
				new { controller = "FacilitySubmittalElement", action = "Detail_USTCertificationofFinancialResponsibility_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
				  GetRouteName(SubmittalElementType.UndergroundStorageTanks, ResourceType.USTResponsePlan_Document, Part.Detail),
				  "Business/{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/ResponsePlan/Detail",
				  new { controller = "FacilitySubmittalElement", action = "Detail_USTResponsePlan_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				  );

			routes.MapRoute(
				GetRouteName(SubmittalElementType.UndergroundStorageTanks, ResourceType.USTOwnerandUSTOperatorWrittenAgreement_Document, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/WrittenAgreement/Detail",
				new { controller = "FacilitySubmittalElement", action = "Detail_USTOwnerandUSTOperatorWrittenAgreement_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
				  GetRouteName(SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLetterfromtheChiefFinancialOfficer_Document, Part.Detail),
				  "Business/{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/LetterCFO/Detail",
				  new { controller = "FacilitySubmittalElement", action = "Detail_USTLetterfromtheChiefFinancialOfficer_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				  );

			routes.MapRoute(
			  GetRouteName(SubmittalElementType.UndergroundStorageTanks, ResourceType.OwnerStatementofDesignatedUSTOperatorCompliance_Document, Part.Detail),
			  "Business/{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/OperatorCompliance/Detail",
			  new { controller = "FacilitySubmittalElement", action = "Detail_OwnerStatementofDesignatedUSTOperatorCompliance_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			  );

			routes.MapRoute(
				GetRouteName(SubmittalElementType.UndergroundStorageTanks, ResourceType.USTLocallyRequired_Document, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/LRDoc/Detail",
				new { controller = "FacilitySubmittalElement", action = "Detail_USTLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName(SubmittalElementType.UndergroundStorageTanks, ResourceType.USTMiscellaneousStateRequired_Document, Part.Detail),
			   "Business/{organizationId}/Facility/{CERSID}/Submittal/UST/{FSEID}/MSRDoc/Detail",
			   new { controller = "FacilitySubmittalElement", action = "Detail_USTMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			#endregion UST

			#region Onsite Hazardous Waste Treatment/Tiered Permitting

			routes.MapRoute(
			  GetRouteName(SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationFacility, Part.Detail, Part.Print),
			  "Business/{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/Facility/Detail/{FSERID}/Print",
			  new { controller = "FacilitySubmittalElement", action = "Detail_OnsiteHazardousWasteTreatmentNotificationFacility_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			  GetRouteName(SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationFacility, Part.Detail),
			  "Business/{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/Facility/Detail/{FSERID}",
			  new { controller = "FacilitySubmittalElement", action = "Detail_OnsiteHazardousWasteTreatmentNotificationFacility", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			  GetRouteName(SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationUnit, Part.Detail, Part.Print),
			  "Business/{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/Unit/Detail/{FSERID}/Print",
			  new { controller = "FacilitySubmittalElement", action = "Detail_OnsiteHazardousWasteTreatmentNotificationUnit_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			  GetRouteName(SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationUnit, Part.Detail),
			  "Business/{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/Unit/Detail/{FSERID}",
			  new { controller = "FacilitySubmittalElement", action = "Detail_OnsiteHazardousWasteTreatmentNotificationUnit", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			  GetRouteName(SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitCertificationofFinancialAssurance, Part.Detail, Part.Print),
			  "Business/{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/CertFinancialAssurance/Detail/{FSERID}/Print",
			  new { controller = "FacilitySubmittalElement", action = "Detail_TieredPermittingUnitCertificationofFinancialAssurance_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
			  GetRouteName(SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitCertificationofFinancialAssurance, Part.Detail),
			  "Business/{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/CertFinancialAssurance/Detail/{FSERID}",
			  new { controller = "FacilitySubmittalElement", action = "Detail_TieredPermittingUnitCertificationofFinancialAssurance", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			  );

			routes.MapRoute(
				GetRouteName(SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentPlotPlanMap_Document, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/PlotPlanMap/Detail",
				new { controller = "FacilitySubmittalElement", action = "Detail_OnsiteHazardousWasteTreatmentPlotPlanMap_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitPriorEnforcementHistory_Document, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/PriorEnforcement/Detail",
				new { controller = "FacilitySubmittalElement", action = "Detail_TieredPermittingUnitPriorEnforcementHistory_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitTankandContainerCertification_Document, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/TankContainerCert/Detail",
				new { controller = "FacilitySubmittalElement", action = "Detail_TieredPermittingUnitTankandContainerCertification_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/NotificationLocAgency/Detail",
				new { controller = "FacilitySubmittalElement", action = "Detail_TieredPermittingUnitNotificationofLocalAgencyorAgencies_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingUnitNotificationofPropertyOwner_Document, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/PropertyOwner/Detail",
				new { controller = "FacilitySubmittalElement", action = "Detail_TieredPermittingUnitNotificationofPropertyOwner_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/WrittenEstimate/Detail",
				new { controller = "FacilitySubmittalElement", action = "Detail_OnsiteHazardousWasteTreatmentWrittenEstimateofClosureCosts_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/ClosureMechanism/Detail",
				new { controller = "FacilitySubmittalElement", action = "Detail_OnsiteHazardousWasteTreatmentFinancialAssuranceClosureMechanism_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/LRDoc/Detail",
				new { controller = "FacilitySubmittalElement", action = "Detail_OnsiteHazardousWasteTreatmentNotificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
			   GetRouteName(SubmittalElementType.OnsiteHazardousWasteTreatmentNotification, ResourceType.TieredPermittingMiscellaneousStateRequired_Document, Part.Detail),
			   "Business/{organizationId}/Facility/{CERSID}/Submittal/OHWTN/{FSEID}/MSRDoc/Detail",
			   new { controller = "FacilitySubmittalElement", action = "Detail_OnsiteHazardousWasteTreatmentNotificationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			#endregion Onsite Hazardous Waste Treatment/Tiered Permitting

			#region Recyclable Materials Report

			routes.MapRoute(
				GetRouteName(SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportLocallyRequired_Document, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/RMR/{FSEID}/LRDoc/Detail",
				new { controller = "FacilitySubmittalElement", action = "Detail_RecyclableMaterialsReportLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName(SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportDocumentation_Document, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/RMR/{FSEID}/RMRDoc/Detail",
				new { controller = "FacilitySubmittalElement", action = "Detail_RecyclableMaterialsReportDocumentation_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName(SubmittalElementType.RecyclableMaterialsReport, ResourceType.RecyclableMaterialsReportMiscellaneousStateRequired_Document, Part.Detail),
			   "Business/{organizationId}/Facility/{CERSID}/Submittal/RMR/{FSEID}/MSRDoc/Detail",
			   new { controller = "FacilitySubmittalElement", action = "Detail_RecyclableMaterialsReportMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			#endregion Recyclable Materials Report

			#region Remote Waste Consolidation Site Annual Notification

			routes.MapRoute(
				GetRouteName(SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationSiteAnnualNotification, Part.Detail, Part.Print),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/RWCAN/{FSEID}/Notification/Detail/{FSERID}/Print",
				new { controller = "FacilitySubmittalElement", action = "Detail_RemoteWasteConsolidationSiteAnnualNotification_Print", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
				GetRouteName(SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationSiteAnnualNotification, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/RWCAN/{FSEID}/Notification/Detail/{FSERID}",
				new { controller = "FacilitySubmittalElement", action = "Detail_RemoteWasteConsolidationSiteAnnualNotification", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional, FSERID = UrlParameter.Optional }
			);

			routes.MapRoute(
			   GetRouteName(SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document, Part.Detail),
			   "Business/{organizationId}/Facility/{CERSID}/Submittal/RWCAN/{FSEID}/LRDoc/Detail",
			   new { controller = "FacilitySubmittalElement", action = "Detail_RemoteWasteConsolidationAnnualNotificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			);

			routes.MapRoute(
			   GetRouteName(SubmittalElementType.RemoteWasteConsolidationSiteAnnualNotification, ResourceType.RemoteWasteConsolidationMiscellaneousStateRequired_Document, Part.Detail),
			   "Business/{organizationId}/Facility/{CERSID}/Submittal/RWCAN/{FSEID}/MSRDoc/Detail",
			   new { controller = "FacilitySubmittalElement", action = "Detail_RemoteWasteConsolidationAnnualNotificationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			#endregion Remote Waste Consolidation Site Annual Notification

			#region Hazardous Waste Tank Closure

			routes.MapRoute(
			   GetRouteName(SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificate_Document, Part.Detail),
			   "Business/{organizationId}/Facility/{CERSID}/Submittal/HWTCC/{FSEID}/Cert/Detail",
			   new { controller = "FacilitySubmittalElement", action = "Detail_HazardousWasteTankClosureCertificate_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
				GetRouteName(SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationLocallyRequired_Document, Part.Detail),
				"Business/{organizationId}/Facility/{CERSID}/Submittal/HWTCC/{FSEID}/LRDoc/Detail",
				new { controller = "FacilitySubmittalElement", action = "Detail_HazardousWasteTankClosureCertificationLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
				);

			routes.MapRoute(
					GetRouteName(SubmittalElementType.HazardousWasteTankClosureCertification, ResourceType.HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document, Part.Detail),
					"Business/{organizationId}/Facility/{CERSID}/Submittal/HWTCC/{FSEID}/MSRDoc/Detail",
					new { controller = "FacilitySubmittalElement", action = "Detail_HazardousWasteTankClosureCertificationMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
					);

			#endregion Hazardous Waste Tank Closure

			#region Aboveground Petroleum Sorage Tanks

			routes.MapRoute(
			   GetRouteName(SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageTanksLocallyRequired_Document, Part.Detail),
			   "Business/{organizationId}/Facility/{CERSID}/Submittal/APST/{FSEID}/LRDoc/Detail",
			   new { controller = "FacilitySubmittalElement", action = "Detail_AbovegroundPetroleumStorageTanksLocallyRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName(SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.AbovegroundPetroleumStorageActDocumentation_Document, Part.Detail),
			   "Business/{organizationId}/Facility/{CERSID}/Submittal/APST/{FSEID}/APSADoc/Detail",
			   new { controller = "FacilitySubmittalElement", action = "Detail_AbovegroundPetroleumStorageActDocumentation_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			routes.MapRoute(
			   GetRouteName(SubmittalElementType.AbovegroundPetroleumStorageTanks, ResourceType.APSAMiscellaneousStateRequired_Document, Part.Detail),
			   "Business/{organizationId}/Facility/{CERSID}/Submittal/APST/{FSEID}/MSRDoc/Detail",
			   new { controller = "FacilitySubmittalElement", action = "Detail_AbovegroundPetroleumStorageTanksMiscellaneousStateRequired_Document", organizationId = UrlParameter.Optional, CERSID = UrlParameter.Optional, FSEID = UrlParameter.Optional }
			   );

			#endregion Aboveground Petroleum Sorage Tanks
		}

		#endregion RegisterFacilitySubmittalElementRoutes

		#region RegisterInspectionRoutes

		public static void RegisterInspectionRoutes(RouteCollection routes)
		{
			routes.MapRoute("InspectionIndex",
				"Inspection",
				new { controller = "Inspection", action = "Index" }
				);

			routes.MapRoute("InspectionExportToExcelInspectionListing", "Inspection/ExportToExcelInspectionListing", new { controller = "Inspection", action = "ExportToExcelInspectionListing" });

			routes.MapRoute(InspectionManage.Index, "Inspection", new { controller = "Inspection", action = "Index" });

			routes.MapRoute("InspectionIndex_AjaxBinding",
				"Inspection/_AjaxBinding",
				new { controller = "Inspection", action = "_AjaxBinding" }
				);

			routes.MapRoute(InspectionManage.Index_AjaxBinding, "Inspection/_AjaxBinding", new { controller = "Inspection", action = "_AjaxBinding" });

			routes.MapRoute("InspectionAdd",
				"Inspection/Add",
				new { controller = "Inspection", action = "Add" }
				);

			routes.MapRoute("FacilityInspectionIndex",
				"Facility/{CERSID}/Inspection",
				new { controller = "Inspection", action = "Index", CERSID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityInspectionCreate",
				"Facility/{CERSID}/Inspection/Create",
				new { controller = "Inspection", action = "Create", CERSID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityInspectionCMEBatchViolation",
				"Facility/{CERSID}/Inspection/CMEBatch/{cmeBatchID}/Violation",
				new { controller = "Inspection", action = "CMEBatchViolation", CERSID = UrlParameter.Optional, cmeBatchID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityInspectionSummary",
				"Facility/{CERSID}/Inspection/{inspectionID}",
				new { controller = "Inspection", action = "Summary", CERSID = UrlParameter.Optional, inspectionID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityInspectionEdit",
				"Facility/{CERSID}/Inspection/{inspectionID}/Edit",
				new { controller = "Inspection", action = "Edit", CERSID = UrlParameter.Optional, inspectionID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityInspectionDelete",
				"Facility/{CERSID}/Inspection/{inspectionID}/Delete",
				new { controller = "Inspection", action = "Delete", CERSID = UrlParameter.Optional, inspectionID = UrlParameter.Optional }
				);
		}

		#endregion RegisterInspectionRoutes

		#region RegisterViolationRoutes

		public static void RegisterViolationRoutes(RouteCollection routes)
		{
			routes.MapRoute("ViolationIndex", "Violation", new { controller = "Violation", action = "Index" });
			routes.MapRoute(ViolationManage.Index, "Violation", new { controller = "Violation", action = "Index" });
			routes.MapRoute("ViolationSearch_GridBindingViolationTypes", "Violation/Search_GridBindingViolationTypes", new { controller = "Violation", action = "Search_GridBindingViolationTypes" });

			routes.MapRoute("Violation_ViolationTypeSearch",
				"Facility/{CERSID}/Inspection/{inspectionID}/Violation/{violationID}/ViolationTypeSearch",
				new { controller = "Violation", action = "_ViolationTypeSearch", CERSID = UrlParameter.Optional, inspectionID = UrlParameter.Optional, violationID = UrlParameter.Optional }
				);

			// Define Routes for use when creating Violations for a specific Inspection CME Batch ID:
			// - FacilityInspectionCMEBatchViolationAdd
			// - FacilityInspectionCMEBatchViolationCreate
			// - FacilityInspectionCMEBatchViolationEdit
			// - FacilityInspectionCMEBatchViolationDelete
			// The CMEBatchID references one or more inspected programs, and is only used during the
			// Inspection creation process to easily identify all inspected programs.

			routes.MapRoute("FacilityInspectionCMEBatchViolationAdd",
				"Facility/{CERSID}/Inspection/{inspectionID}/CMEBatch/{cmeBatchID}/Violation/Add",
				new { controller = "Violation", action = "Add", CERSID = UrlParameter.Optional, inspectionID = UrlParameter.Optional, cmeBatchID = UrlParameter.Optional }
				);

			// Add new Route to allow "Save + Create Copy" Violation Functionality
			routes.MapRoute("FacilityInspectionCMEBatchViolationCreateCopy",
				"Facility/{CERSID}/Inspection/{inspectionID}/CMEBatch/{cmeBatchID}/Violation/Create/{violationTypeID}/{violationID}",
				new { controller = "Violation", action = "Create", CERSID = UrlParameter.Optional, inspectionID = UrlParameter.Optional, cmeBatchID = UrlParameter.Optional, violationTypeID = UrlParameter.Optional, violationID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityInspectionCMEBatchViolationCreate",
				"Facility/{CERSID}/Inspection/{inspectionID}/CMEBatch/{cmeBatchID}/Violation/Create/{violationTypeID}",
				new { controller = "Violation", action = "Create", CERSID = UrlParameter.Optional, inspectionID = UrlParameter.Optional, cmeBatchID = UrlParameter.Optional, violationTypeID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityInspectionCMEBatchViolationEdit",
				"Facility/{CERSID}/Inspection/{inspectionID}/CMEBatch/{cmeBatchID}/Violation/{violationID}/Edit",
				new { controller = "Violation", action = "Edit", CERSID = UrlParameter.Optional, inspectionID = UrlParameter.Optional, cmeBatchID = UrlParameter.Optional, violationID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityInspectionCMEBatchViolationDelete",
				"Facility/{CERSID}/Inspection/{inspectionID}/CMEBatch/{cmeBatchID}/Violation/{violationID}/Delete",
				new { controller = "Violation", action = "Delete", CERSID = UrlParameter.Optional, inspectionID = UrlParameter.Optional, cmeBatchID = UrlParameter.Optional, violationID = UrlParameter.Optional }
				);

			// Define Routes for use when creating Violations for a specific Inspection ID
			// - FacilityInspectionViolationAdd
			// - FacilityInspectionViolationCreate
			// - FacilityInspectionViolationEdit
			// - FacilityInspectionViolationDelete
			// These are used when editing an existing Inspection (no need for CME Batch functionality).

			routes.MapRoute("FacilityInspectionViolationAdd",
				"Facility/{CERSID}/Inspection/{inspectionID}/Violation/Add",
				new { controller = "Violation", action = "Add", CERSID = UrlParameter.Optional, inspectionID = UrlParameter.Optional }
				);

			// Add new Route to allow "Save + Create Copy" Violation Functionality
			routes.MapRoute("FacilityInspectionViolationCreateCopy",
				"Facility/{CERSID}/Inspection/{inspectionID}/Violation/Create/{violationTypeID}/{violationID}",
				new { controller = "Violation", action = "Create", CERSID = UrlParameter.Optional, inspectionID = UrlParameter.Optional, violationTypeID = UrlParameter.Optional, violationID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityInspectionViolationCreate",
				"Facility/{CERSID}/Inspection/{inspectionID}/Violation/Create/{violationTypeID}",
				new { controller = "Violation", action = "Create", CERSID = UrlParameter.Optional, inspectionID = UrlParameter.Optional, violationTypeID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityInspectionViolationEdit",
				"Facility/{CERSID}/Inspection/{inspectionID}/Violation/{violationID}/Edit",
				new { controller = "Violation", action = "Edit", CERSID = UrlParameter.Optional, inspectionID = UrlParameter.Optional, violationID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityInspectionViolationDelete",
				"Facility/{CERSID}/Inspection/{inspectionID}/Violation/{violationID}/Delete",
				new { controller = "Violation", action = "Delete", CERSID = UrlParameter.Optional, inspectionID = UrlParameter.Optional, violationID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityInspectionViolationSummary",
				"Facility/{CERSID}/Inspection/{inspectionID}/Violation/{violationID}",
				new { controller = "Violation", action = "Summary", CERSID = UrlParameter.Optional, inspectionID = UrlParameter.Optional, violationID = UrlParameter.Optional }
				);
		}

		#endregion RegisterViolationRoutes

		#region RegisterEnforcementRoutes

		public static void RegisterEnforcementRoutes(RouteCollection routes)
		{
			routes.MapRoute("EnforcementIndex",
				"Enforcement",
				new { controller = "Enforcement", action = "Index" }
				);

			routes.MapRoute(EnforcementManage.Index, "Enforcement", new { controller = "Enforcement", action = "Index" });

			routes.MapRoute("EnforcementAdd",
				"Enforcement/Add",
				new { controller = "Enforcement", action = "Add" }
				);

			routes.MapRoute("FacilityEnforcementCreate",
				"Facility/{CERSID}/Enforcement/Create",
				new { controller = "Enforcement", action = "Create", CERSID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityEnforcementSummary",
				"Facility/{CERSID}/Enforcement/{enforcementID}",
				new { controller = "Enforcement", action = "Summary", CERSID = UrlParameter.Optional, enforcementID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityEnforcementEdit",
				"Facility/{CERSID}/Enforcement/{enforcementID}/Edit",
				new { controller = "Enforcement", action = "Edit", CERSID = UrlParameter.Optional, enforcementID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityEnforcementDelete",
				"Facility/{CERSID}/Enforcement/{enforcementID}/Delete",
				new { controller = "Enforcement", action = "Delete", CERSID = UrlParameter.Optional, enforcementID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityEnforcementViolation",
				"Facility/{CERSID}/Enforcement/{enforcementID}/Violation",
				new { controller = "Enforcement", action = "Violation", CERSID = UrlParameter.Optional, enforcementID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityEnforcementAddViolation",
				"Facility/{CERSID}/Enforcement/{enforcementID}/AddViolation",
				new { controller = "Enforcement", action = "AddViolation", CERSID = UrlParameter.Optional, enforcementID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityEnforcementLinkViolation",
				"Facility/{CERSID}/Enforcement/{enforcementID}/LinkViolation/{violationID}",
				new { controller = "Enforcement", action = "LinkViolation", CERSID = UrlParameter.Optional, enforcementID = UrlParameter.Optional, violationID = UrlParameter.Optional }
				);

			routes.MapRoute("FacilityEnforcementUnlinkViolation",
				"Facility/{CERSID}/Enforcement/{enforcementID}/UnlinkViolation/{violationID}",
				new { controller = "Enforcement", action = "UnlinkViolation", CERSID = UrlParameter.Optional, enforcementID = UrlParameter.Optional, violationID = UrlParameter.Optional }
				);
		}

		#endregion RegisterEnforcementRoutes

		#region RegisterComplianceRoutes

		public static void RegisterComplianceRoutes(RouteCollection routes)
		{
			routes.MapRoute("ComplianceIndex",
				"Compliance",
				new { controller = "Compliance", action = "Index" }
				);

			routes.MapRoute(ComplianceManage.Index, "Compliance", new { controller = "Compliance", action = "Index" });
			routes.MapRoute(ComplianceManage.UploadCMEData, "Compliance/UploadCMEData", new { controller = "Compliance", action = "UploadCMEData" });
		}

		#endregion RegisterComplianceRoutes

		#region RegisterResponderRoutes

		public static void RegisterResponderRoutes(RouteCollection routes)
		{
			routes.MapRoute(ResponderRoute.Index, "Responder", new { controller = "Responder", action = "Index" });
			routes.MapRoute(ResponderRoute.Map, "Responder/Map", new { controller = "Responder", action = "Map" });
			routes.MapRoute(ResponderRoute.Summary, "Responder/{CERSID}", new { controller = "Responder", action = "Summary", CERSID = UrlParameter.Optional });
			routes.MapRoute(ResponderRoute.InventoryDetail, "Responder/{CERSID}/InventoryDetail", new { controller = "Responder", action = "InventoryDetail", CERSID = UrlParameter.Optional });
			routes.MapRoute(ResponderRoute.SiteMap, "Responder/{CERSID}/SiteMap", new { controller = "Responder", action = "SiteMap", CERSID = UrlParameter.Optional });
		}

		#endregion RegisterResponderRoutes

		#region RegisterReportRoutes

		public static void RegisterReportRoutes(RouteCollection routes)
		{
			routes.MapRoute("ReportsIndex", "Reports", new { controller = "Reports", action = "Index" });
			routes.MapRoute(ReportRoute.HazMatInventoryDownload, "Reports/HazMatInventoryDownload", new { controller = "Reports", action = "HazMatInventoryDownload" });
			routes.MapRoute(ReportRoute.CUPAElectronicReportingStatus, "Reports/CUPAElectronicReportingStatus", new { controller = "Reports", action = "CUPAElectronicReportingStatus" });

			routes.MapRoute(ReportRoute.RegulatorFacInfoDownload, "Reports/FacilityInformationDownload", new { controller = "Reports", action = "FacilityInformationDownload" });
			routes.MapRoute(ReportRoute.NewFacilitiesAddedToCERS, "Reports/NewFacilitiesAddedToCERS", new { controller = "Reports", action = "NewFacilities" });
			routes.MapRoute(ReportRoute.RegulatorSubmittalElementsLocalRequirements, "Reports/RegulatorLocalRequirements", new { controller = "Reports", action = "RegulatorLocalRequirements" });
			routes.MapRoute(ReportRoute.RegulatorSubmittalElementsLocalRequirements_Aysnc, "Reports/RegulatorLocalRequirements_Async", new { controller = "Reports", action = "RegulatorLocalRequirements_Async" });
			routes.MapRoute(ReportRoute.RegionalInventoryMaterialsSearch, "Reports/RegionalInventoryMaterialsSearch", new { controller = "Reports", action = "RegionalInventoryMaterialsSearch" });
            routes.MapRoute( ReportRoute.RegulatorRCRALQGDownloadCME, "Reports/RegulatorRCRALQGDownloadCME", new { controller = "Reports", action = "RegulatorRCRALQGDownloadCME" } );

            routes.MapRoute( ReportRoute.RegulatedFacilitiesbyUnifiedProgramElement, "Reports/RegulatedFacilitiesbyUnifiedProgramElementReport", new { controller = "Reports", action = "RegulatedFacilitiesbyUnifiedProgramElementReport" } );
            routes.MapRoute( ReportRoute.RegulatedFacilityInspection, "Reports/RegulatedFacilityInspectionReport", new { controller = "Reports", action = "RegulatedFacilityInspectionReport" } );
            routes.MapRoute( ReportRoute.SummaryEnforcement, "Reports/SummaryEnforcementReport", new { controller = "Reports", action = "RegulatedFacilitySummaryEnforcementReport" } );

			// UST Report / Data Download Routes:
			routes.MapRoute(ReportRoute.USTBOEFacilitySearch, "Reports/USTBOEFacilitySearch", new { controller = "Reports", action = "USTBOEFacilitySearch" });
			routes.MapRoute(ReportRoute.USTDataDownloadCME, "Reports/USTDataDownloadCME", new { controller = "Reports", action = "USTDataDownloadCME" });
			routes.MapRoute(ReportRoute.USTDataDownloadFacilityTank, "Reports/USTDataDownloadFacilityTank", new { controller = "Reports", action = "USTDataDownloadFacilityTank" });
			routes.MapRoute(ReportRoute.USTEnforcementSummaryByRegulator, "Reports/USTEnforcementSummaryByRegulator", new { controller = "Reports", action = "USTEnforcementSummaryByRegulator" });
			routes.MapRoute(ReportRoute.USTFacilityComplianceDetails, "Reports/USTFacilityComplianceDetails", new { controller = "Reports", action = "USTFacilityComplianceDetails" });
			routes.MapRoute(ReportRoute.USTFacilityFinancialResponsibilitySummaryByRegulator, "Reports/USTFacilityFinancialResponsibilitySummaryByRegulator", new { controller = "Reports", action = "USTFacilityFinancialResponsibilitySummaryByRegulator" });
			routes.MapRoute(ReportRoute.USTFacilityOwnerTypeSummaryByRegulator, "Reports/USTFacilityOwnerTypeSummaryByRegulator", new { controller = "Reports", action = "USTFacilityOwnerTypeSummaryByRegulator" });
			routes.MapRoute(ReportRoute.USTInspectionSummaryByRegulator, "Reports/USTInspectionSummaryByRegulator", new { controller = "Reports", action = "USTInspectionSummaryByRegulator" });
			routes.MapRoute(ReportRoute.USTRedTagFacilityDetails, "Reports/USTRedTagFacilityDetails", new { controller = "Reports", action = "USTRedTagFacilityDetails" });
			routes.MapRoute(ReportRoute.USTReport6SummaryByRegulator, "Reports/USTReport6SummaryByRegulator", new { controller = "Reports", action = "USTReport6SummaryByRegulator" });
			routes.MapRoute(ReportRoute.USTSemiAnnualReport, "Reports/USTSemiAnnualReport", new { controller = "Reports", action = "USTSemiAnnualReport" });
			routes.MapRoute(ReportRoute.USTStatewideLeakPreventionReport, "Reports/USTStatewideLeakPreventionReport", new { controller = "Reports", action = "USTStatewideLeakPreventionReport" });
			routes.MapRoute(ReportRoute.USTTankTypeMonitoringMethodSummaryByRegulator, "Reports/USTTankTypeMonitoringMethodSummaryByRegulator", new { controller = "Reports", action = "USTTankTypeMonitoringMethodSummaryByRegulator" });
		}

		#endregion RegisterReportRoutes

		#region RegisterRegulatorDocRoutes

		public static void RegisterRegulatorDocRoutes(RouteCollection routes)
		{
			routes.MapRoute("RegulatorDocument",
				"{regulatorId}/Document",
				new { controller = "RegulatorDocument", action = "Index", regulatorId = UrlParameter.Optional }

				);

			routes.MapRoute("RegulatorDocumentNew",
				"{regulatorId}/Document/New",
				new { controller = "RegulatorDocument", action = "New", regulatorId = UrlParameter.Optional }
			);

			routes.MapRoute("RegulatorDocumentSummary",
				"{regulatorId}/Document/{regulatorDocumentId}",
				new { controller = "RegulatorDocument", action = "Summary", regulatorId = UrlParameter.Optional, regulatorDocumentId = UrlParameter.Optional }
			 );

			routes.MapRoute("RegulatorDocumentEdit",
				"{regulatorId}/Document/{regulatorDocumentId}/Edit",
				new { controller = "RegulatorDocument", action = "Edit", regulatorId = UrlParameter.Optional, regulatorDocumentId = UrlParameter.Optional }
			 );

			routes.MapRoute("RegulatorDocumentDelete",
				"{regulatorId}/Document/{regulatorDocumentId}/Delete",
				new { controller = "RegulatorDocument", action = "Delete", regulatorId = UrlParameter.Optional, regulatorDocumentId = UrlParameter.Optional }
			 );
		}

		#endregion RegisterRegulatorDocRoutes

		#region Application_Start Method

		protected void Application_Start()
		{
			try
			{
				AreaRegistration.RegisterAllAreas();

				RegisterGlobalFilters(GlobalFilters.Filters);
				RegisterRoutes(RouteTable.Routes);
				BundleConfig.RegisterBundles(BundleTable.Bundles);
				ModelMetadataProviders.Current = new CERSModelMetadataProvider();
                
                //register the custom model binder
                //ModelBinders.Binders.Add(typeof(string), new CleanModelBinder());

				DataRegistry.Initialize();
				LicenseKeys.Intialize();
				ApplicationOnStartShared.Initialize();
			}
			catch ( ReflectionTypeLoadException ex )
			{
				StringBuilder sb = new StringBuilder();
				foreach ( Exception exSub in ex.LoaderExceptions )
				{
					sb.AppendLine(exSub.Message);
					if ( exSub is FileNotFoundException )
					{
						FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
						if ( !string.IsNullOrEmpty(exFileNotFound.FusionLog) )
						{
							sb.AppendLine("Fusion Log:");
							sb.AppendLine(exFileNotFound.FusionLog);
						}
					}
					sb.AppendLine();
				}
				string errorMessage = sb.ToString();
				//Display the error
				throw new System.ArgumentException(errorMessage, "load assembly");
			}
		}

		#endregion Application_Start Method

		#region GetRouteName Methods

		public static string GetRouteName(params Part[] parts)
		{
			return RouteHelper.GetRouteName(parts);
		}

		public static string GetRouteName(SubmittalElementType submittalElement, params Part[] parts)
		{
			return RouteHelper.GetRouteName(submittalElement, parts);
		}

		public static string GetRouteName(SubmittalElementType submittalElement, ResourceType resourceType, params Part[] parts)
		{
			return RouteHelper.GetRouteName(submittalElement, resourceType, parts);
		}

		#endregion GetRouteName Methods
	}
}