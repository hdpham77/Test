using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using CERS.Model;
using CERS.ViewModels.Facilities;
using CERS.Web.Mvc;
using UPF.ViewModel;

namespace CERS.Web.UI.Organization.ViewModels
{
    public class NewFacilityViewModel : EntityGridViewModel<Facility, FacilityGridViewModel>
    {
        public NewFacilityWizardStep WizardStep { get; set; }

        [DataRegistryMetadata("10.0082", DataRegistryDataSourceType.UPDD)]
        [Display(Name = "Street Address")]
        [StringLength(70, ErrorMessage = "Street Address cannot be greater than 70 characters.")]
        public string Street { get; set; }

        [DataRegistryMetadata("10.0022", DataRegistryDataSourceType.UPDD)]
        [Display(Name = "City")]
        [StringLength(60, ErrorMessage = "City cannot be greater than 60 characters.")]
        public string City { get; set; }

        [DataRegistryMetadata("10.0023", DataRegistryDataSourceType.UPDD)]
        [Display(Name = "Zip Code")]
        [StringLength(10, ErrorMessage = "Zip Code cannot be greater than 10 characters.")]
        public string ZipCode { get; set; }

        [DataRegistryMetadata("10.0012", DataRegistryDataSourceType.UPDD)]
        [Display(Name = "Facility ID")]
        [StringLength(13, ErrorMessage = "Facility ID cannot be greater than 13 characters.")]
        public string FacilityID { get; set; }

        [DataRegistryMetadata("10.0010", DataRegistryDataSourceType.UPDD)]
        [Display(Name = "Business Name")]
        [StringLength(100, ErrorMessage = "Business Name cannot be greater than 100 characters.")]
        public string BusinessName { get; set; }

        [DataRegistryMetadata("10.0011", DataRegistryDataSourceType.UPDD)]
        [Display(Name = "CERS ID")]
        //[RegularExpression("^([1-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9])$", ErrorMessage = "CERS ID is a 9-digit number, that never has leading zeroes (begins at 10000001).")]
        public int? CERSID { get; set; }

        public string SearchType { get; set; }

        public override IEnumerable<FacilityGridViewModel> GridView
        {
            get
            {
                List<FacilityGridViewModel> results = new List<FacilityGridViewModel>();
                if (Entities != null)
                {
                    results.AddRange(Entities.ToGridView());
                }
                return results;
            }
        }
    }
}