using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.EDT.Windows.Client
{
	public class RCMEQArguments
	{
		public DateTime? StartDate { get; set; }

		public DateTime? EndDate { get; set; }

		public int? CERSID { get; set; }

		public string Status { get; set; }
	}
}