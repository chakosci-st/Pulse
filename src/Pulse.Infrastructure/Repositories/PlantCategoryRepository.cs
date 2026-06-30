
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
    public class PlantCategoryRepository : BaseRepository<PlantCategory, string>, IPlantCategoryRepository
    {

        public PlantCategoryRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(PlantCategory link)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<PlantCategory>("INSERT INTO PLANTCATEGORIES (PLANTCODE, CATEGORYCODE, ISACTIVE, CREATEDBY) VALUES (:PLANTCODE, :CATEGORYCODE, :ISACTIVE, :CREATEDBY) RETURNING PLANTCATEGORYSYSID INTO :PLANTCATEGORYSYSID", link, "PLANTCATEGORYSYSID");
        }

        public override async Task<int> UpdateAsync(PlantCategory link)
        {
            return await _dataAccess.SaveDataAsync<PlantCategory>("UPDATE PLANTCATEGORIES SET ISACTIVE = :ISACTIVE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() WHERE PLANTCATEGORYSYSID = :PLANTCATEGORYSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", link);
        }




        public async override Task<int> DeleteAsync(string plantcategorysysid)
        {
            return await _dataAccess.SaveDataAsync<PlantCategory>("DELETE FROM PLANTCATEGORIES WHERE PLANTCATEGORYSYSID = :PLANTCATEGORYSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", new PlantCategory { PlantCategorySysId = plantcategorysysid });
        }

        public async override Task<PlantCategory> GetAsync(string plantcategorysysid)
        {
            return await _dataAccess.FindDataAsync<PlantCategory>("SELECT * FROM PLANTCATEGORIES  WHERE PLANTCATEGORYSYSID = :PLANTCATEGORYSYSID ",
                new PlantCategory { PlantCategorySysId = plantcategorysysid });
        }
        #region "EXCLUDED"


        public async override Task<IEnumerable<PlantCategory>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<PlantCategory>("SELECT * FROM PLANTCATEGORIES");
        }

        public async Task<IEnumerable<PlantCategory>> GetListAsync(string plantcode = null, string categorycode = null)
        {

            return await _dataAccess.LoadDataAsync<PlantCategory>(@"
SELECT *
FROM PLANTCATEGORIES  
WHERE (PLANTCODE = :PLANTCODE OR :PLANTCODE IS NULL) AND (CATEGORYCODE = :CATEGORYCODE OR :CATEGORYCODE IS NULL)",
                new PlantCategory { PlantCode = plantcode, CategoryCode = categorycode })
                .ContinueWith(t => (IEnumerable<PlantCategory>)t.Result);

        }
        #endregion


    }
}
