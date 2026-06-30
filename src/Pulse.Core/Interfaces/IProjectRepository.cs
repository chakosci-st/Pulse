using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing project data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProjectRepository : IBaseRepository<Project, string>
    {
        /// <summary>
        /// Retrieves project asynchronously from the repository.
        /// </summary>
        /// <param name="productcode">Thr product code.</param> 
        /// <returns>project by product code</returns>
        Task<Project> GetByProductCodeAsync(string productcode);

        /// <summary>
        /// Retrieves paged projects with optional search functionality asynchronously from the repository.
        /// </summary>
        /// <param name="pageNumber">The current page number (1-based).</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="searchTerm">An optional search term to filter the orders by name.</param>
        /// <returns>A <see cref="PagedResult{Order}"/> containing the total record count and the paged orders.</returns>
        Task<PagedResult<Project>> GetPagedProjectsAsync(int pageNumber, int pageSize, string searchTerm = null, string orderBy = null, string orderDirection = null);

        /// <summary>
        /// Retrieves paged projects with optional search functionality asynchronously from the repository.
        /// </summary>
        /// <param name="pageNumber">The current page number (1-based).</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="searchTerm">An optional search term to filter the orders by name.</param>
        /// <returns>A <see cref="PagedResult{Project}"/> containing the total record count and the paged orders.</returns>

        Task<PagedResult<ProjectExtend>> GetPagedProjectsWithStatsAsync(ProjectExtendSearch searchTerm);


        /// <summary>
        /// Retrieves paged projects with optional search functionality asynchronously from the repository.
        /// </summary>
        /// <param name="pageNumber">The current page number (1-based).</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="searchTerm">An optional search term to filter the orders by name.</param>
        /// <returns>A <see cref="PagedResult{Order}"/> containing the total record count and the paged orders.</returns>
        Task<PagedResult<ProjectExtend>> GetPagedFullProjectsAsync(ProjectExtendSearch searchTerm);


        Task<List<ProjectExtend>> GetProjectNodeChildrenAsync(string projectNo, string nodetype, string nodeid);

        Task<ProjectExtend> GetProjectNodeItemAsync(string projectNo, string nodetype, string nodeid);

        Task<List<ProjectExtend>> GetProjectNodesAsync(string projectNo);


        Task<ProjectDashboardCounter> GetDashboardCardsCounter(string userid);

        Task<List<ProjectExtend>> GetProjectNodesByUserAsync(string userid);

        Task<RoadmapMilestone> GetProjectRoadmapMilestoneAsync(string projectNo, string roadmapMilestoneSysId);
        Task<List<RoadmapMilestone>> GetProjectRoadmapMilestonesAsync(string projectNo);
        Task<List<RoadmapActivity>> GetProjectRoadmapActivitiesAsync(string projectNo);
        Task<List<RoadmapActivityPrerequisite>> GetProjectRoadmapActivityPrerequisitesAsync(string projectNo);
        Task<Roadmap> GetProjectRoadmapAsync(string projectNo);

        Task AddProjectRoadmapMilestoneAsync(string projectNo, string roadmapMilestoneSysId, string roadmapSysId, string maturityCode,
            string parentType, string parentSysId, string milestoneAlias, string milestoneDescription, int orderIndex,
            int isActive, int isRequired, string createdBy, string modifiedBy);
        Task UpdateProjectRoadmapMilestoneAsync(string projectNo, RoadmapMilestone milestone, string modifiedBy);
        Task DeleteProjectRoadmapMilestoneAsync(string projectNo, string roadmapMilestoneSysId);
        Task UpdateProjectRoadmapMilestoneOrderAsync(string projectNo, string roadmapMilestoneSysId, int orderIndex, string modifiedBy);

        Task AddProjectRoadmapActivityAsync(string projectNo, string roadmapActivitySysId, string roadmapSysId, string parentType,
            string parentSysId, string activityName, string activityDescription, int estimatedManDays, int isRequired,
            int orderIndex, int isActive, string createdBy, string modifiedBy);
        Task UpdateProjectRoadmapActivityAsync(string projectNo, RoadmapActivity activity, string modifiedBy);
        Task DeleteProjectRoadmapActivityAsync(string projectNo, string roadmapActivitySysId);
        Task DeleteProjectRoadmapActivityPrerequisitesForActivityAsync(string projectNo, string roadmapActivitySysId);

        Task AddProjectRoadmapActivityPrerequisiteAsync(string projectNo, string roadmapActivityPrereqSysId, string roadmapActivitySysId, string prerequisiteSysId);
        Task AddProjectRoadmapFromMasterAsync(string projectNo, string roadmapSysId, string createdBy, string modifiedBy);

        Task ShiftProjectRoadmapSiblingOrderAsync(string projectNo, string parentType, string parentSysId, int insertOrder, string modifiedBy);
        Task CloseProjectRoadmapMilestoneSiblingGapAsync(string projectNo, string parentType, string parentSysId, int deletedOrderIndex, string modifiedBy);

        Task<List<FormEntityLink>> GetFormEntityLinksByEntityAsync(string entityType, string entitySysId);
        Task DeleteProjectFormSubmissionsByFormEntityLinkAsync(string projectNo, string formEntityLinkSysId);
        Task DeleteProjectFormSubmissionValuesByFormEntityLinkAsync(string projectNo, string formEntityLinkSysId);

        Task DeleteProjectFieldsByMilestoneAsync(string projectNo, string milestoneSysId);
        Task DeleteProjectFieldsByTaskAsync(string projectNo, string taskSysId);
        Task DeleteProjectStatusChangesByEntityAsync(string projectNo, string entityType, string entitySysId);
        Task DeleteProjectTargetRevisionsForMilestoneAsync(string projectNo, string projectNodeSysId, string roadmapMilestoneSysId);
        Task DeleteProjectTargetRevisionsForTaskAsync(string projectNo, string projectNodeSysId, string roadmapActivitySysId);

        Task DeleteProjectOwnersByEntityAsync(string projectNo, string entityType, string entitySysId);
        Task DeleteProjectCommentsByEntityAsync(string projectNo, string entityType, string entitySysId);
        Task DeleteProjectAttachmentsByEntityAsync(string projectNo, string entityType, string entitySysId);
        Task DeleteNotificationViewedByEntityAsync(string entityType, string entitySysId);
        Task DeleteNotificationsByEntityAsync(string entityType, string entitySysId);
    }
}
