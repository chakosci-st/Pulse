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
    public interface IFormRepository : IBaseRepository<Form, string>
    {
        Task<PagedResult<FormExtended>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <returns>Entity.</returns>
        Form Get(string id);


        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <returns>Entity.</returns>
        FormExtended GetCompleteInfo(string id);

        Task<FormExtended> GetCompleteInfoAsync(string id);
    }
}
