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
	public class RegulatorServicesController : AppControllerBase
	{
		#region Index Method

		/// <summary>
		/// ~/Regulator/Help
		/// </summary>
		/// <returns></returns>
		public ActionResult Index()
		{
			return View();
		}

		#endregion Index Method

		#region Authenticate Method

		public ActionResult Authenticate( int regulatorCode )
		{
			//No role based authentication needed, just make sure they have a valid credential for Library services.
			var authResult = CERSSecurityManager.AuthenticateHttpAuthorizationHeaderForRegulator( regulatorCode, EDTEndpoint.RegulatorAuthenticationTest, PermissionRole.EDTLibraryServices );
			if ( authResult.Result.Status == AuthenticationStatus.Success )
			{
				using ( EDTTransactionScope transScope = new EDTTransactionScope( authResult.Account, EDTEndpoint.RegulatorAuthenticationTest, Request.UserHostAddress, authenticationRequestID: authResult.EDTAuthenticationRequestID ) )
				{
					//return Content("<Result><Status>Succeeded</status><Message>Authentication Succeeded - " + authResult.Account.DisplayName + "</Message></Result>");
					return HttpStatusCodeResult( System.Net.HttpStatusCode.OK, "Authentication Succeeded" );
				}
			}
			else
			{
				return HttpStatusCodeResult( HttpStatusCode.Unauthorized, authResult );
			}
		}

		#endregion Authenticate Method

		#region FacilityMetadataSubmit Method

		[HttpPost]
		public ActionResult FacilityMetadata( int regulatorCode )
		{
			//Authenticate
			var authResult = CERSSecurityManager.AuthenticateHttpAuthorizationHeaderForRegulator( regulatorCode, EDTEndpoint.RegulatorFacilityMetadataSubmit, PermissionRole.EDTFacilityMetadataSubmit );
			if ( authResult.IsAuthenticatedAndAuthorized )
			{
				XElement xmlResult = null;

				//Begin Transaction
				using ( EDTTransactionScope transScope = new EDTTransactionScope( authResult.Account, EDTEndpoint.RegulatorFacilityMetadataSubmit, Request.UserHostAddress, authenticationRequestID: authResult.EDTAuthenticationRequestID ) )
				{
					try
					{
						RegulatorFacilityMetadataAdapter adapter = new RegulatorFacilityMetadataAdapter( transScope );
						xmlResult = adapter.Process( Request.InputStream, regulatorCode );
					}
					catch ( Exception ex )
					{
						transScope.WriteMessage( "Exception Occurred. " + ex.Format(), EDTTransactionMessageType.Error );
						transScope.Complete( EDTTransactionStatus.ErrorProcessing );
						return HttpStatusCodeResult( HttpStatusCode.InternalServerError, ex.Format() );
					}
				}

				return XmlElement( xmlResult );
			}
			else
			{
				return HttpStatusCodeResult( HttpStatusCode.Unauthorized, authResult );
			}
		}

		#endregion FacilityMetadataSubmit Method

		#region FacilityQuery Method

		public ActionResult FacilityQuery( int regulatorCode )
		{
			//Authenticate
			var authResult = CERSSecurityManager.AuthenticateHttpAuthorizationHeaderForRegulator( regulatorCode, EDTEndpoint.RegulatorFacilityQuery, PermissionRole.EDTFacilityQuery );
			if ( authResult.IsAuthenticatedAndAuthorized )
			{
				XElement xmlResult = null;

				//Begin Transaction
				using ( EDTTransactionScope transScope = new EDTTransactionScope( authResult.Account, EDTEndpoint.RegulatorFacilityQuery, Request.UserHostAddress, authenticationRequestID: authResult.EDTAuthenticationRequestID ) )
				{
					try
					{
						//build of the argument bag.
						var args = new RegulatorFacilityQueryArguments( Request.QueryString );
						args.RegulatorCode = regulatorCode;

						var adapter = new RegulatorFacilityQueryAdapter( transScope );

						////get the XML back.
						xmlResult = adapter.Process( args );

						//write log
						transScope.WriteActivity( "Successfully Generated XML Package" );
					}
					catch ( Exception ex )
					{
						transScope.WriteActivity( "Exception Occurred.", ex );
						transScope.WriteMessage( "Exception OCcurred. " + ex.Format(), EDTTransactionMessageType.Error );
						return HttpStatusCodeResult( HttpStatusCode.InternalServerError, ex.Format() );
					}
				}

				return XmlElement( xmlResult );
			}
			else
			{
				return HttpStatusCodeResult( HttpStatusCode.Unauthorized, authResult );
			}
		}

		#endregion FacilityQuery Method

		#region FacilityCreate Method

		[HttpPost]
		public ActionResult FacilityCreate( int regulatorCode )
		{
			//Authenticate
			var authResult = CERSSecurityManager.AuthenticateHttpAuthorizationHeaderForRegulator( regulatorCode, EDTEndpoint.RegulatorFacilityCreate, PermissionRole.EDTFacilityCreate );
			if ( authResult.IsAuthenticatedAndAuthorized )
			{
				XElement xmlResult = null;

				//Begin Transaction
				using ( EDTTransactionScope transScope = new EDTTransactionScope( authResult.Account, EDTEndpoint.RegulatorFacilityCreate, Request.UserHostAddress, authenticationRequestID: authResult.EDTAuthenticationRequestID ) )
				{
					try
					{
						RegulatorFacilityCreateAdapter adapter = new RegulatorFacilityCreateAdapter( transScope );
						xmlResult = adapter.Process( Request.InputStream, regulatorCode );
					}
					catch ( Exception ex )
					{
						transScope.WriteMessage( "Exception Occurred. " + ex.Format(), EDTTransactionMessageType.Error );
						transScope.Complete( EDTTransactionStatus.ErrorProcessing );
						return HttpStatusCodeResult( HttpStatusCode.InternalServerError, ex.Format() );
					}
				}

				return XmlElement( xmlResult );
			}
			else
			{
				return HttpStatusCodeResult( HttpStatusCode.Unauthorized, authResult );
			}
		}

		#endregion FacilityCreate Method

		#region OrganizationQuery Method

		public ActionResult OrganizationQuery( int regulatorCode )
		{
			//Authenticate
			var authResult = CERSSecurityManager.AuthenticateHttpAuthorizationHeaderForRegulator( regulatorCode, EDTEndpoint.RegulatorOrganizationQuery, PermissionRole.EDTOrganizationQuery );
			if ( authResult.IsAuthenticatedAndAuthorized )
			{
				XElement xmlResult = null;

				//Begin Transaction
				using ( EDTTransactionScope transScope = new EDTTransactionScope( authResult.Account, EDTEndpoint.RegulatorOrganizationQuery, Request.UserHostAddress, authenticationRequestID: authResult.EDTAuthenticationRequestID ) )
				{
					try
					{
						//build of the argument bag.
						var args = new RegulatorOrganizatonQueryAdapterArguments( Request.QueryString );
						args.RegulatorCode = regulatorCode;

						var adapter = new RegulatorOrganizationQueryAdapter( transScope );

						////get the XML back.
						xmlResult = adapter.Process( args );

						//write log
						transScope.WriteActivity( "Successfully Generated XML Package" );
					}
					catch ( Exception ex )
					{
						transScope.WriteActivity( "Exception Occurred.", ex );
						transScope.WriteMessage( "Exception OCcurred. " + ex.Format(), EDTTransactionMessageType.Error );
						return HttpStatusCodeResult( HttpStatusCode.InternalServerError, ex.Format() );
					}
				}

				return XmlElement( xmlResult );
			}
			else
			{
				return HttpStatusCodeResult( HttpStatusCode.Unauthorized, authResult );
			}
		}

		#endregion OrganizationQuery Method

		#region Listing Method

		public ActionResult Listing()
		{
			XElement data = null;
			using ( ICERSRepositoryManager repo = ServiceLocator.GetRepositoryManager() )
			{
				var regulators = repo.Regulators.Search().OrderBy( p => p.EDTIdentityKey );
				data = regulators.ToXml();
			}
			return XmlElement( data );
		}

		#endregion Listing Method

		public ActionResult ActionItemQuery( int regulatorCode )
		{
			//Authenticate
			var authResult = CERSSecurityManager.AuthenticateHttpAuthorizationHeaderForRegulator( regulatorCode, EDTEndpoint.RegulatorActionItemQuery, PermissionRole.EDTActionItemQuery );
			if ( authResult.IsAuthenticatedAndAuthorized )
			{
				XElement xmlResult = null;

				//Begin Transaction
				using ( EDTTransactionScope transScope = new EDTTransactionScope( authResult.Account, EDTEndpoint.RegulatorActionItemQuery, Request.UserHostAddress, authenticationRequestID: authResult.EDTAuthenticationRequestID ) )
				{
					try
					{
						//build of the argument bag.
						var args = new RegulatorActionItemQueryArguments( Request.QueryString );
						args.RegulatorCode = regulatorCode;

						RegulatorActionItemQueryAdapter adapter = new RegulatorActionItemQueryAdapter( transScope );
						xmlResult = adapter.Process( args );
					}
					catch ( Exception ex )
					{
						transScope.WriteMessage( "Exception Occurred. " + ex.Format(), EDTTransactionMessageType.Error );
						transScope.Complete( EDTTransactionStatus.ErrorProcessing );
						return HttpStatusCodeResult( HttpStatusCode.InternalServerError, ex.Format() );
					}
				}

				return XmlElement( xmlResult );
			}
			else
			{
				return HttpStatusCodeResult( HttpStatusCode.Unauthorized, authResult );
			}
		}

		public ActionResult FacilityTransferQuery( int regulatorCode )
		{
			//Authenticate

			var authResult = CERSSecurityManager.AuthenticateHttpAuthorizationHeaderForRegulator( regulatorCode, EDTEndpoint.RegulatorFacilityTransferQuery, PermissionRole.EDTFacilityTransferQuery );
			if ( authResult.IsAuthenticatedAndAuthorized )
			{
				XElement xmlResult = null;

				//Begin Transaction
				using ( EDTTransactionScope transScope = new EDTTransactionScope( authResult.Account, EDTEndpoint.RegulatorFacilityTransferQuery, Request.UserHostAddress, authenticationRequestID: authResult.EDTAuthenticationRequestID ) )
				{
					try
					{
						//build of the argument bag.
						var args = new RegulatorFacilityTransferQueryArguments( Request.QueryString );
						args.RegulatorCode = regulatorCode;

						RegulatorFacilityTransferQueryAdapter adapter = new RegulatorFacilityTransferQueryAdapter( transScope );
						xmlResult = adapter.Process( args );
					}
					catch ( Exception ex )
					{
						transScope.WriteMessage( "Exception Occurred. " + ex.Format(), EDTTransactionMessageType.Error );
						transScope.Complete( EDTTransactionStatus.ErrorProcessing );
						return HttpStatusCodeResult( HttpStatusCode.InternalServerError, ex.Format() );
					}
				}

				return XmlElement( xmlResult );
			}
			else
			{
				return HttpStatusCodeResult( HttpStatusCode.Unauthorized, authResult );
			}
		}
	}
}