using CERS.Model;
using CERS.ViewModels.Inspections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using UPF.ViewModel;

namespace CERS.Web.UI.Regulator.ViewModels
{
	/**
	 * NewInspectionViewModel contains the additional data necessary to support
	 * the "Create Inspection" screen.
	 */

	public class CreateInspectionViewModel : EntityGridViewModel<Inspection, InspectionGridViewModel>
	{
		// CERSID is required, not nullable
		public int CERSID { get; set; }

		public CMEViolationContext CMEViolationContext { get; set; }

		// Tracks if the New Inspection is being created with the intention of entering Violation
		// Summary data for Detailed data:
		public IEnumerable<SelectListItem> CMEViolationContexts { get; set; }

		public Facility Facility { get; set; }

		public override IEnumerable<InspectionGridViewModel> GridView
		{
			get
			{
				List<InspectionGridViewModel> results = new List<InspectionGridViewModel>();
				if ( Entities != null )
				{
					results.AddRange( Entities.ToGridView() );
				}
				return results;
			}
		}

		// Track List of Programs Inspected (checkboxes, summary counts, comments):
		public List<CreateInspectionProgramViewModel> InspectedPrograms { get; set; }
	}
}