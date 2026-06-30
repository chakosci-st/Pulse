using Newtonsoft.Json;
using Pulse.Core.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    /// <summary>
    /// Represents a field in the system.
    /// </summary>
    public class Field : BaseEntity<int>
    {
        [JsonProperty("id")]
        public string FieldSysId { get; set; } 
        [JsonProperty("name")]
        public string FieldName { get; set; }
        [JsonProperty("title")]
        public string FieldTitle { get; set; }
        [JsonProperty("type")]
        public string FieldType { get; set; }
        [JsonProperty("placeholder")]
        public string Placeholder { get; set; }
        [JsonProperty("tooltip")]
        public string Tooltip { get; set; }
 
        [JsonProperty("minLength")]
        public int MinLength { get; set; }

        [JsonProperty("maxLength")]
        public int MaxLength { get; set; }

        [JsonProperty("caseOption")]
        public string CaseOption { get; set; }

        [JsonProperty("fileTypes")]
        public string FileType { get; set; }

        [JsonProperty("fileMaxSize")]
        public double FileMaxSize { get; set; } 

        [JsonProperty("validate")]
        public string FieldValidate { get; set; }
        [JsonProperty("datasource")]
        public string DataSource { get; set; }
        [JsonProperty("datasourceParamField")]
        public string DataSourceParamField { get; set; }
        [JsonProperty("parentFieldId")]
        public string ParentFieldSysId { get; set; }

        [JsonProperty("defaultPattern")]
        public string DefaultPattern { get; set; }

        [JsonProperty("urlIsParam")]
        [JsonConverter(typeof(IntBoolJsonConverter))]
        public int UrlIsParam { get; set; }
        [JsonProperty("urlDefaultPattern")]
        public string UrlDefaultPattern { get; set; }
        [JsonProperty("defaultValue")]
        public string DefaultValue { get; set; }
        [JsonProperty("defaultClobValue")]
        public string DefaultClobValue { get; set; }


         

        [JsonProperty("isActive")]
        [JsonConverter(typeof(IntBoolJsonConverter))]
        public int IsActive { get; set; } = 1;

        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }

        [JsonProperty("options")]
        [JsonConverter(typeof(FieldOptionListConverter))]
        public List<FieldOption> Options { get; set; } = new List<FieldOption>();



        [JsonProperty("rules")]
        public List<FieldRule> Rules { get; set; } = new List<FieldRule>();
    }
}
