using CERS.Model;
using CERS.ViewModels;
using CERS.Web.Mvc;
using CERS.Web.UI.Organization.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using UPF;
using UPF.Web.Mvc;
using UPF.Web.Mvc.UI;

namespace CERS.Web.UI.Organization.Controllers
{
	public class SubmittalElementControllerBase : AppControllerBase
	{
		/// <summary>
		/// Deletes the Entity linked to the <see cref="FacilitySubmittalElement"/> of the specified Resource Type.
		/// This should only be used for one-to-one relationships, such as BPActivities, BPOwnerOperator,
		/// USTFacilityInfo, etc.
		/// </summary>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="fse"></param>
		/// <param name="resourceType"></param>
		protected virtual void DeleteSingleEntity( FacilitySubmittalElement fse, ResourceType resourceType )
		{
			Services.FacilitySubmittalModelEntities.DeleteSingleEntity( fse, resourceType );
		}

		/// <summary>
		/// Deletes the Entity linked to the <see cref="FacilitySubmittalElement"/> of the specified Resource Type.
		/// and FSER (Resource) ID.  This should only be used for one-to-many relationships, such as USTTankInfo,
		/// USTMonitoringPlan, USTInstallModCert, etc.
		/// </summary>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="fse"></param>
		/// <param name="resourceType"></param>
		/// <param name="fserID"></param>
		protected virtual void DeleteSpecificEntity( FacilitySubmittalElement fse, ResourceType resourceType, int fserID )
		{
			Services.FacilitySubmittalModelEntities.DeleteSpecificEntity( fse, resourceType, fserID );
		}

		/// <summary>
		/// Returns a single Entity for specified <see cref="FacilitySubmittalElement"/> and <see cref="ResourceType"/>.  For
		/// use with ONE-to-ONE FSE-to-FSER relationships only, such as BPActivities, BPOwnerOperator, USTFacilityInfo, etc.
		/// </summary>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="fse"></param>
		/// <param name="resourceType"></param>
		/// <returns></returns>
		protected virtual TModel GetSingleEntity<TModel>( FacilitySubmittalElement fse, ResourceType resourceType ) where TModel : class, IFacilitySubmittalModelEntity, new()
		{
			return Services.FacilitySubmittalModelEntities.GetSingleEntity<TModel>( fse, resourceType );
		}

		/// <summary>
		/// Generically returns a <see cref="FacilitySubmittalElementResourceViewMode{TModel}"/> for one-to-one relationships
		/// (BPActivities, BPOwnerOperator, USTFacilityInfo, etc.). This method will retrieve the only resource available for
		/// the specified Resource Type for this <see cref="FacilitySubmittalElement"/>.  If no <see cref="FacilitySubmittalElement"/> exists, the
		/// call to 'GetFacilitySubmittalElement' automatically generates the appropriate <see cref="FacilitySubmittalElementResource"/> records.
		/// </summary>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="organziationID"></param>
		/// <param name="CERSID"></param>
		/// <param name="submittalElement"></param>
		/// <param name="resourceType"></param>
		/// <param name="fseId"></param>
		/// <param name="fserID"></param>
		/// <returns></returns>
		protected virtual FacilitySubmittalElementResourceViewModel<TModel> GetSingleEntityViewModel<TModel>( int organziationID, int CERSID, SubmittalElementType submittalElement, ResourceType resourceType, int? fseID = null ) where TModel : class, IFacilitySubmittalModelEntity, new()
		{
			return Services.ViewModels.GetSingleEntityViewModel<TModel>( organziationID, CERSID, submittalElement, resourceType, fseID );
		}

		/// <summary>
		/// Returns a specific Entity for specified <see cref="FacilitySubmittalElement"/>, <see cref="ResourceType"/>, and
		/// fserID (FacilitySubmittalElementResourceID).  For use with ONE-to-MANY FSE-to-FSER relationships only, such as
		/// USTTankInfo, USTMonitoringPlan, USTInstallModCert, etc.
		/// </summary>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="fse"></param>
		/// <param name="resourceType"></param>
		/// <param name="fserID">Providing a value of zero will generate an pre-configured model of the appropriate Resource
		/// Type for the specified FSE and ResourceType, to assist with the Create views. A non-zero value will retrieve the
		/// existing Resource for the FSE and FSERID.</param>
		/// <returns></returns>
		protected virtual TModel GetSpecificEntity<TModel>( FacilitySubmittalElement fse, ResourceType resourceType, int fserID ) where TModel : class, IFacilitySubmittalModelEntity, new()
		{
			return Services.FacilitySubmittalModelEntities.GetSpecificEntity<TModel>( fse, resourceType, fserID );
		}

