using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
namespace Pulse.DataTransformationObjects
{
    public class dtoFormField
    {
        public dtoFormField()
        {
            Options = new HashSet<dtoFormFieldOption>();
            Rules = new HashSet<dtoFormFieldRule>();
        }
        [JsonProperty("id")]
        public string FormFieldSysId { get; set; }
        [JsonProperty("fieldSysId")]
        public string FieldSysId { get; set; }
        public string FormSysId { get; set; }
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
        [JsonProperty("isrequired")] 
        public bool IsRequired { get; set; }
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


        [JsonProperty("readAccess")]
        public string ReadAccess { get; set; }

        [JsonProperty("writeAccess")]
        public string WriteAccess { get; set; }
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

        [JsonProperty("useFieldDefaults")]
        public bool UseFieldDefaults { get; set; }

        [JsonProperty("createAsReference")]
        public bool CreateAsReference { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; } = true;




        public int OrderIndex { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }


        public ICollection<dtoFormFieldOption> Options { get; set; } = new List<dtoFormFieldOption>();


        public ICollection<dtoFormFieldRule> Rules { get; set; } = new List<dtoFormFieldRule>();
    }
}
