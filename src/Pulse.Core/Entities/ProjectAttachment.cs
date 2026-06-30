using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class ProjectAttachment
    {
        public string AttachmentSysId { get; set; }
        public string ProjectNo { get; set; }
        public string FileName { get; set; }
        public string AltFileName { get; set; }
        public string FileType { get; set; }
        public int FileSize { get; set; }
        public string FileExtension { get; set; }
        public string EntityType { get; set; }
        public string EntitySysId { get; set; }
        public string CreatedBy { get; set; }
        public string MetaJson { get; set; }
        public DateTime CreatedDate { get; set; }
        public User CreatedByMeta { get; set; }
        public bool CanManageAttachment { get; set; }
        
    }
}
