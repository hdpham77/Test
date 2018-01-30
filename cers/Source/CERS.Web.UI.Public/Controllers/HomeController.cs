using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using CERS.Model;

using CERS.Repository;
using CERS.ViewModels;
using CERS.ViewModels.Regulators;
using CERS.Web.UI.Public.ViewModels;
using Telerik.Web.Mvc;

namespace CERS.Web.UI.Public.Controllers
{
	public class HomeController : AppControllerBase
	{
		public ActionResult Index()
		{
			RegulatorViewModel regulatorViewModel = new RegulatorViewModel();
			regulatorViewModel.Counties = Repository.Counties.GetAll();
			return View(regulatorViewModel);
		}

	}
}