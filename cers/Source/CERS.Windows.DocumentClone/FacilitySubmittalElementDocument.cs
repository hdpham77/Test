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
    
    public partial class FacilitySubmittalElementDocument
    {
        public int ID { get; set; }
        public int FacilitySubmittalElementResourceDocumentID { get; set; }
        public System.Guid Key { get; set; }
        public Nullable<int> DocumentID { get; set; }
        public int TypeID { get; set; }
        public int FormatID { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public System.DateTime DocumentDate { get; set; }
        public string Description { get; set; }
        public string EDTClientKey { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedByID { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public int UpdatedByID { get; set; }
        public bool Voided { get; set; }
    
        public virtual Document Document { get; set; }
        public virtual FacilitySubmittalElementResourceDocument ResourceDocument { get; set; }
    }
}