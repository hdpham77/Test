using CERS.Compositions;
using CERS.Model;
using CERS.Xml;
using CERS.Xml.FacilityManagement;
using CERS.Xml.FacilitySubmittal;
using CERS.Xml.StorageObjects;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UPF;

namespace CERS.EDT
{
	public class RegulatorFacilityMetadataAdapter : InboundAdapter
	{
		#region Constructor

		public RegulatorFacilityMetadataAdapter( EDTTransactionScope transactionScope )
			: base( transactionScope )
		{
		}

		#endregion Constructor

		public override XElement Process( Stream stream, NameValueCollection arguments )
		{
			int regulatorCode = arguments.GetValue( "RegulatorCode", 0 );
			return Process( stream, regulatorCode );
		}

		public XElement Process( Stream stream, int regulatorCode )
		{
			RegulatorFacilityMetadataResponseXmlSerializer responseSerializer = new RegulatorFacilityMetadataResponseXmlSerializer( Repository );
			Regulator regulator = Repository.Regulators.GetByEDTIdentityKey( regulatorCode );
			if ( regulator == null )
			{
				throw new ArgumentException( "RegulatorCode does not contain a valid number.", "regulatorCode" );
			}

			TransactionScope.Connect( regulator );

			//declare the package to be use to inflate the XML.
			RegulatorXmlObjectPacket<RegulatorFacilityMetadataPackage> package = null;

			XmlSchemaValidationResult schemaValidationResult = null;
			var rfmXml = ConvertStreamToXDocument( stream );
			if ( rfmXml != null )
			{
				string xml = rfmXml.ToString();
				TransactionScope.SaveXml( xml, EDTTransactionXmlDirection.Inbound );
				TransactionScope.WriteActivity( "XML Document is validating..." );

				schemaValidationResult = xml.IsValidAgainstSchema( XmlSchema.RegulatorFacilityMetadata );
				if ( !schemaValidationResult.IsValid )
				{
					//package is invalid due to schema validation errors, update the transaction status write some info.
					//Merge in the schema validation error messages and associate them with the transaction.

					TransactionScope.WriteActivity( "XML Document is not valid against the schema." );
					foreach ( var error in schemaValidationResult.Errors )
					{
						TransactionScope.WriteMessage( "Schema Validation Error: " + error, EDTTransactionMessageType.Error );
					}
					TransactionScope.WriteMessage( "XML document does not validate against the schema.", EDTTransactionMessageType.Error );
					TransactionScope.Complete( EDTTransactionStatus.Rejected );
				}
				else
				{
					TransactionScope.WriteActivity( "XML Document is valid against the schema." );

					//deserialize the data into the object structure.
					RegulatorFacilityMetadataXmlSerializer serializer = new RegulatorFacilityMetadataXmlSerializer( Repository );
					package = serializer.Deserialize( rfmXml );

					TransactionScope.SetEDTClientKey( package.RegulatorTransactionKey );

					package.Packet.Workspace.Facility = Repository.Facilities.GetByID( package.Packet.CERSID );
					if ( package.Packet.Workspace.Facility == null )
					{
						TransactionScope.WriteMessage( "There is no facility with CERSID " + package.Packet.CERSID, EDTTransactionMessageType.Error );
						package.Packet.Workspace.Messages.Add( "There is no facility with CERSID " + package.Packet.CERSID, FacilityOperationMessageType.Error );
						TransactionScope.Complete( EDTTransactionStatus.Rejected );
					}
					else
					{
						//get the original FRSE Mappings
						var originalFRSEMappings = GetOriginalFRSEMappings( package.Packet.CERSID );

						//merge the XML data into the existing FRSE objects.
						ServiceManager.Facilities.MergeFacilityRegulatorSubmittalElementCustomMappings( package.Packet, regulator.ID );

						//generate debug messages so we know what happened here.
						DetermineFacilityRegulatorSubmittalElementChanges( package, originalFRSEMappings );

						//merge the CERS Geo data in XML into the object graph.
						ServiceManager.Facilities.MergeCERSFacilityGeoPoint( package.Packet );

						//commit the changes to the database.
						CommitChanges( package );

						//complete the transaction indicating everything went according to plan.
						TransactionScope.Complete( EDTTransactionStatus.Accepted );
					}
				}
			}

			//prepare our output response XML.
			XElement responseXml = responseSerializer.PreparePackage( TransactionScope.Transaction );
			responseSerializer.AddTransaction( responseXml, TransactionScope.Transaction );
			if ( package != null )
			{
				responseSerializer.AddMessages( responseXml, package.Packet );
			}

			TransactionScope.SaveXml( responseXml, EDTTransactionXmlDirection.Outbound );
			return responseXml;
		}

