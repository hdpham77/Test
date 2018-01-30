using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.EDT.Windows.Client
{
	public class ROQArguments
	{
		public int? CERSID { get; set; }

		public DateTime? EstablishedSince { get; set; }

		public bool IncludeFacilities { get; set; }

		public int? OrganizationCode { get; set; }

		public string OrganizationHeadquarters { get; set; }

		public string OrganizationName { get; set; }
	}
}