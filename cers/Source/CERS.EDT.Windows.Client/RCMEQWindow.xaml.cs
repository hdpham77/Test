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
	/// Interaction logic for RCMEQWIndow.xaml
	/// </summary>
	public partial class RCMEQWindow : WindowBase
	{
		private List<SelectableValue<string>> _Statuses;

		public RCMEQWindow()
		{
			InitializeComponent();
			InitBackgroundWorker();
			RegisterOutputPanel(op);

			_Statuses = new List<SelectableValue<string>>();
			_Statuses.Add("Approved", "2");
			_Statuses.Add("Deleted (Supressed by Default)", "3");
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void btnInvoke_Click(object sender, RoutedEventArgs e)
		{
			RCMEQArguments rcmeqArgs = new RCMEQArguments
			{
				CERSID = tbCERSID.ToInt32(),
				StartDate = dpStartDate.SelectedDate,
				EndDate = dpEndDate.SelectedDate,
			};

			rcmeqArgs.Status = _Statuses.GetSelectedValues();

			UpdateControlUsability(false, tbCERSID, dpStartDate, dpEndDate, lbStatuses, btnInvoke);
			RunInBackground(BackgroundOperationType.Primary, rcmeqArgs);
		}

		protected override void DoWork(DoWorkEventArgs e)
		{
			BackgroundOperationArgs<RCMEQArguments> args = e.Argument as BackgroundOperationArgs<RCMEQArguments>;
			if (args != null)
			{
				StringBuilder endpointUrl = new StringBuilder();
				endpointUrl.Append(Endpoints.Format(Endpoints.Endpoint_RegulatorCMESubmittalQuery, App.RegulatorCode));
				endpointUrl.AddQueryString(args.EndpointArguments.CERSID, "CERSID");
				endpointUrl.AddQueryString(args.EndpointArguments.StartDate, "startDate");
				endpointUrl.AddQueryString(args.EndpointArguments.EndDate, "endDate");
				endpointUrl.AddQueryString(args.EndpointArguments.Status, "status");

				UpdateEndpointUrl(endpointUrl);
				UpdateStatus("Invoking Service...Please Wait...");
				RestClient client = new RestClient(App.AuthorizationHeader);
				var result = client.ExecuteXml(endpointUrl.ToString());

				UpdateOutputPanel(result);
			}
		}

		protected override void RunWorkerCompleted(RunWorkerCompletedEventArgs e)
		{
			UpdateControlUsability(true, tbCERSID, tbCERSID, dpStartDate, dpEndDate, lbStatuses, btnInvoke);
		}

		private void WindowBase_Loaded(object sender, RoutedEventArgs e)
		{
			lbStatuses.ItemsSource = _Statuses;
		}
	}
}