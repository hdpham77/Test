using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CERS.EDT
{
	public interface IOutboundAdapter : IAdapter
	{
		XElement Process(NameValueCollection arguments);
	}

	public interface IOutboundAdapter<TArgs> : IOutboundAdapter where TArgs : IAdapterArguments
	{
		XElement Process(TArgs arguments);
	}
}