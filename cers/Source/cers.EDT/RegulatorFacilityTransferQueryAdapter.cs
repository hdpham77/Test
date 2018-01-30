using CERS.Model;
using CERS.Xml.FacilityQuery;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CERS.EDT
{
	public class RegulatorFacilityTransferQueryAdapter : OutboundAdapter<RegulatorFacilityTransferQueryArguments>
	{
		#region Constructor

		public RegulatorFacilityTransferQueryAdapter( EDTTransactionScope edtTransactionScope )
			: base( edtTransactionScope )
		{
		}

		#endregion Constructor

		public override XElement Process( NameValueCollection arguments )
		{
			if ( arguments == null )
			{
				throw new ArgumentNullException( "arguments" );
			}

			RegulatorFacilityTransferQueryArguments args = new RegulatorFacilityTransferQueryArguments( arguments );

			return Process( args );
		}

		public override XElement Process( RegulatorFacilityTransferQueryArguments args )
		{
			Regulator regulator = null;
			TransactionScope.WriteQueryArguments( args );

			if ( args.RegulatorCode.HasValue )
			{
				regulator = Repository.Regulators.GetByEDTIdentityKey( args.RegulatorCode.Value );
				if ( regulator == null )
				{
					throw new Exception( "The specified RegulatorCode \"" + args.RegulatorCode + "\" is invalid." );
				}

				TransactionScope.Connect( regulator );
			}
			else
			{
				throw new ArgumentException( "Missing RegulatorCode", "RegulatorCode" );
			}

			int? organizationID = null;
			if ( args.OrganizationCode.HasValue )
			{
				var organization = Repository.Organizations.GetByEDTIdentityKey( args.OrganizationCode.Value );
				if ( organization != null )
				{
					organizationID = organization.ID;
				}
			}

			var data = Repository.FacilityTransferHistories.GridSearch( args.CERSID, regulator.ID, organizationID, args.OccurredOnStart, args.OccurredOnEnd, args.AssumedOwnershipOnStart, args.AssumedOwnershipOnEnd );

			RegulatorFacilityTransferQueryXmlSerializer serializer = new RegulatorFacilityTransferQueryXmlSerializer( Repository );
			XElement result = serializer.Serialize( TransactionScope.Transaction, data.ToList() );

			TransactionScope.Complete( EDTTransactionStatus.Accepted );
			TransactionScope.SaveXml( result, EDTTransactionXmlDirection.Outbound );
			return result;
		}
	}
}