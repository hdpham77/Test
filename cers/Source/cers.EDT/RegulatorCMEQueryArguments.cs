using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using UPF;

namespace CERS.EDT
{
	public class RegulatorCMEQueryArguments : IAdapterArguments
	{
		#region Constants

		public const string CERSIDArgument_Key = "CERSID";
		public const string EndDateArgument_Key = "endDate";
		public const string RegulatorCodeArgument_Key = "regulatorCode";
		public const string StartDateArgument_Key = "startDate";
		public const string Status_Key = "status";

		#endregion Constants

		#region Fields

		private List<CMEDataStatus> _Statuses;

		#endregion Fields

		#region Properties

		public int? CERSID { get; set; }

		public DateTime? EndDate { get; set; }

		public int RegulatorCode { get; set; }

		public DateTime? StartDate { get; set; }

		public CMEDataStatus? Status { get; set; }

		public List<CMEDataStatus> Statuses
		{
			get
			{
				if ( _Statuses == null )
				{
					_Statuses = new List<CMEDataStatus>();
				}
				return _Statuses;
			}
		}

		#endregion Properties

		#region Constructors

		public RegulatorCMEQueryArguments()
		{
		}

		public RegulatorCMEQueryArguments( NameValueCollection arguments )
		{
			ParseTo( arguments, this );
		}

		#endregion Constructors

		#region Static Methods

		public static RegulatorCMEQueryArguments Parse( NameValueCollection arguments )
		{
			RegulatorCMEQueryArguments args = new RegulatorCMEQueryArguments();
			ParseTo( arguments, args );
			return args;
		}

		public static void ParseTo( NameValueCollection arguments, RegulatorCMEQueryArguments args )
		{
			arguments = arguments.ToLowercaseKey();
			args.RegulatorCode = arguments.GetValueLowercaseKey<int>( RegulatorCodeArgument_Key, 0 );
			args.StartDate = arguments.GetValueLowercaseKey<DateTime?>( StartDateArgument_Key, null );
			args.EndDate = arguments.GetValueLowercaseKey<DateTime?>( EndDateArgument_Key, null );
			args.CERSID = arguments.GetValueLowercaseKey<int?>( CERSIDArgument_Key, null );

			int temp;
			string[] statuses = arguments.GetValueLowercaseKey( Status_Key, string.Empty ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
			foreach ( string status in statuses )
			{
				if ( int.TryParse( status, out temp ) )
				{
					args.Statuses.Add( (CMEDataStatus) temp );
				}
			}
		}

		#endregion Static Methods

		#region Instance Methods

		public NameValueCollection ToNameValueCollection()
		{
			NameValueCollection results = new NameValueCollection();

			results.Add( StartDateArgument_Key, ( StartDate.HasValue ? StartDate.Value.ToString() : "" ) );
			results.Add( EndDateArgument_Key, ( EndDate.HasValue ? EndDate.Value.ToString() : "" ) );
			results.Add( CERSIDArgument_Key, ( CERSID.HasValue ? CERSID.Value.ToString() : "" ) );
			results.Add( Status_Key, ( Statuses.Count > 0 ? ( from s in Statuses select (int) s ).ToDelimitedString( "," ) : "" ) );
			results = results.ToLowercaseKey();
			return results;
		}

		#endregion Instance Methods
	}
}