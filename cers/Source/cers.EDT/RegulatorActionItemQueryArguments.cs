using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using UPF;

namespace CERS.EDT
{
	public class RegulatorActionItemQueryArguments : IAdapterArguments
	{
		#region Constants

		public const string CERSIDArgument_Key = "CERSID";
		public const string CompletedArgument_Key = "completed";
		public const string OrganizationCodeArgument_Key = "organizationCode";
		public const string RegulatorCodeArgument_Key = "regulatorCode";
		public const string RequestedOnEndArgument_Key = "requestedOnEnd";
		public const string RequestedOnStartArgument_Key = "requestedOnStart";
		public const string TypeIDArgument_Key = "typeID";

		#endregion Constants

		#region Properties

		public int? CERSID { get; set; }

		public bool? Completed { get; set; }

		public int? OrganizationCode { get; set; }

		public int? RegulatorCode { get; set; }

		public DateTime? RequestedOnEnd { get; set; }

		public DateTime? RequestedOnStart { get; set; }

		public int? TypeID { get; set; }

		#endregion Properties

		#region Constructors

		public RegulatorActionItemQueryArguments()
		{
		}

		public RegulatorActionItemQueryArguments( NameValueCollection arguments )
		{
			ParseTo( arguments, this );
		}

		#endregion Constructors

		#region Static Methods

		public static RegulatorActionItemQueryArguments Parse( NameValueCollection arguments )
		{
			RegulatorActionItemQueryArguments args = new RegulatorActionItemQueryArguments();
			ParseTo( arguments, args );
			return args;
		}

		public static void ParseTo( NameValueCollection arguments, RegulatorActionItemQueryArguments args )
		{
			arguments = arguments.ToLowercaseKey();
			args.TypeID = arguments.GetValueLowercaseKey<int?>( TypeIDArgument_Key, null );
			args.Completed = arguments.GetValueLowercaseKey<bool?>( CompletedArgument_Key, null );
			args.RequestedOnStart = arguments.GetValueLowercaseKey<DateTime?>( RequestedOnStartArgument_Key, null );
			args.RequestedOnEnd = arguments.GetValueLowercaseKey<DateTime?>( RequestedOnEndArgument_Key, null );

			args.RegulatorCode = arguments.GetValueLowercaseKey<int>( RegulatorCodeArgument_Key, 0 );
			args.CERSID = arguments.GetValueLowercaseKey<int?>( CERSIDArgument_Key, null );
			args.OrganizationCode = arguments.GetValueLowercaseKey<int?>( OrganizationCodeArgument_Key, null );
		}

		#endregion Static Methods

		#region Instance Methods

		public NameValueCollection ToNameValueCollection()
		{
			NameValueCollection results = new NameValueCollection();

			results.Add( TypeIDArgument_Key, ( Completed.HasValue ? Completed.Value.ToString() : "" ) );
			results.Add( CompletedArgument_Key, ( TypeID.HasValue ? TypeID.Value.ToString() : "" ) );
			results.Add( RequestedOnStartArgument_Key, ( RequestedOnStart.HasValue ? RequestedOnStart.Value.ToString() : "" ) );
			results.Add( RequestedOnEndArgument_Key, ( RequestedOnEnd.HasValue ? RequestedOnEnd.Value.ToString() : "" ) );
			results.Add( OrganizationCodeArgument_Key, ( OrganizationCode.HasValue ? OrganizationCode.Value.ToString() : "" ) );
			results.Add( RegulatorCodeArgument_Key, RegulatorCode.ToString() );
			results.Add( CERSIDArgument_Key, ( CERSID.HasValue ? CERSID.Value.ToString() : "" ) );

			results = results.ToLowercaseKey();
			return results;
		}

		#endregion Instance Methods
	}
}