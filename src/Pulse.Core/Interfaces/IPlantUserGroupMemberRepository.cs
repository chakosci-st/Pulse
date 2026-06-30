using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing plant - user group member data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IPlantUserGroupMemberRepository : IBaseRepository<PlantUserGroupMember, string>
    {
        /// <summary>
        /// Retrieves all members of plant- usergroup from the repository.
        /// </summary>
        /// <returns>A list of all members of plant- usergroup.</returns>
        Task<IEnumerable<PlantUserGroupMember>> GetListAsync(string plantcode, int usergroupid);


        /// <summary>
        /// Retrieves all members of plant- usergroup from the repository.
        /// </summary>
        /// <returns>A list of all members of plant- usergroup.</returns>
        Task<IEnumerable<PlantMember>> GetMembersOnlyListAsync(string plantcode);

        Task<IEnumerable<PlantUserGroupMember>> GetListByUserIdAsync(string userid);


    }
}
