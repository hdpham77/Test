using CERS.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using UPF;
using UPF.Core;

namespace CERS.EDT
{
	public class EDTTransactionScope : IDisposable
	{
		#region Fields

		private bool _Completed = false;
		private bool _Disposed = false;
		private bool _Initialized = false;
		private ICERSRepositoryManager _Repository;

		#endregion Fields

		#region Properties

		public IAccount Account { get; protected set; }

		public bool Authenticated { get; protected set; }

		public int? AuthenticationRequestID { get; set; }

		public CallerContext CallerContext { get; protected set; }

		public bool Completed
		{
			get
			{
				return _Completed;
			}
		}

		public Context Context { get; protected set; }

		public EDTEndpoint Endpoint { get; protected set; }

		public string RemoteHost { get; protected set; }

		public ICERSRepositoryManager Repository
		{
			get
			{
				if ( _Repository == null )
				{
					_Repository = ServiceLocator.GetRepositoryManager( Account.ID );
				}
				return _Repository;
			}
			protected set
			{
				_Repository = value;
			}
		}

		public EDTTransactionStatus Status
		{
			get
			{
				return Transaction.GetStatus();
			}

			set
			{
				if ( Transaction != null )
				{
					Transaction.SetStatus( value );
				}
			}
		}

		public EDTTransaction Transaction { get; protected set; }

		#endregion Properties

		#region Constructor(s)

		public EDTTransactionScope( IAccount account, EDTEndpoint endpoint, string remoteHost = null, Context context = Context.Regulator, ICERSRepositoryManager repository = null, int? authenticationRequestID = null, CallerContext callerContext = CallerContext.EDT )
		{
			account.CheckNull( "account" );

			Account = account;
			Context = context;
			Repository = repository;
			Endpoint = endpoint;
			RemoteHost = remoteHost;
			AuthenticationRequestID = authenticationRequestID;
			CallerContext = callerContext;
			Initialize();
		}

		#endregion Constructor(s)

		#region Destructor

		~EDTTransactionScope()
		{
			Dispose( false );
		}

		#endregion Destructor

		public void Complete( EDTTransactionStatus status = EDTTransactionStatus.Accepted )
		{
			if ( !_Completed )
			{
				if ( Transaction == null )
				{
					throw new Exception( "Invalid state of EDTTransactionScope" );
				}
				Transaction.StatusID = (int)status;
				Transaction.ProcessedOn = DateTime.Now;
				Transaction.CompletedOn = DateTime.Now;
				Repository.EDTTransactions.Save( Transaction );
				_Completed = true;
			}
		}

		public void Connect( Facility facility )
		{
			if ( Transaction == null )
			{
				throw new InvalidOperationException( "Transaction is required." );
			}
			facility.CheckNull( "facility" );
			Repository.FacilityEDTTransactions.EnsureExists( facility, Transaction );
		}

		public void Connect( FacilitySubmittalElement fse )
		{
			if ( Transaction == null )
			{
				throw new InvalidOperationException( "Transaction is required." );
			}
			fse.CheckNull( "fse" );
			Repository.FacilitySubmittalElementEDTTransactions.EnsureExists( fse, Transaction );
		}

		public void Connect( FacilityRegulatorSubmittalElement frse )
		{
			if ( Transaction == null )
			{
				throw new InvalidOperationException( "Transaction is required." );
			}
			frse.CheckNull( "frse" );
			Repository.FacilityRegulatorSubmittalElementEDTTransactions.EnsureExists( frse, Transaction );
		}

		public void Connect( Regulator regulator )
		{
			if ( Transaction == null )
			{
				throw new InvalidOperationException( "Transaction is required." );
			}

			regulator.CheckNull( "regulator" );
			Repository.RegulatorEDTTransactions.EnsureExists( regulator, Transaction );
		}

		public void Connect( int regulatorID )
		{
			if ( Transaction == null )
			{
				throw new InvalidOperationException( "Transaction is required." );
			}
			Repository.RegulatorEDTTransactions.EnsureExists( regulatorID, Transaction.ID );
		}

		public void Connect( FacilitySubmittal facilitySubmittal )
		{
			Repository.FacilitySubmittalEDTTransactions.EnsureExists( facilitySubmittal, Transaction );
		}

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		public void Execute( Action method )
		{
			Initialize();

			method();
		}

		public EDTTransactionStatus GetStatus()
		{
			return Transaction.GetStatus();
		}

		public void Initialize()
		{
			if ( !_Initialized )
			{
				Transaction = new EDTTransaction();
				Transaction.Key = Guid.NewGuid();
				Transaction.ReceivedOn = DateTime.Now;
				Transaction.StatusID = (int)EDTTransactionStatus.InProcess;
				Transaction.AccountID = Account.ID;
				Transaction.EndpointID = (int)Endpoint;
				Transaction.IPAddress = RemoteHost;
				Transaction.AuthenticationRequestID = AuthenticationRequestID;
				Repository.EDTTransactions.Save( Transaction );
				_Initialized = true;
			}
		}

		/// <summary>
  /// Save
  /// </summary>
  /// <param name="direction"></param>
  /// <param name="xml"></param>
		public void SaveXml( XDocument xml, EDTTransactionXmlDirection direction )
		{
			if ( Transaction == null )
			{
				throw new InvalidOperationException( "Unable to save the XML when there is no transaction." );
			}

			if ( xml != null )
			{
				var transactionXml = Repository.EDTTransactionXmls.Create( xml, direction, Transaction );
				Transaction.Xmls.Add( transactionXml );
			}
		}

