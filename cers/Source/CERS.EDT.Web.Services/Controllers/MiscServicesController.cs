using CERS.Web.Mvc;
using CERS.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using UPF.Core;

namespace CERS.EDT.Web.Services.Controllers
{
	public class MiscServicesController : AppControllerBase
	{
		public ActionResult EndpointMetadata()
		{
			EndpointMetadataXmlSerializer serializer = new EndpointMetadataXmlSerializer( Repository );
			return XmlElement( serializer.Serialize() );
		}
	}
}