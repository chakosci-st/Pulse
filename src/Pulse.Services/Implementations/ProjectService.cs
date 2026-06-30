using AutoMapper;
using Newtonsoft.Json.Linq;
using Pulse.Core.Entities;
using Pulse.Core.EventArgs;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Task = System.Threading.Tasks.Task;

namespace Pulse.Services.Implementations
{

    public class ProjectService : IProjectService
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectmemberRepository;
        private readonly IProjectOwnerRepository _projectownerRepository;
        private readonly IProjectProductRepository _projectproductRepository;
        private readonly IProjectStatusChangeRepository _projectstatuschangeRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPlantCategoryMilestoneRepository _plantcategorymilestoneRepository;
        private readonly IProjectMilestoneRepository _projectmilestoneRepository;
        private readonly IProjectFormSubmissionRepository _projectformsubmissionRepository;
        private readonly IProjectFormSubmissionValueRepository _projectformsubmissionvalueRepository;
        private readonly IFormEntityLinkRepository _formEntityLinkRepository;


        private readonly IProjectTaskRepository _projecttaskRepository;
        private readonly IProjectFieldRepository _projectfieldRepository;
        private readonly IRoadmapRepository _roadmapRepository;
        private readonly IRoadmapActivityRepository _roadmapactivityRepository;
        private readonly IRoadmapActivityPrerequisiteRepository _roadmapactivityprerequisiteRepository;

        private readonly IRoadmapMilestoneRepository _roadmapmilestoneRepository;


        //private readonly IWorkItemRepository _workitemRepository;
        //private readonly IWorkItemMemberRepository _workitemmemberRepository;
        //private readonly IWorkItemPrerequisiteRepository _workitemprerequisiteRepository;
        ////private readonly ITaskRepository _taskRepository;
        ////private readonly ITaskMemberRepository _taskmemberRepository;
        ////private readonly ITaskPrerequisiteRepository _taskprerequisiteRepository;
        private readonly IPlantFieldRepository _plantfieldRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStatusChangeRepository _statuschangeRepository;
        private readonly IEmailSender _emailSender;
        private readonly IEventPublisher _eventBus;
        private readonly IActiveDirectoryService _activedirectoryService;
        private readonly IProjectMilestoneService _projectmilestoneService;
        private readonly IProjectTaskService _projecttaskService;
        private readonly IApplicationService _applicationService;

        

        public ProjectService(IEventPublisher eventBus, OracleDataAccessLayer dataAccess, IProjectRepository projectRepositorysitory, IProjectMemberRepository projectmemberRepository,
            IProjectOwnerRepository projectownerRepository, IProjectProductRepository projectproductRepository, IProductRepository productRepository, IProjectStatusChangeRepository projectstatuschangeRepository,
            IPlantCategoryMilestoneRepository plantcategorymilestoneRepository, IProjectMilestoneRepository projectmilestoneRepository, IProjectTaskRepository projecttaskRepository,
            IProjectFieldRepository projectfieldRepository, IProjectFormSubmissionRepository projectformsubmissionRepository, IProjectFormSubmissionValueRepository projectformsubmissionvalueRepository,
            IFormEntityLinkRepository formEntityLinkRepository,

            //IWorkItemRepository workitemRepository, IWorkItemMemberRepository workitemmemberRepository, IWorkItemPrerequisiteRepository workitemprerequisiteRepository,
            //ITaskRepository taskRepository, ITaskMemberRepository taskmemberRepository, ITaskPrerequisiteRepository taskprerequisiteRepository, 
            IPlantFieldRepository plantfieldRepository, IUserRepository userRepository, IRoadmapRepository roadmapRepository, IRoadmapMilestoneRepository roadmapmilestoneRepository,
            IRoadmapActivityRepository roadmapactivityRepository, IRoadmapActivityPrerequisiteRepository roadmapactivityprerequisiteRepository,
            IStatusChangeRepository statuschangeRepository, IEmailSender emailSender,
            IProjectMilestoneService projectmilestoneService, IProjectTaskService projecttaskService, IActiveDirectoryService activedirectoryService,
            IApplicationService applicationService)
        {
            _dataAccess = dataAccess;
            _projectRepository = projectRepositorysitory;
            _projectmemberRepository = projectmemberRepository;
            _projectownerRepository = projectownerRepository;
            _projectproductRepository = projectproductRepository;
            _projectstatuschangeRepository = projectstatuschangeRepository;
            _projecttaskRepository = projecttaskRepository;
            _productRepository = productRepository;
            _plantcategorymilestoneRepository = plantcategorymilestoneRepository;
            _projectmilestoneRepository = projectmilestoneRepository;
            _projectformsubmissionRepository = projectformsubmissionRepository;
            _projectformsubmissionvalueRepository = projectformsubmissionvalueRepository;
            _formEntityLinkRepository = formEntityLinkRepository;




            _projectfieldRepository = projectfieldRepository;
            _roadmapRepository = roadmapRepository;
            _roadmapactivityRepository = roadmapactivityRepository;
            _roadmapactivityprerequisiteRepository = roadmapactivityprerequisiteRepository;
            //_workitemRepository = workitemRepository;
            //_workitemmemberRepository = workitemmemberRepository;
            //_workitemprerequisiteRepository = workitemprerequisiteRepository;

            _roadmapmilestoneRepository = roadmapmilestoneRepository;
            //_taskRepository = taskRepository;
            //_taskmemberRepository = taskmemberRepository;
            //_taskprerequisiteRepository = taskprerequisiteRepository;
            _plantfieldRepository = plantfieldRepository;
            _userRepository = userRepository;
            _statuschangeRepository = statuschangeRepository;
            _emailSender = emailSender;

            _projectmilestoneService = projectmilestoneService;
            _projecttaskService = projecttaskService;
            _activedirectoryService = activedirectoryService;
            _applicationService = applicationService;
            _eventBus = eventBus;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _projectRepository.GetListAsync();
        }
        public async Task<Project> GetProjectByIdAsync(string projectid)
        {
            return await _projectRepository.GetAsync(projectid);
        }

        private static string ResolveProjectMemberUserId(IEnumerable<ProjectMember> members, string ownerUserName)
        {
            if (string.IsNullOrWhiteSpace(ownerUserName))
            {
                return string.Empty;
            }

            return members?
                .FirstOrDefault(member => string.Equals(
                    member?.User?.UserName?.Trim(),
                    ownerUserName.Trim(),
                    StringComparison.OrdinalIgnoreCase))?
                .UserId ?? string.Empty;
        }

        private async Task<User> ResolveProjectUserAsync(string userName, string userId = null)
        {
            var normalizedUserId = string.IsNullOrWhiteSpace(userId) ? null : userId.Trim();
            var normalizedUserName = string.IsNullOrWhiteSpace(userName) ? null : userName.Trim();

            if (!string.IsNullOrWhiteSpace(normalizedUserId))
            {
                var existingById = await _userRepository.GetAsync(normalizedUserId);
                if (existingById != null)
                {
                    return existingById;
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedUserName))
            {
                var existingByUserName = await _userRepository.GetByUserNameAsync(normalizedUserName);
                if (existingByUserName != null)
                {
                    return existingByUserName;
                }
            }

            if (string.IsNullOrWhiteSpace(normalizedUserName))
            {
                return null;
            }

            var directoryUser = Mapper.Map<User>(_activedirectoryService.FindUser(normalizedUserName, Core.Enums.ActiveDirectoryKeyType.Username));
            if (directoryUser == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(directoryUser.UserId))
            {
                var existingDirectoryUserById = await _userRepository.GetAsync(directoryUser.UserId.Trim());
                if (existingDirectoryUserById != null)
                {
                    return existingDirectoryUserById;
                }
            }

            if (!string.IsNullOrWhiteSpace(directoryUser.UserName))
            {
                var existingDirectoryUserByUserName = await _userRepository.GetByUserNameAsync(directoryUser.UserName.Trim());
                if (existingDirectoryUserByUserName != null)
                {
                    return existingDirectoryUserByUserName;
                }
            }

            return directoryUser;
        }

        private async Task<User> EnsureProjectUserRegisteredAsync(User user, string createdBy)
        {
            if (user == null)
            {
                return null;
            }

            if (user.Registered)
            {
                return user;
            }

            user.CreatedBy = createdBy;

            try
            {
                await _userRepository.AddAsync(user);
                user.Registered = true;
                return user;
            }
            catch (Exception ex) when (!string.IsNullOrWhiteSpace(ex.Message)
                && ex.Message.IndexOf("ORA-00001", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var existingUser = await ResolveProjectUserAsync(user.UserName, user.UserId);
                if (existingUser != null)
                {
                    return existingUser;
                }

                throw;
            }
        }

        public async Task<string> CreateProjectAsync(Project project)
        {
            try
            {
                _dataAccess.BeginTransaction();
                project.CreatedDate = DateTime.Now;

                var autostart = project.ActualStartDate != null;
                var startdate = project.ActualStartDate;
                var currentroadmapmilestonesysid = project.RoadmapMilestoneSysId;
                //temporarily clear actual
                project.ActualStartedBy = null;
                project.ActualCompletedBy = null;



                var owner = await ResolveProjectUserAsync(project.ProjectOwnerUserName, project.ProjectOwnerId);
                owner = await EnsureProjectUserRegisteredAsync(owner, project.CreatedBy);
                if (owner != null)
                {
                    project.ProjectOwnerId = owner.UserId;
                    project.ProjectOwner = owner;
                }
                // 1. Create Project
                var projectno = await _projectRepository.AddAsync(project);
                project.ProjectNo = projectno;

                // 2. Prepare and add members in parallel
                var members = project.Members.ToList();
                foreach (var member in members)
                {
                    var memberinfo = await ResolveProjectUserAsync(member.User?.UserName, member.UserId);
                    memberinfo = await EnsureProjectUserRegisteredAsync(memberinfo, project.CreatedBy);
                    member.UserId = memberinfo.UserId;
                    member.User = memberinfo;
                    member.IsOwner = string.Equals(member.UserId, project.ProjectOwnerId, StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                    member.CreatedBy = project.CreatedBy;
                    member.ProjectNo = projectno;
                }
                project.Members = members;
                var memberAddTasks = project.Members.Select(m => _projectmemberRepository.AddAsync(m));
                await Task.WhenAll(memberAddTasks);

                // 3. Prepare and add products in parallel
                var productAddTasks = project.Products.Select(p => _productRepository.AddAsync(p.Product));
                await Task.WhenAll(productAddTasks);

                var products = project.Products.ToList();

                products.ForEach(p =>
                {
                    p.ProjectNo = projectno;
                });

                project.Products = products;
                var projectproductAddTasks = project.Products.Select(p => _projectproductRepository.AddAsync(p));
                await Task.WhenAll(projectproductAddTasks);

                // 4. Fetch all workitems once
                var milestones = project.Milestones.ToList();

                milestones.ForEach(m =>
                {
                    m.ProjectNo = projectno;
                    m.PlantRoadmapLinkSysId = project.PlantRoadmapLinkSysId;
                    m.RoadmapSysId = project.RoadmapSysId;
                    m.CreatedBy = project.CreatedBy;
                    m.TargetStartedBy = m.TargetStartYear != null ? project.CreatedBy : "";
                });

                foreach (var milestone in milestones)
                {
                    milestone.MilestoneSysId = await _projectmilestoneRepository.AddAsync(milestone);
                    var owners = milestone.Owners.ToList();
                    owners.ForEach(o =>
                    {
                        o.ProjectNo = projectno;
                        o.ParentType = "MILESTONE";
                        o.ParentSysId = milestone.MilestoneSysId;
                        o.UserId = ResolveProjectMemberUserId(project.Members, o.OwnerMeta?.UserName);
                    });

                    owners = owners
                        .Where(o => !string.IsNullOrWhiteSpace(o.UserId))
                        .ToList();

                    var projectownerAddTasks = owners.Select(o => _projectownerRepository.AddAsync(o));
                    await Task.WhenAll(projectownerAddTasks);

                    milestone.Owners = owners;

                    var tasks = milestone.Tasks.ToList();

                    tasks.ForEach(t =>
                    {
                        t.ProjectNo = projectno;
                        t.PlantRoadmapLinkSysId = project.PlantRoadmapLinkSysId;
                        t.RoadmapSysId = project.RoadmapSysId;
                        t.CreatedBy = project.CreatedBy;
                    });

                    foreach (var task in tasks)
                    {
                        task.ProjectTaskSysId = await _projecttaskRepository.AddAsync(task);
                        var taskowners = task.Owners.ToList();
                        taskowners.ForEach(o =>
                        {
                            o.ProjectNo = projectno;
                            o.ParentType = "TASK";
                            o.ParentSysId = task.ProjectTaskSysId;
                            o.UserId = ResolveProjectMemberUserId(project.Members, o.OwnerMeta?.UserName);

                        });

                        taskowners = taskowners
                            .Where(o => !string.IsNullOrWhiteSpace(o.UserId))
                            .ToList();

                        var projecttaskownerAddTasks = taskowners.Select(o => _projectownerRepository.AddAsync(o));
                        await Task.WhenAll(projecttaskownerAddTasks);

                        task.Owners = taskowners;
                    }

                    milestone.Tasks = tasks;

                }
                project.Milestones = milestones;



                //5. Log status - NOT STARTED
                // PROJECT
                await this.InitializeAsync(project, "", notify: false);

                // MILESTONES
                foreach (var milestone in project.Milestones)
                {
                    milestone.ModifiedBy = project.CreatedBy;
                    await _projectmilestoneService.InitializeAsync(milestone, "", notify: false);

                    // TASKS
                    foreach (var task in milestone.Tasks)
                    {
                        task.ModifiedBy = project.CreatedBy;
                        await _projecttaskService.InitializeAsync(task, "", notify: false);

                    }

                }




                //6. Log status - ONGOING
                if (autostart)
                {
                    project.ActualStartDate = startdate;
                    project.ActualStartedBy = project.CreatedBy;
                    project.MilestoneSysId = project.Milestones.Where(m => m.RoadmapMilestoneSysId == currentroadmapmilestonesysid).FirstOrDefault().MilestoneSysId;

                    // PROJECT
                    await this.StartAsync(project, "AUTO-START", false);

                    // MILESTONE
                    foreach (var milestone in project.Milestones)
                    {
                        milestone.ActualStartDate = startdate;
                        milestone.ActualStartedBy = project.CreatedBy;

                        await _projectmilestoneService.StartAsync(milestone, "AUTO-START", notify: false);

                        // TASK
                        foreach (var task in milestone.Tasks)
                        {
                            task.ActualStartDate = startdate;
                            task.ActualStartedBy = project.CreatedBy;

                            await _projecttaskService.StartAsync(task, "AUTO-START", notify: false);

                        }

                        // END LOOP
                        if (milestone.RoadmapMilestoneSysId == project.RoadmapMilestoneSysId) break;
                    }

                    // ACTIVATE ALL ROOT TASKS
                    foreach (var milestone in project.Milestones.Where(m => string.IsNullOrEmpty(m.RoadmapMilestoneSysId)))
                    {
                        milestone.ActualStartDate = startdate;
                        milestone.ActualStartedBy = project.CreatedBy;

                        await _projectmilestoneService.StartAsync(milestone, "AUTO-START", notify: false);

                        // TASK
                        foreach (var task in milestone.Tasks)
                        {
                            task.ActualStartDate = startdate;
                            task.ActualStartedBy = project.CreatedBy;

                            await _projecttaskService.StartAsync(task, "AUTO-START", notify: false);

                        }

                        // END LOOP
                        if (milestone.RoadmapMilestoneSysId == project.RoadmapMilestoneSysId) break;
                    }
                }





                //COMMIT
                _dataAccess.CommitTransaction();

                var createdby = await _userRepository.GetAsync(project.CreatedBy);
                var _createdby = createdby.FirstName + " " + createdby.LastName;
                var emails = new List<string>();
                // get all project members
                var memberGetTasks = (await _projectmemberRepository.GetListAsync(project.ProjectNo)).Select(m => _userRepository.GetAsync(m.UserId));
                var listEmails = (await Task.WhenAll(memberGetTasks)).Select(m => m.Email);
                emails.AddRange(listEmails);


                if (autostart)
                {

                    var _roadmapmilestone = await _roadmapmilestoneRepository.GetAsync(project.RoadmapMilestoneSysId);

                    await _eventBus.Publish(new ProjectStartedEventArgs(project.ProjectNo,
                        project.ProjectName, string.Join(", ", project.Products.Select(p => p.ProductCode)),
                        project.PlantCode, project.CategoryCode, _createdby, _roadmapmilestone.MilestoneAlias, project.ActualStartDate.Value,
                        "", string.Join(",", emails), ""));
                }
                else
                {
                    await _eventBus.Publish(new ProjectNotStartedEventArgs(project.ProjectNo,
                        project.ProjectName, string.Join(", ", project.Products.Select(p => p.ProductCode)),
                        project.PlantCode, project.CategoryCode, _createdby, project.CreatedDate,
                        "", string.Join(",", emails), ""));

                    //await _eventBus.Publish(new ProjectNotStartedEventArgs(_projectId, loggeduser, DateTime.UtcNow, ""));
                }

                return projectno;
            }
            catch (Exception e)
            {
                _dataAccess.RollbackTransaction();
                throw new Exception(e.Message);
            }
        }
        public async Task UpdateProjectAsync(Project project)
        {
            _dataAccess.BeginTransaction();

            try
            {
                await _projectRepository.UpdateAsync(project);

                _dataAccess.CommitTransaction();

                //GET DETAILS
                var members = await _projectmemberRepository.GetListAsync(project.ProjectNo);

                var memberemails = string.Join(", ", members.Select(m => m.User.Email));

                var urlpath = _applicationService.GetHomeUrl();

                // raise event that project was updated
                await _eventBus.Publish(new ProjectUpdatedEventArgs(project.ProjectNo, project.ProjectName, project.UserModified.FirstName + " " + project.UserModified.LastName, project.ModifiedDate.Value, urlpath, memberemails, project.UserModified.Email));
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw new Exception(ex.Message);
            }



        }

        private async Task ApplyStatusAsync(Project obj, string status, string reason, DateTime? actualdate)
        {
            var _project = await _projectRepository.GetAsync(obj.ProjectNo);

            if (!string.IsNullOrEmpty(obj.TransactionKey))
            {
                _project.TransactionKey = obj.TransactionKey;
            }
            _project.MilestoneSysId = obj.MilestoneSysId;
            _project.Status = status;
            _project.ModifiedBy = obj.ModifiedBy;
            _project.ModifiedDate = DateTime.UtcNow;
            _project.ActualStartDate = status == "NOT STARTED" ? null : (status == "ONGOING" ? actualdate : _project.ActualStartDate);
            _project.ActualStartedBy = status == "NOT STARTED" ? null : (status == "ONGOING" ? obj.ModifiedBy : _project.ActualStartedBy);
            _project.ActualCompletionDate = status == "NOT STARTED" ? null : ((status == "COMPLETED" || status == "ARCHIVED") ? actualdate : _project.ActualCompletionDate);
            _project.ActualCompletedBy = status == "NOT STARTED" ? null : ((status == "COMPLETED" || status == "ARCHIVED") ? obj.ModifiedBy : _project.ActualCompletedBy);
            var rowsaffected = await _projectRepository.UpdateAsync(_project);
            if (rowsaffected > 0)
            {
                var statusActualDate = status == "NOT STARTED"
                    ? DateTime.UtcNow
                    : (status == "ONGOING"
                        ? (actualdate ?? DateTime.UtcNow)
                        : (actualdate ?? _project.ActualCompletionDate ?? _project.ActualStartDate ?? DateTime.UtcNow));

                await _projectstatuschangeRepository.AddAsync(new ProjectStatusChange
                {
                    Status = status,
                    ProjectNo = obj.ProjectNo,
                    ActualDate = statusActualDate,
                    CreatedBy = obj.ModifiedBy,
                    EntitySysId = obj.ProjectNo,
                    EntityType = "PROJECT",
                    Remarks = reason
                });
            }
            else
            {
                throw new Exception("Project is either recently updated or removed.");
            }
        }

        public async Task InitializeAsync(Project obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                await ApplyStatusAsync(obj, "NOT STARTED", reason, null);

                ////if (notify.Value)
                ////    // raise event that Task status was changed
                ////    await _eventBus.Publish(new ProjectNotStartedEventArgs(obj.ProjectNo, obj.ModifiedBy, obj.ModifiedDate.Value, reason));

            }
            catch
            {

                throw;
            }
        }

        public async Task StartAsync(Project obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                await ApplyStatusAsync(obj, "ONGOING", reason, obj.ActualStartDate);

                //  if (notify.Value)
                // raise event that Task status was changed
                //  await _eventBus.Publish(new ProjectStartedEventArgs(obj.ProjectNo, obj.ModifiedBy, obj.ActualStartDate.Value, reason));

            }
            catch
            {
                throw;
            }

        }

