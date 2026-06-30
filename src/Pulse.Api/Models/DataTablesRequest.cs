using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pulse.Api.Models
{
    public class DataTablesRequest
    {
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }

        public string sortBy { get; set; }
        public string sortDirection { get; set; }

        public Search search { get; set; }
        public bool? isActive { get; set; }


        public class Search
        {
            public string value { get; set; }
            public bool regex { get; set; }
        }
    }

    public class DataTablesRequestProject : DataTablesRequest {

        public string ProjectOwnerId { get; set; }
        public string ProjectNo { get; set; }
        public string ProductCode { get; set; }
        public string ProductGroupCode { get; set; }
        public string ProductDivisionCode { get; set; }
        public string PlantCode { get; set; }
        public string Status { get; set; }
        public string ParentType { get; set; }
        public string NodeType { get; set; }
        public string CategoryCode { get; set; }
        public string OrderColumn { get; set; }
        public string OrderDir { get; set; }
        public string StartIndex { get; set; }
        public string LengthCount { get; set; }
        public bool ShowAllUsers { get; set; }
    }
}