		/// <summary>
		/// Generically gets a <see cref="FacilitySubmittalElementResourceViewMode{TModel}"/> for one-to-many relationships
		/// (USTTankInfo, USTMonitoringPlan, USTInstallModCert, etc.). This method will retrieve the resource specified by
		/// fserID, or generates a new FacilitySubmittalElementResource and Entity (or the specified type) if an available
		/// FacilitySubmittalElementResource is not found.
		/// </summary>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="organziationID"></param>
		/// <param name="CERSID"></param>
		/// <param name="submittalElement"></param>
		/// <param name="resourceType"></param>
		/// <param name="fseID"></param>
		/// <param name="fserID"></param>
		/// <returns></returns>
		protected virtual FacilitySubmittalElementResourceViewModel<TModel> GetSpecificEntityViewModel<TModel>( int organziationID, int CERSID, SubmittalElementType submittalElement, ResourceType resourceType, int? fseID = null, int? fserID = null, int? parentFSERID = null ) where TModel : class, IFacilitySubmittalModelEntity, new()
		{
			return Services.ViewModels.GetSpecificEntityViewModel<TModel>( organziationID, CERSID, submittalElement, resourceType, fseID, fserID, parentFSERID );
		}

		#region SupplementalDocuments (Document Upload)

		public virtual ActionResult Handle_Cloning( int organizationId, int CERSID, int FSEID, SubmittalElementType submittalElementType )
		{
			//This time persist the new fse
			var fseCurrentDraft = Services.BusinessLogic.SubmittalElements.GetFacilitySubmittalElement( CERSID, submittalElementType, SubmittalElementStatus.Draft, true );

			Repository.FacilitySubmittalElements.CloneWithEntities( CERSID, FSEID, fseCurrentDraft );

			//Sets the LastSubmittalDelta
			fseCurrentDraft.SetLastSubmittalDelta( Repository );

			fseCurrentDraft.ValidateAndCommitResults( Repository, CallerContext.UI );

            //If the SubmittalElementType is Facility Information, set the Reporting Requirements per the Biz Activities form 
            if (fseCurrentDraft.SubmittalElementID == (int)SubmittalElementType.FacilityInformation)
            {
                Services.BusinessLogic.SubmittalElements.FacilityInformation.UpdateFRSEFromBizActivities( fseCurrentDraft );
            }

			return RedirectToRoute( "OrganizationFacilityHome", new { organizationID = organizationId, CERSID = CERSID } );
		}

		/// <summary>
		/// Generic method used to discard a supplemental document for a specific FSE and FSER.
		/// </summary>
		/// <param name="organizationId"></param>
		/// <param name="CERSID"></param>
		/// <param name="FSEID"></param>
		/// <param name="FSERID"></param>
		/// <returns></returns>
		public virtual ActionResult Handle_DocumentDelete( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			//TODO: TAGGED FOR REVIEW 2/28/2014 by Mike Reagan
			// Retrieve FSER and Return Anchor (Submittal Element Acronym)
			var fser = Repository.FacilitySubmittalElementResources.GetByID( FSERID );
			string anchor = fser.FacilitySubmittalElement.SubmittalElement.Acronym;

			// Discard Resource (discards documents under this resource)
			fser.Discard( Repository );

			//Validate FSE
			fser.FacilitySubmittalElement.ValidateAndCommitResults( Repository, CallerContext.UI );

			//Set LastSubmittalDeltaId for FSE
			fser.FacilitySubmittalElement.SetLastSubmittalDelta( Repository );

			// Always send user back to the Draft Submittal Summary Page
			return RedirectToFacilityDraftSubmittals( organizationId, CERSID, "#" + anchor );
		}

