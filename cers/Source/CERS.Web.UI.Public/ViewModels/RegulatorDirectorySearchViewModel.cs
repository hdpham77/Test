using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CERS.Model;
using UPF;
using UPF.ViewModel;

namespace CERS.Web.UI.Public.ViewModels
{
	public class RegulatorDirectorySearchViewModel
	{
		[Display(Name = "Facility Street Address")]
		public string Address { get; set; }

		[Display(Name = "City")]
		[Required(ErrorMessage = "City is Required.")]
		public string City { get; set; }

		[Display(Name = "ZIP Code")]
		public string ZipCode { get; set; }

		[Display( Name = "ZIP Code" )]
		public string RegulatorZipCode { get; set; }

		public IEnumerable<ISystemLookupEntity> RegulatorTypes { get; set; }

		public IEnumerable<string> Cities { get; set; }

		public IEnumerable<string> ZipCodes { get; set; }

		public IEnumerable<County> Counties { get; set; }
	}
}