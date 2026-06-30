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
    public class ProjectNotificationService : IProjectNotificationService
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IProjectNotificationRepository _projectNotificationRepository;

        public ProjectNotificationService(OracleDataAccessLayer dataAccess, IProjectNotificationRepository projectNotificationRepository)
        {
            _dataAccess = dataAccess;
            _projectNotificationRepository = projectNotificationRepository;
        } 

        public async Task<string> AddAsync(ProjectNotification obj)
        {
            return await _projectNotificationRepository.AddAsync(obj);
        }

        public async Task<IEnumerable<ProjectNotification>> GetByProjectAsync(string projectno)
        {
            return await _projectNotificationRepository.GetListAsync(projectno);
        }

        public Task<IEnumerable<ProjectNotification>> GetByEntityAsync(string projectno, string entitytype, string entitysysid)
        {
            throw new NotImplementedException();
        } 

        public Task NotifyProjectCancelled(ProjectCanceledEventArgs eventMessage)
        {
            throw new NotImplementedException();
        }

        public Task NotifyProjectCompleted(ProjectCompletedEventArgs eventMessage)
        {
            throw new NotImplementedException();
        }

        public Task NotifyProjectCreated(ProjectCreatedEventArgs eventMessage)
        {
            throw new NotImplementedException();
        }

        public Task NotifyProjectFailed(ProjectFailedEventArgs eventMessage)
        {
            throw new NotImplementedException();
        }

        public Task NotifyProjectHold(ProjectHoldEventArgs eventMessage)
        {
            throw new NotImplementedException();
        }

        public Task NotifyProjectStarted(ProjectStartedEventArgs eventMessage)
        {
            throw new NotImplementedException();
        }
    }
}
