using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace CERS.EDT.Windows.Client
{
	/// <summary>
	/// Interaction logic for RFSQWindow.xaml
	/// </summary>
	public partial class RFSQWindow : WindowBase
	{
		private List<SelectableValue<string>> _SubmittalElementStatuses;
		private List<SelectableValue<int>> _SubmittalElements;

		public RFSQWindow()
		{
			InitializeComponent();
			InitBackgroundWorker();
			RegisterOutputPanel(op);
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void btnInvoke_Click(object sender, RoutedEventArgs e)
		{
			RFSQArguments rfsqArgs = new RFSQArguments
			{
				CERSID = tbCERSID.ToInt32(),
				SubmittedOnStart = dpSubmittedOnStart.SelectedDate,
				SubmittedOnEnd = dpSubmittedOnEnd.SelectedDate,
				SubmittalActionOnStart = dpSubmittalActionOnStart.SelectedDate,
				SubmittalActionOnEnd = dpSubmittalActionOnEnd.SelectedDate
			};

			if (cboSubmittalElementCode.SelectedItem != null)
			{
				SelectableValue<int> item = cboSubmittalElementCode.SelectedItem as SelectableValue<int>;
				if (item != null)
				{
					rfsqArgs.SubmittalElementCode = item.Value;
				}
			}

			rfsqArgs.SubmittalStatus = _SubmittalElementStatuses.GetSelectedValues();

			UpdateControlUsability(false, tbCERSID, dpSubmittalActionOnEnd, dpSubmittalActionOnStart, dpSubmittedOnEnd, dpSubmittedOnStart, cboSubmittalElementCode, lbSubmittalElementStatus, btnInvoke);
			RunInBackground(BackgroundOperationType.Primary, rfsqArgs);
		}

		protected override void DoWork(DoWorkEventArgs e)
		{
			BackgroundOperationArgs<RFSQArguments> args = e.Argument as BackgroundOperationArgs<RFSQArguments>;
			if (args != null)
			{
				StringBuilder endpointUrl = new StringBuilder();
				endpointUrl.Append(Endpoints.Format(Endpoints.Endpoint_RegulatorFacilitySubmittalQuery, App.RegulatorCode));
				endpointUrl.AddQueryString(args.EndpointArguments.CERSID, "CERSID");
				endpointUrl.AddQueryString(args.EndpointArguments.SubmittedOnStart, "submittedOnStart");
				endpointUrl.AddQueryString(args.EndpointArguments.SubmittedOnEnd, "submittedOnEnd");
				endpointUrl.AddQueryString(args.EndpointArguments.SubmittalActionOnStart, "submittalActionOnStart");
				endpointUrl.AddQueryString(args.EndpointArguments.SubmittalActionOnEnd, "submittalActionOnEnd");
				endpointUrl.AddQueryString(args.EndpointArguments.SubmittalStatus, "status");
				endpointUrl.AddQueryString(args.EndpointArguments.SubmittalElementCode, "submittalElement");

				UpdateEndpointUrl(endpointUrl);
				UpdateStatus("Invoking Service...Please Wait...");
				RestClient client = new RestClient(App.AuthorizationHeader);
				var result = client.ExecuteXml(endpointUrl.ToString());

				UpdateOutputPanel(result);
			}
		}

		protected override void RunWorkerCompleted(RunWorkerCompletedEventArgs e)
		{
			UpdateControlUsability(true, tbCERSID, dpSubmittalActionOnEnd, dpSubmittalActionOnStart, dpSubmittedOnEnd, dpSubmittedOnStart, cboSubmittalElementCode, lbSubmittalElementStatus, btnInvoke);
		}

		private void WindowBase_Loaded(object sender, RoutedEventArgs e)
		{
			_SubmittalElementStatuses = App.GetSubmittalElementStatusList();
			lbSubmittalElementStatus.ItemsSource = _SubmittalElementStatuses;

			_SubmittalElements = App.GetSubmittalElementList();
			cboSubmittalElementCode.ItemsSource = _SubmittalElements;
		}

		private void btnDocumentBrowser_Click(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(op.OutputText))
			{
				RFSQAttachmentBrowser rfsqAttachmentBrowserWindow = new RFSQAttachmentBrowser(op.OutputText);
				rfsqAttachmentBrowserWindow.Owner = this;
				rfsqAttachmentBrowserWindow.ShowDialog();
			}
		}
	}
}