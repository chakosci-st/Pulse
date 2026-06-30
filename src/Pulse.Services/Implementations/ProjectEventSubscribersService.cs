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
    public class ProjectEventSubscribersService : IEventSubscriber<ProjectCreatedEventArgs>,  IEventSubscriber<ProjectCompletedEventArgs>, IEventSubscriber<ProjectFailedEventArgs>,
        //
        IEventSubscriber<ProjectStartedEventArgs>, IEventSubscriber<ProjectNotStartedEventArgs>,  IEventSubscriber<ProjectUpdatedEventArgs>,
        IEventSubscriber<ProjectDeletedEventArgs>, IEventSubscriber<ProjectPromotedEventArgs>
    {
        //private readonly ITaskRepository _taskRepository;
        //private readonly ITaskMemberRepository _taskmemberRepository;
        ////private readonly IStatusChangeRepository _statuschangeRepository;
        //private readonly IProjectRepository _projectRepository;
        ////
        ////private readonly IProjectProductRepository _projectproductRepository;
        ////private readonly IProductRepository _productRepository;
        ////private readonly IProjectMilestoneRepository _projectmilestoneRepository;
        
        ////private readonly IUserGroupMemberRepository _usergroupmemberRepository;
        ////private readonly IActiveDirectoryGroupRepository _activedirectorygroupRepository;
        ///
        ////private readonly IProjectRepository _projectRepository;
        ////private readonly IProjectProductRepository _projectproductRepository;
        ////private readonly IUserRepository _userRepository;
        ////private readonly IProjectMemberRepository _projectmemberRepository;
        private readonly IEmailSender _emailSender;


        public ProjectEventSubscribersService() { }

        public ProjectEventSubscribersService(
            IEmailSender emailSender
            ////, IProjectRepository projectRepository, IProjectProductRepository projectproductRepository, 
            ////IUserRepository userRepository, IProjectMemberRepository projectmemberRepository
            //ITaskRepository taskRepository, ITaskMemberRepository taskmemberRepository, 
            //////,IStatusChangeRepository statuschangeRepository,
            //////, IProductRepository productRepository,  IProjectMilestoneRepository projectmilestoneRepository,
            ////// IUserGroupMemberRepository usergroupmemberRepository, IActiveDirectoryGroupRepository activedirectorygroupRepository
            )
        {
            //_taskRepository = taskRepository;
            //_taskmemberRepository = taskmemberRepository;
            //////_statuschangeRepository = statuschangeRepository;

            //////
            //////
            //////_productRepository = productRepository;
            //////_projectmilestoneRepository = projectmilestoneRepository;
            //////
            //////_usergroupmemberRepository = usergroupmemberRepository;
            //////_activedirectorygroupRepository = activedirectorygroupRepository;
            ////_projectRepository = projectRepository;
            ////_projectproductRepository = projectproductRepository;
            ////_userRepository = userRepository;
            ////_projectmemberRepository = projectmemberRepository;
            _emailSender = emailSender;
        }



        public async Task Handle(ProjectNotStartedEventArgs eventMessage)
        {
            await _emailSender.SendProjectCreatedNotificationAsync(eventMessage.ProjectNo, eventMessage.ProjectName,
     eventMessage.ProductCode, eventMessage.Plant, eventMessage.Category, eventMessage.StartedBy, eventMessage.StartedDate, eventMessage.RecipientEmail.Split(','), "".Split(','));
        }
        public async Task Handle(ProjectStartedEventArgs eventMessage)
        { 
            await _emailSender.SendProjectCreatedAndStartedNotificationAsync(eventMessage.ProjectNo, eventMessage.ProjectName,
                 eventMessage.ProductCode, eventMessage.Plant, eventMessage.Category, eventMessage.StartedBy, eventMessage.Milestone,  eventMessage.StartedDate, eventMessage.RecipientEmail.Split(','), "".Split(','));

        }


        public async Task Handle(ProjectCreatedEventArgs eventMessage)
        {
            ////
            ////var products = string.Join(", ", (await _projectproductRepository.GetListAsync(project.ProjectNo)).Select(p => p.ProductCode));
            ////var createdby = await _userRepository.GetAsync(eventMessage.CreatedBy);

            ////var emails = new List<string>();
            ////// get all project members
            ////var memberGetTasks = (await _projectmemberRepository.GetListAsync(eventMessage.ProjectNo)).Select(m => _userRepository.GetAsync(m.UserId));

            ////var listEmails = (await Task.WhenAll(memberGetTasks)).Select(m => m.Email);

            ////emails.AddRange(listEmails);

            ////await _emailSender.SendProjectCreatedNotificationAsync(eventMessage.ProjectNo, project.ProjectName, products, project.PlantCode, project.CategoryCode, createdby.FirstName + " " + createdby.LastName, eventMessage.CreatedDate, emails, "".Split(','));

            await _emailSender.SendProjectCreatedNotificationAsync(eventMessage.ProjectNo, "", "", "", "", "", eventMessage.CreatedDate, "charles.legaspi@st.com".Split(','), "".Split(','));
        }


        public Task Handle(ProjectCanceledEventArgs eventMessage)
        {
            throw new NotImplementedException();
        }

        public Task Handle(ProjectCompletedEventArgs eventMessage)
        {
            // Some deployments resolve this subscriber for project completion events.
            // Keep completion flow non-blocking until a concrete completion action is required.
            return Task.CompletedTask;
        }


        public Task Handle(ProjectFailedEventArgs eventMessage)
        {
            throw new NotImplementedException();
        }

        ////public async Task Handle(ProjectHoldEventArgs eventMessage)
        ////{
        ////    ////var project = await _projectRepository.GetAsync(eventMessage.ProjectNo);
        ////    ////var products = string.Join(", ", (await _projectproductRepository.GetListAsync(project.ProjectNo)).Select(p => p.ProductCode));
        ////    ////var loggedUser = await _userRepository.GetAsync(eventMessage.UpdatedBy);


        ////    ////var emails = new List<string>();
        ////    ////// get all project members
        ////    ////var memberGetTasks = (await _projectmemberRepository.GetListAsync(eventMessage.ProjectNo)).Select(m => _userRepository.GetAsync(m.UserId));

        ////    ////var listEmails = (await Task.WhenAll(memberGetTasks)).Select(m => m.Email);

        ////    ////emails.AddRange(listEmails);

        ////    await _emailSender.SendStatusChangeOnProjectNotificationAsync(eventMessage.ProjectNo,"", "","", "", "-", "CANCELLED", eventMessage.Reason, loggedUser.FirstName + " " + loggedUser.LastName, project.ModifiedDate.Value, emails, "".Split(','));

        ////}




        public Task Handle(ProjectResumedEventArgs eventMessage)
        {
            throw new NotImplementedException();
        }

        public async Task Handle(ProjectUpdatedEventArgs eventMessage)
        {
            await _emailSender.SendProjectDetailsUpdatedNotificationAsync(eventMessage.ProjectNo, eventMessage.ProjectName,
          eventMessage.UpdatedBy, eventMessage.UpdatedDate, eventMessage.URLPath, eventMessage.RecipientEmail.Split(','), eventMessage.CCEmail.Split(','));
        }

        public Task Handle(ProjectDeletedEventArgs eventMessage)
        {
            throw new NotImplementedException();
        }

        public Task Handle(ProjectPromotedEventArgs eventMessage)
        {
            throw new NotImplementedException();
        }
    }
}
