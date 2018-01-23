using CERS.Guidance;
using CERS.Model;
using CERS.Validation;
using CERS.Xml;
using CERS.Xml.CME;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UPF;
using UPF.Core;

namespace CERS.EDT
{
	public class RegulatorCMESubmitAdapter : InboundAdapter
	{
		public RegulatorCMESubmitAdapter( EDTTransactionScope transactionScope )
			: base( transactionScope )
		{
		}

		/// <summary>
		/// Method accepts a stream (either XML text or ZIP file), converts the stream
		/// to an XDocument, and calls Process(xDocument).
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public override XElement Process( Stream stream, NameValueCollection arguments )
		{
			return Process( ConvertStreamToXDocument( stream ) );
		}

		/// <summary>
		/// Performs the main work of the CMESubmittalEDTProcessor.  Accepts an XDocument
		/// conforming to the CMESubmittal XML Schema Definition, and returns an XElement
		/// conforming to the CMESubmittalResponse XML Schema Definition.
		/// </summary>
		/// <param name="xDocument"></param>
		/// <returns></returns>
		public XElement Process( XDocument xDocument )
		{
			TransactionScope.SaveXml( xDocument, EDTTransactionXmlDirection.Inbound );

			XElement cmeSubmittalResponse = null;

			// Set up CMESubmittal XML Deserializer and initialize CMEPackage
			CMESubmittalXmlDeserializer cmeSubmittalXmlDeserializer = new CMESubmittalXmlDeserializer( Repository );
			CMEPackage cmePackage = new CMEPackage();

			// Retrieve CME Submittal Schema Definition
			IXmlSchemaMetadata schema = XmlSchemas.GetSchemaMetadata( XmlSchema.RegulatorCMESubmit );
			cmePackage.IsValidXMLAgainstSchema = false; // Default to false, until validation proves otherwise
			cmeSubmittalXmlDeserializer.InitializeCMEPackage( cmePackage, accountID: Repository.ContextAccountID, edtTransaction: TransactionScope.Transaction, xDocument: xDocument );

			if ( xDocument == null )
			{
				var entityGuidanceMessage = EntityGuidanceMessage.Create( GuidanceMessageCode.XMLSchemaValidationError, null, null, GuidanceLevel.Required, null, "XML Document Not Found" );
				cmePackage.GuidanceMessages.Add( entityGuidanceMessage.ToGuidanceMessage() );
			}
			else
			{
				// Validate XDocument:
				var validationGuidance = xDocument.ValidateCMESubmittal( schema );
				cmePackage.GuidanceMessages.AddRange( validationGuidance );
			}

			// Set CME Package's "IsValidAgainstXMLSchema" bool based on return Guidance Messages
			cmePackage.IsValidXMLAgainstSchema = !cmePackage.GuidanceMessages.Any( gm => gm.LevelID == (int) GuidanceLevel.Required && !gm.Voided );

			// If serialized document is valid, proceed with Deserializing and Validating the individual CME Entities:
			if ( cmePackage.IsValidXMLAgainstSchema )
			{
				cmePackage = cmeSubmittalXmlDeserializer.DeserializeCMESubmittalPackage( xDocument, cmePackage, Repository.ContextAccountID, schema: schema );

				// Call PreSave Methods for all CME Entities (Insp, Vio, Enf, EnfVio):
				cmeSubmittalXmlDeserializer.CallPreSaveMethods( cmePackage, Repository );

				var permissionRoleMatrixCollection = ServiceManager.Security.GetRolesForAccount( Repository.ContextAccount );

				// Perform Standard Validation on the CMEPackage:
				cmePackage.Validate( Repository, callerContext: TransactionScope.CallerContext, permissions: permissionRoleMatrixCollection );
			}

			// Determine a list of Regulator IDs for the affected CME Data Entities:
			List<int> regulatorIDList = GenerateDistinctAffectedRegulatorIDs( cmePackage );

			// If CME Package is NOT valid (meaning one or more Required Guidance Messages were created,
			// we need to remove the CME Entities from the model:
			if ( !cmePackage.IsValid )
			{
				cmeSubmittalXmlDeserializer.DetachEntitiesFromTransaction( cmePackage, Repository );
			}

			// Update GuidanceMessage Common fields, and link all to EDTTransaction Entity
			cmePackage.GuidanceMessages.SetCommonFields( Repository.ContextAccountID, creating: true );
			foreach ( var gm in cmePackage.GuidanceMessages )
			{
				gm.EDTTransaction = cmePackage.EDTTransaction;
			}

			// Populate EDT Transaction Messages
			PopulateEDTTranactionMessages( cmePackage );
			PopulateCMEBatchMetaData( cmePackage );

			// Persist the changes to the DB by calling Complete on the EDTTransactionScope
			if ( cmePackage.IsValid )
			{
				TransactionScope.Complete( EDTTransactionStatus.Accepted );
			}
			else
			{
				TransactionScope.Complete( EDTTransactionStatus.Rejected );
			}

			// Populate RegulatorEDTTransactions Based Upon CMEBatch CME Data Entities
			GenerateRegulatorEDTTransactions( regulatorIDList );

			// Retrieve the EDT Transaction Key:
			Guid edtTransactionKey = cmePackage.EDTTransaction.Key;

			// Build a CMEPackage Object from an EDT Transaction Key
			CMEPackage cmePackageResponse = EDTTransactionToCMEPackage.BuildCMEPackageFromEDTTransaction( Repository, edtTransactionKey: edtTransactionKey );

			// Generate a CMESubmittalResponse from an EDT Transaction Key
			CMESubmittalResponseXmlSerializer cmeSubmittalResponseXmlSerializer = new CMESubmittalResponseXmlSerializer( Repository );
			cmeSubmittalResponse = cmeSubmittalResponseXmlSerializer.GenerateCMESubmittalResponsePackage( cmePackageResponse );

			TransactionScope.SaveXml( cmeSubmittalResponse, EDTTransactionXmlDirection.Outbound );
			return cmeSubmittalResponse;
		}

