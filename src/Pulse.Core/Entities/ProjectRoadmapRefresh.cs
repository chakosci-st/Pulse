using System;
using System.Collections.Generic;

namespace Pulse.Core.Entities
{
    public class ProjectRoadmapRefreshPreview
    {
        public ProjectRoadmapRefreshPreview()
        {
            Items = new List<ProjectRoadmapRefreshItem>();
        }

        public string ProjectNo { get; set; }
        public string RoadmapSysId { get; set; }
        public string RoadmapName { get; set; }
        public int AddedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int DependencyLinkCount { get; set; }
        public List<ProjectRoadmapRefreshItem> Items { get; set; }
    }

    public class ProjectRoadmapRefreshItem
    {
        public string ChangeKey { get; set; }
        public string ChangeType { get; set; }
        public string ItemType { get; set; }
        public string NodeId { get; set; }
        public string ParentType { get; set; }
        public string ParentNodeId { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public string Summary { get; set; }
        public bool HasSnapshotRow { get; set; }
        public bool HasExecutionRow { get; set; }
        public bool SelectedByDefault { get; set; }
        public bool IsSelectable { get; set; }
    }

    public class ProjectRoadmapRefreshSelection
    {
        public ProjectRoadmapRefreshSelection()
        {
            SelectedChangeKeys = new List<string>();
        }

        public string ProjectNo { get; set; }
        public List<string> SelectedChangeKeys { get; set; }
    }

    public class ProjectRoadmapRefreshApplyResult
    {
        public int AddedMilestones { get; set; }
        public int UpdatedMilestones { get; set; }
        public int AddedActivities { get; set; }
        public int UpdatedActivities { get; set; }
        public int AddedDependencyLinks { get; set; }
        public int AutoIncludedParents { get; set; }
    }

    public class ProjectMilestoneTemplateCatalog
    {
        public ProjectMilestoneTemplateCatalog()
        {
            Roadmaps = new List<ProjectMilestoneTemplateRoadmap>();
        }

        public List<ProjectMilestoneTemplateRoadmap> Roadmaps { get; set; }
    }

    public class ProjectMilestoneTemplateRoadmap
    {
        public ProjectMilestoneTemplateRoadmap()
        {
            Milestones = new List<ProjectMilestoneTemplateItem>();
        }

        public string RoadmapSysId { get; set; }
        public string RoadmapName { get; set; }
        public string RoadmapDescription { get; set; }
        public List<ProjectMilestoneTemplateItem> Milestones { get; set; }
    }

    public class ProjectMilestoneTemplateItem
    {
        public string RoadmapMilestoneSysId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
    }

    public class ProjectAdditionalMilestoneCatalog
    {
        public ProjectAdditionalMilestoneCatalog()
        {
            Milestones = new List<ProjectAdditionalMilestoneItem>();
        }

        public List<ProjectAdditionalMilestoneItem> Milestones { get; set; }
    }

    public class ProjectAdditionalMilestoneItem
    {
        public string RoadmapMilestoneSysId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrderIndex { get; set; }
        public bool CanMoveUp { get; set; }
        public bool CanMoveDown { get; set; }
    }

    public class ProjectMilestoneArrangementCatalog
    {
        public ProjectMilestoneArrangementCatalog()
        {
            Groups = new List<ProjectMilestoneArrangementGroup>();
        }

        public List<ProjectMilestoneArrangementGroup> Groups { get; set; }
    }

    public class ProjectMilestoneArrangementGroup
    {
        public ProjectMilestoneArrangementGroup()
        {
            Milestones = new List<ProjectMilestoneArrangementItem>();
        }

        public string ParentType { get; set; }
        public string ParentSysId { get; set; }
        public string ParentPath { get; set; }
        public List<ProjectMilestoneArrangementItem> Milestones { get; set; }
    }

    public class ProjectMilestoneArrangementItem
    {
        public string RoadmapMilestoneSysId { get; set; }
        public string RoadmapSysId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrderIndex { get; set; }
        public bool CanDelete { get; set; }
    }

    public class ProjectMilestoneArrangementSaveRequest
    {
        public ProjectMilestoneArrangementSaveRequest()
        {
            Groups = new List<ProjectMilestoneArrangementSaveGroup>();
        }

        public string ProjectNo { get; set; }
        public List<ProjectMilestoneArrangementSaveGroup> Groups { get; set; }
    }

    public class ProjectMilestoneArrangementSaveGroup
    {
        public ProjectMilestoneArrangementSaveGroup()
        {
            OrderedRoadmapMilestoneSysIds = new List<string>();
        }

        public string ParentType { get; set; }
        public string ParentSysId { get; set; }
        public List<string> OrderedRoadmapMilestoneSysIds { get; set; }
    }
}