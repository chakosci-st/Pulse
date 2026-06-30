using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for module-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IModuleService
    {
        /// <summary>
        /// Retrieves all modules from the system.
        /// </summary>
        /// <returns>A list of all modules.</returns>
        Task<IEnumerable<Module>> GetAllModulesAsync();

        /// <summary>
        /// Retrieves all modules from the system.
        /// </summary>
        /// <param name="searchValue">Search key.</param>
        /// <param name="sortBy">sortBy</param>
        /// <param name="sortDirection">sortDirection</param>
        /// <param name="isActive">The module status.</param>
        /// <param name="pageNumber">page number</param>
        /// <param name="pageSize">page size</param>
        /// <returns>A list of all modules.</returns>
        Task<PagedResult<Module>> GetPagedModulesAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);

        /// <summary>
        /// Retrieves a module by its unique identifier.
        /// </summary>
        /// <param name="modulecode">The unique identifier of the module.</param>
        /// <returns>The module with the specified MODULECODE, or null if not found.</returns>
        Module GetModuleByCode(string modulecode);


        /// <summary>
        /// Retrieves a module by its unique identifier.
        /// </summary>
        /// <param name="modulecode">The unique identifier of the module.</param>
        /// <returns>The module with the specified PLANTCODE, or null if not found.</returns>
        Task<Module> GetModuleByCodeAsync(string modulecode);

        /// <summary>
        /// Adds a new module to the system.
        /// </summary>
        /// <param name="module">The module to add.</param>
        /// <returns>Primary Key.</returns>
        Task<string> AddModuleAsync(Module module);

        /// <summary>
        /// Updates an existing module in the system.
        /// </summary>
        /// <param name="module">The module to update.</param>
        /// <returns>Number of rows affected.</returns>
        Task<int> UpdateModuleAsync(Module module);

        /// <summary>
        /// Deletes a module from the system by its unique identifier.
        /// </summary>
        /// <param name="modulecode">The unique identifier of the product to delete.</param>
        /// <param name="userid">The user who deleted the module.</param>
        /// <returns>Number of rows affected.</returns>
        Task<int> DeleteModuleAsync(string modulecode, string userid);
    }
}
