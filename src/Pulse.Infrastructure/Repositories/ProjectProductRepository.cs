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
    public class ProjectProductRepository : BaseRepository<ProjectProduct, string>, IProjectProductRepository
    {
        public ProjectProductRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(ProjectProduct product)
        {
            var returnvalue = await _dataAccess.SaveDataAsync<ProjectProduct>(@" INSERT INTO PROJECTPRODUCTS
      (
        PRODUCTCODE,
        PROJECTNO
      )
    VALUES
      (
        :PRODUCTCODE,
        :PROJECTNO
      ) 
", product);


            return product.ProductCode;
        }


        public override async Task<int> DeleteAsync(string productcode)
        {
            return await _dataAccess.SaveDataAsync<ProjectProduct>("DELETE FROM PROJECTPRODUCTS WHERE PRODUCTCODE = :PRODUCTCODE", new ProjectProduct { ProductCode = productcode });
        }



        public override async Task<ProjectProduct> GetAsync(string productcode)
        {

            var returnvalue =
                                     await _dataAccess.GetMappedDataAsync<ProjectProduct, Product, ProjectProduct>(
                           dataQuery: @"
SELECT pp.projectno,
       pp.productcode,
       pr.producttype,
       pr.productcurrentmaturity,
       pr.routing,
       pr.routingdescription,
       pr.subrouting,
       pr.subroutingdescription,
       pr.productlink,
       pr.plantcode,
       pr.planttype,
       pr.planttypedescription,
       pr.productfamilycode,
       pr.productfamilydescription,
       pr.macropackagecode,
       pr.macropackagedescription,
       pr.packagecode,
       pr.packagedescription,
       pr.productlinecode,
       pr.productline,
       pr.maturitycode,
       pr.createdby,
       pr.createddate,
       pr.modifiedby,
       pr.modifieddate,
       pr.transactionkey
  FROM    projectproducts pp
       INNER JOIN
          products pr
       ON pr.productcode = pp.productcode WHERE pp.productcode = :productcode",
                           parameters: new { ProductCode = productcode },
                           map: (pp, pr) => new ProjectProduct
                           {
                               ProjectNo = pp.ProjectNo,
                               ProductCode = pr.ProductCode,
                               // From Product
                               Product = new Product
                               {
                                   ProductCode = pr.ProductCode,
                                   ProductType = pr.ProductType,
                                   ProductCurrentMaturity = pr.ProductCurrentMaturity,
                                   Routing = pr.Routing,
                                   RoutingDescription = pr.RoutingDescription,
                                   SubRouting = pr.SubRouting,
                                   SubRoutingDescription = pr.SubRoutingDescription,
                                   ProductLink = pr.ProductLink,
                                   PlantCode = pr.PlantCode,
                                   PlantType = pr.PlantType,
                                   PlantTypeDescription = pr.PlantTypeDescription,
                                   ProductFamilyCode = pr.ProductFamilyCode,
                                   ProductFamilyDescription = pr.ProductFamilyDescription,
                                   MacroPackageCode = pr.MacroPackageCode,
                                   MacroPackageDescription = pr.MacroPackageDescription,
                                   PackageCode = pr.PackageCode,
                                   PackageDescription = pr.PackageDescription,
                                   ProductLineCode = pr.ProductLineCode,
                                   ProductLine = pr.ProductLine,
                                   MaturityCode = pr.MaturityCode,
                                   CreatedBy = pr.CreatedBy,
                                   CreatedDate = pr.CreatedDate,
                                   ModifiedBy = pr.ModifiedBy,
                                   ModifiedDate = pr.ModifiedDate,

                               }
                           },
                           splitOn: "productcode"
                       );

            return returnvalue.FirstOrDefault();
        }
        public async Task<ProjectProduct> GetAsync(string projectno, string productcode)
        {

            var returnvalue = await _dataAccess.GetMappedDataAsync<ProjectProduct, Product, ProjectProduct>(
                           dataQuery: @"
SELECT pp.projectno,
       pp.productcode,
       pr.producttype,
       pr.productcurrentmaturity,
       pr.routing,
       pr.routingdescription,
       pr.subrouting,
       pr.subroutingdescription,
       pr.productlink,
       pr.plantcode,
       pr.planttype,
       pr.planttypedescription,
       pr.productfamilycode,
       pr.productfamilydescription,
       pr.macropackagecode,
       pr.macropackagedescription,
       pr.packagecode,
       pr.packagedescription,
       pr.productlinecode,
       pr.productline,
       pr.maturitycode,
       pr.createdby,
       pr.createddate,
       pr.modifiedby,
       pr.modifieddate,
       pr.transactionkey
  FROM    projectproducts pp
       INNER JOIN
          products pr
       ON pr.productcode = pp.productcode WHERE pp.projectno = :PROJECTNO AND pp.productcode = :productcode",
                           parameters: new { ProjectNo = projectno, ProductCode = productcode },
                           map: (pp, pr) => new ProjectProduct
                           {
                               ProjectNo = pp.ProjectNo,
                               ProductCode = pr.ProductCode, 
                               // From Product
                               Product = new Product
                               {
                                   ProductCode = pr.ProductCode,
                                   ProductType = pr.ProductType,
                                   ProductCurrentMaturity = pr.ProductCurrentMaturity,
                                   Routing = pr.Routing,
                                   RoutingDescription = pr.RoutingDescription,
                                   SubRouting = pr.SubRouting,
                                   SubRoutingDescription = pr.SubRoutingDescription,
                                   ProductLink = pr.ProductLink,
                                   PlantCode = pr.PlantCode,
                                   PlantType = pr.PlantType,
                                   PlantTypeDescription = pr.PlantTypeDescription,
                                   ProductFamilyCode = pr.ProductFamilyCode,
                                   ProductFamilyDescription = pr.ProductFamilyDescription,
                                   MacroPackageCode = pr.MacroPackageCode,
                                   MacroPackageDescription = pr.MacroPackageDescription,
                                   PackageCode = pr.PackageCode,
                                   PackageDescription = pr.PackageDescription,
                                   ProductLineCode = pr.ProductLineCode,
                                   ProductLine = pr.ProductLine,
                                   MaturityCode = pr.MaturityCode,
                                   CreatedBy = pr.CreatedBy,
                                   CreatedDate = pr.CreatedDate,
                                   ModifiedBy = pr.ModifiedBy,
                                   ModifiedDate = pr.ModifiedDate,

                               }
                           },
                           splitOn: "productcode"
                       );

            return returnvalue.FirstOrDefault();
        }



        public async Task<IEnumerable<ProjectProduct>> GetListAsync(string projectno)
        {

            var returnvalue =
                                     await _dataAccess.GetMappedDataAsync<ProjectProduct, Product, ProjectProduct>(
                           dataQuery: @"
SELECT pp.projectno,
       pp.productcode,
       pr.producttype,
       pr.productcurrentmaturity,
       pr.routing,
       pr.routingdescription,
       pr.subrouting,
       pr.subroutingdescription,
       pr.productlink,
       pr.plantcode,
       pr.planttype,
       pr.planttypedescription,
       pr.productfamilycode,
       pr.productfamilydescription,
       pr.macropackagecode,
       pr.macropackagedescription,
       pr.packagecode,
       pr.packagedescription,
       pr.productlinecode,
       pr.productline,
       pr.maturitycode,
       pr.createdby,
       pr.createddate,
       pr.modifiedby,
       pr.modifieddate,
       pr.transactionkey
  FROM    projectproducts pp
       INNER JOIN
          products pr
       ON pr.productcode = pp.productcode WHERE pp.projectno = :PROJECTNO",
                           parameters: new { ProjectNo = projectno },
                           map: (pp, pr) => new ProjectProduct
                           {
                               ProjectNo = pp.ProjectNo,
                               ProductCode = pr.ProductCode, 
                               // From Product
                               Product = new Product
                               {
                                   ProductCode = pr.ProductCode,
                                   ProductType = pr.ProductType,
                                   ProductCurrentMaturity = pr.ProductCurrentMaturity,
                                   Routing = pr.Routing,
                                   RoutingDescription = pr.RoutingDescription,
                                   SubRouting = pr.SubRouting,
                                   SubRoutingDescription = pr.SubRoutingDescription,
                                   ProductLink = pr.ProductLink,
                                   PlantCode = pr.PlantCode,
                                   PlantType = pr.PlantType,
                                   PlantTypeDescription = pr.PlantTypeDescription,
                                   ProductFamilyCode = pr.ProductFamilyCode,
                                   ProductFamilyDescription = pr.ProductFamilyDescription,
                                   MacroPackageCode = pr.MacroPackageCode,
                                   MacroPackageDescription = pr.MacroPackageDescription,
                                   PackageCode = pr.PackageCode,
                                   PackageDescription = pr.PackageDescription,
                                   ProductLineCode = pr.ProductLineCode,
                                   ProductLine = pr.ProductLine,
                                   MaturityCode = pr.MaturityCode,
                                   CreatedBy = pr.CreatedBy,
                                   CreatedDate = pr.CreatedDate,
                                   ModifiedBy = pr.ModifiedBy,
                                   ModifiedDate = pr.ModifiedDate,

                               }
                           },
                           splitOn: "productcode"
                       );

            return returnvalue;

        }





        public override Task<int> UpdateAsync(ProjectProduct entity) => throw new NotImplementedException();

        public override Task<IEnumerable<ProjectProduct>> GetListAsync() => throw new NotImplementedException();
    }
}
