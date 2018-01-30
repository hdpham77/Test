using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CERS.Model;
using CERS.Web.Mvc;
using CERS.Web.Mvc.ViewModels;
using CERS.Web.UI.Organization.ViewModels;

namespace CERS.Web.UI.Organization.Controllers
{
	[VerifyCERSID(true)]
	public class ErrorController : ErrorControllerBase
	{
		public ActionResult CERSIDNotFound(int CERSID)
		{
			CERSIDInputValidationErrorViewModel viewModel = new CERSIDInputValidationErrorViewModel
			{
				CERSID = CERSID
			};
			return View(viewModel);
		}

		public ActionResult CERSIDDeleted(int CERSID)
		{
			CERSIDInputValidationErrorViewModel viewModel = new CERSIDInputValidationErrorViewModel
			{
				CERSID = CERSID
			};

			//get the Facility
			Facility facility = Repository.Facilities.GetByID(CERSID, true);
			viewModel.Facility = facility;

			//find a delete event.
			viewModel.RelatedEvent = Repository.Events.Search(CERSID: CERSID, completed: true, eventTypeCode: EventTypeCode.FacilityDeleteRequestAccepted).FirstOrDefault();
			if (viewModel.RelatedEvent == null)
			{
				viewModel.RelatedEvent = Repository.Events.Search(CERSID: CERSID, completed: true, eventTypeCode: EventTypeCode.FacilityDeletedByRegulator).FirstOrDefault();
			}

			return View(viewModel);
		}

		public ActionResult CERSIDOrganizationIDMismatch(int CERSID, int organizationID)
		{
			CERSIDInputValidationErrorViewModel viewModel = new CERSIDInputValidationErrorViewModel
			{
				CERSID = CERSID,
				OrganizationID = organizationID
			};

			viewModel.Organization = Repository.Organizations.GetByID(organizationID);
			Facility facility = Repository.Facilities.GetByID(CERSID, true);
			viewModel.Facility = facility;

			//look for transfer accepted request
			viewModel.RelatedEvent = Repository.Events.Search(CERSID: CERSID, completed: true, eventTypeCode: EventTypeCode.FacilityTransferRequestAccepted).FirstOrDefault();
			if (viewModel.RelatedEvent == null)
			{
				viewModel.RelatedEvent = Repository.Events.Search(CERSID: CERSID, completed: true, eventTypeCode: EventTypeCode.FacilityTransferredByRegulator).FirstOrDefault();
			}

			return View(viewModel);
		}

		public ActionResult DocumentNotFound()
		{
			return View();
		}

		public ActionResult OrganizationDeferredProcessingExists(int organizationID)
		{
			var dp = Repository.DeferredProcessingUpload.GetUnprocessedItemByOrganizationID(organizationID);
			DeferredProcessingViewModel dpViewModel = new DeferredProcessingViewModel();
			if (dp != null)
			{
				var account = Repository.Contacts.GetByAccount(dp.CreatedByID);
				dpViewModel.ID = dp.ID;
				dpViewModel.SubmittalElementName = dp.SubmittalElement.Name;
				if (account != null)
				{
					dpViewModel.SubmittedByName = account.FullName;
					dpViewModel.SubmittedByEmail = account.Email;
				}
				dpViewModel.SubmittedOn = dp.CreatedOn;

			};
			CERSIDInputValidationErrorViewModel viewModel = new CERSIDInputValidationErrorViewModel
			{
				OrganizationID = organizationID,
				Organization = Repository.Organizations.GetByID(organizationID),
				DeferredProcessing = dpViewModel,
			};

			return View(viewModel);
		}

	}
}