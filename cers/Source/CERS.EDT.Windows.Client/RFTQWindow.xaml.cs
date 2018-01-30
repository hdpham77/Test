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
	/// Interaction logic for RFTQWindow.xaml
	/// </summary>
	public partial class RFTQWindow : WindowBase
	{
		public RFTQWindow()
		{
			InitializeComponent(); InitBackgroundWorker();
			RegisterOutputPanel( op );
		}

		protected override void DoWork( DoWorkEventArgs e )
		{
			BackgroundOperationArgs<RFTQArguments> args = e.Argument as BackgroundOperationArgs<RFTQArguments>;
			if ( args != null )
			{
				StringBuilder endpointUrl = new StringBuilder();
				endpointUrl.Append( Endpoints.Format( Endpoints.Endpoint_RegulatorFacilityTransferQuery, App.RegulatorCode ) );
				endpointUrl.AddQueryString( args.EndpointArguments.CERSID, "cersid" );
				endpointUrl.AddQueryString( args.EndpointArguments.OccurredOnStart, "occurredOnStart" );
				endpointUrl.AddQueryString( args.EndpointArguments.OccurredOnEnd, "occurredOnEnd" );
				endpointUrl.AddQueryString( args.EndpointArguments.AssumedOwnershipOnStart, "assumedOwershipOnStart" );
				endpointUrl.AddQueryString( args.EndpointArguments.AssumedOwnershipOnEnd, "assumedOwnershipOnEnd" );
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
			UpdateControlUsability( true, tbCERSID, tbOrganizationCode, dpOccurredOnStart, dpOccurredOnEnd, dpAssumedOwnershipOnStart, dpAssumedOwnershipOnEnd, btnInvoke );
		}

		private void btnClose_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}

		private void btnInvoke_Click( object sender, RoutedEventArgs e )
		{
			RFTQArguments rfsqArgs = new RFTQArguments
			{
				CERSID = tbCERSID.ToInt32(),
				OccurredOnStart = dpOccurredOnStart.SelectedDate,
				OccurredOnEnd = dpOccurredOnEnd.SelectedDate,
				AssumedOwnershipOnStart = dpAssumedOwnershipOnStart.SelectedDate,
				AssumedOwnershipOnEnd = dpAssumedOwnershipOnEnd.SelectedDate,
				OrganizationCode = tbOrganizationCode.ToInt32()
			};

			UpdateControlUsability( false, tbCERSID, tbOrganizationCode, dpOccurredOnStart, dpOccurredOnEnd, dpAssumedOwnershipOnStart, dpAssumedOwnershipOnEnd, btnInvoke );
			RunInBackground( BackgroundOperationType.Primary, rfsqArgs );
		}

		private void WindowBase_Loaded( object sender, RoutedEventArgs e )
		{
		}
	}
}