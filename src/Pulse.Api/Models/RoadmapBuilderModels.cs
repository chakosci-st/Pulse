using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pulse.Api.Models.Roadmap
{
    public class Root
    {
        public List<TreeNode> TreeData { get; set; }
        public List<Form> RootForms { get; set; }
    }

    public class TreeNode
    {
        public string Key { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }

 
        public MilestoneData Data { get; set; }
 
        public List<TreeNode> Children { get; set; }
        public List<Form> Forms { get; set; }
        public List<string> Prerequisites { get; set; }
        public bool Collapsed { get; set; }
    }


    public class MilestoneData
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Maturity { get; set; }    // used by "milestone"
        public string Mandays { get; set; }     // used by "activity"
        public bool? IsRequired { get; set; }   // used by "activity"
    }

    public class Form
    {
        public string Key { get; set; }
        public string Id { get; set; }
        public string Sysid { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
    }
}