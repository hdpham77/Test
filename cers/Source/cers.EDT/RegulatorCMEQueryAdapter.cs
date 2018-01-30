using CERS.Model;
using CERS.Xml.CME;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CERS.EDT
{
	public class RegulatorCMEQueryAdapter : OutboundAdapter<RegulatorCMEQueryArguments>
	{
		#region Fields

		private CMESubmittalQueryXmlSerializer _Serializer;

		#endregion Fields

		#region Properties

		protected CMESubmittalQueryXmlSerializer Serializer
		{
			get
			{
				if ( _Serializer == null )
				{
					_Serializer = new CMESubmittalQueryXmlSerializer( Repository );
				}
				return _Serializer;
			}
		}

		#endregion Properties

		#region Constructor

		public RegulatorCMEQueryAdapter( EDTTransactionScope transactionScope )
			: base( transactionScope )
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

			RegulatorCMEQueryArguments args = new RegulatorCMEQueryArguments( arguments );

			return Process( args );
		}

		public override XElement Process( RegulatorCMEQueryArguments args )
		{
			TransactionScope.WriteQueryArguments( args );
			var regulator = Repository.Regulators.GetByEDTIdentityKey( args.RegulatorCode );
			if ( regulator == null )
			{
				throw new Exception( "The specified RegulatorCode \"" + args.RegulatorCode + "\" did not return a Regulator." );
			}

			// Do not allow users to export Draft CME Data:
			if ( args.Status.HasValue && args.Status.Value == CMEDataStatus.Draft )
			{
				throw new Exception( "Exporting \"Draft\" CME data is not supported." );
			}

			if ( args.Statuses != null && args.Statuses.Contains( CMEDataStatus.Draft ) )
			{
				throw new Exception( "Exporting \"Draft\" CME data is not supported." );
			}

			List<Inspection> inspections = new List<Inspection>();
			List<Violation> violations = new List<Violation>();
			List<Enforcement> enforcements = new List<Enforcement>();
			List<EnforcementViolation> enforcementViolations = new List<EnforcementViolation>();

			using ( TimedCodeBlockCallbackInvoker codeBlock = new TimedCodeBlockCallbackInvoker( "Fetching Source Data", null ) )
			{
				if ( args.Statuses.Count > 0 )
				{
					foreach ( var status in args.Statuses )
					{
						inspections.AddRange( Repository.Inspections.Search(
								earliestOccurredOn: args.StartDate,
								latestOccurredOn: args.EndDate,
								regulatorID: regulator.ID,
								CERSID: args.CERSID,
								cmeDataStatusID: (int) status
							).ToList() );

						violations.AddRange( Repository.Violations.Search(
								earliestOccurredOn: args.StartDate,
								latestOccurredOn: args.EndDate,
								regulatorID: regulator.ID,
								CERSID: args.CERSID,
								cmeDataStatusID: (int) status
							).ToList() );

						enforcements.AddRange( Repository.Enforcements.Search(
								earliestOccurredOn: args.StartDate,
								latestOccurredOn: args.EndDate,
								regulatorID: regulator.ID,
								CERSID: args.CERSID,
								cmeDataStatusID: (int) status
							).ToList() );

						enforcementViolations.AddRange( Repository.EnforcementViolations.Search(
								earliestOccurredOn: args.StartDate,
								latestOccurredOn: args.EndDate,
								regulatorID: regulator.ID,
								CERSID: args.CERSID,
								cmeDataStatusID: (int) status
							).ToList() );
					}
				}
				else
				{
					inspections.AddRange( Repository.Inspections.Search(
							earliestOccurredOn: args.StartDate,
							latestOccurredOn: args.EndDate,
							regulatorID: regulator.ID,
							CERSID: args.CERSID
						).Where( p => p.CMEDataStatusID != (int) CMEDataStatus.Draft ).ToList() );

					violations.AddRange( Repository.Violations.Search(
							earliestOccurredOn: args.StartDate,
							latestOccurredOn: args.EndDate,
							regulatorID: regulator.ID,
							CERSID: args.CERSID
						).Where( p => p.CMEDataStatusID != (int) CMEDataStatus.Draft ).ToList() );

					enforcements.AddRange( Repository.Enforcements.Search(
							earliestOccurredOn: args.StartDate,
							latestOccurredOn: args.EndDate,
							regulatorID: regulator.ID,
							CERSID: args.CERSID
						).Where( p => p.CMEDataStatusID != (int) CMEDataStatus.Draft ).ToList() );

					enforcementViolations.AddRange( Repository.EnforcementViolations.Search(
							earliestOccurredOn: args.StartDate,
							latestOccurredOn: args.EndDate,
							regulatorID: regulator.ID,
							CERSID: args.CERSID
						).Where( p => p.CMEDataStatusID != (int) CMEDataStatus.Draft ).ToList() );
				}
			}

			var result = Serializer.Serialize( inspections, violations, enforcements, enforcementViolations );

			TransactionScope.Complete();

			TransactionScope.SaveXml( result.Xml, EDTTransactionXmlDirection.Outbound );

			return result.Xml;
		}

		#endregion Process Method(s)
	}
}