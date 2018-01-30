using CERS.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CERS.Web.UI.Organization.Controllers
{
	[NonContextualAuthorize( PermissionRole.SystemAdmin )]
	public class AdminSystemController : AppControllerBase
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult ReloadAppCache()
		{
			return View();
		}

		public JsonResult ReloadAppCache_Async()
		{
			//reload CDR stuff.
			DataRegistry.ReloadCache();
			Repository.SystemLookupTables.RebuildCache();

			var result = new
			{
				Success = true
			};

			return Json( result, JsonRequestBehavior.AllowGet );
		}
	}
}