		public virtual ActionResult Handle_DocumentUploadGet( int organizationId, int CERSID, SubmittalElementType submittalElementType, ResourceType resourceType, int? fseID = null, int? fserID = null )
		{
			var viewModel = BuildUpDocumentUploadViewModel( organizationId, CERSID, submittalElementType, resourceType, fseID, fserID );
			return View( viewModel );
		}

		[ValidateOnlyIncomingValues]
		public virtual ActionResult Handle_DocumentUploadPost( int organizationId, int CERSID, SubmittalElementType submittalElementType, ResourceType resourceType, DocumentUploadViewModel formValues, int? fseId = null, int? fserID = null )
		{
			Server.ScriptTimeout = 180;

			bool isValid = true;
			var viewModel = BuildUpDocumentUploadViewModel( organizationId, CERSID, submittalElementType, resourceType, fseId, fserID );
			var entity = viewModel.Entity;
			Document document = null;

			HttpPostedFileBase file = null;
			int? typeID;
			int? formatID;
			string errorHelper;

			if ( TryUpdateModel( entity, "Entity" ) )
			{
				isValid = ValidateDocumentUploadViewModel( formValues, entity, ref file, (int)resourceType, out typeID, out formatID, out errorHelper );

				//must set all file values before validating model.
				if ( ModelState.IsValid && isValid )
				{
					//See if this is a delete document action
					entity.Resource = viewModel.FacilitySubmittalElementResource;
					entity.Resource.IsDocument = true;
					entity.Resource.IsStarted = true;
					entity.Resource.DocumentSourceID = viewModel.Entity.SourceID;

					#region Save Everything

					if ( formValues.ActionSourceID == (int)DocumentSource.UploadDocuments && formValues.ActionValue.Contains( "Save" ) )
					{
						try
						{
							document = Repository.Documents.Save( file.InputStream, file.FileName, DocumentContext.Organization, organizationId );
						}
						catch ( Exception ex )
						{
							ModelState.AddModelError( "File", "Unable to save file. " + ex.Message );
							isValid = false;
						}
						if ( document != null )
						{
							FacilitySubmittalElementDocument fseDocument = new FacilitySubmittalElementDocument();
							fseDocument.DocumentID = document.ID;
							fseDocument.TypeID = typeID.Value;
							fseDocument.FormatID = formatID.Value;
							fseDocument.Name = formValues.Name;
							fseDocument.FileName = Path.GetFileName( file.FileName );
							fseDocument.FileSize = file.ContentLength;
							fseDocument.DocumentDate = formValues.DocumentDate.Value;
							fseDocument.Description = formValues.Description;
							fseDocument.Key = Guid.NewGuid();
							// initialize EDTClientKey even if not EDT so that EDT RFSQ's have a DocumentRegulatorKey element under the Attachment element
							fseDocument.SetDDCommonFields( CurrentAccountID, true, edtClientKey: Guid.NewGuid().ToString() );
							entity.Documents.Add( fseDocument );
							Repository.DataModel.SaveChanges();
						}
					}

					if ( formValues.ActionSourceID.IfInRange( Repository.SystemLookupTables.DocumentSources.Select( p => p.ID ).ToArray() ) && formValues.ActionValue.Contains( "Save" ) )
					{
						//Save FacilitySubmittalElementResourceDocuemnt
						entity.Resource.UpdateResourceEntityCount();
						Repository.FacilitySubmittalElementResourceDocuments.Save( entity );

						//Validate this Submittal Element
						entity.Resource.FacilitySubmittalElement.ValidateAndCommitResults( Repository, CallerContext.UI );

						var fser = Repository.FacilitySubmittalElementResources.GetByID( entity.Resource.ID );

						//Set LastSubmittalDeltaId for FSE
						fser.SetLastSubmittalDelta( entity, CERSID, Repository );

						if ( formValues.ActionValue.Contains( "Continue" ) || formValues.ActionValue.Contains( "Again" ) )
						{
							//redirect to Edit
							return RedirectToAction( "Edit_" + resourceType.ToString(), new { organizationId = organizationId, CERSID = CERSID, FSEID = fser.FacilitySubmittalElementID } );
						}
						else
						{
							//redirect to Org home page (aka crazy page)
							return RedirectToRoute( OrganizationFacility.Home, new { organizationId = organizationId, CERSID = CERSID } );
						}
					}

					if ( !string.IsNullOrWhiteSpace( formValues.ActionValue ) && formValues.ActionValue.Contains( "Delete Document" ) )
					{
						//get the id from the action name.
						string actionName = formValues.ActionValue;
						int underscoreIndex = ( actionName.IndexOf( "_" ) ) + 1;
						int fseDocumentID;
						if ( int.TryParse( actionName.Substring( underscoreIndex ), out fseDocumentID ) )
						{
							var documentToDelete = Repository.FacilitySubmittalElementDocuments.GetByID( fseDocumentID );
							try
							{   //void the facilitySubmittalElementDoucment
								documentToDelete.SetCommonFields( CurrentAccountID, false, true );
								documentToDelete.FacilitySubmittalElementResourceDocument.Resource.UpdateResourceEntityCount();
								documentToDelete.FacilitySubmittalElementResourceDocument.SetCommonFields( CurrentAccountID );
								Repository.FacilitySubmittalElementDocuments.Save( documentToDelete );

								//Set LastSubmittalDeltaId for FSER
								//Repository.FacilitySubmittalElementResourceDocuments.SetLastSubmittalDelta(entity, CERSID);

								//Set LastSubmittalDeltaId for FSE
								var fser = Repository.FacilitySubmittalElementResources.GetByID( entity.FacilitySubmittalElementResourceID );

								//fser.FacilitySubmittalElement.SetLastSubmittalDelta(Repository);
								fser.SetLastSubmittalDelta( entity, CERSID, Repository );

								//redirect to edit page.
								return RedirectToAction( "Edit_" + resourceType.ToString() );
							}
							catch ( Exception ex )
							{
								ModelState.AddModelError( "", ex.Message );
							}
						}
					}

					#endregion Save Everything
				}
			}
			return View( viewModel );
		}

