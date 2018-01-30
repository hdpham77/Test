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
	/// Interaction logic for RFCWindow.xaml
	/// </summary>
	public partial class RFCWindow : WindowBase
	{
		public RFCWindow()
		{
			InitializeComponent();
			InitBackgroundWorker();
			RegisterOutputPanel( op );
		}

		protected override void DoWork( DoWorkEventArgs e )
		{
			BackgroundOperationArgs<GenericDataUploadArguments> args = e.Argument as BackgroundOperationArgs<GenericDataUploadArguments>;
			if ( args != null )
			{
				string endpointUrl = Endpoints.Format( Endpoints.Endpoint_RegulatorFacilityCreate, App.RegulatorCode );

				UpdateEndpointUrl( endpointUrl );

				if ( !string.IsNullOrWhiteSpace( args.EndpointArguments.VersionIdentifier ) )
				{
					endpointUrl += "&version=" + args.EndpointArguments.VersionIdentifier;
				}

				UpdateStatus( "Invoking Service...Please Wait..." );

				RestClient client = new RestClient( App.AuthorizationHeader );
				var result = client.ExecuteXml( endpointUrl, args.EndpointArguments.Data );

				UpdateOutputPanel( result );
			}
		}

		protected override void RunWorkerCompleted( RunWorkerCompletedEventArgs e )
		{
			UpdateControlUsability( true, ip, btnInvoke );
		}

		private void btnClose_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}

		private void btnInvoke_Click( object sender, RoutedEventArgs e )
		{
			GenericDataUploadArguments args = new GenericDataUploadArguments();

			if ( ip.InputMode == InputDataMode.BinaryFile )
			{
				args.Data = Utility.ReadBinaryFile( ip.InputFile );
			}
			else if ( ip.InputMode == InputDataMode.Text && !string.IsNullOrWhiteSpace( ip.InputText ) )
			{
				args.Data = Encoding.UTF8.GetBytes( ip.InputText );
			}

			args.VersionIdentifier = ip.VersionIdentifier;

			if ( args.Data == null )
			{
				MessageBox.Show( this, "No information to send was specified", "RFC", MessageBoxButton.OK, MessageBoxImage.Error );
				return;
			}

			UpdateControlUsability( false, ip, btnInvoke );

			RunInBackground( BackgroundOperationType.Primary, args );
		}

		private void ip_Loaded( object sender, RoutedEventArgs e )
		{
		}

		private void WindowBase_Loaded( object sender, RoutedEventArgs e )
		{
		}
	}
}