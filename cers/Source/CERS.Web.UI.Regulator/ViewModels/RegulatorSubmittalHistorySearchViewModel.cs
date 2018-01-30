using CERS.Compositions;
using CERS.Model;
using CERS.ViewModels.Inspections;
using CERS.ViewModels.SubmittalElements;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using UPF.ViewModel;

namespace CERS.Web.UI.Regulator.ViewModels
{
    public class RegulatorSubmittalHistorySearchViewModel : ViewModel
    {
        private List<SimpleNamedID> _SearchOptions;

        [Display(Name = "CERS ID")]
        public int? CERSID { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        public int DefaultRegulatorID { get; set; }

        [Display(Name = "Facility ID")]
        public string FacilityID { get; set; }

        [Display(Name = "Facility")]
        public string FacilityName { get; set; }

        [Display(Name = "From")]
        public DateTime? From { get; set; }

        public DateTime? LastRefreshedOn { get; set; }

        public StringSearchOption NameSearchOption { get; set; }

        public List<CERS.Model.Regulator> RegulatorCollection { get; set; }

        [Display(Name = "Regulator")]
        [Required(ErrorMessage = "Please select a Regulator")]
        public int RegulatorID { get; set; }

        public List<SimpleNamedID> SearchOptions
        {
            get
            {
                if (_SearchOptions == null)
                {
                    InitSearchOptions();
                }
                return _SearchOptions;
            }
        }

        [Display(Name = "Local Facility Grouping")]
        [StringLength(20, ErrorMessage = "Local Facility Grouping value cannot exceed 20 characters. ")]
        [DataRegistryMetadata("20.0404", DataRegistryDataSourceType.Supplemental)]
        public string LocalRegulatorDistrict { get; set; }

        [Display(Name = "Show Hidden Submittals")]
        public bool ShowHiddenSubmittals { get; set; }

        public List<SimpleNamedID> StatusCollection { get; set; }

        [Display(Name = "Status")]
        public int? StatusID { get; set; }
        
        public IEnumerable<SubmittalElement> SubmittalElementCollection { get; set; }

        [Display(Name = "Submittal Element")]
        public int? SubmittalElementID { get; set; }

        public IEnumerable<SubmittalEvent> SubmittedSubmittalElements { get; set; }

        public IEnumerable<uspSubmittalProcessingSearch_Result> SubmittedSubmittalElementsForProcessing { get; set; }

        [Display(Name = "To")]
        public DateTime? To { get; set; }

        [Display(Name = "Zip")]
        public string Zip { get; set; }

        private void InitSearchOptions()
        {
            _SearchOptions = new List<SimpleNamedID>();
            _SearchOptions.Add(new SimpleNamedID { ID = (int)StringSearchOption.StartsWith, Name = "Starts with" });
            _SearchOptions.Add(new SimpleNamedID { ID = (int)StringSearchOption.Contains, Name = "Contains" });
            _SearchOptions.Add(new SimpleNamedID { ID = (int)StringSearchOption.EndsWith, Name = "Ends with" });
        }

        public List<SimpleNamedID> ChangeTypeCollection { get; set; }

        public int ChangeTypeID { get; set; }
    }
}