		/// <summary>
		/// Builds up the Supplemental Document pages view model.
		/// </summary>
		/// <param name="organizationId"></param>
		/// <param name="CERSID"></param>
		/// <param name="submittalElementType"></param>
		/// <param name="resourceType"></param>
		/// <param name="fseID"></param>
		/// <returns></returns>
		protected virtual DocumentUploadViewModel BuildUpDocumentUploadViewModel( int organizationId, int CERSID, SubmittalElementType submittalElementType, ResourceType resourceType, int? fseID = null, int? fserID = null )
		{
			//declare the viewModel
			DocumentUploadViewModel viewModel = new DocumentUploadViewModel();
			viewModel.OrganizationID = organizationId;

			FacilitySubmittalElement fse = null;
			if ( fseID != null )
			{
				//since we have an fseID parameter this means we are pulling up an existing submittal
				//which could also mean, we also need to check the status of this FacilitySubmittalElement to see whether editing is allowed.
				//
				fse = Services.BusinessLogic.SubmittalElements.GetFacilitySubmittalElement( fseID.Value );
				if ( fse.CERSID != CERSID )
				{
					throw new Exception( "The CERSID and the FacilitySubmittalElementID do not belong to one another.  This is probably an attempt to breach security." );
				}

				//When the submittal element is in draft, editing is allowed, but not in ANY other status.
				viewModel.EditingAllowed = ( (SubmittalElementStatus)fse.StatusID == SubmittalElementStatus.Draft );
			}
			else
			{
				//in this particlar case, we ALWAYS bring back a Draft submittal element. The method below locates an existing draft is there is one OR creates on, if there
				//is no active draft for the submittal element.
				fse = Services.BusinessLogic.SubmittalElements.GetFacilitySubmittalElement( CERSID, submittalElementType, SubmittalElementStatus.Draft, true );
			}

			//at this point, we should have a FacilitySubmittalElement to work against, if not (should not happen) lets blow up.
			if ( fse == null )
			{
				throw new Exception( "Unable to find a usable FacilitySubmittalElement." );
			}
			viewModel.FacilitySubmittalElement = fse;

			//Get resource.
			var resource = fse.Resources.FirstOrDefault( p => p.ResourceTypeID == (int)resourceType && !p.Voided );
			if ( fserID != null )
			{
				resource = fse.Resources.SingleOrDefault( p => p.ResourceTypeID == (int)resourceType && !p.Voided && p.ID == fserID );
			}

			//Documents can be voided. In that case the resource will be null. So, spin up a new resource.
			if ( resource == null )
			{
				//Make sure the resource type is Document. If not, then there are bigger problems.
				var isDocument = Repository.SystemLookupTables.ResourceTypes.Where( p => !p.Voided && p.DocumentTypeID != null && p.ID == (int)resourceType );
				if ( isDocument != null )
				{
					SubmittalElementTemplateResource setr = Repository.SubmittalElementTemplateResources.Search( submittalElementID: fse.SubmittalElementID, templateID: fse.TemplateID, typeID: (int)resourceType ).SingleOrDefault();
					var parentResource = fse.Resources.Where( r => !r.Voided && r.ResourceTypeID == setr.ParentResourceID ).FirstOrDefault();
					resource = fse.AddResource( setr, parentResource, true );
				}
			}

			//Set viewModle resource
			viewModel.FacilitySubmittalElementResource = resource;

			//Get Entity
			viewModel.Entity = resource.FacilitySubmittalElementResourceDocuments.FirstOrDefault( p => !p.Voided );

			//Create viewModel.Document

			//If entity is null nothing was found above.  Create new Entity object
			if ( viewModel.Entity == null )
			{
				viewModel.Entity = new FacilitySubmittalElementResourceDocument();
				viewModel.Entity.SetCommonFields( CurrentAccountID, true );
			}
			else
			{
				if ( viewModel.Entity.SourceID == (int)DocumentSource.Exempt )
				{
					viewModel.ExemptComments = viewModel.Entity.Comments;
				}
				else if ( viewModel.Entity.SourceID == (int)DocumentSource.ProvidedInOtherSubmittalElement )
				{
					viewModel.ProvidedInSubmittalElementComments = viewModel.Entity.Comments;
				}
			}

			//Get existing Documents.
			viewModel.Documents = viewModel.Entity.Documents.Where( p => !p.Voided );

			#region Load list values

			//Get DocumentSources for source radio button list.  i.e. exempt, other Submittal Element, stored at Facility.
			viewModel.DocumentSources = new SelectList( Repository.SystemLookupTables.GetValues( SystemLookupTable.DocumentSource ), "ID", "Name" );

			//Get otherSubmittalElements the documents could be included in.
			var otherSubmittalElements = Repository.FacilityRegulatorSubmittalElements.GetCurrentFacilitySubmittalElements( CERSID ).Where( p => p.ReportingRequirementID != (int)ReportingRequirement.NotApplicable );
			viewModel.OtherSubmittalElements = ( from se in otherSubmittalElements
												 select new SelectListItem
												 {
													 Value = se.SubmittalElementID.ToString(),
													 Text = se.Name
												 } ).ToList();

			//Get other facilities documents could be stored at.
			viewModel.Facilities = ( from f in Repository.Facilities.Search( organizationID: organizationId )
									 select new SelectListItem
									 {
										 Value = f.CERSID.ToString(),
										 Text = f.Name
									 } ).ToList();

			#endregion Load list values

			//SET DEFAULT VALUES

			//Set entity.LinkUrl default value.
			if ( string.IsNullOrEmpty( viewModel.Entity.LinkUrl ) )
			{
				viewModel.Entity.LinkUrl = "http://";
			}

			//Set default date in Date Authored field of "Document Upload"
			viewModel.DocumentDate = DateTime.Today;

			//default name for the document
			viewModel.Name = resource.Type.Name.ToLower().Trim().Contains( "locally-required" ) || resource.Type.Name.ToLower().Trim().Contains( "state-required" ) ? "" : resource.Type.Name;

			return viewModel;
		}

