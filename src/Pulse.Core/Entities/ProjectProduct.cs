using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
   public class ProjectProduct
    {
        public string ProjectNo { get; set; }
        public string ProductCode { get; set; } 
        public Product Product { get; set; }
    }
}
