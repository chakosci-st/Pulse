using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Pulse.DataTransformationObjects
{
    public class dtoProductCodeRow
    {
        [JsonProperty("productCode")]
        public string ProductCode { get; set; }
        [JsonProperty("plantType")]
        public string PlantType { get; set; }
        [JsonProperty("plantTypeDesc")]
        public string PlantTypeDesc { get; set; }
        [JsonProperty("productFamily")]
        public string ProductFamily { get; set; }
        [JsonProperty("productFamilyDesc")]
        public string ProductFamilyDesc { get; set; }
        [JsonProperty("macroPackage")]
        public string MacroPackage { get; set; }
        [JsonProperty("macroPackageDesc")]
        public string MacroPackageDesc { get; set; }
        [JsonProperty("pack")]
        public string Pack { get; set; }
        [JsonProperty("packDesc")]
        public string PackDesc { get; set; }
        [JsonProperty("pLine")]
        public string PLine { get; set; }
        [JsonProperty("pLineDesc")]
        public string PLineDesc { get; set; }
        [JsonProperty("maturity")]
        public string Maturity { get; set; }
    }
}
