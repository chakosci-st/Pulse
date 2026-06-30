using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Pulse.Core.Entities
{
    public class Form : BaseEntity<string>
    {
        public Form()
        {
            Fields = new HashSet<FormField>();
            EntityLinks = new HashSet<FormEntityLink>();
        }

        [Key]
        [JsonProperty("id")]
        public string FormSysId { get; set; }
        [JsonProperty("name")]
        public string FormName { get; set; }
        [JsonProperty("description")]
        public string FormDescription { get; set; }
        public int IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }

        [JsonProperty("fields")]
        public ICollection<FormField> Fields { get; set; }
        public ICollection<FormEntityLink> EntityLinks { get; set; }


    }
}
