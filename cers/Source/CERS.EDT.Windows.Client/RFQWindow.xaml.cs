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
	/// Interaction logic for RFQWindow.xaml
	/// </summary>
	public partial class RFQWindow : WindowBase
	{
		public RFQWindow()
		{
			InitializeComponent();
			InitBackgroundWorker();
			RegisterOutputPanel( op );
		}

		protected override void DoWork( DoWorkEventArgs e )
		{
			BackgroundOperationArgs<RFQArguments> args = e.Argument as BackgroundOperationArgs<RFQArguments>;
			if ( args != null )
			{
				StringBuilder endpointUrl = new StringBuilder();
				endpointUrl.Append( Endpoints.Format( Endpoints.Endpoint_RegulatorFacilityQuery, App.RegulatorCode ) );
				endpointUrl.AddQueryString( args.EndpointArguments.CERSID, "cersid" );
				endpointUrl.AddQueryString( args.EndpointArguments.CreatedOnStart, "createdOnStart" );
				endpointUrl.AddQueryString( args.EndpointArguments.CreatedOnEnd, "createdOnEnd" );
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
			UpdateControlUsability( true, tbCERSID, tbOrganizationCode, dpCreatedOnStart, dpCreatedOnEnd, btnInvoke );
		}

		private void btnClose_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}

		private void btnInvoke_Click( object sender, RoutedEventArgs e )
		{
			RFQArguments rfsqArgs = new RFQArguments
			{
				CERSID = tbCERSID.ToInt32(),
				CreatedOnStart = dpCreatedOnStart.SelectedDate,
				CreatedOnEnd = dpCreatedOnEnd.SelectedDate,
				OrganizationCode = tbOrganizationCode.ToInt32()
			};

			UpdateControlUsability( false, tbCERSID, tbOrganizationCode, dpCreatedOnStart, dpCreatedOnEnd, btnInvoke );
			RunInBackground( BackgroundOperationType.Primary, rfsqArgs );
		}

		private void WindowBase_Loaded( object sender, RoutedEventArgs e )
		{
		}
	}
}