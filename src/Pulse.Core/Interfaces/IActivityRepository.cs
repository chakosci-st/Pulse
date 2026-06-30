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
    public interface IActivityRepository : IBaseRepository<Activity, string>
    { 
        Task<IEnumerable<Activity>> GetListAsync(string keyword);
    }
}
