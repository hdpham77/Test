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
	/// Interaction logic for CLQWindow.xaml
	/// </summary>
	public partial class CLQWindow : WindowBase
	{
		public CLQWindow()
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
			CLQArguments vlqArgs = new CLQArguments { Identifier = tbIdentifier.Text };
			UpdateControlUsability(false, tbIdentifier, btnInvoke);
			RunInBackground(BackgroundOperationType.Primary, vlqArgs);
		}

		protected override void DoWork(DoWorkEventArgs e)
		{
			BackgroundOperationArgs<CLQArguments> args = e.Argument as BackgroundOperationArgs<CLQArguments>;
			if (args != null)
			{
				//format our endpoint URL
				string endpointUrl = Endpoints.Endpoint_ChemicalLibraryQuery;
				if (!string.IsNullOrWhiteSpace(args.EndpointArguments.Identifier))
				{
					endpointUrl += "/" + args.EndpointArguments.Identifier;
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
			UpdateControlUsability(true, tbIdentifier, btnInvoke);
		}
	}
}