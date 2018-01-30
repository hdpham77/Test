using CERS.Model;
using CERS.Xml;
using CERS.Xml.FacilitySubmittal;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UPF;

namespace CERS.EDT
{
	public class RegulatorFacilitySubmittalQueryAdapter : OutboundAdapter<RegulatorFacilitySubmittalQueryArguments>
	{
		#region Fields

		private RegulatorFacilitySubmittalQueryXmlSerializer _Serializer;

		#endregion Fields

		#region Properties

		protected RegulatorFacilitySubmittalQueryXmlSerializer Serializer
		{
			get
			{
				if ( _Serializer == null )
				{
					_Serializer = new RegulatorFacilitySubmittalQueryXmlSerializer( Repository );
					_Serializer.NotificationReceived += new EventHandler<CERS.Xml.XmlSerializationNotificationEventArgs>( Serializer_NotificationReceived );
				}
				return _Serializer;
			}
		}

		#endregion Properties

		#region Constructor

		public RegulatorFacilitySubmittalQueryAdapter( EDTTransactionScope edtTransactionScope )

			: base( edtTransactionScope )
		{
		}

		#endregion Constructor

		#region Process Method(s)

		public override XElement Process( NameValueCollection arguments )
		{
			if ( arguments == null )
			{
				throw new ArgumentNullException( "arguments" );
			}

			RegulatorFacilitySubmittalQueryArguments args = new RegulatorFacilitySubmittalQueryArguments( arguments );

			return Process( args );
		}

		public override XElement Process( RegulatorFacilitySubmittalQueryArguments args )
		{
			TransactionScope.WriteQueryArguments( args );
			var regulator = Repository.Regulators.GetByEDTIdentityKey( args.RegulatorCode );
			if ( regulator == null )
			{
				throw new Exception( "The specified RegulatorCode \"" + args.RegulatorCode + "\" is invalid." );
			}

			if ( args.Statuses.Contains( SubmittalElementStatus.Draft ) )
			{
				throw new Exception( "You cannot download Facility Submittals with a Status of Draft" );
			}

			bool useFSEXmlCache = EDTConfig.Serialization.RegulatorFacilitySubmittalQuery.UseXmlCache;

			//attach the regulator to this transaction so that we know that this regulator made this transaction easily.
			TransactionScope.Connect( regulator );

			List<FacilitySubmittalElement> fses = new List<FacilitySubmittalElement>();

			fses.AddRange( Repository.FacilitySubmittalElements.EDTSearch(
							args.SubmittedOnStart,
							args.SubmittedOnEnd,
							args.SubmittalElementType,
							args.CERSID,
							regulator.ID,
							args.SubmittalActionOnStart,
							args.SubmittalActionOnEnd,
							args.Statuses.ToArray()
							).ToList()
				);

			XmlSerializationResult result = null;

			try
			{
				result = Serializer.Serialize( fses, TransactionScope.Transaction, useFSEXmlCache: useFSEXmlCache );
				TransactionScope.Complete( EDTTransactionStatus.Accepted );
				Serializer.UpdateTransactionStatus( result, TransactionScope.Transaction );
			}
			catch ( Exception ex )
			{
				TransactionScope.WriteActivity( "Unable to serialize the data.", ex );
				TransactionScope.WriteMessage( "Unable to serialize the data.", ex );
				TransactionScope.Complete( EDTTransactionStatus.Rejected );
			}

			TransactionScope.SaveXml( result.Xml, EDTTransactionXmlDirection.Outbound );
			return result.Xml;
		}

		#endregion Process Method(s)

		#region Protected Methods

		protected virtual void Serializer_NotificationReceived( object sender, Xml.XmlSerializationNotificationEventArgs e )
		{
			TransactionScope.WriteActivity( e.Message );
		}

		#endregion Protected Methods
	}
}