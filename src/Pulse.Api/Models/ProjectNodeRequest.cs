using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pulse.Api.Models
{
    public class ProjectNodeRequest
    {
        public string ProjectNo { get; set; }
        public string NodeId { get; set; }
        public string NodeType { get; set; }

    }
}