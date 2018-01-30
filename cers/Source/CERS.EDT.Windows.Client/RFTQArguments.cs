using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.EDT.Windows.Client
{
	public class RFTQArguments
	{
		public DateTime? AssumedOwnershipOnEnd { get; set; }

		public DateTime? AssumedOwnershipOnStart { get; set; }

		public int? CERSID { get; set; }

		public DateTime? OccurredOnEnd { get; set; }

		public DateTime? OccurredOnStart { get; set; }

		public int? OrganizationCode { get; set; }

		public int? RegulatorCode { get; set; }
	}
}