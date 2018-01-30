using System;
using System.ComponentModel.DataAnnotations;
using UPF.ViewModel;

namespace CERS.Web.UI.Regulator.ViewModels
{
	// This class does not fit within the Inspection, Enforcement, or Violation
	// paths in the main CERS project, and therefore was added to be specific
	// to the Regulator Web project.
	public class CMEDataUploadGridViewModel : GridViewModel
	{
		[Display(Name = "CME Data Upload ID")]
		public int CMEBatchID { get; set; }

        public int UploadedByID { get; set; }

		[Display( Name = "Uploaded By" )]
		public string UploadedBy { get; set; }

		[Display(Name = "Status")]
		public string EDTTransactionStatus { get; set; }

		[Display(Name = "Processed On")]
		public DateTime? ProcessedOn { get; set; }

		[Display(Name = "# Inspections")]
		public int? InspectionCount { get; set; }

		[Display(Name = "# Violations")]
		public int? ViolationCount { get; set; }

		[Display(Name = "# Enforcements")]
		public int? EnforcementCount { get; set; }

		[Display(Name = "# Enf Vios")]
		public int? EnforcementViolationCount { get; set; }
	}
}