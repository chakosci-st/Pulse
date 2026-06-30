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
    public class MaturityLevelRepository : BaseRepository<MaturityLevel, string>, IMaturityLevelRepository
    {


        public MaturityLevelRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(MaturityLevel maturitylevel)
        {
            var returnvalue = await _dataAccess.SaveDataAsync<MaturityLevel>(@"
INSERT INTO MATURITYLEVELS (MATURITYCODE, MATURITYNUMBER, SEQUENCENO, ISACTIVE, CREATEDBY) 
VALUES (:MATURITYCODE, :MATURITYNUMBER, :SEQUENCENO, :ISACTIVE, :CREATEDBY) ", maturitylevel);

            return maturitylevel.MaturityCode;
        }

        public override async Task<int> UpdateAsync(MaturityLevel maturitylevel)
        {
            return await _dataAccess.SaveDataAsync<MaturityLevel>("UPDATE MATURITYLEVELS SET MATURITYNUMBER = :MATURITYNUMBER, SEQUENCENO = :SEQUENCENO, ISACTIVE = :ISACTIVE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() WHERE MATURITYCODE = :MATURITYCODE AND TRANSACTIONKEY = :TRANSACTIONKEY", maturitylevel);
        }

        public override async Task<int> DeleteAsync(string maturitycode)
        {
            return await _dataAccess.SaveDataAsync<MaturityLevel>("DELETE FROM MATURITYLEVELS WHERE MATURITYCODE = :MATURITYCODE", new MaturityLevel { MaturityCode = maturitycode });
        }

        public override async Task<IEnumerable<MaturityLevel>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<MaturityLevel>("SELECT * FROM MATURITYLEVELS")
                .ContinueWith(t => (IEnumerable<MaturityLevel>)t.Result);
        }

        public override async Task<MaturityLevel> GetAsync(string maturitycode)
        {
            return await _dataAccess.FindDataAsync<MaturityLevel>("SELECT * FROM MATURITYLEVELS WHERE MATURITYCODE = :MATURITYCODE", new MaturityLevel { MaturityCode = maturitycode });
        }
        public MaturityLevel Get(string maturitycode)
        {
            return _dataAccess.FindData<MaturityLevel>("SELECT * FROM MATURITYLEVELS WHERE MATURITYCODE = :MATURITYCODE", new MaturityLevel { MaturityCode = maturitycode });
        }
        public async Task<PagedResult<MaturityLevelWithStats>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            string pagedQuery = $@"
  SELECT *
    FROM (SELECT l.*, ROW_NUMBER () OVER (ORDER BY  {sortBy} {sortDirection}) rn
            FROM (SELECT ml.*,
                         cb.firstname || ' ' || cb.lastname createdbyname,
                         mb.firstname || ' ' || mb.lastname modifiedbyname 
                    FROM maturitylevels ml
                         INNER JOIN users cb
                            ON cb.userid = ml.createdby
                         LEFT OUTER JOIN users mb
                            ON mb.userid = ml.modifiedby
                   WHERE (LOWER (ml.maturitycode) LIKE LOWER (:searchvalue) || '%')
                         AND (:isactivestate IS NULL
                              OR ml.ISACTIVE = :isactivestate)) l)
   WHERE rn BETWEEN :offset + 1 AND :offset + :pagesize
ORDER BY rn
";

            string countQuery = @"
SELECT COUNT(1)
FROM maturitylevels ml
WHERE (LOWER (ml.maturitycode) LIKE LOWER (:searchvalue) || '%')
    AND (:isactivestate IS NULL OR ml.ISACTIVE = :isactivestate)
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

                var data = (await _dataAccess.QueryAsync<MaturityLevelWithStats>(pagedQuery, parameters)).ToList();

                return new PagedResult<MaturityLevelWithStats>
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
