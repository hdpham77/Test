using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.Plugins.Xml
{
	[Plugin("Regulator Facility Submittal XML Processor", Description = "Receives an XML or ZIP file containing an XML file to process.", DeveloperName = "Mike R", EnableLog = true)]
	public class RegulatorFacilitySubmittalXml : InputOutputStreamPlugin
	{
		protected override void DoWork()
		{
		}
	}
}