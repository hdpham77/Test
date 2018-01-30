using CERS.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using UPF;
using Winnovative.ExcelLib;

namespace CERS.Plugins
{
	[Plugin( "Hazardous Materials Inventory Upload", Description = "Processes Deferred HMI Uploads", DeveloperName = "Andre", EnableLog = false, Order = 3 )]
	public class HazardousMaterialsInventoryUpload : SimplePlugin
	{
		protected override void DoWork()
		{
			Services.BusinessLogic.SubmittalElements.HazardousMaterialsInventory.ActivityLogEntryReceived += HazardousMaterialsInventory_ActivityLogEntryReceived;
			Services.BusinessLogic.SubmittalElements.HazardousMaterialsInventory.ProcessDeferredProcessingQueue();
			Services.BusinessLogic.SubmittalElements.HazardousMaterialsInventory.ActivityLogEntryReceived -= HazardousMaterialsInventory_ActivityLogEntryReceived;
		}

		private void HazardousMaterialsInventory_ActivityLogEntryReceived( object sender, ActivityLogEntryReceivedEventArgs e )
		{
			OnNotification( e.Message, e.Exception != null, e.Exception );
		}
	}
}