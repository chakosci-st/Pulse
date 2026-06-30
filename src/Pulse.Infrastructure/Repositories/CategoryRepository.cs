
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
    public class CategoryRepository : BaseRepository<Category, string>, ICategoryRepository
    {

        public CategoryRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(Category category)
        {
            var affectedrows =  await _dataAccess.SaveDataAsync<Category>(@"INSERT INTO CATEGORIES (CATEGORYCODE, CATEGORYNAME, CATEGORYDESCRIPTION, ISACTIVE, CREATEDBY) 
VALUES (:CATEGORYCODE, :CATEGORYNAME, :CATEGORYDESCRIPTION, :ISACTIVE, :CREATEDBY) ", category);

            return category.CategoryCode;
        }

        public override async Task<int> UpdateAsync(Category category)
        {
            return await _dataAccess.SaveDataAsync<Category>("UPDATE CATEGORIES SET CATEGORYNAME = :CATEGORYNAME, CATEGORYDESCRIPTION = :CATEGORYDESCRIPTION, ISACTIVE = :ISACTIVE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() WHERE CATEGORYCODE = :CATEGORYCODE AND TRANSACTIONKEY = :TRANSACTIONKEY", category);
        }

        public override async Task<int> DeleteAsync(string categorycode)
        {
            return await _dataAccess.SaveDataAsync<Category>("DELETE FROM CATEGORIES WHERE CATEGORYCODE = :CATEGORYCODE", new Category { CategoryCode = categorycode });
        }
        public Category Get(string categorycode)
        {
            return _dataAccess.FindData<Category>("SELECT * FROM CATEGORIES WHERE CATEGORYCODE = :CATEGORYCODE", new Category { CategoryCode = categorycode });
        }
        public override async Task<Category> GetAsync(string categorycode)
        {
            return await _dataAccess.FindDataAsync<Category>("SELECT * FROM CATEGORIES WHERE CATEGORYCODE = :CATEGORYCODE", new Category { CategoryCode = categorycode });
        }
        public override async Task<IEnumerable<Category>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<Category>("SELECT * FROM CATEGORIES")
                .ContinueWith(t => (IEnumerable<Category>)t.Result);
        }

        public async Task<PagedResult<CategoryWithStats>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            string pagedQuery = $@"
  SELECT *
    FROM (SELECT l.*, ROW_NUMBER () OVER (ORDER BY {sortBy} {sortDirection}) rn
            FROM (SELECT cat.*,
                         cb.firstname || ' ' || cb.lastname createdbyname,
                         mb.firstname || ' ' || mb.lastname modifiedbyname,
                         (SELECT COUNT (1)
                            FROM projects prj
                           WHERE prj.categorycode = cat.categorycode
                                 AND LOWER(status) IN
                                        ('not started', 'ongoing', 'hold'))
                            activeprojectscount,
                         (SELECT COUNT (tsk.projecttasksysid)
                            FROM    projects prj
                                 INNER JOIN
                                    projecttasks tsk
                                 ON tsk.projectno = prj.projectno
                           WHERE cat.categorycode = cat.categorycode
                                 AND LOWER(tsk.status) IN ('ongoing', 'hold'))
                            activetaskscount,
                         (SELECT COUNT (tsk.projecttasksysid)
                            FROM    projects prj
                                 INNER JOIN
                                       projecttasks tsk
                                    LEFT OUTER JOIN
                                       PRODUCTIONCALENDARS cal
                                    ON cal.calendarworkweek = tsk.targetstartworkweek
                                 ON tsk.projectno = prj.projectno
                           WHERE     cat.categorycode = cat.categorycode
                                 AND lower(tsk.status) IN ('ongoing', 'hold')
                                 AND cal.fiscaldate <= TRUNC (SYSDATE))
                            taskduecount,
                         (SELECT COUNT (1)
                            FROM    projects prj
                                 INNER JOIN
                                    PROJECTPRODUCTS pd
                                 ON pd.projectno = prj.projectno
                           WHERE prj.categorycode = cat.categorycode)
                            productcount
                    FROM categories cat
                         INNER JOIN users cb
                            ON cb.userid = cat.createdby
                         LEFT OUTER JOIN users mb
                            ON mb.userid = cat.modifiedby
                   WHERE (LOWER (cat.categorycode) LIKE
                             LOWER (:searchvalue) || '%'
                          OR LOWER (cat.categoryname) LIKE
                                LOWER (:searchvalue) || '%')
                         AND (:isactivestate IS NULL
                              OR cat.ISACTIVE = :isactivestate)) l)
   WHERE rn BETWEEN :offset + 1 AND :offset + :pagesize
ORDER BY rn 
";

            string countQuery = @"
SELECT COUNT(1)
FROM categories cat 
WHERE (LOWER (cat.categorycode) LIKE LOWER (:searchvalue) || '%'
    OR LOWER (cat.categoryname) LIKE  '%' || LOWER (:searchvalue) || '%')
    AND (:isactivestate IS NULL OR cat.ISACTIVE = :isactivestate)
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

                var data = (await _dataAccess.QueryAsync<CategoryWithStats>(pagedQuery, parameters)).ToList();

                return new PagedResult<CategoryWithStats>
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
