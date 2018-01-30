using CERS.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using UPF.Web.Mvc;

namespace CERS.EDT.Web.Services.Controllers
{
	public class AppControllerBase : UPF.Web.Mvc.ControllerBase
	{
		private ICERSRepositoryManager _DataRepository;

		public ICERSRepositoryManager Repository
		{
			get
			{
				if ( _DataRepository == null )
				{
					_DataRepository = ServiceLocator.GetRepositoryManager( CurrentAccountID );
				}
				return _DataRepository;
			}
		}

		protected virtual HttpStatusCodeResult HttpStatusCodeResult( HttpStatusCode statusCode, HttpAuthorizationHeaderAuthenticationResult authResult )
		{
			string description = authResult.Result.Status.ToString();
			if ( authResult.Result.Status == UPF.Core.AuthenticationStatus.Missing_AuthorizationHeader )
			{
				description = "Missing Authorization Header";
			}

			return HttpStatusCodeResult( statusCode, description );
		}
	}
}