using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing form data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IFieldRepository : IBaseRepository<Field, string>
    {
        Task<bool> IsReferencedAsync(string formFieldSysId);
        Task<int> ChangeStatusAsync(string formFieldSysId, int isActive, string modifiedBy);
        Task<ISet<string>> GetActiveFieldIdsByFormAsync(string formSysId);

        Task<PagedResult<FieldWithStats>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <returns>Entity.</returns>
        Field Get(string id);
    }
}
