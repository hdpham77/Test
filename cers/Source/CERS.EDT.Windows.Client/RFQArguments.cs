using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.EDT.Windows.Client
{
	public class RFQArguments
	{
		public DateTime? CreatedOnStart { get; set; }

		public DateTime? CreatedOnEnd { get; set; }

		public int? CERSID { get; set; }

		public int? OrganizationCode { get; set; }
	}
}