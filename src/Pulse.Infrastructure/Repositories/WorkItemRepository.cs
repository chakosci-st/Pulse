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
    public class WorkItemRepository : BaseRepository<WorkItem, string>, IWorkItemRepository
    {

        public WorkItemRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(WorkItem workitem)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<WorkItem>(@"INSERT INTO WORKITEMS (CATEGORYCODE, PLANTCODE, MATURITYCODE, TASKNAME, TASKTYPE, ESTIMATEDMANDAYS, ISREQUIRED, CREATEDBY) 
VALUES (:CATEGORYCODE, :PLANTCODE, :MATURITYCODE, :TASKNAME, :TASKTYPE, :ESTIMATEDMANDAYS, :ISREQUIRED, :CREATEDBY) 
RETURNING WORKITEMSYSID = :WORKITEMSYSID", workitem, "WORKITEMSYSID");
        }

        public override async Task<int> UpdateAsync(WorkItem workitem)
        {
            return await _dataAccess.SaveDataAsync<WorkItem>("UPDATE WORKITEMS SET TASKNAME = :TASKNAME, TASKTYPE = :TASKTYPE, ESTIMATEDMANDAYS = :ESTIMATEDMANDAYS, ISREQUIRED = :ISREQUIRED, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() WHERE WORKITEMSYSID = :WORKITEMSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", workitem);
        }

        public override async Task<int> DeleteAsync(string workitemsysid)
        {
            return await _dataAccess.SaveDataAsync<WorkItem>("DELETE FROM WORKITEMS WHERE WORKITEMSYSID = :WORKITEMSYSID", new WorkItem { WorkItemSysId = workitemsysid });
        }
        public async override Task<IEnumerable<WorkItem>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<WorkItem>("SELECT * FROM WORKITEMS")
       .ContinueWith(t => (IEnumerable<WorkItem>)t.Result);
        }
        public async Task<IEnumerable<WorkItem>> GetListAsync(string plantcode, string categorycode, string maturitycode = null)
        {
            return await _dataAccess.LoadDataAsync<WorkItem>(@"SELECT WORKITEMSYSID, CATEGORYCODE, PLANTCODE, MATURITYCODE, TASKNAME, TASKTYPE, ESTIMATEDMANDAYS, ISREQUIRED, CREATEDBY, CREATEDDATE, MODIFIEDBY, MODIFIEDDATE 
FROM WORKITEMS
WHERE (PLANTCODE = :PLANTCODE OR :PLANTCODE IS NULL)
AND (CATEGORYCODE = :CATEGORYCODE OR :CATEGORYCODE IS NULL)
AND (MATURITYCODE = :MATURITYCODE OR :MATURITYCODE IS NULL)
", 
                new WorkItem { PlantCode = plantcode, CategoryCode = categorycode, MaturityCode = maturitycode })
                  .ContinueWith(t => (IEnumerable<WorkItem>)t.Result);
        }

        public override async Task<WorkItem> GetAsync(string workitemsysid)
        {
            return await _dataAccess.FindDataAsync<WorkItem>("SELECT CATEGORYCODE, CATEGORYNAME, CATEGORYDESCRIPTION, ISACTIVE, CREATEDBY, CREATEDDATE, MODIFIEDBY, MODIFIEDDATE FROM WORKITEMS WHERE WORKITEMSYSID = :WORKITEMSYSID", new WorkItem { WorkItemSysId = workitemsysid });
        }
 
    }
}
