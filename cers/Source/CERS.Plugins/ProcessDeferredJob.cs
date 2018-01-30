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
    [Plugin( "Process Deferred Job", Description = "Processes Deferred Job Queue", DeveloperName = "Andre", EnableLog = false, Order = 3 )]
    internal class ProcessDeferredJob : SimplePlugin
	{
		protected override void DoWork()
		{
            Services.DeferredJob.ActivityLogEntryReceived += DeferredJob_ActivityLogEntryReceived;
            Services.DeferredJob.ProcessDeferredJobQueue();
            Services.DeferredJob.ActivityLogEntryReceived -= DeferredJob_ActivityLogEntryReceived;
		}

        private void DeferredJob_ActivityLogEntryReceived( object sender, ActivityLogEntryReceivedEventArgs e )
		{
			OnNotification( e.Message, e.Exception != null, e.Exception );
		}
	}
}