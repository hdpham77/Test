using CERS.Guidance;
using CERS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;

namespace CERS.Web.UI.Organization.ViewModels
{
	public class LandingPageViewModel
	{
		public CurrentSubmittalElementViewModel CurrentSEViewModel { get; set; }

		public Facility Facility { get; set; }

		public FacilitySubmittalElement FacilitySubmittalElement { get; set; }

		public FacilitySubmittalElementResource FacilitySubmittalElementResource { get; set; }

		public List<FacilitySubmittalElement> FacilitySubmittalElements { get; set; }

		public List<GuidanceMessage> GuidanceMessages { get; set; }

		public string Instructions { get; set; }

		public bool PreviouslyRespondedToSurvey { get; set; }

		public Survey Survey { get; set; }

		public IEnumerable<WhatsNextItem> WhatsNext { get; set; }
	}
}