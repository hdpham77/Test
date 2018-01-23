using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using UPF;

namespace CERS.EDT
{
	public class RegulatorOrganizatonQueryAdapterArguments : IAdapterArguments
	{
		#region Constants

		public const string CERSIDArgument_Key = "CERSID";
		public const string EstablishedSinceArgument_Key = "establishedSince";
		public const string IncludeFacilitiesArgument_Key = "includeFacilities";
		public const string OrganizationCodeArgument_Key = "organizationCode";
		public const string OrganizationHeadquartersArgument_Key = "organizationHeadquarters";
		public const string OrganizationNameArgument_Key = "organizationName";
		public const string RegulatorCodeArgument_Key = "regulatorCode";

		#endregion Constants

		#region Properties

		public int? CERSID { get; set; }

		public DateTime? EstablishedSince { get; set; }

		public bool IncludeFacilities { get; set; }

		public int? OrganizationCode { get; set; }

		public string OrganizationHeadquarters { get; set; }

		public string OrganizationName { get; set; }

		public int? RegulatorCode { get; set; }

		#endregion Properties

		#region Constructors

		public RegulatorOrganizatonQueryAdapterArguments()
		{
		}

		public RegulatorOrganizatonQueryAdapterArguments( NameValueCollection arguments )
		{
			ParseTo( arguments, this );
		}

		#endregion Constructors

		#region Static Methods

		public static RegulatorOrganizatonQueryAdapterArguments Parse( NameValueCollection arguments )
		{
			RegulatorOrganizatonQueryAdapterArguments args = new RegulatorOrganizatonQueryAdapterArguments();
			ParseTo( arguments, args );
			return args;
		}

		public static void ParseTo( NameValueCollection arguments, RegulatorOrganizatonQueryAdapterArguments args )
		{
			arguments = arguments.ToLowercaseKey();
			args.EstablishedSince = arguments.GetValueLowercaseKey<DateTime?>( EstablishedSinceArgument_Key, null );
			args.OrganizationName = arguments.GetValueLowercaseKey<string>( OrganizationNameArgument_Key, null );
			args.OrganizationHeadquarters = arguments.GetValueLowercaseKey<string>( OrganizationHeadquartersArgument_Key, null );
			args.OrganizationCode = arguments.GetValueLowercaseKey<int?>( OrganizationCodeArgument_Key, null );
			args.IncludeFacilities = arguments.GetValueLowercaseKey<bool>( IncludeFacilitiesArgument_Key, false );
			args.RegulatorCode = arguments.GetValueLowercaseKey<int?>( RegulatorCodeArgument_Key, 0 );
			args.CERSID = arguments.GetValueLowercaseKey<int?>( CERSIDArgument_Key, null );
		}

		#endregion Static Methods

		#region Instance Methods

		public NameValueCollection ToNameValueCollection()
		{
			NameValueCollection results = new NameValueCollection();
			results.Add( EstablishedSinceArgument_Key, ( EstablishedSince.HasValue ? EstablishedSince.Value.ToString() : "" ) );
			results.Add( OrganizationNameArgument_Key, OrganizationName );
			results.Add( OrganizationHeadquartersArgument_Key, OrganizationHeadquarters );
			results.Add( OrganizationCodeArgument_Key, ( OrganizationCode.HasValue ? OrganizationCode.Value.ToString() : "" ) );
			results.Add( IncludeFacilitiesArgument_Key, IncludeFacilities.ToString() );
			results.Add( RegulatorCodeArgument_Key, RegulatorCode.ToString() );
			results.Add( CERSIDArgument_Key, ( CERSID.HasValue ? CERSID.Value.ToString() : "" ) );

			results = results.ToLowercaseKey();
			return results;
		}

		#endregion Instance Methods
	}
}