		#region Supplemental Document Helper methods

		#region ValidateDocumentUploadViewModel Method

		protected virtual bool ValidateDocumentUploadViewModel( DocumentUploadViewModel model, FacilitySubmittalElementResourceDocument entity, ref HttpPostedFileBase file, int resourceType, out int? typeID, out int? formatID, out string errorHelper )
		{
			bool isValid = true;

			typeID = null;
			formatID = null;
			errorHelper = null;

			//[ak 09/27/2013] this cause problems on saving, I'm not sure what is the intention. for now I commented out
			//if ( file == null || model == null )
			//{
			//	return false;
			//}

			if ( model.ActionSourceID == (int)DocumentSource.UploadDocuments && model.ActionValue.Contains( "Save" ) )
			{
				#region Add Document Validation

				//Validate if file has been uploaded.
				int maxFileSize = Repository.Settings.GetMaxUploadFileSize();
				if ( model.File != null && model.File.ContentLength > maxFileSize )
				{
					ModelState.AddModelError( "File", String.Format( "The file {0} is too large for upload. Maximum file size is {1} KB", model.File.FileName, maxFileSize / 1024 ) );
					isValid = false;
				}
				else if ( model.File == null || !( model.File.ContentLength > 0 ) )
				{
					ModelState.AddModelError( "File", "Cannot save without an uploaded document." );
					isValid = false;
				}
				else
				{
					var fileformatValidationResult = Repository.Documents.ValidateFileFormat( model.File.FileName );
					if ( !fileformatValidationResult.IsValid )
					{
						ModelState.AddModelError( "File", fileformatValidationResult.ValidationErrorMessage );
						isValid = false;
					}
					else
					{
						formatID = (int)fileformatValidationResult.Format;
						file = model.File;
					}
				}

				//Validate resource type has a document type value.
				typeID = Repository.Documents.GetDocumentTypeID( resourceType );
				if ( typeID == null )
				{
					ModelState.AddModelError( "DocumentType", "The resource type is invalid.  No document type available." );
					isValid = false;
				}
				if ( string.IsNullOrEmpty( model.Name ) )
				{
					ModelState.AddModelError( "Name", "File Name is required." );
					isValid = false;
				}
				if ( !model.DocumentDate.HasValue )
				{
					ModelState.AddModelError( "DocumentDate", "Date Authored is required." );
					isValid = false;
				}

				//if (string.IsNullOrEmpty(model.Name))
				//{
				//    ModelState.AddModelError("Name", "Document Title is required.");
				//    isValid = false;
				//}
				//Set the Link URL to null (default value is 'http://')
				entity.LinkUrl = null;

				#endregion Add Document Validation
			}
			if ( model.ActionSourceID.IfInRange( Repository.SystemLookupTables.DocumentSources.Select( p => p.ID ).ToArray() ) && model.ActionValue.Contains( "Save" ) )
			{
				#region Save Validation

				//Remove the state validators for Document Upload.
				RemoveModelStateError( ModelState, "Document.Name" );
				RemoveModelStateError( ModelState, "Document.DocumentDate" );
				//Validate depending on what Document Source was selected.
				switch ( model.Entity.SourceID )
				{
					case (int)DocumentSource.Exempt:
						model.DocumentDate = null;
						entity.LinkUrl = null;
						entity.DateProvidedToRegulator = null;
						entity.StoredAtFacilityCERSID = null;
						entity.ProvidedInSubmittalElementID = null;
						entity.Comments = model.ExemptComments;
						if ( string.IsNullOrEmpty( model.ExemptComments ) )
						{
							ModelState.AddModelError( "ExemptComments", "Exemption Description is required." );
							isValid = false;
						} break;
					case (int)DocumentSource.ProvidedToRegulator:
						entity.LinkUrl = null;
						model.DocumentDate = null;
						entity.StoredAtFacilityCERSID = null;
						entity.ProvidedInSubmittalElementID = null;
						entity.Comments = null;
						if ( !model.Entity.DateProvidedToRegulator.HasValue )
						{
							ModelState.AddModelError( "Entity.DateProvidedToRegulator", "Date Provided To Regulator is required." );
							isValid = false;
						} break;
					case (int)DocumentSource.ProvidedInOtherSubmittalElement:
						entity.LinkUrl = null;
						model.DocumentDate = null;
						entity.DateProvidedToRegulator = null;
						entity.StoredAtFacilityCERSID = null;
						entity.Comments = model.ProvidedInSubmittalElementComments;
						break;

					case (int)DocumentSource.PublicInternetURL:
						entity.DateProvidedToRegulator = null;
						model.DocumentDate = null;
						entity.StoredAtFacilityCERSID = null;
						entity.ProvidedInSubmittalElementID = null;
						entity.Comments = null;
						if ( string.IsNullOrEmpty( model.Entity.LinkUrl ) )
						{
							ModelState.AddModelError( "Entity.LinkUrl", "Public URL is required." );
							isValid = false;
						}
						Uri uriResult;
						if ( !Uri.TryCreate( model.Entity.LinkUrl, UriKind.Absolute, out uriResult ) )
						{
							ModelState.AddModelError( "Entity.LinkUrl", "Public URL is not a valid URL" );
							isValid = false;
						}
						break;

					case (int)DocumentSource.UploadDocuments:
						entity.LinkUrl = null;
						entity.DateProvidedToRegulator = null;
						entity.StoredAtFacilityCERSID = null;
						entity.ProvidedInSubmittalElementID = null;
						entity.Comments = null;
						if ( model.File == null || model.File.ContentLength == 0 )
						{
							ModelState.AddModelError( "Documents", "You must upload at least one file." );

							//per Chris add last bullet for cancel button
							ModelState.AddModelError( "Documents", "Select Cancel button to exit." );

							isValid = false;
						}
						break;

					case (int)DocumentSource.StoredAtFacility:
						entity.LinkUrl = null;
						model.DocumentDate = null;
						entity.DateProvidedToRegulator = null;
						entity.ProvidedInSubmittalElementID = null;
						entity.Comments = null;
						if ( model.Entity.StoredAtFacilityCERSID == null )
						{
							ModelState.AddModelError( "Entity.StoredAtFacilityCERSID", "Facilities is required." );
							isValid = false;
						}
						break;

					default:
						ModelState.AddModelError( "DocumentSource", "An error occurred on submission. Please try again." );
						isValid = false;
						break;
				}

				#endregion Save Validation
			}

			return isValid;
		}

