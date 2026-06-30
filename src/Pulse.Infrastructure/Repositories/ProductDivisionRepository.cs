
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
    public class ProductDivisionRepository : BaseRepository<ProductDivision, string>, IProductDivisionRepository
    {
        public ProductDivisionRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(ProductDivision productdivision)
        {
            var returnvalue = await _dataAccess.SaveDataAsync<ProductDivision>(@"INSERT INTO PRODUCTDIVISIONS (PRODUCTDIVISIONCODE, PRODUCTDIVISIONNAME, PRODUCTDIVISIONDESCRIPTION, ISACTIVE, CREATEDBY) 
VALUES (:PRODUCTDIVISIONCODE, :PRODUCTDIVISIONNAME, :PRODUCTDIVISIONDESCRIPTION, :ISACTIVE, :CREATEDBY)", productdivision);

            return productdivision.ProductDivisionCode;
        }

        public override async Task<int> UpdateAsync(ProductDivision productdivision)
        {
            return await _dataAccess.SaveDataAsync<ProductDivision>(@"UPDATE PRODUCTDIVISIONS 
                        SET PRODUCTDIVISIONNAME = :PRODUCTDIVISIONNAME, PRODUCTDIVISIONDESCRIPTION = :PRODUCTDIVISIONDESCRIPTION, ISACTIVE = :ISACTIVE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
                        WHERE PRODUCTDIVISIONCODE = :PRODUCTDIVISIONCODE AND TRANSACTIONKEY = :TRANSACTIONKEY", productdivision);
        }

        public override async Task<int> DeleteAsync(string productdivisioncode)
        {
            return await _dataAccess.SaveDataAsync<ProductDivision>("DELETE FROM PRODUCTDIVISIONS WHERE PRODUCTDIVISIONCODE = :PRODUCTDIVISIONCODE", new ProductDivision { ProductDivisionCode = productdivisioncode });
        }

        public override async Task<IEnumerable<ProductDivision>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<ProductDivision>("SELECT * FROM PRODUCTDIVISIONS")
                .ContinueWith(t => (IEnumerable<ProductDivision>)t.Result);
        }

        public override async Task<ProductDivision> GetAsync(string productdivisioncode)
        {
            return await _dataAccess.FindDataAsync<ProductDivision>("SELECT * FROM PRODUCTDIVISIONS WHERE PRODUCTDIVISIONCODE = :PRODUCTDIVISIONCODE",
                new ProductDivision { ProductDivisionCode = productdivisioncode });
        }
        public ProductDivision Get(string productdivisioncode)
        {
            return _dataAccess.FindData<ProductDivision>("SELECT * FROM PRODUCTDIVISIONS WHERE PRODUCTDIVISIONCODE = :PRODUCTDIVISIONCODE", new ProductDivision { ProductDivisionCode = productdivisioncode });
        }
        public async Task<PagedResult<ProductDivisionWithStats>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            string pagedQuery = $@"
  SELECT *
    FROM (SELECT l.*, ROW_NUMBER () OVER (ORDER BY  {sortBy} {sortDirection}) rn
            FROM (SELECT pd.*,
                         cb.firstname || ' ' || cb.lastname createdbyname,
                         mb.firstname || ' ' || mb.lastname modifiedbyname 
                    FROM productdivisions pd
                         INNER JOIN users cb
                            ON cb.userid = pd.createdby
                         LEFT OUTER JOIN users mb
                            ON mb.userid = pd.modifiedby
                   WHERE (LOWER (pd.productdivisioncode) LIKE LOWER (:searchvalue) || '%'
                          OR LOWER (pd.productdivisionname) LIKE
                                LOWER (:searchvalue) || '%')
                         AND (:isactivestate IS NULL
                              OR pd.ISACTIVE = :isactivestate)) l)
   WHERE rn BETWEEN :offset + 1 AND :offset + :pagesize
ORDER BY rn
";

            string countQuery = @"
SELECT COUNT(1)
FROM productdivisions pd
WHERE (LOWER (pd.productdivisioncode) LIKE LOWER (:searchvalue) || '%'
    OR LOWER (pd.productdivisionname) LIKE  '%' || LOWER (:searchvalue) || '%')
    AND (:isactivestate IS NULL OR pd.ISACTIVE = :isactivestate)
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

                var data = (await _dataAccess.QueryAsync<ProductDivisionWithStats>(pagedQuery, parameters)).ToList();

                return new PagedResult<ProductDivisionWithStats>
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
