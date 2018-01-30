using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CERS.Model;
using CERS.Repository;
using CERS.Xml;

namespace CERS.Plugins
{
	[Plugin("Generate FSE Regulator Facility Submittal XML Packages", Description = "Generate the XML Fragments for each Facility Submittal Element", DeveloperName = "Mike", DefaultArguments = "Statuses=Submitted", EnableLog = true)]
	public class GenerateSubmittalElementRegulatorExportXml : SimplePlugin
	{
		//private const string StatusesArgumentKey = "Statuses";

		public List<SubmittalElementType> TargetSubmittalElementTypes { get; set; }

		public List<SubmittalElementStatus> TargetSubmittalElementStatuses { get; set; }

		protected override void DoWork()
		{
			Repository.FacilitySubmittalElements.NotificationReceived += new EventHandler<RepositoryNotificationReceivedEventArgs>(FacilitySubmittalElements_NotificationReceived);

			var availableSubmittalElementStatuses = Enum.GetValues(typeof(SubmittalElementStatus)).OfType<SubmittalElementStatus>().Where(p => p != SubmittalElementStatus.Draft).ToList();
			var availableSubmittalElementTypes = Enum.GetValues(typeof(SubmittalElementType)).OfType<SubmittalElementType>().ToList();

			//Default to All
			TargetSubmittalElementStatuses = availableSubmittalElementStatuses;
			TargetSubmittalElementTypes = availableSubmittalElementTypes;

			//Define overrides if necessary
			List<SubmittalElementStatus> selectedSubmittalElementStatuses = new List<SubmittalElementStatus>();
			List<SubmittalElementType> selectedSubmittalElementTypes = new List<SubmittalElementType>();

			//selectedSubmittalElementStatuses.Add(SubmittalElementStatus.NotAccepted);
			//selectedSubmittalElementTypes.Add(SubmittalElementType.FacilityInformation);
			//selectedSubmittalElementTypes.Add(SubmittalElementType.EmergencyResponseandTrainingPlans);

			if (selectedSubmittalElementStatuses.Count > 0)
			{
				TargetSubmittalElementStatuses = TargetSubmittalElementStatuses.Intersect(selectedSubmittalElementStatuses).ToList();
			}

			if (selectedSubmittalElementTypes.Count > 0)
			{
				TargetSubmittalElementTypes = TargetSubmittalElementTypes.Intersect(selectedSubmittalElementTypes).ToList();
			}

			OnNotification("Fetching Submittal Elements...");

			foreach (var targetSubmittalElementStatus in TargetSubmittalElementStatuses)
			{
				ProcessSubmittalElements(targetSubmittalElementStatus);
				GC.Collect();
				ResetPercentComplete();
			}

			Repository.FacilitySubmittalElements.NotificationReceived -= FacilitySubmittalElements_NotificationReceived;
		}

		private void FacilitySubmittalElements_NotificationReceived(object sender, RepositoryNotificationReceivedEventArgs e)
		{
			OnNotification(e.Message);
		}

		protected void ProcessSubmittalElements(SubmittalElementStatus targetSubmittalElementStatus)
		{
			foreach (SubmittalElementType targetSubmittalElementType in TargetSubmittalElementTypes)
			{
				ProcessSubmittalElements(targetSubmittalElementStatus, targetSubmittalElementType);
				GC.Collect();
				ResetPercentComplete();
			}
		}

		protected void ProcessSubmittalElements(SubmittalElementStatus status, SubmittalElementType type)
		{
			int fseIndex = 0;
			int fseCount = 0;
			XmlSerializationResult serializationResult = null;
			List<FacilitySubmittalElement> fses = null;
			bool unprocessedOnly = true;

			OnNotification("Fetching " + (unprocessedOnly ? " Unprocessed " : "") + " '" + status.ToString() + "' FSE Records for '" + type.ToString() + "'...");

			fses = Repository.FacilitySubmittalElements.EDTBatchSearch(type, status, unprocessed: unprocessedOnly).ToList();

			fseCount = fses.Count;
			fseIndex = 0;
			OnNotification("Fetched " + (unprocessedOnly ? " Unprocessed " : "") + " '" + status.ToString() + "' FSE Records for " + type.ToString() + ". Processing " + fseCount + " FSE's...");

			foreach (var fse in fses)
			{
				try
				{
					serializationResult = Repository.FacilitySubmittalElements.UpdateXmlMetadata(fse, saveChanges: true, ignoreCommonFieldUpdate: true);
					serializationResult.Xml = null;
					serializationResult.Schema = null;
					serializationResult = null;
					OnNotification("Updated XML For '" + type + "' (" + status + ") FSEID: " + fse.ID + " (" + fseIndex + " of " + fseCount + ")");
				}
				catch (Exception ex)
				{
					OnNotification("Problem updating " + fse.ID + ".", ex: ex);
				}
				fseIndex++;
				CalculateProgress(fseIndex, fseCount);
				GC.Collect();
			}
		}
	}
}