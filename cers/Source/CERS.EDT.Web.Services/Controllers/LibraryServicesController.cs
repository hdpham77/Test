using CERS.Model;
using CERS.Web.Mvc;
using CERS.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using UPF;
using UPF.Core;

namespace CERS.EDT.Web.Services.Controllers
{
	public class LibraryServicesController : AppControllerBase
	{
		public ActionResult Index()
		{
			return View();
		}

		#region Violation Library

		public ActionResult ViolationLibrary( string violationTypeNumber = "" )
		{
			Server.ScriptTimeout = 600;
			//No role based authentication needed, just make sure they have a valid credential for Library services.
			var authResult = CERSSecurityManager.AuthenticateHttpAuthorizationHeader();
			if ( authResult.Result.Status == AuthenticationStatus.Success )
			{
				using ( EDTTransactionScope transScope = new EDTTransactionScope( authResult.Account, EDTEndpoint.ViolationLibraryQuery, Request.UserHostAddress, authenticationRequestID: authResult.EDTAuthenticationRequestID ) )
				{
					try
					{
						var xmlResult = transScope.Repository.ViolationTypes.Search( typeNumber: violationTypeNumber ).ToXml();
						transScope.WriteActivity( "Generated XML Package Successfully" );
						return XmlElement( xmlResult );
					}
					catch ( Exception ex )
					{
						transScope.WriteActivity( "Exception Occurred", ex );
						transScope.WriteMessage( "Exception Occurred. " + ex.Format(), EDTTransactionMessageType.Error );
						transScope.Complete( EDTTransactionStatus.ErrorProcessing );
						return HttpStatusCodeResult( HttpStatusCode.InternalServerError, ex.Message );
					}
				}
			}
			else
			{
				return HttpStatusCodeResult( HttpStatusCode.Unauthorized, authResult );
			}
		}

		#endregion Violation Library

		#region DataDictionary Library

		public ActionResult DataDictionaryLibrary( DataRegistryDataSourceType dictionary, string identifier )
		{
			Server.ScriptTimeout = 600;

			//No role based authentication needed, just make sure they have a valid credential for Library services.
			var authResult = CERSSecurityManager.AuthenticateHttpAuthorizationHeader();
			if ( authResult.Result.Status == AuthenticationStatus.Success )
			{
				using ( EDTTransactionScope transScope = new EDTTransactionScope( authResult.Account, EDTEndpoint.DataDictionaryQuery, Request.UserHostAddress, authenticationRequestID: authResult.EDTAuthenticationRequestID ) )
				{
					try
					{
						if ( !string.IsNullOrWhiteSpace( identifier ) )
						{
							transScope.WriteActivity( "Finding DataElement by Specific Identifier: \"" + identifier + "\"." );
						}
						else
						{
							transScope.WriteActivity( "Get All DataElements." );
						}

						var elements = DataRegistry.GetDataElements( dictionary, identifier );
						transScope.WriteActivity( "Fetched " + elements.Count + " Data Elements" );
						var xmlResult = elements.ToXml();
						transScope.WriteActivity( "Generated XML Package Successfully" );
						return XmlElement( xmlResult );
					}
					catch ( Exception ex )
					{
						transScope.WriteActivity( "Exception Occurred", ex );
						transScope.WriteMessage( "Exception Occurred. " + ex.Format(), EDTTransactionMessageType.Error );
						transScope.Complete( EDTTransactionStatus.ErrorProcessing );
						return HttpStatusCodeResult( HttpStatusCode.InternalServerError, ex.Message );
					}
				}
			}
			else
			{
				return HttpStatusCodeResult( HttpStatusCode.Unauthorized, authResult );
			}
		}

		#endregion DataDictionary Library

		#region Chemical Library

		public ActionResult ChemicalLibrary( string identifier )
		{
			Server.ScriptTimeout = 600;
			//No role based authentication needed, just make sure they have a valid credential for Library services.
			var authResult = CERSSecurityManager.AuthenticateHttpAuthorizationHeader();
			if ( authResult.Result.Status == AuthenticationStatus.Success )
			{
				using ( EDTTransactionScope transScope = new EDTTransactionScope( authResult.Account, EDTEndpoint.ChemicalLibraryQuery, Request.UserHostAddress, authenticationRequestID: authResult.EDTAuthenticationRequestID ) )
				{
					try
					{
						if ( !string.IsNullOrWhiteSpace( identifier ) )
						{
							List<Chemical> chemicals = new List<Chemical>();
							Chemical chemical = null;

							//1.) Check one.  Check cerskey
							//Guid key = new Guid(identifier);
							chemical = transScope.Repository.Chemicals.GetByCCLID( identifier );

							//2.) Check two. Check CAS
							if ( chemical == null )
							{
								chemical = transScope.Repository.Chemicals.GetByCAS( identifier );

								if ( chemical == null )
								{
									//3.) Check 3. Check USEpaSRSNumber
									chemical = transScope.Repository.Chemicals.GetByUSEPASRSNumber( identifier );
								}
							}

							if ( chemical != null )
							{
								chemicals.Add( chemical );
							}

							var xmlResult = chemicals.ToXml();
							transScope.WriteActivity( "Generated XML Package Successfully" );
							return XmlElement( xmlResult );
						}
						else
						{
							var xmlResult = transScope.Repository.Chemicals.Search().ToXml();
							transScope.WriteActivity( "Generated XML Package Successfully" );
							return XmlElement( xmlResult );
						}
					}
					catch ( Exception ex )
					{
						transScope.WriteActivity( "Exception Occurred", ex );
						transScope.WriteMessage( "Exception Occurred. " + ex.Format(), EDTTransactionMessageType.Error );
						transScope.Complete( EDTTransactionStatus.ErrorProcessing );
						return HttpStatusCodeResult( HttpStatusCode.InternalServerError, ex.Message );
					}
				}
			}
			else
			{
				return HttpStatusCodeResult( HttpStatusCode.Unauthorized, authResult );
			}
		}

		#endregion Chemical Library
	}
}