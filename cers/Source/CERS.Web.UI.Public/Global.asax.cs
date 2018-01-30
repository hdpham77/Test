using CERS.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CERS.Web.UI.Public
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode,
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		#region RegisterGlobalFilters

		public static void RegisterGlobalFilters( GlobalFilterCollection filters )
		{
			filters.Add( new CERSHandleErrorAttribute() );
		}

		#endregion RegisterGlobalFilters

		#region RegisterRoutes

		public static void RegisterRoutes( RouteCollection routes )
		{
			routes.IgnoreRoute( "{resource}.axd/{*pathInfo}" );

			routes.MapRoute( "HelpItemsTestWidget", "Help/TestWidget", new { controller = "Help", action = "TestWidget", portalID = UrlParameter.Optional } );

			routes.MapRoute( "HelpItemDocumentRetrieval", "Help/ID/{id}", new { controller = "Help", action = "GetDocument" } );

			routes.MapRoute( "HelpItemsXML", "Help", new { controller = "Help", action = "Index", portalID = UrlParameter.Optional } );

			routes.MapRoute( "Seeding", "Seeding", new { controller = "Home", action = "Seeding" } );

			routes.MapRoute( "UPAListing", "UPAListing", new { controller = "Directory", action = "UPAListing", regulatorID = UrlParameter.Optional } );

			routes.MapRoute( "RegulatorDetails", "Directory/RegulatorDetails/{regulatorID}", new { controller = "Directory", action = "RegulatorDetails", regulatorID = UrlParameter.Optional } );

			routes.MapRoute( "EvaluationDocuments", "Directory/EvaluationDocuments", new { controller = "Directory", action = "EvaluationDocuments" } );

			routes.MapRoute( "EnforcementSummaries", "Directory/EnforcementSummaries", new { controller = "Directory", action = "EnforcementSummaries" } );

			routes.MapRoute( "RegulatorDocuments", "Directory/RegulatorDocuments/{regulatorID}", new { controller = "Directory", action = "RegulatorDocuments", regulatorID = UrlParameter.Optional } );

			routes.MapRoute( "DataRegistryDataElementSearch", "DataRegistry/DataElements", new { controller = "DataRegistry", action = "Search" } );
			routes.MapRoute( "DataRegistryDataElementDetail", "DataRegistry/DataElements/{elementID}", new { controller = "DataRegistry", action = "ElementDetail", elementID = UrlParameter.Optional } );

			routes.MapRoute( "DataRegistryDataElementDetailBackwardCompatibility", "DataRegistry/Dictionary/{dataSourceKey}/{identifier}", new { controller = "DataRegistry", action = "ElementDetailByDataSourceKeyAndIdentifier", dataSourceKey = UrlParameter.Optional, identifier = UrlParameter.Optional } );

			routes.RegisterServiceRoutes();

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);
		}

		#endregion RegisterRoutes

		#region Application_Start

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters( GlobalFilters.Filters );
			RegisterRoutes( RouteTable.Routes );
			ApplicationOnStartShared.Initialize();
		}

		#endregion Application_Start
	}
}