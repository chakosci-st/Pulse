using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class ProjectTaskItem
    {
        public string UserId { get; set; }
        public string ProjectTaskSysId { get; set; }
        public string RoadmapActivitySysId { get; set; }
        public string ActivityName { get; set; }
        public string ActivityDescription { get; set; }
        public string ProjectNo { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public string ProjectIcon { get; set; }
        public string ProjectIconColor { get; set; }
        public string PlantCode { get; set; }
        public string PlantName { get; set; }
        public string CategoryName { get; set; }
        public string ProductGroupName { get; set; }
        public string ProductDivisionName { get; set; }
        public string Members { get; set; }
        public int EstimatedMandays { get; set; }
        public int TargetStartYear { get; set; }
        public string TargetStartWorkWeek { get; set; }
        public DateTime TargetStartDate { get; set; }
        public int TargetCompletionYear { get; set; }
        public int? TargetCompletionWorkWeek { get; set; }
        public DateTime? TargetCompletionDate { get; set; }
        public int? ProjectTargetCompletionYear { get; set; }
        public string ProjectCompletionWorkWeek { get; set; }
        public DateTime? ProjectWkFiscalDate { get; set; }
        public DateTime? TaskWkFiscalDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public string Status { get; set; }
        public int IsRequired { get; set; }
        public string TransactionKey { get; set; }
    }
}
