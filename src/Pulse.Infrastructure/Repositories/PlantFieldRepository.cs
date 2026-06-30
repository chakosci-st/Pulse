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
    public class PlantFieldRepository : BaseRepository<PlantField, string>, IPlantFieldRepository
    {

        public PlantFieldRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(PlantField link)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync(@"INSERT INTO PLANTFIELDS (PLANTCODE, CATEGORYCODE, MATURITYCODE, WORKITEMSYSID, FIELDID, ISLOCALOPTIONS, OPTIONS, SEQUENCENO, CREATEDBY) 
VALUES (:PLANTCODE, :CATEGORYCODE, :MATURITYCODE,  :WORKITEMSYSID, :FIELDID, :ISLOCALOPTIONS, :OPTIONS, :SEQUENCENO, :CREATEDBY) RETURNING PLANTFIELDSYSID INTO :PLANTFIELDSYSID", new
            {
                link.PlantCode,
                link.FieldId,
                link.IsLocalOptions,
                Options = string.IsNullOrEmpty(link.Options) ? null : link.Options as object,
                link.SequenceNo,
                link.CreatedBy
            }, "PLANTFIELDSYSID");
        }

        public override async Task<int> UpdateAsync(PlantField link)
        {
            return await _dataAccess.SaveDataAsync<PlantField>(@"
UPDATE PLANTFIELDS 
SET PLANTCODE = :PLANTCODE, CATEGORYCODE = :CATEGORYCODE, MATURITYCODE = :MATURITYCODE,  WORKITEMSYSID = :WORKITEMSYSID, FIELDID = :FIELDID, 
ISLOCALOPTIONS= :ISLOCALOPTIONS, OPTIONS= :OPTIONS, SEQUENCENO = :SEQUENCENO, 
MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
WHERE PLANTFIELDSYSID = :PLANTFIELDSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", link);
        }




        public async override Task<int> DeleteAsync(string plantfieldsysid)
        {
            return await _dataAccess.SaveDataAsync<PlantField>("DELETE FROM PLANTFIELDS WHERE PLANTFIELDSYSID = :PLANTFIELDSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", new PlantField { PlantFieldSysId = plantfieldsysid });
        }

        public async override Task<PlantField> GetAsync(string plantfieldsysid)
        {
            return await _dataAccess.FindDataAsync<PlantField>("SELECT * FROM PLANTFIELDS  WHERE PLANTFIELDSYSID = :PLANTFIELDSYSID ",
                new PlantField { PlantFieldSysId = plantfieldsysid });
        }
        public async override Task<IEnumerable<PlantField>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<PlantField>(@"SELECT * FROM PLANTFIELDS " )
              .ContinueWith(t => (IEnumerable<PlantField>)t.Result);
        }
        public  async Task<IEnumerable<PlantField>> GetListAsync(string plantcode = null, int? fieldid = null)
        {

            return await _dataAccess.LoadDataAsync<PlantField>(@"
SELECT *
FROM PLANTFIELDS  
WHERE (PLANTCODE = :PLANTCODE OR :PLANTCODE IS NULL) AND (FIELDID = :FIELDID OR :FIELDID IS NULL)",
                new PlantField { PlantCode = plantcode, FieldId = fieldid })
                .ContinueWith(t => (IEnumerable<PlantField>)t.Result);

        }

    }
}
