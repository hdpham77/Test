using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CERS.Model;
using UPF;

namespace CERS.Plugins
{
	[Plugin("Notification Template Reprocessor", Description = "Reprocesses Events and Notifications, regenerates Notifications against Templates and resends email. Useful to regenerate against newer templates.", DeveloperName = "Mike", Order = 2)]
	public class EventNotificationTemplateReprocessor : SimplePlugin
	{
		protected override void DoWork()
		{
			bool reprocessOARHelpCenters = Arguments.GetValue("reprocessOARToHelpCenter", true);
			if (reprocessOARHelpCenters)
			{
				ReProcessOARHelpCenter();
			}
		}

		protected virtual void ReProcessOARHelpCenter()
		{
			var calepaUP = Repository.Regulators.GetByEDTIdentityKey(CERSConstants.UnifiedProgramRegulatorEDTIdentityKey);

			OnNotification("Re-Processing Organization Access Requests to Help Center Events...");
			int typeID = (int)EventTypeCode.FacilityTransferRequest;
			var events = (from e in DataModel.Events
							 .Include("Contact")
							 .Include("SubmittalElement")
							 .Include("Organization")
							 .Include("Regulator")
							 .Include("Facility")
							 .Include("Parameters")
							 .Include("Type")
						  where e.TypeID == typeID && !e.Voided && !e.Completed && e.RegulatorID == calepaUP.ID
						  select e).ToList();
			int total = events.Count;
			OnNotification("Found " + total + " Event(s) that are Uncompleted, and Targetted to the Help Center");
			int index = 0;
			foreach (var e in events)
			{
				OnNotification("Processing EventID: " + e.ID);
				Services.Events.ReProcessExistingOrganizationAccessRequest(e);
				OnNotification("Completed EventID: " + e.ID);
				CalculateProgress(index, total);
				index++;
			}
		}
	}
}