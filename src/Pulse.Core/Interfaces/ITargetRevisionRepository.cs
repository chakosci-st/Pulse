using Pulse.Core.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing status change data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface ITargetRevisionRepository : IBaseRepository<TargetRevision, string>
    {
        Task<IEnumerable<TargetRevision>> GetListAsync(string projectno = null, string milestonesysid = null, string tasksysid = null);
    }
}
