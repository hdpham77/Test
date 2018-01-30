using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using CERS.Compositions;
using CERS.Model;
using UPF.ViewModel;

namespace CERS.Web.UI.Regulator.ViewModels
{
	// Currently this ViewModel is used solely to support the CME Data Upload functionality
	// for the Compliance Views in the Regulator Portal.  In the future, it could be expanded
	// to provide an overview of Compliance Data in CERS.
	public class ComplianceViewModel : ViewModel
	{
		// Properties to assist with CME Data Upload
		public HttpPostedFileBase CMEDataUploadFile { get; set; }

		public IEnumerable<GuidanceMessage> CMEDataUploadGuidanceMessages { get; set; }

		public EDTTransaction EDTTransaction { get; set; }

		// CME Entity Counts (Assists with results from CME Data Upload)
		public int InspectionCount { get; set; }

		public int ViolationCount { get; set; }

		public int EnforcementCount { get; set; }

		public int EnforcementViolationCount { get; set; }

		//use by search function

		[Display( Name = "Regulator" )]
		[Required( ErrorMessage = "Please select a Regulator" )]
		public int RegulatorID { get; set; }

		[Display( Name = "Status" )]
		public int? StatusID { get; set; }

		public List<CERS.Model.Regulator> RegulatorCollection { get; set; }

		public List<SimpleNamedID> StatusCollection { get; set; }

	}
}