		public void SaveXml( XElement xml, EDTTransactionXmlDirection direction )
		{
			if ( Transaction == null )
			{
				throw new InvalidOperationException( "Unable to save the XML when there is no transaction." );
			}
			if ( xml != null )
			{
				var transactionXml = Repository.EDTTransactionXmls.Create( xml, direction, Transaction );
				Transaction.Xmls.Add( transactionXml );
			}
		}

		public void SaveXml( string xml, EDTTransactionXmlDirection direction )
		{
			XDocument doc = XDocument.Parse( xml );
			SaveXml( doc, direction );
		}

		public void SetEDTClientKey( string edtClientKey )
		{
			Transaction.EDTClientKey = edtClientKey;
		}

		public void SetProcessedOn( DateTime? value = null )
		{
			if ( Transaction != null )
			{
				if ( value == null )
				{
					value = DateTime.Now;
				}
				Transaction.ProcessedOn = value;
			}
		}

		public void SetStatus( EDTTransactionStatus status )
		{
			if ( Transaction != null )
			{
				Transaction.SetStatus( status );
			}
		}

		public void UpdateEntityCount( int count )
		{
			Transaction.EntityCount = count;
		}

		public void WriteActivity( string message, Exception exception = null, EDTTransactionMessageType? type = null )
		{
			string activityLogMessagePrefix = "";
			if ( type.HasValue )
			{
				switch ( type.Value )
				{
					case EDTTransactionMessageType.Error:
						activityLogMessagePrefix = "Error: ";
						break;

					case EDTTransactionMessageType.Required:
						activityLogMessagePrefix = "Required Guidance Error: ";
						break;

					case EDTTransactionMessageType.Warning:
						activityLogMessagePrefix = "Warning Guidance: ";
						break;

					case EDTTransactionMessageType.Advisory:
						activityLogMessagePrefix = "Advisory Guidance: ";
						break;
				}
			}

			if ( !string.IsNullOrWhiteSpace( message ) || exception != null )
			{
				string body = string.Empty;

				if ( !string.IsNullOrWhiteSpace( activityLogMessagePrefix ) )
				{
					body += activityLogMessagePrefix;
				}

				if ( !string.IsNullOrWhiteSpace( message ) )
				{
					body += message;
				}

				if ( exception != null )
				{
					if ( body.Trim().Length > 0 )
					{
						body += " ";
					}
					body += "Exception: " + exception.Format();
				}

				EDTTransactionActivityLog entry = new EDTTransactionActivityLog();
				entry.EDTTransaction = Transaction;
				entry.Message = body;
				Repository.EDTTransactionActivityLogs.Create( entry );
			}
		}

		public void WriteGuidanceMessage( string message, GuidanceLevel guidanceLevel )
		{
			GuidanceMessage gm = new GuidanceMessage();
			gm.Message = message;
			gm.EDTTransactionID = Transaction.ID;
			gm.LevelID = (int)guidanceLevel;
			Repository.GuidanceMessages.Create( gm );
		}

		public void WriteMessage( string message, EDTTransactionMessageType type )
		{
			EDTTransactionMessage entry = new EDTTransactionMessage();
			entry.TypeID = (int)type;
			entry.EDTTransaction = Transaction;
			entry.Message = message;
			Repository.EDTTransactionMessages.Save( entry );

			WriteActivity( message, type: type );
		}

		public void WriteMessage( string message, Exception ex )
		{
			string reformattedMessage = "Error: " + message;
			if ( ex != null )
			{
				reformattedMessage += " >> " + ex.Message;
			}

			WriteMessage( reformattedMessage, EDTTransactionMessageType.Error );
		}

		public virtual void WriteQueryArgument( string name, string value )
		{
			if ( Transaction == null )
			{
				throw new InvalidOperationException( "Transaction is required." );
			}

			if ( string.IsNullOrWhiteSpace( name ) )
			{
				throw new ArgumentNullException( "name" );
			}

			EDTTransactionQueryArgument argument = new EDTTransactionQueryArgument()
			{
				EDTTransaction = Transaction,
				Name = name,
				Value = value
			};
			argument.SetCommonFields( Account.ID, creating: true );
			Transaction.QueryArguments.Add( argument );
		}

		public virtual void WriteQueryArguments( IAdapterArguments processorArguments )
		{
			processorArguments.CheckNull( "processorArguments" );
			var arguments = processorArguments.ToNameValueCollection();
			if ( arguments != null )
			{
				foreach ( string argument in arguments.AllKeys )
				{
					WriteQueryArgument( argument, arguments[argument] );
				}
			}
		}

		public virtual void WriteQueryArguments( NameValueCollection arguments )
		{
			foreach ( string argument in arguments.AllKeys )
			{
				WriteQueryArgument( argument, arguments[argument] );
			}
		}

		protected virtual void Dispose( bool disposing )
		{
			if ( !_Disposed )
			{
				if ( disposing )
				{
					if ( !_Completed )
					{
						Complete();
					}
					_Disposed = true;
				}
			}
		}
	}
}