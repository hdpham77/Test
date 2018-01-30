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
	/// Interaction logic for RAIQWindow.xaml
	/// </summary>
	public partial class RAIQWindow : WindowBase
	{
		private List<SelectableValue<int>> _ActionItemTypeList;

		public RAIQWindow()
		{
			InitializeComponent();
			InitBackgroundWorker();
			RegisterOutputPanel( op );
		}

		public void btnClose_Click( object sender, RoutedEventArgs e )
		{
			// TODO: Implement this method
			this.DialogResult = false;
		}

		public void btnInvoke_Click( object sender, RoutedEventArgs e )
		{
			int? typeID = null;
			if ( cboType.SelectedItem != null )
			{
				var item = (SelectableValue<int>) cboType.SelectedItem;
				if ( item != null )
				{
					typeID = item.Value;
				}
			}

			RAIQArguments args = new RAIQArguments
			{
				CERSID = tbCERSID.ToInt32(),
				OrganizationCode = tbOrganizationCode.ToInt32(),
				RequestedOnStart = dpRequestedOnStart.SelectedDate,
				RequestedOnEnd = dpRequestedOnEnd.SelectedDate,
				Completed = cbCompleted.IsChecked.Value,
				TypeID = typeID
			};

			UpdateControlUsability( false, tbCERSID, tbOrganizationCode, dpRequestedOnStart, dpRequestedOnEnd, cbCompleted, cboType, btnInvoke );
			RunInBackground( BackgroundOperationType.Primary, args );
		}

		public void WindowBase_Loaded( object sender, RoutedEventArgs e )
		{
			_ActionItemTypeList = App.GetActionItemTypeList();
			cboType.ItemsSource = _ActionItemTypeList;
		}

		protected override void DoWork( DoWorkEventArgs e )
		{
			BackgroundOperationArgs<RAIQArguments> args = e.Argument as BackgroundOperationArgs<RAIQArguments>;
			if ( args != null )
			{
				StringBuilder endpointUrl = new StringBuilder();
				endpointUrl.Append( Endpoints.Format( Endpoints.Endpoint_RegulatorActionItemQuery, App.RegulatorCode ) );
				endpointUrl.AddQueryString( args.EndpointArguments.CERSID, "cersid" );
				endpointUrl.AddQueryString( args.EndpointArguments.RequestedOnStart, "requestedOnStart" );
				endpointUrl.AddQueryString( args.EndpointArguments.RequestedOnEnd, "requestedOnEnd" );
				endpointUrl.AddQueryString( args.EndpointArguments.Completed, "completed" );
				endpointUrl.AddQueryString( args.EndpointArguments.TypeID, "typeID" );
				endpointUrl.AddQueryString( args.EndpointArguments.OrganizationCode, "organizationCode" );

				UpdateEndpointUrl( endpointUrl );
				UpdateStatus( "Invoking Service...Please Wait..." );
				RestClient client = new RestClient( App.AuthorizationHeader );
				var result = client.ExecuteXml( endpointUrl.ToString() );

				UpdateOutputPanel( result );
			}
		}

		protected override void RunWorkerCompleted( RunWorkerCompletedEventArgs e )
		{
			UpdateControlUsability( true, tbCERSID, tbOrganizationCode, dpRequestedOnStart, dpRequestedOnEnd, cbCompleted, cboType, btnInvoke );
		}
	}
}