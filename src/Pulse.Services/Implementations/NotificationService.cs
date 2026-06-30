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
    public class NotificationService : INotificationService
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(OracleDataAccessLayer dataAccess, INotificationRepository notificationRepository)
        {
            _dataAccess = dataAccess;
            _notificationRepository = notificationRepository;
        }

        public async Task<string> AddAsync(Notification obj)
        {
            return await _notificationRepository.AddAsync(obj);
        }

        public async Task<int> DeleteAsync(Notification obj)
        {
            return await _notificationRepository.DeleteAsync(obj.NotificationSysId);
        }

        public async Task<int> EditAsync(Notification obj)
        {
            return await _notificationRepository.UpdateAsync(obj);
        }

        public async Task<IEnumerable<Notification>> GetActiveAsync(string userid)
        {
            return await _notificationRepository.GetActiveAsync(userid);
        }

        public async Task<IEnumerable<Notification>> GetActiveUnreadAsync(string userid)
        {
            return await _notificationRepository.GetActiveUnreadAsync(userid);
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await _notificationRepository.GetListAsync();
        }

        public async Task<Notification> GetAsync(string id)
        {
            return await _notificationRepository.GetAsync(id);
        }

        public async Task<IEnumerable<Notification>> GetByEntityAsync(string entitytype, string entitysysid)
        {
            return await _notificationRepository.GetListAsync(entitytype, entitysysid);
        }

        public async Task<IEnumerable<Notification>> GetByProjectAsync(string projectno)
        {
            return await _notificationRepository.GetListAsync(projectno);
        }

        public async Task<int> MarkAsReadAsync(string userid, string notificationsysid)
        {
            return await _notificationRepository.MarkAsReadAsync(userid, notificationsysid);
        }

        public async Task<int> MarkAsReadAsync(string userid, IEnumerable<string> notificationsysids)
        {
            return await _notificationRepository.MarkAsReadAsync(userid, notificationsysids);
        }
    }
}


