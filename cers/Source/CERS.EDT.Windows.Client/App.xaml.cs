using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace CERS.EDT.Windows.Client
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static Dictionary<TargetEnvironment, string> _Environments;

		public static string AuthorizationHeader { get; set; }

		public static TargetEnvironment CurrentEnvironment { get; set; }

		public static Dictionary<TargetEnvironment, string> Environments
		{
			get
			{
				if ( _Environments == null )
				{
					LoadEnvironments();
				}
				return _Environments;
			}
		}

		public static int RegulatorCode { get; set; }

		public static string RegulatorName { get; set; }

		public static List<Regulator> Regulators { get; set; }

		public static string Username { get; set; }

		public static List<SelectableValue<int>> GetActionItemTypeList()
		{
			List<SelectableValue<int>> results = new List<SelectableValue<int>>();
			results.Add( "6) Regulator User Access Request", 6 );
			results.Add( "21) Business User Access Request (Regulator)", 21 );
			results.Add( "24) Facility Delete Request", 24 );
			results.Add( "25) Facility Merge Request", 25 );
			results.Add( "26) Facility Transfer Request", 26 );
			return results;
		}

		public static List<SelectableValue<int>> GetSubmittalElementList()
		{
			List<SelectableValue<int>> results = new List<SelectableValue<int>>();
			results.Add( "1) Facility Information", 1 );
			results.Add( "2) Hazardous Materials Inventory", 2 );
			results.Add( "3) Emergency Response and Training Plans", 3 );
			results.Add( "4) Underground Storage Tanks", 4 );
			results.Add( "5) Tiered Permitting", 5 );
			results.Add( "6) Recyclable Materials Report", 6 );
			results.Add( "7) Remote Waste Consolidation Site Annual Notification", 7 );
			results.Add( "8) Hazardous Waste Tank Closure Certification", 8 );
			results.Add( "9) Aboveground Petroleum Storage Act", 9 );
			results.Add( "10) California Accidental Release Program", 10 );
			return results;
		}

		public static List<SelectableValue<string>> GetSubmittalElementStatusList( bool omitDraftStatus = true )
		{
			List<SelectableValue<string>> results = new List<SelectableValue<string>>();
			if ( !omitDraftStatus )
			{
				results.Add( "Draft (1)", "1" );
			}
			results.Add( "Submitted (2)", "2" );
			results.Add( "Under Review (3)", "3" );
			results.Add( "Accepted (4)", "4" );
			results.Add( "Not Accepted (5)", "5" );
			results.Add( "Not Applicable (6)", "6" );
			return results;
		}

		public static void LoadEnvironments()
		{
			_Environments = new Dictionary<TargetEnvironment, string>();
			_Environments.Add( TargetEnvironment.Staging, "https://cersservices.calepa.ca.gov/Staging" );
			_Environments.Add( TargetEnvironment.Production, "https://cersservices.calepa.ca.gov/EDT" );
            _Environments.Add( TargetEnvironment.Testing, "https://cersapps.calepa.ca.gov/Testing/EDT" ); 
            _Environments.Add( TargetEnvironment.Development, "http://localhost/CERS.EDT.Web.Services" );
		}

		public static void LoadRegulators()
		{
			//get list of Regulator's
			RestClient client = new RestClient();
			try
			{
				var regulatorsXml = client.ExecuteXml( Endpoints.Endpoint_RegulatorListing, HttpMethod.Get );
				if ( regulatorsXml.Element != null )
				{
					var xml = regulatorsXml.Element;

					var regulators = from r in regulatorsXml.Element.Elements( "regulator" )
									 select new Regulator
									 {
										 Code = Convert.ToInt32( r.Attribute( "code" ).Value ),
										 Type = r.Attribute( "type" ).Value,
										 Name = r.Value
									 };

					App.Regulators = regulators.ToList();
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( "Unable to load Regulators. " + ex.Message );
			}
		}

		private void Application_LoadCompleted( object sender, System.Windows.Navigation.NavigationEventArgs e )
		{
		}
	}
}