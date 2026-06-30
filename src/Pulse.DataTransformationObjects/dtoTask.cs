using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Pulse.DataTransformationObjects
{
    public class dtoTask  
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("owners")]
        public List<string> Owners { get; set; }
        [JsonProperty("startDate")]
        public string StartDate { get; set; }   // year
        [JsonProperty("endDate")]
        public string EndDate { get; set; }     // year
        [JsonProperty("startWeek")]
        public string StartWeek { get; set; }
        [JsonProperty("endWeek")]
        public string EndWeek { get; set; }
        [JsonProperty("meta")]
        public dtoTaskMeta Meta { get; set; }


        ////public dtoTask()
        ////{
        ////    Prerequisites = new HashSet<dtoTaskPrerequisite>();
        ////    Members = new HashSet<dtoTaskMember>();
        ////    StatusChanges = new HashSet<dtoStatusChange>();
        ////    TargetRevisions = new HashSet<dtoTargetRevision>();
        ////}
        ////[Key]
        ////public string TaskSysId { get; set; }
        ////[Required]
        ////public string ProjectNo { get; set; }
        ////public string MilestoneSysId { get; set; }
        ////public string WorkItemSysId { get; set; }
        ////[Required]
        ////public string TaskName { get; set; }
        ////[Required]
        ////public string TaskType { get; set; }
        ////[StringLength(200)]
        ////public string TaskValue { get; set; }
        ////public string TargetSart { get; set; }
        ////public string TargetCompletion { get; set; }
        ////public DateTime? ActualStartDate { get; set; }
        ////public DateTime? ActualCompletionDate { get; set; }
        ////public string Status { get; set; }
        ////[StringLength(200)]
        ////public string Remarks { get; set; }
        ////public bool IsRequired { get; set; }
        ////[Required]
        ////public string CreatedBy { get; set; }
        ////public DateTime CreatedDate { get; set; }
        ////public string ModifiedBy { get; set; }
        ////public DateTime? ModifiedDate { get; set; }
        ////public string TransactionKey { get; set; }

        ////public IEnumerable<dtoTaskPrerequisite> Prerequisites { get; set; }
        ////public IEnumerable<dtoTaskMember> Members { get; set; }
        ////public IEnumerable<dtoStatusChange> StatusChanges { get; set; }
        ////public IEnumerable<dtoTargetRevision> TargetRevisions { get; set; }
    }

}
