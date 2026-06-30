using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
/// <summary>
/// Interface for plant-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProjectService : IBaseChangeStatusService<Project>
    {
        /// <summary>
        /// Adds a new project to the system.
        /// </summary>
        /// <param name="project">The project to add.</param>
        Task<string> CreateProjectAsync(Project project);

        /// <summary>
        /// Updates an existing project in the system.
        /// </summary>
        /// <param name="project">The project to update.</param>
        Task UpdateProjectAsync(Project project);

        /// <summary>
        /// Deletes a project from the system by its unique identifier.
        /// </summary>
        /// <param name="projectno">The unique identifier of the project to delete.</param>
        /// <param name="loggeduser">The project who deleted the project.</param>
        /// <param name="reason">reason whe the project is deleted.</param>
        Task DeleteProjectAsync(string projectno, string loggeduser, string reason);

        /// <summary>
        /// Retrieves a project by its unique identifier.
        /// </summary>
        /// <param name="projectno">The unique identifier of the project.</param>
        /// <returns>The project with the specified USERID, or null if not found.</returns>
        Task<Project> GetProjectByIdAsync(string projectno);

        /// <summary>
        /// Retrieves a project by product code.
        /// </summary>
        /// <param name="productcode">Product code linked to the project.</param>
        /// <returns>The project with the specified product code if not found.</returns>
        Task<Project> GetProjectByProductCodeAsync(string productcode);

        /// <summary>
        /// Retrieves all projects from the system.
        /// </summary>
        /// <returns>A list of all projects.</returns>
        Task<IEnumerable<Project>> GetAllProjectsAsync();





        /// <summary>
        /// Retrieves paged projects with optional search functionality asynchronously from the system.
        /// </summary>
        /// <param name="pageNumber">The current page number (1-based).</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="searchTerm">An optional search term to filter the orders by name.</param>
        /// <returns>A <see cref="PagedResult{Order}"/> containing the total record count and the paged orders.</returns>
        Task<PagedResult<Project>> GetPagedProjectsAsync(int pageNumber, int pageSize, string searchTerm = null, string orderBy = null, string orderDirection = null);


        /// <summary>
        /// Retrieves paged projects with optional search functionality asynchronously from the system.
        /// </summary>
        /// <param name="pageNumber">The current page number (1-based).</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="searchTerm">An optional search term to filter the orders by name.</param>
        /// <returns>A <see cref="PagedResult{Project}"/> containing the total record count and the paged orders.</returns>
        Task<PagedResult<ProjectExtend>> GetPagedProjectsWithStatsAsync(ProjectExtendSearch searchTerm);

        /// <summary>
        /// Retrieves paged projects with optional search functionality asynchronously from the system.
        /// </summary>
        /// <param name="pageNumber">The current page number (1-based).</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="searchTerm">An optional search term to filter the orders by name.</param>
        /// <returns>A <see cref="PagedResult{ProjectExtend}"/> containing the total record count and the paged orders.</returns>
        Task<PagedResult<ProjectExtend>> GetPagedFullProjectsAsync(ProjectExtendSearch searchTerm);

        /// <summary>
        /// Add statuschange to Completed in the system.
        /// </summary>
        /// <param name="project">The project to promote.</param>
        /// <param name="loggeduser">logged user.</param>
        Task PromoteProject(Project project, string loggeduser);


        /// <summary>
        /// Get Submitted Value
        /// </summary>
        /// <param name="project">The project to promote.</param> 
        Task<IList<ProjectFormSubmissionExtended>> GetSubmittedForms(string projectNo);

        /// <summary>
        /// Get Node Children
        /// </summary>
        /// <param name="project">The project to retrieve.</param> 
        /// <param name="nodeType">The nodeType to retrieve.</param> 
        /// <param name="nodeId">The nodeId to retrieve.</param> 
        Task<List<ProjectExtend>> GetProjectNodeChildrenAsync(string projectNo, string nodeType, string nodeId);

        /// <summary>
        /// Get Node Items
        /// </summary>
        /// <param name="project">The project to retrieve.</param> 
        /// <param name="nodeType">The nodeType to retrieve.</param> 
        /// <param name="nodeId">The nodeId to retrieve.</param> 
        Task<ProjectExtend> GetProjectNodeItemAsync(string projectNo, string nodeType, string nodeId);


        /// <summary>
        /// Get Node Items
        /// </summary>
        /// <param name="project">The project to retrieve.</param>  
        Task<List<ProjectExtend>> GetProjectNodesAsync(string projectNo);

        /// <summary>
        /// Get Submitted Value
        /// </summary>
        /// <param name="project">The project to retrieve.</param> 
        /// <param name="nodeType">The nodeType to retrieve.</param> 
        /// <param name="nodeId">The nodeId to retrieve.</param> 
        Task<IList<ProjectFormSubmissionExtended>> GetSubmittedForms(string projectNo, string nodeType, string nodeId);



        Task<string> SubmitForm(ProjectFormSubmission form);

        Task<int> UpdateForm(ProjectFormSubmission form);


        Task LinkProduct(ProjectProduct product);

        Task UnlinkProduct(ProjectProduct product);

        Task<ProjectDashboardCounter> GetDashboardCardsCounter(string userid);

        Task<ProjectMonitoringReport> GetProjectMonitoringReportAsync(string loggedUser);

        Task<ProjectMilestoneTemplateCatalog> GetMilestoneTemplateCatalogAsync(string projectNo);

        Task<ProjectMilestoneArrangementCatalog> GetMilestoneArrangementCatalogAsync(string projectNo);

        Task<ProjectAdditionalMilestoneCatalog> GetAdditionalMilestonesAsync(string projectNo);

        Task<string> InsertProjectMilestoneAsync(string projectNo, string anchorNodeId, string insertPosition, string title, string description, bool isRequired, string loggedUser);

        Task<IList<string>> InsertProjectMilestoneTemplateAsync(string projectNo, string anchorNodeId, string insertPosition, string templateRoadmapSysId, IEnumerable<string> templateMilestoneSysIds, string loggedUser);

        Task SaveMilestoneArrangementAsync(ProjectMilestoneArrangementSaveRequest request, string loggedUser);

        Task ReorderAdditionalMilestoneAsync(string projectNo, string roadmapMilestoneSysId, string direction, string loggedUser);

        Task DeleteAdditionalMilestoneAsync(string projectNo, string roadmapMilestoneSysId, string loggedUser);

        Task<ProjectRoadmapRefreshPreview> PreviewRoadmapRefreshAsync(string projectNo);

        Task<ProjectRoadmapRefreshApplyResult> ApplyRoadmapRefreshAsync(ProjectRoadmapRefreshSelection selection, string loggedUser);

    }
}
