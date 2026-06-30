using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing plant - category link data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IPlantCategoryRepository : IBaseRepository<PlantCategory, string>
    {
        /// <summary>
        /// Retrieves all categories per plant from the repository.
        /// </summary>
        /// <param name="plantcode">The unique identifier of the member to retrieve.</param> 
        /// <param name="categorycode">The unique identifier of the member to retrieve.</param> 
        /// <returns>A list of all plant - categories.</returns>
        Task<IEnumerable<PlantCategory>> GetListAsync(string plantcode = null, string categorycode=null);

 
    }
}
