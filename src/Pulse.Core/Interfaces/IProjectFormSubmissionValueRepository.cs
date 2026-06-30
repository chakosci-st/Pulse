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
    public interface IProjectFormSubmissionValueRepository : IBaseRepository<ProjectFormSubmissionValue, string>
    {
        Task<List<ProjectFormSubmissionValue>> GetBySubmissionAsync(string id);
        Task<IList<ProjectMonitoringDmsValue>> GetDmsValuesForMonitoringAsync(string loggedUser);
    }
}
