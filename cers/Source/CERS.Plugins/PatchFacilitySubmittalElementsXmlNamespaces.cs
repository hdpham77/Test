using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.Plugins
{
	[Plugin("Patch Facility Submittal Elements Xml Namespaces", Description = "Fixes namespaces originally generated with old XML Schema namespace", DeveloperName = "Mike", DefaultArguments = "Statuses=Submitted", EnableLog = true)]
	public class PatchFacilitySubmittalElementsXmlNamespaces : SimplePlugin
	{
		protected override void DoWork()
		{
			var fses = from fse in DataModel.FacilitySubmittalElements where fse.StatusID != 1 && !fse.Voided select fse;
			var fsesList = fses.ToList();
		}
	}
}