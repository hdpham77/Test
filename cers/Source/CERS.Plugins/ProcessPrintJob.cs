using CERS.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using UPF;
using Winnovative.ExcelLib;

namespace CERS.Plugins
{
	[Plugin( "Process Print Job", Description = "Processes Print Job Queue", DeveloperName = "Andre", EnableLog = false, Order = 3 )]
	internal class ProcessPrintJob : SimplePlugin
	{
		protected override void DoWork()
		{
            Services.PrintJob.ActivityLogEntryReceived += PrintJob_ActivityLogEntryReceived;
            Services.PrintJob.ProcessPrintJobQueue();
            Services.PrintJob.ActivityLogEntryReceived -= PrintJob_ActivityLogEntryReceived;
		}

		private void PrintJob_ActivityLogEntryReceived( object sender, ActivityLogEntryReceivedEventArgs e )
		{
			OnNotification( e.Message, e.Exception != null, e.Exception );
		}
	}
}