
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
    public class ProductGroupRepository : BaseRepository<ProductGroup, string>, IProductGroupRepository
    {
        public ProductGroupRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(ProductGroup productgroup)
        {
            var returnvalue= await _dataAccess.SaveDataAsync<ProductGroup>(@"INSERT INTO PRODUCTGROUPS (PRODUCTGROUPCODE, PRODUCTGROUPNAME, PRODUCTGROUPDESCRIPTION, ISACTIVE, CREATEDBY) 
VALUES (:PRODUCTGROUPCODE, :PRODUCTGROUPNAME, :PRODUCTGROUPDESCRIPTION, :ISACTIVE, :CREATEDBY) 
", productgroup);
            return productgroup.ProductGroupCode;
        }

        public override async Task<int> UpdateAsync(ProductGroup productgroup)
        {
            return await _dataAccess.SaveDataAsync<ProductGroup>(@"UPDATE PRODUCTGROUPS 
                        SET PRODUCTGROUPNAME = :PRODUCTGROUPNAME, PRODUCTGROUPDESCRIPTION = :PRODUCTGROUPDESCRIPTION, ISACTIVE = :ISACTIVE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
                        WHERE PRODUCTGROUPCODE = :PRODUCTGROUPCODE AND TRANSACTIONKEY = :TRANSACTIONKEY", productgroup);
        }

        public override async Task<int> DeleteAsync(string productgroupcode)
        {
            return await _dataAccess.SaveDataAsync<ProductGroup>("DELETE FROM PRODUCTGROUPS WHERE PRODUCTGROUPCODE = :PRODUCTGROUPCODE", new ProductGroup { ProductGroupCode = productgroupcode });
        }

        public override async Task<IEnumerable<ProductGroup>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<ProductGroup>("SELECT * FROM PRODUCTGROUPS")
                  .ContinueWith(t => (IEnumerable<ProductGroup>)t.Result);
        }

        public override async Task<ProductGroup> GetAsync(string productgroupcode)
        {
            return await _dataAccess.FindDataAsync<ProductGroup>("SELECT * FROM PRODUCTGROUPS WHERE PRODUCTGROUPCODE = :PRODUCTGROUPCODE", 
                new ProductGroup { ProductGroupCode = productgroupcode });
        }

        public ProductGroup Get(string productgroupcode)
        {
            return _dataAccess.FindData<ProductGroup>("SELECT * FROM PRODUCTGROUPS WHERE PRODUCTGROUPCODE = :PRODUCTGROUPCODE", new ProductGroup { ProductGroupCode = productgroupcode });
        }
        public async Task<PagedResult<ProductGroupWithStats>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            string pagedQuery = $@"
  SELECT *
    FROM (SELECT l.*, ROW_NUMBER () OVER (ORDER BY  {sortBy} {sortDirection}) rn
            FROM (SELECT pg.*,
                         cb.firstname || ' ' || cb.lastname createdbyname,
                         mb.firstname || ' ' || mb.lastname modifiedbyname 
                    FROM productgroups pg
                         INNER JOIN users cb
                            ON cb.userid = pg.createdby
                         LEFT OUTER JOIN users mb
                            ON mb.userid = pg.modifiedby
                   WHERE (LOWER (pg.productgroupcode) LIKE LOWER (:searchvalue) || '%'
                          OR LOWER (pg.productgroupname) LIKE
                                LOWER (:searchvalue) || '%')
                         AND (:isactivestate IS NULL
                              OR pg.ISACTIVE = :isactivestate)) l)
   WHERE rn BETWEEN :offset + 1 AND :offset + :pagesize
ORDER BY rn
";

            string countQuery = @"
SELECT COUNT(1)
FROM productgroups pg
WHERE (LOWER (pg.productgroupcode) LIKE LOWER (:searchvalue) || '%'
    OR LOWER (pg.productgroupname) LIKE  '%' || LOWER (:searchvalue) || '%')
    AND (:isactivestate IS NULL OR pg.ISACTIVE = :isactivestate)
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

                var data = (await _dataAccess.QueryAsync<ProductGroupWithStats>(pagedQuery, parameters)).ToList();

                return new PagedResult<ProductGroupWithStats>
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
