using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CERS.ViewModels.EDT;

namespace CERS.EDT.Web.Services.Controllers
{
	public class HomeController : AppControllerBase
	{
		public ActionResult Index()
		{
			var viewModel = new EDTViewModel
			{
				EndpointParameters = Repository.EDTEndpoints.GetParameters(),
				Endpoints = Repository.EDTEndpoints.Search().GroupBy(e => e.Acronym).Select(g => g.First())
			};

			return View(viewModel);
		}

		public ActionResult EndpointDetail(string id)
		{
			var viewModel = new EDTViewModel
			{
				EndpointParameters = Repository.EDTEndpoints.GetParameters(),
				Endpoint = Repository.EDTEndpoints.Search().FirstOrDefault(p => p.Acronym == id),
				Endpoints = Repository.EDTEndpoints.Search().Where(p => p.Acronym == id)
			};
			
			return View(viewModel);
		}
	}
}
