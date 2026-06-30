using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class FieldOption
    {
        [JsonProperty("id")]
        public string FieldOptionSysId { get; set; }
        public string FieldSysId { get; set; }
        [JsonProperty("value")]
        public string OptionValue { get; set; }

        [JsonProperty("label")]
        public string OptionLabel { get; set; }

        [JsonProperty("orderIndex")]
        public int OrderIndex { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
    }
}
