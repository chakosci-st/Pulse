
using log4net; 
using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Infrastructure.Repositories
{
    public class ActivityRepository : BaseRepository<Activity, string>, IActivityRepository
    {

        public ActivityRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public async Task<IEnumerable<Activity>> GetListAsync(string keyword)
        {
            return await _dataAccess.LoadDataAsync<Activity>("SELECT distinct activityname, activitydescription FROM ROADMAPACTIVITIES WHERE LOWER(ActivityName) like :ActivityName ||'%' ",
                new Activity { ActivityName = keyword.ToLower() });
        }



        public override Task<string> AddAsync(Activity entity)
        {
            throw new NotImplementedException();
        }

        public override Task<int> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public override Task<Activity> GetAsync(string id)
        {
            throw new NotImplementedException();
        }



        public override Task<IEnumerable<Activity>> GetListAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<int> UpdateAsync(Activity entity)
        {
            throw new NotImplementedException();
        }
    }
}
