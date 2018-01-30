using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CERS.Xml;
using CERS.Xml.FacilitySubmittal;

namespace CERS.Plugins.Xml
{
	[
	Plugin(
		"Regulator Facility Submittal Export XML Generator",
		Description = "Generates a Regulator Facility Submittal Export XML file based on parameters received.",
		DeveloperName = "Mike R",
		EnableLog = true,
		DefaultArguments = "start=1/9/2012;end=4/30/2012;regulatorCode="
		)
	]
	public class RegulatorFacilitySubmittalExportXml : OutputStreamPlugin
	{
		private RegulatorFacilitySubmittalQueryXmlSerializer _Serializer;

		protected override void DoWork()
		{
			_Serializer = new RegulatorFacilitySubmittalQueryXmlSerializer(Repository);
		}
	}
}