using Pulse.Core;
using Pulse.Core.EventArgs;
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class ProjectFieldEventSubscribersService : IEventSubscriber<ProjectFieldCreatedEventArgs>, IEventSubscriber<ProjectFieldUpdatedEventArgs>, IEventSubscriber<ProjectFieldDeletedEventArgs> 
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ITaskMemberRepository _taskmemberRepository;
        private readonly IStatusChangeRepository _statuschangeRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectmemberRepository;
        private readonly IProductRepository _projectproductRepository;
        private readonly IProjectMilestoneRepository _projectmilestoneRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserGroupMemberRepository _usergroupmemberRepository;
        private readonly IActiveDirectoryGroupRepository _activedirectorygroupRepository;
        private readonly IEmailSender _emailSender;


        public ProjectFieldEventSubscribersService(ITaskRepository taskRepository, ITaskMemberRepository taskmemberRepository, IStatusChangeRepository statuschangeRepository,
            IProjectRepository projectRepository, IProjectMemberRepository projectmemberRepository, IProductRepository projectproductRepository, IProjectMilestoneRepository projectmilestoneRepository,
            IUserRepository userRepository, IUserGroupMemberRepository usergroupmemberRepository, IActiveDirectoryGroupRepository activedirectorygroupRepository, IEmailSender emailSender)
        {
            _taskRepository = taskRepository;
            _taskmemberRepository = taskmemberRepository;
            _statuschangeRepository = statuschangeRepository;
            _projectRepository = projectRepository;
            _projectmemberRepository = projectmemberRepository;
            _projectproductRepository = projectproductRepository;
            _projectmilestoneRepository = projectmilestoneRepository;
            _userRepository = userRepository;
            _usergroupmemberRepository = usergroupmemberRepository;
            _activedirectorygroupRepository = activedirectorygroupRepository;
            _emailSender = emailSender;
        }

        public Task Handle(ProjectFieldCreatedEventArgs eventMessage)
        {
            throw new NotImplementedException();
        }

        public Task Handle(ProjectFieldUpdatedEventArgs eventMessage)
        {
            throw new NotImplementedException();
        }

        public Task Handle(ProjectFieldDeletedEventArgs eventMessage)
        {
            throw new NotImplementedException();
        }
    }
}
