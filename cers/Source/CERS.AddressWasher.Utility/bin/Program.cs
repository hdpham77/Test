using CERS;
using CERS.AddressServices;
using CERS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UPF;
using UPF.Linq;

namespace CERS.AddressWasher.Utility
{
	internal class Program
	{
		public static void CheckLength( string fieldName, string value, int length, ref bool lengthViolation )
		{
			if ( !string.IsNullOrWhiteSpace( value ) )
			{
				if ( value.Trim().Length > length )
				{
					Console.WriteLine( "\t\t" + fieldName + " exceeds max length (" + length + "): " + value );
					lengthViolation = true;
				}
			}
		}

		public static string GetWashedAddress( AddressInformation info )
		{
			return info.Street + " " + info.City + ", " + info.State + " " + info.ZipCode;
		}

		private static string GetSourceAddress( Facility facility )
		{
			string result = facility.Street + " " + facility.City + ", " + facility.State + " " + facility.ZipCode;
			return result;
		}

		private static void Main( string[] args )
		{
			Console.WriteLine( "CERS2 Address Washing Utility, v0.01" );
			Console.WriteLine( "Initializing Data Registry..." );
			DataRegistry.Initialize();
			int successCount = 0;
			int failureCount = 0;
			int lengthViolationsCount = 0;
			int skippedDueToMissingStreet = 0;

			bool lengthViolation = false;
			using ( ICERSRepositoryManager repo = ServiceLocator.GetRepositoryManager() )
			{
				ICERSSystemServiceManager services = ServiceLocator.GetSystemServiceManager( repo );
				Console.WriteLine( "Gathering Facilities..." );

                //DateTime washDate = DateTime.Now.Date.AddDays( -3 );
                //DateTime washDate = Convert.ToDateTime( "12/31/2013" );
                DateTime washDate = Convert.ToDateTime( "6/30/2014" );

				//var facilities = repo.Facilities.Search(washDate: washDate).ToList();

                var facilities = ( from fac in repo.DataModel.Facilities where !fac.Voided && !fac.IsAddressWashed && fac.WashDate > washDate select fac ).ToList();

				//var facilities = ( from facil in repo.DataModel.Facilities where facil.Voided == false select facil ).ToList();

				Console.WriteLine( "Found " + facilities.Count() + " Facilities..." );
				foreach ( var facility in facilities )
				{
					lengthViolation = false;
					Console.WriteLine( "CERSID: " + facility.CERSID );
					Console.WriteLine( "Facility Name: " + facility.Name );
					Console.Write( "\tSource Address: " );
					Console.WriteLine( GetSourceAddress( facility ) );
					if ( !string.IsNullOrWhiteSpace( facility.Street ) )
					{
						Console.Write( "\tWashed Address:" );
						AddressInformation result = services.Geo.GetAddressInformation( facility.Street, facility.City, facility.ZipCode, facility.State );
						if ( result != null )
						{
							CheckLength( "WashedStreet", result.Street, 100, ref lengthViolation );
							CheckLength( "WashedCity", result.City, 100, ref lengthViolation );
							CheckLength( "WashedState", result.State, 2, ref lengthViolation );
							CheckLength( "WashedZipCode", result.ZipCode, 10, ref lengthViolation );

							if ( !lengthViolation )
							{
								int washConfidence;
								if ( int.TryParse( result.MelissaAddressWashConfidence, out washConfidence ) == false )
								{
									washConfidence = 0;
								}

								if ( result.MelissaAddressWashSucceeded && Convert.ToInt32( washConfidence ) > 0 )
								{
									facility.IsAddressWashed = true;
									facility.WashDate = DateTime.Now;
									facility.WashedStreet = result.Street;
									facility.WashedStreetWithoutSuite = result.StreetWithoutSuiteNumber;
									facility.WashedSuite = result.Suite;
									facility.WashedCity = result.City;
									facility.WashedState = result.State;
									facility.WashedZipCode = result.ZipCode;
									facility.WashedStreetName = result.StreetName;
									facility.WashedStreetPostDirection = result.PostDirection;
									facility.WashedStreetPreDirection = result.PreDirection;
									facility.WashedStreetRange = result.Range;
									facility.WashedStreetSuffix = result.Suffix;
									facility.WashConfidence = washConfidence;
									facility.WashSourceID = (int) WashSource.Melissa;
									if ( facility.CountyID == null )
									{
										facility.CountyID = result.CountyID;
									}

									CERSFacilityGeoPoint geoPoint = facility.CERSFacilityGeoPoint;

									if ( geoPoint == null )
									{
										geoPoint = new CERSFacilityGeoPoint();
									}

									//make sure the result coordinates are in range before saving them
									if ( ( result.Latitude >= 32 && result.Latitude < 43 ) && ( result.Longitude >= -114 && result.Longitude < 125 ) )
									{
										geoPoint.CERSID = facility.CERSID;
										geoPoint.LatitudeMeasure = result.Latitude;
										geoPoint.LongitudeMeasure = result.Longitude;
										geoPoint.HorizontalAccuracyMeasure = result.HorizontalAccuracyMeasure;
										geoPoint.HorizontalCollectionMethodID = result.HorizontalCollectionMethodID;
										geoPoint.HorizontalReferenceDatumID = result.HorizontalReferenceDatumID;
										geoPoint.GeographicReferencePointID = result.GeographicReferencePointID;
										geoPoint.SetCommonFields();
										repo.CERSFacilityGeoPoints.Save( geoPoint );
									}
								}
								else if ( result.MelissaAddressWashSucceeded )
								{
									//Use the parsed street values to get a standardized address

									facility.IsAddressWashed = false;
									facility.WashDate = DateTime.Now;
									facility.WashSourceID = (int) WashSource.Melissa;
									facility.WashedStreet = result.Street;
									facility.WashedStreetWithoutSuite = result.StreetWithoutSuiteNumber;
									facility.WashedSuite = result.Suite;
									facility.WashedCity = result.City;
									facility.WashedState = result.State;
									facility.WashedZipCode = result.ZipCode;
									facility.WashedStreetName = result.StreetName;
									facility.WashedStreetPostDirection = null;
									facility.WashedStreetPreDirection = null;
									facility.WashedStreetRange = result.Range;
									facility.WashedStreetSuffix = result.Suffix;
								}

								Console.WriteLine( GetWashedAddress( result ) );
								Console.WriteLine( "\tUpdating Facility..." );
								repo.Facilities.Save( facility );
								Console.WriteLine( "\tFacility Updated" );
								successCount++;
							}
							else
							{
								Console.WriteLine( "\tFacility NOT Updated" );
								lengthViolationsCount++;
							}
						}
						else
						{
							Console.WriteLine( "Unable to obtain washed address." );
							failureCount++;
						}
					}
					else
					{
						skippedDueToMissingStreet++;
						Console.WriteLine( "\tSkipped Due to Missing Street!" );
					}
				}

				Console.WriteLine( "Process Completed." );
				Console.WriteLine( "Successfully Washed: " + successCount );
				Console.WriteLine( "Failed To Wash: " + failureCount );
				Console.WriteLine( "Length Violations (NOT Washed): " + lengthViolationsCount );
				Console.WriteLine( "Skipped Due to Missing Street: " + skippedDueToMissingStreet );
			}
			Console.Write( "Press enter to quit." );
			Console.ReadLine();
		}
	}
}