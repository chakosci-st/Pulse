using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
namespace Pulse.DataTransformationObjects
{
    public class dtoField
    {
        public dtoField()
        {
            Options = new HashSet<dtoFieldOption>();
            Rules = new HashSet<dtoFieldRule>();
        }
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
        [JsonProperty("urlIsParam")]
        public bool UrlIsParameter { get; set; }
        [JsonProperty("urlDefaultPattern")]
        public string UrlDefaultPattern { get; set; }

        [JsonProperty("defaultPattern")]
        public string DefaultPattern { get; set; }
        [JsonProperty("defaultValue")]
        public string DefaultValue { get; set; }
        [JsonProperty("defaultClobValue")]
        public string DefaultClobValue { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; } = true;



         
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }


        public ICollection<dtoFieldOption> Options { get; set; } = new List<dtoFieldOption>();


        public ICollection<dtoFieldRule> Rules { get; set; } = new List<dtoFieldRule>();
    }
}
