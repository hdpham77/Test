using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using UPF;

namespace CERS.EDT
{
	public class RegulatorFacilityTransferQueryArguments : IAdapterArguments
	{
		#region Constants

		public const string AssumedOwnershipOnEndArgument_Key = "assumedOwnershipOnEnd";
		public const string AssumedOwnershipOnStartArgument_Key = "assumedOwnershipOnStart";
		public const string CERSIDArgument_Key = "CERSID";
		public const string OccurredOnEndArgument_Key = "occurredOnEnd";
		public const string OccurredOnStartArgument_Key = "occurredOnStart";
		public const string OrganizationCodeArgument_Key = "organizationCode";
		public const string RegulatorCodeArgument_Key = "regulatorCode";

		#endregion Constants

		#region Properties

		public DateTime? AssumedOwnershipOnEnd { get; set; }

		public DateTime? AssumedOwnershipOnStart { get; set; }

		public int? CERSID { get; set; }

		public DateTime? OccurredOnEnd { get; set; }

		public DateTime? OccurredOnStart { get; set; }

		public int? OrganizationCode { get; set; }

		public int? RegulatorCode { get; set; }

		#endregion Properties

		#region Constructors

		public RegulatorFacilityTransferQueryArguments()
		{
		}

		public RegulatorFacilityTransferQueryArguments( NameValueCollection arguments )
		{
			ParseTo( arguments, this );
		}

		#endregion Constructors

		#region Static Methods

		public static RegulatorFacilityTransferQueryArguments Parse( NameValueCollection arguments )
		{
			RegulatorFacilityTransferQueryArguments args = new RegulatorFacilityTransferQueryArguments();
			ParseTo( arguments, args );
			return args;
		}

		public static void ParseTo( NameValueCollection arguments, RegulatorFacilityTransferQueryArguments args )
		{
			arguments = arguments.ToLowercaseKey();
			args.OccurredOnStart = arguments.GetValueLowercaseKey<DateTime?>( OccurredOnStartArgument_Key, null );
			args.OccurredOnEnd = arguments.GetValueLowercaseKey<DateTime?>( OccurredOnEndArgument_Key, null );
			args.AssumedOwnershipOnStart = arguments.GetValueLowercaseKey<DateTime?>( AssumedOwnershipOnStartArgument_Key, null );
			args.AssumedOwnershipOnEnd = arguments.GetValueLowercaseKey<DateTime?>( AssumedOwnershipOnEndArgument_Key, null );

			args.RegulatorCode = arguments.GetValueLowercaseKey<int>( RegulatorCodeArgument_Key, 0 );
			args.CERSID = arguments.GetValueLowercaseKey<int?>( CERSIDArgument_Key, null );
			args.OrganizationCode = arguments.GetValueLowercaseKey<int?>( OrganizationCodeArgument_Key, null );
		}

		#endregion Static Methods

		#region Instance Methods

		public NameValueCollection ToNameValueCollection()
		{
			NameValueCollection results = new NameValueCollection();

			results.Add( OccurredOnStartArgument_Key, ( OccurredOnStart.HasValue ? OccurredOnStart.Value.ToString() : "" ) );
			results.Add( OccurredOnEndArgument_Key, ( OccurredOnEnd.HasValue ? OccurredOnEnd.Value.ToString() : "" ) );
			results.Add( AssumedOwnershipOnStartArgument_Key, ( AssumedOwnershipOnStart.HasValue ? AssumedOwnershipOnStart.Value.ToString() : "" ) );
			results.Add( AssumedOwnershipOnEndArgument_Key, ( AssumedOwnershipOnEnd.HasValue ? AssumedOwnershipOnEnd.Value.ToString() : "" ) );
			results.Add( OrganizationCodeArgument_Key, ( OrganizationCode.HasValue ? OrganizationCode.Value.ToString() : "" ) );
			results.Add( RegulatorCodeArgument_Key, RegulatorCode.ToString() );
			results.Add( CERSIDArgument_Key, ( CERSID.HasValue ? CERSID.Value.ToString() : "" ) );

			results = results.ToLowercaseKey();
			return results;
		}

		#endregion Instance Methods
	}
}