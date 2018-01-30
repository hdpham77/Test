using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CERS.EDT
{
	public abstract class InboundAdapter : Adapter, IInboundAdapter
	{
		public InboundAdapter(EDTTransactionScope transactionScope)
			: base(transactionScope)
		{
		}

		public abstract XElement Process(Stream stream, NameValueCollection arguments);
	}

	public abstract class InboundAdapter<TArgs> : InboundAdapter, IEDTInboundProcessor<TArgs> where TArgs : IAdapterArguments
	{
		public InboundAdapter(EDTTransactionScope transactionScope)
			: base(transactionScope)
		{
		}

		public abstract XElement Process(Stream stream, TArgs arguments);
	}
}