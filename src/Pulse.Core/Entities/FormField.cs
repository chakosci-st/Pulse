using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pulse.Core.Converters;

namespace Pulse.Core.Entities
{
    public class FormField
    {
        public FormField()
        {
            //  Options = new HashSet<FormFieldOption>();
            //  Rules = new HashSet<FormFieldRule>();
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
        [JsonConverter(typeof(IntBoolJsonConverter))]
        public int IsRequired { get; set; }
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

        [JsonProperty("useFieldDefaults")]
        public bool UseFieldDefaults { get; set; }

        [JsonProperty("createAsReference")]
        public bool CreateAsReference { get; set; }




        [JsonProperty("orderIndex")]
        public int OrderIndex { get; set; }

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
        [JsonConverter(typeof(FormFieldOptionListConverter))]
        public List<FormFieldOption> Options { get; set; } = new List<FormFieldOption>();



        [JsonProperty("rules")]
        public List<FormFieldRule> Rules { get; set; } = new List<FormFieldRule>();
    }
}
