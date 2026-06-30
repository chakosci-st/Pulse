using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing plant data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IRoadmapRepository : IBaseRepository<Roadmap, string>
    {
        Roadmap Get(string roadmapsysid);

        Task<RoadmapExtended> GetCompleteInfoAsync(string roadmapsysid);

        Task<PagedResult<RoadmapExtended>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);

        Task<IEnumerable<NodeRow>> GetNodes(string roadmapsysid);
    }
}
