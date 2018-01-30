using System;
using System.ComponentModel.DataAnnotations;
using UPF.ViewModel;

namespace CERS.Web.UI.Regulator.ViewModels
{
    public class SubmittalDeltaDocumentViewModel : GridViewModel
    {
        [Display(Name = "Significance")]
        public string Significance { get; set; }

        [Display(Name = "Document")]
        public string Document { get; set; }

        [Display(Name = "Change")]
        public string Change { get; set; }

        [Display(Name = "Option")]
        public string Option { get; set; }

        [Display(Name = "Value")]
        public string OptionValue { get; set; }

        public int DocumentID { get; set; }
        public string DocumentDate { get; set; }
        public string DocumentSize { get; set; }
        public string DocumentType { get; set; }
        public string CERSID { get; set; }
    }
}