using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for plant field link-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IPlantFieldService
    {
        /// <summary>
        /// Retrieves all fields per plant from the system.
        /// </summary>
        /// <param name="plantcode">plant</param>
        /// <returns>A list of all fields per plant.</returns>
        Task<IEnumerable<PlantField>> GetAllCategoriesByPlantAsync(string plantcode);

        /// <summary>
        /// Retrieves all plants per field from the system.
        /// </summary>
        /// <param name="fieldid">Field id</param>
        /// <returns>A list of all plants per field.</returns>
        Task<IEnumerable<PlantField>> GetAllPlantsByFieldAsync(int fieldid);

        /// <summary>
        /// Retrieves a plant - field by its unique identifier.
        /// </summary>
        /// <param name="plantfieldsysid">The unique identifier of the member to retrieve.</param> 
        /// <returns>The plant - field with the specified PLANTCODE and field, or null if not found.</returns>
        Task<PlantField> GetByIdAsync(string plantfieldsysid);

        /// <summary>
        /// Adds a new plant - field link to the system.
        /// </summary>
        /// <param name="entity">The link to add.</param>
        Task<string> AddLinkAsync(PlantField entity);

        /// <summary>
        /// Updates an existing plant - field in the system.
        /// </summary>
        /// <param name="entity">The plant - field to update.</param>
        Task<int> UpdateLinkAsync(PlantField entity);

        /// <summary>
        /// Deletes a plant-field link from the system by its unique identifier.
        /// </summary>
        /// <param name="plantfieldsysid">The unique identifier of the member to delete.</param> 
        /// <param name="loggeduserid">The user who deleted the member.</param>
        Task<int>DeleteLinkAsync(string plantfieldsysid, string loggeduserid);

         
    }
}
