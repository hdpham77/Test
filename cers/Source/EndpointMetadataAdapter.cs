using CERS.Xml;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CERS.EDT
{
	public class EndpointMetadataAdapter : OutboundAdapter
	{
		public EndpointMetadataAdapter( EDTTransactionScope scope )
			: base( scope )
		{
		}

		public override XElement Process( NameValueCollection arguments )
		{
			EndpointMetadataXmlSerializer serializer = new EndpointMetadataXmlSerializer( Repository );

			return serializer.Serialize();
		}
	}
}