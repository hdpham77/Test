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
	/// Interaction logic for VLQWindow.xaml
	/// </summary>
	public partial class VLQWindow : WindowBase
	{
		public VLQWindow()
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
			VLQArguments vlqArgs = new VLQArguments { ViolationTypeNumber = tbViolationTypeNumber.Text };
			UpdateControlUsability(false, tbViolationTypeNumber, btnInvoke);
			RunInBackground(BackgroundOperationType.Primary, vlqArgs);
		}

		protected override void DoWork(DoWorkEventArgs e)
		{
			BackgroundOperationArgs<VLQArguments> args = e.Argument as BackgroundOperationArgs<VLQArguments>;
			if (args != null)
			{
				//format our endpoint URL
				string endpointUrl = Endpoints.Endpoint_ViolationLibraryQuery;
				if (!string.IsNullOrWhiteSpace(args.EndpointArguments.ViolationTypeNumber))
				{
					endpointUrl += "/" + args.EndpointArguments.ViolationTypeNumber;
				}

				UpdateEndpointUrl(endpointUrl);

				UpdateStatus("Invoking Service...Please Wait...");

				//invoke the REST call to the endpoint.
				RestClient client = new RestClient(App.AuthorizationHeader);
				var result = client.ExecuteXml(endpointUrl);

				UpdateOutputPanel(result);
			}
		}

		protected override void RunWorkerCompleted(RunWorkerCompletedEventArgs e)
		{
			UpdateControlUsability(true, tbViolationTypeNumber, btnInvoke);
		}
	}
}