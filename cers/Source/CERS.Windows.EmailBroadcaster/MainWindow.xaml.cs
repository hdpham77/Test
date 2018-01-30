using CERS.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UPF;

namespace CERS.Windows.EmailBroadcaster
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : WindowBase
	{
		public MainWindow()
		{
			InitializeComponent();
			InitDefaultWorker( true );
		}

		protected override void DoWork( DoWorkEventArgs e )
		{
			WorkerArgs args = e.Argument as WorkerArgs;
			if ( args != null )
			{
				var dataModel = Repository.DataModel;

				List<Recipient> recipients = new List<Recipient>();
				if ( args.AllCERSBizLeadUsersForBizWithAtLeastFiveFacilities )
				{
					recipients.AddRange( ( from q in dataModel.OrganizationContactRoleWithFacilityCounts
										   where q.FacilityCount >= 5 && q.RoleName == "OrgAdmin"
										   select new Recipient
										   {
											   Email = q.Email,
											   ContactID = q.ContactID,
											   FullName = q.FullName
										   } ).ToList().Distinct( new RecipientEmailEqualityComparer() )

										   );
				}
				else
				{
					recipients.AddRange( ( from q in dataModel.OrganizationContactRoleWithFacilityCounts
										   where q.RoleName == "OrgAdmin"
										   select new Recipient
										   {
											   Email = q.Email,
											   ContactID = q.ContactID,
											   FullName = q.FullName
										   } ).ToList().Distinct( new RecipientEmailEqualityComparer() )
										   );
				}

				if ( !string.IsNullOrWhiteSpace( args.ToCustomAddresses ) )
				{
					string[] customRecipients = tbToCustomAddressList.Text.Split( new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries );

					if ( customRecipients.Length > 0 )
					{
						recipients.AddRange( ( from c in customRecipients
											   select new Recipient
											   {
												   Email = c
											   } ).Distinct( new RecipientEmailEqualityComparer() )
											   );
					}
				}

				//make sure we really are dealing with unique email addresses
				recipients = recipients.Distinct( new RecipientEmailEqualityComparer() ).ToList();

				bool debugMode = args.DebugMode;

				NotifyMessageFromThread( "Found " + recipients.Count + " recipients to notify." );
				int total = recipients.Count;
				int index = 0;
				string targetRecipientEmail = string.Empty;
				foreach ( var recipient in recipients )
				{
					index++;
					targetRecipientEmail = recipient.Email;
					if ( debugMode )
					{
						targetRecipientEmail = "mreagan@calepa.ca.gov";
					}
					NotifyMessageFromThread( "Sending " + index + " of " + total + ": " + recipient.Email + ( debugMode ? "(debug: " + targetRecipientEmail + ") " : "" ) );
					Services.Emails.Send( targetRecipientEmail, args.Subject, args.Body, args.Priority, args.HTMLEmail, contactID: recipient.ContactID );
					CalculateAndReportWorkerProgress( index, total );
				}
			}
		}

		protected override void NotifyMessage<T>( string message, T obj )
		{
			tbStatus.Text = message;
		}

		protected override void NotifyMessage( string message )
		{
			tbStatus.Text = message;
		}

		protected override void ProgressChanged( ProgressChangedEventArgs e )
		{
			pbStatus.Value = e.ProgressPercentage;
		}

		protected override void RunWorkerCompleted( RunWorkerCompletedEventArgs e )
		{
			UpdateControlUsability( true, rbAllOrgLeadUsersForBizWithAtLeast5Facilities,
					rbAllOrgLeadUsers,
					cboEmailPriority,
					cbHtmlEmail,
					btnSend,
					tbToCustomAddressList,
					tbEmailSubject,
					tbEmailBody
					);
		}

		private void btnClose_Click( object sender, RoutedEventArgs e )
		{
			App.Current.Shutdown();
		}

		private void btnSend_Click( object sender, RoutedEventArgs e )
		{
			//verify we have enough information to actually do anything.
			if ( InputsValid() )
			{
				EmailPriority priority = EmailPriority.Normal;
				var prioritySelection = cboEmailPriority.SelectedItem as ISystemLookupEntity;
				priority = (EmailPriority) prioritySelection.ID;
				WorkerArgs args = new WorkerArgs
				{
					AllCERSBizLeadUsers = rbAllOrgLeadUsers.IsChecked.Value,
					AllCERSBizLeadUsersForBizWithAtLeastFiveFacilities = rbAllOrgLeadUsersForBizWithAtLeast5Facilities.IsChecked.Value,
					Body = tbEmailBody.Text,
					HTMLEmail = cbHtmlEmail.IsChecked.Value,
					Priority = priority,
					Subject = tbEmailSubject.Text,
					ToCustomAddresses = tbToCustomAddressList.Text,
					DebugMode = cbDebugMode.IsChecked.Value
				};

				UpdateControlUsability( false, rbAllOrgLeadUsersForBizWithAtLeast5Facilities,
					rbAllOrgLeadUsers,
					cboEmailPriority,
					cbHtmlEmail,
					btnSend,
					tbToCustomAddressList,
					tbEmailSubject,
					tbEmailBody
					);

				RunInBackground( args );
			}
			else
			{
				MessageBox.Show( this, "You must provide enough information", "CERS Email Broadcaster", MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}

		private bool InputsValid()
		{
			bool result = true;

			if ( !rbAllOrgLeadUsers.IsChecked.Value && !rbAllOrgLeadUsersForBizWithAtLeast5Facilities.IsChecked.Value && string.IsNullOrWhiteSpace( tbToCustomAddressList.Text ) )
			{
				result = false;
			}

			if ( string.IsNullOrWhiteSpace( tbEmailSubject.Text ) )
			{
				result = false;
			}

			if ( string.IsNullOrWhiteSpace( tbEmailBody.Text ) )
			{
				result = false;
			}

			return result;
		}

		private void LoadData()
		{
			cboEmailPriority.ItemsSource = Repository.SystemLookupTables.GetValues( SystemLookupTable.EmailPriority );
			cboEmailPriority.DisplayMemberPath = "Name";
			cboEmailPriority.SelectedValuePath = "ID";
			cboEmailPriority.SelectedValue = "2";
		}

		private void WindowBase_Loaded( object sender, RoutedEventArgs e )
		{
			this.Title = this.AssemblyProductName;
			var currentEnvironment = ( EnvironmentProfileManager.Current );
			this.Title += " (" + currentEnvironment.Environment.ToString();
#if DEBUG
			this.Title += "/DEBUG";
#elif RELEASE
			this.Title += "/RELEASE";
#endif
			this.Title += ")";
			LoadData();

			CERSConfigurationSection cersConfig = CERSConfigurationSection.Current;
			tbConfigEmailDelivery.Text = cersConfig.Email.DeliveryMode.ToString();
			tbConfigEnvironmentProfile.Text = EnvironmentProfileManager.Current.Environment.ToString();
			tbConfigCERSConnectionString.Text = ConfigurationManager.ConnectionStrings["CERSEntities"].ConnectionString;
		}

		public class Recipient
		{
			public int? ContactID { get; set; }

			public string Email { get; set; }

			public string FullName { get; set; }
		}

		public class RecipientEmailEqualityComparer : IEqualityComparer<Recipient>
		{
			public bool Equals( Recipient x, Recipient y )
			{
				return x.Email.ToLower().Trim() == y.Email.ToLower().Trim();
			}

			public int GetHashCode( Recipient obj )
			{
				return obj.Email.GetHashCode();
			}
		}

		public class WorkerArgs
		{
			public bool AllCERSBizLeadUsers { get; set; }

			public bool AllCERSBizLeadUsersForBizWithAtLeastFiveFacilities { get; set; }

			public string Body { get; set; }

			public bool DebugMode { get; set; }

			public bool HTMLEmail { get; set; }

			public EmailPriority Priority { get; set; }

			public string Subject { get; set; }

			public string ToCustomAddresses { get; set; }
		}
	}
}