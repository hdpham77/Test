using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.Plugins
{
	[Plugin("Event Template Reprocessor", Description = "Reprocesses the Event Messages against the Templates. Useful to regenerate against newer templates.", Order = 1, DeveloperName = "Mike")]
	public class EventMessageTemplateReprocessor : SimplePlugin
	{
		protected override void DoWork()
		{
			var events = (from e in DataModel.Events.Include("Contact").Include("SubmittalElement").Include("Organization").Include("Regulator").Include("Facility").Include("Parameters").Include("Type")
						  where !e.Voided //&& e.OrganizationMessage.Contains("$") || e.RegulatorMessage.Contains("$")
						  select e
						  ).ToList();

			int total = events.Count();
			int index = 0;
			OnNotification("Processing " + total + " Event(s)...");
			foreach (var e in events)
			{
				OnNotification("Processing Event ID: " + e.ID + "...");
				Services.TextMacroProcessing.ProcessEvent(e, forceRefreshTemplateContent: true);
				Repository.Events.Update(e);
				OnNotification("Updated Event ID: " + e.ID + ".");
				CalculateProgress(index, total);
				index++;
			}
		}
	}
}