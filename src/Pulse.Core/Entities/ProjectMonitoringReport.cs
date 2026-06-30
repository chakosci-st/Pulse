using System;
using System.Collections.Generic;

namespace Pulse.Core.Entities
{
    public class ProjectMonitoringReport
    {
        public DateTime GeneratedAt { get; set; }
        public IList<ProjectMonitoringMilestoneGroup> Milestones { get; set; } = new List<ProjectMonitoringMilestoneGroup>();
        public IList<ProjectMonitoringRow> Rows { get; set; } = new List<ProjectMonitoringRow>();
    }

    public class ProjectMonitoringMilestoneGroup
    {
        public string MilestoneKey { get; set; }
        public string MilestoneName { get; set; }
        public int OrderIndex { get; set; }
        public IList<ProjectMonitoringTaskColumn> Tasks { get; set; } = new List<ProjectMonitoringTaskColumn>();
    }

    public class ProjectMonitoringTaskColumn
    {
        public string ColumnKey { get; set; }
        public string TaskName { get; set; }
        public string Prerequisites { get; set; }
        public int OrderIndex { get; set; }
    }

    public class ProjectMonitoringRow
    {
        public string ProjectNo { get; set; }
        public string ProjectName { get; set; }
        public string OwnerName { get; set; }
        public string Status { get; set; }
        public string PlantCode { get; set; }
        public string CategoryCode { get; set; }
        public string ProductCodes { get; set; }
        public DateTime? TargetStart { get; set; }
        public DateTime? TargetCompletion { get; set; }
        public IDictionary<string, string> TaskValues { get; set; } = new Dictionary<string, string>();
    }
}