		#endregion ValidateDocumentUploadViewModel Method

		#endregion Supplemental Document Helper methods

		#endregion SupplementalDocuments (Document Upload)

		#region Handle_DiscardResource Method

		public virtual ActionResult Handle_DiscardResource( int organizationId, int CERSID, int FSEID, int FSERID )
		{
			// Retrieve FSER and Return Anchor (Submittal Element Acronym)
			var fser = Repository.FacilitySubmittalElementResources.GetByID( FSERID );
			string anchor = fser.FacilitySubmittalElement.SubmittalElement.Acronym;

			Services.FacilitySubmittals.DiscardResource( fser, CallerContext.UI );

			// Always send user back to the Draft Submittal Summary Page
			return RedirectToFacilityDraftSubmittals( organizationId, CERSID, "#" + anchor );
		}

		#endregion Handle_DiscardResource Method

		#region Landing Pages.

		public virtual ActionResult Handle_LandingPageGet( int organizationId, int CERSID, SubmittalElementType submittalElementType, ResourceType resourceType, string instructions, IEnumerable<WhatsNextItem> whatsNext, int? fseID = null, int? fserID = null )
		{
			LandingPageViewModel viewModel = new LandingPageViewModel();

			FacilitySubmittalElement fse = null;
			if ( fseID != null )
			{
				//since we have an fseID parameter this means we are pulling up an existing submittal
				//which could also mean, we also need to check the status of this FacilitySubmittalElement to see whether editing is allowed.
				//
				fse = Services.BusinessLogic.SubmittalElements.GetFacilitySubmittalElement( fseID.Value );
				if ( fse.CERSID != CERSID )
				{
					throw new Exception( "The CERSID and the FacilitySubmittalElementID do not belong to one another.  This is probably an attempt to breach security." );
				}

				//When the submittal element is in draft, editing is allowed, but not in ANY other status.
			}
			else
			{
				//in this particlar case, we ALWAYS bring back a Draft submittal element. The method below locates an existing draft is there is one OR creates on, if there
				//is no active draft for the submittal element.
				fse = Services.BusinessLogic.SubmittalElements.GetFacilitySubmittalElement( CERSID, submittalElementType, SubmittalElementStatus.Draft, true );
			}

			//at this point, we should have a FacilitySubmittalElement to work against, if not (should not happen) lets blow up.
			if ( fse == null )
			{
				throw new Exception( "Unable to find a usable FacilitySubmittalElement." );
			}
			viewModel.FacilitySubmittalElement = fse;

			// Get Resource
			FacilitySubmittalElementResource resource = new FacilitySubmittalElementResource();
			if ( fserID == null )
			{
				// If fserID is not specified, retrieve the first resource for this FSE and ResourceType
				resource = fse.Resources.FirstOrDefault( p => p.ResourceTypeID == (int)resourceType && !p.Voided );
			}
			else
			{
				// If fserID *is* specified, resource ID must match the specified value
				// Use "SingleOrDefault" - specifying by fserID should never return more than one record
				resource = fse.Resources.SingleOrDefault( p => p.ResourceTypeID == (int)resourceType && p.ID == fserID && !p.Voided );
			}

			// Set ViewModel Resource
			viewModel.FacilitySubmittalElementResource = resource;

			//Get current submittal element view model
			var currentSubmittalElement = Repository.FacilityRegulatorSubmittalElements.GetCurrentFacilitySubmittalElements( CERSID, (int)submittalElementType ).FirstOrDefault();
			CurrentSubmittalElementViewModel viewModelcurrSub = new CurrentSubmittalElementViewModel()
			{
				CurrentSubmittalElement = currentSubmittalElement,
				OrganizationID = organizationId,
				isFirstElement = true
			};

			//Set current submittal element view model
			viewModel.CurrentSEViewModel = viewModelcurrSub;

			//Get guidance.
			var guidance = Repository.GuidanceMessages.Search( facilitySubmittalElementResourceID: viewModel.FacilitySubmittalElementResource.ID ).ToList();
			if ( guidance == null )
			{
				guidance = new List<GuidanceMessage>();
			}

			//Set Guidance Messages
			viewModel.GuidanceMessages = guidance;

			//Set Instructions
			viewModel.Instructions = instructions;

			//Set WhatsNext
			viewModel.WhatsNext = whatsNext;

			return View( viewModel );
		}

