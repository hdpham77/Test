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
	[Plugin( "Facility Information Upload", Description = "Processes Deferred Facility Information Uploads", DeveloperName = "Andre", EnableLog = false, Order = 3 )]
	internal class FacilityInformationUpload : SimplePlugin
	{
		protected override void DoWork()
		{
			Services.BusinessLogic.SubmittalElements.FacilityInformation.ActivityLogEntryReceived += FacilityInformation_ActivityLogEntryReceived;
			Services.BusinessLogic.SubmittalElements.FacilityInformation.ProcessDeferredProcessingQueue();
			Services.BusinessLogic.SubmittalElements.FacilityInformation.ActivityLogEntryReceived -= FacilityInformation_ActivityLogEntryReceived;
		}

		private void FacilityInformation_ActivityLogEntryReceived( object sender, ActivityLogEntryReceivedEventArgs e )
		{
			OnNotification( e.Message, e.Exception != null, e.Exception );
		}
	}
}