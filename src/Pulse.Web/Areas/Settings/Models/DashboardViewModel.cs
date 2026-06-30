using Pulse.Core.Entities;
using System;
using System.Collections.Generic;

namespace Pulse.Web.Areas.Settings.Models
{
    public class DashboardViewModel
    {
        public User User { get; set; }

        public string FullName { get; set; }

        public string Initials { get; set; }

        public int PendingTaskCount { get; set; }

        public int CompletedTaskCount { get; set; }

        public int ActiveNotificationCount { get; set; }

        public int UnreadNotificationCount { get; set; }

        public int ProjectCount { get; set; }

        public int UserGroupCount { get; set; }

        public IReadOnlyCollection<string> UserGroups { get; set; } = Array.Empty<string>();

        public IReadOnlyCollection<DashboardTaskSummaryViewModel> DueSoonTasks { get; set; } = Array.Empty<DashboardTaskSummaryViewModel>();

        public IReadOnlyCollection<ProfileRecentActivityViewModel> RecentActivities { get; set; } = Array.Empty<ProfileRecentActivityViewModel>();
    }

    public class DashboardTaskSummaryViewModel
    {
        public string ProjectTaskSysId { get; set; }

        public string ProjectNo { get; set; }

        public string ProjectName { get; set; }

        public string ActivityName { get; set; }

        public string Status { get; set; }

        public DateTime? TargetCompletionDate { get; set; }

        public bool IsOverdue { get; set; }
    }
}