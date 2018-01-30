using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CERS.EDT.Windows.Client
{
	/// <summary>
	/// Interaction logic for RFSQAttachmentBrowser.xaml
	/// </summary>
	public partial class RFSQAttachmentBrowser : WindowBase
	{
		public string RawXml { get; set; }

		public List<Attachment> Attachments { get; set; }

		public RFSQAttachmentBrowser(string rawXml)
		{
			InitializeComponent();
			Attachments = new List<Attachment>();
			RawXml = rawXml;
		}

		private void WindowBase_Loaded(object sender, RoutedEventArgs e)
		{
			var doc = XDocument.Parse(RawXml);
			if (doc != null)
			{
				XElement root = doc.Root;
				Attachment attachment = null;
				try
				{
					var facilitySubmittalsXml = root.XPathSelectElements("FacilitySubmittal");

					foreach (var facilitySubmittalXml in facilitySubmittalsXml)
					{
						var attachmentsXml = facilitySubmittalXml.XPathSelectElements("//Attachment");
						try
						{
							Debug.WriteLine(attachmentsXml.ToString());

							foreach (var attachmentXml in attachmentsXml)
							{
								attachment = new Attachment();
								attachment.CERSID = int.Parse(facilitySubmittalXml.Element("CERSID").Value);
								attachment.FileName = attachmentXml.Element("AttachmentFileName").Value;
								attachment.Title = attachmentXml.Element("DocumentTitle").Value;
								attachment.Description = attachmentXml.Element("DocumentDescription").Value;
								attachment.DocumentRegulatorKey = attachmentXml.Element("DocumentRegulatorKey").Value;
								if (attachmentXml.Element("DateAuthored") != null)
								{
									attachment.DateAuthored = DateTime.Parse(attachmentXml.Element("DateAuthored").Value);
								}
								attachment.CERSUniqueKey = attachmentXml.Element("CERSUniqueKey").Value;
								Attachments.Add(attachment);
							}
						}
						catch (Exception ex)
						{
							MessageBox.Show("Error: " + ex.Message);
						}
					}

					lvAttachments.ItemsSource = Attachments;
				}
				catch (Exception oex)
				{
					MessageBox.Show("Error: " + oex.Message);
				}
			}
		}

		public class Attachment
		{
			public int CERSID { get; set; }

			public string FileName { get; set; }

			public string Title { get; set; }

			public string Description { get; set; }

			public DateTime DateAuthored { get; set; }

			public string DocumentRegulatorKey { get; set; }

			public string CERSUniqueKey { get; set; }

			public string Status { get; set; }
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void lvAttachments_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (lvAttachments.SelectedItem != null)
			{
				Attachment attach = lvAttachments.SelectedItem as Attachment;
				if (attach != null)
				{
					RFSDQWindow window = new RFSDQWindow(attach.CERSUniqueKey);
					window.Owner = this;
					window.ShowDialog();
				}
			}
		}

		private void btnDownloadAll_Click(object sender, RoutedEventArgs e)
		{
			string targetPath = Utility.SelectFolder(@"C:\temp");

			DownloadAllArgs args = new DownloadAllArgs
			{
				TargetPath = targetPath
			};

			btnDownloadAll.IsEnabled = false;
			lvAttachments.IsEnabled = false;

			Worker.RunWorkerAsync(args);
		}

		protected override void DoWork(DoWorkEventArgs e)
		{
			DownloadAllArgs args = e.Argument as DownloadAllArgs;
			if (args != null)
			{
				RestClient client = new RestClient(App.AuthorizationHeader);

				Action<string> updaterMethod = p =>
				{
					tbBatchStatus.Text = p;
				};

				int docIndex = 0;
				int failCount = 0;
				int okCount = 0;
				foreach (var attachment in Attachments)
				{
					docIndex++;
					string endpointUrl = Endpoints.Endpoint_RegulatorFacilitySubmittalDocumentQuery.Replace("{CERSUniqueKey}", attachment.CERSUniqueKey);
					Dispatcher.Invoke(updaterMethod, "Download " + docIndex + " of " + Attachments.Count + " Document(s): " + attachment.CERSUniqueKey + "...");
					var result = client.Execute(endpointUrl);

					if (result.Status == System.Net.HttpStatusCode.OK)
					{
						string targetFileName = attachment.CERSID + " - " + attachment.FileName;
						targetFileName = args.TargetPath + "\\" + targetFileName;
						try
						{
							result.RawData.WriteToFile(targetFileName);
							Dispatcher.Invoke(updaterMethod, "Document: " + attachment.CERSUniqueKey + " Saved!");
							okCount++;
						}
						catch (Exception ex)
						{
							attachment.Status = "Error: " + ex.Message;
							Dispatcher.Invoke(updaterMethod, "Document: " + attachment.CERSUniqueKey + " Not Saved! " + ex.Message);
							failCount++;
						}
					}
					else
					{
						attachment.Status = result.Status + " - " + result.StatusDescription;
						Dispatcher.Invoke(updaterMethod, "Document: " + attachment.CERSUniqueKey + " - " + result.Status + " - " + result.StatusDescription);
						failCount++;
					}
				}

				Action statusUpdater = () =>
				{
					tbBatchStatus.Text = okCount + " out of " + Attachments.Count + " downloaded successfully";
				};

				Dispatcher.Invoke(statusUpdater, null);
			}
		}

		protected override void RunWorkerCompleted(RunWorkerCompletedEventArgs e)
		{
			btnDownloadAll.IsEnabled = true;
			lvAttachments.IsEnabled = true;
		}

		public class DownloadAllArgs
		{
			public string TargetPath { get; set; }
		}

		private void btnExport_Click(object sender, RoutedEventArgs e)
		{
			if (Attachments != null)
			{
				StringBuilder result = new StringBuilder();
				foreach (var attachment in Attachments)
				{
					result.AppendLine(attachment.CERSUniqueKey);
				}

				byte[] data = Encoding.UTF8.GetBytes(result.ToString());
				Utility.SaveFile(this, data, "Save Text File", "TXT Files|*.txt", true);
			}
		}
	}
}