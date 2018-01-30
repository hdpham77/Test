using CERS.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CERS.Web.UI.Organization.Controllers
{
	[NonContextualAuthorize( PermissionRole.SystemAdmin, PermissionRole.HelpAdmin, PermissionRole.UPEditor, PermissionRole.UPViewer )]
	public class AdminHomeController : AppControllerBase
	{
		public ActionResult Index()
		{
			return View();
		}
	}
}