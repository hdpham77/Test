using CERS.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using UPF;
using UPF.Core;
using UPF.Web.Mvc;

namespace CERS.EDT.Web.Services.Controllers
{
	public class RegulatorFacilitySubmittalServicesController : AppControllerBase
	{
		#region Index Method

		/// <summary>
		/// ~/Regulator/FacilitySubmittals/Help
		/// </summary>
		/// <returns></returns>
		public ActionResult Index()
		{
			return View();
		}

		#endregion Index Method

		#region QueryXml EndPoint

		public ActionResult QueryXml( int regulatorCode )
		{
			//Authenticate
			var authResult = CERSSecurityManager.AuthenticateHttpAuthorizationHeaderForRegulator( regulatorCode, EDTEndpoint.RegulatorFacilitySubmittalQuery, PermissionRole.EDTFacilitySubmittalQuery );
			if ( authResult.IsAuthenticatedAndAuthorized )
			{
				XElement xmlResult = null;

				//Begin Transaction
				using ( EDTTransactionScope transScope = new EDTTransactionScope( authResult.Account, EDTEndpoint.RegulatorFacilitySubmittalQuery, Request.UserHostAddress, authenticationRequestID: authResult.EDTAuthenticationRequestID ) )
				{
					try
					{
						//build of the argument bag.
						var args = new RegulatorFacilitySubmittalQueryArguments( Request.QueryString );
						args.RegulatorCode = regulatorCode;

						//intialize the processor to handle this EDT transaction for this Data Flow.
						var processor = new RegulatorFacilitySubmittalQueryAdapter( transScope );

						//get the XML back.
						xmlResult = processor.Process( args );

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

		#endregion QueryXml EndPoint

		#region ActionNotification EndPoint

		[HttpPost]
		public ActionResult ActionNotification( int regulatorCode )
		{
			//Authenticate
			var authResult = CERSSecurityManager.AuthenticateHttpAuthorizationHeaderForRegulator( regulatorCode, EDTEndpoint.RegulatorFacilitySubmittalActionNotification, PermissionRole.EDTFacilitySubmittalActionNotification );
			if ( authResult.IsAuthenticatedAndAuthorized )
			{
				XElement xmlResult = null;

				//Begin Transaction
				using ( EDTTransactionScope transScope = new EDTTransactionScope( authResult.Account, EDTEndpoint.RegulatorFacilitySubmittalActionNotification, Request.UserHostAddress, authenticationRequestID: authResult.EDTAuthenticationRequestID ) )
				{
					try
					{
						RegulatorFacilitySubmittalActionNotificationAdapter processor = new RegulatorFacilitySubmittalActionNotificationAdapter( transScope );
						xmlResult = processor.Process( Request.InputStream, regulatorCode );
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

		#endregion ActionNotification EndPoint

		#region Document EndPoint

		public ActionResult Document( Guid uniqueKey )
		{
			var authResult = CERSSecurityManager.AuthenticateHttpAuthorizationHeader();
			if ( authResult.IsAuthenticatedAndAuthorized )
			{
				byte[] docData = null;
				bool docNotFound = false;
				bool docUniqueKeyNotFound = false;
				bool docPhysicallyNotFound = false;

				//Begin Transaction
				using ( EDTTransactionScope transScope = new EDTTransactionScope( authResult.Account, EDTEndpoint.RegulatorFacilitySubmittalDocumentQuery, Request.UserHostAddress, authenticationRequestID: authResult.EDTAuthenticationRequestID ) )
				{
					try
					{
						var dataModel = transScope.Repository.DataModel;
						var doc = dataModel.FacilitySubmittalElementDocuments.SingleOrDefault( p => p.Key == uniqueKey && !p.Voided );
						if ( doc != null )
						{
                            transScope.WriteActivity( string.Format( "FacilitySubmittalElementDocument CERSUniqueKey {0} metadata found.", uniqueKey ) );
							var edtAuthorizationResult = CERSSecurityManager.EDTAuthorize( authResult.Account, doc.FacilitySubmittalElementResourceDocument.Resource.FacilitySubmittalElement.CERSID, PermissionRole.EDTFacilitySubmittalQuery );
							transScope.Connect( edtAuthorizationResult.RegulatorID );

							if ( edtAuthorizationResult.Authorized )
							{
								if ( doc.Document != null )
								{
									transScope.WriteActivity( "Document metadata found." );
									docData = DocumentStorage.GetBytes( doc.Document.Location );
									if ( docData != null )
									{
                                        transScope.WriteActivity( string.Format( "Document physically found in storage file size {0}kb.", doc.FileSize ) );
										transScope.Complete( EDTTransactionStatus.Accepted );
									}
									else
									{
										docPhysicallyNotFound = true;
										transScope.WriteMessage( "Document NOT physically found in storage.", EDTTransactionMessageType.Error );
										transScope.Complete( EDTTransactionStatus.Rejected );
									}
								}
								else
								{
									transScope.WriteMessage( "Document metadata not found.", EDTTransactionMessageType.Error );
									transScope.Complete( EDTTransactionStatus.Rejected );
									docNotFound = true;
								}
							}
							else
							{
								string errorMessage = "Account is not authorized to download this document. The account is not affiliated with any regulators associated with this Facility, or the account does not have sufficient rights.";
								transScope.SetStatus( EDTTransactionStatus.Rejected );
								transScope.WriteActivity( errorMessage );
								return HttpStatusCodeResult( HttpStatusCode.Unauthorized, errorMessage );
							}
						}
						else
						{
							transScope.WriteActivity( "Document Unique Key Not Found" );
							transScope.Complete( EDTTransactionStatus.Rejected );
							docUniqueKeyNotFound = true;
						}
					}
					catch ( Exception ex )
					{
						transScope.WriteActivity( "Exception Occurred.", ex );
						transScope.WriteMessage( "Exception Occurred. " + ex.Format(), EDTTransactionMessageType.Error );
						return HttpStatusCodeResult( HttpStatusCode.InternalServerError, ex.Format() );
					}
				}

				if ( docNotFound )
				{
					return HttpStatusCodeResult( HttpStatusCode.NotFound, "Document for UniqueKey '" + uniqueKey + "' was not found." );
				}
				else if ( docUniqueKeyNotFound )
				{
					return HttpStatusCodeResult( HttpStatusCode.NotFound, "A Document does not exists with the UniqueKey '" + uniqueKey + "'." );
				}
				else if ( docPhysicallyNotFound )
				{
					return HttpStatusCodeResult( HttpStatusCode.NotFound, "The document's metadata exists, but the physical document (filesystem) was not found." );
				}
				else
				{
					return File( docData, "application/octet-stream" );
				}
			}
			else
			{
				return HttpStatusCodeResult( HttpStatusCode.Unauthorized, authResult );
			}
		}

		#endregion Document EndPoint

		#region Submit Method

		public ActionResult Submit( int regulatorCode )
		{
			//Authenticate
			var authResult = CERSSecurityManager.AuthenticateHttpAuthorizationHeaderForRegulator( regulatorCode, EDTEndpoint.RegulatorFacilitySubmittalSubmit, PermissionRole.EDTFacilitySubmittalSubmit );
			if ( authResult.IsAuthenticatedAndAuthorized )
			{
				XElement xmlResult = null;

				//Begin Transaction
				using ( EDTTransactionScope transScope = new EDTTransactionScope( authResult.Account, EDTEndpoint.RegulatorFacilitySubmittalSubmit, Request.UserHostAddress, authenticationRequestID: authResult.EDTAuthenticationRequestID ) )
				{
					try
					{
						RegulatorFacilitySubmittalAdapter adapter = new RegulatorFacilitySubmittalAdapter( transScope );
						xmlResult = adapter.Process( Request.InputStream, regulatorCode );

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

		#endregion Submit Method
	}
}