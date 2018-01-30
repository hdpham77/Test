//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CERS.Windows.DocumentClone
{
    using System;
    using System.Collections.Generic;
    
    public partial class FacilitySubmittalElementResourceDocument
    {
        public FacilitySubmittalElementResourceDocument()
        {
            this.FacilitySubmittalElementDocuments = new HashSet<FacilitySubmittalElementDocument>();
        }
    
        public int ID { get; set; }
        public int FacilitySubmittalElementResourceID { get; set; }
        public int SourceID { get; set; }
        public string LinkUrl { get; set; }
        public Nullable<System.DateTime> DateProvidedToRegulator { get; set; }
        public Nullable<int> StoredAtFacilityCERSID { get; set; }
        public Nullable<int> ProvidedInSubmittalElementID { get; set; }
        public string Comments { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedByID { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public int UpdatedByID { get; set; }
        public bool Voided { get; set; }
    
        public virtual ICollection<FacilitySubmittalElementDocument> FacilitySubmittalElementDocuments { get; set; }
        public virtual FacilitySubmittalElementResource Resource { get; set; }
    }
}
