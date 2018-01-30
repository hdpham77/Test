using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.Plugins.Xml
{
	[Plugin("Regulator Facility Submittal Action Notification XML Handler", Description = "Receives an XML or ZIP file containing an XML file to process for Regulator Facility Submittal Action Notifications.", DeveloperName = "Mike R", EnableLog = true)]
	public class RegulatorFacilitySubmittalActionNotificationXml : InputOutputStreamPlugin
	{
		protected override void DoWork()
		{
		}
	}
}