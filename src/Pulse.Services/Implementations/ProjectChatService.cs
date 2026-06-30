using Pulse.Core.Entities;
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
    public class ProjectChatService : IProjectChatService
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IProjectChatRepository _projectChatRepository;

        public ProjectChatService(OracleDataAccessLayer dataAccess, IProjectChatRepository projectChatRepository)
        {
            _dataAccess = dataAccess;
            _projectChatRepository = projectChatRepository;
        }


        public async Task<string> AddAsync(ProjectChat obj)
        {
            return await _projectChatRepository.AddAsync(obj);
        }

        public async Task<IEnumerable<ProjectChat>> GetByProjectAsync(string projectno)
        {
            return await _projectChatRepository.GetListAsync(projectno);
        }


        public async Task<IEnumerable<ProjectChat>> GetByProjectAsync(string projectno, string user)
        {
            return await _projectChatRepository.GetListAsync(projectno, user);
        }

        public async Task<IEnumerable<ProjectChat>> GetUnreadByUserAsync(string user)
        {
            return await _projectChatRepository.GetUnreadListByUserAsync(user);
        }

        public async Task<RoomMeta> GetRoomMetaAsync(string room)
        {
            return await _projectChatRepository.GetRoomMetaAsync(room);
        }

        public async Task<IEnumerable<RoomMeta>> GetRoomsByUserIdAsync(string userid) {
            return await _projectChatRepository.GetRoomsByUserIdAsync(userid);
        }

        public async Task<IEnumerable<string>> GetParticipantsByRoomAsync(string room) {
            return await _projectChatRepository.GetParticipantsByRoomAsync(room);
        }
    }
}
