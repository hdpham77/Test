using System.Collections.Generic;
using System.Linq;
using CERS.Model;
using UPF.Core.Model;
using CERS.Web.UI.Regulator.ViewModels;

namespace CERS
{
    public static class ViewTransformationExtensionMethods
    {
        public static IEnumerable<CreateInspectionProgramViewModel> ToCreateInspectionProgramView(this IEnumerable<CMEProgramElement> cmeProgramElements, IEnumerable<FacilityRegulatorSubmittalElement> facilityRegulatorSubmittalElements)
        {
            var results = from c in cmeProgramElements
                          select new CreateInspectionProgramViewModel
                          {
                               CMEProgramElement = c,
                               ProgramInspected = false,
                               ClassIViolationCount = 0,
                               ClassIIViolationCount = 0,
                               MinorViolationCount = 0,
                               Comment = "",
                               SOCDetermination = "",

                               //bad table relationship design, however since it's already in full production need to manually remap Program Element 1 into Hazard Material Program Element 
                               //maybe we need to address this on CERS 3 development
                               LastSubmittalOn = ( from frse in facilityRegulatorSubmittalElements where ( c.ProgramElementID == 1 ? frse.SubmittalElementID == (int)SubmittalElementType.HazardousMaterialsInventory : frse.SubmittalElement.ProgramElementID == c.ProgramElementID ) && !frse.Voided select frse.LastSubmittedFacilitySubmittalElementOn ).FirstOrDefault(),
                               LastSubmittalStatus = ( from frse in facilityRegulatorSubmittalElements where ( c.ProgramElementID == 1 ? frse.SubmittalElementID == (int)SubmittalElementType.HazardousMaterialsInventory : frse.SubmittalElement.ProgramElementID == c.ProgramElementID ) && !frse.Voided && frse.LastSubmittedFacilitySubmittalElement != null select frse.LastSubmittedFacilitySubmittalElement.Status.Name ).FirstOrDefault(),
                               LastFacilitySubmittalID = ( from frse in facilityRegulatorSubmittalElements where ( c.ProgramElementID == 1 ? frse.SubmittalElementID == (int)SubmittalElementType.HazardousMaterialsInventory : frse.SubmittalElement.ProgramElementID == c.ProgramElementID ) && !frse.Voided && frse.LastSubmittedFacilitySubmittalElement != null select frse.LastSubmittedFacilitySubmittalElement.FacilitySubmittalID ).FirstOrDefault(),
                               RegulatingAgency = string.Join( ",", ( from frse in facilityRegulatorSubmittalElements where frse.SubmittalElement.ProgramElementID == c.ProgramElementID && !frse.Voided select frse.Regulator.NameAbbreviation ).Distinct().ToList() )
                          };

            return results;
        }

        public static CreateInspectionProgramViewModel ToCreateInspectionProgramView(this CMEProgramElement cmeProgramElement)
        {
            var result = new CreateInspectionProgramViewModel
                         {
                             CMEProgramElement = cmeProgramElement,
                             ProgramInspected = false,
                             ClassIViolationCount = 0,
                             ClassIIViolationCount = 0,
                             MinorViolationCount = 0,
                             Comment = "",
                             SOCDetermination = ""
                         };

            return result;
        }
    }
}