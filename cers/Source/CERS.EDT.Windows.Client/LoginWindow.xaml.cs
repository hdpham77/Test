using System;
using System.ComponentModel;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace CERS.EDT.Windows.Client
{
	/// <summary>
	/// Interaction logic for LoginWindow.xaml
	/// </summary>
	public partial class LoginWindow : WindowBase
	{
		private string _PromptText;

		public LoginWindow()
		{
			InitializeComponent();
			InitBackgroundWorker();
			Title = AssemblyMetadata.Title + ": Sign-In";
		}

		protected override void DoWork( DoWorkEventArgs e )
		{
			BackgroundOperationArgs<int> args = e.Argument as BackgroundOperationArgs<int>;
			if ( args != null )
			{
				if ( args.Type == BackgroundOperationType.Primary )
				{
					RestClient client = new RestClient( App.AuthorizationHeader );
					var restClientResult = client.Execute( Endpoints.Format( Endpoints.Endpoint_RegulatorAuthenticationTest, args.EndpointArguments ), HttpMethod.Get );
					e.Result = new BackgroundOperationResult<RestClientResult>( true, args.Type, restClientResult, "Sign In Successfull!" );
				}
				else if ( args.Type == BackgroundOperationType.Secondary )
				{
					App.LoadRegulators();
					e.Result = new BackgroundOperationResult<RestClientResult>( true, args.Type, null, "Regulators Loaded Successfully" );
				}
			}
		}

		protected override void RunWorkerCompleted( RunWorkerCompletedEventArgs e )
		{
			BackgroundOperationResult<RestClientResult> result = e.Result as BackgroundOperationResult<RestClientResult>;
			if ( result != null )
			{
				if ( result.Success && result.Type == BackgroundOperationType.Primary )
				{
					if ( result.Data.Status == System.Net.HttpStatusCode.OK )
					{
						this.Hide();
						MainWindow window = new MainWindow();
						window.Owner = this;
						window.Show();
					}
					else
					{
						UpdateControlUsability( true, btnOK, tbUsername, tbPassword, cboRegulators );
						lblPrompt.Content = "Authentication Failed";
					}
				}
				else if ( result.Success && result.Type == BackgroundOperationType.Secondary )
				{
					cboRegulators.ItemsSource = App.Regulators;
					cboRegulators.SelectedIndex = 0;

					UpdateControlUsability( true, btnOK, tbUsername, tbPassword, cboRegulators );
					lblPrompt.Content = _PromptText;
				}
			}
		}

		private void btnCancel_Click( object sender, RoutedEventArgs e )
		{
			App.Current.Shutdown();
		}

		private void btnOK_Click( object sender, RoutedEventArgs e )
		{
			bool isValid = IsNotEmpty( tbUsername );
			isValid &= IsNotEmpty( tbPassword );

			if ( isValid )
			{
				//hash the password.
				SHA1 sha = SHA1.Create();
				string hashedPassword = Convert.ToBase64String( sha.ComputeHash( Encoding.UTF8.GetBytes( tbPassword.Password ) ) );

				//make header.
				App.AuthorizationHeader = "user " + tbUsername.Text + ":" + hashedPassword;
				App.Username = tbUsername.Text;

				Regulator regulator = ( cboRegulators.SelectedItem as Regulator );
				App.RegulatorName = regulator.Display;
				App.RegulatorCode = regulator.Code;

				UpdateControlUsability( false, btnOK, tbUsername, tbPassword, cboRegulators );
				RunInBackground( new BackgroundOperationArgs<int>( regulator.Code, BackgroundOperationType.Primary ) );
			}
		}

		private void WindowBase_Loaded( object sender, RoutedEventArgs e )
		{
			tbServer.Text = Endpoints.ServerBase;
			string regulatorCode = ConfigurationManager.AppSettings["RegulatorCode"];
			string username = ConfigurationManager.AppSettings["Username"];
			string password = ConfigurationManager.AppSettings["Password"];
			if ( !string.IsNullOrWhiteSpace( regulatorCode ) && !string.IsNullOrWhiteSpace( username ) && !string.IsNullOrWhiteSpace( password ) )
			{
				lblPrompt.Content = "Attempting automatic sign-in from configuration, Please wait...";

				//hash the password.
				SHA1 sha = SHA1.Create();
				string hashedPassword = Convert.ToBase64String( sha.ComputeHash( Encoding.UTF8.GetBytes( password ) ) );

				//make header.
				App.AuthorizationHeader = "user " + username + ":" + hashedPassword;
				App.Username = username;
				App.RegulatorCode = int.Parse( regulatorCode );

				App.RegulatorName = regulatorCode;
				UpdateControlUsability( false, btnOK, tbUsername, tbPassword, cboRegulators );
				RunInBackground( new BackgroundOperationArgs<int>( int.Parse( regulatorCode ), BackgroundOperationType.Primary ) );
			}
			else
			{
				_PromptText = lblPrompt.Content.ToString();
				UpdateControlUsability( false, btnOK, tbUsername, tbPassword, cboRegulators );
				lblPrompt.Content = "Initializing, Please wait...";
				RunInBackground( new BackgroundOperationArgs<int>( 0, BackgroundOperationType.Secondary ) );
			}
		}
	}
}