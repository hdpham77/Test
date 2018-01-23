using CERS.Model;
using CERS.Xml;
using CERS.Xml.FacilitySubmittal;
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
	public class RegulatorFacilitySubmittalActionNotificationAdapter : InboundAdapter
	{
		#region Constructor

		public RegulatorFacilitySubmittalActionNotificationAdapter( EDTTransactionScope transactionScope )
			: base( transactionScope )
		{
		}

		#endregion Constructor

		#region Process Method(s)

		public XElement Process( Stream stream, int regulatorCode )
		{
			//make serializers needed
			RegulatorFacilitySubmittalResponseXmlSerializer responseSerializer = new RegulatorFacilitySubmittalResponseXmlSerializer( Repository );
			RegulatorFacilitySubmittalActionNotificationXmlSerializer serializer = new RegulatorFacilitySubmittalActionNotificationXmlSerializer( Repository );

			//contextRegulator is always the regulator this Processor is working under the context of. In this case, the assumption is, this Regulator is
			//ALWAYS a CUPA.
			Regulator contextRegulator = Repository.Regulators.GetByEDTIdentityKey( regulatorCode );
			if ( contextRegulator == null )
			{
				throw new ArgumentException( "RegulatorCode does not contain a valid number.", "regulatorCode" );
			}

			TransactionScope.Connect( contextRegulator );

			XmlSchemaValidationResult schemaValidationResult = null;

			var rfsanXml = ConvertStreamToXDocument( stream );
			if ( rfsanXml != null )
			{
				string xml = rfsanXml.ToString();
				TransactionScope.SaveXml( xml, EDTTransactionXmlDirection.Inbound );
				TransactionScope.WriteActivity( "XML Document is validating..." );

				schemaValidationResult = xml.IsValidAgainstSchema( XmlSchema.RegulatorFacilitySubmittalActionNotification );
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

					//deserialize the XML into an object graph package.
					var package = serializer.Deserialize( rfsanXml );
					TransactionScope.SetEDTClientKey( package.RegulatorTransactionKey );
					int totalErrorCount = 0;
					int errorCount = 0;

					//lets analyze and process each one.
					foreach ( var se in package.Packets )
					{
						errorCount = 0;
						var targetSE = Repository.FacilitySubmittalElements.FindByKey( se.CERSUniqueKey.Value );
						if ( targetSE != null )
						{
							//verify the CERSID from the XML matches the FSE
							if ( targetSE.CERSID != se.CERSID.Value )
							{
								errorCount++;
								TransactionScope.WriteMessage( "Facility Submittal Element with CERSUniqueKey '" + se.CERSUniqueKey.Value.ToString() + "' does not belong to CERSID '" + se.CERSID.Value + "'.", EDTTransactionMessageType.Error );
							}

							//Verify the SubmittalElementID from the XML matches the FSE
							if ( targetSE.SubmittalElementID != (int) se.SubmittalElement.Value )
							{
								errorCount++;
								TransactionScope.WriteMessage( "Facility Submittal Element with CERSUniqueKey '" + se.CERSUniqueKey.Value.ToString() + "' is not a Submittal Element of type '" + (int) se.SubmittalElement.Value + "'.", EDTTransactionMessageType.Error );
							}

							if ( targetSE.SubmittalActionDateTime.HasValue )
							{
								//verify the SubmittalActionDateTime on the existing FSE is prior to the SubmittalActionDateTime on the targetFSE (from XML)
								if ( se.ActionOn.Value < targetSE.SubmittalActionDateTime.Value )
								{
									TransactionScope.WriteMessage( "The SubmittalActionOn value must be greater than the existing SubmittalActionOn value (" + targetSE.SubmittalActionDateTime.Value.ToString() + ") for Submittal Element with CERSUniqueKey " + se.CERSUniqueKey.Value.ToString() + ".", EDTTransactionMessageType.Error );
								}
							}

							//Get the Facility, and make sure it exists first.
							Facility facility = Repository.Facilities.GetByID( targetSE.CERSID );
							if ( facility != null )
							{
								//figure out the CUPA.
								var CUPARegulator = Repository.Facilities.GetCUPA( facility );
								if ( CUPARegulator != null )
								{
									//Ensure that the Facilities CUPA is associated with the CUPA calling this processor.
									if ( contextRegulator.ID != CUPARegulator.ID )
									{
										errorCount++;
										TransactionScope.WriteMessage( "Facility Submittal Element with CERSUniqueKey '" + se.CERSUniqueKey.Value.ToString() + "' is assigned to a CUPA the current account is not authorized to submit for.", EDTTransactionMessageType.Error );
									}
								}
								else
								{
									errorCount++;
									TransactionScope.WriteMessage( "Cannot identify the CUPA for the Facility Submittal Element with CERSUniqueKey '" + se.CERSUniqueKey.Value.ToString() + "'.", EDTTransactionMessageType.Error );
								}
							}
							else
							{
								errorCount++;
								TransactionScope.WriteMessage( "A facility with CERSID '" + targetSE.CERSID + "' cannot be found and therefore Facility Submittal Element with CERSUniqueKey '" + se.CERSUniqueKey.Value.ToString() + "' cannot be updated/found.", EDTTransactionMessageType.Error );
							}

							//Get the regulator that is described in the XML as the Regulating body that is changing the submittal action information.
							var regulator = Repository.Regulators.GetByEDTIdentityKey( se.RegulatorCode.Value );
							if ( regulator != null )
							{
								se.TargetRegulator = regulator;
							}
							else
							{
								errorCount++;
								TransactionScope.WriteMessage( "Facility Submittal Element with CERSUniqueKey '" + se.CERSUniqueKey.Value.ToString() + "' specifies '" + se.RegulatorCode.Value + "' for the RegulatorCode element which is not valid.", EDTTransactionMessageType.Error );
							}

							//make sure that NextDueDate is in the future.
							if ( se.NextDueDate != null )
							{
								if ( se.NextDueDate.Value < DateTime.Now )
								{
									errorCount++;
									TransactionScope.WriteMessage( "Facility Submittal Element with CERSUniqueKey '" + se.CERSUniqueKey.Value.ToString() + "' has a NextDueDate of '" + se.NextDueDate.Value + "' which occurs in the past and is not allowed.", EDTTransactionMessageType.Error );
								}
							}
						}
						else
						{
							errorCount++;
							TransactionScope.WriteMessage( "Facility Submittal Element with CERSUniqueKey '" + se.CERSUniqueKey.Value.ToString() + "' was not found.", EDTTransactionMessageType.Error );
						}

						if ( errorCount == 0 )
						{
							se.FacilitySubmittalElement = targetSE;
						}

						totalErrorCount += errorCount;
					}

					if ( totalErrorCount > 0 )
					{
						TransactionScope.WriteActivity( "Found " + totalErrorCount + " total error(s) with package. Setting to Rejected." );
						TransactionScope.Complete( EDTTransactionStatus.Rejected );
					}
					else
					{
						try
						{
							FacilitySubmittalElement fse = null;

							//all is good, so lets update them all and process it.
							foreach ( var se in package.Packets )
							{
								fse = se.FacilitySubmittalElement;
								if ( fse != null )
								{
									ServiceManager.BusinessLogic.SubmittalElements.UpdateFacilitySubmittalElementStatus
										(
										fse,
										se.Status.Value,
										se.ActionOn,
										se.Comments,
										se.NextDueDate,
										Repository.ContextAccount,
										se.TargetRegulator.ID,
										se.AgentName,
										se.AgentEmail,
										submittalActionEDTTransaction: TransactionScope.Transaction
										);

									TransactionScope.Connect( fse );
								}
							}
							TransactionScope.WriteActivity( "All Facility Submittal Elements (" + package.Packets + ") updated successfully." );
							TransactionScope.SetStatus( EDTTransactionStatus.Accepted );
							TransactionScope.SetProcessedOn();
						}
						catch ( Exception ex )
						{
							TransactionScope.WriteActivity( "Errors occurred persisting updates to database.", ex );
							TransactionScope.Complete( EDTTransactionStatus.ErrorProcessing );
						}
					}
				}
			}
			else
			{
				TransactionScope.WriteMessage( "No package received.", EDTTransactionMessageType.Error );
				TransactionScope.Complete( EDTTransactionStatus.ErrorProcessing );
			}

			XElement responseXml = responseSerializer.PreparePackage( TransactionScope.Transaction );
			responseSerializer.AddTransaction( responseXml, TransactionScope.Transaction );
			responseSerializer.AddSummary( responseXml, TransactionScope.Transaction );
			TransactionScope.SaveXml( responseXml, EDTTransactionXmlDirection.Outbound );
			return responseXml;
		}

		public override XElement Process( Stream stream, NameValueCollection arguments )
		{
			int regulatorCode = arguments.GetValue( "RegulatorCode", 0 );
			return Process( stream, regulatorCode );
		}

		#endregion Process Method(s)

		internal class RegulatorSubmittalElement
		{
			public int RegulatorID { get; set; }

			public int SubmittalElementID { get; set; }
		}
	}
}