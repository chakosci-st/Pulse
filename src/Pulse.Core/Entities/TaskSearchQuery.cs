using System; 
namespace Pulse.Core.Entities
{
    public class TaskSearchQuery : Task
    {
        public string MaturityCode { get; set; }
        public string UserId { get; set; }
        public int? UserGroupId { get; set; }
        public string ADGroup { get; set; }
        public DateTime? DisplayRangeFrom { get; set; }
        public DateTime? DisplayRangeTo { get; set; }

    }
}
