using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing module data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IModuleRepository : IBaseRepository<Module, string>
    {
        Task<PagedResult<Module>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <returns>Entity.</returns>
        Module Get(string id);
    }
}
