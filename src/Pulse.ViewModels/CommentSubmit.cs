using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.ViewModels
{
   public class CommentSubmit
    {
        public string Comments { get; set; }
        public string CommentsRichText { get; set; }
        public string ProjectNo { get; set; }
        public string EntityType { get; set; }
        public string EntitySysId { get; set; }
    }
}
