using CERS.Model;
using CERS.Validation;
using CERS.Xml;
using CERS.Xml.FacilitySubmittal;
using CERS.Xml.StorageObjects;
using Ionic.Zip;
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
	/// <summary>
 /// This is for processing Regulator Facility Submittals, exepcted result should be
 /// RegulatorFaciltiySubmittalResponse.
 /// </summary>
	public class RegulatorFacilitySubmittalAdapter : InboundAdapter
	{
		#region Constructor

		public RegulatorFacilitySubmittalAdapter( EDTTransactionScope transactionScope )
			: base( transactionScope )
		{
		}

		#endregion Constructor

		#region Process Method (Override)

		public override XElement Process( Stream stream, NameValueCollection arguments )
		{
			int regulatorCode = arguments.GetValue( "RegulatorCode", 0 );
			return Process( stream, regulatorCode );
		}

		#endregion Process Method (Override)

		#region Process Method

		public virtual XElement Process( Stream stream, int regulatorCode )
		{
			//contextRegulator is always the regulator this Processor is working under the context of. In this case, the assumption is, this Regulator is
			//ALWAYS a CUPA.
			Regulator contextRegulator = Repository.Regulators.GetByEDTIdentityKey( regulatorCode );
			if ( contextRegulator == null )
			{
				throw new ArgumentException( "RegulatorCode does not contain a valid number.", "regulatorCode" );
			}

			TransactionScope.Connect( contextRegulator );

			ZipFile zipFile = null;
			XDocument rfsXml = null;

			if ( IsZipPackage( stream ) )
			{
				//get the zip file.
				zipFile = ZipFile.Read( stream );

				if ( ValidateZip( zipFile ) )
				{
					TransactionScope.WriteActivity( "The ZipFile passed validation." );

					//so now that we have validated the zip file's package integrity, lets pull the XML file out
					//zipFile = ZipFile.Read(stream);
					rfsXml = ExtractXmlFile( zipFile );
				}
				else
				{
					TransactionScope.WriteActivity( "The ZipFile failed validation." );
					TransactionScope.WriteMessage( "The ZipFile failed validation.", EDTTransactionMessageType.Error );
					TransactionScope.SetStatus( EDTTransactionStatus.Rejected );
				}
			}
			else
			{
				//there is no Zip File, so convert the Stream to XML.
				rfsXml = ConvertStreamToXDocument( stream );
			}

			if ( rfsXml != null )
			{
				TransactionScope.SaveXml( rfsXml, EDTTransactionXmlDirection.Inbound );
			}

			try
			{
				if ( rfsXml != null && ValidateXml( rfsXml, XmlSchema.RegulatorFacilitySubmittal ) )
				{
					//at this point, at the high level the package appears to be good, so lets deserialize it, this part sucks...
					//call the Serializer for this type of schema that will deserialize the XML into object graphs
					RegulatorFacilitySubmittalXmlSerializer serializer = new RegulatorFacilitySubmittalXmlSerializer( Repository );
					var package = serializer.Deserialize( rfsXml, zipFile );

					if ( VerifyPackageRules( package ) )
					{
						TransactionScope.SetEDTClientKey( package.RegulatorTransactionKey );
						if ( ValidateCommitPackage( package ) )
						{
                            // queue up submittal deltas for submitted FSE's
                            if ( package.Packets != null )
                            {
                                foreach ( RegulatorFacilitySubmittalPacket packet in package.Packets )
                                {
                                    var submittedSubmittalElements = (from sp in packet.FacilitySubmittalElementPackets select sp.FacilitySubmittalElement).ToList();
                                    ServiceLocator.GetSystemServiceManager( Repository ).SubmittalDelta.QueueDeltaFSEs( submittedSubmittalElements );
                                }
                            }
							TransactionScope.Complete( EDTTransactionStatus.Accepted );
						}
						else
						{
							TransactionScope.Complete( EDTTransactionStatus.Rejected );
						}
					}
					else
					{
						//package verification failed.
						TransactionScope.Complete( EDTTransactionStatus.Rejected );
					}
				}
				else
				{
					//either we didn't get XML or we did but it didn't validate.
					TransactionScope.Complete( EDTTransactionStatus.Rejected );
				}
			}
			catch ( Exception ex )
			{
				Repository.ClearObjectContextState();
				TransactionScope.WriteMessage( ex.Message, EDTTransactionMessageType.Error );
				TransactionScope.WriteActivity( "An error occurred while attempting to process the XML.", ex, type: EDTTransactionMessageType.Error );
				TransactionScope.Complete( EDTTransactionStatus.Rejected );
			}

			var responseXml = PrepareResponsePackage();
			TransactionScope.SaveXml( responseXml, EDTTransactionXmlDirection.Outbound );
			return responseXml;
		}

		protected virtual bool VerifyPackageRules( RegulatorXmlObjectCollectionPackage<RegulatorFacilitySubmittalPacket> package )
		{
			bool result = true;

			//this method verifies that the SubmittedOn value for each facility submittal and submittal element is unique for that submittal element and facility (CERSID).
			//its also meant to verify other data submitted through XML. This verifies the data deserialized from XML before it
			//goes through the main business logic validations sequence.

			foreach ( var rfsPacket in package.Packets )
			{
				foreach ( var fsePacket in rfsPacket.FacilitySubmittalElementPackets )
				{
					if ( Repository.FacilitySubmittalElements.IsDuplicate( fsePacket.FacilitySubmittalElement ) )
					{
						TransactionScope.WriteMessage( "Submittal already exists for " + ( (SubmittalElementType)fsePacket.FacilitySubmittalElement.SubmittalElementID ).ToString() + " for CERSID " + fsePacket.FacilitySubmittalElement.CERSID + " with submittedOn value (" + fsePacket.FacilitySubmittalElement.SubmittedDateTime.Value.ToString() + ").", EDTTransactionMessageType.Error );
						result = false;
					}
				}
			}

			return result;
		}

		#endregion Process Method

		#region ValidateCommitPackage Method

		protected virtual bool ValidateCommitPackage( RegulatorFacilitySubmittalPacket fsPacket )
		{
			if ( fsPacket == null )
			{
				throw new ArgumentException( "fsPacket" );
			}

			bool result = false;

			//lets create our FacilitySubmittal, and default it to voided so it stay's hidden until
			//after all validations are processed and fixed up.
			fsPacket.FacilitySubmittal = new FacilitySubmittal();
			fsPacket.FacilitySubmittal.ReceivedOn = DateTime.Now;
			fsPacket.FacilitySubmittal.TransactionID = TransactionScope.Transaction.ID;

			//use the Facility Information submittal elements SubmittedDateTime value for this FacilitySubmittal's SubmittedOn field.
			var facInfoFSE = fsPacket.FacilitySubmittalElementPackets.SingleOrDefault( p => p.Type == SubmittalElementType.FacilityInformation );
			if ( facInfoFSE != null )
			{
				fsPacket.FacilitySubmittal.SubmittedOn = facInfoFSE.FacilitySubmittalElement.SubmittedDateTime.Value;
			}

			fsPacket.FacilitySubmittal.SetCommonFields( ContextAccountID, creating: true, voided: true );
			Repository.FacilitySubmittals.Save( fsPacket.FacilitySubmittal );

			foreach ( var fsePacket in fsPacket.FacilitySubmittalElementPackets )
			{
				//if (fsePacket.Header.RegulatorID == )
				//TODO: Add FRSE.NextDueDate validation here, add regulator validation here as well.
				//do validation
				fsePacket.FacilitySubmittalElementValidationResult = fsePacket.FacilitySubmittalElement.Validate( Repository, CallerContext.EDT );

				if ( fsePacket.Type == SubmittalElementType.FacilityInformation )
				{
					//process additional business rules information.
					PreProcessFacilityInformationSubmittalElementBusinessLogic( fsePacket.FacilitySubmittalElement );
				}

				//attach the FSE to the fs
				fsePacket.FacilitySubmittalElement.FacilitySubmittalID = fsPacket.FacilitySubmittal.ID;

				//save our stuff.
				Repository.FacilitySubmittalElements.Update( fsePacket.FacilitySubmittalElement );

				//commit the validation results.
				fsePacket.FacilitySubmittalElementValidationResult.CommitValidationResults( TransactionScope );
			}

			return result;
		}

		protected virtual bool ValidateCommitPackage( RegulatorXmlObjectCollectionPackage<RegulatorFacilitySubmittalPacket> package )
		{
			if ( package == null )
			{
				throw new ArgumentException( "package" );
			}

			bool result = false;

			foreach ( var fsPacket in package.Packets )
			{
				ValidateCommitPackage( fsPacket );
			}

			//see if we have any invalid FacilitySubmittal Packets
			int invalidCount = package.Packets.Where( p => !p.IsValid ).Count();

			//if not, we are good to go to officially say we are ok with this package and flip all the voided to false and update metadata.
			if ( invalidCount == 0 )
			{
				result = true;
				//now we need to go through and trigger all the updates to flip the voided flag on everything.
				foreach ( var fsPacket in package.Packets )
				{
					//change voided to false for each and every FacilitySubmittalElement.
					foreach ( var fsePacket in fsPacket.FacilitySubmittalElementPackets )
					{
						if ( fsePacket.Type == SubmittalElementType.FacilityInformation )
						{
							//process additional business rules information.
							PostProcessFacilityInformationSubmittalElementBusinessLogic( fsePacket.FacilitySubmittalElement );
							
							//If the Facility Information FSE has a Status of "Accepted", call Business Service method to update Facility from Facility Information FSE.
							//This ensures that any changes to important Facility data such as Name, Address, FacilityID, EPAID, etc. that are accepted by the Regulator 
							//get propegated to the Facility table
							if (fsePacket.FacilitySubmittalElement.StatusID == (int)SubmittalElementStatus.Accepted)
							{
								ServiceManager.BusinessLogic.SubmittalElements.FacilityInformation.UpdateFacilityFromFacilityInformation(fsePacket.FacilitySubmittalElement);
							}
						}

						//update voided for FSE and save.
						fsePacket.FacilitySubmittalElement.Voided = false;

						//save this submittal element.
						Repository.FacilitySubmittalElements.Save( fsePacket.FacilitySubmittalElement );

						TransactionScope.Connect( fsePacket.FacilitySubmittalElement );

						//update FRSE metadata.
						ProcessFacilityRegulatorSubmittalElementMetadata( fsPacket.Facility, fsePacket );
						UpdateFacilityRegulatorSubmittalElement( fsPacket.Facility, fsePacket );
					}

					//if we got a FacilitySubmittal lets update it's voided to false as well and update it.
					if ( fsPacket.FacilitySubmittal != null )
					{
						fsPacket.FacilitySubmittal.Voided = false;
						Repository.FacilitySubmittals.Save( fsPacket.FacilitySubmittal );
					}

					ServiceManager.Events.CreateFacilitySubmittalNotification( fsPacket.Facility, fsPacket.FacilitySubmittal );
					TransactionScope.Connect( fsPacket.FacilitySubmittal );
				}
			}

			return result;
		}

		#endregion ValidateCommitPackage Method

		#region FacilityInformation Business Logic Handlers

		#region PreProcessFacilityInformationSubmittalElementBusinessLogic Method

		protected virtual void PreProcessFacilityInformationSubmittalElementBusinessLogic( FacilitySubmittalElement fse )
		{
			if ( fse == null )
			{
				throw new ArgumentNullException( "fse" );
			}

			if ( fse.SubmittalElementID != (int)SubmittalElementType.FacilityInformation )
			{
				throw new ArgumentException( "The FacilitySubmittalElement is not a Facility Information submittal element.", "fse" );
			}

			//get each form again.
			var bpActivity = ServiceManager.FacilitySubmittalModelEntities.GetSingleEntity<BPActivity>( fse, ResourceType.BusinessActivities );
			if ( bpActivity != null )
			{
				ServiceManager.BusinessLogic.SubmittalElements.FacilityInformation.PreSaveActivitiesForm( fse, bpActivity, CallerContext.EDT );
			}

			var bpOwnerOperator = ServiceManager.FacilitySubmittalModelEntities.GetSingleEntity<BPOwnerOperator>( fse, ResourceType.BusinessOwnerOperatorIdentification );
			if ( bpOwnerOperator != null )
			{
				ServiceManager.BusinessLogic.SubmittalElements.FacilityInformation.PreSaveOwnerOperatorForm( fse, bpOwnerOperator, CallerContext.EDT );
			}
		}

		#endregion PreProcessFacilityInformationSubmittalElementBusinessLogic Method

		#region PostProcessFacilityInformationSubmittalElementBusinessLogic Method

		protected virtual void PostProcessFacilityInformationSubmittalElementBusinessLogic( FacilitySubmittalElement fse )
		{
			if ( fse == null )
			{
				throw new ArgumentNullException( "fse" );
			}

			if ( fse.SubmittalElementID != (int)SubmittalElementType.FacilityInformation )
			{
				throw new ArgumentException( "The FacilitySubmittalElement is not a Facility Information submittal element.", "fse" );
			}

			//get each form again.
			var bpActivity = ServiceManager.FacilitySubmittalModelEntities.GetSingleEntity<BPActivity>( fse, ResourceType.BusinessActivities );
			if ( bpActivity != null )
			{
				ServiceManager.BusinessLogic.SubmittalElements.FacilityInformation.PostSaveActivitiesForm( fse, bpActivity, CallerContext.EDT );
			}

			var bpOwnerOperator = ServiceManager.FacilitySubmittalModelEntities.GetSingleEntity<BPOwnerOperator>( fse, ResourceType.BusinessOwnerOperatorIdentification );
			if ( bpOwnerOperator != null )
			{
				ServiceManager.BusinessLogic.SubmittalElements.FacilityInformation.PostSaveOwnerOperatorForm( fse, bpOwnerOperator, CallerContext.EDT );
			}
		}

		#endregion PostProcessFacilityInformationSubmittalElementBusinessLogic Method

		#endregion FacilityInformation Business Logic Handlers

		#region ProcessFacilityRegulatorSubmittalElementMetadata Method

		protected virtual void ProcessFacilityRegulatorSubmittalElementMetadata( Facility facility, FacilitySubmittalElementPacket<RegulatorFacilitySubmittalElementHeaderXmlObject> fsePacket )
		{
			//if the xml data included a new NextDueDate and/or Reporting Requirement lets find the FacilityRegulatorSubmittalElement (FRSE) and update it
			//with the new data bits.
			if ( fsePacket.Header.NextDueDate != null )
			{
				//find the FRSE for this Facility and Submittal Element Type.
				fsePacket.FacilityRegulatorSubmittalElement = Repository.FacilityRegulatorSubmittalElements.GetForFacility( facility, fsePacket.Type );

				//if NextDueDate was specified lets update it.
				if ( fsePacket.Header.NextDueDate != null )
				{
					//=> SubmittalActionDateTime

					fsePacket.FacilityRegulatorSubmittalElement.NextDueDate = fsePacket.Header.NextDueDate;
					TransactionScope.Connect( fsePacket.FacilityRegulatorSubmittalElement );
				}

				//update the FRSE
				Repository.FacilityRegulatorSubmittalElements.Save( fsePacket.FacilityRegulatorSubmittalElement );
			}
		}

		#endregion ProcessFacilityRegulatorSubmittalElementMetadata Method

		protected virtual FacilitySubmittalElement GetMostRecentFSE( FacilitySubmittalElementPacket<RegulatorFacilitySubmittalElementHeaderXmlObject> fsePacket )
		{
			SubmittalElementStatus status = (SubmittalElementStatus)fsePacket.FacilitySubmittalElement.StatusID;
			SubmittalElementType type = (SubmittalElementType)fsePacket.FacilitySubmittalElement.SubmittalElementID;

			return GetMostRecentFSE( fsePacket.FacilitySubmittalElement.CERSID, type, status, fsePacket.FacilitySubmittalElement.ID );
		}

		protected virtual FacilitySubmittalElement GetMostRecentFSE( int CERSID, SubmittalElementType type, SubmittalElementStatus status, int thisFSEID )
		{
			var result = Repository.FacilitySubmittalElements
				.Search( CERSID: CERSID, type: type, status: status )
				.Where( p => p.ID != thisFSEID )
				.OrderByDescending( p => p.SubmittedDateTime )
				.FirstOrDefault();

			return result;
		}

		protected virtual void UpdateFacilityRegulatorSubmittalElement( Facility facility, FacilitySubmittalElementPacket<RegulatorFacilitySubmittalElementHeaderXmlObject> fsePacket )
		{
			FacilityRegulatorSubmittalElement frse = Repository.FacilityRegulatorSubmittalElements.GetForFacility( facility.CERSID, fsePacket.FacilitySubmittalElement.SubmittalElementID );

			var status = (SubmittalElementStatus)fsePacket.FacilitySubmittalElement.StatusID;
			var type = (SubmittalElementType)fsePacket.FacilitySubmittalElement.SubmittalElementID;
			var submittedOn = fsePacket.FacilitySubmittalElement.SubmittedDateTime.Value;

			bool frseUpdated = false;

			//if this submittal is accepted, then lets see if this is the most recent accepted.
			//if so, then update the FRSE with the LastAccepted PK and date/time.
            if ( status == SubmittalElementStatus.Accepted )
            {
                var lastAccepted = GetMostRecentFSE( fsePacket );
                if ( ( lastAccepted != null && submittedOn > lastAccepted.SubmittedDateTime.Value )
                    || ( frse.LastAcceptedFacilitySubmittalElementOn == null || ( frse.LastAcceptedFacilitySubmittalElementOn != null && frse.LastAcceptedFacilitySubmittalElementOn.Value < fsePacket.FacilitySubmittalElement.SubmittalActionDateTime ) ) )
                {
                    frse.LastAcceptedFacilitySubmittalElementID = fsePacket.FacilitySubmittalElement.ID;
                    frse.LastAcceptedFacilitySubmittalElementOn = fsePacket.FacilitySubmittalElement.SubmittalActionDateTime;
                    frseUpdated = true;
                }
            }

			//now see if this is the most recent facility submittal element received for this submittal element.
			//if so, then lets update the LastSubmitted fields.
			var results = Repository.FacilitySubmittalElements.Search( CERSID: facility.CERSID, type: type );
			results = from r in results where r.StatusID != (int)SubmittalElementStatus.Draft && r.ID != fsePacket.FacilitySubmittalElement.ID select r;
			var lastFSE = results.OrderByDescending( p => p.SubmittedDateTime ).FirstOrDefault();
            if ( ( lastFSE != null && submittedOn > lastFSE.SubmittedDateTime.Value )
                || ( frse.LastSubmittedFacilitySubmittalElementOn == null || ( frse.LastSubmittedFacilitySubmittalElementOn != null && frse.LastSubmittedFacilitySubmittalElementOn.Value < fsePacket.FacilitySubmittalElement.SubmittedDateTime ) ) )
            {
                frse.LastSubmittedFacilitySubmittalElementID = fsePacket.FacilitySubmittalElement.ID;
                frse.LastSubmittedFacilitySubmittalElementOn = fsePacket.FacilitySubmittalElement.SubmittedDateTime;
                frseUpdated = true;

            }

			Repository.FacilityRegulatorSubmittalElements.Save( frse );

			//update the last Fac Info
			if ( fsePacket.FacilitySubmittalElement.SubmittalElementID == (int)SubmittalElementType.FacilityInformation )
			{
				if ( ServiceManager.BusinessLogic.SubmittalElements.SetLastFacilityInfoSubmittal( fsePacket.FacilitySubmittalElement ) )
				{
					TransactionScope.Connect( fsePacket.FacilitySubmittalElement.Facility );
				}

				frseUpdated = true;
			}

			if ( frseUpdated )
			{
				TransactionScope.Connect( frse );
			}
		}

		#region PrepareResponsePackage Method

		protected virtual XElement PrepareResponsePackage()
		{
			//make serializers needed
			RegulatorFacilitySubmittalResponseXmlSerializer responseSerializer = new RegulatorFacilitySubmittalResponseXmlSerializer( Repository );
			XElement responseXml = responseSerializer.PreparePackage( TransactionScope.Transaction );
			responseSerializer.AddTransaction( responseXml, TransactionScope.Transaction );
			responseSerializer.AddSummary( responseXml, TransactionScope.Transaction );
			responseSerializer.AddEntityKeys( responseXml, TransactionScope.Transaction );
			responseSerializer.AddGuidanceMessages( responseXml, TransactionScope.Transaction );
			responseSerializer.AddSubmittalElementKeys( responseXml, TransactionScope.Transaction );
			return responseXml;
		}

		#endregion PrepareResponsePackage Method
	}
}