using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using UPF;

namespace CERS.EDT
{
	public class RegulatorFacilitySubmittalQueryArguments : IAdapterArguments
	{
		#region Constants

		public const string CERSIDArgument_Key = "CERSID";
		public const string RegulatorCodeArgument_Key = "regulatorCode";
		public const string SubmittalActionOnEndArgument_Key = "submittalActionOnEnd";
		public const string SubmittalActionOnStartArgument_Key = "submittalActionOnStart";
		public const string SubmittalElementStatus_Key = "status";
		public const string SubmittalElementType_Key = "submittalElement";
		public const string SubmittedOnEndArgument_Key = "submittedOnEnd";
		public const string SubmittedOnStartArgument_Key = "submittedOnStart";

		#endregion Constants

		#region Fields

		private List<SubmittalElementStatus> _Statuses;

		#endregion Fields

		#region Properties

		public int? CERSID { get; set; }

		public int RegulatorCode { get; set; }

		public List<SubmittalElementStatus> Statuses
		{
			get
			{
				if ( _Statuses == null )
				{
					_Statuses = new List<SubmittalElementStatus>();
				}
				return _Statuses;
			}
		}

		public DateTime? SubmittalActionOnEnd { get; set; }

		public DateTime? SubmittalActionOnStart { get; set; }

		public SubmittalElementType? SubmittalElementType { get; set; }

		public DateTime? SubmittedOnEnd { get; set; }

		public DateTime? SubmittedOnStart { get; set; }

		#endregion Properties

		#region Constructors

		public RegulatorFacilitySubmittalQueryArguments()
		{
		}

		public RegulatorFacilitySubmittalQueryArguments( NameValueCollection arguments )
		{
			ParseTo( arguments, this );
		}

		#endregion Constructors

		#region Static Methods

		public static RegulatorFacilitySubmittalQueryArguments Parse( NameValueCollection arguments )
		{
			RegulatorFacilitySubmittalQueryArguments args = new RegulatorFacilitySubmittalQueryArguments();
			ParseTo( arguments, args );
			return args;
		}

		public static void ParseTo( NameValueCollection arguments, RegulatorFacilitySubmittalQueryArguments args )
		{
			arguments = arguments.ToLowercaseKey();
			args.SubmittedOnStart = arguments.GetValueLowercaseKey<DateTime?>( SubmittedOnStartArgument_Key, null );
			args.SubmittedOnEnd = arguments.GetValueLowercaseKey<DateTime?>( SubmittedOnEndArgument_Key, null );

			args.SubmittalActionOnStart = arguments.GetValueLowercaseKey<DateTime?>( SubmittalActionOnStartArgument_Key, null );
			args.SubmittalActionOnEnd = arguments.GetValueLowercaseKey<DateTime?>( SubmittalActionOnEndArgument_Key, null );

			args.RegulatorCode = arguments.GetValueLowercaseKey<int>( RegulatorCodeArgument_Key, 0 );
			args.CERSID = arguments.GetValueLowercaseKey<int?>( CERSIDArgument_Key, null );
			args.SubmittalElementType = arguments.GetValueLowercaseKey<SubmittalElementType?>( SubmittalElementType_Key, null );

			int temp;
			string[] statuses = arguments.GetValueLowercaseKey( SubmittalElementStatus_Key, string.Empty ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
			foreach ( string status in statuses )
			{
				if ( int.TryParse( status, out temp ) )
				{
					args.Statuses.Add( (SubmittalElementStatus) temp );
				}
			}
		}

		#endregion Static Methods

		#region Instance Methods

		public NameValueCollection ToNameValueCollection()
		{
			NameValueCollection results = new NameValueCollection();

			results.Add( SubmittedOnStartArgument_Key, ( SubmittedOnStart.HasValue ? SubmittedOnStart.Value.ToString() : "" ) );
			results.Add( SubmittedOnEndArgument_Key, ( SubmittedOnEnd.HasValue ? SubmittedOnEnd.Value.ToString() : "" ) );

			results.Add( SubmittalActionOnStartArgument_Key, ( SubmittalActionOnStart.HasValue ? SubmittalActionOnStart.Value.ToString() : "" ) );
			results.Add( SubmittalActionOnEndArgument_Key, ( SubmittalActionOnEnd.HasValue ? SubmittalActionOnEnd.Value.ToString() : "" ) );

			results.Add( RegulatorCodeArgument_Key, RegulatorCode.ToString() );
			results.Add( CERSIDArgument_Key, ( CERSID.HasValue ? CERSID.Value.ToString() : "" ) );
			results.Add( SubmittalElementType_Key, ( SubmittalElementType.HasValue ? ( (int) SubmittalElementType.Value ).ToString() : "" ) );
			results.Add( SubmittalElementStatus_Key, ( Statuses.Count > 0 ? ( from s in Statuses select (int) s ).ToDelimitedString( "," ) : "" ) );

			results = results.ToLowercaseKey();
			return results;
		}

		#endregion Instance Methods
	}
}