using System;

namespace Pulse.Api.Models
{
    public class NotificationItemModel
    {
        public string NotificationSysId { get; set; }
        public string EntityType { get; set; }
        public string EntitySysId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Recipients { get; set; }
        public string ContextLabel { get; set; }
        public string ProjectNo { get; set; }
        public string ProjectName { get; set; }
        public string ParentNodeName { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByDisplayName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime NotificationDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsRead { get; set; }
        public bool IsArchived { get; set; }
        public bool CanManage { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}