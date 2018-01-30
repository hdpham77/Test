using CERS.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using UPF;

namespace CERS.Plugins
{
    [Plugin( "Process Deferred Job Document Clean Up", Description = "Processes Purging Deferred Job Physical Document", DeveloperName = "Andre", EnableLog = false, Order = 3 )]
	internal class ProcessJobDocumentCleanUp : SimplePlugin
	{
		protected override void DoWork()
		{
            Services.JobDocumentCleanUp.ActivityLogEntryReceived += JobDocumentCleanUp_ActivityLogEntryReceived;
            Services.JobDocumentCleanUp.ProcessJobDocumentCleanUp();
            Services.JobDocumentCleanUp.ActivityLogEntryReceived -= JobDocumentCleanUp_ActivityLogEntryReceived;
		}

        private void JobDocumentCleanUp_ActivityLogEntryReceived( object sender, ActivityLogEntryReceivedEventArgs e )
		{
			OnNotification( e.Message, e.Exception != null, e.Exception );
		}
	}
}