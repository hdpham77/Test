using CERS.Model;
using CERS.ViewModels;
using CERS.ViewModels.AccountManagement;
using CERS.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using UPF;
using UPF.Core;
using UPF.Core.Cryptography;
using UPF.Core.Model;
using UPF.Core.Repository;
using UPF.Web.Mvc.UI;

namespace CERS.Web.UI.Organization.Controllers
{
	public class AccountController : AccountControllerBase
	{
		#region RedirectFromSignInPage Method

		protected override ActionResult RedirectFromSignInPage( Account account, string actionName = "SelectOrganization", string controllerName = "Home", string returnUrl = null )
		{
			var stat = Repository.ContactStatistics.EnsureExists( account );
			ActionResult result = null;
			//RedirectToRouteResult returnRoute =
			if ( stat.UALastShown != null )
			{
				TimeSpan diff = DateTime.Now - stat.UALastShown.Value;
				if ( diff.Days >= 30 )
				{
					result = RedirectToRoute( CommonRoute.UserAgreement, new { returnUrl = returnUrl } );
				}
				else
				{
					result = RedirectToOrganizationPortalSwitchboard( account, returnUrl );
				}
			}
			else
			{
				result = RedirectToRoute( CommonRoute.UserAgreement, new { returnUrl = returnUrl } );
			}
			return result;
		}

		#endregion RedirectFromSignInPage Method

		#region EmailHistory

		[Authorize]
		public ActionResult EmailHistory()
		{
			var account = Repository.CoreData.Accounts.GetByID( CurrentAccountID );
			var contact = Repository.Contacts.GetByAccount( CurrentAccountID );
			var viewModel = new EmailViewModel<UPF.Core.Model.Account>()
			{
				Entity = account,
                ControllerName = "Account",
                ActionMethodName = "EmailHistory_Grid",
                GridReadData = "AccountEmailHistoryGrid_ReadData",
            };
			return View( viewModel );
		}

        public JsonResult EmailHistory_Grid( [DataSourceRequest]DataSourceRequest request, string facilityName = null, int? CERSID = null, DateTime? begin = null, DateTime? end = null )
        {
            var contact = Repository.Contacts.GetByAccount( CurrentAccountID );
            var emails = Repository.Emails.GridSearch( accountID:CurrentAccountID, contactID:contact.ID, facilityName:facilityName, CERSID:CERSID, begin:begin, end:end );

            return Json( emails.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

		#endregion EmailHistory

		#region Notifications

		[GridAction]
		public ActionResult Notifications_Async( int? maxCount = null, int? typeID = null, int? priorityID = null, int? occurredOnInLastDays = null )
		{
			EventPriority? priority = null;
			if ( priorityID != null )
			{
				priority = (EventPriority)priorityID.Value;
			}
			var notifications = Repository.Events.GetNotificationsForAccountGridSearch(
				CurrentAccount,
				Context.Organization,
				includeAccountEvents: true,
				includeContactEvents: true,
				priority: priority,
				occurredOnLastNumberOfDays: occurredOnInLastDays,
				baseImageContentUrl: Url.Content( "~/Content/Images/Circles/" )
				)
				.ToList();

			var results = from n in notifications select n;

			if ( maxCount != null )
			{
				results = results.Take( maxCount.Value );
			}

			if ( typeID != null )
			{
				var type = (EventTypeCode)typeID;
				results = from n in results where n.EventType == type select n;
			}

			notifications = results.OrderByDescending( p => p.OccurredOn ).ThenBy( p => p.Priority ).ToList();

			return View( new GridModel( notifications ) );
		}

		#endregion Notifications
	}
}