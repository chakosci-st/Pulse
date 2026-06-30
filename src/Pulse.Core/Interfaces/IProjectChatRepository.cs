using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing project member data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProjectChatRepository : IBaseRepository<ProjectChat, string>
    {
        /// <summary>
        /// Retrieves project Chats asynchronously from the repository.
        /// </summary>
        /// <param name="projectno">The project no.</param> 
        /// <returns>Chats</returns>
        Task<IEnumerable<ProjectChat>> GetListAsync(string projectno);

        Task<IEnumerable<ProjectChat>> GetListAsync(string projectno, string user);

        Task<IEnumerable<ProjectChat>> GetUnreadListByUserAsync(string user);
        Task<RoomMeta> GetRoomMetaAsync(string room);
        Task<IEnumerable<RoomMeta>> GetRoomsByUserIdAsync(string userid);

        Task<IEnumerable<string>> GetParticipantsByRoomAsync(string room);
    }
}
