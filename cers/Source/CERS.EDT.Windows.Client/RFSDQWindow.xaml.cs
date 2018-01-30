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
	/// Interaction logic for RFSDQWindow.xaml
	/// </summary>
	public partial class RFSDQWindow : WindowBase
	{
		public RFSDQWindow()
			: this(null)
		{
		}

		public RFSDQWindow(string CERSUniqueKey)
		{
			InitializeComponent(); InitBackgroundWorker();
			RegisterOutputPanel(op);

			if (!string.IsNullOrWhiteSpace(CERSUniqueKey))
			{
				tbIdentifier.Text = CERSUniqueKey;
			}
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void btnInvoke_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(tbIdentifier.Text))
			{
				MessageBox.Show(this, "You must provide a valid CERSUniqueKey of a document to download.", "RFSDQ", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			RFSDQArguments args = new RFSDQArguments
			{
				Identifier = tbIdentifier.Text
			};

			UpdateControlUsability(false, tbIdentifier, btnInvoke);
			RunInBackground(BackgroundOperationType.Primary, args);
		}

		protected override void DoWork(DoWorkEventArgs e)
		{
			BackgroundOperationArgs<RFSDQArguments> args = e.Argument as BackgroundOperationArgs<RFSDQArguments>;
			if (args != null)
			{
				string endpointUrl = Endpoints.Endpoint_RegulatorFacilitySubmittalDocumentQuery.Replace("{CERSUniqueKey}", args.EndpointArguments.Identifier);

				UpdateEndpointUrl(endpointUrl);
				UpdateStatus("Invoking Service...Please Wait...");

				RestClient client = new RestClient(App.AuthorizationHeader);
				var result = client.Execute(endpointUrl);

				UpdateOutputPanel(result);

				if (result.Status == System.Net.HttpStatusCode.OK)
				{
					Action<RestClientResult> saveFileResponseHandler = arg =>
					{
						Utility.SaveFile(this, arg.RawData, "Save Binary File", "PDF|*.pdf|XML Files|*.xml");
					};

					Dispatcher.Invoke(saveFileResponseHandler, result);
				}
			}
		}

		protected override void RunWorkerCompleted(RunWorkerCompletedEventArgs e)
		{
			UpdateControlUsability(true, tbIdentifier, btnInvoke);
		}

		private void WindowBase_Loaded(object sender, RoutedEventArgs e)
		{
		}
	}
}