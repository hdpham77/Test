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
	public class RegulatorFacilityQueryAdapter : OutboundAdapter<RegulatorFacilityQueryArguments>
	{
		#region Constants

		private const int MaximumResultItems = 5000;

		#endregion Constants

		#region Constructor

		public RegulatorFacilityQueryAdapter( EDTTransactionScope transactionScope )
			: base( transactionScope )
		{
		}

		#endregion Constructor

		#region Process Method

		public override XElement Process( NameValueCollection arguments )
		{
			if ( arguments == null )
			{
				throw new ArgumentNullException( "arguments" );
			}

			RegulatorFacilityQueryArguments args = new RegulatorFacilityQueryArguments( arguments );

			return Process( args );
		}

		public override XElement Process( RegulatorFacilityQueryArguments args )
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
			IEnumerable<FacilityQueryResult> data = null;
			int total = 0;

			data = Repository.Facilities.EDTFacilityQuery( args.OrganizationCode, args.CERSID, args.RegulatorCode, args.CreatedOnStart, args.CreatedOnEnd, args.Street, args.City, args.ZIP );
			total = data.Count();
			if ( total > MaximumResultItems )
			{
				data = data.Take( MaximumResultItems );
			}

			RegulatorFacilityQueryXmlSerializer serializer = new RegulatorFacilityQueryXmlSerializer( Repository );
			result = serializer.Serialize( TransactionScope.Transaction, data, total );

			TransactionScope.Complete( EDTTransactionStatus.Accepted );
			TransactionScope.SaveXml( result, EDTTransactionXmlDirection.Outbound );
			return result;
		}

		#endregion Process Method
	}
}