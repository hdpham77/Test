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
    [Plugin( "Process Submittal Delta", Description = "Processes Submittal Delta", DeveloperName = "Andre", EnableLog = false, Order = 3 )]
    internal class ProcessSubmittalDelta : SimplePlugin
	{
		protected override void DoWork()
		{
            Services.SubmittalDelta.ActivityLogEntryReceived += SubmittalDelta_ActivityLogEntryReceived;
            Services.SubmittalDelta.ProcessDeltaFSEQueue( 300 );
            Services.SubmittalDelta.ActivityLogEntryReceived -= SubmittalDelta_ActivityLogEntryReceived;
		}

		private void SubmittalDelta_ActivityLogEntryReceived( object sender, ActivityLogEntryReceivedEventArgs e )
		{
			OnNotification( e.Message, e.Exception != null, e.Exception );
		}
	}
}