		protected virtual void CommitChanges( RegulatorXmlObjectPacket<RegulatorFacilityMetadataPackage> package )
		{
			//if no errors, then submit to the database.
			if ( !package.Packet.Workspace.Messages.HasErrors() )
			{
				foreach ( var frse in package.Packet.Workspace.FacilityRegulatorSubmittalElements )
				{
					TransactionScope.Connect( frse );
					Repository.FacilityRegulatorSubmittalElements.Save( frse );
				}

				if ( package.Packet.Workspace.CERSFacilityGeoPoint != null )
				{
					if ( Repository.CERSFacilityGeoPoints.GetByID( package.Packet.Workspace.CERSFacilityGeoPoint.CERSID ) == null )
					{
						// have to create and not save because primary key is not zero
						Repository.CERSFacilityGeoPoints.Create( package.Packet.Workspace.CERSFacilityGeoPoint );
					}
					else
					{
						// record already exists
						Repository.CERSFacilityGeoPoints.Update( package.Packet.Workspace.CERSFacilityGeoPoint );
					}
				}

				bool facilityUpdates = false;

				if ( !string.IsNullOrWhiteSpace( package.Packet.FacilityID ) )
				{
					package.Packet.Workspace.Facility.FacilityID = package.Packet.FacilityID;
					facilityUpdates = true;
				}

				if ( !string.IsNullOrWhiteSpace( package.Packet.FacilityRegulatorKey ) )
				{
					package.Packet.Workspace.Facility.FacilityRegulatorKey = package.Packet.FacilityRegulatorKey;
					facilityUpdates = true;
				}

				if ( facilityUpdates )
				{
					Repository.Facilities.Save( package.Packet.Workspace.Facility );
					TransactionScope.Connect( package.Packet.Workspace.Facility );
					UpdateDraftFacilityInformation( package );
				}
			}
		}

		protected virtual void DetermineFacilityRegulatorSubmittalElementChanges( RegulatorXmlObjectPacket<RegulatorFacilityMetadataPackage> package, List<OriginalFRSEMapping> originalFRSEMappings )
		{
			foreach ( var frse in package.Packet.Workspace.FacilityRegulatorSubmittalElements )
			{
				var originalFRSE = originalFRSEMappings.SingleOrDefault( p => p.SubmittalElementID == frse.SubmittalElementID );
				if ( originalFRSE != null )
				{
					if ( originalFRSE.NextDueDate != frse.NextDueDate )
					{
						TransactionScope.WriteActivity( frse.SubmittalElement.Name + " NextDueDate updated to: " + ( frse.NextDueDate.HasValue ? frse.NextDueDate.Value.ToShortDateString() : "NULL" ) + " from " + ( originalFRSE.NextDueDate.HasValue ? originalFRSE.NextDueDate.Value.ToShortDateString() : " NULL " ) );
					}

					if ( originalFRSE.ReportingRequirement != (ReportingRequirement) frse.ReportingRequirementID )
					{
						TransactionScope.WriteActivity( frse.SubmittalElement.Name + " Reporting Requirement updated to " + frse.ReportingRequirement.Name + " from " + originalFRSE.ReportingRequirementName );
					}

					if ( originalFRSE.RegulatorCode != frse.Regulator.EDTIdentityKey )
					{
						TransactionScope.WriteActivity( frse.SubmittalElement.Name + " RegulatorCode updated to " + frse.Regulator.EDTIdentityKey + " from " + originalFRSE.RegulatorCode );
					}
				}
			}
		}

		protected virtual List<OriginalFRSEMapping> GetOriginalFRSEMappings( int CERSID )
		{
			var originalFRSEMappings = ( from frse in Repository.FacilityRegulatorSubmittalElements.GetForFacility( CERSID )
										 select new OriginalFRSEMapping
										 {
											 SubmittalElementID = frse.SubmittalElementID,
											 CERSID = frse.CERSID,
											 RegulatorID = frse.RegulatorID,
											 RegulatorCode = frse.Regulator.EDTIdentityKey,
											 NextDueDate = frse.NextDueDate,
											 ReportingRequirement = (ReportingRequirement) frse.ReportingRequirementID,
											 ReportingRequirementName = frse.ReportingRequirement.Name
										 } ).ToList();

			return originalFRSEMappings;
		}

		protected virtual void UpdateDraftFacilityInformation( RegulatorXmlObjectPacket<RegulatorFacilityMetadataPackage> package )
		{
			if ( !string.IsNullOrWhiteSpace( package.Packet.FacilityID ) )
			{
				//need to update all DRAFTS with this latest information for the FacilityID.
				var frse = Repository.FacilityRegulatorSubmittalElements.GetForFacility( package.Packet.CERSID, SubmittalElementType.FacilityInformation );
				if ( frse != null )
				{
					if ( frse.DraftFacilitySubmittalElementID != null )
					{
						var draftFRSE = frse.DraftFacilitySubmittalElement;
						var bpooResource = draftFRSE.Resources.FirstOrDefault( p => p.ResourceTypeID == (int) ResourceType.BusinessOwnerOperatorIdentification && !p.Voided );
						if ( bpooResource != null )
						{
							var bpoo = bpooResource.BPOwnerOperators.FirstOrDefault( p => !p.Voided );
							if ( bpoo != null )
							{
								bpoo.FacilityID = package.Packet.FacilityID;
								Repository.BPOwnerOperators.Save( bpoo );
							}
						}
					}
				}
			}
		}

		public class OriginalFRSEMapping
		{
			public int CERSID { get; set; }

			public DateTime? NextDueDate { get; set; }

			public int RegulatorCode { get; set; }

			public int RegulatorID { get; set; }

			public ReportingRequirement ReportingRequirement { get; set; }

			public string ReportingRequirementName { get; set; }

			public int SubmittalElementID { get; set; }
		}
	}
}