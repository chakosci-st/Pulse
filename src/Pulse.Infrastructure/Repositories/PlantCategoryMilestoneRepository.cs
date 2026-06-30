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
    public class PlantCategoryMilestoneRepository : BaseRepository<PlantCategoryMilestone, string>, IPlantCategoryMilestoneRepository
    {

        public PlantCategoryMilestoneRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(PlantCategoryMilestone milestone)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<PlantCategoryMilestone>(@"INSERT INTO PLANTCATEGORYMILESTONES (PLANTCODE, CATEGORYCODE, MATURITYCODE, PARENTSYSID, ALIAS, CREATEDBY) 
VALUES (:PLANTCODE, :CATEGORYCODE, :MATURITYCODE, :PARENTSYSID, :ALIAS, :CREATEDBY) 
RETURNING PLANTCATEGORYMILESTONESYSID INTO PLANTCATEGORYMILESTONESYSID", milestone, "PLANTCATEGORYMILESTONESYSID");
        }

        public override async Task<int> UpdateAsync(PlantCategoryMilestone milestone)
        {
            return await _dataAccess.SaveDataAsync<PlantCategoryMilestone>(@"UPDATE PLANTCATEGORYMILESTONES SET PLANTCODE = :PLANTCODE, CATEGORYCODE = :CATEGORYCODE, MATURITYCODE = :MATURITYCODE,  PARENTSYSID = :PARENTSYSID, ALIAS = :ALIAS,MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
WHERE PLANTCATEGORYMILESTONESYSID = :PLANTCATEGORYMILESTONESYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", milestone);
        }



        public async Task<IEnumerable<PlantCategoryMilestone>> GetAllAsync(string plantcategorymilestonesysid)
        {
            return await _dataAccess.LoadDataAsync<PlantCategoryMilestone>("SELECT PLANTCATEGORYMILESTONESYSID, PLANTCODE, CATEGORYCODE, MATURITYCODE, PARENTSYSID, ALIAS, CREATEDBY, CREATEDDATE, MODIFIEDBY, MODIFIEDDATE FROM PLANTCATEGORYMILESTONES  WHERE PLANTCATEGORYMILESTONESYSID = :PLANTCATEGORYMILESTONESYSID ",
                new PlantCategoryMilestone { PlantCategoryMilestoneSysId = plantcategorymilestonesysid })
                .ContinueWith(t => (IEnumerable<PlantCategoryMilestone>)t.Result);
        }



        public async override Task<int> DeleteAsync(string plantcategorymilestonesysid)
        {
            return await _dataAccess.SaveDataAsync<PlantCategoryMilestone>("DELETE FROM PLANTCATEGORYMILESTONES WHERE PLANTCATEGORYMILESTONESYSID = :PLANTCATEGORYMILESTONESYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", new PlantCategoryMilestone { PlantCategoryMilestoneSysId = plantcategorymilestonesysid });
        }


        public async override Task<PlantCategoryMilestone> GetAsync(string plantcategorymilestonesysid)
        {
            return await _dataAccess.FindDataAsync<PlantCategoryMilestone>("SELECT * FROM PLANTCATEGORYMILESTONES  WHERE PLANTCATEGORYMILESTONESYSID = :PLANTCATEGORYMILESTONESYSID",
                new PlantCategoryMilestone { PlantCategoryMilestoneSysId = plantcategorymilestonesysid });
        }


        public override async Task<IEnumerable<PlantCategoryMilestone>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<PlantCategoryMilestone>("SELECT * FROM PLANTCATEGORYMILESTONES")
                .ContinueWith(t => (IEnumerable<PlantCategoryMilestone>)t.Result);
        }

        public async Task<IEnumerable<PlantCategoryMilestone>> GetListAsync(string plantcode, string categorycode)
        {
            return await _dataAccess.LoadDataAsync<PlantCategoryMilestone>("SELECT * FROM PLANTCATEGORYMILESTONES  WHERE PLANTCODE = :PLANTCODE AND CATEGORYCODE = :CATEGORYCODE ",
                new PlantCategoryMilestone { PlantCode = plantcode, CategoryCode = categorycode })
                .ContinueWith(t => (IEnumerable<PlantCategoryMilestone>)t.Result);
        }
    }
}
