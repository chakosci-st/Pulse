using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class ProductDivisionWithStats : ProductDivision
    {
        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }
    }
}
