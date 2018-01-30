using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace CERS.EDT.Windows.Client
{
	public class Endpoints
	{
		#region Properties

		public static string ServerBase { get { return GetEnvironmentServiceUri( App.CurrentEnvironment ); } }

		#region Endpoints

		public static string Endpoint_ChemicalLibraryQuery { get { return ServerBase + "/Library/Chemicals"; } }

		public static string Endpoint_DataDictionaryQuery { get { return ServerBase + "/Library/DataDictionary/{Dictionary}"; } }

		public static string Endpoint_EndpointMetadataQuery { get { return ServerBase + "/Endpoint/Metadata"; } }

		public static string Endpoint_RegulatorActionItemQuery { get { return ServerBase + "/Regulator/ActionItem?regulatorCode={RegulatorCode}"; } }

		public static string Endpoint_RegulatorAuthenticationTest { get { return ServerBase + "/Regulator/Authenticate?regulatorCode={RegulatorCode}"; } }

		public static string Endpoint_RegulatorCMESubmittalQuery { get { return ServerBase + "/Regulator/CME/Query?regulatorCode={RegulatorCode}"; } }

		public static string Endpoint_RegulatorCMESubmittalSubmit { get { return ServerBase + "/Regulator/CME/Submit?regulatorCode={RegulatorCode}"; } }

		public static string Endpoint_RegulatorFacilityCreate { get { return ServerBase + "/Regulator/Facility/Create?regulatorCode={RegulatorCode}"; } }

		public static string Endpoint_RegulatorFacilityInformationSubmit { get { return ServerBase + "/Regulator/FacilitySubmittal/FacilityInformationSubmit?regulatorCode={RegulatorCode}"; } }

		public static string Endpoint_RegulatorFacilityMetadata { get { return ServerBase + "/Regulator/Facility/Metadata?regulatorCode={RegulatorCode}"; } }

		public static string Endpoint_RegulatorFacilityQuery { get { return ServerBase + "/Regulator/Facility?regulatorCode={RegulatorCode}"; } }

		public static string Endpoint_RegulatorFacilitySubmittalActionNotification { get { return ServerBase + "/Regulator/FacilitySubmittal/ActionNotification?regulatorCode={RegulatorCode}"; } }

		public static string Endpoint_RegulatorFacilitySubmittalDocumentQuery { get { return ServerBase + "/Regulator/FacilitySubmittal/Query/Document/{CERSUniqueKey}"; } }

		public static string Endpoint_RegulatorFacilitySubmittalQuery { get { return ServerBase + "/Regulator/FacilitySubmittal/Query?regulatorCode={RegulatorCode}"; } }

		public static string Endpoint_RegulatorFacilitySubmittalSubmit { get { return ServerBase + "/Regulator/FacilitySubmittal/Submit?regulatorCode={RegulatorCode}"; } }

		public static string Endpoint_RegulatorFacilityTransferQuery { get { return ServerBase + "/Regulator/Faclility/Transfer?regulatorCode={RegulatorCode}"; } }

		public static string Endpoint_RegulatorListing { get { return ServerBase + "/Regulator/Listing"; } }

		public static string Endpoint_RegulatorOrganizationQuery { get { return ServerBase + "/Regulator/Organization?regulatorCode={RegulatorCode}"; } }

		public static string Endpoint_ViolationLibraryQuery { get { return ServerBase + "/Library/Violations"; } }

		#endregion Endpoints

		#endregion Properties

		#region Methods

		public static string Format( string endpointUrl, int regulatorCode )
		{
			return endpointUrl.Replace( "{RegulatorCode}", regulatorCode.ToString() );
		}

		public static string GetEnvironmentServiceUri()
		{
			return GetEnvironmentServiceUri( App.CurrentEnvironment );
		}

		public static string GetEnvironmentServiceUri( TargetEnvironment environment )
		{
			return App.Environments[environment];
		}

		#endregion Methods
	}
}