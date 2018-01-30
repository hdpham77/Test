using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.Plugins
{
	[Plugin("Organization Metadata Builder", Description = "Finds all organizations and recomputes the Metadata (FacilityCount, ContactCount, etc.)", DeveloperName = "Mike", EnableLog = false, Order = 3)]
	public class OrganizationMetadataBuilder : SimplePlugin
	{
		protected override void DoWork()
		{
			OnNotification("Fetching All Organizations...");
			var organizations = Repository.Organizations.Search().ToList();
			OnNotification("Found " + organizations.Count + " Organizations.");
			foreach (var org in organizations)
			{
				OnNotification("Recomputing Metadata for: " + org.ID + " - " + org.Name + "...");
				Repository.Organizations.Save(org);
				OnNotification("Recomputed Metadata for: " + org.ID + " - " + org.Name);
			}
		}
	}
}