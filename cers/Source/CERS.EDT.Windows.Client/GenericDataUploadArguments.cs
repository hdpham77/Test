using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.EDT.Windows.Client
{
	public class GenericDataUploadArguments
	{
		public byte[] Data { get; set; }

		public string VersionIdentifier { get; set; }
	}
}