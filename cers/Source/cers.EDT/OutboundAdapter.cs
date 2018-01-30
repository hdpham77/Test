using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CERS.Xml;

namespace CERS.EDT
{
	public abstract class OutboundAdapter : Adapter, IOutboundAdapter
	{
		public OutboundAdapter(EDTTransactionScope edtTransactionScope)

			: base(edtTransactionScope)
		{
		}

		public abstract XElement Process(NameValueCollection arguments);
	}

	public abstract class OutboundAdapter<TArgs> : Adapter, IOutboundAdapter<TArgs> where TArgs : IAdapterArguments
	{
		public OutboundAdapter(EDTTransactionScope transaction)
			: base(transaction)
		{
		}

		public abstract XElement Process(NameValueCollection arguments);

		public abstract XElement Process(TArgs args);
	}
}