using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Pulse.DataTransformationObjects
{
    public class dtoTaskSearchQuery : dtoTask
    {
        public string MaturityCode { get; set; }
        public string UserId { get; set; }
        public int? UserGroupId { get; set; }
        public string ADGroup { get; set; }
        public DateTime? DisplayRangeFrom { get; set; }
        public DateTime? DisplayRangeTo { get; set; }

    }
}
