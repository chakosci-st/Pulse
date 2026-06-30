using log4net; 
using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Infrastructure.Repositories
{
    public class StatusChangeRepository : BaseRepository<StatusChange, string>, IStatusChangeRepository
    {


        public StatusChangeRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(StatusChange status)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<StatusChange>(@"INSERT INTO STATUSCHANGE (PROJECTNO, TASKSYSID, USERID, USERGROUPID, ADGROUP,  CREATEDBY, CREATEDDATE) 
VALUES (:PROJECTNO, :TASKSYSID, :USERID, :USERGROUPID, :ADGROUP, :CREATEDBY, NVL(:CREATEDDATE,SYSTIMESTAMP))) 
RETURNING STATUSCHANGESYSID = :STATUSCHANGESYSID", status, "STATUSCHANGESYSID");
        }



        public override async Task<StatusChange> GetAsync(string statuschangesysid)
        {
            return await _dataAccess.FindDataAsync<StatusChange>("SELECT * FROM STATUSCHANGE WHERE STATUSCHANGESYSID = :STATUSCHANGESYSID", new StatusChange { StatusChangeSysId = statuschangesysid });
        }
        public async override Task<IEnumerable<StatusChange>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<StatusChange>("SELECT * FROM STATUSCHANGE")
       .ContinueWith(t => (IEnumerable<StatusChange>)t.Result);
        }

        public async Task<IEnumerable<StatusChange>> GetListAsync(string projectno)
        {
            return await _dataAccess.LoadDataAsync<StatusChange>("SELECT * FROM STATUSCHANGE WHERE PROJECTNO = :PROJECTNO", new StatusChange { ProjectNo = projectno })
  .ContinueWith(t => (IEnumerable<StatusChange>)t.Result);
        }
        // not applicable
        public override Task<int> UpdateAsync(StatusChange entity)
        {
            throw new NotImplementedException();
        }
        // not applicable
        public override Task<int> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}
