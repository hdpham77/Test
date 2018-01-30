using CERS.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using UPF;

namespace CERS.EDT.Web.Services.Controllers
{
	public class CMESubmittalServicesController : AppControllerBase
	{
		public ActionResult Index()
		{
			return View();
		}

		/// <summary>
		/// Based on Regulator Code and CMESubmittalExportArguments, method generates and
		/// returns a instance of CMESubmittalExport XML containing CME Entities matching
		/// the provided filters.
		/// </summary>
		/// <param name="regulatorCode"></param>
		/// <returns></returns>
		public ActionResult Query( int regulatorCode )
		{
			//Authenticate
			var authResult = CERSSecurityManager.AuthenticateHttpAuthorizationHeaderForRegulator( regulatorCode, EDTEndpoint.RegulatorCMEQuery, PermissionRole.EDTCMESubmittalQuery );
			if ( authResult.IsAuthenticatedAndAuthorized )
			{
				XElement xmlResult = null;

				//Begin Transaction
				using ( EDTTransactionScope transScope = new EDTTransactionScope( authResult.Account, EDTEndpoint.RegulatorFacilitySubmittalQuery, Request.UserHostAddress, authenticationRequestID: authResult.EDTAuthenticationRequestID ) )
				{
					try
					{
						//build of the argument bag.
						var args = new RegulatorCMEQueryArguments( Request.QueryString );
						args.RegulatorCode = regulatorCode;

						//intialize the processor to handle this EDT transaction for this Data Flow.
						IOutboundAdapter<RegulatorCMEQueryArguments> processor = new RegulatorCMEQueryAdapter( transScope );

						//get the XML back.
						xmlResult = processor.Process( args );

						//write log
						transScope.WriteActivity( "Successfully Generated XML Package" );
					}
					catch ( Exception ex )
					{
						transScope.WriteActivity( "Exception Occurred.", ex );
						transScope.WriteMessage( "Exception OCcurred. " + ex.Format(), EDTTransactionMessageType.Error );
						return HttpStatusCodeResult( HttpStatusCode.InternalServerError, ex.Message );
					}
				}

				return XmlElement( xmlResult );
			}
			else
			{
				return HttpStatusCodeResult( HttpStatusCode.Unauthorized, authResult );
			}
		}

		/// <summary>
		/// Method processes a instance of CMESubmittal XML passed through the Request.InputStream
		/// and returns a CMESubmittalResponse.  This is the CMESubmittal "Submit" End Point.
		/// </summary>
		/// <param name="regulatorCode"></param>
		/// <returns></returns>
		public ActionResult Submit( int regulatorCode )
		{
			// Authenticate
			// TODO: For Authenticating CMESubmittal Rights, is having UPAInspector Rights for the specified Regulator Sufficient?
			var authResult = CERSSecurityManager.AuthenticateHttpAuthorizationHeaderForRegulator( regulatorCode, EDTEndpoint.RegulatorCMESubmit, PermissionRole.EDTCMESubmittalSubmit );
			if ( authResult.IsAuthenticatedAndAuthorized )
			{
				XElement xmlResult = null;

				//Begin Transaction
				using ( EDTTransactionScope transScope = new EDTTransactionScope( authResult.Account, EDTEndpoint.RegulatorCMESubmit, Request.UserHostAddress, authenticationRequestID: authResult.EDTAuthenticationRequestID ) )
				{
					try
					{
						RegulatorCMESubmitAdapter processor = new RegulatorCMESubmitAdapter( transScope );
						xmlResult = processor.Process( Request.InputStream, null );
					}
					catch ( Exception ex )
					{
						transScope.WriteActivity( "Exception Occurred.", ex );
						transScope.WriteMessage( "Exception Occurred. " + ex.Format(), EDTTransactionMessageType.Error );
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