using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.EDT.Windows.Client
{
	public class RAIQArguments
	{
		public int? CERSID { get; set; }

		public bool? Completed { get; set; }

		public int? OrganizationCode { get; set; }

		public int? RegulatorCode { get; set; }

		public DateTime? RequestedOnEnd { get; set; }

		public DateTime? RequestedOnStart { get; set; }

		public int? TypeID { get; set; }
	}
}