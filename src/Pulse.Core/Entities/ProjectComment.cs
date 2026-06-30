using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
   public class ProjectComment
    {
        public string CommentSysId { get; set; }
        public string ProjectNo { get; set; }
        public string Comments { get; set; }
        public string CommentsRichText { get; set; }
        public string EntityType { get; set; }
        public string EntitySysId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string MetaJson { get; set; } 
        public User CreatedByMeta { get; set; }
    }
}