		#region CME Package Populate EDTTranactionMessages

		/// <summary>
		/// Method builds a set of EDTTransactionMessage objects based on Guidance Message Counts and
		/// whether the XML passed schema validate, and binds these new objects to the EDTTransaction.
		/// </summary>
		/// <param name="cmePackage"></param>
		private void PopulateEDTTranactionMessages( CMEPackage cmePackage )
		{
			if ( cmePackage == null )
			{
				throw new ArgumentNullException( "cmePackage" );
			}

			// Retrieve Guidance Message Counts for EDT Transaction Messages
			var requiredGuidanceMessageCount = cmePackage.GuidanceMessages.Count( gm => gm.LevelID == (int) GuidanceLevel.Required && !gm.Voided );
			var warningGuidanceMessageCount = cmePackage.GuidanceMessages.Count( gm => gm.LevelID == (int) GuidanceLevel.Warning && !gm.Voided );
			var advisoryGuidanceMessageCount = cmePackage.GuidanceMessages.Count( gm => gm.LevelID == (int) GuidanceLevel.Advisory && !gm.Voided );

			// Add RequiredSummary EDT Transaction Message
			if ( requiredGuidanceMessageCount > 0 )
			{
				var messageText = "Found " + requiredGuidanceMessageCount + " Required Guidance Messages.";
				if ( !cmePackage.IsValidXMLAgainstSchema )
				{
					messageText = "XML did not pass schema validation. " + messageText;
				}

				var requiredEDTTransactionMessage = new EDTTransactionMessage
				{
					EDTTransaction = cmePackage.EDTTransaction,
					TypeID = (int) EDTTransactionMessageType.Required,
					Message = messageText
				};
				requiredEDTTransactionMessage.SetCommonFields( currentUserID: Repository.ContextAccountID, creating: true );
				cmePackage.EDTTransactionMessages.Add( requiredEDTTransactionMessage );
			}

			// Add WarningSummary EDT Transaction Message
			if ( cmePackage.GuidanceMessages.Count( gm => gm.LevelID == (int) GuidanceLevel.Warning && !gm.Voided ) > 0 )
			{
				var warningEDTTransactionMessage = new EDTTransactionMessage
				{
					EDTTransaction = cmePackage.EDTTransaction,
					TypeID = (int) EDTTransactionMessageType.Warning,
					Message = "Found " + requiredGuidanceMessageCount + " Warning Guidance Messages."
				};
				warningEDTTransactionMessage.SetCommonFields( currentUserID: Repository.ContextAccountID, creating: true );
				cmePackage.EDTTransactionMessages.Add( warningEDTTransactionMessage );
			}

			// Add AdvisorySummary EDT Transaction Message
			if ( cmePackage.GuidanceMessages.Count( gm => gm.LevelID == (int) GuidanceLevel.Advisory && !gm.Voided ) > 0 )
			{
				var advisoryEDTTransactionMessage = new EDTTransactionMessage
				{
					EDTTransaction = cmePackage.EDTTransaction,
					TypeID = (int) EDTTransactionMessageType.Advisory,
					Message = "Found " + requiredGuidanceMessageCount + " Advisory Guidance Messages."
				};
				advisoryEDTTransactionMessage.SetCommonFields( currentUserID: Repository.ContextAccountID, creating: true );
				cmePackage.EDTTransactionMessages.Add( advisoryEDTTransactionMessage );
			}
		}

