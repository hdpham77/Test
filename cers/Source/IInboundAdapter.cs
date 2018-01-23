using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CERS.EDT
{
	public interface IInboundAdapter : IAdapter
	{
		XElement Process(Stream stream, NameValueCollection arguments);
	}

	public interface IEDTInboundProcessor<TArgs> : IInboundAdapter where TArgs : IAdapterArguments
	{
		XElement Process(Stream stream, TArgs arguments);
	}
}