using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing active directory group data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IActiveDirectoryGroupRepository : IBaseRepository<ActiveDirectoryGroup, string>
    {
 
    }
}
