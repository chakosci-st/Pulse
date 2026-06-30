using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing plant - field link data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IPlantFieldRepository : IBaseRepository<PlantField, string>
    {
        /// <summary>
        /// Retrieves all categories per plant from the repository.
        /// </summary>
        /// <param name="plantcode">The unique identifier of the member to retrieve.</param> 
        /// <param name="fieldid">The unique identifier of the member to retrieve.</param> 
        /// <returns>A list of all plant - categories.</returns>
        Task<IEnumerable<PlantField>> GetListAsync(string plantcode = null, int? fieldid=null);

 
    }
}
