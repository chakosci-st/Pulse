using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoActivityMeta
    {
        public string Id { get; set; }
        public string Desc { get; set; }
        public string Maturity { get; set; }
        public double? Mandays { get; set; }
        public bool? IsRequired { get; set; }
        public List<string> Prerequisites { get; set; }
        public List<string> Forms { get; set; }
        public bool? Collapsed { get; set; }
    }
}
