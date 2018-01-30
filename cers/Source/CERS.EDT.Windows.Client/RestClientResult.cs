using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace CERS.EDT.Windows.Client
{
	public class RestClientResult
	{
		public string EndpointUrl { get; set; }

		public HttpStatusCode Status { get; set; }

		public byte[] RawData { get; set; }

		public string ContentType { get; set; }

		public long ContentLength { get; set; }

		public string StatusDescription { get; set; }

		public Exception Exception { get; set; }

		public bool Error
		{
			get
			{
				return (Exception != null);
			}
		}

		public RestClientResult()
		{
		}
	}
}