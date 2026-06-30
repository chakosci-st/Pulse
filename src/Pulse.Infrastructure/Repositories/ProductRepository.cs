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
    public class ProductRepository : BaseRepository<Product, string>, IProductRepository
    {
        public ProductRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(Product product)
        {
            try
            {
                var returnvalue = await _dataAccess.SaveDataAsync<Product>(@"  INSERT INTO PRODUCTS
      (
        PRODUCTCODE, 
        PRODUCTTYPE,
        PRODUCTCURRENTMATURITY,
        ROUTING,
        ROUTINGDESCRIPTION,
        SUBROUTING,
        SUBROUTINGDESCRIPTION,
        PRODUCTLINK,
        PLANTCODE,
        PLANTTYPE,
        PLANTTYPEDESCRIPTION,
        PRODUCTFAMILYCODE,
        PRODUCTFAMILYDESCRIPTION,
        MACROPACKAGECODE,
        MACROPACKAGEDESCRIPTION,
        PACKAGECODE,
        PACKAGEDESCRIPTION,
        PRODUCTLINECODE,
        PRODUCTLINE,
        MATURITYCODE,
        CREATEDBY
      )
    VALUES
      (
        :PRODUCTCODE, 
        :PRODUCTTYPE,
        :PRODUCTCURRENTMATURITY,
        :ROUTING,
        :ROUTINGDESCRIPTION,
        :SUBROUTING,
        :SUBROUTINGDESCRIPTION,
        :PRODUCTLINK,
        :PLANTCODE,
        :PLANTTYPE,
        :PLANTTYPEDESCRIPTION,
        :PRODUCTFAMILYCODE,
        :PRODUCTFAMILYDESCRIPTION,
        :MACROPACKAGECODE,
        :MACROPACKAGEDESCRIPTION,
        :PACKAGECODE,
        :PACKAGEDESCRIPTION,
        :PRODUCTLINECODE,
        :PRODUCTLINE,
        :MATURITYCODE,
        :CREATEDBY
      ) 
", product);


                return product.ProductCode;
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("PRODUCTS_PK"))
                {
                    throw new Exception(e.Message.ToString());
                }
                else {
                    return product.ProductCode;
                }
            }

        }

        public override async Task<int> UpdateAsync(Product projectmember)
        {
            return await _dataAccess.SaveDataAsync<Product>(@"UPDATE PRODUCTS 
    SET PRODUCTTYPE = :PRODUCTTYPE,
        PRODUCTCURRENTMATURITY = :PRODUCTCURRENTMATURITY,
        ROUTING = :ROUTING,
        ROUTINGDESCRIPTION = :ROUTINGDESCRIPTION,
        SUBROUTING = :SUBROUTING,
        SUBROUTINGDESCRIPTION = :SUBROUTINGDESCRIPTION,
        PRODUCTLINK = :PRODUCTLINK,
        PLANTCODE = :PLANTCODE,
        PLANTTYPE = :PLANTTYPE,
        PLANTTYPEDESCRIPTION = :PLANTTYPEDESCRIPTION,
        PRODUCTFAMILYCODE = :PRODUCTFAMILYCODE,
        PRODUCTFAMILYDESCRIPTION = :PRODUCTFAMILYDESCRIPTION,
        MACROPACKAGECODE = :MACROPACKAGECODE,
        MACROPACKAGEDESCRIPTION = :MACROPACKAGEDESCRIPTION,
        PACKAGECODE = :PACKAGECODE,
        PACKAGEDESCRIPTION = :PACKAGEDESCRIPTION,
        PRODUCTLINECODE = :PRODUCTLINECODE,
        PRODUCTLINE = :PRODUCTLINE,
        MATURITYCODE = :MATURITYCODE,
        MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
                        WHERE PRODUCTCODE = :PRODUCTCODE AND TRANSACTIONKEY = :TRANSACTIONKEY", projectmember);
        }

        public override async Task<int> DeleteAsync(string productcode)
        {
            return await _dataAccess.SaveDataAsync<Product>("DELETE FROM PROJECTPRODUCTS WHERE PRODUCTCODE = :PRODUCTCODE", new Product { ProductCode = productcode });
        }

        public override async Task<IEnumerable<Product>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<Product>("SELECT * FROM PRODUCTS")
                  .ContinueWith(t => (IEnumerable<Product>)t.Result);
        }

        public override async Task<Product> GetAsync(string productcode)
        {
            return await _dataAccess.FindDataAsync<Product>("SELECT * FROM PRODUCTS WHERE PRODUCTCODE = :PRODUCTCODE",
new Product { ProductCode = productcode });
        }




        public async Task<Product> GetDetailsFromIEDBByProductCodePlantCodeAsync(string productcode, string plantcode)
        {
            return await _dataAccess.FindDataAsync<Product>(@"
SELECT DISTINCT ipt productcode,
                plt plantcode,
                mpf0 planttype,
                mpf0_d planttypedescription,
                mpf1 productfamilycode,
                mpf1_d productfamilydescription,
                mpf2 macropackagecode,
                mpf2_d macropackagedescription,
                mpf3 packagecode,
                mpf3_d packagedescription,
                ple_c productlinecode,
                ple productline,
                mss maturitycode,
                (SELECT projectno
                   FROM PROJECTPRODUCTS
                  WHERE productcode = ipt)
                   projectno
  FROM  CHK_REF_PROCESS_ITEMS@ECHECKLIST
 WHERE ipt = :productcode AND plt = :plantcode
",
new Product { ProductCode = productcode, PlantCode = plantcode });
        }
    }
}