		#endregion CME Package Populate EDTTranactionMessages

		#region CME Package Update CMEBatch MetaData

		/// <summary>
		/// Method is responsible for updating CMEBatch properties CallerContextID, InspectionCount,
		/// ViolationCount, EnforcementCount, and EnforcementViolationCount for the CMEPackage.
		/// </summary>
		/// <param name="cmePackage"></param>
		/// <param name="callerContext"></param>
		public void PopulateCMEBatchMetaData( CMEPackage cmePackage )
		{
			if ( cmePackage == null )
			{
				throw new ArgumentNullException( "cmePackage" );
			}

			// Set Caller Context Accordingly:
			cmePackage.CMEBatch.CallerContextID = (int) TransactionScope.CallerContext;

			// Calculate CME Data Counts.  Note, we do not filter out Voided records for these
			// counts, because the CMEBatch may have been responsible for setting some of those
			// Voided flags:
			cmePackage.CMEBatch.InspectionCount = cmePackage.CMEBatch.Inspections.Count;
			cmePackage.CMEBatch.ViolationCount = cmePackage.CMEBatch.Violations.Count;
			cmePackage.CMEBatch.EnforcementCount = cmePackage.CMEBatch.Enforcements.Count;
			cmePackage.CMEBatch.EnforcementViolationCount = cmePackage.CMEBatch.EnforcementViolations.Count;
		}

		#endregion CME Package Update CMEBatch MetaData

		#region CME Package Generate RegulatorEDTTransaction Records

		/// <summary>
		/// Method iterates through all CME Entities in a CMEPackage, and returns a distinct
		/// list of Regulator IDs for those CME Entities.  Method is used to help populate
		/// RegulatorEDTTransaction.
		/// </summary>
		/// <param name="cmePackage"></param>
		/// <returns></returns>
		public List<int> GenerateDistinctAffectedRegulatorIDs( CMEPackage cmePackage )
		{
			if ( cmePackage == null )
			{
				throw new ArgumentNullException( "cmePackage" );
			}

			// Iterate through all CME Data Entities in this batch, generating a list of
			// RegulatorIDs associated with this batch:
			List<int> regulatorIDList = new List<int>();
			foreach ( var inspection in cmePackage.CMEBatch.Inspections.Where( i => i.RegulatorID > 0 ) )
			{
				regulatorIDList.Add( inspection.RegulatorID );
			}
			foreach ( var violation in cmePackage.CMEBatch.Violations.Where( v => v.RegulatorID > 0 ) )
			{
				regulatorIDList.Add( violation.RegulatorID );
			}
			foreach ( var enforcement in cmePackage.CMEBatch.Enforcements.Where( e => e.RegulatorID > 0 ) )
			{
				regulatorIDList.Add( enforcement.RegulatorID );
			}
			foreach ( var enforcementViolation in cmePackage.CMEBatch.EnforcementViolations.Where( ev => ev.RegulatorID > 0 ) )
			{
				regulatorIDList.Add( enforcementViolation.RegulatorID );
			}

			return regulatorIDList.Distinct().ToList();
		}

		/// <summary>
		/// Method accepts a list of Regulator IDs and binds the current EDTTransaction
		/// (defined in the TransactionScope) to those Regulators by calling
		/// TransactionScope.ConnectRegulatorToTransaction(regulatorID) for each.
		/// </summary>
		/// <param name="regulatorIDList"></param>
		public void GenerateRegulatorEDTTransactions( List<int> regulatorIDList )
		{
			if ( regulatorIDList.Distinct() == null )
			{
				throw new ArgumentNullException( "regulatorIDList.Distinct()" );
			}

			// Generate and bind an instance of RegulatorEDTTransaction to EDTTransaction
			// for each distinct Regulator ID found:
			foreach ( var regulatorID in regulatorIDList.Distinct() )
			{
				TransactionScope.Connect( regulatorID );
			}
		}

		#endregion CME Package Generate RegulatorEDTTransaction Records
	}
}