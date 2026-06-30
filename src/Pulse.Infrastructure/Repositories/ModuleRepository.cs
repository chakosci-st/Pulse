
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
    public class ModuleRepository : BaseRepository<Module, string>, IModuleRepository
    {

 
        public ModuleRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(Module module)
        {
            var returnvalue =  await _dataAccess.SaveDataAsync<Module>(@"INSERT INTO MODULES (MODULECODE, MODULENAME, MODULEDESCRIPTION, WITHREAD, WITHWRITE, ISACTIVE, CREATEDBY) 
VALUES (:MODULECODE, :MODULENAME, :MODULEDESCRIPTION, :WITHREAD, :WITHWRITE, :ISACTIVE, :CREATEDBY)", module);

            return module.ModuleCode;
        }

        public override async Task<int> UpdateAsync(Module module)
        {
            return await _dataAccess.SaveDataAsync<Module>(@"UPDATE MODULES 
                        SET MODULENAME = :MODULENAME, MODULEDESCRIPTION = :MODULEDESCRIPTION, WITHREAD = :WITHREAD, WITHWRITE = :WITHWRITE, ISACTIVE = :ISACTIVE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
                        WHERE MODULECODE = :MODULECODE AND TRANSACTIONKEY = :TRANSACTIONKEY", module);
        }

        public override async Task<int> DeleteAsync(string modulecode)
        {
            return await _dataAccess.SaveDataAsync<Module>("DELETE FROM MODULES WHERE MODULECODE = :MODULECODE", new Module { ModuleCode = modulecode });
        }

        public override async Task<IEnumerable<Module>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<Module>("SELECT * FROM MODULES")
                .ContinueWith(t => (IEnumerable<Module>)t.Result);
        }

        public async Task<PagedResult<Module>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            string pagedQuery = $@"
  SELECT *
    FROM (SELECT l.*, ROW_NUMBER () OVER (ORDER BY {sortBy} {sortDirection}) rn
            FROM (SELECT m.*,
                         cb.firstname || ' ' || cb.lastname createdbyname,
                         mb.firstname || ' ' || mb.lastname modifiedbyname
                    FROM modules m
                         INNER JOIN users cb
                            ON cb.userid = m.createdby
                         LEFT OUTER JOIN users mb
                            ON mb.userid = m.modifiedby
                   WHERE (LOWER (m.modulecode) LIKE LOWER (:searchvalue) || '%'
                          OR LOWER (m.modulename) LIKE
                                LOWER (:searchvalue) || '%')
                         AND (:isactivestate IS NULL
                              OR m.ISACTIVE = :isactivestate)) l)
   WHERE rn BETWEEN :offset + 1 AND :offset + :pagesize
ORDER BY rn 
";
            string countQuery = @"
SELECT COUNT(1)
FROM modules m
WHERE (LOWER (m.modulecode) LIKE LOWER (:searchvalue) || '%'
    OR LOWER (m.modulename) LIKE  '%' || LOWER (:searchvalue) || '%')
    AND (:isactivestate IS NULL OR m.ISACTIVE = :isactivestate)
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

                var data = (await _dataAccess.QueryAsync<Module>(pagedQuery, parameters)).ToList();

                return new PagedResult<Module>
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
        public Module Get(string modulecode)
        {
            return _dataAccess.FindData<Module>("SELECT * FROM MODULES WHERE MODULECODE = :MODULECODE", new Module { ModuleCode = modulecode });
        }

        public override async Task<Module> GetAsync(string modulecode)
        {
            return await _dataAccess.FindDataAsync<Module>("SELECT * FROM MODULES WHERE MODULECODE = :MODULECODE", 
                new Module { ModuleCode = modulecode });
        }
 
    }
}
