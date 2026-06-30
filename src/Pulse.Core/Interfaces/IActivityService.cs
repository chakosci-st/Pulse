using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for Form-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IActivityService
    {
        Task<IEnumerable<Activity>> GetByKeywordAsync(string keyword);
    }
}
