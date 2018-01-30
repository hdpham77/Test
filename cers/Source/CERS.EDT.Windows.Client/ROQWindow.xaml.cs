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
	/// Interaction logic for ROQWindow.xaml
	/// </summary>
	public partial class ROQWindow : WindowBase
	{
		public ROQWindow()
		{
			InitializeComponent(); InitBackgroundWorker();
			RegisterOutputPanel( op );
		}

		protected override void DoWork( DoWorkEventArgs e )
		{
			BackgroundOperationArgs<ROQArguments> args = e.Argument as BackgroundOperationArgs<ROQArguments>;
			if ( args != null )
			{
				StringBuilder endpointUrl = new StringBuilder();
				endpointUrl.Append( Endpoints.Format( Endpoints.Endpoint_RegulatorOrganizationQuery, App.RegulatorCode ) );
				endpointUrl.AddQueryString( args.EndpointArguments.CERSID, "CERSID" );
				endpointUrl.AddQueryString( args.EndpointArguments.EstablishedSince, "establishedSince" );
				endpointUrl.AddQueryString( args.EndpointArguments.OrganizationCode, "organizationCode" );
				endpointUrl.AddQueryString( args.EndpointArguments.OrganizationName, "organizationName" );
				endpointUrl.AddQueryString( args.EndpointArguments.OrganizationHeadquarters, "organizationHeadquarters" );
				endpointUrl.AddQueryString( args.EndpointArguments.IncludeFacilities, "includeFacilities" );

				UpdateEndpointUrl( endpointUrl );
				UpdateStatus( "Invoking Service...Please Wait..." );
				RestClient client = new RestClient( App.AuthorizationHeader );
				var result = client.ExecuteXml( endpointUrl.ToString() );

				UpdateOutputPanel( result );
			}
		}

		protected override void RunWorkerCompleted( RunWorkerCompletedEventArgs e )
		{
			UpdateControlUsability( true, tbCERSID, tbOrganizationCode, tbOrganizationHeadquarters, tbOrganizationName, cbIncludeFacilities, btnInvoke );
		}

		private void btnClose_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}

		private void btnInvoke_Click( object sender, RoutedEventArgs e )
		{
			ROQArguments rfsqArgs = new ROQArguments
			{
				CERSID = tbCERSID.ToInt32(),
				OrganizationCode = tbOrganizationCode.ToInt32(),
				EstablishedSince = dpEstablishedSince.SelectedDate,
				IncludeFacilities = cbIncludeFacilities.IsChecked.Value,
				OrganizationHeadquarters = tbOrganizationHeadquarters.Text.Trim(),
				OrganizationName = tbOrganizationName.Text.Trim()
			};

			UpdateControlUsability( false, tbCERSID, tbOrganizationCode, tbOrganizationHeadquarters, tbOrganizationName, cbIncludeFacilities, btnInvoke );
			RunInBackground( BackgroundOperationType.Primary, rfsqArgs );
		}

		private void WindowBase_Loaded( object sender, RoutedEventArgs e )
		{
		}
	}
}