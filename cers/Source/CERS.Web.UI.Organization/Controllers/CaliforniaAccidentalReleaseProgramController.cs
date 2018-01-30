using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CERS.Web.UI.Organization.Controllers
{
	[Authorize]
	public class CaliforniaAccidentalReleaseProgramController : SubmittalElementControllerBase
	{
		//
  // GET: /CaliforniaAccidentalReleaseProgram/

		public ActionResult Index()
		{
			return View();
		}
	}
}