		#region Landing page Helper methods

		protected string BuildHyperlink( KeyValuePair<string, string> link )
		{
			StringBuilder result = new StringBuilder();

			result.Append( "<a href=\"" ).Append( link.Value ).Append( "\">" ).Append( link.Key ).Append( "</a>" );

			return result.ToString();
		}

		protected string GetUrlForForm( int CERSID, SubmittalElementType submittalElement, ResourceType resource, RouteValueDictionary routeValues )
		{
			string result = null;

			string routeName = GetDraftFormRouteName( CERSID, submittalElement, resource );
			var routeVirtualPath = RouteTable.Routes.GetVirtualPath( null, routeName, routeValues );
			if ( routeVirtualPath != null )
			{
				result = routeVirtualPath.VirtualPath;
			}
			return result;
		}

		#endregion Landing page Helper methods

		#endregion Landing Pages.

		#region Handle_FormCancel Method

		public virtual ActionResult Handle_FormCancel( int organizationId, int CERSID, int FSEID )
		{
			var facilitySubmittalElement = Repository.FacilitySubmittalElements.GetByID( FSEID );

			//If this element is in a Draft State, send them to Submittal Summary(Crazy) page
			if ( facilitySubmittalElement.StatusID == (int)SubmittalElementStatus.Draft )
			{
				return RedirectToAction( GetRouteName( OrganizationFacility.Home ), new { organizationID = organizationId, CERSID = CERSID } );
			}
			else
			{
				return RedirectToAction( GetRouteName( OrganizationFacility.SubmittalEvent ), new { organizationID = organizationId, FSID = facilitySubmittalElement.FacilitySubmittalID } );
			}
		}

		#endregion Handle_FormCancel Method

		#region RemoveModelStateError Method

		private void RemoveModelStateError( ModelStateDictionary ModelState, string key )
		{
			if ( !string.IsNullOrEmpty( key ) )
			{
				if ( ModelState.ContainsKey( key ) )
				{
					ModelState[key].Errors.Clear();
				}
			}
		}

		#endregion RemoveModelStateError Method
	}
}