using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoActivity
    {
        public string Name { get; set; }
        public List<string> Owners { get; set; }
        public string StartDate { get; set; }   // year
        public string EndDate { get; set; }     // year
        public string StartWeek { get; set; }
        public string EndWeek { get; set; }
        public dtoActivityMeta Meta { get; set; }
    }
}
