using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoFormSubmissionValue
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public string TransactionKey { get; set; }
        public string SubmissionSysId { get; set; }
        public string SubmissionTransactionKey { get; set; }
        public string FormSysId { get; set; }
        public string FormEntityLinkSysId { get; set; }
        public string EntitySysId { get; set; }
        public string EntityType { get; set; }
        public string FormFieldSysId { get; set; }
    }
}
