using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoProduct
    {
        public string ProductCode { get; set; }
        public string ProjectNo { get; set; }
        public string ProductType { get; set; } 
        public string Routing { get; set; }
        public string RoutingDescription { get; set; }
        public string SubRouting { get; set; }
        public string SubRoutingDescription { get; set; }
        public string ProductLink { get; set; }


        public string PlantCode { get; set; }
        public string PlantType { get; set; }
        public string PlantTypeDescription { get; set; }
        public string ProductFamilyCode { get; set; }
        public string ProductFamilyDescription { get; set; }
        public string MacroPackageCode { get; set; }
        public string MacroPackageDescription { get; set; }
        public string PackageCode { get; set; }
        public string PackageDescription { get; set; }
        public string ProductLineCode { get; set; }
        public string ProductLine { get; set; }
        public string MaturityCode { get; set; }

        public bool Linked { get; set; }


        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
    }
}
