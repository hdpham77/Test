using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using UPF;

namespace CERS.EDT
{
	public class RegulatorFacilityQueryArguments : IAdapterArguments
	{
		#region Constants

		public const string CERSID_Key = "CERSID";
		public const string City_Key = "city";
		public const string CreatedOnEnd_Key = "createdOnEnd";
		public const string CreatedOnStart_Key = "createdOnStart";
		public const string OrganizationCode_Key = "organizationCode";
		public const string RegulatorCode_Key = "regulatorCode";
		public const string Street_Key = "street";
		public const string ZIP_Key = "zip";

		#endregion Constants

		#region Properties

		public int? CERSID { get; set; }

		public string City { get; set; }

		public DateTime? CreatedOnEnd { get; set; }

		public DateTime? CreatedOnStart { get; set; }

		public int? OrganizationCode { get; set; }

		public int? RegulatorCode { get; set; }

		public string Street { get; set; }

		public string ZIP { get; set; }

		#endregion Properties

		#region Constructors

		public RegulatorFacilityQueryArguments()
		{
		}

		public RegulatorFacilityQueryArguments( NameValueCollection arguments )
		{
			ParseTo( arguments, this );
		}

		#endregion Constructors

		#region Static Methods

		public static RegulatorFacilityQueryArguments Parse( NameValueCollection arguments )
		{
			RegulatorFacilityQueryArguments args = new RegulatorFacilityQueryArguments();
			ParseTo( arguments, args );
			return args;
		}

		public static void ParseTo( NameValueCollection arguments, RegulatorFacilityQueryArguments args )
		{
			arguments = arguments.ToLowercaseKey();
			args.CreatedOnStart = arguments.GetValueLowercaseKey<DateTime?>( CreatedOnStart_Key, null );
			args.CreatedOnEnd = arguments.GetValueLowercaseKey<DateTime?>( CreatedOnEnd_Key, null );

			args.Street = arguments.GetValueLowercaseKey<string>( Street_Key, null );
			args.City = arguments.GetValueLowercaseKey<string>( City_Key, null );
			args.ZIP = arguments.GetValueLowercaseKey<string>( ZIP_Key, null );

			args.RegulatorCode = arguments.GetValueLowercaseKey<int>( RegulatorCode_Key, 0 );
			args.CERSID = arguments.GetValueLowercaseKey<int?>( CERSID_Key, null );
			args.OrganizationCode = arguments.GetValueLowercaseKey<int?>( OrganizationCode_Key, null );
		}

		#endregion Static Methods

		#region Instance Methods

		public NameValueCollection ToNameValueCollection()
		{
			NameValueCollection results = new NameValueCollection();
			results.Add( CreatedOnStart_Key, CreatedOnStart );
			results.Add( CreatedOnEnd_Key, CreatedOnEnd );
			results.Add( RegulatorCode_Key, RegulatorCode );
			results.Add( CERSID_Key, CERSID );
			results.Add( OrganizationCode_Key, OrganizationCode );
			results.Add( Street_Key, Street );
			results.Add( City_Key, City );
			results.Add( ZIP_Key, ZIP );

			results = results.ToLowercaseKey();
			return results;
		}

		#endregion Instance Methods
	}
}