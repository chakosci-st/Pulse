using Newtonsoft.Json;
using Pulse.DataTransformationObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.ViewModels
{
    public class ProjectInitViewModel
    {
        // Basic project fields
        [JsonProperty("siteValue")]
        public string SiteValue { get; set; }
        [JsonProperty("siteText")]
        public string SiteText { get; set; }
        [JsonProperty("templatePlantRoadmapLinkSysId")]
        public string TemplatePlantRoadmapLinkSysId { get; set; }
        [JsonProperty("templateValue")]
        public string TemplateValue { get; set; }
        [JsonProperty("templateText")]
        public string TemplateText { get; set; }
        [JsonProperty("templateDescription")]
        public string TemplateDescription { get; set; }
        [JsonProperty("templateCategory")]
        public string TemplateCategory { get; set; }
        [JsonProperty("templateCategoryValue")]
        public string TemplateCategoryValue { get; set; }
        [JsonProperty("templateJson")]
        public string TemplateJson { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("icon")]
        public string Icon { get; set; }
        [JsonProperty("iconColor")]
        public string IconColor { get; set; }
        [JsonProperty("ownerValue")]
        public string OwnerValue { get; set; }
        [JsonProperty("ownerText")]
        public string OwnerText { get; set; }
        [JsonProperty("productgroupValue")]
        public string ProductgroupValue { get; set; }
        [JsonProperty("productgroupText")]
        public string ProductgroupText { get; set; }
        [JsonProperty("productdivisionValue")]
        public string ProductdivisionValue { get; set; }
        [JsonProperty("productdivisionText")]
        public string ProductdivisionText { get; set; }
        [JsonProperty("projectstartYear")]
        public string ProjectstartYear { get; set; }
        [JsonProperty("projectstartWorkWeek")]
        public string ProjectstartWorkWeek { get; set; }
        [JsonProperty("projectendYear")]
        public string ProjectendYear { get; set; }
        [JsonProperty("projectendWorkWeek")]
        public string ProjectendWorkWeek { get; set; }
        [JsonProperty("autoStart")]
        public bool AutoStart { get; set; }
        [JsonProperty("projectMaturityCode")]
        public string ProjectMaturityCode { get; set; }
        [JsonProperty("currentMilestoneSysId")]
        public string CurrentMilestoneSysId { get; set; }
        [JsonProperty("actualStartDate")]
        public DateTime? ActualStartDate { get; set; }

        // Members: [{ name }]
        [JsonProperty("members")]
        public List<dtoMember> Members { get; set; }

        // Milestones and tasks
        [JsonProperty("milestones")]
        public List<dtoMilestone> Milestones { get; set; }

        // Product codes table rows
        [JsonProperty("productCodes")]
        public List<dtoProductCodeRow> ProductCodes { get; set; }
    }
}
