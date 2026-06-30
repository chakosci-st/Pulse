using AutoMapper;
using Pulse.Core.Interfaces;
using Pulse.DataTransformationObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Pulse.Api.Controllers
{
    [RoutePrefix("api/products")]
    public class ProductsController : ApiController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }
 


        [HttpGet]
        
        [Route("iedb/{productcode:string}/{plantcode:string}")]
        [Route("iedb")]
        [Route("{productcode:string}/{plantcode:string}")]
        [Route("")]
        public async Task<IHttpActionResult> GetByProductCodePlantCode(string productcode, string plantcode)
        {
            var obj = await _productService.GetDetailsFromIEDBByProductCodePlantCodeAsync(productcode.ToUpper(), plantcode);
            if (obj == null)
                return NotFound();

            return Ok(Mapper.Map<dtoProduct>(obj));
        }
    }
}
