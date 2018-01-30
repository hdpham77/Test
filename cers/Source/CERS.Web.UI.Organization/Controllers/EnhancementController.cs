using CERS.Model;
using CERS.ViewModels.Enhancements;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using UPF;

namespace CERS.Web.UI.Organization.Controllers
{
	public class EnhancementController : AppControllerBase
	{
		//
		// GET: /Enhancement/

		#region Index

		//[Authorize] // authorize only in Admin portal
		public ActionResult Index()
		{
			EnhancementSearchViewModel viewModel = BuildSearchViewModel( null );
			return View( viewModel );
		}

        public JsonResult Index_Grid( [DataSourceRequest]DataSourceRequest request, int? statusID = null, int? assigneeID = null, string name = null, int? portalID = null, int? priorityID = null, string id = null, DateTime? updatedSince = null, DateTime? commentsSince = null )
        {
            // action for grid ajax when paging, etc
            var enhancements = from enhancement in Repository.Enhancements.Search( IsNull( statusID, 0 ), IsNull( assigneeID, 0 ), name, IsNull( portalID, 0 ), IsNull( priorityID, 0 ), id, updatedSince, commentsSince )
                               //where enhancement.ID != (int)EnhancementStatus.CERS3Proposed
                               select new EnhancementGridViewModel
                               {
                                   ID = enhancement.ID,
                                   Status = enhancement.Status.Name,
                                   Name = enhancement.Name,
                                   Portal = enhancement.Portal.Name,
                                   ImplementationTargetDate = enhancement.ImplementationTargetDate,
                                   //Assignee = enhancement.Assignee.Name,
                                   UpdatedOn = enhancement.UpdatedOn,
                                   NumberOfComments = enhancement.EnhancementUserComments.Count( c => !c.Voided )
                               };

            return Json( enhancements.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

		[Authorize]
		public ActionResult MyIndex()
		{
			EnhancementSearchViewModel viewModel = BuildMyCommentViewModel( null );
			return View( viewModel );
		}

        public JsonResult MyIndex_Grid( [DataSourceRequest]DataSourceRequest request )
        {
            var myEnhancements = from enhancement in Repository.Enhancements.SearchByCommentAccountID( Repository.ContextAccountID )
                                 select new EnhancementMyCommentGridViewModel
                                 {
                                     ID = enhancement.ID,
                                     Name = enhancement.Name,
                                     Portal = enhancement.Portal.Name,
                                     Comments = enhancement.EnhancementUserComments.LastOrDefault() != null ? enhancement.EnhancementUserComments.Last().Comments : string.Empty
                                 };

            return Json( myEnhancements.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

		private EnhancementSearchViewModel BuildMyCommentViewModel( EnhancementSearchViewModel viewModel )
		{
			if ( viewModel == null )
			{
				viewModel = new EnhancementSearchViewModel();
				viewModel.StatusID = 0;
				viewModel.AssigneeID = 0;
				viewModel.Name = null;
				viewModel.PortalID = 0;
				viewModel.ID = null;
				viewModel.UpdatedSince = null;
			}

			viewModel.Repository = Repository;
			viewModel.Entities = Repository.Enhancements.SearchByCommentAccountID( Repository.ContextAccountID );

			return viewModel;
		}

		private EnhancementSearchViewModel BuildSearchViewModel( EnhancementSearchViewModel viewModel )
		{
			if ( viewModel == null )
			{
				viewModel = new EnhancementSearchViewModel();
				viewModel.StatusID = 0;
				viewModel.AssigneeID = 0;
				viewModel.Name = null;
				viewModel.PortalID = 0;
				viewModel.ID = null;
				viewModel.UpdatedSince = null;
                viewModel.PriorityID = 0;
			}

			viewModel.Repository = Repository;

			viewModel.Statuses = ( from s in Repository.Enhancements.Statuses
                                   //where s.ID != (int)EnhancementStatus.CERS3Proposed
								   select new SelectListItem
								   {
									   Value = s.ID.ToString(),
									   Text = s.Name
								   } ).ToList();
			viewModel.Assignees = ( from a in Repository.Enhancements.Assignees
									select new SelectListItem
									{
										Value = a.ID.ToString(),
										Text = a.Name
									} ).ToList();
			viewModel.Portals = ( from p in Repository.Enhancements.Portals
								  select new SelectListItem
								  {
									  Value = p.ID.ToString(),
									  Text = p.Name
								  } ).ToList();
			return viewModel;
		}

		#endregion Index

		#region Edit

        //[Authorize]
        //[GridAction]
        //public ActionResult _Detail( string id = "0" )
        //{
        //    EnhancementDetailViewModel viewModel = new EnhancementDetailViewModel();
        //    viewModel.Repository = Repository;
        //    viewModel.Entities = Repository.EnhancementComments.Search( id.ToInt32() );
        //    return View( new GridModel( viewModel.GridView.ToList() ) );
        //}

		[Authorize]
		public ActionResult Add()
		{
			//if (CurrentAccountID > 0 && CurrentUserRoles.IsEnhancementAdmin)
			//{
			//    Enhancement enhancementModel = new Enhancement();
			//    EnhancementEditViewModel viewModel = BuildEditViewModel(null, enhancementModel);
			//    return View(viewModel);
			//}
			//else
			//{
			return RedirectToAction( "Index" );
			//}
		}

		[Authorize]
		[HttpPost]
		[ValidateInput( false )]
		public ActionResult Add( EnhancementEditViewModel enhancementEditViewModel )
		{
			//if (CurrentAccountID > 0 && CurrentUserRoles.IsEnhancementAdmin && enhancementEditViewModel != null)
			//{
			//    Repository.Enhancements.Create(enhancementEditViewModel.Enhancement);
			//    return RedirectToAction("Index");
			//}
			//else
			//{
			return RedirectToAction( "Index" );
			//}
		}

		public ActionResult Detail( int id )
		{
			EnhancementDetailViewModel viewModel = new EnhancementDetailViewModel();
			viewModel.Repository = Repository;
			viewModel.Enhancement = Repository.Enhancements.GetByID( id );
			return View( viewModel );
		}

        public JsonResult Detail_Grid( [DataSourceRequest]DataSourceRequest request, string id = "0" )
		{
            var comments = from comment in Repository.EnhancementComments.Search( id.ToInt32() )
                           select new EnhancementCommentGridViewModel
                         {
                             ID = comment.ID,
                             EnhancementID = comment.EnhancementID,
                             User = Repository.Enhancements.AccountName( comment.AccountID ),
                             Regulator = comment.Regulator == null ? String.Empty : comment.Regulator.NameAbbreviation,
                             Organization = comment.Organization == null ? String.Empty : comment.Organization.Name,
                             UserPriority = comment.UserPriority == null ? String.Empty : comment.UserPriority.Name,
                             UserLikes = comment.UserLikes,
                             Comments = Strings.Truncate( comment.Comments, 60, true ),
                             UpdatedOn = comment.UpdatedOn,
                             AccountID = comment.AccountID
                         };

            return Json( comments.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
		}


		[Authorize]
		public ActionResult Edit( int id )
		{
			return RedirectToAction( "Index" );
		}

		[Authorize]
		[HttpPost]
		[ValidateInput( false )]
		public ActionResult Edit( EnhancementEditViewModel enhancementEditViewModel )
		{
			//if (CurrentAccountID > 0 && CurrentUserRoles.IsEnhancementAdmin && enhancementEditViewModel != null)
			//{
			//    Enhancement enhancement = Repository.Enhancements.GetByID(enhancementEditViewModel.Enhancement.ID);
			//    enhancementEditViewModel.UpdateModel(enhancement);
			//    Repository.Enhancements.Update(enhancement);
			//    return RedirectToAction("Detail", new { id = enhancementEditViewModel.Enhancement.ID });
			//}
			//else
			//{
			return RedirectToAction( "Index" );
			//}
		}

		[Authorize]
		public ActionResult Submit()
		{
			Enhancement enhancementModel = new Enhancement();
			EnhancementEditViewModel viewModel = BuildEditViewModel( null, enhancementModel );
			return View( viewModel );
		}

		[Authorize]
		[HttpPost]
		[ValidateInput( false )]
		public ActionResult Submit( EnhancementEditViewModel enhancementEditViewModel )
		{
			if ( enhancementEditViewModel != null )
			{
				StringBuilder body = new StringBuilder();
				Enhancement enhancement = enhancementEditViewModel.Enhancement;
				body.Append( "<html><head><style>body {font-family:Ariel,Helvetica,sans-serif}</style></head><body>" );
				body.Append( "<h2>CERS Enhancement Request</h2>" );

				string portalName = ( from p in Repository.Enhancements.Portals
									  where p.ID.Equals( enhancement.PortalID )
									  select p.Name ).ToArray<string>()[0];
				body.Append( "<br/><b>Portal:</b>" );
				if ( !String.IsNullOrEmpty( portalName ) )
				{
					body.Append( portalName );
				}

				body.Append( "<br/><strong>Name:</strong>" );
				body.Append( enhancement.Name );

				body.Append( "<br/><strong>Description:</strong><br/>" );
				body.Append( System.Net.WebUtility.HtmlDecode( enhancement.Description ) );

				body.Append( "<br/><br/><strong>Submitter Name:</strong>" );
				body.Append( CurrentAccount.FirstName );
				body.Append( " " );
				body.Append( CurrentAccount.LastName );

				body.Append( "<br/><strong>Submitter Email:</strong>" );
				body.Append( CurrentAccount.Email );
				body.Append( "</body></html>" );

				//MailHelper.Send(CurrentAccount.Email, "randy.westra@calepa.ca.gov", "CERS Enhancement Request", body.ToString(), System.Net.Mail.MailPriority.Normal, true);
				Services.Emails.Send( ConfigurationManager.AppSettings["EnhancementSubmitEmail"], "CERS Enhancement Request", body.ToString(), EmailPriority.Normal, true );
			}

			return RedirectToAction( "SubmitReceive" );
		}

		[Authorize]
		public ActionResult SubmitReceive()
		{
			return View();
		}

		private EnhancementEditViewModel BuildEditViewModel( EnhancementEditViewModel viewModel, Enhancement enhancementModel )
		{
			if ( viewModel == null )
			{
				viewModel = new EnhancementEditViewModel();
				viewModel.AccountName = CurrentAccount.FirstName + " " + CurrentAccount.LastName;
				viewModel.AccountEmail = CurrentAccount.Email;
				viewModel.Enhancement = enhancementModel;
			}

			viewModel.Statuses = ( from s in Repository.Enhancements.Statuses
								   select new SelectListItem
								   {
									   Value = s.ID.ToString(),
									   Text = s.Name
								   } ).ToList();
			viewModel.Assignees = ( from a in Repository.Enhancements.Assignees
									select new SelectListItem
									{
										Value = a.ID.ToString(),
										Text = a.Name
									} ).ToList();
			viewModel.Portals = ( from p in Repository.Enhancements.Portals
								  select new SelectListItem
								  {
									  Value = p.ID.ToString(),
									  Text = p.Name
								  } ).ToList();
			viewModel.Priorities = ( from p in Repository.Enhancements.Priorities
									 select new SelectListItem
									 {
										 Value = p.ID.ToString(),
										 Text = p.Name
									 } ).ToList();
			return viewModel;
		}

		#endregion Edit

		#region Comment

		[Authorize]
		public ActionResult CommentAdd( int enhancementID )
		{
			EnhancementCommentEditViewModel viewModel = BuildCommentEditViewModel( null, null );
			viewModel.UserComment.EnhancementID = enhancementID;
			viewModel.UserComment.AccountID = Repository.ContextAccountID;
			//return View(viewModel);
			var results = RenderPartialViewToString( "_CommentAdd", viewModel );
			return Json( new { success = true, message = results }, JsonRequestBehavior.AllowGet );
		}

		[Authorize]
		[HttpPost]
		public ActionResult CommentAdd( int enhancementID, EnhancementCommentEditViewModel commentEditViewModel )
		{
			if ( commentEditViewModel != null &&
				( commentEditViewModel.UserComment.AccountID == CurrentAccountID /*|| CurrentUserRoles.IsEnhancementAdmin*/) )
			{
				Repository.EnhancementComments.Create( commentEditViewModel.UserComment );
				//return RedirectToAction("Detail", new { id = commentEditViewModel.UserComment.EnhancementID });
				return Json( new { success = true } );
			}
			else
			{
				//return RedirectToAction("Index");
				return Json( new { success = false } );
			}
		}

		[Authorize]
		[HttpPost]
		public ActionResult CommentDelete( int commentID )
		{
			EnhancementUserComment comment = null;
			comment = Repository.EnhancementComments.GetByID( commentID );
			if ( comment != null &&
				( comment.AccountID == CurrentAccountID /*|| CurrentUserRoles.IsEnhancementAdmin*/) )
			{
				Repository.EnhancementComments.Delete( comment );
				return Json( new { success = true } );
			}
			else
			{
				//return RedirectToAction("Index");
				return Json( new { success = false } );
			}
		}

		[Authorize]
		public ActionResult CommentDetail( int commentID )
		{
			EnhancementUserComment userCommentModel = Repository.EnhancementComments.GetByID( commentID );
			EnhancementCommentEditViewModel viewModel = BuildCommentEditViewModel( null, userCommentModel );

			//return View(viewModel);
			var results = RenderPartialViewToString( "_CommentDetail", viewModel );
			return Json( new { success = true, message = results }, JsonRequestBehavior.AllowGet );
		}

		[Authorize]
		public ActionResult CommentEdit( int commentID )
		{
			EnhancementUserComment userCommentModel = Repository.EnhancementComments.GetByID( commentID );
			EnhancementCommentEditViewModel viewModel = BuildCommentEditViewModel( null, userCommentModel );

			//return View(viewModel);
			var results = RenderPartialViewToString( "_CommentEdit", viewModel );
			return Json( new { success = true, message = results }, JsonRequestBehavior.AllowGet );
		}

		[Authorize]
		[HttpPost]
		public ActionResult CommentEdit( int commentID, EnhancementCommentEditViewModel enhancementCommentEditViewModel )
		{
			EnhancementUserComment comment = null;
			if ( enhancementCommentEditViewModel != null )
			{
				comment = Repository.EnhancementComments.GetByID( commentID );
			}
			if ( comment != null &&
				( comment.AccountID == CurrentAccountID /*|| CurrentUserRoles.IsEnhancementAdmin*/) )
			{
				enhancementCommentEditViewModel.UpdateModel( comment );
				Repository.EnhancementComments.Update( comment );
				//return RedirectToAction("Detail", new { id = enhancementCommentEditViewModel.UserComment.EnhancementID });
				return Json( new { success = true } );
			}
			else
			{
				//return RedirectToAction("Index");
				return Json( new { success = false } );
			}
		}

		private EnhancementCommentEditViewModel BuildCommentEditViewModel( EnhancementCommentEditViewModel viewModel, EnhancementUserComment userCommentModel )
		{
			if ( viewModel == null )
			{
				viewModel = new EnhancementCommentEditViewModel();
				viewModel.AccountName = String.Empty;
				viewModel.AccountEmail = String.Empty;
				if ( userCommentModel == null )
				{
					userCommentModel = new EnhancementUserComment();
					userCommentModel.ID = 0;
					userCommentModel.AccountID = 0;
					userCommentModel.RegulatorID = 0;
					userCommentModel.OrganizationID = 0;
					userCommentModel.UserPriorityID = 0;
				}
				viewModel.UserComment = userCommentModel;
			}
			if ( userCommentModel.AccountID > 0 )
			{
				string name;
				string email;
				Repository.Enhancements.AccountNameEmail( userCommentModel.AccountID, out name, out email );
				viewModel.AccountName = name;
				viewModel.AccountEmail = email;
			}
			else
			{
				viewModel.AccountName = CurrentAccount.FirstName + " " + CurrentAccount.LastName;
				viewModel.AccountEmail = CurrentAccount.Email;
			}

			viewModel.Regulators = ( from r in Repository.Regulators.Search( accountID: Repository.ContextAccountID).ToList() 
									 select new SelectListItem
									 {
										 Value = r.ID.ToString(),
										 Text = r.NameShort
									 } ).ToList();
			viewModel.Organizations = ( from o in Repository.Organizations.GetByAccount( Repository.ContextAccountID )
										select new SelectListItem
										{
											Value = o.ID.ToString(),
											Text = o.Name
										} ).ToList();

			viewModel.Priorities = ( from p in Repository.Enhancements.Priorities
									 select new SelectListItem
									 {
										 Value = p.ID.ToString(),
										 Text = p.Name
									 } ).ToList();
			return viewModel;
		}

		#endregion Comment

		#region Support Functions

		private int IsNull( int? value, int defaultValue )
		{
			return value == null ? 0 : (int) value;
		}

		#endregion Support Functions
	}
}