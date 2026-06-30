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
    public interface IRoadmapMilestoneRepository : IBaseRepository<RoadmapMilestone, string>
    {
        Task<IEnumerable<RoadmapMilestone>> GetListAsync(string roadmapsysid, string parenttype = null, string parentsysid = null);
        Task<int> DeleteAsync(string roadmapmilestonesysid, string roadmapsysid);
    }
}
