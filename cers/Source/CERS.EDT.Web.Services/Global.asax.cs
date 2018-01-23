using CERS.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CERS.EDT.Web.Services
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode,
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterGlobalFilters( GlobalFilterCollection filters )
		{
			filters.Add( new HandleErrorAttribute() );
		}

		public static void RegisterRoutes( RouteCollection routes )
		{
			routes.IgnoreRoute( "{resource}.axd/{*pathInfo}" );

			#region Home Route

			routes.MapRoute( EDTServiceRoute.Home, "", new { controller = "Home", action = "Index" } );

			#endregion Home Route

			#region Regulator Routes

			routes.MapRoute( EDTServiceRoute.RegulatorServicesHelp, "Regulator/Help", new { controller = "RegulatorServices", action = "Index" } );
			routes.MapRoute( EDTServiceRoute.RegulatorAuthenticate, "Regulator/Authenticate", new { controller = "RegulatorServices", action = "Authenticate" } );
			routes.MapRoute( EDTServiceRoute.RegulatorListing, "Regulator/Listing", new { controller = "RegulatorServices", action = "Listing" } );
			routes.MapRoute( EDTServiceRoute.RegulatorFacilityQuery, "Regulator/Facility", new { controller = "RegulatorServices", action = "FacilityQuery" } );
			routes.MapRoute( EDTServiceRoute.RegulatorOrganizationQuery, "Regulator/Organization", new { controller = "RegulatorServices", action = "OrganizationQuery" } );
			routes.MapRoute( EDTServiceRoute.RegulatorFacilityCreate, "Regulator/Facility/Create", new { controller = "RegulatorServices", action = "FacilityCreate" } );
			routes.MapRoute( EDTServiceRoute.RegulatorFacilityMetadataSubmit, "Regulator/Facility/Metadata", new { controller = "RegulatorServices", action = "FacilityMetadata" } );
			routes.MapRoute( EDTServiceRoute.RegulatorFacilityTransferQuery, "Regulator/Faclility/Transfer", new { controller = "RegulatorServices", action = "FacilityTransferQuery" } );
			routes.MapRoute( EDTServiceRoute.RegulatorActionItemQuery, "Regulator/ActionItem", new { controller = "RegulatorServices", action = "ActionItemQuery" } );
			routes.MapRoute( EDTServiceRoute.EndpointMetadata, "Endpoint/Metadata", new { controller = "MiscServices", action = "EndpointMetadata" } );

			#region Facility Submittals

			routes.MapRoute( EDTServiceRoute.RegulatorFacilitySubmittalQueryXml, "Regulator/FacilitySubmittal/Query", new { controller = "RegulatorFacilitySubmittalServices", action = "QueryXml" } );
			routes.MapRoute( EDTServiceRoute.RegulatorFacilitySubmittalDocumentQuery, "Regulator/FacilitySubmittal/Query/Document/{uniqueKey}", new { controller = "RegulatorFacilitySubmittalServices", action = "Document", uniqueKey = UrlParameter.Optional } );
			routes.MapRoute( EDTServiceRoute.RegulatorFacilitySubmittalQueryTransaction, "Regulator/FacilitySubmittal/Query/{CERSTransactionKey}", new { controller = "FacilitySubmittalServices", action = "QueryTransaction", CERSTransactionKey = UrlParameter.Optional } );

			routes.MapRoute( EDTServiceRoute.RegulatorFacilitySubmittalSubmit, "Regulator/FacilitySubmittal/Submit", new { controller = "RegulatorFacilitySubmittalServices", action = "Submit" } );
			routes.MapRoute( EDTServiceRoute.RegulatorFacilitySubmittalSubmitTransaction, "Regulator/FacilitySubmittal/Submit/{CERSTransactionKey}", new { controller = "FacilitySubmittalServices", action = "SubmitTransaction", CERSTransactionKey = UrlParameter.Optional } );
			routes.MapRoute( EDTServiceRoute.RegulatorFacilityInformationSubmittalSubmit, "Regulator/FacilitySubmittal/FacilityInformationSubmit", new { controller = "RegulatorFacilitySubmittalServices", action = "SubmitFacilityInformation" } );
			routes.MapRoute( EDTServiceRoute.RegulatorFacilitySubmittalActionNotifications, "Regulator/FacilitySubmittal/ActionNotification", new { controller = "RegulatorFacilitySubmittalServices", action = "ActionNotification" } );

			routes.MapRoute( EDTServiceRoute.RegulatorFacilitySubmittalsHelp, "Regulator/FacilitySubmittal/Help", new { controller = "RegulatorFacilitySubmittalServices", action = "Index" } );

			#endregion Facility Submittals

			#region CME

			routes.MapRoute( EDTServiceRoute.CMESubmittalsHelp, "Regulator/CME/Help", new { controller = "CMESubmittalServices", action = "Index" } );
			routes.MapRoute( EDTServiceRoute.CMESubmittalQuery, "Regulator/CME/Query", new { controller = "CMESubmittalServices", action = "Query" } );
			routes.MapRoute( EDTServiceRoute.CMESubmittalQueryTransaction, "Regulator/CME/Query/{CERSTransactionKey}", new { controller = "CMESubmittalServices", action = "QueryTransaction", CERSTransactionKey = UrlParameter.Optional } );
			routes.MapRoute( EDTServiceRoute.CMESubmittalSubmit, "Regulator/CME/Submit", new { controller = "CMESubmittalServices", action = "Submit" } );
			routes.MapRoute( EDTServiceRoute.CMESubmittalSubmitTransaction, "Regulator/CME/Submit/{CERSTransactionKey}", new { controller = "CMESubmittalServices", action = "SubmitTransaction", CERSTransactionKey = UrlParameter.Optional } );

			#endregion CME

			#endregion Regulator Routes

			#region Library Routes

			routes.MapRoute( EDTServiceRoute.LibraryHelp, "Library/Help", new { controller = "LibraryServices", action = "Index" } );

			routes.MapRoute( EDTServiceRoute.ChemicalLibrary_WithIdentifier, "Library/Chemicals/{identifier}", new { controller = "LibraryServices", action = "ChemicalLibrary", identifier = UrlParameter.Optional } );
			routes.MapRoute( EDTServiceRoute.ChemicalLibrary, "Library/Chemicals", new { controller = "LibraryServices", action = "ChemicalLibrary" } );

			routes.MapRoute( EDTServiceRoute.ViolationLibrary_WithViolationNumber, "Library/Violations/{violationTypeNumber}", new { controller = "LibraryServices", action = "ViolationLibrary", violationTypeNumber = UrlParameter.Optional } );
			routes.MapRoute( EDTServiceRoute.ViolationLibrary, "Library/Violations", new { controller = "LibraryServices", action = "ViolationLibrary" } );

			routes.MapRoute( EDTServiceRoute.DataDictionaryLibrary_UPDD_WithIdentifier, "Library/DataDictionary/UPDD/{identifier}", new { controller = "LibraryServices", action = "DataDictionaryLibrary", dictionary = CERS.DataRegistryDataSourceType.UPDD, identifier = UrlParameter.Optional } );
			routes.MapRoute( EDTServiceRoute.DataDictionaryLibrary_UPDD, "Library/DataDictionary/UPDD", new { controller = "LibraryServices", action = "DataDictionaryLibrary", dictionary = CERS.DataRegistryDataSourceType.UPDD, identifier = UrlParameter.Optional } );

			routes.MapRoute( EDTServiceRoute.DataDictionaryLibrary_System_WithIdentifier, "Library/DataDictionary/System/{identifier}", new { controller = "LibraryServices", action = "DataDictionaryLibrary", dictionary = CERS.DataRegistryDataSourceType.System, identifier = UrlParameter.Optional } );
			routes.MapRoute( EDTServiceRoute.DataDictionaryLibrary_System, "Library/DataDictionary/System", new { controller = "LibraryServices", action = "DataDictionaryLibrary", dictionary = CERS.DataRegistryDataSourceType.System, identifier = UrlParameter.Optional } );

			routes.MapRoute( EDTServiceRoute.DataDictionaryLibrary_Supplemental_WithIdentifier, "Library/DataDictionary/Supplemental/{identifier}", new { controller = "LibraryServices", action = "DataDictionaryLibrary", dictionary = CERS.DataRegistryDataSourceType.System, identifier = UrlParameter.Optional } );
			routes.MapRoute( EDTServiceRoute.DataDictionaryLibrary_Supplemental, "Library/DataDictionary/Supplemental", new { controller = "LibraryServices", action = "DataDictionaryLibrary", dictionary = CERS.DataRegistryDataSourceType.System, identifier = UrlParameter.Optional } );

			#endregion Library Routes

			#region Public Routes

			//routes.MapRoute("EndpointDetail", "EndpointDetail", new { controller = "Home", action = "EndpointDetail", acronym = UrlParameter.Optional });
			//routes.MapRoute("Endpoints", "Endpoints", new { controller = "Home", action = "Index" });

			#endregion Public Routes

			#region Default Route

			routes.MapRoute(
					"Default", // Route name
					"{controller}/{action}/{id}", // URL with parameters
					new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
				);

			#endregion Default Route
		}

		#region ApplicationStart Method

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters( GlobalFilters.Filters );
			RegisterRoutes( RouteTable.Routes );
			ModelMetadataProviders.Current = new CERSModelMetadataProvider();
			DataRegistry.Initialize();
			LicenseKeys.Intialize();
		}

		#endregion ApplicationStart Method
	}
}