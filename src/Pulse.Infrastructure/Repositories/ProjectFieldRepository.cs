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
    public class ProjectFieldRepository : BaseRepository<ProjectField, string>, IProjectFieldRepository
    {
        public ProjectFieldRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(ProjectField projectfield)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<ProjectField>(@"  INSERT INTO PROJECTFIELDS
      (
       PROJECTNO
      ,MILESTONESYSID
      ,TASKSYSID
      ,PLANTFIELDSYSID
      ,FIELDVALUE
      ,CREATEDBY  
      )
    VALUES
      (
       :PROJECTNO
      ,:MILESTONESYSID
      ,:TASKSYSID
      ,:PLANTFIELDSYSID
      ,:FIELDVALUE 
      ,:CREATEDBY  
      )
RETURNING PROJECTFIELDSYSID INTO :PROJECTFIELDSYSID
", projectfield, "PROJECTFIELDSYSID");
        }

        public override async Task<int> UpdateAsync(ProjectField projectfield)
        {
            return await _dataAccess.SaveDataAsync<ProjectField>(@"UPDATE PROJECTFIELDS 
                        SET
                            FIELDVALUE = :FIELDVALUE
                            ,MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
                        WHERE PROJECTFIELDSYSID = :PROJECTFIELDSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", projectfield);
        }

        public override async Task<int> DeleteAsync(string projectfieldid)
        {
            return await _dataAccess.SaveDataAsync<ProjectField>("DELETE FROM PROJECTFIELDS WHERE PROJECTFIELDSYSID = :PROJECTFIELDSYSID", new ProjectField { ProjectFieldSysId = projectfieldid });
        }

        public override async Task<IEnumerable<ProjectField>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<ProjectField>("SELECT * FROM PROJECTFIELDS")
                  .ContinueWith(t => (IEnumerable<ProjectField>)t.Result);
        }

 
 
        public async Task<IEnumerable<ProjectField>> GetListAsync(string projectno = null, string milestonesysid = null, string tasksysid = null)
        {
            return await _dataAccess.LoadDataAsync<ProjectField>("SELECT * FROM PROJECTFIELDS WHERE (:PROJECTNO IS NULL OR PROJECTNO = :PROJECTNO) AND (:MILESTONESYSID IS NULL OR MILESTONESYSID = :MILESTONESYSID) AND (:TASKSYSID IS NULL OR TASKSYSID = :TASKSYSID)",
new ProjectField { ProjectNo = projectno, MilestoneSysId= milestonesysid, TaskSysId = tasksysid });
        }

        public override async Task<ProjectField> GetAsync(string id)
        {
            return await _dataAccess.FindDataAsync<ProjectField>("SELECT * FROM PROJECTFIELDS WHERE PROJECTFIELDSYSID = :PROJECTFIELDSYSID",
new ProjectField { ProjectFieldSysId = id });
        }
    }
}
