using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.Plugins
{
	[Plugin("Process Email Queue", Description = "Finds all emails queued and sends them.", DeveloperName = "Mike", EnableLog = false, Order = 3)]
	public class ProcessEmailQueue : SimplePlugin
	{
		protected override void DoWork()
		{
			Services.Emails.ProcessQueue();
		}
	}
}