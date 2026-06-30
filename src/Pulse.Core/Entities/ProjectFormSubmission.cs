using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class ProjectFormSubmission
    {
        public ProjectFormSubmission() {
            SubmissionValues = new HashSet<ProjectFormSubmissionValue>();
        }

        public string SubmissionSysId { get; set; }
        public string ProjectNo { get; set; }
        public string FormEntityLinkSysId { get; set; }
        public string FormSysId { get; set; }
        public int IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }


        public ICollection<ProjectFormSubmissionValue> SubmissionValues { get; set; }
    }
}
