using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing plant - member link data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IPlantMemberRepository : IBaseRepository<PlantMember, string>
    {
        /// <summary>
        /// Retrieves all categories per plant from the repository.
        /// </summary>
        /// <param name="plantcode">The unique identifier of the member to retrieve.</param> 
        /// <param name="userid">The unique identifier of the member to retrieve.</param> 
        /// <returns>A list of all plant - members.</returns>
        Task<IEnumerable<PlantMember>> GetListAsync(string plantcode = null, string userid = null);


    }
}
