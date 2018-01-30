using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CERS.Web.UI.Organization.ViewModels
{
    public class Address
    {
        public bool IsWashed { get; set; }

        public string Street { get; set; }

        public string Supplemental { get; set; }

        public string City { get; set; }

        public string ZipCode { get; set; }

        public string CountyID { get; set; }

        public string WashedStreet { get; set; }

        public string WashedCity { get; set; }

        public string WashedZipCode { get; set; }

        public string WashedCountyID { get; set; }

    }
}