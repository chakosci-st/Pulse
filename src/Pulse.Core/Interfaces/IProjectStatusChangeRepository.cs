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
    public interface IProjectStatusChangeRepository : IBaseRepository<ProjectStatusChange, string>
    {
        Task<IEnumerable<ProjectStatusChange>> GetListByProject(string projectno);
        Task<IEnumerable<ProjectStatusChange>> GetListByEntity(string entitytype, string entitysysid);

    }
}
