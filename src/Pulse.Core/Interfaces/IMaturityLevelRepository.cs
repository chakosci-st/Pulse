using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing maturity level data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IMaturityLevelRepository : IBaseRepository<MaturityLevel, string>
    {
        Task<PagedResult<MaturityLevelWithStats>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <returns>Entity.</returns>
        MaturityLevel Get(string id);
    }



}
