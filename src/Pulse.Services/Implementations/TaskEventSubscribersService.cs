using Pulse.Core;
using Pulse.Core.Enums;
using Pulse.Core.EventArgs;
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class TaskEventSubscribersService// : IEventSubscriber<TaskCanceledEventArgs>, IEventSubscriber<TaskCompletedEventArgs>
        //, IEventSubscriber<TaskContinuedEventArgs>, IEventSubscriber<TaskCreatedEventArgs>
        //, IEventSubscriber<TaskFailedEventArgs>, IEventSubscriber<TaskHoldEventArgs>
        //, IEventSubscriber<TaskStartedEventArgs>, IEventSubscriber<TaskUpdatedEventArgs>
    {
        //private readonly ITaskRepository _taskRepository;
        //private readonly ITaskMemberRepository _taskmemberRepository;
        private readonly IStatusChangeRepository _statuschangeRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectmemberRepository;
        private readonly IProjectProductRepository _projectproductRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProjectMilestoneRepository _projectmilestoneRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserGroupMemberRepository _usergroupmemberRepository;
        private readonly IActiveDirectoryGroupRepository _activedirectorygroupRepository;
        private readonly IEmailSender _emailSender;


        public TaskEventSubscribersService(//ITaskRepository taskRepository, ITaskMemberRepository taskmemberRepository, 
            IStatusChangeRepository statuschangeRepository,
            IProjectRepository projectRepository, IProjectMemberRepository projectmemberRepository, IProjectProductRepository projectproductRepository, IProductRepository productRepository, IProjectMilestoneRepository projectmilestoneRepository,
            IUserRepository userRepository, IUserGroupMemberRepository usergroupmemberRepository, IActiveDirectoryGroupRepository activedirectorygroupRepository, IEmailSender emailSender)
        {
            //_taskRepository = taskRepository;
            //_taskmemberRepository = taskmemberRepository;
            _statuschangeRepository = statuschangeRepository;
            _projectRepository = projectRepository;
            _projectmemberRepository = projectmemberRepository;
            _projectproductRepository = projectproductRepository;
            _productRepository = productRepository;
            _projectmilestoneRepository = projectmilestoneRepository;
            _userRepository = userRepository;
            _usergroupmemberRepository = usergroupmemberRepository;
            _activedirectorygroupRepository = activedirectorygroupRepository;
            _emailSender = emailSender;
        }


        //////public async Task Handle(TaskCanceledEventArgs eventMessage)
        //////{
        //////    await this.NotifyStatusChange(eventMessage.TaskSysId, eventMessage.PreviousStatus, Status.Canceled, eventMessage.CanceledBy, eventMessage.CanceledDate, eventMessage.Reason);
        //////}
        //////public async Task Handle(TaskCompletedEventArgs eventMessage)
        //////{
        //////    await this.NotifyStatusChange(eventMessage.TaskSysId, eventMessage.PreviousStatus, Status.Completed, eventMessage.CompletedBy, eventMessage.CompletedDate, eventMessage.Remarks);
        //////}
        //////public async Task Handle(TaskContinuedEventArgs eventMessage)
        //////{
        //////    await this.NotifyStatusChange(eventMessage.TaskSysId, eventMessage.PreviousStatus, Status.Ongoing, eventMessage.ContinuedBy, eventMessage.ContinuedDate, eventMessage.Reason);
        //////}
        //////public async Task Handle(TaskCreatedEventArgs eventMessage)
        //////{
        //////    var taskStatusChanged = await _taskRepository.GetAsync(eventMessage.TaskSysId);
        //////    var createdby = await _userRepository.GetAsync(eventMessage.CreatedBy);
        //////    var project = await _projectRepository.GetAsync(taskStatusChanged.ProjectNo);
        //////    var products = string.Join(", ", (await _projectproductRepository.GetListAsync(taskStatusChanged.ProjectNo)).Select(p => p.ProductCode));
        //////    var milestone = await _projectmilestoneRepository.GetAsync(taskStatusChanged.MilestoneSysId);

        //////    var emails = new List<string>();
        //////    // get all project members
        //////    var memberGetTasks = (await _projectmemberRepository.GetListAsync(taskStatusChanged.ProjectNo)).Select(m => _userRepository.GetAsync(m.UserId));

        //////    var listEmails = (await Task.WhenAll(memberGetTasks)).Select(m => m.Email);

        //////    emails.AddRange(listEmails);

        //////    // get all task members - user
        //////    var taskmemberlist = await _taskmemberRepository.GetListAsync(taskStatusChanged.ProjectNo);

        //////    memberGetTasks = taskmemberlist.Where(m => m.UserId != null).Select(m => _userRepository.GetAsync(m.UserId));
        //////    listEmails = (await Task.WhenAll(memberGetTasks)).Select(m => m.Email);

        //////    emails.AddRange(listEmails);

        //////    // get all task members - usergroup
        //////    var groupmemberGetTasks = taskmemberlist.Where(m => m.UserGroupId != null).Select(m => _usergroupmemberRepository.GetListAsync(m.UserGroupId.ToString()));
        //////    var listgroupmembers = await Task.WhenAll(groupmemberGetTasks);

        //////    memberGetTasks = listgroupmembers.SelectMany(m => m.Select(x => _userRepository.GetAsync(x.UserId)));
        //////    listEmails = (await Task.WhenAll(memberGetTasks)).Select(m => m.Email);


        //////    // get all task members - adgroup
        //////    var adgroupmemberGetTasks = taskmemberlist.Where(m => m.ADGroup != null).Select(m => _activedirectorygroupRepository.GetAsync(m.ADGroup));
        //////    listEmails = (await Task.WhenAll(adgroupmemberGetTasks)).Select(m => m.Email);
        //////    emails.AddRange(listEmails);


        //////    await _emailSender.SendTaskCreatedNotificationAsync(taskStatusChanged.ProjectNo, project.ProjectName, products, project.PlantCode, project.CategoryCode, milestone == null ? "" : milestone.MaturityCode, taskStatusChanged.TaskName,
        //////           createdby.FirstName + " " + createdby.LastName, eventMessage.CreatedDate, emails, createdby.Email.Split(','));
        //////}
        //////public async Task Handle(TaskFailedEventArgs eventMessage)
        //////{
        //////    await this.NotifyStatusChange(eventMessage.TaskSysId, eventMessage.PreviousStatus, Status.Failed, eventMessage.FailedBy, eventMessage.FailedDate, eventMessage.Reason);
        //////}
        //////public async Task Handle(TaskHoldEventArgs eventMessage)
        //////{
        //////    await this.NotifyStatusChange(eventMessage.TaskSysId, eventMessage.PreviousStatus, Status.Hold, eventMessage.HoldBy, eventMessage.HoldDate, eventMessage.Reason);
        //////}
        //////public async Task Handle(TaskStartedEventArgs eventMessage)
        //////{
        //////    await this.NotifyStatusChange(eventMessage.TaskSysId, null, Status.Ongoing, eventMessage.StartedBy, eventMessage.StartedDate, eventMessage.Remarks);
        //////}

        public Task Handle(TaskUpdatedEventArgs eventMessage)
        {
            throw new NotImplementedException();
        }

        //////private async Task NotifyStatusChange(string taskSysId, Status? fromStatus, Status toStatus, string changedById, DateTime dateChanged, string reason)
        //////{
        //////    var taskStatusChanged = await _taskRepository.GetAsync(taskSysId);
        //////    var createdby = await _userRepository.GetAsync(changedById);
        //////    var project = await _projectRepository.GetAsync(taskStatusChanged.ProjectNo);
        //////    var products = string.Join(", ", (await _projectproductRepository.GetListAsync(taskStatusChanged.ProjectNo)).Select(p => p.ProductCode));
        //////    var milestone = await _projectmilestoneRepository.GetAsync(taskStatusChanged.MilestoneSysId);



        //////    var emails = new List<string>();
        //////    // get all project members
        //////    var memberGetTasks = (await _projectmemberRepository.GetListAsync(taskStatusChanged.ProjectNo)).Select(m => _userRepository.GetAsync(m.UserId));

        //////    var listEmails = (await Task.WhenAll(memberGetTasks)).Select(m => m.Email);

        //////    emails.AddRange(listEmails);

        //////    // get all task members - user
        //////    var taskmemberlist = await _taskmemberRepository.GetListAsync(taskStatusChanged.ProjectNo);

        //////    memberGetTasks = taskmemberlist.Where(m => m.UserId != null).Select(m => _userRepository.GetAsync(m.UserId));
        //////    listEmails = (await Task.WhenAll(memberGetTasks)).Select(m => m.Email);

        //////    emails.AddRange(listEmails);

        //////    // get all task members - usergroup
        //////    var groupmemberGetTasks = taskmemberlist.Where(m => m.UserGroupId != null).Select(m => _usergroupmemberRepository.GetListAsync(m.UserGroupId.ToString()));
        //////    var listgroupmembers = await Task.WhenAll(groupmemberGetTasks);

        //////    memberGetTasks = listgroupmembers.SelectMany(m => m.Select(x => _userRepository.GetAsync(x.UserId)));
        //////    listEmails = (await Task.WhenAll(memberGetTasks)).Select(m => m.Email);


        //////    // get all task members - adgroup
        //////    var adgroupmemberGetTasks = taskmemberlist.Where(m => m.ADGroup != null).Select(m => _activedirectorygroupRepository.GetAsync(m.ADGroup));
        //////    listEmails = (await Task.WhenAll(adgroupmemberGetTasks)).Select(m => m.Email);
        //////    emails.AddRange(listEmails);


        //////    await _emailSender.SendStatusChangeOnTaskNotificationAsync(taskStatusChanged.ProjectNo, project.ProjectName, products, project.PlantCode, project.CategoryCode, milestone == null ? "" : milestone.MaturityCode, taskStatusChanged.TaskName,
        //////          fromStatus.ToString(), toStatus.ToString(), reason, createdby.FirstName + " " + createdby.LastName, dateChanged, emails, createdby.Email.Split(','));
        //////}


    }
}