        public async Task HoldAsync(Project obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                var _project = await _projectRepository.GetAsync(obj.ProjectNo);
                await ApplyStatusAsync(obj, "HOLD", reason, DateTime.UtcNow);

                if (notify.Value) { 
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectHoldEventArgs(obj.ProjectNo, _project.Status, "HOLD", reason, obj.ModifiedBy));
                }
                    

            }
            catch
            {
                throw;
            }
        }

        public async Task UnholdAsync(Project obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {

                await ApplyStatusAsync(obj, "ONGOING", reason, DateTime.UtcNow);

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectResumedEventArgs(obj.ProjectNo, "HOLD", obj.ModifiedBy, DateTime.UtcNow, reason));

            }
            catch
            {
                throw;
            }
        }

        public async Task CancelAsync(Project obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {

                await ApplyStatusAsync(obj, "CANCEL", reason, DateTime.UtcNow);

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectCanceledEventArgs(obj.ProjectNo, obj.ModifiedBy, DateTime.UtcNow, reason));

            }
            catch
            {
                throw;
            }
        }

        public async Task ArchiveAsync(Project obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                await ApplyStatusAsync(obj, "ARCHIVED", reason, DateTime.UtcNow);
            }
            catch
            {
                throw;
            }
        }

        public async Task CompleteAsync(Project obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                //CHECK IF ALL REQUIRED TASKS ARE COMPLETE 



                await ApplyStatusAsync(obj, "COMPLETED", reason, DateTime.UtcNow);

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectCompletedEventArgs(obj.ProjectNo, obj.ModifiedBy, DateTime.UtcNow, reason));

            }
            catch
            {
                throw;
            }
        }

        public async Task ForceCompleteAsync(Project obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                await ApplyStatusAsync(obj, "COMPLETED", reason, DateTime.UtcNow);

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectCompletedEventArgs(obj.ProjectNo, obj.ModifiedBy, DateTime.UtcNow, reason));

            }
            catch
            {
                throw;
            }
        }





        #region Form Submission
        public async Task<string> SubmitForm(ProjectFormSubmission form)
        {

            _dataAccess.BeginTransaction();

            try
            {
                var submissionsysid = await _projectformsubmissionRepository.AddAsync(form);
                foreach (var entity in form.SubmissionValues)
                {
                    await _projectformsubmissionvalueRepository.AddAsync(entity);
                }


                _dataAccess.CommitTransaction();


                return submissionsysid;

                // raise event that project was updated
                //await _eventBus.Publish(new ProjectUpdatedEventArgs(project.ProjectNo, project.ModifiedBy, project.ModifiedDate.Value));
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> UpdateForm(ProjectFormSubmission form)
        {
            _dataAccess.BeginTransaction();

            try
            {
                var submissionsysid = await _projectformsubmissionRepository.UpdateAsync(form);
                foreach (var entity in form.SubmissionValues)
                {
                    await _projectformsubmissionvalueRepository.UpdateAsync(entity);
                }


                _dataAccess.CommitTransaction();


                return submissionsysid;

                // raise event that project was updated
                //await _eventBus.Publish(new ProjectUpdatedEventArgs(project.ProjectNo, project.ModifiedBy, project.ModifiedDate.Value));
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }
        #endregion



        public Task DeleteProjectAsync(string projectno, string loggeduser, string reason)
        {
            return _projectRepository.DeleteAsync(projectno);
        }

        public Task<Project> GetProjectByProductCodeAsync(string productcode)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<Project>> GetPagedProjectsAsync(int pageNumber, int pageSize, string searchTerm = null, string orderBy = null, string orderDirection = null)
        {
            throw new NotImplementedException();
        }

        public Task PromoteProject(Project project, string loggeduser)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResult<ProjectExtend>> GetPagedFullProjectsAsync(ProjectExtendSearch searchTerm)
        {
            return await _projectRepository.GetPagedFullProjectsAsync(searchTerm);
        }

        public async Task<PagedResult<ProjectExtend>> GetPagedProjectsWithStatsAsync(ProjectExtendSearch searchTerm)
        {
            return await _projectRepository.GetPagedProjectsWithStatsAsync(searchTerm);
        }

        public async Task<List<ProjectExtend>> GetProjectNodeChildrenAsync(string projectNo, string nodeType, string nodeId)
        {
            return await _projectRepository.GetProjectNodeChildrenAsync(projectNo, nodeType, nodeId);
        }

        public async Task<ProjectExtend> GetProjectNodeItemAsync(string projectNo, string nodeType, string nodeId)
        {
            return await _projectRepository.GetProjectNodeItemAsync(projectNo, nodeType, nodeId);
        }

        public async Task<List<ProjectExtend>> GetProjectNodesAsync(string projectNo)
        {
            return await _projectRepository.GetProjectNodesAsync(projectNo);
        }

        public async Task<IList<ProjectFormSubmissionExtended>> GetSubmittedForms(string projectNo)
        {
            return await _projectformsubmissionRepository.GetSubmittedForms(projectNo);
        }

        public async Task<IList<ProjectFormSubmissionExtended>> GetSubmittedForms(string projectNo, string entityType, string entitySysId)
        {
            return await _projectformsubmissionRepository.GetSubmittedForms(projectNo, entityType, entitySysId);
        }

        #region Product
        public async Task LinkProduct(ProjectProduct product)
        {
            await _projectproductRepository.AddAsync(product);

        }

        public async Task UnlinkProduct(ProjectProduct product)
        {
            await _projectproductRepository.DeleteAsync(product.ProductCode);
        }

        public async Task<ProjectDashboardCounter> GetDashboardCardsCounter(string userid)
        {
            return await _projectRepository.GetDashboardCardsCounter(userid);
        }

        public async Task<ProjectMonitoringReport> GetProjectMonitoringReportAsync(string loggedUser)
        {
            var projects = (await _projectRepository.GetPagedFullProjectsAsync(new ProjectExtendSearch
            {
                Search = string.Empty,
                Status = "NOT STARTED,ONGOING,HOLD,COMPLETED,CANCELLED,ARCHIVED",
                LoggedUser = loggedUser,
                NodeType = "roadmap",
                OrderColumn = "projectname",
                OrderDir = "asc",
                StartIndex = 0,
                LengthCount = int.MaxValue
            })).Data
            .GroupBy(project => project.ProjectNo)
            .Select(group => group.First())
            .ToList();

            var projectNodes = await _projectRepository.GetProjectNodesByUserAsync(loggedUser);
            var milestoneNodes = projectNodes
                .Where(node => string.Equals(node.NodeType, "milestone", StringComparison.OrdinalIgnoreCase))
                .Where(node => !string.IsNullOrWhiteSpace(node.ProjectNo) && !string.IsNullOrWhiteSpace(node.NodeName))
                .GroupBy(node => BuildProjectMilestoneNameKey(node.ProjectNo, node.NodeName), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, SelectMonitoringMilestoneNode, StringComparer.OrdinalIgnoreCase);
            var taskNodes = projectNodes
                .Where(node => string.Equals(node.NodeType, "activity", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var dmsLookup = BuildDmsLookup(await _projectformsubmissionvalueRepository.GetDmsValuesForMonitoringAsync(loggedUser));

            var milestoneGroups = new Dictionary<string, ProjectMonitoringMilestoneGroup>(StringComparer.OrdinalIgnoreCase);
            foreach (var taskNode in taskNodes)
            {
                var milestoneName = ResolveMilestoneName(taskNode, milestoneNodes);
                var milestoneKey = NormalizeReportKey(milestoneName);
                if (!milestoneGroups.TryGetValue(milestoneKey, out var group))
                {
                    group = new ProjectMonitoringMilestoneGroup
                    {
                        MilestoneKey = milestoneKey,
                        MilestoneName = milestoneName,
                        OrderIndex = ResolveMilestoneOrder(taskNode, milestoneNodes)
                    };
                    milestoneGroups[milestoneKey] = group;
                }

                var columnKey = BuildMonitoringColumnKey(milestoneName, taskNode.NodeName);
                if (group.Tasks.Any(task => string.Equals(task.ColumnKey, columnKey, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                group.Tasks.Add(new ProjectMonitoringTaskColumn
                {
                    ColumnKey = columnKey,
                    TaskName = taskNode.NodeName,
                    Prerequisites = ResolvePrerequisites(taskNode.PrerequisitesJson),
                    OrderIndex = taskNode.OrderIndex
                });
            }

            var orderedMilestones = milestoneGroups.Values
                .OrderBy(group => group.OrderIndex)
                .ThenBy(group => group.MilestoneName)
                .Select(group =>
                {
                    group.Tasks = group.Tasks
                        .OrderBy(task => task.OrderIndex)
                        .ThenBy(task => task.TaskName)
                        .ToList();
                    return group;
                })
                .ToList();

            var rows = new List<ProjectMonitoringRow>();
            foreach (var project in projects)
            {
                var row = new ProjectMonitoringRow
                {
                    ProjectNo = project.ProjectNo,
                    ProjectName = project.ProjectName,
                    OwnerName = $"{project.ProjectOwnerFirstName} {project.ProjectOwnerLastName}".Trim(),
                    Status = ResolveDisplayStatus(project.Status, project.TargetCompletion),
                    PlantCode = project.PlantCode,
                    CategoryCode = project.CategoryCode,
                    ProductCodes = project.ProductCodes,
                    TargetStart = project.TargetStart,
                    TargetCompletion = project.TargetCompletion
                };

                foreach (var milestone in orderedMilestones)
                {
                    foreach (var task in milestone.Tasks)
                    {
                        row.TaskValues[task.ColumnKey] = string.Empty;
                    }
                }

                var projectTaskNodes = taskNodes.Where(node => string.Equals(node.ProjectNo, project.ProjectNo, StringComparison.OrdinalIgnoreCase));
                foreach (var taskNode in projectTaskNodes)
                {
                    var milestoneName = ResolveMilestoneName(taskNode, milestoneNodes);
                    var columnKey = BuildMonitoringColumnKey(milestoneName, taskNode.NodeName);
                    var dmsValue = ResolveTaskDmsValue(dmsLookup, project.ProjectNo, taskNode.NodeId, taskNode.ProjectNodeSysId);

                    row.TaskValues[columnKey] = !string.IsNullOrWhiteSpace(dmsValue)
                        ? dmsValue
                        : ResolveDisplayStatus(taskNode.ProjectNodeStatus, taskNode.ProjectNodeTargetCompletion ?? taskNode.ProjectNodeTargetCompletionDate);
                }

                rows.Add(row);
            }

            return new ProjectMonitoringReport
            {
                GeneratedAt = DateTime.Now,
                Milestones = orderedMilestones,
                Rows = rows.OrderBy(row => row.ProjectName).ThenBy(row => row.ProjectNo).ToList()
            };
        }

        private static IDictionary<string, IDictionary<string, string>> BuildDmsLookup(IEnumerable<ProjectMonitoringDmsValue> values)
        {
            var dmsLookup = new Dictionary<string, IDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var value in values ?? Enumerable.Empty<ProjectMonitoringDmsValue>())
            {
                if (string.IsNullOrWhiteSpace(value.ProjectNo)
                    || string.IsNullOrWhiteSpace(value.NodeId)
                    || string.IsNullOrWhiteSpace(value.DmsValue))
                {
                    continue;
                }

                if (!dmsLookup.TryGetValue(value.ProjectNo, out var projectValues))
                {
                    projectValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    dmsLookup[value.ProjectNo] = projectValues;
                }

                projectValues[value.NodeId] = value.DmsValue.Trim();
            }

            return dmsLookup;
        }

        private static string ResolveTaskDmsValue(
            IDictionary<string, IDictionary<string, string>> dmsLookup,
            string projectNo,
            string roadmapNodeId,
            string projectNodeSysId)
        {
            if (dmsLookup == null || string.IsNullOrWhiteSpace(projectNo)
                || !dmsLookup.TryGetValue(projectNo, out var projectValues)
                || projectValues == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(roadmapNodeId)
                && projectValues.TryGetValue(roadmapNodeId, out var roadmapValue)
                && !string.IsNullOrWhiteSpace(roadmapValue))
            {
                return roadmapValue;
            }

            if (!string.IsNullOrWhiteSpace(projectNodeSysId)
                && projectValues.TryGetValue(projectNodeSysId, out var projectNodeValue)
                && !string.IsNullOrWhiteSpace(projectNodeValue))
            {
                return projectNodeValue;
            }

            return string.Empty;
        }

        private static string ResolvePrerequisites(string prerequisitesJson)
        {
            if (string.IsNullOrWhiteSpace(prerequisitesJson))
            {
                return "-";
            }

            try
            {
                var root = JObject.Parse(prerequisitesJson);
                var prerequisites = root["prerequisites"] as JArray;
                if (prerequisites == null || prerequisites.Count == 0)
                {
                    return "-";
                }

                var names = prerequisites
                    .OfType<JObject>()
                    .Select(item => ((string)item["name"] ?? string.Empty).Trim())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return names.Count == 0 ? "-" : string.Join(", ", names);
            }
            catch
            {
                return "-";
            }
        }

        private static string ResolveMilestoneName(ProjectExtend taskNode, IDictionary<string, ProjectExtend> milestoneNodes)
        {
            var milestone = FindMonitoringMilestone(taskNode, milestoneNodes);
            if (milestone != null)
            {
                return milestone.NodeName;
            }

            return "Root Activity";
        }

        private static ProjectExtend SelectMonitoringMilestoneNode(IEnumerable<ProjectExtend> nodes)
        {
            return (nodes ?? Enumerable.Empty<ProjectExtend>())
                .OrderByDescending(node => !string.IsNullOrWhiteSpace(node.ProjectNodeSysId))
                .ThenByDescending(node => !string.IsNullOrWhiteSpace(node.NodeName))
                .ThenBy(node => node.OrderIndex)
                .FirstOrDefault();
        }

        private static int ResolveMilestoneOrder(ProjectExtend taskNode, IDictionary<string, ProjectExtend> milestoneNodes)
        {
            var milestone = FindMonitoringMilestone(taskNode, milestoneNodes);
            if (milestone != null)
            {
                return milestone.OrderIndex;
            }

            return taskNode.OrderIndex;
        }

        private static string ResolveDisplayStatus(string status, DateTime? targetDate)
        {
            var normalized = (status ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return "NOT STARTED";
            }

            if (normalized == "COMPLETED")
            {
                normalized = "COMPLETED";
            }

            if (normalized == "CANCELED")
            {
                normalized = "CANCELLED";
            }

            if (normalized == "ONGOING" && targetDate.HasValue && targetDate.Value.Date < DateTime.Today)
            {
                return "AT RISK";
            }

            return normalized;
        }

        private static string BuildMonitoringColumnKey(string milestoneName, string taskName)
        {
            return $"{NormalizeReportKey(milestoneName)}::{NormalizeReportKey(taskName)}";
        }

        private static ProjectExtend FindMonitoringMilestone(ProjectExtend taskNode, IDictionary<string, ProjectExtend> milestoneNodes)
        {
            if (taskNode == null || milestoneNodes == null
                || !string.Equals(taskNode.ParentType, "milestone", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(taskNode.ParentSysId))
            {
                return null;
            }

            return milestoneNodes.Values.FirstOrDefault(milestone => milestone != null
                && string.Equals(milestone.ProjectNo, taskNode.ProjectNo, StringComparison.OrdinalIgnoreCase)
                && (string.Equals(milestone.NodeId, taskNode.ParentSysId, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(milestone.ProjectNodeSysId, taskNode.ParentSysId, StringComparison.OrdinalIgnoreCase)));
        }

        private static string BuildProjectMilestoneNameKey(string projectNo, string nodeName)
        {
            return $"{projectNo}::{NormalizeReportKey(nodeName)}";
        }

        private static string NormalizeReportKey(string value)
        {
            return (value ?? string.Empty).Trim().ToUpperInvariant();
        }

        public async Task<ProjectMilestoneTemplateCatalog> GetMilestoneTemplateCatalogAsync(string projectNo)
        {
            if (string.IsNullOrWhiteSpace(projectNo))
            {
                throw new Exception("Project number is required.");
            }

            var project = await _projectRepository.GetAsync(projectNo);
            if (project == null)
            {
                throw new Exception("Project does not exist.");
            }

            var catalog = new ProjectMilestoneTemplateCatalog();
            var roadmaps = (await _roadmapRepository.GetListAsync())
                .Where(roadmap => roadmap != null && roadmap.IsActive == 1)
                .OrderBy(roadmap => roadmap.RoadmapName)
                .ThenBy(roadmap => roadmap.RoadmapSysId)
                .ToList();

            foreach (var roadmap in roadmaps)
            {
                var nodes = (await _roadmapRepository.GetNodes(roadmap.RoadmapSysId))
                    .Where(node => node != null && (node.IsActive ?? 1) == 1)
                    .ToList();

                var nodeMap = nodes.ToDictionary(node => NormalizeNodeKey(node.NodeId), node => node, StringComparer.OrdinalIgnoreCase);
                var milestoneItems = nodes
                    .Where(node => string.Equals(node.NodeType, "milestone", StringComparison.OrdinalIgnoreCase))
                    .Select(node => new ProjectMilestoneTemplateItem
                    {
                        RoadmapMilestoneSysId = node.NodeId,
                        Title = node.DataName,
                        Description = node.DataDescription,
                        Path = BuildRoadmapNodePath(node, nodeMap)
                    })
                    .OrderBy(item => item.Path)
                    .ToList();

                if (!milestoneItems.Any())
                {
                    continue;
                }

                catalog.Roadmaps.Add(new ProjectMilestoneTemplateRoadmap
                {
                    RoadmapSysId = roadmap.RoadmapSysId,
                    RoadmapName = roadmap.RoadmapName,
                    RoadmapDescription = roadmap.RoadmapDescription,
                    Milestones = milestoneItems
                });
            }

            return catalog;
        }

        public async Task<ProjectAdditionalMilestoneCatalog> GetAdditionalMilestonesAsync(string projectNo)
        {
            var (project, snapshotMilestones, snapshotActivities, _) = await LoadProjectMilestoneManagementContextAsync(projectNo);
            var catalog = new ProjectAdditionalMilestoneCatalog();
            var snapshotMilestoneMap = snapshotMilestones.ToDictionary(milestone => NormalizeNodeKey(milestone.RoadmapMilestoneSysId), milestone => milestone, StringComparer.OrdinalIgnoreCase);
            var snapshotActivityMap = snapshotActivities.ToDictionary(activity => NormalizeNodeKey(activity.RoadmapActivitySysId), activity => activity, StringComparer.OrdinalIgnoreCase);

            var milestoneSiblings = snapshotMilestones
                .GroupBy(milestone => BuildRoadmapParentKey(milestone.ParentType, milestone.ParentSysId), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderBy(milestone => milestone.OrderIndex)
                        .ThenBy(milestone => milestone.MilestoneAlias ?? string.Empty)
                        .ThenBy(milestone => milestone.RoadmapMilestoneSysId)
                        .ToList(),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var milestone in snapshotMilestones
                .Where(milestone => CanDeleteInsertedMilestoneRoot(milestone, project.RoadmapSysId, snapshotMilestoneMap, snapshotActivityMap))
                .OrderByDescending(milestone => milestone.CreatedDate)
                .ThenBy(milestone => milestone.MilestoneAlias ?? string.Empty))
            {
                milestoneSiblings.TryGetValue(BuildRoadmapParentKey(milestone.ParentType, milestone.ParentSysId), out var siblings);
                siblings = siblings ?? new List<RoadmapMilestone> { milestone };

                var siblingIndex = siblings.FindIndex(item => string.Equals(item.RoadmapMilestoneSysId, milestone.RoadmapMilestoneSysId, StringComparison.OrdinalIgnoreCase));

                catalog.Milestones.Add(new ProjectAdditionalMilestoneItem
                {
                    RoadmapMilestoneSysId = milestone.RoadmapMilestoneSysId,
                    Title = milestone.MilestoneAlias,
                    Description = milestone.MilestoneDescription,
                    Path = BuildMilestonePath(milestone, snapshotMilestones, snapshotActivities),
                    CreatedBy = milestone.CreatedBy,
                    CreatedDate = milestone.CreatedDate,
                    OrderIndex = milestone.OrderIndex,
                    CanMoveUp = siblingIndex > 0,
                    CanMoveDown = siblingIndex >= 0 && siblingIndex < siblings.Count - 1
                });
            }

            return catalog;
        }

        public async Task<ProjectMilestoneArrangementCatalog> GetMilestoneArrangementCatalogAsync(string projectNo)
        {
            var (project, snapshotMilestones, snapshotActivities, _) = await LoadProjectMilestoneManagementContextAsync(projectNo);
            var catalog = new ProjectMilestoneArrangementCatalog();
            var snapshotMilestoneMap = snapshotMilestones.ToDictionary(milestone => NormalizeNodeKey(milestone.RoadmapMilestoneSysId), milestone => milestone, StringComparer.OrdinalIgnoreCase);
            var snapshotActivityMap = snapshotActivities.ToDictionary(activity => NormalizeNodeKey(activity.RoadmapActivitySysId), activity => activity, StringComparer.OrdinalIgnoreCase);

            foreach (var group in snapshotMilestones
                .GroupBy(milestone => BuildRoadmapParentKey(milestone.ParentType, milestone.ParentSysId), StringComparer.OrdinalIgnoreCase)
                .OrderBy(group => BuildMilestoneParentPath(group.First().ParentType, group.First().ParentSysId, snapshotMilestones, snapshotActivities))
                .ThenBy(group => group.Key))
            {
                var orderedMilestones = group
                    .OrderBy(milestone => milestone.OrderIndex)
                    .ThenBy(milestone => milestone.MilestoneAlias ?? string.Empty)
                    .ThenBy(milestone => milestone.RoadmapMilestoneSysId)
                    .ToList();

                var firstMilestone = orderedMilestones.First();
                catalog.Groups.Add(new ProjectMilestoneArrangementGroup
                {
                    ParentType = firstMilestone.ParentType,
                    ParentSysId = firstMilestone.ParentSysId,
                    ParentPath = BuildMilestoneParentPath(firstMilestone.ParentType, firstMilestone.ParentSysId, snapshotMilestones, snapshotActivities),
                    Milestones = orderedMilestones.Select(milestone => new ProjectMilestoneArrangementItem
                    {
                        RoadmapMilestoneSysId = milestone.RoadmapMilestoneSysId,
                        RoadmapSysId = milestone.RoadmapSysId,
                        Title = milestone.MilestoneAlias,
                        Description = milestone.MilestoneDescription,
                        Path = BuildMilestonePath(milestone, snapshotMilestones, snapshotActivities),
                        CreatedBy = milestone.CreatedBy,
                        CreatedDate = milestone.CreatedDate,
                        OrderIndex = milestone.OrderIndex,
                        CanDelete = CanDeleteInsertedMilestoneRoot(milestone, project.RoadmapSysId, snapshotMilestoneMap, snapshotActivityMap)
                    }).ToList()
                });
            }

            return catalog;
        }

        public async Task SaveMilestoneArrangementAsync(ProjectMilestoneArrangementSaveRequest request, string loggedUser)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProjectNo))
            {
                throw new Exception("Project sequence update is required.");
            }

            var (_, snapshotMilestones, _, _) = await LoadProjectMilestoneManagementContextAsync(request.ProjectNo);
            var milestoneGroups = snapshotMilestones
                .GroupBy(milestone => BuildRoadmapParentKey(milestone.ParentType, milestone.ParentSysId), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderBy(milestone => milestone.OrderIndex)
                        .ThenBy(milestone => milestone.MilestoneAlias ?? string.Empty)
                        .ThenBy(milestone => milestone.RoadmapMilestoneSysId)
                        .ToList(),
                    StringComparer.OrdinalIgnoreCase);

            var submittedGroups = (request.Groups ?? new List<ProjectMilestoneArrangementSaveGroup>())
                .Where(group => group != null)
                .ToList();

            if (!submittedGroups.Any())
            {
                throw new Exception("At least one milestone group is required.");
            }

            _dataAccess.BeginTransaction();
            try
            {
                foreach (var group in submittedGroups)
                {
                    var groupKey = BuildRoadmapParentKey(group.ParentType, group.ParentSysId);
                    if (!milestoneGroups.TryGetValue(groupKey, out var currentMilestones) || !currentMilestones.Any())
                    {
                        throw new Exception("One of the milestone groups could not be found. Refresh and try again.");
                    }

                    var orderedIds = (group.OrderedRoadmapMilestoneSysIds ?? new List<string>())
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Select(id => id.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var currentIds = currentMilestones.Select(milestone => milestone.RoadmapMilestoneSysId).ToList();
                    if (orderedIds.Count != currentIds.Count
                        || currentIds.Except(orderedIds, StringComparer.OrdinalIgnoreCase).Any()
                        || orderedIds.Except(currentIds, StringComparer.OrdinalIgnoreCase).Any())
                    {
                        throw new Exception("One of the milestone groups changed while you were arranging it. Refresh and try again.");
                    }

                    var currentById = currentMilestones.ToDictionary(milestone => milestone.RoadmapMilestoneSysId, milestone => milestone, StringComparer.OrdinalIgnoreCase);
                    var orderSlots = currentMilestones.Select(milestone => milestone.OrderIndex).OrderBy(orderIndex => orderIndex).ToList();

                    for (var index = 0; index < orderedIds.Count; index++)
                    {
                        var roadmapMilestoneSysId = orderedIds[index];
                        var targetOrderIndex = orderSlots[index];
                        if (!currentById.TryGetValue(roadmapMilestoneSysId, out var milestone) || milestone.OrderIndex == targetOrderIndex)
                        {
                            continue;
                        }

                        await UpdateProjectRoadmapMilestoneOrderAsync(request.ProjectNo, roadmapMilestoneSysId, targetOrderIndex, loggedUser);
                    }
                }

                _dataAccess.CommitTransaction();
            }
            catch
            {
                _dataAccess.RollbackTransaction();
                throw;
            }
        }

        public async Task<string> InsertProjectMilestoneAsync(string projectNo, string anchorNodeId, string insertPosition, string title, string description, bool isRequired, string loggedUser)
        {
            if (string.IsNullOrWhiteSpace(projectNo))
            {
                throw new Exception("Project number is required.");
            }

            if (string.IsNullOrWhiteSpace(anchorNodeId))
            {
                throw new Exception("Reference milestone is required.");
            }

            var normalizedTitle = (title ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedTitle))
            {
                throw new Exception("Milestone title is required.");
            }

            var project = await _projectRepository.GetAsync(projectNo);
            if (project == null)
            {
                throw new Exception("Project does not exist.");
            }

                        var anchor = await _projectRepository.GetProjectRoadmapMilestoneAsync(projectNo, anchorNodeId.Trim());

            if (anchor == null)
            {
                throw new Exception("The selected milestone no longer exists.");
            }

            var normalizedPosition = NormalizeInsertPosition(insertPosition);
            var insertOrder = normalizedPosition == "BEFORE" ? anchor.OrderIndex : anchor.OrderIndex + 1;
            var customRoadmapMilestoneSysId = Guid.NewGuid().ToString();
            var customRoadmapSourceSysId = BuildInsertedMilestoneRoadmapSysId();
            var normalizedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

            _dataAccess.BeginTransaction();
            try
            {
                await ShiftProjectSnapshotSiblingOrderAsync(projectNo, anchor.ParentType, anchor.ParentSysId, insertOrder, loggedUser);

                await _projectRepository.AddProjectRoadmapMilestoneAsync(
                    projectNo,
                    customRoadmapMilestoneSysId,
                    customRoadmapSourceSysId,
                    anchor.MaturityCode,
                    anchor.ParentType,
                    anchor.ParentSysId,
                    normalizedTitle,
                    normalizedDescription,
                    insertOrder,
                    1,
                    isRequired ? 1 : 0,
                    loggedUser,
                    loggedUser);

                var executionMilestone = new ProjectMilestone
                {
                    ProjectNo = projectNo,
                    PlantRoadmapLinkSysId = project.PlantRoadmapLinkSysId,
                    RoadmapSysId = project.RoadmapSysId,
                    RoadmapMilestoneSysId = customRoadmapMilestoneSysId,
                    Status = "NOT STARTED",
                    IsRequired = isRequired ? 1 : 0,
                    IsActive = 1,
                    CreatedBy = loggedUser,
                    ModifiedBy = loggedUser
                };

                executionMilestone.MilestoneSysId = await _projectmilestoneRepository.AddAsync(executionMilestone);
                await _projectmilestoneService.InitializeAsync(executionMilestone, "PROJECT MILESTONE INSERT", notify: false);

                await _projectownerRepository.AddAsync(new ProjectOwner
                {
                    ProjectNo = projectNo,
                    UserId = loggedUser,
                    ParentType = "MILESTONE",
                    ParentSysId = executionMilestone.MilestoneSysId
                });

                _dataAccess.CommitTransaction();
                return customRoadmapMilestoneSysId;
            }
            catch
            {
                _dataAccess.RollbackTransaction();
                throw;
            }
        }

        public async Task<IList<string>> InsertProjectMilestoneTemplateAsync(string projectNo, string anchorNodeId, string insertPosition, string templateRoadmapSysId, IEnumerable<string> templateMilestoneSysIds, string loggedUser)
        {
            if (string.IsNullOrWhiteSpace(projectNo))
            {
                throw new Exception("Project number is required.");
            }

            if (string.IsNullOrWhiteSpace(anchorNodeId))
            {
                throw new Exception("Reference milestone is required.");
            }

            var normalizedTemplateMilestoneIds = (templateMilestoneSysIds ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (string.IsNullOrWhiteSpace(templateRoadmapSysId) || !normalizedTemplateMilestoneIds.Any())
            {
                throw new Exception("Template roadmap and milestone selection are required.");
            }

            var project = await _projectRepository.GetAsync(projectNo);
            if (project == null)
            {
                throw new Exception("Project does not exist.");
            }

            var sourceRoadmap = await _roadmapRepository.GetAsync(templateRoadmapSysId.Trim());
            if (sourceRoadmap == null || sourceRoadmap.IsActive != 1)
            {
                throw new Exception("The selected roadmap template is no longer available.");
            }

                        var anchor = await _projectRepository.GetProjectRoadmapMilestoneAsync(projectNo, anchorNodeId.Trim());

            if (anchor == null)
            {
                throw new Exception("The selected milestone no longer exists.");
            }

            var sourceMilestones = (await _roadmapmilestoneRepository.GetListAsync(sourceRoadmap.RoadmapSysId)).ToList();
            var sourceActivities = (await _roadmapactivityRepository.GetListAsync(sourceRoadmap.RoadmapSysId)).ToList();
            var sourcePrereqs = (await _roadmapactivityprerequisiteRepository.GetListAsync(sourceRoadmap.RoadmapSysId)).ToList();
            var sourceForms = (await _formEntityLinkRepository.GetListByRoadmapAsync(sourceRoadmap.RoadmapSysId)).ToList();

            var sourceMilestoneMap = sourceMilestones.ToDictionary(milestone => NormalizeNodeKey(milestone.RoadmapMilestoneSysId), milestone => milestone, StringComparer.OrdinalIgnoreCase);
            var sourceActivityMap = sourceActivities.ToDictionary(activity => NormalizeNodeKey(activity.RoadmapActivitySysId), activity => activity, StringComparer.OrdinalIgnoreCase);

            var milestonesByParent = sourceMilestones
                .GroupBy(milestone => BuildRoadmapParentKey(milestone.ParentType, milestone.ParentSysId), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);
            var activitiesByParent = sourceActivities
                .GroupBy(activity => BuildRoadmapParentKey(activity.ParentType, activity.ParentSysId), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

            var requestedRootMilestones = new List<RoadmapMilestone>();
            foreach (var templateMilestoneSysId in normalizedTemplateMilestoneIds)
            {
                var sourceRootKey = NormalizeNodeKey(templateMilestoneSysId);
                if (!sourceMilestoneMap.TryGetValue(sourceRootKey, out var sourceRootMilestone))
                {
                    throw new Exception("One or more selected milestone templates no longer exist.");
                }

                requestedRootMilestones.Add(sourceRootMilestone);
            }

            var selectedRootKeys = new HashSet<string>(
                requestedRootMilestones.Select(milestone => NormalizeNodeKey(milestone.RoadmapMilestoneSysId)),
                StringComparer.OrdinalIgnoreCase);
            var effectiveRootMilestones = requestedRootMilestones
                .Where(milestone => !HasSelectedMilestoneAncestor(milestone, selectedRootKeys, sourceMilestoneMap, sourceActivityMap))
                .ToList();

            var normalizedPosition = NormalizeInsertPosition(insertPosition);
            var insertOrder = normalizedPosition == "BEFORE" ? anchor.OrderIndex : anchor.OrderIndex + 1;

            var executionMilestoneMap = (await _projectmilestoneRepository.GetListAsync(projectNo))
                .Where(milestone => milestone != null && !string.IsNullOrWhiteSpace(milestone.RoadmapMilestoneSysId))
                .ToDictionary(milestone => NormalizeNodeKey(milestone.RoadmapMilestoneSysId), milestone => milestone, StringComparer.OrdinalIgnoreCase);
            var executionTaskMap = (await _projecttaskRepository.GetListAsync(projectNo))
                .Where(task => task != null && !string.IsNullOrWhiteSpace(task.RoadmapActivitySysId))
                .ToDictionary(task => NormalizeNodeKey(task.RoadmapActivitySysId), task => task, StringComparer.OrdinalIgnoreCase);
            var insertedRootMilestoneIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            _dataAccess.BeginTransaction();
            try
            {
                foreach (var sourceRootMilestone in effectiveRootMilestones.AsEnumerable().Reverse())
                {
                    var sourceRootKey = NormalizeNodeKey(sourceRootMilestone.RoadmapMilestoneSysId);
                    var includedMilestones = new Dictionary<string, RoadmapMilestone>(StringComparer.OrdinalIgnoreCase);
                    var includedActivities = new Dictionary<string, RoadmapActivity>(StringComparer.OrdinalIgnoreCase);
                    CollectRoadmapSubtree(sourceRootMilestone, milestonesByParent, activitiesByParent, includedMilestones, includedActivities);

                    var milestoneIdMap = includedMilestones.Keys.ToDictionary(key => key, _ => Guid.NewGuid().ToString(), StringComparer.OrdinalIgnoreCase);
                    var activityIdMap = includedActivities.Keys.ToDictionary(key => key, _ => Guid.NewGuid().ToString(), StringComparer.OrdinalIgnoreCase);
                    var sortedMilestones = includedMilestones.Values
                        .OrderBy(milestone => GetNodeDepth(milestone, sourceMilestoneMap, sourceActivityMap))
                        .ThenBy(milestone => milestone.OrderIndex)
                        .ToList();
                    var sortedActivities = includedActivities.Values
                        .OrderBy(activity => GetNodeDepth(activity, sourceMilestoneMap, sourceActivityMap))
                        .ThenBy(activity => activity.OrderIndex)
                        .ToList();

                    await ShiftProjectSnapshotSiblingOrderAsync(projectNo, anchor.ParentType, anchor.ParentSysId, insertOrder, loggedUser);

                    foreach (var sourceMilestone in sortedMilestones)
                    {
                        var sourceMilestoneKey = NormalizeNodeKey(sourceMilestone.RoadmapMilestoneSysId);
                        var isRootMilestone = string.Equals(sourceMilestoneKey, sourceRootKey, StringComparison.OrdinalIgnoreCase);
                        var clonedMilestoneId = milestoneIdMap[sourceMilestoneKey];

                        var parentType = isRootMilestone ? anchor.ParentType : sourceMilestone.ParentType;
                        var parentSysId = isRootMilestone
                            ? anchor.ParentSysId
                            : ResolveClonedParentSysId(sourceMilestone.ParentType, sourceMilestone.ParentSysId, milestoneIdMap, activityIdMap);
                        var orderIndex = isRootMilestone ? insertOrder : sourceMilestone.OrderIndex;

                        await _projectRepository.AddProjectRoadmapMilestoneAsync(
                            projectNo,
                            clonedMilestoneId,
                            isRootMilestone ? BuildInsertedMilestoneRoadmapSysId() : sourceRoadmap.RoadmapSysId,
                            sourceMilestone.MaturityCode,
                            parentType,
                            parentSysId,
                            sourceMilestone.MilestoneAlias,
                            sourceMilestone.MilestoneDescription,
                            orderIndex,
                            sourceMilestone.IsActive,
                            sourceMilestone.IsRequired,
                            loggedUser,
                            loggedUser);

                        var executionMilestone = new ProjectMilestone
                        {
                            ProjectNo = projectNo,
                            PlantRoadmapLinkSysId = project.PlantRoadmapLinkSysId,
                            RoadmapSysId = project.RoadmapSysId,
                            RoadmapMilestoneSysId = clonedMilestoneId,
                            Status = "NOT STARTED",
                            IsRequired = sourceMilestone.IsRequired,
                            IsActive = sourceMilestone.IsActive,
                            CreatedBy = loggedUser,
                            ModifiedBy = loggedUser
                        };

                        executionMilestone.MilestoneSysId = await _projectmilestoneRepository.AddAsync(executionMilestone);
                        await _projectmilestoneService.InitializeAsync(executionMilestone, "PROJECT MILESTONE TEMPLATE INSERT", notify: false);
                        executionMilestoneMap[NormalizeNodeKey(clonedMilestoneId)] = executionMilestone;

                        if (isRootMilestone)
                        {
                            await _projectownerRepository.AddAsync(new ProjectOwner
                            {
                                ProjectNo = projectNo,
                                UserId = loggedUser,
                                ParentType = "MILESTONE",
                                ParentSysId = executionMilestone.MilestoneSysId
                            });

                            insertedRootMilestoneIds[sourceRootKey] = clonedMilestoneId;
                        }
                    }

                    foreach (var sourceActivity in sortedActivities)
                    {
                        var sourceActivityKey = NormalizeNodeKey(sourceActivity.RoadmapActivitySysId);
                        var clonedActivityId = activityIdMap[sourceActivityKey];
                        var parentSysId = ResolveClonedParentSysId(sourceActivity.ParentType, sourceActivity.ParentSysId, milestoneIdMap, activityIdMap);

                        await _projectRepository.AddProjectRoadmapActivityAsync(
                            projectNo,
                            clonedActivityId,
                            sourceRoadmap.RoadmapSysId,
                            sourceActivity.ParentType,
                            parentSysId,
                            sourceActivity.ActivityName,
                            sourceActivity.ActivityDescription,
                            sourceActivity.EstimatedManDays,
                            sourceActivity.IsRequired,
                            sourceActivity.OrderIndex,
                            sourceActivity.IsActive,
                            loggedUser,
                            loggedUser);

                        var executionParent = ResolveExecutionParent(
                            new RoadmapActivity
                            {
                                ParentType = sourceActivity.ParentType,
                                ParentSysId = parentSysId
                            },
                            executionMilestoneMap,
                            executionTaskMap,
                            null);

                        if (executionParent == null)
                        {
                            throw new Exception($"Unable to resolve the parent for roadmap activity '{sourceActivity.ActivityName}'.");
                        }

                        var executionTask = new ProjectTask
                        {
                            ProjectNo = projectNo,
                            RoadmapActivitySysId = clonedActivityId,
                            PlantRoadmapLinkSysId = project.PlantRoadmapLinkSysId,
                            RoadmapSysId = project.RoadmapSysId,
                            ParentType = executionParent.Item1,
                            ParentSysId = executionParent.Item2,
                            EstimatedMandays = sourceActivity.EstimatedManDays,
                            Status = "NOT STARTED",
                            IsRequired = sourceActivity.IsRequired,
                            OrderIndex = sourceActivity.OrderIndex,
                            IsActive = sourceActivity.IsActive,
                            CreatedBy = loggedUser,
                            ModifiedBy = loggedUser
                        };

                        executionTask.ProjectTaskSysId = await _projecttaskRepository.AddAsync(executionTask);
                        await _projecttaskService.InitializeAsync(executionTask, "PROJECT MILESTONE TEMPLATE INSERT", notify: false);
                        executionTaskMap[NormalizeNodeKey(clonedActivityId)] = executionTask;
                    }

                    foreach (var sourceForm in sourceForms)
                    {
                        var normalizedEntityType = (sourceForm.EntityType ?? string.Empty).Trim().ToLowerInvariant();
                        string clonedEntitySysId = null;

                        if (normalizedEntityType == "milestone")
                        {
                            milestoneIdMap.TryGetValue(NormalizeNodeKey(sourceForm.EntitySysId), out clonedEntitySysId);
                        }
                        else if (normalizedEntityType == "activity")
                        {
                            activityIdMap.TryGetValue(NormalizeNodeKey(sourceForm.EntitySysId), out clonedEntitySysId);
                        }

                        if (string.IsNullOrWhiteSpace(clonedEntitySysId))
                        {
                            continue;
                        }

                        await _formEntityLinkRepository.AddAsync(new FormEntityLink
                        {
                            FormSysId = sourceForm.FormSysId,
                            EntityType = sourceForm.EntityType,
                            EntitySysId = clonedEntitySysId,
                            OrderIndex = sourceForm.OrderIndex,
                            CreatedBy = loggedUser
                        });
                    }

                    foreach (var prereq in sourcePrereqs)
                    {
                        var sourceActivityKey = NormalizeNodeKey(prereq.RoadMapActivitySysId);
                        var prerequisiteKey = NormalizeNodeKey(prereq.PrerequisiteSysId);
                        if (!activityIdMap.ContainsKey(sourceActivityKey) || !activityIdMap.ContainsKey(prerequisiteKey))
                        {
                            continue;
                        }

                        await _projectRepository.AddProjectRoadmapActivityPrerequisiteAsync(
                            projectNo,
                            Guid.NewGuid().ToString(),
                            activityIdMap[sourceActivityKey],
                            activityIdMap[prerequisiteKey]);
                    }
                }

                _dataAccess.CommitTransaction();
                return effectiveRootMilestones
                    .Select(milestone => insertedRootMilestoneIds[NormalizeNodeKey(milestone.RoadmapMilestoneSysId)])
                    .ToList();
            }
            catch
            {
                _dataAccess.RollbackTransaction();
                throw;
            }
        }

        public async Task ReorderAdditionalMilestoneAsync(string projectNo, string roadmapMilestoneSysId, string direction, string loggedUser)
        {
            if (string.IsNullOrWhiteSpace(roadmapMilestoneSysId))
            {
                throw new Exception("Milestone is required.");
            }

            var normalizedDirection = NormalizeMilestoneMoveDirection(direction);
            var (project, snapshotMilestones, snapshotActivities, _) = await LoadProjectMilestoneManagementContextAsync(projectNo);
            var snapshotMilestoneMap = snapshotMilestones.ToDictionary(item => NormalizeNodeKey(item.RoadmapMilestoneSysId), item => item, StringComparer.OrdinalIgnoreCase);
            var snapshotActivityMap = snapshotActivities.ToDictionary(item => NormalizeNodeKey(item.RoadmapActivitySysId), item => item, StringComparer.OrdinalIgnoreCase);

            var milestone = snapshotMilestones.SingleOrDefault(item => string.Equals(item.RoadmapMilestoneSysId, roadmapMilestoneSysId.Trim(), StringComparison.OrdinalIgnoreCase));
            if (milestone == null)
            {
                throw new Exception("The selected milestone no longer exists.");
            }

            if (!CanDeleteInsertedMilestoneRoot(milestone, project.RoadmapSysId, snapshotMilestoneMap, snapshotActivityMap))
            {
                throw new Exception("Only newly inserted milestones can be reordered here.");
            }

            var siblings = snapshotMilestones
                .Where(item => string.Equals(item.ParentType ?? string.Empty, milestone.ParentType ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(item.ParentSysId ?? string.Empty, milestone.ParentSysId ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.OrderIndex)
                .ThenBy(item => item.MilestoneAlias ?? string.Empty)
                .ThenBy(item => item.RoadmapMilestoneSysId)
                .ToList();

            var currentIndex = siblings.FindIndex(item => string.Equals(item.RoadmapMilestoneSysId, milestone.RoadmapMilestoneSysId, StringComparison.OrdinalIgnoreCase));
            if (currentIndex < 0)
            {
                throw new Exception("The selected milestone could not be found in its current sequence.");
            }

            var swapIndex = normalizedDirection == "UP" ? currentIndex - 1 : currentIndex + 1;
            if (swapIndex < 0 || swapIndex >= siblings.Count)
            {
                throw new Exception(normalizedDirection == "UP"
                    ? "The milestone is already the first item in this milestone sequence."
                    : "The milestone is already the last item in this milestone sequence.");
            }

            var swapTarget = siblings[swapIndex];
            var originalOrder = milestone.OrderIndex;
            var targetOrder = swapTarget.OrderIndex;

            _dataAccess.BeginTransaction();
            try
            {
                await UpdateProjectRoadmapMilestoneOrderAsync(projectNo, milestone.RoadmapMilestoneSysId, -999999999, loggedUser);
                await UpdateProjectRoadmapMilestoneOrderAsync(projectNo, swapTarget.RoadmapMilestoneSysId, originalOrder, loggedUser);
                await UpdateProjectRoadmapMilestoneOrderAsync(projectNo, milestone.RoadmapMilestoneSysId, targetOrder, loggedUser);

                _dataAccess.CommitTransaction();
            }
            catch
            {
                _dataAccess.RollbackTransaction();
                throw;
            }
        }

        public async Task DeleteAdditionalMilestoneAsync(string projectNo, string roadmapMilestoneSysId, string loggedUser)
        {
            if (string.IsNullOrWhiteSpace(roadmapMilestoneSysId))
            {
                throw new Exception("Milestone is required.");
            }

            var (project, snapshotMilestones, snapshotActivities, _) = await LoadProjectMilestoneManagementContextAsync(projectNo);
            var snapshotMilestoneMap = snapshotMilestones.ToDictionary(milestone => NormalizeNodeKey(milestone.RoadmapMilestoneSysId), milestone => milestone, StringComparer.OrdinalIgnoreCase);
            var snapshotActivityMap = snapshotActivities.ToDictionary(activity => NormalizeNodeKey(activity.RoadmapActivitySysId), activity => activity, StringComparer.OrdinalIgnoreCase);

            var rootMilestone = snapshotMilestones.SingleOrDefault(item => string.Equals(item.RoadmapMilestoneSysId, roadmapMilestoneSysId.Trim(), StringComparison.OrdinalIgnoreCase));
            if (rootMilestone == null)
            {
                throw new Exception("The selected milestone no longer exists.");
            }

            if (!CanDeleteInsertedMilestoneRoot(rootMilestone, project.RoadmapSysId, snapshotMilestoneMap, snapshotActivityMap))
            {
                throw new Exception("Only newly inserted milestones can be removed here.");
            }

            var milestonesByParent = snapshotMilestones
                .GroupBy(milestone => BuildRoadmapParentKey(milestone.ParentType, milestone.ParentSysId), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);
            var activitiesByParent = snapshotActivities
                .GroupBy(activity => BuildRoadmapParentKey(activity.ParentType, activity.ParentSysId), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

            var includedMilestones = new Dictionary<string, RoadmapMilestone>(StringComparer.OrdinalIgnoreCase);
            var includedActivities = new Dictionary<string, RoadmapActivity>(StringComparer.OrdinalIgnoreCase);
            CollectRoadmapSubtree(rootMilestone, milestonesByParent, activitiesByParent, includedMilestones, includedActivities);

            if (includedMilestones.Values.Any(milestone => string.Equals(milestone.RoadmapSysId ?? string.Empty, project.RoadmapSysId ?? string.Empty, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("This milestone cannot be removed because it includes roadmap-owned descendants.");
            }

            var milestoneNodeIds = new HashSet<string>(includedMilestones.Values.Select(milestone => milestone.RoadmapMilestoneSysId), StringComparer.OrdinalIgnoreCase);
            var activityNodeIds = new HashSet<string>(includedActivities.Values.Select(activity => activity.RoadmapActivitySysId), StringComparer.OrdinalIgnoreCase);

            var executionMilestones = (await _projectmilestoneRepository.GetListAsync(projectNo))
                .Where(milestone => milestone != null && !string.IsNullOrWhiteSpace(milestone.RoadmapMilestoneSysId))
                .ToList();
            var executionTasks = (await _projecttaskRepository.GetListAsync(projectNo)).Where(task => task != null).ToList();

            var deletedExecutionMilestones = executionMilestones
                .Where(milestone => milestoneNodeIds.Contains(milestone.RoadmapMilestoneSysId))
                .ToList();
            var deletedExecutionMilestoneIds = new HashSet<string>(deletedExecutionMilestones.Select(milestone => NormalizeNodeKey(milestone.MilestoneSysId)), StringComparer.OrdinalIgnoreCase);

            var deletedExecutionTasks = executionTasks
                .Where(task => !string.IsNullOrWhiteSpace(task.RoadmapActivitySysId) && activityNodeIds.Contains(task.RoadmapActivitySysId))
                .Concat(executionTasks.Where(task => string.IsNullOrWhiteSpace(task.RoadmapActivitySysId)
                    && string.Equals(task.ParentType, "MILESTONE", StringComparison.OrdinalIgnoreCase)
                    && deletedExecutionMilestoneIds.Contains(NormalizeNodeKey(task.ParentSysId))))
                .GroupBy(task => NormalizeNodeKey(task.ProjectTaskSysId), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            var formLinks = await LoadFormEntityLinksAsync(milestoneNodeIds, activityNodeIds);

            _dataAccess.BeginTransaction();
            try
            {
                foreach (var task in deletedExecutionTasks)
                {
                    await DeleteProjectTaskExecutionArtifactsAsync(projectNo, task);
                }

                foreach (var milestone in deletedExecutionMilestones)
                {
                    await DeleteProjectMilestoneExecutionArtifactsAsync(projectNo, milestone);
                }

                foreach (var formLink in formLinks)
                {
                    await DeleteProjectFormLinkArtifactsAsync(projectNo, formLink);
                }

                foreach (var activity in includedActivities.Values
                    .OrderByDescending(item => GetNodeDepth(item, snapshotMilestoneMap, snapshotActivityMap))
                    .ThenByDescending(item => item.OrderIndex))
                {
                    await _projectRepository.DeleteProjectRoadmapActivityPrerequisitesForActivityAsync(projectNo, activity.RoadmapActivitySysId);
                    await _projectRepository.DeleteProjectRoadmapActivityAsync(projectNo, activity.RoadmapActivitySysId);
                }

                foreach (var milestone in includedMilestones.Values
                    .OrderByDescending(item => GetNodeDepth(item, snapshotMilestoneMap, snapshotActivityMap))
                    .ThenByDescending(item => item.OrderIndex))
                {
                                        await _projectRepository.DeleteProjectRoadmapMilestoneAsync(projectNo, milestone.RoadmapMilestoneSysId);
                }

                await CloseProjectMilestoneSiblingGapAsync(projectNo, rootMilestone.ParentType, rootMilestone.ParentSysId, rootMilestone.OrderIndex, loggedUser);

                _dataAccess.CommitTransaction();
            }
            catch
            {
                _dataAccess.RollbackTransaction();
                throw;
            }
        }

        private static string NormalizeInsertPosition(string insertPosition)
        {
            var normalized = (insertPosition ?? string.Empty).Trim().ToUpperInvariant();
            if (normalized == "BEFORE" || normalized == "AFTER")
            {
                return normalized;
            }

            throw new Exception("Insert position must be either BEFORE or AFTER.");
        }

        public async Task<ProjectRoadmapRefreshPreview> PreviewRoadmapRefreshAsync(string projectNo)
        {
            var project = await _projectRepository.GetAsync(projectNo);
            if (project == null)
            {
                throw new Exception("Project does not exist.");
            }

            var roadmap = await _roadmapRepository.GetAsync(project.RoadmapSysId);
            if (roadmap == null)
            {
                throw new Exception("The linked roadmap no longer exists.");
            }

            var sourceMilestones = (await _roadmapmilestoneRepository.GetListAsync(project.RoadmapSysId)).ToList();
            var sourceActivities = (await _roadmapactivityRepository.GetListAsync(project.RoadmapSysId)).ToList();
            var sourcePrereqs = (await _roadmapactivityprerequisiteRepository.GetListAsync(project.RoadmapSysId)).ToList();

                        var snapshotMilestones = await _projectRepository.GetProjectRoadmapMilestonesAsync(projectNo);
                        var snapshotActivities = await _projectRepository.GetProjectRoadmapActivitiesAsync(projectNo);
                        var snapshotPrereqs = await _projectRepository.GetProjectRoadmapActivityPrerequisitesAsync(projectNo);

            var executionMilestones = (await _projectmilestoneRepository.GetListAsync(projectNo)).ToList();
            var executionTasks = (await _projecttaskRepository.GetListAsync(projectNo)).ToList();

            var snapshotMilestoneMap = snapshotMilestones.ToDictionary(m => NormalizeNodeKey(m.RoadmapMilestoneSysId), StringComparer.OrdinalIgnoreCase);
            var snapshotActivityMap = snapshotActivities.ToDictionary(a => NormalizeNodeKey(a.RoadmapActivitySysId), StringComparer.OrdinalIgnoreCase);
            var snapshotMilestoneCompositeMap = snapshotMilestones
                .ToDictionary(m => BuildMilestoneSnapshotKey(m.RoadmapMilestoneSysId, m.RoadmapSysId), StringComparer.OrdinalIgnoreCase);
            var snapshotActivityCompositeMap = snapshotActivities
                .ToDictionary(a => BuildActivitySnapshotKey(a.RoadmapActivitySysId, a.RoadmapSysId), StringComparer.OrdinalIgnoreCase);
            var executionMilestoneMap = executionMilestones
                .Where(m => !string.IsNullOrWhiteSpace(m.RoadmapMilestoneSysId))
                .ToDictionary(m => NormalizeNodeKey(m.RoadmapMilestoneSysId), StringComparer.OrdinalIgnoreCase);
            var executionTaskMap = executionTasks
                .Where(t => !string.IsNullOrWhiteSpace(t.RoadmapActivitySysId))
                .ToDictionary(t => NormalizeNodeKey(t.RoadmapActivitySysId), StringComparer.OrdinalIgnoreCase);

            var preview = new ProjectRoadmapRefreshPreview
            {
                ProjectNo = projectNo,
                RoadmapSysId = project.RoadmapSysId,
                RoadmapName = roadmap.RoadmapName
            };

            foreach (var milestone in sourceMilestones)
            {
                var nodeKey = NormalizeNodeKey(milestone.RoadmapMilestoneSysId);
                var snapshotMilestoneKey = BuildMilestoneSnapshotKey(milestone.RoadmapMilestoneSysId, milestone.RoadmapSysId);
                snapshotMilestoneCompositeMap.TryGetValue(snapshotMilestoneKey, out var snapshotMilestone);
                executionMilestoneMap.TryGetValue(nodeKey, out var executionMilestone);

                var state = GetMilestoneRefreshState(snapshotMilestone, milestone);
                var isSelectable = !string.Equals(state, "NOCHANGE", StringComparison.OrdinalIgnoreCase);

                preview.Items.Add(new ProjectRoadmapRefreshItem
                {
                    ChangeKey = $"milestone:{state}:{snapshotMilestoneKey}",
                    ChangeType = state,
                    ItemType = "milestone",
                    NodeId = nodeKey,
                    ParentType = milestone.ParentType,
                    ParentNodeId = NormalizeNodeKey(milestone.ParentSysId),
                    Title = milestone.MilestoneAlias,
                    Path = BuildMilestonePath(milestone, sourceMilestones, sourceActivities),
                    Summary = string.Equals(state, "NEW", StringComparison.OrdinalIgnoreCase)
                        ? "Insert this milestone into PROJECTROADMAPMILESTONES."
                        : string.Equals(state, "UPDATE", StringComparison.OrdinalIgnoreCase)
                            ? "Update this milestone in PROJECTROADMAPMILESTONES."
                            : "No action required for this milestone.",
                    HasSnapshotRow = snapshotMilestone != null,
                    HasExecutionRow = executionMilestone != null,
                    SelectedByDefault = isSelectable,
                    IsSelectable = isSelectable
                });
            }

            foreach (var activity in sourceActivities)
            {
                var nodeKey = NormalizeNodeKey(activity.RoadmapActivitySysId);
                var snapshotActivityKey = BuildActivitySnapshotKey(activity.RoadmapActivitySysId, activity.RoadmapSysId);
                snapshotActivityCompositeMap.TryGetValue(snapshotActivityKey, out var snapshotActivity);
                executionTaskMap.TryGetValue(nodeKey, out var executionTask);

                var state = GetActivityRefreshState(snapshotActivity, activity);
                var isSelectable = !string.Equals(state, "NOCHANGE", StringComparison.OrdinalIgnoreCase);

                preview.Items.Add(new ProjectRoadmapRefreshItem
                {
                    ChangeKey = $"activity:{state}:{snapshotActivityKey}",
                    ChangeType = state,
                    ItemType = "activity",
                    NodeId = nodeKey,
                    ParentType = activity.ParentType,
                    ParentNodeId = NormalizeNodeKey(activity.ParentSysId),
                    Title = activity.ActivityName,
                    Path = BuildActivityPath(activity, sourceMilestones, sourceActivities),
                    Summary = string.Equals(state, "NEW", StringComparison.OrdinalIgnoreCase)
                        ? "Insert this activity into PROJECTROADMAPACTIVITIES."
                        : string.Equals(state, "UPDATE", StringComparison.OrdinalIgnoreCase)
                            ? "Update this activity in PROJECTROADMAPACTIVITIES."
                            : "No action required for this activity.",
                    HasSnapshotRow = snapshotActivity != null,
                    HasExecutionRow = executionTask != null,
                    SelectedByDefault = isSelectable,
                    IsSelectable = isSelectable
                });
            }

            var sourceActivityNameMap = sourceActivities
                .Where(activity => activity != null && !string.IsNullOrWhiteSpace(activity.RoadmapActivitySysId))
                .ToDictionary(activity => NormalizeNodeKey(activity.RoadmapActivitySysId), activity => activity.ActivityName ?? string.Empty, StringComparer.OrdinalIgnoreCase);
            var snapshotPrereqKeys = new HashSet<string>(
                snapshotPrereqs.Select(p => BuildProjectPrereqSnapshotKey(p.RoadmapActivityPrereqSysId, p.RoadMapActivitySysId, p.PrerequisiteSysId)),
                StringComparer.OrdinalIgnoreCase);

            foreach (var prereq in sourcePrereqs)
            {
                var prereqKey = BuildProjectPrereqSnapshotKey(prereq.RoadmapActivityPrereqSysId, prereq.RoadMapActivitySysId, prereq.PrerequisiteSysId);
                var isNew = !snapshotPrereqKeys.Contains(prereqKey);
                var state = isNew ? "NEW" : "NOCHANGE";
                var isSelectable = isNew;

                sourceActivityNameMap.TryGetValue(NormalizeNodeKey(prereq.RoadMapActivitySysId), out var activityName);
                sourceActivityNameMap.TryGetValue(NormalizeNodeKey(prereq.PrerequisiteSysId), out var prerequisiteName);

                preview.Items.Add(new ProjectRoadmapRefreshItem
                {
                    ChangeKey = $"prereq:{state}:{prereqKey}",
                    ChangeType = state,
                    ItemType = "prereq",
                    NodeId = NormalizeNodeKey(prereq.RoadmapActivityPrereqSysId),
                    ParentType = "activity",
                    ParentNodeId = NormalizeNodeKey(prereq.RoadMapActivitySysId),
                    Title = $"{activityName ?? string.Empty} -> {prerequisiteName ?? string.Empty}",
                    Path = BuildPrereqPath(prereq, sourceMilestones, sourceActivities),
                    Summary = isNew
                        ? "Insert this dependency link into PROJECTROADMAPACTIVITYPREREQS."
                        : "No action required for this dependency link.",
                    HasSnapshotRow = !isNew,
                    HasExecutionRow = true,
                    SelectedByDefault = isSelectable,
                    IsSelectable = isSelectable
                });
            }

            preview.DependencyLinkCount = preview.Items.Count(i => i.ItemType == "prereq" && string.Equals(i.ChangeType, "NEW", StringComparison.OrdinalIgnoreCase));
            preview.AddedCount = preview.Items.Count(i => (i.ItemType == "milestone" || i.ItemType == "activity") && string.Equals(i.ChangeType, "NEW", StringComparison.OrdinalIgnoreCase));
            preview.UpdatedCount = preview.Items.Count(i => (i.ItemType == "milestone" || i.ItemType == "activity") && string.Equals(i.ChangeType, "UPDATE", StringComparison.OrdinalIgnoreCase));

            preview.Items = preview.Items
                .OrderBy(i => i.ItemType == "milestone" ? 0 : i.ItemType == "activity" ? 1 : 2)
                .ThenBy(i => i.Path)
                .ToList();

            return preview;
        }

        public async Task<ProjectRoadmapRefreshApplyResult> ApplyRoadmapRefreshAsync(ProjectRoadmapRefreshSelection selection, string loggedUser)
        {
            if (selection == null || string.IsNullOrWhiteSpace(selection.ProjectNo))
            {
                throw new Exception("Project selection is required.");
            }

            var preview = await PreviewRoadmapRefreshAsync(selection.ProjectNo);
            var selectedKeys = new HashSet<string>((selection.SelectedChangeKeys ?? new List<string>())
                .Where(k => !string.IsNullOrWhiteSpace(k)), StringComparer.OrdinalIgnoreCase);

            if (selectedKeys.Count == 0)
            {
                throw new Exception("Select at least one roadmap item to apply.");
            }

            var selectedItems = preview.Items
                .Where(i => selectedKeys.Contains(i.ChangeKey) && !string.Equals(i.ChangeType, "NOCHANGE", StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (!selectedItems.Any())
            {
                throw new Exception("The selected roadmap changes are no longer available. Refresh the preview and try again.");
            }

            var project = await _projectRepository.GetAsync(selection.ProjectNo);
            if (project == null)
            {
                throw new Exception("Project does not exist.");
            }

            var roadmap = await _roadmapRepository.GetAsync(project.RoadmapSysId);
            var sourceMilestones = (await _roadmapmilestoneRepository.GetListAsync(project.RoadmapSysId)).ToList();
            var sourceActivities = (await _roadmapactivityRepository.GetListAsync(project.RoadmapSysId)).ToList();
            var sourcePrereqs = (await _roadmapactivityprerequisiteRepository.GetListAsync(project.RoadmapSysId)).ToList();

            var sourceMilestoneMap = sourceMilestones.ToDictionary(m => NormalizeNodeKey(m.RoadmapMilestoneSysId), StringComparer.OrdinalIgnoreCase);
            var sourceActivityMap = sourceActivities.ToDictionary(a => NormalizeNodeKey(a.RoadmapActivitySysId), StringComparer.OrdinalIgnoreCase);

                        var snapshotRoadmap = await _projectRepository.GetProjectRoadmapAsync(selection.ProjectNo);
                        var snapshotMilestones = await _projectRepository.GetProjectRoadmapMilestonesAsync(selection.ProjectNo);
                        var snapshotActivities = await _projectRepository.GetProjectRoadmapActivitiesAsync(selection.ProjectNo);
                        var snapshotPrereqs = await _projectRepository.GetProjectRoadmapActivityPrerequisitesAsync(selection.ProjectNo);

            var executionMilestones = (await _projectmilestoneRepository.GetListAsync(selection.ProjectNo)).ToList();
            var executionTasks = (await _projecttaskRepository.GetListAsync(selection.ProjectNo)).ToList();

            var snapshotMilestoneMap = snapshotMilestones.ToDictionary(m => NormalizeNodeKey(m.RoadmapMilestoneSysId), StringComparer.OrdinalIgnoreCase);
            var snapshotActivityMap = snapshotActivities.ToDictionary(a => NormalizeNodeKey(a.RoadmapActivitySysId), StringComparer.OrdinalIgnoreCase);
            var snapshotMilestoneCompositeKeys = new HashSet<string>(snapshotMilestones.Select(m => BuildMilestoneSnapshotKey(m.RoadmapMilestoneSysId, m.RoadmapSysId)), StringComparer.OrdinalIgnoreCase);
            var snapshotActivityCompositeKeys = new HashSet<string>(snapshotActivities.Select(a => BuildActivitySnapshotKey(a.RoadmapActivitySysId, a.RoadmapSysId)), StringComparer.OrdinalIgnoreCase);
            var executionMilestoneMap = executionMilestones.ToDictionary(m => NormalizeNodeKey(m.RoadmapMilestoneSysId), StringComparer.OrdinalIgnoreCase);
            var executionTaskMap = executionTasks.ToDictionary(t => NormalizeNodeKey(t.RoadmapActivitySysId), StringComparer.OrdinalIgnoreCase);

            var milestoneAddIds = new HashSet<string>(selectedItems.Where(i => i.ItemType == "milestone" && string.Equals(i.ChangeType, "NEW", StringComparison.OrdinalIgnoreCase)).Select(i => NormalizeNodeKey(i.NodeId)), StringComparer.OrdinalIgnoreCase);
            var activityAddIds = new HashSet<string>(selectedItems.Where(i => i.ItemType == "activity" && string.Equals(i.ChangeType, "NEW", StringComparison.OrdinalIgnoreCase)).Select(i => NormalizeNodeKey(i.NodeId)), StringComparer.OrdinalIgnoreCase);
            var milestoneUpdateIds = new HashSet<string>(selectedItems.Where(i => i.ItemType == "milestone" && string.Equals(i.ChangeType, "UPDATE", StringComparison.OrdinalIgnoreCase)).Select(i => NormalizeNodeKey(i.NodeId)), StringComparer.OrdinalIgnoreCase);
            var activityUpdateIds = new HashSet<string>(selectedItems.Where(i => i.ItemType == "activity" && string.Equals(i.ChangeType, "UPDATE", StringComparison.OrdinalIgnoreCase)).Select(i => NormalizeNodeKey(i.NodeId)), StringComparer.OrdinalIgnoreCase);
            var prereqAddKeys = new HashSet<string>(selectedItems
                .Where(i => i.ItemType == "prereq" && string.Equals(i.ChangeType, "NEW", StringComparison.OrdinalIgnoreCase))
                .Select(i => ExtractChangeKeyPayload(i.ChangeKey)), StringComparer.OrdinalIgnoreCase);

            var autoIncludedParents = ExpandAncestorSelection(milestoneAddIds, activityAddIds, sourceMilestoneMap, sourceActivityMap, snapshotMilestoneMap, snapshotActivityMap, executionMilestoneMap, executionTaskMap);

            var result = new ProjectRoadmapRefreshApplyResult
            {
                AutoIncludedParents = autoIncludedParents
            };

            _dataAccess.BeginTransaction();

            try
            {
                if (snapshotRoadmap == null)
                {
                                        await _projectRepository.AddProjectRoadmapFromMasterAsync(selection.ProjectNo, project.RoadmapSysId, loggedUser, loggedUser);
                }

                foreach (var milestoneId in milestoneUpdateIds)
                {
                    if (!sourceMilestoneMap.TryGetValue(milestoneId, out var sourceMilestone)
                        || !snapshotMilestoneMap.TryGetValue(milestoneId, out var snapshotMilestone)
                        || !snapshotMilestoneCompositeKeys.Contains(BuildMilestoneSnapshotKey(sourceMilestone.RoadmapMilestoneSysId, sourceMilestone.RoadmapSysId)))
                    {
                        continue;
                    }

                    await _projectRepository.UpdateProjectRoadmapMilestoneAsync(selection.ProjectNo, sourceMilestone, loggedUser);

                    result.UpdatedMilestones++;
                }

                foreach (var activityId in activityUpdateIds)
                {
                    if (!sourceActivityMap.TryGetValue(activityId, out var sourceActivity)
                        || !snapshotActivityMap.TryGetValue(activityId, out var snapshotActivity)
                        || !snapshotActivityCompositeKeys.Contains(BuildActivitySnapshotKey(sourceActivity.RoadmapActivitySysId, sourceActivity.RoadmapSysId)))
                    {
                        continue;
                    }

                    await _projectRepository.UpdateProjectRoadmapActivityAsync(selection.ProjectNo, sourceActivity, loggedUser);

                    result.UpdatedActivities++;
                }

                var sortedMilestones = milestoneAddIds
                    .Select(id => sourceMilestoneMap[id])
                    .OrderBy(m => GetNodeDepth(m, sourceMilestoneMap, sourceActivityMap))
                    .ToList();

                var sortedActivities = activityAddIds
                    .Select(id => sourceActivityMap[id])
                    .OrderBy(a => GetNodeDepth(a, sourceMilestoneMap, sourceActivityMap))
                    .ToList();

                foreach (var milestone in sortedMilestones)
                {
                    var nodeKey = NormalizeNodeKey(milestone.RoadmapMilestoneSysId);
                    var milestoneCompositeKey = BuildMilestoneSnapshotKey(milestone.RoadmapMilestoneSysId, milestone.RoadmapSysId);
                    if (!snapshotMilestoneCompositeKeys.Contains(milestoneCompositeKey))
                    {
                        await _projectRepository.AddProjectRoadmapMilestoneAsync(
                            selection.ProjectNo,
                            milestone.RoadmapMilestoneSysId,
                            milestone.RoadmapSysId,
                            milestone.MaturityCode,
                            milestone.ParentType,
                            milestone.ParentSysId,
                            milestone.MilestoneAlias,
                            milestone.MilestoneDescription,
                            milestone.OrderIndex,
                            milestone.IsActive,
                            milestone.IsRequired,
                            loggedUser,
                            loggedUser);

                        snapshotMilestoneMap[nodeKey] = milestone;
                        snapshotMilestoneCompositeKeys.Add(milestoneCompositeKey);
                    }

                    if (!executionMilestoneMap.ContainsKey(nodeKey))
                    {
                        var executionMilestone = new ProjectMilestone
                        {
                            ProjectNo = selection.ProjectNo,
                            PlantRoadmapLinkSysId = project.PlantRoadmapLinkSysId,
                            RoadmapSysId = project.RoadmapSysId,
                            RoadmapMilestoneSysId = milestone.RoadmapMilestoneSysId,
                            Status = "NOT STARTED",
                            IsRequired = milestone.IsRequired,
                            IsActive = milestone.IsActive,
                            CreatedBy = loggedUser,
                            ModifiedBy = loggedUser
                        };

                        executionMilestone.MilestoneSysId = await _projectmilestoneRepository.AddAsync(executionMilestone);
                        await _projectmilestoneService.InitializeAsync(executionMilestone, "ROADMAP REFRESH", notify: false);
                        executionMilestoneMap[nodeKey] = executionMilestone;
                        result.AddedMilestones++;
                    }
                }

                ProjectMilestone rootExecutionMilestone = executionMilestones.FirstOrDefault(m => string.IsNullOrWhiteSpace(m.RoadmapMilestoneSysId));
                var requiresRootContainer = sortedActivities.Any(a => IsRoadmapRoot(a.ParentType));
                if (requiresRootContainer && rootExecutionMilestone == null)
                {
                    rootExecutionMilestone = new ProjectMilestone
                    {
                        ProjectNo = selection.ProjectNo,
                        PlantRoadmapLinkSysId = project.PlantRoadmapLinkSysId,
                        RoadmapSysId = project.RoadmapSysId,
                        RoadmapMilestoneSysId = null,
                        Status = "NOT STARTED",
                        IsRequired = 1,
                        IsActive = 1,
                        CreatedBy = loggedUser,
                        ModifiedBy = loggedUser
                    };

                    rootExecutionMilestone.MilestoneSysId = await _projectmilestoneRepository.AddAsync(rootExecutionMilestone);
                    await _projectmilestoneService.InitializeAsync(rootExecutionMilestone, "ROADMAP REFRESH", notify: false);
                    executionMilestoneMap[NormalizeNodeKey(null)] = rootExecutionMilestone;
                    result.AddedMilestones++;
                }

                foreach (var activity in sortedActivities)
                {
                    var nodeKey = NormalizeNodeKey(activity.RoadmapActivitySysId);
                    var activityCompositeKey = BuildActivitySnapshotKey(activity.RoadmapActivitySysId, activity.RoadmapSysId);
                    if (!snapshotActivityCompositeKeys.Contains(activityCompositeKey))
                    {
                        await _projectRepository.AddProjectRoadmapActivityAsync(
                            selection.ProjectNo,
                            activity.RoadmapActivitySysId,
                            activity.RoadmapSysId,
                            activity.ParentType,
                            activity.ParentSysId,
                            activity.ActivityName,
                            activity.ActivityDescription,
                            activity.EstimatedManDays,
                            activity.IsRequired,
                            activity.OrderIndex,
                            activity.IsActive,
                            loggedUser,
                            loggedUser);

                        snapshotActivityMap[nodeKey] = activity;
                        snapshotActivityCompositeKeys.Add(activityCompositeKey);
                    }

                    if (!executionTaskMap.ContainsKey(nodeKey))
                    {
                        var executionParent = ResolveExecutionParent(activity, executionMilestoneMap, executionTaskMap, rootExecutionMilestone);
                        if (executionParent == null)
                        {
                            throw new Exception($"Unable to resolve the parent for roadmap activity '{activity.ActivityName}'.");
                        }

                        var executionTask = new ProjectTask
                        {
                            ProjectNo = selection.ProjectNo,
                            RoadmapActivitySysId = activity.RoadmapActivitySysId,
                            PlantRoadmapLinkSysId = project.PlantRoadmapLinkSysId,
                            RoadmapSysId = project.RoadmapSysId,
                            ParentType = executionParent.Item1,
                            ParentSysId = executionParent.Item2,
                            EstimatedMandays = activity.EstimatedManDays,
                            Status = "NOT STARTED",
                            IsRequired = activity.IsRequired,
                            OrderIndex = activity.OrderIndex,
                            IsActive = activity.IsActive,
                            CreatedBy = loggedUser,
                            ModifiedBy = loggedUser
                        };

                        executionTask.ProjectTaskSysId = await _projecttaskRepository.AddAsync(executionTask);
                        await _projecttaskService.InitializeAsync(executionTask, "ROADMAP REFRESH", notify: false);
                        executionTaskMap[nodeKey] = executionTask;
                        result.AddedActivities++;
                    }
                }

                var finalActivityIds = new HashSet<string>(snapshotActivityMap.Keys, StringComparer.OrdinalIgnoreCase);
                var snapshotPrereqKeys = new HashSet<string>(snapshotPrereqs.Select(p => BuildProjectPrereqSnapshotKey(p.RoadmapActivityPrereqSysId, p.RoadMapActivitySysId, p.PrerequisiteSysId)), StringComparer.OrdinalIgnoreCase);
                var sourcePrereqMap = sourcePrereqs.ToDictionary(prereq => BuildProjectPrereqSnapshotKey(prereq.RoadmapActivityPrereqSysId, prereq.RoadMapActivitySysId, prereq.PrerequisiteSysId), prereq => prereq, StringComparer.OrdinalIgnoreCase);

                foreach (var prereqKey in prereqAddKeys)
                {
                    if (!sourcePrereqMap.TryGetValue(prereqKey, out var prereq))
                    {
                        continue;
                    }

                    var activityKey = NormalizeNodeKey(prereq.RoadMapActivitySysId);
                    var dependencyKey = NormalizeNodeKey(prereq.PrerequisiteSysId);

                    if (!finalActivityIds.Contains(activityKey)
                        || !finalActivityIds.Contains(dependencyKey)
                        || snapshotPrereqKeys.Contains(prereqKey))
                    {
                        continue;
                    }

                    await _projectRepository.AddProjectRoadmapActivityPrerequisiteAsync(
                        selection.ProjectNo,
                        string.IsNullOrWhiteSpace(prereq.RoadmapActivityPrereqSysId) ? Guid.NewGuid().ToString() : prereq.RoadmapActivityPrereqSysId,
                        prereq.RoadMapActivitySysId,
                        prereq.PrerequisiteSysId);

                    snapshotPrereqKeys.Add(prereqKey);
                    result.AddedDependencyLinks++;
                }

                _dataAccess.CommitTransaction();
                return result;
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }
        #endregion

        private async Task ShiftProjectSnapshotSiblingOrderAsync(string projectNo, string parentType, string parentSysId, int insertOrder, string loggedUser)
        {
            await _projectRepository.ShiftProjectRoadmapSiblingOrderAsync(projectNo, parentType, parentSysId, insertOrder, loggedUser);
        }

        private async Task<(Project Project, List<RoadmapMilestone> SnapshotMilestones, List<RoadmapActivity> SnapshotActivities, HashSet<string> MasterMilestoneIds)> LoadProjectMilestoneManagementContextAsync(string projectNo)
        {
            if (string.IsNullOrWhiteSpace(projectNo))
            {
                throw new Exception("Project number is required.");
            }

            var project = await _projectRepository.GetAsync(projectNo);
            if (project == null)
            {
                throw new Exception("Project does not exist.");
            }

            var snapshotMilestones = await _projectRepository.GetProjectRoadmapMilestonesAsync(projectNo);
            var snapshotActivities = await _projectRepository.GetProjectRoadmapActivitiesAsync(projectNo);

            var masterMilestoneIds = new HashSet<string>(
                (await _roadmapmilestoneRepository.GetListAsync(project.RoadmapSysId))
                    .Where(milestone => milestone != null && !string.IsNullOrWhiteSpace(milestone.RoadmapMilestoneSysId))
                    .Select(milestone => NormalizeNodeKey(milestone.RoadmapMilestoneSysId)),
                StringComparer.OrdinalIgnoreCase);

            return (project, snapshotMilestones, snapshotActivities, masterMilestoneIds);
        }

        private async Task UpdateProjectRoadmapMilestoneOrderAsync(string projectNo, string roadmapMilestoneSysId, int orderIndex, string loggedUser)
        {
            await _projectRepository.UpdateProjectRoadmapMilestoneOrderAsync(projectNo, roadmapMilestoneSysId, orderIndex, loggedUser);
        }

        private async Task CloseProjectMilestoneSiblingGapAsync(string projectNo, string parentType, string parentSysId, int deletedOrderIndex, string loggedUser)
        {
            await _projectRepository.CloseProjectRoadmapMilestoneSiblingGapAsync(projectNo, parentType, parentSysId, deletedOrderIndex, loggedUser);
        }

        private async Task<List<FormEntityLink>> LoadFormEntityLinksAsync(IEnumerable<string> milestoneNodeIds, IEnumerable<string> activityNodeIds)
        {
            var formLinks = new List<FormEntityLink>();

            foreach (var milestoneNodeId in (milestoneNodeIds ?? Enumerable.Empty<string>()).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                                formLinks.AddRange(await _projectRepository.GetFormEntityLinksByEntityAsync("milestone", milestoneNodeId));
            }

            foreach (var activityNodeId in (activityNodeIds ?? Enumerable.Empty<string>()).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                                formLinks.AddRange(await _projectRepository.GetFormEntityLinksByEntityAsync("activity", activityNodeId));
            }

            return formLinks
                .GroupBy(link => NormalizeNodeKey(link.FormEntityLinkSysId), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
        }

        private async Task DeleteProjectFormLinkArtifactsAsync(string projectNo, FormEntityLink formLink)
        {
            if (formLink == null || string.IsNullOrWhiteSpace(formLink.FormEntityLinkSysId))
            {
                return;
            }

            await _projectRepository.DeleteProjectFormSubmissionValuesByFormEntityLinkAsync(projectNo, formLink.FormEntityLinkSysId);
            await _projectRepository.DeleteProjectFormSubmissionsByFormEntityLinkAsync(projectNo, formLink.FormEntityLinkSysId);

            await _formEntityLinkRepository.DeleteAsync(formLink.FormEntityLinkSysId);
        }

        private async Task DeleteProjectMilestoneExecutionArtifactsAsync(string projectNo, ProjectMilestone milestone)
        {
            if (milestone == null || string.IsNullOrWhiteSpace(milestone.MilestoneSysId))
            {
                return;
            }

            await DeleteProjectNodeEntityArtifactsAsync(projectNo, milestone.MilestoneSysId, "milestone");

            await _projectRepository.DeleteProjectFieldsByMilestoneAsync(projectNo, milestone.MilestoneSysId);
            await _projectRepository.DeleteProjectStatusChangesByEntityAsync(projectNo, "MILESTONE", milestone.MilestoneSysId);
            await _projectRepository.DeleteProjectTargetRevisionsForMilestoneAsync(projectNo, milestone.MilestoneSysId, milestone.RoadmapMilestoneSysId);

            await _projectmilestoneRepository.DeleteAsync(milestone.MilestoneSysId);
        }

        private async Task DeleteProjectTaskExecutionArtifactsAsync(string projectNo, ProjectTask task)
        {
            if (task == null || string.IsNullOrWhiteSpace(task.ProjectTaskSysId))
            {
                return;
            }

            await DeleteProjectNodeEntityArtifactsAsync(projectNo, task.ProjectTaskSysId, "task", "activity");

            await _projectRepository.DeleteProjectFieldsByTaskAsync(projectNo, task.ProjectTaskSysId);
            await _projectRepository.DeleteProjectStatusChangesByEntityAsync(projectNo, "TASK", task.ProjectTaskSysId);
            await _projectRepository.DeleteProjectTargetRevisionsForTaskAsync(projectNo, task.ProjectTaskSysId, task.RoadmapActivitySysId);

            await _projecttaskRepository.DeleteAsync(task.ProjectTaskSysId);
        }

        private async Task DeleteProjectNodeEntityArtifactsAsync(string projectNo, string entitySysId, params string[] entityTypes)
        {
            foreach (var entityType in (entityTypes ?? Array.Empty<string>()).Where(type => !string.IsNullOrWhiteSpace(type)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                await _projectRepository.DeleteProjectOwnersByEntityAsync(projectNo, entityType, entitySysId);
                await _projectRepository.DeleteProjectCommentsByEntityAsync(projectNo, entityType, entitySysId);
                await _projectRepository.DeleteProjectAttachmentsByEntityAsync(projectNo, entityType, entitySysId);
                await _projectRepository.DeleteNotificationViewedByEntityAsync(entityType, entitySysId);
                await _projectRepository.DeleteNotificationsByEntityAsync(entityType, entitySysId);
            }
        }

        private static string NormalizeNodeKey(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "__ROOT_ACTIVITIES__" : value.Trim();
        }

        private static string BuildInsertedMilestoneRoadmapSysId()
        {
            return $"INS-{Guid.NewGuid():N}";
        }

        private static bool IsInsertedMilestoneRoadmapSysId(string roadmapSysId)
        {
            return !string.IsNullOrWhiteSpace(roadmapSysId)
                && roadmapSysId.StartsWith("INS-", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasSelectedMilestoneAncestor(
            RoadmapMilestone milestone,
            ISet<string> selectedMilestoneKeys,
            IDictionary<string, RoadmapMilestone> milestones,
            IDictionary<string, RoadmapActivity> activities)
        {
            if (milestone == null || selectedMilestoneKeys == null || selectedMilestoneKeys.Count == 0)
            {
                return false;
            }

            var parentType = milestone.ParentType;
            var parentSysId = milestone.ParentSysId;

            while (!string.IsNullOrWhiteSpace(parentType))
            {
                if (string.Equals(parentType, "milestone", StringComparison.OrdinalIgnoreCase))
                {
                    var milestoneKey = NormalizeNodeKey(parentSysId);
                    if (selectedMilestoneKeys.Contains(milestoneKey))
                    {
                        return true;
                    }

                    if (milestones == null || !milestones.TryGetValue(milestoneKey, out var parentMilestone))
                    {
                        return false;
                    }

                    parentType = parentMilestone.ParentType;
                    parentSysId = parentMilestone.ParentSysId;
                    continue;
                }

                if (string.Equals(parentType, "activity", StringComparison.OrdinalIgnoreCase))
                {
                    var activityKey = NormalizeNodeKey(parentSysId);
                    if (activities == null || !activities.TryGetValue(activityKey, out var parentActivity))
                    {
                        return false;
                    }

                    parentType = parentActivity.ParentType;
                    parentSysId = parentActivity.ParentSysId;
                    continue;
                }

                return false;
            }

            return false;
        }

        private static bool CanDeleteInsertedMilestoneRoot(
            RoadmapMilestone milestone,
            string projectRoadmapSysId,
            IDictionary<string, RoadmapMilestone> milestones,
            IDictionary<string, RoadmapActivity> activities)
        {
            if (milestone == null || string.IsNullOrWhiteSpace(milestone.RoadmapMilestoneSysId))
            {
                return false;
            }

            if (IsInsertedMilestoneRoadmapSysId(milestone.RoadmapSysId))
            {
                return true;
            }

            if (string.Equals(milestone.RoadmapSysId ?? string.Empty, projectRoadmapSysId ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (string.Equals(milestone.ParentType, "milestone", StringComparison.OrdinalIgnoreCase)
                && milestones != null
                && milestones.TryGetValue(NormalizeNodeKey(milestone.ParentSysId), out var parentMilestone))
            {
                return !string.Equals(parentMilestone.RoadmapSysId ?? string.Empty, milestone.RoadmapSysId ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            }

            if (string.Equals(milestone.ParentType, "activity", StringComparison.OrdinalIgnoreCase)
                && activities != null
                && activities.TryGetValue(NormalizeNodeKey(milestone.ParentSysId), out var parentActivity))
            {
                return !string.Equals(parentActivity.RoadmapSysId ?? string.Empty, milestone.RoadmapSysId ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        private static string NormalizeMilestoneMoveDirection(string direction)
        {
            var normalized = (direction ?? string.Empty).Trim().ToUpperInvariant();
            if (normalized == "UP" || normalized == "DOWN")
            {
                return normalized;
            }

            throw new Exception("Move direction must be either UP or DOWN.");
        }

        private static string BuildRoadmapParentKey(string parentType, string parentSysId)
        {
            return $"{(parentType ?? string.Empty).Trim().ToUpperInvariant()}::{NormalizeNodeKey(parentSysId)}";
        }

        private static bool IsRoadmapRoot(string parentType)
        {
            return string.Equals(parentType, "roadmap", StringComparison.OrdinalIgnoreCase);
        }

        private static void CollectRoadmapSubtree(
            RoadmapMilestone rootMilestone,
            IDictionary<string, List<RoadmapMilestone>> milestonesByParent,
            IDictionary<string, List<RoadmapActivity>> activitiesByParent,
            IDictionary<string, RoadmapMilestone> includedMilestones,
            IDictionary<string, RoadmapActivity> includedActivities)
        {
            if (rootMilestone == null)
            {
                return;
            }

            CollectRoadmapMilestoneBranch(rootMilestone, milestonesByParent, activitiesByParent, includedMilestones, includedActivities);
        }

        private static void CollectRoadmapMilestoneBranch(
            RoadmapMilestone milestone,
            IDictionary<string, List<RoadmapMilestone>> milestonesByParent,
            IDictionary<string, List<RoadmapActivity>> activitiesByParent,
            IDictionary<string, RoadmapMilestone> includedMilestones,
            IDictionary<string, RoadmapActivity> includedActivities)
        {
            var milestoneKey = NormalizeNodeKey(milestone.RoadmapMilestoneSysId);
            if (includedMilestones.ContainsKey(milestoneKey))
            {
                return;
            }

            includedMilestones[milestoneKey] = milestone;

            var childKey = BuildRoadmapParentKey("milestone", milestone.RoadmapMilestoneSysId);
            if (milestonesByParent.TryGetValue(childKey, out var childMilestones))
            {
                foreach (var childMilestone in childMilestones.OrderBy(item => item.OrderIndex))
                {
                    CollectRoadmapMilestoneBranch(childMilestone, milestonesByParent, activitiesByParent, includedMilestones, includedActivities);
                }
            }

            if (activitiesByParent.TryGetValue(childKey, out var childActivities))
            {
                foreach (var childActivity in childActivities.OrderBy(item => item.OrderIndex))
                {
                    CollectRoadmapActivityBranch(childActivity, milestonesByParent, activitiesByParent, includedMilestones, includedActivities);
                }
            }
        }

        private static void CollectRoadmapActivityBranch(
            RoadmapActivity activity,
            IDictionary<string, List<RoadmapMilestone>> milestonesByParent,
            IDictionary<string, List<RoadmapActivity>> activitiesByParent,
            IDictionary<string, RoadmapMilestone> includedMilestones,
            IDictionary<string, RoadmapActivity> includedActivities)
        {
            var activityKey = NormalizeNodeKey(activity.RoadmapActivitySysId);
            if (includedActivities.ContainsKey(activityKey))
            {
                return;
            }

            includedActivities[activityKey] = activity;

            var childKey = BuildRoadmapParentKey("activity", activity.RoadmapActivitySysId);
            if (milestonesByParent.TryGetValue(childKey, out var childMilestones))
            {
                foreach (var childMilestone in childMilestones.OrderBy(item => item.OrderIndex))
                {
                    CollectRoadmapMilestoneBranch(childMilestone, milestonesByParent, activitiesByParent, includedMilestones, includedActivities);
                }
            }

            if (activitiesByParent.TryGetValue(childKey, out var childActivities))
            {
                foreach (var childActivity in childActivities.OrderBy(item => item.OrderIndex))
                {
                    CollectRoadmapActivityBranch(childActivity, milestonesByParent, activitiesByParent, includedMilestones, includedActivities);
                }
            }
        }

        private static string ResolveClonedParentSysId(
            string parentType,
            string parentSysId,
            IDictionary<string, string> milestoneIdMap,
            IDictionary<string, string> activityIdMap)
        {
            if (string.Equals(parentType, "milestone", StringComparison.OrdinalIgnoreCase))
            {
                if (milestoneIdMap.TryGetValue(NormalizeNodeKey(parentSysId), out var clonedMilestoneId))
                {
                    return clonedMilestoneId;
                }

                throw new Exception("Unable to resolve the cloned parent milestone.");
            }

            if (string.Equals(parentType, "activity", StringComparison.OrdinalIgnoreCase))
            {
                if (activityIdMap.TryGetValue(NormalizeNodeKey(parentSysId), out var clonedActivityId))
                {
                    return clonedActivityId;
                }

                throw new Exception("Unable to resolve the cloned parent activity.");
            }

            return parentSysId;
        }

        private static string BuildPrereqKey(string roadmapActivitySysId, string prerequisiteSysId)
        {
            return $"{NormalizeNodeKey(roadmapActivitySysId)}::{NormalizeNodeKey(prerequisiteSysId)}";
        }

        private static string BuildMilestoneSnapshotKey(string roadmapMilestoneSysId, string roadmapSysId)
        {
            return $"{NormalizeNodeKey(roadmapMilestoneSysId)}::{NormalizeNodeKey(roadmapSysId)}";
        }

        private static string BuildActivitySnapshotKey(string roadmapActivitySysId, string roadmapSysId)
        {
            return $"{NormalizeNodeKey(roadmapActivitySysId)}::{NormalizeNodeKey(roadmapSysId)}";
        }

        private static string BuildProjectPrereqSnapshotKey(string roadmapActivityPrereqSysId, string roadmapActivitySysId, string prerequisiteSysId)
        {
            return $"{NormalizeNodeKey(roadmapActivityPrereqSysId)}::{NormalizeNodeKey(roadmapActivitySysId)}::{NormalizeNodeKey(prerequisiteSysId)}";
        }

        private static bool MilestoneSnapshotDiffers(RoadmapMilestone snapshot, RoadmapMilestone source)
        {
            return !string.Equals(snapshot.MaturityCode ?? string.Empty, source.MaturityCode ?? string.Empty, StringComparison.Ordinal)
                || !string.Equals(snapshot.ParentType ?? string.Empty, source.ParentType ?? string.Empty, StringComparison.Ordinal)
                || !string.Equals(snapshot.ParentSysId ?? string.Empty, source.ParentSysId ?? string.Empty, StringComparison.Ordinal)
                || !string.Equals(snapshot.MilestoneAlias ?? string.Empty, source.MilestoneAlias ?? string.Empty, StringComparison.Ordinal)
                || !string.Equals(snapshot.MilestoneDescription ?? string.Empty, source.MilestoneDescription ?? string.Empty, StringComparison.Ordinal)
                || snapshot.OrderIndex != source.OrderIndex
                || snapshot.IsActive != source.IsActive
                || snapshot.IsRequired != source.IsRequired;
        }

        private static bool ActivitySnapshotDiffers(RoadmapActivity snapshot, RoadmapActivity source)
        {
            return !string.Equals(snapshot.RoadmapSysId ?? string.Empty, source.RoadmapSysId ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(snapshot.ParentType ?? string.Empty, source.ParentType ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(snapshot.ParentSysId ?? string.Empty, source.ParentSysId ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(snapshot.ActivityName ?? string.Empty, source.ActivityName ?? string.Empty, StringComparison.Ordinal)
                || !string.Equals(snapshot.ActivityDescription ?? string.Empty, source.ActivityDescription ?? string.Empty, StringComparison.Ordinal)
                || snapshot.EstimatedManDays != source.EstimatedManDays
                || snapshot.OrderIndex != source.OrderIndex
                || snapshot.IsActive != source.IsActive
                || snapshot.IsRequired != source.IsRequired;
        }

        private static string GetMilestoneRefreshState(RoadmapMilestone snapshot, RoadmapMilestone source)
        {
            if (snapshot == null)
            {
                return "NEW";
            }

            return MilestoneSnapshotDiffers(snapshot, source) ? "UPDATE" : "NOCHANGE";
        }

        private static string GetActivityRefreshState(RoadmapActivity snapshot, RoadmapActivity source)
        {
            if (snapshot == null)
            {
                return "NEW";
            }

            var changed = !string.Equals(snapshot.ActivityName ?? string.Empty, source.ActivityName ?? string.Empty, StringComparison.Ordinal)
                || !string.Equals(snapshot.ActivityDescription ?? string.Empty, source.ActivityDescription ?? string.Empty, StringComparison.Ordinal)
                || snapshot.EstimatedManDays != source.EstimatedManDays
                || snapshot.IsRequired != source.IsRequired
                || snapshot.OrderIndex != source.OrderIndex;

            return changed ? "UPDATE" : "NOCHANGE";
        }

        private static string BuildPrereqPath(RoadmapActivityPrerequisite prereq, IList<RoadmapMilestone> milestones, IList<RoadmapActivity> activities)
        {
            var activityMap = (activities ?? Enumerable.Empty<RoadmapActivity>())
                .Where(act => act != null && !string.IsNullOrWhiteSpace(act.RoadmapActivitySysId))
                .ToDictionary(act => NormalizeNodeKey(act.RoadmapActivitySysId), act => act, StringComparer.OrdinalIgnoreCase);

            activityMap.TryGetValue(NormalizeNodeKey(prereq?.RoadMapActivitySysId), out var parentActivity);
            activityMap.TryGetValue(NormalizeNodeKey(prereq?.PrerequisiteSysId), out var dependencyActivity);

            var activityPath = parentActivity != null ? BuildActivityPath(parentActivity, milestones, activities) : "(missing activity)";
            var dependencyName = dependencyActivity?.ActivityName ?? "(missing prerequisite)";
            return $"{activityPath} -> prerequisite: {dependencyName}";
        }

        private static string ExtractChangeKeyPayload(string changeKey)
        {
            if (string.IsNullOrWhiteSpace(changeKey))
            {
                return string.Empty;
            }

            var first = changeKey.IndexOf(':');
            if (first < 0)
            {
                return changeKey;
            }

            var second = changeKey.IndexOf(':', first + 1);
            if (second < 0 || second + 1 >= changeKey.Length)
            {
                return string.Empty;
            }

            return changeKey.Substring(second + 1);
        }

        private static int GetNodeDepth(RoadmapMilestone milestone, IDictionary<string, RoadmapMilestone> milestones, IDictionary<string, RoadmapActivity> activities)
        {
            if (string.IsNullOrWhiteSpace(milestone.ParentType))
            {
                return 1;
            }

            if (string.Equals(milestone.ParentType, "milestone", StringComparison.OrdinalIgnoreCase) && milestones.TryGetValue(NormalizeNodeKey(milestone.ParentSysId), out var parentMilestone))
            {
                return GetNodeDepth(parentMilestone, milestones, activities) + 1;
            }

            if (string.Equals(milestone.ParentType, "activity", StringComparison.OrdinalIgnoreCase) && activities.TryGetValue(NormalizeNodeKey(milestone.ParentSysId), out var parentActivity))
            {
                return GetNodeDepth(parentActivity, milestones, activities) + 1;
            }

            return 1;
        }

        private static int GetNodeDepth(RoadmapActivity activity, IDictionary<string, RoadmapMilestone> milestones, IDictionary<string, RoadmapActivity> activities)
        {
            if (string.IsNullOrWhiteSpace(activity.ParentType) || IsRoadmapRoot(activity.ParentType))
            {
                return 1;
            }

            if (string.Equals(activity.ParentType, "milestone", StringComparison.OrdinalIgnoreCase) && milestones.TryGetValue(NormalizeNodeKey(activity.ParentSysId), out var parentMilestone))
            {
                return GetNodeDepth(parentMilestone, milestones, activities) + 1;
            }

            if (string.Equals(activity.ParentType, "activity", StringComparison.OrdinalIgnoreCase) && activities.TryGetValue(NormalizeNodeKey(activity.ParentSysId), out var parentActivity))
            {
                return GetNodeDepth(parentActivity, milestones, activities) + 1;
            }

            return 1;
        }

        private static string BuildMilestonePath(RoadmapMilestone milestone, IList<RoadmapMilestone> milestones, IList<RoadmapActivity> activities)
        {
            var milestoneMap = milestones.ToDictionary(m => NormalizeNodeKey(m.RoadmapMilestoneSysId));
            var activityMap = activities.ToDictionary(a => NormalizeNodeKey(a.RoadmapActivitySysId));
            return BuildMilestonePath(milestone, milestoneMap, activityMap);
        }

        private static string BuildMilestonePath(RoadmapMilestone milestone, IDictionary<string, RoadmapMilestone> milestones, IDictionary<string, RoadmapActivity> activities)
        {
            if (string.IsNullOrWhiteSpace(milestone.ParentType))
            {
                return milestone.MilestoneAlias;
            }

            if (string.Equals(milestone.ParentType, "milestone", StringComparison.OrdinalIgnoreCase) && milestones.TryGetValue(NormalizeNodeKey(milestone.ParentSysId), out var parentMilestone))
            {
                return $"{BuildMilestonePath(parentMilestone, milestones, activities)} / {milestone.MilestoneAlias}";
            }

            if (string.Equals(milestone.ParentType, "activity", StringComparison.OrdinalIgnoreCase) && activities.TryGetValue(NormalizeNodeKey(milestone.ParentSysId), out var parentActivity))
            {
                return $"{BuildActivityPath(parentActivity, milestones, activities)} / {milestone.MilestoneAlias}";
            }

            return milestone.MilestoneAlias;
        }

        private static string BuildActivityPath(RoadmapActivity activity, IList<RoadmapMilestone> milestones, IList<RoadmapActivity> activities)
        {
            var milestoneMap = milestones.ToDictionary(m => NormalizeNodeKey(m.RoadmapMilestoneSysId));
            var activityMap = activities.ToDictionary(a => NormalizeNodeKey(a.RoadmapActivitySysId));
            return BuildActivityPath(activity, milestoneMap, activityMap);
        }

        private static string BuildActivityPath(RoadmapActivity activity, IDictionary<string, RoadmapMilestone> milestones, IDictionary<string, RoadmapActivity> activities)
        {
            if (string.IsNullOrWhiteSpace(activity.ParentType) || IsRoadmapRoot(activity.ParentType))
            {
                return $"Root activities / {activity.ActivityName}";
            }

            if (string.Equals(activity.ParentType, "milestone", StringComparison.OrdinalIgnoreCase) && milestones.TryGetValue(NormalizeNodeKey(activity.ParentSysId), out var parentMilestone))
            {
                return $"{BuildMilestonePath(parentMilestone, milestones, activities)} / {activity.ActivityName}";
            }

            if (string.Equals(activity.ParentType, "activity", StringComparison.OrdinalIgnoreCase) && activities.TryGetValue(NormalizeNodeKey(activity.ParentSysId), out var parentActivity))
            {
                return $"{BuildActivityPath(parentActivity, milestones, activities)} / {activity.ActivityName}";
            }

            return activity.ActivityName;
        }

        private static string BuildMilestoneParentPath(string parentType, string parentSysId, IList<RoadmapMilestone> milestones, IList<RoadmapActivity> activities)
        {
            var milestoneMap = milestones.ToDictionary(m => NormalizeNodeKey(m.RoadmapMilestoneSysId));
            var activityMap = activities.ToDictionary(a => NormalizeNodeKey(a.RoadmapActivitySysId));
            return BuildMilestoneParentPath(parentType, parentSysId, milestoneMap, activityMap);
        }

        private static string BuildMilestoneParentPath(string parentType, string parentSysId, IDictionary<string, RoadmapMilestone> milestones, IDictionary<string, RoadmapActivity> activities)
        {
            if (string.IsNullOrWhiteSpace(parentType) || IsRoadmapRoot(parentType))
            {
                return "Roadmap root";
            }

            if (string.Equals(parentType, "milestone", StringComparison.OrdinalIgnoreCase) && milestones.TryGetValue(NormalizeNodeKey(parentSysId), out var parentMilestone))
            {
                return BuildMilestonePath(parentMilestone, milestones, activities);
            }

            if (string.Equals(parentType, "activity", StringComparison.OrdinalIgnoreCase) && activities.TryGetValue(NormalizeNodeKey(parentSysId), out var parentActivity))
            {
                return BuildActivityPath(parentActivity, milestones, activities);
            }

            return "Roadmap root";
        }

        private static string BuildRoadmapNodePath(NodeRow node, IDictionary<string, NodeRow> nodeMap)
        {
            if (node == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(node.ParentType) || IsRoadmapRoot(node.ParentType))
            {
                return string.Equals(node.NodeType, "activity", StringComparison.OrdinalIgnoreCase)
                    ? $"Root activities / {node.DataName}"
                    : node.DataName;
            }

            if (nodeMap.TryGetValue(NormalizeNodeKey(node.ParentSysId), out var parentNode))
            {
                return $"{BuildRoadmapNodePath(parentNode, nodeMap)} / {node.DataName}";
            }

            return node.DataName;
        }

        private static int ExpandAncestorSelection(
            ISet<string> milestoneIds,
            ISet<string> activityIds,
            IDictionary<string, RoadmapMilestone> sourceMilestones,
            IDictionary<string, RoadmapActivity> sourceActivities,
            IDictionary<string, RoadmapMilestone> snapshotMilestones,
            IDictionary<string, RoadmapActivity> snapshotActivities,
            IDictionary<string, ProjectMilestone> executionMilestones,
            IDictionary<string, ProjectTask> executionTasks)
        {
            var added = 0;
            var changed = true;
            while (changed)
            {
                changed = false;

                foreach (var activityId in activityIds.ToList())
                {
                    if (!sourceActivities.TryGetValue(activityId, out var activity))
                    {
                        continue;
                    }

                    if (string.Equals(activity.ParentType, "milestone", StringComparison.OrdinalIgnoreCase))
                    {
                        var parentId = NormalizeNodeKey(activity.ParentSysId);
                        if (!snapshotMilestones.ContainsKey(parentId) && !executionMilestones.ContainsKey(parentId) && !milestoneIds.Contains(parentId) && sourceMilestones.ContainsKey(parentId))
                        {
                            milestoneIds.Add(parentId);
                            changed = true;
                            added++;
                        }
                    }
                    else if (string.Equals(activity.ParentType, "activity", StringComparison.OrdinalIgnoreCase))
                    {
                        var parentId = NormalizeNodeKey(activity.ParentSysId);
                        if (!snapshotActivities.ContainsKey(parentId) && !executionTasks.ContainsKey(parentId) && !activityIds.Contains(parentId) && sourceActivities.ContainsKey(parentId))
                        {
                            activityIds.Add(parentId);
                            changed = true;
                            added++;
                        }
                    }
                }

                foreach (var milestoneId in milestoneIds.ToList())
                {
                    if (!sourceMilestones.TryGetValue(milestoneId, out var milestone) || string.IsNullOrWhiteSpace(milestone.ParentType))
                    {
                        continue;
                    }

                    if (string.Equals(milestone.ParentType, "milestone", StringComparison.OrdinalIgnoreCase))
                    {
                        var parentId = NormalizeNodeKey(milestone.ParentSysId);
                        if (!snapshotMilestones.ContainsKey(parentId) && !executionMilestones.ContainsKey(parentId) && !milestoneIds.Contains(parentId) && sourceMilestones.ContainsKey(parentId))
                        {
                            milestoneIds.Add(parentId);
                            changed = true;
                            added++;
                        }
                    }
                    else if (string.Equals(milestone.ParentType, "activity", StringComparison.OrdinalIgnoreCase))
                    {
                        var parentId = NormalizeNodeKey(milestone.ParentSysId);
                        if (!snapshotActivities.ContainsKey(parentId) && !executionTasks.ContainsKey(parentId) && !activityIds.Contains(parentId) && sourceActivities.ContainsKey(parentId))
                        {
                            activityIds.Add(parentId);
                            changed = true;
                            added++;
                        }
                    }
                }
            }

            return added;
        }

        private static Tuple<string, string> ResolveExecutionParent(
            RoadmapActivity activity,
            IDictionary<string, ProjectMilestone> executionMilestones,
            IDictionary<string, ProjectTask> executionTasks,
            ProjectMilestone rootExecutionMilestone)
        {
            if (IsRoadmapRoot(activity.ParentType))
            {
                return rootExecutionMilestone == null ? null : Tuple.Create("MILESTONE", rootExecutionMilestone.MilestoneSysId);
            }

            if (string.Equals(activity.ParentType, "milestone", StringComparison.OrdinalIgnoreCase))
            {
                return executionMilestones.TryGetValue(NormalizeNodeKey(activity.ParentSysId), out var milestoneParent)
                    ? Tuple.Create("MILESTONE", milestoneParent.MilestoneSysId)
                    : null;
            }

            if (string.Equals(activity.ParentType, "activity", StringComparison.OrdinalIgnoreCase))
            {
                return executionTasks.TryGetValue(NormalizeNodeKey(activity.ParentSysId), out var taskParent)
                    ? Tuple.Create("TASK", taskParent.ProjectTaskSysId)
                    : null;
            }

            return null;
        }

        ////await this.ChangeStatusToNotStartedAsync(project, "", !autostart);

        ////if (autostart)
        ////{
        ////    project.ActualStartDate = startdate;
        ////    project.ActualStartedBy = project.CreatedBy;
        ////    // ONGOING
        ////    await this.ChangeStatusToStartedAsync(project, "AUTO-START");
        ////}

        //////5.1 Log status - TASKS
        ////// NOT STARTED
        ////await this.ChangeStatusToNotStartedAsync(project, "", !autostart);

        ////if (autostart)
        ////{
        ////    project.ActualStartDate = startdate;
        ////    project.ActualStartedBy = project.CreatedBy;
        ////    // ONGOING
        ////    await this.ChangeStatusToStartedAsync(project, "AUTO-START");
        ////}

        // 5. Fetch all tasks once
        ////var tasks = project.Tasks.ToList();

        ////tasks.ForEach(t =>
        ////{
        ////    t.ProjectNo = projectno;
        ////    t.PlantRoadmapLinkSysId = project.PlantRoadmapLinkSysId;
        ////    t.RoadmapSysId = project.RoadmapSysId;
        ////    t.CreatedBy = project.CreatedBy;

        ////});


        ////var workitems = await _workitemRepository.GetListAsync(project.PlantCode, project.CategoryCode);


        ////// 5. Add project-level tasks (MaturityCode == null) in parallel
        ////var projectLevelWorkitems = workitems.Where(wi => wi.MaturityCode is null).ToList();
        ////var projectLevelTaskAddTasks = projectLevelWorkitems.Select(wi =>
        ////    _taskRepository.AddAsync(new Core.Entities.Task
        ////    {
        ////        ProjectNo = projectno,
        ////        WorkItemSysId = wi.WorkItemSysId,
        ////        TaskName = wi.TaskName,
        ////        TaskType = wi.TaskType,
        ////        IsRequired = wi.IsRequired,
        ////        CreatedBy = project.CreatedBy
        ////    })
        ////);
        ////await Task.WhenAll(projectLevelTaskAddTasks);

        ////// 6. Fetch all fields once
        ////var fields = await _plantfieldRepository.GetListAsync(plantcode: project.PlantCode);

        ////// 7. Add project-level fields in parallel
        ////var projectLevelFields = fields.Where(f =>
        ////    (f.CategoryCode is null || f.CategoryCode == project.CategoryCode) &&
        ////    f.MaturityCode is null &&
        ////    f.WorkItemSysId is null).ToList();

        ////var projectFieldAddTasks = projectLevelFields.Select(f => _projectfieldRepository.AddAsync(new ProjectField
        ////{
        ////    ProjectNo = projectno,
        ////    PlantFieldSysId = f.PlantFieldSysId,
        ////    CreatedBy = project.CreatedBy
        ////}));
        ////await Task.WhenAll(projectFieldAddTasks);

        ////// 8. Fetch and order milestones once
        ////var milestones = (await _plantcategorymilestoneRepository.GetListAsync(project.PlantCode, project.CategoryCode))
        ////    .OrderBy(p => p.ParentSysId == null) // false first (parents), true later (children)
        ////    .ThenBy(p => p.ParentSysId)
        ////    .ToList();

        ////var projectMilestones = new List<ProjectMilestone>();
        ////var milestoneTaskAddTasks = new List<Task>();
        ////int sequenceNo = 0;

        ////foreach (var milestone in milestones)
        ////{
        ////    // Determine parent milestone SysId if applicable
        ////    string parentMilestoneSysId = null;
        ////    if (!string.IsNullOrEmpty(milestone.ParentSysId))
        ////    {
        ////        parentMilestoneSysId = projectMilestones
        ////            .Where(m => m.MaturityCode == milestone.MaturityCode)
        ////            .Select(m => m.MilestoneSysId)
        ////            .SingleOrDefault();
        ////    }

        ////    var newMilestone = new ProjectMilestone
        ////    {
        ////        ProjectNo = projectno,
        ////        MaturityCode = milestone.MaturityCode,
        ////        ParentSysId = parentMilestoneSysId,
        ////        SequenceNo = sequenceNo++,
        ////        CreatedBy = project.CreatedBy
        ////    };

        ////    // Add milestone to DB to get MilestoneSysId
        ////    newMilestone.MilestoneSysId = await _projectmilestoneRepository.AddAsync(newMilestone);
        ////    projectMilestones.Add(newMilestone);

        ////    // Add milestone-level fields in parallel
        ////    var milestoneFields = fields.Where(f =>
        ////        (f.CategoryCode is null || f.CategoryCode == project.CategoryCode) &&
        ////        f.MaturityCode == milestone.MaturityCode &&
        ////        f.WorkItemSysId is null).ToList();

        ////    var milestoneFieldAddTasks = milestoneFields.Select(f => _projectfieldRepository.AddAsync(new ProjectField
        ////    {
        ////        ProjectNo = projectno,
        ////        PlantFieldSysId = f.PlantFieldSysId,
        ////        MilestoneSysId = newMilestone.MilestoneSysId,
        ////        CreatedBy = project.CreatedBy
        ////    }));
        ////    await Task.WhenAll(milestoneFieldAddTasks);

        ////    // Add milestone-level tasks
        ////    var milestoneWorkitems = workitems.Where(wi => wi.MaturityCode == milestone.MaturityCode).ToList();
        ////    milestoneTaskAddTasks.AddRange(milestoneWorkitems.Select(wi =>
        ////        _taskRepository.AddAsync(new Core.Entities.Task
        ////        {
        ////            ProjectNo = projectno,
        ////            MilestoneSysId = newMilestone.MilestoneSysId,
        ////            WorkItemSysId = wi.WorkItemSysId,
        ////            TaskName = wi.TaskName,
        ////            TaskType = wi.TaskType,
        ////            IsRequired = wi.IsRequired,
        ////            CreatedBy = project.CreatedBy
        ////        })
        ////    ));
        ////}
        ////await Task.WhenAll(milestoneTaskAddTasks);

        ////// 9. Get all tasks for linking prerequisites
        ////var tasks = await _taskRepository.GetListAsync(projectno);
        ////var taskDict = tasks.ToDictionary(t => t.WorkItemSysId);

        ////// 10. Sort workitems by PlantCode, CategoryCode, MaturityCode
        ////var sortedWorkitems = workitems
        ////    .OrderBy(wi => wi.PlantCode)
        ////    .ThenBy(wi => wi.CategoryCode)
        ////    .ThenBy(wi => wi.MaturityCode)
        ////    .ToList();

        ////var prerequisiteAddTasks = new List<Task>();

        ////foreach (var workitem in sortedWorkitems)
        ////{
        ////    // Add members and fields per project-milestone-task in parallel if task exists
        ////    if (taskDict.TryGetValue(workitem.WorkItemSysId, out var taskItem))
        ////    {
        ////        var taskFields = fields.Where(f => f.WorkItemSysId == workitem.WorkItemSysId).ToList();
        ////        var taskFieldAddTasks = taskFields.Select(f => _projectfieldRepository.AddAsync(new ProjectField
        ////        {
        ////            ProjectNo = projectno,
        ////            PlantFieldSysId = f.PlantFieldSysId,
        ////            MilestoneSysId = taskItem.MilestoneSysId,
        ////            TaskSysId = taskItem.TaskSysId,
        ////            CreatedBy = project.CreatedBy
        ////        }));
        ////        await Task.WhenAll(taskFieldAddTasks);


        ////        // Assign task members
        ////        // Retrieve members per workitem
        ////        workitem.Members = await _workitemmemberRepository.GetListAsync(workitem.WorkItemSysId);

        ////        var taskmemberAddTasks = workitem.Members.Select(m => _taskmemberRepository.AddAsync(new TaskMember
        ////        {
        ////            ProjectNo = projectno,
        ////            TaskSysId = taskItem.TaskSysId,
        ////            UserId = m.UserId,
        ////            UserGroupId = m.UserGroupId,
        ////            ADGroup = m.ADGroup
        ////        }));
        ////        await Task.WhenAll(taskmemberAddTasks);

        ////    }



        ////    // Fetch prerequisites for this workitem
        ////    var workitemPrerequisites = await _workitemprerequisiteRepository.GetListAsync(workitemsysid: workitem.WorkItemSysId);

        ////    foreach (var prerequisite in workitemPrerequisites)
        ////    {
        ////        if (taskDict.TryGetValue(prerequisite.WorkItemSysId, out var item) &&
        ////            taskDict.TryGetValue(prerequisite.PrerequisiteWorkItemSysId, out var prerequisiteItem))
        ////        {
        ////            prerequisiteAddTasks.Add(_taskprerequisiteRepository.AddAsync(new TaskPrerequisite
        ////            {
        ////                TaskSysId = item.TaskSysId,
        ////                PrerequisiteTaskSysId = prerequisiteItem.TaskSysId,
        ////                ProjectNo = projectno
        ////            }));
        ////        }
        ////    }
        ////}
        ////await Task.WhenAll(prerequisiteAddTasks);

        // Commit all changes




        //////public async Task DeleteProjectAsync(string projectno, string loggeduser, string reason)
        //////{
        //////    _dataAccess.BeginTransaction();

        //////    try
        //////    {
        //////        //GET PROJECT INFO
        //////        var project = await _projectRepository.GetAsync(projectno);


        //////        //SET USER WHO DELETES THE Project
        //////        project.ModifiedBy = loggeduser;
        //////        await _projectRepository.UpdateAsync(project);

        //////        //DELETE RECORD
        //////        await _projectRepository.DeleteAsync(projectno);



        //////        _dataAccess.CommitTransaction();

        //////        // raise event that project was deleted
        //////        _eventBus.Publish(new ProjectDeletedEventArgs(project.ProjectNo, project.ModifiedBy, project.ModifiedDate.Value, reason));

        //////    }
        //////    catch (Exception ex)
        //////    {
        //////        _dataAccess.RollbackTransaction();
        //////        throw new Exception(ex.Message);

        //////    }


        //////}
        //////public async Task<PagedResult<Project>> GetPagedProjectsAsync(int pageNumber, int pageSize, string searchTerm = null, string orderBy = null, string orderDirection = null)
        //////{
        //////    return await _projectRepository.GetPagedProjectsAsync(pageNumber, pageSize, searchTerm, orderBy, orderDirection);
        //////}
        //////public async Task<Project> GetProjectByProductCodeAsync(string productcode)
        //////{
        //////    return await _projectRepository.GetByProductCodeAsync(productcode);
        //////}

        //////public async Task PromoteProject(Project project, string loggeduser)
        //////{
        //////    _dataAccess.BeginTransaction();

        //////    try
        //////    {
        //////        //current project
        //////        var _project = await _projectRepository.GetAsync(project.ProjectNo);

        //////        if (_project.TransactionKey != project.TransactionKey)
        //////        {
        //////            _dataAccess.RollbackTransaction();
        //////            throw new Exception($"Project was updated recently, please refresh the page again to get the latest update.");
        //////        }

        //////        var statusCompleted = Core.Enums.Status.Completed.ToString();
        //////        _project.ModifiedBy = loggeduser;
        //////        _project.ModifiedDate = DateTime.UtcNow;
        //////        // check all pending task on current milestone
        //////        string[] invalidstatus = { "Completed", "Canceled" };
        //////        var countTasksPerMilestone = (await _taskRepository.GetListAsync(project.ProjectNo, maturitycode: _project.ProjectMaturityCode)).Where(t => !invalidstatus.Contains(t.Status)).Count();

        //////        if (countTasksPerMilestone > 0)
        //////        {
        //////            if (countTasksPerMilestone == 1)
        //////                throw new Exception($"Project still has 1 pending task on Current Milestone.");
        //////            else
        //////                throw new Exception($"Project still have {countTasksPerMilestone} pending tasks on Current Milestone.");
        //////        }

        //////        var milestone = (await _projectmilestoneRepository.GetListAsync(project.ProjectNo));
        //////        // check all pending milestone, set as Project is Complete if no remaining milestone
        //////        var remainingMilestones = milestone.Where(t => t.MaturityCode != _project.ProjectMaturityCode || !invalidstatus.Contains(t.Status));

        //////        // get current milestone and set as complete
        //////        var currentmilestone = milestone.Where(m => m.MaturityCode == _project.ProjectMaturityCode);
        //////        var currentmilestoneTask = currentmilestone.Select(cm =>
        //////        {
        //////            cm.Status = statusCompleted;
        //////            cm.ModifiedBy = _project.ModifiedBy;
        //////            cm.ModifiedDate = _project.ModifiedDate;
        //////            return _projectmilestoneRepository.UpdateAsync(cm);
        //////        });
        //////        await Task.WhenAll(currentmilestoneTask);


        //////        if (remainingMilestones.Count() == 0)
        //////        {
        //////            // Project is Complete
        //////            _project.Status = statusCompleted;
        //////            _project.ProjectEndDate = project.ProjectEndDate ?? DateTime.Now;
        //////            _project.TransactionKey = project.TransactionKey;
        //////            var rowsaffected = await _projectRepository.UpdateAsync(_project);

        //////            await _statuschangeRepository.AddAsync(new StatusChange
        //////            {
        //////                ProjectNo = _project.ProjectNo,
        //////                Status = _project.Status,
        //////                CreatedBy = _project.ModifiedBy,
        //////                CreatedDate = _project.ModifiedDate.Value
        //////            });

        //////            _dataAccess.CommitTransaction();


        //////            // raise event that Task status was changed
        //////            _eventBus.Publish(new ProjectCompletedEventArgs(project.ProjectNo, project.ModifiedBy, project.ModifiedDate.Value, ""));
        //////        }
        //////        else
        //////        {

        //////            // activate task of next milestone
        //////            var nextmilestonetask = remainingMilestones.OrderBy(m => m.SequenceNo).Select(s =>
        //////            {
        //////                s.Status = Core.Enums.Status.Ongoing.ToString();
        //////                s.ModifiedBy = _project.ModifiedBy;
        //////                s.ModifiedDate = _project.ModifiedDate;
        //////                return _projectmilestoneRepository.UpdateAsync(s);
        //////            }).FirstOrDefault();
        //////            await Task.WhenAll(nextmilestonetask);

        //////            _dataAccess.CommitTransaction();

        //////            currentmilestone = remainingMilestones.OrderBy(m => m.SequenceNo);

        //////            // raise event that Task status was changed
        //////            _eventBus.Publish(new ProjectPromotedEventArgs(project.ProjectNo, currentmilestone.FirstOrDefault().MaturityCode, project.ModifiedBy, project.ModifiedDate.Value));

        //////        }



        //////    }
        //////    catch (Exception ex)
        //////    {
        //////        _dataAccess.RollbackTransaction();
        //////        throw;
        //////    }
        //////}
        //////public async Task ChangeStatusToNotStartedAsync(Project project, string reason)
        //////{
        //////    _dataAccess.BeginTransaction();
        //////    try
        //////    {

        //////        await this.ChangeStatus(Core.Enums.Status.NotStarted, project, reason);

        //////        var _project = await _projectRepository.GetAsync(project.ProjectNo);
        //////        var products = (await _projectproductRepository.GetListAsync(project.ProjectNo)).Select(p => string.Join(",", p.ProductCode));


        //////        //_emailSender.SendStatusChangeOnTaskNotificationAsync(project.ProjectNo, _project.ProjectName, );
        //////        _dataAccess.CommitTransaction();


        //////        // raise event that Task status was changed
        //////        _eventBus.Publish(new ProjectNotStartedEventArgs(project.ProjectNo, project.ModifiedBy, project.ModifiedDate.Value, reason));
        //////    }
        //////    catch
        //////    {
        //////        _dataAccess.RollbackTransaction();
        //////        throw;
        //////    }


        //////}
        //////public async Task ChangeStatusToStartedAsync(Project project, string remarks)
        //////{
        //////    _dataAccess.BeginTransaction();
        //////    var status = Core.Enums.Status.Ongoing.ToString();

        //////    try
        //////    {
        //////        var _project = await _projectRepository.GetAsync(project.ProjectNo);

        //////        if (_project.TransactionKey != project.TransactionKey)
        //////        {
        //////            // rollback all changes
        //////            _dataAccess.RollbackTransaction();
        //////            throw new Exception("Status Change Failed! Project was recently updated");
        //////        }
        //////        _project.Status = status;
        //////        _project.ModifiedBy = project.ModifiedBy;
        //////        _project.ModifiedDate = project.ModifiedDate;

        //////        // update project status
        //////        var rowsaffected = await _projectRepository.UpdateAsync(_project);

        //////        if (rowsaffected == 0)
        //////        {
        //////            // rollback all changes
        //////            _dataAccess.RollbackTransaction();
        //////            throw new Exception("Status Change Failed! Project was recently updated");
        //////        }

        //////        // add project status
        //////        await _statuschangeRepository.AddAsync(new StatusChange
        //////        {
        //////            ProjectNo = _project.ProjectNo,
        //////            Status = status,
        //////            Reason = remarks,
        //////            CreatedBy = project.ModifiedBy,
        //////            CreatedDate = project.ModifiedDate.Value
        //////        });




        //////        // get all milestones
        //////        var _milestones = (await _projectmilestoneRepository.GetListAsync(project.ProjectNo)).OrderBy(p => p.SequenceNo);
        //////        // filter the first milestone with status not Completed
        //////        var _firstmilestone = _milestones.FirstOrDefault();
        //////        // set status to Ongoing.
        //////        _firstmilestone.Status = status;
        //////        _firstmilestone.ModifiedBy = project.ModifiedBy;
        //////        _firstmilestone.ModifiedDate = project.ModifiedDate;
        //////        // milestone status
        //////        rowsaffected = await _projectmilestoneRepository.UpdateAsync(_firstmilestone);

        //////        if (rowsaffected == 0)
        //////        {
        //////            // rollback all changes
        //////            _dataAccess.RollbackTransaction();
        //////            throw new Exception("Cannot change Milestone to Ongoing! Milestone was recently updated.");
        //////        }

        //////        // add milestone status
        //////        await _statuschangeRepository.AddAsync(new StatusChange
        //////        {
        //////            ProjectNo = _project.ProjectNo,
        //////            MilestoneSysId = _firstmilestone.MilestoneSysId,
        //////            Status = status,
        //////            Reason = remarks,
        //////            CreatedBy = project.ModifiedBy,
        //////            CreatedDate = project.ModifiedDate.Value
        //////        });


        //////        // commit all changes
        //////        _dataAccess.CommitTransaction();


        //////        // raise event that Task status was changed
        //////        _eventBus.Publish(new ProjectStartedEventArgs(project.ProjectNo, project.ModifiedBy, project.ModifiedDate.Value, remarks));
        //////    }
        //////    catch
        //////    {
        //////        // rollback all changes
        //////        _dataAccess.RollbackTransaction();
        //////        throw;
        //////    }
        //////}
        //////public async Task ChangeStatusToResumedAsync(Project project, string reason)
        //////{
        //////    _dataAccess.BeginTransaction();
        //////    var status = Core.Enums.Status.Ongoing.ToString();

        //////    try
        //////    {
        //////        var _project = await _projectRepository.GetAsync(project.ProjectNo);

        //////        if (_project.TransactionKey != project.TransactionKey)
        //////        {
        //////            // rollback all changes
        //////            _dataAccess.RollbackTransaction();
        //////            throw new Exception("Status Change Failed! Project was recently updated");
        //////        }

        //////        var previousStatus = _project.Status;

        //////        _project.Status = status;
        //////        _project.ModifiedBy = project.ModifiedBy;
        //////        _project.ModifiedDate = project.ModifiedDate;

        //////        // update project status
        //////        var rowsaffected = await _projectRepository.UpdateAsync(_project);

        //////        if (rowsaffected == 0)
        //////        {
        //////            // rollback all changes
        //////            _dataAccess.RollbackTransaction();
        //////            throw new Exception("Status Change Failed! Project was recently updated");
        //////        }

        //////        // add project status
        //////        await _statuschangeRepository.AddAsync(new StatusChange
        //////        {
        //////            ProjectNo = _project.ProjectNo,
        //////            Status = status,
        //////            Reason = reason,
        //////            CreatedBy = project.ModifiedBy,
        //////            CreatedDate = project.ModifiedDate.Value
        //////        });




        //////        // get all milestones that are not completed
        //////        var _milestones = (await _projectmilestoneRepository.GetListAsync(project.ProjectNo)).Where(m => m.Status != "Completed").OrderBy(p => p.SequenceNo);
        //////        // filter the first milestone with status not Completed
        //////        var _firstmilestone = _milestones.FirstOrDefault();
        //////        // set status to Ongoing.
        //////        _firstmilestone.Status = status;
        //////        _firstmilestone.ModifiedBy = project.ModifiedBy;
        //////        _firstmilestone.ModifiedDate = project.ModifiedDate;
        //////        // milestone status
        //////        rowsaffected = await _projectmilestoneRepository.UpdateAsync(_firstmilestone);

        //////        if (rowsaffected == 0)
        //////        {
        //////            // rollback all changes
        //////            _dataAccess.RollbackTransaction();
        //////            throw new Exception("Cannot change Milestone to Ongoing! Milestone was recently updated.");
        //////        }

        //////        // add milestone status
        //////        await _statuschangeRepository.AddAsync(new StatusChange
        //////        {
        //////            ProjectNo = _project.ProjectNo,
        //////            MilestoneSysId = _firstmilestone.MilestoneSysId,
        //////            Status = status,
        //////            Reason = reason,
        //////            CreatedBy = project.ModifiedBy,
        //////            CreatedDate = project.ModifiedDate.Value
        //////        });


        //////        // commit all changes
        //////        _dataAccess.CommitTransaction();


        //////        // raise event that Task status was changed
        //////        _eventBus.Publish(new ProjectResumedEventArgs(project.ProjectNo, (Core.Enums.Status)Enum.Parse(typeof(Core.Enums.Status), previousStatus), project.ModifiedBy, project.ModifiedDate.Value, reason));
        //////    }
        //////    catch
        //////    {
        //////        // rollback all changes
        //////        _dataAccess.RollbackTransaction();
        //////        throw;
        //////    }
        //////}

        //////public async Task ChangeStatusToCompletedAsync(Project project, string remarks)
        //////{


        //////    // check if all tasks are closed (Not -> Completed,Canceled)
        //////    string[] invalidstatus = { "Completed", "Canceled" };
        //////    var countTasks = (await _taskRepository.GetListAsync(project.ProjectNo)).Where(t => !invalidstatus.Contains(t.Status)).Count();
        //////    // throw error if there are unclosed tasks (ask user to confirm if to proceed)
        //////    if (countTasks > 0)
        //////    {
        //////        throw new Exception("Project still has pending tasks.");

        //////    }

        //////    // check if all milestones are closed
        //////    var countMilestones = (await _projectmilestoneRepository.GetListAsync(project.ProjectNo)).Where(t => !invalidstatus.Contains(t.Status)).Count();

        //////    // throw error if there are unclosed milestones (ask user to confirm if to proceed)
        //////    if (countTasks > 1)
        //////    {
        //////        throw new Exception("Project still has more than 1 pending milestone.");

        //////    }

        //////    try
        //////    {
        //////        var _project = await _projectRepository.GetAsync(project.ProjectNo);
        //////        var previousStatus = _project.Status;
        //////        _project.Status = project.Status.ToString();
        //////        _project.TransactionKey = project.TransactionKey;

        //////        _project.ProjectEndDate = _project.ProjectEndDate ?? DateTime.Now;

        //////        var rowsaffected = await _projectRepository.UpdateAsync(_project);

        //////        await _statuschangeRepository.AddAsync(new StatusChange
        //////        {
        //////            ProjectNo = _project.ProjectNo,
        //////            Status = project.Status.ToString(),
        //////            Reason = remarks,
        //////            CreatedBy = _project.ModifiedBy,
        //////            CreatedDate = _project.ModifiedDate.Value
        //////        });

        //////        _dataAccess.CommitTransaction();


        //////        // raise event that Task status was changed
        //////        _eventBus.Publish(new ProjectCompletedEventArgs(project.ProjectNo, project.ModifiedBy, project.ModifiedDate.Value, remarks));

        //////    }
        //////    catch (Exception ex)
        //////    {
        //////        _dataAccess.RollbackTransaction();
        //////        throw;
        //////    }
        //////}
        //////public async Task ChangeStatusToForcedCompletedAsync(Project project, string reason)
        //////{
        //////    _dataAccess.BeginTransaction();
        //////    try
        //////    {
        //////        string[] validstatus = { "Ongoing", "Hold", "NotStarted", "Failed" };
        //////        await ChangeStatusForced(Core.Enums.Status.Completed, validstatus, project, reason);
        //////        _dataAccess.CommitTransaction();

        //////        // raise event that Task status was changed
        //////        _eventBus.Publish(new ProjectCompletedEventArgs(project.ProjectNo, project.ModifiedBy, project.ModifiedDate.Value, reason));
        //////    }
        //////    catch (Exception ex)
        //////    {
        //////        _dataAccess.RollbackTransaction();
        //////        throw;
        //////    }

        //////}
        //////public async Task ChangeStatusToCanceledAsync(Project project, string reason)
        //////{


        //////    _dataAccess.BeginTransaction();
        //////    try
        //////    {
        //////        string[] validstatus = { "Ongoing", "Hold" };
        //////        await ChangeStatusForced(Core.Enums.Status.Canceled, validstatus, project, reason);
        //////        _dataAccess.CommitTransaction();

        //////        // raise event that Task status was changed
        //////        _eventBus.Publish(new ProjectCanceledEventArgs(project.ProjectNo, project.ModifiedBy, project.ModifiedDate.Value, reason));
        //////    }
        //////    catch (Exception ex)
        //////    {
        //////        _dataAccess.RollbackTransaction();
        //////        throw;
        //////    }
        //////}
        //////public async Task ChangeStatusToFailedAsync(Project project, string reason)
        //////{
        //////    _dataAccess.BeginTransaction();
        //////    try
        //////    {
        //////        string[] validstatus = { "Ongoing", "Hold" };
        //////        await ChangeStatusForced(Core.Enums.Status.Failed, validstatus, project, reason);
        //////        _dataAccess.CommitTransaction();

        //////        // raise event that Task status was changed
        //////        _eventBus.Publish(new ProjectFailedEventArgs(project.ProjectNo, project.ModifiedBy, project.ModifiedDate.Value, reason));
        //////    }
        //////    catch (Exception ex)
        //////    {
        //////        _dataAccess.RollbackTransaction();
        //////        throw;
        //////    }
        //////}
        //////public async Task ChangeStatusToHoldAsync(Project project, string reason)
        //////{
        //////    _dataAccess.BeginTransaction();
        //////    try
        //////    {
        //////        string[] validstatus = { "Ongoing" };
        //////        await ChangeStatusForced(Core.Enums.Status.Hold, validstatus, project, reason);
        //////        _dataAccess.CommitTransaction();

        //////        // raise event that Task status was changed
        //////        _eventBus.Publish(new ProjectFailedEventArgs(project.ProjectNo, project.ModifiedBy, project.ModifiedDate.Value, reason));
        //////    }
        //////    catch (Exception ex)
        //////    {
        //////        _dataAccess.RollbackTransaction();
        //////        throw;
        //////    }
        //////}


        //////private async Task ChangeStatus(Core.Enums.Status status, Project project, string reason)
        //////{

        //////    try
        //////    {
        //////        var _project = await _projectRepository.GetAsync(project.ProjectNo);
        //////        _project.Status = status.ToString();
        //////        _project.TransactionKey = project.TransactionKey;
        //////        _project.ProjectStartDate = status == Core.Enums.Status.Ongoing ? _project.ProjectStartDate ?? DateTime.Now : _project.ProjectStartDate;
        //////        _project.ProjectEndDate = status == Core.Enums.Status.Completed ? _project.ProjectEndDate ?? DateTime.Now : _project.ProjectEndDate;

        //////        var rowsaffected = await _projectRepository.UpdateAsync(_project);

        //////        await _statuschangeRepository.AddAsync(new StatusChange
        //////        {
        //////            ProjectNo = _project.ProjectNo,
        //////            Status = status.ToString(),
        //////            Reason = reason,
        //////            CreatedBy = _project.ModifiedBy
        //////        });





        //////    }
        //////    catch (Exception ex)
        //////    {
        //////        _dataAccess.RollbackTransaction();
        //////        throw;
        //////    }

        //////}
        //////private async Task ChangeStatusForced(Core.Enums.Status status, string[] validstatus, Project project, string remarks)
        //////{


        //////    // get all Ongoing, Hold, NotStarted, Failed tasks
        //////    var tasks = (await _taskRepository.GetListAsync(project.ProjectNo)).Where(t => validstatus.Contains(t.Status)).ToList();
        //////    var changestatustasksAddTasks = tasks.Select(t => _taskRepository.UpdateAsync(
        //////        new Core.Entities.Task
        //////        {
        //////            TaskSysId = t.TaskSysId,
        //////            ProjectNo = t.ProjectNo,
        //////            MilestoneSysId = t.MilestoneSysId,
        //////            WorkItemSysId = t.WorkItemSysId,
        //////            TaskName = t.TaskName,
        //////            TaskType = t.TaskType,
        //////            TaskValue = t.TaskValue,
        //////            TargetSart = t.TargetSart,
        //////            TargetCompletion = t.TargetCompletion,
        //////            ActualStartDate = t.ActualStartDate,
        //////            ActualCompletionDate = t.ActualCompletionDate,
        //////            Status = status.ToString(),
        //////            Remarks = t.Remarks,
        //////            IsRequired = t.IsRequired,
        //////            CreatedBy = t.CreatedBy,
        //////            CreatedDate = t.CreatedDate,
        //////            ModifiedBy = project.ModifiedBy,
        //////            ModifiedDate = project.ModifiedDate,
        //////            TransactionKey = t.TransactionKey,
        //////        }
        //////        ));
        //////    await Task.WhenAll(changestatustasksAddTasks);

        //////    var statuschangeAddTasks = tasks.Select(t =>
        //////        _statuschangeRepository.AddAsync(new StatusChange
        //////        {
        //////            TaskSysId = t.TaskSysId,
        //////            ProjectNo = t.ProjectNo,
        //////            MilestoneSysId = t.MilestoneSysId,
        //////            Status = status.ToString(),
        //////            Reason = remarks,
        //////            CreatedBy = project.ModifiedBy,
        //////            CreatedDate = project.ModifiedDate.Value
        //////        })
        //////    );
        //////    await Task.WhenAll(statuschangeAddTasks);


        //////    // set status to Completed all tasks with remarks as forced closed.
        //////    // get all Ongoing, Hold, NotStarted milestones
        //////    var milestones = (await _projectmilestoneRepository.GetListAsync(project.ProjectNo)).Where(t => validstatus.Contains(t.Status)).ToList();
        //////    var changestatusilestonesAddTasks = milestones.Select(m => _projectmilestoneRepository.UpdateAsync(
        //////        new Core.Entities.ProjectMilestone
        //////        {
        //////            MilestoneSysId = m.MilestoneSysId,
        //////            ProjectNo = m.ProjectNo,
        //////            MaturityCode = m.MaturityCode,
        //////            ParentSysId = m.ParentSysId,
        //////            TargetStart = m.TargetStart,
        //////            TargetCompletion = m.TargetCompletion,
        //////            ActualStartDate = m.ActualStartDate,
        //////            ActualCompletionDate = m.ActualCompletionDate,
        //////            Status = status.ToString(),
        //////            Remarks = m.Remarks,
        //////            SequenceNo = m.SequenceNo,
        //////            CreatedBy = m.CreatedBy,
        //////            CreatedDate = m.CreatedDate,
        //////            ModifiedBy = project.ModifiedBy,
        //////            ModifiedDate = project.ModifiedDate,
        //////            TransactionKey = m.TransactionKey,
        //////        }
        //////        ));

        //////    // set status to Completed all milestones
        //////    await Task.WhenAll(changestatustasksAddTasks);

        //////    var statuschangemilestoneAddTasks = milestones.Select(t =>
        //////        _statuschangeRepository.AddAsync(new StatusChange
        //////        {
        //////            ProjectNo = t.ProjectNo,
        //////            MilestoneSysId = t.MilestoneSysId,
        //////            Status = status.ToString(),
        //////            Reason = remarks,
        //////            CreatedBy = project.ModifiedBy,
        //////            CreatedDate = project.ModifiedDate.Value
        //////        })
        //////    );
        //////    await Task.WhenAll(statuschangeAddTasks);



        //////    try
        //////    {
        //////        var _project = await _projectRepository.GetAsync(project.ProjectNo);
        //////        var previousStatus = _project.Status;
        //////        _project.Status = status.ToString();
        //////        _project.TransactionKey = project.TransactionKey;

        //////        _project.ProjectEndDate = _project.ProjectEndDate ?? DateTime.Now;

        //////        var rowsaffected = await _projectRepository.UpdateAsync(_project);

        //////        await _statuschangeRepository.AddAsync(new StatusChange
        //////        {
        //////            ProjectNo = _project.ProjectNo,
        //////            Status = status.ToString(),
        //////            Reason = remarks,
        //////            CreatedBy = _project.ModifiedBy,
        //////            CreatedDate = _project.ModifiedDate.Value
        //////        });

        //////        _dataAccess.CommitTransaction();


        //////        // raise event that Task status was changed
        //////        _eventBus.Publish(new ProjectCompletedEventArgs(project.ProjectNo, project.ModifiedBy, project.ModifiedDate.Value, remarks));

        //////    }
        //////    catch (Exception ex)
        //////    {
        //////        _dataAccess.RollbackTransaction();
        //////        throw;
        //////    }
        //////}


    }

}
