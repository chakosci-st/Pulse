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
    public interface IFormEntityLinkRepository : IBaseRepository<FormEntityLink, string>
    {
        Task<IEnumerable<FormEntityLink>> GetListAsync(string entitysysid);
        Task<IEnumerable<NodeFormRow>> GetNodeFormsAsync(string parentsysid, string type);
        Task<IEnumerable<RootFormRow>> GetRootNodeFormsAsync(string roadmapsysid);
        Task<IEnumerable<FormEntityLink>> GetListByRoadmapAsync(string roadmapsysid);
    }
}
