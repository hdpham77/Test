using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CERS.Web.UI.Organization.Controllers
{
    public class SubmittalElementSubmittalController : AppControllerBase
    {

        public ActionResult FacilityInfoIndex(int organizationId, int CERSID, int submittalId)
        {
            return View();
        }

        public ActionResult FacilityInfoHome(int organizationId, int CERSID, int submittalId, int SEID)
        {
            return View();
        }

        [HttpPost]
        public ActionResult FacilityInfoHome(int organizationId, int CERSID, int submittalId, int SEID, string action)
        {
            ViewBag.Posted = "Yes";
            ViewBag.UriString = organizationId + "." + CERSID + "." + submittalId + "." + SEID;
            return View();
        }

        public ActionResult FacilityInfoBizActivityIndex(int organizationId, int CERSID, int submittalId, int SEID)
        {
            return View();
        }

        public ActionResult FacilityInfoBizActivityHome(int organizationId, int CERSID, int submittalId, int SEID, int BZAID)
        {
            return View();
        }
    }
}