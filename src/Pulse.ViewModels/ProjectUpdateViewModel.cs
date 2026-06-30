using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.ViewModels
{
   public class ProjectUpdateViewModel
    {
        public string ProjectNo { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ProductGroupCode { get; set; }
        public string ProductDivisionCode { get; set; }
        public string Icon { get; set; }
        public string IconColor { get; set; }
        public string TransactionKey { get; set; }
    }
}
