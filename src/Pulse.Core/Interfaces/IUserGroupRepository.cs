using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing usergroup data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IUserGroupRepository : IBaseRepository<UserGroup, int>
    {
        Task<PagedResult<UserGroup>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <returns>Entity.</returns>
        UserGroup Get(int id);
    }
}
