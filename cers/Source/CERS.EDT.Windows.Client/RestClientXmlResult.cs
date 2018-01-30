using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CERS.EDT.Windows.Client
{
	public class RestClientXmlResult : RestClientResult
	{
		public RestClientXmlResult()
			: base()
		{
		}

		private XElement _Element;

		public XElement Element
		{
			get
			{
				if (RawData != null && _Element == null)
				{
					_Element = RawData.GetXElement();
				}
				return _Element;
			}
		}
	}
}