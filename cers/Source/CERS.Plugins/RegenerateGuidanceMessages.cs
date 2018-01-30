using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.Plugins
{
	[Plugin("Regenerate Guidance Messages", Description = "Regenerates Guidance Messages/Revalidates all Draft Facility Submittal Elements", DeveloperName = "Mike", EnableLog = false, Order = 3)]
	public class RegenerateGuidanceMessages : SimplePlugin
	{
		protected override void DoWork()
		{
			OnNotification("Fetching Draft FacilitySubmittalElements...");
			var fses = (from fse in DataModel.FacilitySubmittalElements where !fse.Voided && fse.StatusID == 1 select fse).ToList();
			OnNotification("Found " + fses.Count + " Draft FacilitySubmittal Elements...");
			int total = fses.Count;
			int index = 0;
			foreach (var fse in fses)
			{
				fse.ValidateAndCommitResults(Repository, CallerContext.UI);
				OnNotification("FSE Validation Updated for ID: " + fse.ID);
				CalculateProgress(index, total);
				index++;
			}
		}
	}
}