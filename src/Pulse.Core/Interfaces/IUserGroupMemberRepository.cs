using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing usergroup member data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IUserGroupMemberRepository : IBaseRepository<UserGroupMember, string>
    {
        /// <summary>
        /// Retrieves all work items from the repository.
        /// </summary>
        /// <param name="userid">The unique identifier of the member to retrieve.</param>  
        /// <param name="usergroupid">The unique identifier of the member to retrieve.</param>   
        /// <returns>A list of all usergroup members.</returns>
        Task<IEnumerable<UserGroupMember>> GetListAsync(string userid = null, int? usergroupid = null);
    }
}
