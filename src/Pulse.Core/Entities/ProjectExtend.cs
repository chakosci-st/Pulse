using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class ProjectExtendSearch
    {
        public string Search { get; set; }
        public string ProjectNo { get; set; }
        public string ProductCode { get; set; }
        public string Status { get; set; }
        public string NodeId { get; set; }
        public string NodeType { get; set; }
        public string ParentType { get; set; }
        public string ProductGroupCode { get; set; }
        public string ProductDivisionCode { get; set; } 
        public string PlantCode { get; set; }
        public string CategoryCode { get; set; }
        public string LoggedUser { get; set; }
        public int StartIndex { get; set; }
        public int LengthCount { get; set; }
        public string ProjectOwnerId { get; set; }
        public string OrderColumn { get; set; }
        public string OrderDir { get; set; }
    }


    public class ProjectExtend : Project
    {
        public ProjectExtend() {
            Nodes = new HashSet<ProjectExtend>();
        }

        public string Search { get; set; }
        public string LoggedUser { get; set; }
        public string OrderColumn { get; set; }
        public string OrderDir { get; set; }
        public int StartIndex { get; set; }
        public int LengthCount { get; set; }

        public string JsonMembers { get; set; }
        public string JsonNodeOwners { get; set; }
        public DateTime? TargetStart { get; set; }
        public DateTime? TargetCompletion { get; set; }
        public string NodeName { get; set; }
        public string NodeMaturityCode { get; set; }
        public string NodeDescription { get; set; }
        public int NodeLevel { get; set; }
        public string NodeFullPath { get; set; }
        public string NodeIdPath { get; set; }
        public string NodeTypeIdPath { get; set; }
        public string NodeId { get; set; }
        public string NodeType { get; set; }
        public string ParentSysId { get; set; }
        public string ParentType { get; set; }
        public int? EstimatedMandays { get; set; }
        public int? IsRequired { get; set; }
        public int OrderIndex { get; set; }
        public int NodeTotalDescendants { get; set; }
        public int IsResched { get; set; }


        public int ProjectCount { get; set; }
        public int ProjectCompleteCount { get; set; }
        public int ProjectCancelCount { get; set; }
        public int ProjectOngoingCount { get; set; }
        public int ProjectPendingCount { get; set; }

        public int ProjectTaskPendingCount { get; set; }
        public int ProjectTaskAtRiskCount { get; set; }
        public int ProjectTaskClosedCount { get; set; }
        public int ProjectTaskClosedDelayedCount { get; set; }

        public int ProjectNodeCount { get; set; }
        public int ProjectNodeCompleteCount { get; set; }
        public int ProjectNodeCancelCount { get; set; }
        public int ProjectNodeOngoingCount { get; set; }
        public int ProjectNodePendingCount { get; set; }

        public string ProductCodes { get; set; }

        public string ProjectNodeSysId { get; set; }
        public string ProjectNodeStatus { get; set; }
        public string ProjectNodeRemarks { get; set; }
        public string PrerequisitesJson { get; set; }
        public string ProjectNodeTargetStartedBy { get; set; }
        public int? ProjectNodeTargetStartYear { get; set; }
        public string ProjectNodeTargetStartWorkWeek { get; set; }
        public DateTime? ProjectNodeTargetStartDate { get; set; }
        public DateTime? ProjectNodeTargetStart { get; set; }
        public string ProjectNodeTargetCompletedBy { get; set; }
        public int? ProjectNodeTargetCompletionYear { get; set; }
        public string ProjectNodeTargetCompletionWorkWeek { get; set; }
        public DateTime? ProjectNodeTargetCompletionDate { get; set; }
        public DateTime? ProjectNodeTargetCompletion { get; set; }
        public DateTime? ProjectNodeActualStartDate { get; set; }
        public string ProjectNodeActualStartedBy { get; set; }
        public DateTime? ProjectNodeActualCompletionDate { get; set; }
        public string ProjectNodeActualCompletedBy { get; set; }




        public int ProjectNodeIsResched { get; set; }

        public string ProjectOwnerFirstName { get; set; }
        public string ProjectOwnerLastName { get; set; }
        public string ProjectOwnerEmail { get; set; }

        public string NodeFormValue { get; set; }

        public int? TotalRows { get; set; }

        public ICollection<ProjectExtend> Nodes { get; set; }


    }
}
