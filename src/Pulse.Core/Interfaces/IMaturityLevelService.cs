using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for maturity level-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IMaturityLevelService
    {
        /// <summary>
        /// Retrieves all maturity levels from the system.
        /// </summary>
        /// <returns>A list of all maturity levels.</returns>
        Task<IEnumerable<MaturityLevel>> GetAllMaturityLevelsAsync();

        /// <summary>
        /// Retrieves a maturity level by its unique identifier.
        /// </summary>
        /// <param name="maturitycode">The unique identifier of the maturitylevel.</param>
        /// <returns>The maturity level with the specified MATURITYCODE, or null if not found.</returns>
        Task<MaturityLevel> GetMaturityLevelByCodeAsync(string maturitycode);

        /// <summary>
        /// Adds a new maturity level to the system.
        /// </summary>
        /// <param name="maturitylevel">The maturity level to add.</param>
        /// <returns>Primary Key.</returns>
        Task<string> AddMaturityLevelAsync(MaturityLevel maturitylevel);

        /// <summary>
        /// Updates an existing maturity level in the system.
        /// </summary>
        /// <param name="maturitylevel">The maturity level to update.</param>
        /// <returns>Number of rows affected.</returns>
        Task<int> UpdateMaturityLevelAsync(MaturityLevel maturitylevel);

        /// <summary>
        /// Deletes a maturity level from the system by its unique identifier.
        /// </summary>
        /// <param name="maturitycode">The unique identifier of the maturity level to delete.</param>
        /// <param name="userid">The user who deleted the maturitylevel.</param>
        /// <returns>Number of rows affected.</returns>
        Task<int> DeleteMaturityLevelAsync(string maturitycode, string userid);

        /// <summary>
        /// Retrieves all maturity levels from the system.
        /// </summary>
        /// <param name="searchValue">Search key.</param>
        /// <param name="isActive">The maturity level status.</param>
        /// <param name="pageNumber">page number</param>
        /// <param name="pageSize">page size</param>
        /// <returns>A list of all maturity levels.</returns>
        Task<PagedResult<MaturityLevelWithStats>> GetPagedMaturityLevelsAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);

        /// <summary>
        /// Retrieves a maturity level by its unique identifier.
        /// </summary>
        /// <param name="maturitycode">The unique identifier of the maturity level.</param>
        /// <returns>The maturity level with the specified maturitycode, or null if not found.</returns>
        MaturityLevel GetMaturityByCode(string maturitycode);
    }
}
