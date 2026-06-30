using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for Field-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IFieldService
    {
        /// <summary>
        /// Retrieves all fields from the system.
        /// </summary>
        /// <returns>A list of all fields.</returns>
        Task<IEnumerable<Field>> GetAllFieldsAsync();

        /// <summary>
        /// Retrieves a field by its unique identifier.
        /// </summary>
        /// <param name="fieldsysid">The unique identifier of the field.</param>
        /// <returns>The field with the specified USERGROUPCODE, or null if not found.</returns>
        Task<Field> GetFieldByIdAsync(string fieldsysid);

        /// <summary>
        /// Adds a new field to the system.
        /// </summary>
        /// <param name="field">The field to add.</param>
        /// <returns>Rows affected.</returns>
        Task<int> AddAsync(Field field);

        /// <summary>
        /// Updates an existing field in the system.
        /// </summary>
        /// <param name="field">The field to update.</param>
        /// <returns>Rows affected.</returns>
        Task<int> UpdateAsync(Field field);

        /// <summary>
        /// Deletes a field from the system by its unique identifier.
        /// </summary>
        /// <param name="fieldsysid">The unique identifier of the user to delete.</param>
        /// <param name="userid">The user who deleted the field.</param>
        /// <returns>Rows affected.</returns>
        Task<int> DeleteAsync(string fieldsysid, string userid);

        Task<PagedResult<FieldWithStats>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);
    }
}
