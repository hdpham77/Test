using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.EDT.Windows.Client
{
	public class RFSQArguments
	{
		public DateTime? SubmittedOnStart { get; set; }

		public DateTime? SubmittedOnEnd { get; set; }

		public DateTime? SubmittalActionOnStart { get; set; }

		public DateTime? SubmittalActionOnEnd { get; set; }

		public int? CERSID { get; set; }

		public string SubmittalStatus { get; set; }

		public int? SubmittalElementCode { get; set; }
	}
}