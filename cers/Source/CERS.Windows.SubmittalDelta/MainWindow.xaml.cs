using CERS;
using CERS.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
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
using UPF.Core;
using UPF.Core.Cryptography;
using UPF.Core.Model;

namespace CERS.Windows.SubmittalDelta
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ICoreRepositoryManager _CoreDataRepository;
		private ICERSRepositoryManager _DataRepository;
		private ICERSRepositoryManager _Repository;

		public MainWindow()
		{
			InitializeComponent();
			CurrentAccountID = UPF.Core.Constants.DefaultAccountID;
		}

		public ICoreRepositoryManager CoreDataRepository
		{
			get
			{
				if ( _CoreDataRepository == null )
				{
					_CoreDataRepository = CoreServiceLocator.GetRepositoryManager( CurrentAccountID );
				}
				return _CoreDataRepository;
			}
		}

		public int CurrentAccountID
		{
			get;
			set;
		}

		public ICERSRepositoryManager DataRepository
		{
			get
			{
				if ( _DataRepository == null )
				{
					_DataRepository = ServiceLocator.GetRepositoryManager( CurrentAccountID );
				}
				return _DataRepository;
			}
		}

		public ICERSRepositoryManager Repository
		{
			get
			{
				if ( _Repository == null )
				{
					_Repository = ServiceLocator.GetRepositoryManager();
				}
				return _Repository;
			}
		}

		private IAccount MyAccount { get; set; }

		public static AuthenticationResult Authenticate( string userName, string password, out Account account, bool passwordIsHashed = false )
		{
			if ( string.IsNullOrWhiteSpace( userName ) )
			{
				throw new ArgumentNullException( "userName" );
			}

			if ( string.IsNullOrWhiteSpace( password ) )
			{
				throw new ArgumentNullException( "password" );
			}

			int authenticationAttemptID = 0;
			AuthenticationStatus result = AuthenticationStatus.Failure_Unknown;

			using ( ICoreRepositoryManager repository = CoreServiceLocator.GetRepositoryManager() )
			{
				CoreEntities ctx = repository.DataModel;
				account = null;

				//lets patch up the username and remove any leading or trailing spaces.
				userName = userName.Trim();

				//lets look and see if the userName is an email address.
				account = ctx.Accounts.SingleOrDefault( p => p.UserName == userName && !p.Voided );

				if ( account != null )
				{
					//check account approved/disabled
					result = account.Approved ? AuthenticationStatus.Success : AuthenticationStatus.Failure_AccountNotApproved;
					result = account.Disabled ? AuthenticationStatus.Failure_AccountDisabled : AuthenticationStatus.Success;
					result = account.Password == null ? AuthenticationStatus.Failure_NoPassword : AuthenticationStatus.Success;

					//if so far we are approved and not disabled, and password is not null, then lets check the password against the input.
					if ( result == AuthenticationStatus.Success && !VerifyPasswordMatch( password, account.Password, passwordIsHashed ) )
					{
						result = AuthenticationStatus.Failure_IncorrectPassword;
					}
				}
				else
				{
					result = AuthenticationStatus.Failure_AccountNotExist;
				}

				if ( account != null )
				{
					//detach the contact so we don't have to worry about duplicates being committed into the database since this context is going out of scope *very* soon.
					ctx.Detach( account );
				}
			}

			return new AuthenticationResult( result, authenticationAttemptID );
		}

		public static bool VerifyPasswordMatch( string passwordToTest, string correctPassword, bool passwordIsHashed = false )
		{
			bool result = false;
			if ( passwordIsHashed )
			{
				SHA1 sha = SHA1.Create();
				byte[] correctHashedPasswordBytes = sha.ComputeHash( Encoding.UTF8.GetBytes( correctPassword ) );
				string correctHashedPasswordString = Convert.ToBase64String( correctHashedPasswordBytes );

				if ( passwordToTest == correctHashedPasswordString )
				{
					result = true;
				}
			}
			else
			{
				if ( passwordToTest == correctPassword )
				{
					result = true;
				}
			}
			return result;
		}

		public static bool VerifyPasswordMatch( string passwordToTest, byte[] encryptedCorrectPassword, bool passwordIsHashed = false )
		{
			string decryptedCorrectPassword = Symmetric.Decrypt( encryptedCorrectPassword );
			return VerifyPasswordMatch( passwordToTest, decryptedCorrectPassword, passwordIsHashed );
		}

		private void LoginButton_Click( object sender, RoutedEventArgs e )
		{
			Account account;

            OutputTB.Clear();
			account = CoreDataRepository.Accounts.GetByUserName( useridTB.Text );
			AuthenticationResult result = Authenticate( useridTB.Text, passwordTB.Password, out account );
            if (result.Status == AuthenticationStatus.Success)
            {
                CurrentAccountID = account.ID;
                Contact contact = Repository.Contacts.GetByAccount(account);
                TestControl testControl = new TestControl();
                testControl.DataRepository = DataRepository;
                this.Content = testControl;
            }
            else
            {
                OutputTB.Text = "Invalid Login";
            }
		}
	}
}