using System;
using System.ComponentModel.DataAnnotations;
using UPF.ViewModel;

namespace CERS.Web.UI.Regulator.ViewModels
{
    public class SubmittalDeltaFieldViewModel : GridViewModel
    {
        [Display(Name = "Significance")]
        public string Significance { get; set; }

        [Display(Name = "Form")]
        public string Form { get; set; }

        [Display(Name = "Field Name")]
        public string FieldName { get; set; }

        [Display(Name = "New Value")]
        public string NewValue { get; set; }

        [Display(Name = "Old Value")]
        public string OldValue { get; set; }
    }
}