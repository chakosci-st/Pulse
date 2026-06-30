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
    public interface IPlantRoadmapLinkRepository : IBaseRepository<PlantRoadmapLink, string>
    {
         Task<IEnumerable<PlantRoadmapLinkExtended>> GetLinkListAsync(string plantcode = null, string roadmapsysid = null);
    }
}
