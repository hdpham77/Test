using CERS.Compositions;
using CERS.Model;
using CERS.Xml;
using CERS.Xml.FacilityManagement;
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
	public class RegulatorFacilityCreateAdapter : InboundAdapter
	{
		public RegulatorFacilityCreateAdapter( EDTTransactionScope transactionScope )
			: base( transactionScope )
		{
		}

		public XElement Process( Stream stream, int regulatorCode )
		{
			Regulator contextRegulator = Repository.Regulators.GetByEDTIdentityKey( regulatorCode );
			if ( contextRegulator == null )
			{
				throw new ArgumentException( "RegulatorCode does not contain a valid number.", "regulatorCode" );
			}

			//Connect the regulator after we verified it exists.
			TransactionScope.Connect( contextRegulator );

			//this endpoint supports multiple versions of the schema simultaneously.
			IXmlSchemaMetadata inputSchemaMetadata = null;
			IXmlSchemaMetadata outputSchemaMetadata = null;
			string version = null;

			RegulatorFacilityCreateResponseXmlSerializer responseSerializer = new RegulatorFacilityCreateResponseXmlSerializer( Repository );

			//lets default to Trial mode in case the XML is so messed up we can't parse it, and this is obviously less danagerous than
			//a Commit.
			RegulatorFacilityCreateMode mode = RegulatorFacilityCreateMode.Trial;

			//declare the package to be use to inflate the XML.
			RegulatorXmlObjectPacket<RegulatorFacilityCreateMode, FacilityCreatePackage> package = null;

			XmlSchemaValidationResult schemaValidationResult = null;
			var rfcXml = ConvertStreamToXDocument( stream );
			if ( rfcXml != null )
			{
				string xml = rfcXml.ToString();
				TransactionScope.SaveXml( xml, EDTTransactionXmlDirection.Inbound );
				TransactionScope.WriteActivity( "XML Document is validating..." );

				//get the root element of the doc (should be named RegulatorFacilityCreate).
				var rootElement = rfcXml.Root;

				//try and obtain the targetNamespace
				var targetNamespace = rootElement.GetDefaultNamespace();
				if ( string.IsNullOrWhiteSpace( targetNamespace.NamespaceName ) )
				{
					//couldn't obtain the schema we need because the root node didn't have a default non-prefixed xmlns attribute.
					//lets see of the document is using prefixes and look for the xmlns:cers="" attribute.
					targetNamespace = rootElement.GetNamespaceOfPrefix( "cers" );
				}

				//if we have no targetNamespace then we should not proceed any further.
				if ( targetNamespace == null || targetNamespace.NamespaceName == string.Empty )
				{
					TransactionScope.WriteMessage( "Unable to resolve the targetNamespace of the received XML data.", EDTTransactionMessageType.Error );
					throw new Exception( "Unable to resolve the targetNamespace of the received XML data." );
				}

				//find the metadata for the RegulatorFacilityCreate schema with the specified targetNamespace.
				inputSchemaMetadata = XmlSchemas.GetSchemaMetdataForNamespace( XmlSchema.RegulatorFacilityCreate, targetNamespace ); //XmlSchemas.GetSchemaMetadata( XmlSchema.RegulatorFacilityCreate, version );
				if ( inputSchemaMetadata == null )
				{
					TransactionScope.WriteMessage( "Unable to find a RegulatorFacilityCreate schema with the specified namespace: " + targetNamespace.NamespaceName, EDTTransactionMessageType.Error );
					throw new Exception( "Unable to find a RegulatorFacilityCreate schema with the specified version: " + version );
				}

				//the outgoing schema version is dependant on the version specified in the input schema.
				//so parse the version and then look for the appropriate output (response schema).
				version = targetNamespace.ExtractVersion();

				//now try and find the response schema with the version parsed from the targetSchema.
				outputSchemaMetadata = XmlSchemas.GetSchemaMetadata( XmlSchema.RegulatorFacilityCreateResponse, version );
				if ( outputSchemaMetadata == null )
				{
					TransactionScope.WriteMessage( "Unable to find a RegulatorFacilityCreateResponse schema with the specified version: " + version, EDTTransactionMessageType.Error );
					throw new Exception( "Unable to find a RegulatorFacilityCreateResponse schema with the specified version: " + version );
				}

				schemaValidationResult = xml.IsValidAgainstSchema( inputSchemaMetadata );
				if ( !schemaValidationResult.IsValid )
				{
					//make a new package so we have a storage area for the errors to be shipped back in the response xml.
					package = new RegulatorXmlObjectPacket<RegulatorFacilityCreateMode, FacilityCreatePackage>();

					//package is invalid due to schema validation errors, update the transaction status, and write some diagnostic information.
					//Merge in the schema validation error messages and associate them with the transaction.
					TransactionScope.WriteActivity( "XML Document is not valid against the schema." );
					package.Packet.Workspace.Messages.Add( "XML Document is not valid against the schema.", FacilityOperationMessageType.Error );

					foreach ( var error in schemaValidationResult.Errors )
					{
						TransactionScope.WriteMessage( "Schema Validation Error: " + error, EDTTransactionMessageType.Error );
						package.Packet.Workspace.Messages.Add( "Schema Validation Error: " + error, FacilityOperationMessageType.Error );
					}

					TransactionScope.WriteMessage( "XML document does not validate against the schema.", EDTTransactionMessageType.Error );
					TransactionScope.Complete( EDTTransactionStatus.Rejected );
				}
				else
				{
					TransactionScope.WriteActivity( "XML Document is valid against the schema." );

					//deserialize the data into the object structure.
					RegulatorFacilityCreateXmlSerializer serializer = new RegulatorFacilityCreateXmlSerializer( Repository );
					package = serializer.Deserialize( rfcXml );
					package.Packet.Version = version; //add in the version 4/1/2014.
					mode = package.Mode;

					// error if DataCollectionDate is less than 1970 and greater than today at 11:59
					if ( package.Packet.CERSPoint != null &&
						package.Packet.CERSPoint.DataCollectionDate != null &&
						( package.Packet.CERSPoint.DataCollectionDate < new DateTime( 1970, 1, 1 ) || package.Packet.CERSPoint.DataCollectionDate > DateTime.Now.AddDays( 1 ).AddTicks( -1 ) ) )
					{
						package.Packet.Workspace.Messages.Add( "DataCollectionDate is out of range.", FacilityOperationMessageType.Error );
					}

					if ( !package.Packet.HasErrors() )
					{
						ServiceManager.Facilities.Create( package.Packet, regulatorCode, mode, true );

						if ( mode == RegulatorFacilityCreateMode.Commit && package.Packet.Workspace.FacilityCreated )
						{
							TransactionScope.Connect( package.Packet.Workspace.Facility );
						}
					}

					//if the package had errors, set rejected, otherwise set accepted.
					if ( package.Packet.HasErrors() )
					{
						TransactionScope.Complete( EDTTransactionStatus.Rejected );
					}
					else
					{
						TransactionScope.Complete( EDTTransactionStatus.Accepted );
					}
				}
			}

			//prepare our output response XML.
			XElement responseXml = responseSerializer.PreparePackage( TransactionScope.Transaction, outputSchemaMetadata );
			responseSerializer.AddTransaction( responseXml, TransactionScope.Transaction, mode );

			//we should always have a package (unless schema validation failed).
			if ( package != null )
			{
				//add the result hive to the root node if we created a facility (or virtually did in trial mode)
				responseSerializer.AddResult( responseXml, package.Packet, mode );

				//if using the 1/06 schema version...
				if ( !string.IsNullOrWhiteSpace( package.Packet.Version ) && ( package.Packet.Version.IndexOf( "1/06" ) > -1 ) )
				{
					responseSerializer.AddPotentialDuplicates( responseXml, package.Packet );
				}
			}

			//add the messages generated during the process.
			responseSerializer.AddMessages( responseXml, package.Packet );

			TransactionScope.SaveXml( responseXml, EDTTransactionXmlDirection.Outbound );
			return responseXml;
		}

		public override XElement Process( Stream stream, NameValueCollection arguments )
		{
			int regulatorCode = arguments.GetValue( "RegulatorCode", 0 );
			return Process( stream, regulatorCode );
		}
	}
}