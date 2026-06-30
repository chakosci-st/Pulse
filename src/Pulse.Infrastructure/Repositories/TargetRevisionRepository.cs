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
    public class TargetRevisionRepository : BaseRepository<TargetRevision, string>, ITargetRevisionRepository
    {


        public TargetRevisionRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(TargetRevision status)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<TargetRevision>(@"INSERT INTO TARGETREVISION (PROJECTNO, MILESTONESYSID, TASKSYSID, TARGETSTART, TARGETCOMPLETION, REASON, CREATEDBY, CREATEDDATE) 
VALUES (:PPROJECTNO, :MILESTONESYSID, :TASKSYSID, :TARGETSTART, :TARGETCOMPLETION, :REASON, :CREATEDBY, NVL(:CREATEDDATE,SYSTIMESTAMP))) 
RETURNING TARGETREVISIONSYSID = :TARGETREVISIONSYSID", status, "TARGETREVISIONSYSID");
        }

        public override async Task<TargetRevision> GetAsync(string statuschangesysid)
        {
            return await _dataAccess.FindDataAsync<TargetRevision>("SELECT * FROM TARGETREVISION WHERE TARGETREVISIONSYSID = :TARGETREVISIONSYSID", new TargetRevision { TargetRevisionSysId = statuschangesysid });
        }
        public async override Task<IEnumerable<TargetRevision>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<TargetRevision>("SELECT * FROM TARGETREVISION")
       .ContinueWith(t => (IEnumerable<TargetRevision>)t.Result);
        }

        public async Task<IEnumerable<TargetRevision>> GetListAsync(string projectno = null, string milestonesysid = null, string tasksysid = null)
        {
            return await _dataAccess.LoadDataAsync<TargetRevision>("SELECT * FROM TARGETREVISION WHERE (:PROJECTNO IS NULL OR PROJECTNO = :PROJECTNO) AND (:MILESTONESYSID IS NULL OR MILESTONESYSID = :MILESTONESYSID) AND (:TASKSYSID IS NULL OR TASKSYSID = :TASKSYSID)",
                new TargetRevision
                {
                    ProjectNo = projectno,
                    MilestoneSysId = milestonesysid,
                    TaskSysId = tasksysid
                })
  .ContinueWith(t => (IEnumerable<TargetRevision>)t.Result);
        }

        // not applicable
        public override Task<int> UpdateAsync(TargetRevision entity)
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
