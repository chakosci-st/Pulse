using Pulse.Core.Entities;
using System;
using System.Collections.Generic;

namespace Pulse.Web.Areas.Settings.Models
{
    public class ProfileViewModel
    {
        public User User { get; set; }

        public string FullName { get; set; }

        public string Initials { get; set; }

        public string PhotoUrl { get; set; }

        public bool HasPhoto { get; set; }

        public bool CanEditPhoto { get; set; }

        public string PhotoStatusMessage { get; set; }

        public string PhotoErrorMessage { get; set; }

        public IReadOnlyCollection<string> UserGroups { get; set; } = Array.Empty<string>();

        public int PendingTaskCount { get; set; }

        public int CompletedTaskCount { get; set; }

        public int OwnedProjectCount { get; set; }

        public int MemberProjectCount { get; set; }

        public int ActiveNotificationCount { get; set; }

        public int UnreadNotificationCount { get; set; }

        public IReadOnlyCollection<ProfileRecentActivityViewModel> RecentActivities { get; set; } = Array.Empty<ProfileRecentActivityViewModel>();
    }

    public class ProfileRecentActivityViewModel
    {
        public string Title { get; set; }

        public string Message { get; set; }

        public string Context { get; set; }

        public string TargetUrl { get; set; }

        public string TargetLabel { get; set; }

        public string CreatedBy { get; set; }

        public DateTime OccurredAt { get; set; }

        public bool IsUnread { get; set; }
    }
}