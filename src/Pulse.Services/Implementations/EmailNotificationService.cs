using Pulse.Core;
using Pulse.Core.Entities;
using Pulse.Core.EventArgs;
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class EmailNotificationService : IEventSubscriber<ProjectCreatedEventArgs>, IEventSubscriber<ProjectHoldEventArgs>, IEventSubscriber<ProjectCanceledEventArgs>, IEventSubscriber<ProjectResumedEventArgs>,
        IEventSubscriber<ProjectMilestoneNotStartedEventArgs>, IEventSubscriber<ProjectMilestoneStartedEventArgs>, IEventSubscriber<ProjectMilestoneHoldEventArgs>, IEventSubscriber<ProjectMilestoneResumedEventArgs>, IEventSubscriber<ProjectMilestoneCanceledEventArgs>, IEventSubscriber<ProjectMilestoneCompletedEventArgs>,
        IEventSubscriber<ProjectTaskNotStartedEventArgs>, IEventSubscriber<ProjectTaskStartedEventArgs>, IEventSubscriber<ProjectTaskHoldEventArgs>, IEventSubscriber<ProjectTaskResumedEventArgs>, IEventSubscriber<ProjectTaskCanceledEventArgs>, IEventSubscriber<ProjectTaskCompletedEventArgs>
    {
        ////private readonly ITaskRepository _taskRepository;
        ////private readonly ITaskMemberRepository _taskmemberRepository;
        private readonly IStatusChangeRepository _statuschangeRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectmemberRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProjectProductRepository _projectproductRepository;
        private readonly IProjectMilestoneRepository _projectmilestoneRepository;
        private readonly IProjectTaskRepository _projecttaskRepository;
        private readonly IRoadmapMilestoneRepository _roadmapMilestoneRepository;
        private readonly IRoadmapActivityRepository _roadmapActivityRepository;
        private readonly INotificationRepository _notificationRepository;
        //private readonly IProjectNotificationRepository _projectNotificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserGroupMemberRepository _usergroupmemberRepository;
        private readonly IActiveDirectoryGroupRepository _activedirectorygroupRepository;
        private readonly IEmailSender _emailSender;


        public EmailNotificationService(//ITaskRepository taskRepository, ITaskMemberRepository taskmemberRepository, 
            IStatusChangeRepository statuschangeRepository,
            IProjectRepository projectRepository, IProjectMemberRepository projectmemberRepository, IProductRepository productRepository, IProjectProductRepository projectproductRepository, IProjectMilestoneRepository projectmilestoneRepository,
            IProjectTaskRepository projecttaskRepository, IRoadmapMilestoneRepository roadmapMilestoneRepository, IRoadmapActivityRepository roadmapActivityRepository, INotificationRepository notificationRepository,  //IProjectNotificationRepository projectNotificationRepository,
            IUserRepository userRepository, IUserGroupMemberRepository usergroupmemberRepository, IActiveDirectoryGroupRepository activedirectorygroupRepository, IEmailSender emailSender)
        {
            //_taskRepository = taskRepository;
            //_taskmemberRepository = taskmemberRepository;
            _statuschangeRepository = statuschangeRepository;
            _projectRepository = projectRepository;
            _projectmemberRepository = projectmemberRepository;
            _projectproductRepository = projectproductRepository;
            _projectmilestoneRepository = projectmilestoneRepository;
            _projecttaskRepository = projecttaskRepository;
            _roadmapMilestoneRepository = roadmapMilestoneRepository;
            _roadmapActivityRepository = roadmapActivityRepository;
            _notificationRepository = notificationRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _usergroupmemberRepository = usergroupmemberRepository;
            _activedirectorygroupRepository = activedirectorygroupRepository;
            _emailSender = emailSender;
        }



        // Handle ProjectCreatedEventArgs
        public async Task Handle(ProjectCreatedEventArgs eventMessage)
        {
            var project = await _projectRepository.GetAsync(eventMessage.ProjectNo);
            var products = string.Join(", ", (await _projectproductRepository.GetListAsync(project.ProjectNo)).Select(p => p.ProductCode));
            var createdby = await _userRepository.GetAsync(eventMessage.CreatedBy);


            var emails = new List<string>();
            // get all project members
            var memberGetTasks = (await _projectmemberRepository.GetListAsync(eventMessage.ProjectNo)).Select(m => _userRepository.GetAsync(m.UserId));

            var listEmails = (await Task.WhenAll(memberGetTasks)).Select(m => m.Email);

            emails.AddRange(listEmails);
 
            await _emailSender.SendProjectCreatedNotificationAsync(eventMessage.ProjectNo, project.ProjectName, products, project.PlantCode, project.CategoryCode, createdby.FirstName + " " + createdby.LastName, eventMessage.CreatedDate, emails, "".Split(','));


        }


        // Handle ProjectHoldEventArgs
        public async Task Handle(ProjectHoldEventArgs eventMessage)
        {
            var project = await _projectRepository.GetAsync(eventMessage.ProjectNo);
            var products = string.Join(", ", (await _projectproductRepository.GetListAsync(project.ProjectNo)).Select(p => p.ProductCode));
            var loggedUser = await _userRepository.GetAsync(eventMessage.UpdatedBy);


            var emails = new List<string>();
            // get all project members
            var memberGetTasks = (await _projectmemberRepository.GetListAsync(eventMessage.ProjectNo)).Select(m => _userRepository.GetAsync(m.UserId));

            var listEmails = (await Task.WhenAll(memberGetTasks)).Select(m => m.Email);

            emails.AddRange(listEmails);

            await _emailSender.SendStatusChangeOnProjectNotificationAsync(eventMessage.ProjectNo, project.ProjectName, products, project.PlantCode, project.CategoryCode, eventMessage.FromStatus, eventMessage.ToStatus, eventMessage.Reason, loggedUser.FirstName + " " + loggedUser.LastName, project.ModifiedDate.Value, emails, "".Split(','));


        }

        public async Task Handle(ProjectResumedEventArgs eventMessage)
        {
            var project = await _projectRepository.GetAsync(eventMessage.ProjectNo);
            var products = string.Join(", ", (await _projectproductRepository.GetListAsync(project.ProjectNo)).Select(p => p.ProductCode));
            var loggedUser = await _userRepository.GetAsync(eventMessage.ContinuedBy);


            var emails = new List<string>();
            // get all project members
            var memberGetTasks = (await _projectmemberRepository.GetListAsync(eventMessage.ProjectNo)).Select(m => _userRepository.GetAsync(m.UserId));

            var listEmails = (await Task.WhenAll(memberGetTasks)).Select(m => m.Email);

            emails.AddRange(listEmails);

            await _emailSender.SendStatusChangeOnProjectNotificationAsync(eventMessage.ProjectNo, project.ProjectName, products, project.PlantCode, project.CategoryCode, "HOLD", "ONGOING", eventMessage.Reason, loggedUser.FirstName + " " + loggedUser.LastName, project.ModifiedDate.Value, emails, "".Split(','));


        }

        public async Task Handle(ProjectCanceledEventArgs eventMessage)
        {
            var project = await _projectRepository.GetAsync(eventMessage.ProjectNo);
            var products = string.Join(", ", (await _projectproductRepository.GetListAsync(project.ProjectNo)).Select(p => p.ProductCode));
            var loggedUser = await _userRepository.GetAsync(eventMessage.CanceledBy);


            var emails = new List<string>();
            // get all project members
            var memberGetTasks = (await _projectmemberRepository.GetListAsync(eventMessage.ProjectNo)).Select(m => _userRepository.GetAsync(m.UserId));

            var listEmails = (await Task.WhenAll(memberGetTasks)).Select(m => m.Email);

            emails.AddRange(listEmails);

            await _emailSender.SendStatusChangeOnProjectNotificationAsync(eventMessage.ProjectNo, project.ProjectName, products, project.PlantCode, project.CategoryCode, "-", "CANCELLED", eventMessage.Reason, loggedUser.FirstName + " " + loggedUser.LastName, project.ModifiedDate.Value, emails, "".Split(','));


        }

        public async Task Handle(ProjectMilestoneNotStartedEventArgs eventMessage)
        {
            await NotifyMilestoneStatusChangeAsync(eventMessage.SysId, eventMessage.NotStartedBy, eventMessage.NotStartedDate, "-", "NOT STARTED", eventMessage.Reason);
        }

        public async Task Handle(ProjectMilestoneStartedEventArgs eventMessage)
        {
            await NotifyMilestoneStatusChangeAsync(eventMessage.SysId, eventMessage.StartedBy, eventMessage.StartedDate, "NOT STARTED", "ONGOING", eventMessage.Remarks);
        }

        public async Task Handle(ProjectMilestoneHoldEventArgs eventMessage)
        {
            await NotifyMilestoneStatusChangeAsync(eventMessage.SysId, eventMessage.HoldBy, eventMessage.HoldDate, "ONGOING", "HOLD", eventMessage.Reason);
        }

        public async Task Handle(ProjectMilestoneResumedEventArgs eventMessage)
        {
            await NotifyMilestoneStatusChangeAsync(eventMessage.SysId, eventMessage.ContinuedBy, eventMessage.ContinuedDate, eventMessage.PreviousStatus.ToString().ToUpperInvariant(), "ONGOING", eventMessage.Reason);
        }

        public async Task Handle(ProjectMilestoneCanceledEventArgs eventMessage)
        {
            await NotifyMilestoneStatusChangeAsync(eventMessage.SysId, eventMessage.CanceledBy, eventMessage.CanceledDate, "-", "CANCELLED", eventMessage.Reason);
        }

        public async Task Handle(ProjectMilestoneCompletedEventArgs eventMessage)
        {
            await NotifyMilestoneStatusChangeAsync(eventMessage.SysId, eventMessage.CompletedBy, eventMessage.CompletedDate, "ONGOING", "COMPLETED", eventMessage.Remarks);
        }

        public async Task Handle(ProjectTaskNotStartedEventArgs eventMessage)
        {
            await NotifyTaskStatusChangeAsync(eventMessage.SysId, eventMessage.NotStartedBy, eventMessage.NotStartedDate, "-", "NOT STARTED", eventMessage.Reason);
        }

        public async Task Handle(ProjectTaskStartedEventArgs eventMessage)
        {
            await NotifyTaskStatusChangeAsync(eventMessage.SysId, eventMessage.StartedBy, eventMessage.StartedDate, "NOT STARTED", "ONGOING", eventMessage.Remarks);
        }

        public async Task Handle(ProjectTaskHoldEventArgs eventMessage)
        {
            await NotifyTaskStatusChangeAsync(eventMessage.SysId, eventMessage.HoldBy, eventMessage.HoldDate, "ONGOING", "HOLD", eventMessage.Reason);
        }

        public async Task Handle(ProjectTaskResumedEventArgs eventMessage)
        {
            await NotifyTaskStatusChangeAsync(eventMessage.SysId, eventMessage.ContinuedBy, eventMessage.ContinuedDate, eventMessage.PreviousStatus.ToString().ToUpperInvariant(), "ONGOING", eventMessage.Reason);
        }

        public async Task Handle(ProjectTaskCanceledEventArgs eventMessage)
        {
            await NotifyTaskStatusChangeAsync(eventMessage.SysId, eventMessage.CanceledBy, eventMessage.CanceledDate, "-", "CANCELLED", eventMessage.Reason);
        }

        public async Task Handle(ProjectTaskCompletedEventArgs eventMessage)
        {
            await NotifyTaskStatusChangeAsync(eventMessage.SysId, eventMessage.CompletedBy, eventMessage.CompletedDate, "ONGOING", "COMPLETED", eventMessage.Remarks);
        }

        private async Task NotifyMilestoneStatusChangeAsync(string milestoneSysId, string changedBy, DateTime changedDate, string fromStatus, string toStatus, string reason)
        {
            var milestone = await _projectmilestoneRepository.GetAsync(milestoneSysId);
            if (milestone == null)
            {
                return;
            }

            var project = await _projectRepository.GetAsync(milestone.ProjectNo);
            if (project == null)
            {
                return;
            }

            var milestoneName = await ResolveProjectMilestoneNameAsync(project.ProjectNo, milestone) ?? "Milestone";
            var products = string.Join(", ", (await _projectproductRepository.GetListAsync(project.ProjectNo)).Select(p => p.ProductCode));
            var loggedUser = await _userRepository.GetAsync(changedBy);
            var recipientEmails = await GetProjectMemberEmailsAsync(project.ProjectNo);

            if (recipientEmails.Count == 0)
            {
                return;
            }

            var changedByDisplayName = loggedUser == null ? changedBy : $"{loggedUser.FirstName} {loggedUser.LastName}".Trim();
            var normalizedReason = string.IsNullOrWhiteSpace(reason) ? "-" : reason;

            await _notificationRepository.AddAsync(new Core.Entities.Notification
            {
                //ProjectNo = project.ProjectNo,
                Title = $"Milestone status updated: {milestoneName}",
                Message = $"Milestone '{milestoneName}' changed from {fromStatus} to {toStatus} by {changedByDisplayName}. Reason: {normalizedReason}",
                Recipients = string.Join(";", recipientEmails),
                NotificationDate = changedDate,
                EntityType = "milestone",
                EntitySysId = milestone.MilestoneSysId,
                CreatedBy = changedBy
            });

            await _emailSender.SendStatusChangeOnMilestoneNotificationAsync(project.ProjectNo, project.ProjectName, products, project.PlantCode, project.CategoryCode, milestoneName, fromStatus, toStatus, normalizedReason, changedByDisplayName, changedDate, recipientEmails, new string[0]);
        }

        private async Task NotifyTaskStatusChangeAsync(string taskSysId, string changedBy, DateTime changedDate, string fromStatus, string toStatus, string reason)
        {
            var task = await _projecttaskRepository.GetAsync(taskSysId);
            if (task == null)
            {
                return;
            }

            var project = await _projectRepository.GetAsync(task.ProjectNo);
            if (project == null)
            {
                return;
            }

            var roadmapActivity = !string.IsNullOrWhiteSpace(task.RoadmapActivitySysId)
                ? await _roadmapActivityRepository.GetAsync(task.RoadmapActivitySysId)
                : null;
            var milestone = ResolveTaskMilestone(task);
            var taskName = task.AltTaskName ?? roadmapActivity?.ActivityName ?? "Task";
            var milestoneName = milestone == null ? string.Empty : await ResolveProjectMilestoneNameAsync(project.ProjectNo, milestone) ?? string.Empty;
            var products = string.Join(", ", (await _projectproductRepository.GetListAsync(project.ProjectNo)).Select(p => p.ProductCode));
            var loggedUser = await _userRepository.GetAsync(changedBy);
            var recipientEmails = await GetProjectMemberEmailsAsync(project.ProjectNo);

            if (recipientEmails.Count == 0)
            {
                return;
            }

            var changedByDisplayName = loggedUser == null ? changedBy : $"{loggedUser.FirstName} {loggedUser.LastName}".Trim();
            var normalizedReason = string.IsNullOrWhiteSpace(reason) ? "-" : reason;

            await _notificationRepository.AddAsync(new Core.Entities.Notification
            {
                //ProjectNo = project.ProjectNo,
                Title = $"Task status updated: {taskName}",
                Message = $"Task '{taskName}' changed from {fromStatus} to {toStatus} by {changedByDisplayName}. Reason: {normalizedReason}",
                Recipients = string.Join(";", recipientEmails),
                NotificationDate = changedDate,
                EntityType = "activity",
                EntitySysId = task.ProjectTaskSysId,
                CreatedBy = changedBy
            });

            await _emailSender.SendStatusChangeOnTaskNotificationAsync(project.ProjectNo, project.ProjectName, products, project.PlantCode, project.CategoryCode, milestoneName, taskName, fromStatus, toStatus, normalizedReason, changedByDisplayName, changedDate, recipientEmails, new string[0]);
        }

        private async Task<string> ResolveProjectMilestoneNameAsync(string projectNo, ProjectMilestone milestone)
        {
            if (string.IsNullOrWhiteSpace(projectNo) || milestone == null || string.IsNullOrWhiteSpace(milestone.RoadmapMilestoneSysId))
            {
                return null;
            }

            var milestoneNode = await _projectRepository.GetProjectNodeItemAsync(projectNo, "milestone", milestone.RoadmapMilestoneSysId);
            if (milestoneNode != null && !string.IsNullOrWhiteSpace(milestoneNode.NodeName))
            {
                return milestoneNode.NodeName;
            }

            var roadmapMilestone = await _roadmapMilestoneRepository.GetAsync(milestone.RoadmapMilestoneSysId);
            return roadmapMilestone?.MilestoneAlias;
        }

        private async Task<List<string>> GetProjectMemberEmailsAsync(string projectNo)
        {
            var memberGetTasks = (await _projectmemberRepository.GetListAsync(projectNo)).Select(m => _userRepository.GetAsync(m.UserId));
            return (await Task.WhenAll(memberGetTasks))
                .Select(m => m?.Email)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private ProjectMilestone ResolveTaskMilestone(ProjectTask task)
        {
            if (task == null)
            {
                return null;
            }

            if (string.Equals(task.ParentType, "MILESTONE", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(task.ParentSysId))
            {
                return _projectmilestoneRepository.GetAsync(task.ParentSysId).GetAwaiter().GetResult();
            }

            if (!string.Equals(task.ParentType, "TASK", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(task.ParentSysId))
            {
                return null;
            }

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var currentTask = task;
            while (currentTask != null && !string.IsNullOrWhiteSpace(currentTask.ParentSysId) && visited.Add(currentTask.ProjectTaskSysId ?? currentTask.ParentSysId))
            {
                if (string.Equals(currentTask.ParentType, "MILESTONE", StringComparison.OrdinalIgnoreCase))
                {
                    return _projectmilestoneRepository.GetAsync(currentTask.ParentSysId).GetAwaiter().GetResult();
                }

                if (!string.Equals(currentTask.ParentType, "TASK", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                currentTask = _projecttaskRepository.GetAsync(currentTask.ParentSysId).GetAwaiter().GetResult();
            }

            return null;
        }



        ////// Handle TaskStatusChangeEventArgs
        ////public async Task Handle(TaskStatusChangeEventArgs eventMessage)
        ////{
        ////    var taskStatusChanged = await _taskRepository.GetAsync(eventMessage.TaskSysId);
        ////    var createdby = await _userRepository.GetAsync(eventMessage.ChangedById);
        ////    var project = await _projectRepository.GetAsync(taskStatusChanged.ProjectNo);
        ////    var products = string.Join(", ", (await _projectproductRepository.GetListAsync(taskStatusChanged.ProjectNo)).Select(p => p.ProductCode));
        ////    var milestone = await _projectmilestoneRepository.GetAsync(taskStatusChanged.MilestoneSysId);



        ////    var emails = new List<string>();
        ////    // get all project members
        ////    var memberGetTasks = (await _projectmemberRepository.GetListAsync(taskStatusChanged.ProjectNo)).Select(m => _userRepository.GetAsync(m.UserId));

        ////    var listEmails = (await Task.WhenAll(memberGetTasks)).Select(m => m.Email);

        ////    emails.AddRange(listEmails);

        ////    // get all task members - user
        ////    var taskmemberlist = await _taskmemberRepository.GetListAsync(taskStatusChanged.ProjectNo);

        ////    memberGetTasks = taskmemberlist.Where(m => m.UserId != null).Select(m => _userRepository.GetAsync(m.UserId));
        ////    listEmails = (await Task.WhenAll(memberGetTasks)).Select(m => m.Email);

        ////    emails.AddRange(listEmails);

        ////    // get all task members - usergroup
        ////    var groupmemberGetTasks = taskmemberlist.Where(m => m.UserGroupId != null).Select(m => _usergroupmemberRepository.GetListAsync(m.UserGroupId.ToString()));
        ////    var listgroupmembers = await Task.WhenAll(groupmemberGetTasks);

        ////    memberGetTasks = listgroupmembers.SelectMany(m => m.Select(x => _userRepository.GetAsync(x.UserId)));
        ////    listEmails = (await Task.WhenAll(memberGetTasks)).Select(m => m.Email);


        ////    // get all task members - adgroup
        ////    var adgroupmemberGetTasks = taskmemberlist.Where(m => m.ADGroup != null).Select(m => _activedirectorygroupRepository.GetAsync(m.ADGroup));
        ////    listEmails = (await Task.WhenAll(adgroupmemberGetTasks)).Select(m => m.Email);
        ////    emails.AddRange(listEmails);


        ////    await _emailSender.SendStatusChangeOnTaskNotificationAsync(taskStatusChanged.ProjectNo, project.ProjectName, products, project.PlantCode, project.CategoryCode, milestone == null ? "" : milestone.MaturityCode, taskStatusChanged.TaskName,
        ////          eventMessage.FromStatus.ToString(), eventMessage.ToStatus.ToString(), eventMessage.Reason, createdby.FirstName + " " + createdby.LastName, eventMessage.DateChanged, emails, createdby.Email.Split(','));
        ////}
    }
}
