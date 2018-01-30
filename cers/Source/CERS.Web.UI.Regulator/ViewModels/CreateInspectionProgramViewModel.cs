using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using UPF.ViewModel;
using CERS.Model;

namespace CERS.Web.UI.Regulator.ViewModels
{
    /**
     * InspectionProgramGridViewModel is used by the NewInspectionViewModel to track
     * the programs inspected and details regarding the inspection results.
     */
    public class CreateInspectionProgramViewModel : ViewModel
    {
        // Track the Inspected Program:
        public CMEProgramElement CMEProgramElement { get; set; }

        // Track if the user indicates that this Inspection inspected this Program:
        public bool ProgramInspected { get; set; }

        // Track the Violatino Summary Counts (only used if entering an Inspection
        // using a ViolationContext of Summary):
        
        [Display(Name = "Class I Violation Count")]
        [DataRegistryMetadata("15.0009", DataRegistryDataSourceType.UPDD)]
        public int ClassIViolationCount { get; set; }

        [Display(Name = "Class II Violation Count")]
        [DataRegistryMetadata("15.0010", DataRegistryDataSourceType.UPDD)]
        public int ClassIIViolationCount { get; set; }

        [Display(Name = "Minor Violation Count")]
        [DataRegistryMetadata("15.0011", DataRegistryDataSourceType.UPDD)]
        public int MinorViolationCount { get; set; }

        [Display(Name = "Violations RTC")]
        [DataRegistryMetadata("15.0021", DataRegistryDataSourceType.UPDD)]
        public DateTime? ViolationsRTCOn { get; set; }

        [Display(Name = "Comment")]
        [DataRegistryMetadata("15.0027", DataRegistryDataSourceType.UPDD)]
        [StringLength(1000, ErrorMessage = "Comment cannot be greater than 1000 characters.")]
        public string Comment { get; set; }

        // Significant Operational Compliance (UST Program Only):
        [Display(Name = "SOCDetermination")]
        [DataRegistryMetadata("15.0012", DataRegistryDataSourceType.UPDD)]
        public string SOCDetermination { get; set; }

        // Track Date and Status of Latest Submittal for this Program Element:
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? LastSubmittalOn { get; set; }
        public string LastSubmittalStatus { get; set; }
        public int? LastFacilitySubmittalID { get; set; }

        // Track Current Regulator for this Program Element:
        public string RegulatingAgency { get; set; }
    }
}
