using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CERS.EDT.Web.Services
{
	public enum EDTServiceRoute
	{
		RegulatorAuthenticate,
		ChemicalLibrary,
		ViolationLibrary,
		DataDictionaryLibrary_Supplemental,
		DataDictionaryLibrary_System,
		DataDictionaryLibrary_UPDD,
		RegulatorServicesHelp,
		LibraryHelp,
		DataDictionaryLibrary_UPDD_WithIdentifier,
		DataDictionaryLibrary_System_WithIdentifier,
		DataDictionaryLibrary_Supplemental_WithIdentifier,
		RegulatorFacilitySubmittalDocumentQuery,
		RegulatorFacilitySubmittalQueryTransaction,
		RegulatorFacilitySubmittalQueryXml,
		RegulatorFacilitySubmittalSubmitTransaction,
		RegulatorFacilitySubmittalSubmit,
		RegulatorFacilitySubmittalActionNotifications,
		RegulatorFacilitySubmittalsHelp,
		CMESubmittalsHelp,
		CMESubmittalQueryTransaction,
		CMESubmittalQuery,
		CMESubmittalSubmitTransaction,
		CMESubmittalSubmit,
		ViolationLibrary_WithViolationNumber,
		ChemicalLibrary_WithIdentifier,
		Home,
		RegulatorFacilityQuery,
		RegulatorFacilityCreate,
		RegulatorFacilityInformationSubmittalSubmit,
		RegulatorFacilityMetadataSubmit,
		RegulatorListing,
		RegulatorOrganizationQuery,
		RegulatorActionItemQuery,
		RegulatorFacilityTransferQuery,
		EndpointMetadata
	}
}