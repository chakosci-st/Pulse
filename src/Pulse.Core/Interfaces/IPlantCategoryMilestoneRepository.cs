using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing plant - category milestone data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IPlantCategoryMilestoneRepository : IBaseRepository<PlantCategoryMilestone, string>
    {
        /// <summary>
        /// Retrieves all milestones per plant - category from the repository.
        /// </summary>
        /// <param name="plantcode">The unique identifier of the member to retrieve.</param> 
        /// <param name="categorycode">The unique identifier of the member to retrieve.</param> 
        /// <returns>A list of all milestones per plant - category.</returns>
        Task<IEnumerable<PlantCategoryMilestone>> GetListAsync(string plantcode, string categorycode);

        /// <summary>
        /// Retrieves all milestones per plantcategorysysid from the repository.
        /// </summary>
        /// <param name="plantcategorysysid">The unique identifier of the member to retrieve.</param>  
        /// <returns>A list of all milestones per plant - category.</returns>
        Task<IEnumerable<PlantCategoryMilestone>> GetAllAsync(string plantcategorysysid);


    }
}
