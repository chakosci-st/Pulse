using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing project-products data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProjectProductRepository : IBaseRepository<ProjectProduct, string>
    {
        /// <summary>
        /// Retrieves products per project asynchronously from the repository.
        /// </summary>
        /// <param name="projectno">The project no.</param> 
        /// <returns>members by projectno</returns>
        Task<IEnumerable<ProjectProduct>> GetListAsync(string projectno);


        Task<ProjectProduct> GetAsync(string projectno, string productcode);

    }
}
