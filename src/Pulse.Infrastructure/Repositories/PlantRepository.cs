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
    public class PlantRepository : BaseRepository<Plant, string>, IPlantRepository
    {

        public PlantRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(Plant plant)
        {
            var affectedrows = await _dataAccess.SaveDataAsync<Plant>("INSERT INTO PLANTS (PLANTCODE, PLANTNAME, CREATEDBY) VALUES (:PLANTCODE, :PLANTNAME, :CREATEDBY)", plant);

            if (affectedrows > 0)
            {
                return plant.PlantCode;
            }
            else return "";
        }

        public override async Task<int> UpdateAsync(Plant plant)
        {
            return await _dataAccess.SaveDataAsync<Plant>("UPDATE PLANTS SET PLANTNAME = :PLANTNAME, ISACTIVE = :ISACTIVE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() WHERE PLANTCODE = :PLANTCODE AND TRANSACTIONKEY = :TRANSACTIONKEY", plant);
        }

        public override async Task<int> DeleteAsync(string plantcode)
        {
            return await _dataAccess.SaveDataAsync<Plant>("DELETE FROM PLANTS WHERE PLANTCODE = :PLANTCODE", new Plant { PlantCode = plantcode });
        }

        public override async Task<IEnumerable<Plant>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<Plant>("SELECT * FROM PLANTS")
                .ContinueWith(t => (IEnumerable<Plant>)t.Result);
        }

        public  async Task<IEnumerable<Plant>> GetListByUserAsync(string userid)
        {
            return await _dataAccess.LoadDataAsync<Plant>(@"select pl.* from plants pl 
inner join plantmembers pm
on pm.plantcode = pl.plantcode
where pm.userid = :CreatedBy", new Plant {  CreatedBy = userid })
                .ContinueWith(t => (IEnumerable<Plant>)t.Result);
        }


        public override async Task<Plant> GetAsync(string plantcode)
        {
            return await _dataAccess.FindDataAsync<Plant>("SELECT * FROM PLANTS WHERE PLANTCODE = :PLANTCODE", new Plant { PlantCode = plantcode });
        }
        public Plant Get(string plantcode)
        {
            return _dataAccess.FindData<Plant>("SELECT * FROM PLANTS WHERE PLANTCODE = :PLANTCODE", new Plant { PlantCode = plantcode });
        }
        public async Task<PagedResult<PlantWithStats>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            string pagedQuery = $@"
  SELECT *
    FROM (SELECT l.*, ROW_NUMBER () OVER (ORDER BY  {sortBy} {sortDirection}) rn
            FROM (SELECT pl.*,
                         cb.firstname || ' ' || cb.lastname createdbyname,
                         mb.firstname || ' ' || mb.lastname modifiedbyname,
                         (SELECT COUNT (1)
                            FROM projects prj
                           WHERE prj.plantcode = pl.plantcode
                                 AND lower(status) IN
                                        ('not started', 'ongoing', 'hold'))
                            activeprojectscount,
                         (SELECT COUNT (tsk.projecttasksysid)
                            FROM    projects prj
                                 INNER JOIN
                                    projecttasks tsk
                                 ON tsk.projectno = prj.projectno
                           WHERE prj.plantcode = pl.plantcode
                                 AND lower(tsk.status) IN ('ongoing', 'hold'))
                            activetaskscount,
                         (SELECT COUNT (tsk.projecttasksysid)
                            FROM    projects prj
                                 INNER JOIN
                                       projecttasks tsk
                                    LEFT OUTER JOIN
                                       PRODUCTIONCALENDARS cal
                                    ON cal.calendarworkweek = tsk.targetstartworkweek
                                 ON tsk.projectno = prj.projectno
                           WHERE     prj.plantcode = pl.plantcode
                                 AND lower(tsk.status) IN ('ongoing', 'hold')
                                 AND cal.fiscaldate <= TRUNC (SYSDATE))
                            taskduecount,
                         (SELECT COUNT (1)
                            FROM    projects prj
                                 INNER JOIN
                                    projectproducts pd
                                 ON pd.projectno = prj.projectno
                           WHERE prj.plantcode = pl.plantcode)
                            productcount
                    FROM plants pl
                         INNER JOIN users cb
                            ON cb.userid = pl.createdby
                         LEFT OUTER JOIN users mb
                            ON mb.userid = pl.modifiedby
                   WHERE (LOWER (pl.plantcode) LIKE LOWER (:searchvalue) || '%'
                          OR LOWER (pl.plantname) LIKE
                                LOWER (:searchvalue) || '%')
                         AND (:isactivestate IS NULL
                              OR pl.ISACTIVE = :isactivestate)) l)
   WHERE rn BETWEEN :offset + 1 AND :offset + :pagesize
ORDER BY rn
";

            string countQuery = @"
SELECT COUNT(1)
FROM plants pl
WHERE (LOWER (pl.plantcode) LIKE LOWER (:searchvalue) || '%'
    OR LOWER (pl.plantname) LIKE  '%' || LOWER (:searchvalue) || '%')
    AND (:isactivestate IS NULL OR pl.ISACTIVE = :isactivestate)
";


            var parameters = new
            {
                searchvalue = searchValue,
                isactivestate = (isActive == null ? (char?)null : (isActive.Value ? '1' : '0')),
                offset = (pageNumber - 1) * pageSize,
                pagesize = pageSize
            };
            try
            {
                // Use Dapper's QueryAsync for mapping
                int totalRecords = await _dataAccess.ExecuteScalarAsync<int>(countQuery, parameters);

                var data = (await _dataAccess.QueryAsync<PlantWithStats>(pagedQuery, parameters)).ToList();

                return new PagedResult<PlantWithStats>
                {
                    TotalRecords = totalRecords,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }


    }
}
