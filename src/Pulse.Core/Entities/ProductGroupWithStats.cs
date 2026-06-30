using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class ProductGroupWithStats : ProductGroup
    {
        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }
    }
}
