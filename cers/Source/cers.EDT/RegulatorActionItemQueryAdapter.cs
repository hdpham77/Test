using CERS.Model;
using CERS.Xml.ActionItems;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CERS.EDT
{
	public class RegulatorActionItemQueryAdapter : OutboundAdapter<RegulatorActionItemQueryArguments>
	{
		#region Constructor

		public RegulatorActionItemQueryAdapter( EDTTransactionScope edtTransactionScope )
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

			RegulatorActionItemQueryArguments args = new RegulatorActionItemQueryArguments( arguments );

			return Process( args );
		}

		public override XElement Process( RegulatorActionItemQueryArguments args )
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

			var data = Repository.Events.SearchActionItems( args.RequestedOnStart, args.RequestedOnEnd, args.CERSID, organizationCode: args.OrganizationCode, typeID: args.TypeID, completed: args.Completed );

			RegulatorActionItemQueryXmlSerializer serializer = new RegulatorActionItemQueryXmlSerializer( Repository );
			XElement result = serializer.Serialize( TransactionScope.Transaction, data );

			TransactionScope.Complete( EDTTransactionStatus.Accepted );
			try
			{
				TransactionScope.SaveXml( result, EDTTransactionXmlDirection.Outbound );
			}
			catch { }
			return result;
		}
	}
}