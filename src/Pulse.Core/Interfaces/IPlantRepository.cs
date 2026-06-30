using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing plant data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IPlantRepository : IBaseRepository<Plant, string>
    {
        Task<PagedResult<PlantWithStats>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <returns>Entity.</returns>
        Plant Get(string id);

        Task<IEnumerable<Plant>> GetListByUserAsync(string userid);
    }
}
