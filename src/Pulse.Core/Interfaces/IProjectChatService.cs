using Entities = Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Pulse.Core.Entities;
/// <summary>
/// Interface for Chat-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProjectChatService
    {
        /// <summary>
        /// Adds a new Chat to the system.
        /// </summary>
        /// <param name="attachment">The Chat to add.</param>
        Task<string> AddAsync(ProjectChat obj);

        /// <summary>
        /// Retrieves all Chat from the system.
        /// </summary>
        /// <param name="projectno">projectno</param>
        /// <returns>A list of all Chat.</returns>
        Task<IEnumerable<ProjectChat>> GetByProjectAsync(string projectno);

        /// <summary>
        /// Retrieves all Chat from the system.
        /// </summary>
        /// <param name="projectno">projectno</param>
        /// <returns>A list of all Chat.</returns>
        Task<IEnumerable<ProjectChat>> GetByProjectAsync(string projectno, string user);

        Task<IEnumerable<ProjectChat>> GetUnreadByUserAsync(string user);

        Task<RoomMeta> GetRoomMetaAsync(string room);
        Task<IEnumerable<RoomMeta>> GetRoomsByUserIdAsync(string userid);

        Task<IEnumerable<string>> GetParticipantsByRoomAsync(string room);
    }
}
