using CERS.Model;
using CERS.ViewModels.Facilities;
using CERS.ViewModels.Organizations;
using CERS.Xml.OrganizationQuery;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CERS.EDT
{
	public class RegulatorOrganizationQueryAdapter : OutboundAdapter<RegulatorOrganizatonQueryAdapterArguments>
	{
		public RegulatorOrganizationQueryAdapter( EDTTransactionScope transactionScope )
			: base( transactionScope )
		{
		}

		public override XElement Process( NameValueCollection arguments )
		{
			if ( arguments == null )
			{
				throw new ArgumentNullException( "arguments" );
			}

			RegulatorOrganizatonQueryAdapterArguments args = new RegulatorOrganizatonQueryAdapterArguments( arguments );

			return Process( args );
		}

		public override XElement Process( RegulatorOrganizatonQueryAdapterArguments args )
		{
			Regulator regulator = null;
			TransactionScope.WriteQueryArguments( args );

			if ( args.RegulatorCode != null )
			{
				regulator = Repository.Regulators.GetByEDTIdentityKey( args.RegulatorCode.Value );
				if ( regulator == null )
				{
					throw new Exception( "The specified RegulatorCode \"" + args.RegulatorCode + "\" is invalid." );
				}

				TransactionScope.Connect( regulator );
			}

			XElement result = null;
			IEnumerable<OrganizationGridViewModel> organizationData = null;
			IEnumerable<FacilityGridViewModel> facilityData = null;
			int total = 0;

			organizationData = Repository.Organizations.GridSearch(
				args.OrganizationName,
				args.OrganizationCode,
				args.OrganizationHeadquarters,
				(int) OrganizationStatus.Active,
				CERSID: args.CERSID,
				nameSearchOption: StringSearchOption.StartsWith
			);

			if ( args.EstablishedSince.HasValue )
			{
				organizationData = from d in organizationData where d.CreatedOnDate >= args.EstablishedSince select d;
			}

			total = organizationData.Count();
			TransactionScope.WriteActivity( "Query returned " + total + " Organization(s)" );

			if ( args.IncludeFacilities )
			{
				TransactionScope.WriteActivity( "Basic facility data requested. Fetching..." );
				List<int> organizationIds = organizationData.Select( p => p.ID ).ToList();
				facilityData = Repository.Facilities.GridSearch().Where( p => organizationIds.Contains( p.OrganizationID ) ).ToList();
			}

			RegulatorOrganizationQueryXmlSerializer serializer = new RegulatorOrganizationQueryXmlSerializer( Repository );
			result = serializer.Serialize( TransactionScope.Transaction, organizationData, total, facilityData );

			TransactionScope.Complete( EDTTransactionStatus.Accepted );
			TransactionScope.SaveXml( result, EDTTransactionXmlDirection.Outbound );
			return result;
		}
	}
}