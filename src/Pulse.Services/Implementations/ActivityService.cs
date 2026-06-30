using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pulse.SharedUtilities.Helpers;

namespace Pulse.Services.Implementations
{
    public class ActivityService : IActivityService
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IActivityRepository _activityRepository; 
        public ActivityService(OracleDataAccessLayer dataAccess, IActivityRepository activityRepository)
        {
            _dataAccess = dataAccess;
            _activityRepository = activityRepository; 
        }

        public async Task<IEnumerable<Activity>> GetByKeywordAsync(string keyword)
        {
            return await _activityRepository.GetListAsync(keyword);
        }
    }
}
