using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using SmQueryOptionsNs;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductMongoController : MongoSmController<DemoModels.Product, DemoModels.ProductDto>
    {
        public ProductMongoController()
        {
            Database.MongoDatabase.InitRandomData();
            Table = Database.MongoDatabase.Products;
        }

        /*protected override IFindFluent<DemoModels.Product, DemoModels.Product> GetFilteredQuery(SmQueryOptions? smQueryOptions)
        {
            if (smQueryOptions.Search == null)
                return Table.Find(x => true);
            var fo = new FindOptions() { Collation = new Collation("hu", strength: CollationStrength.Primary) };
            var filterbuilder = Builders<DemoModels.Product>.Filter;
            var searchFilter =
               filterbuilder.Gte(x => x.Name, smQueryOptions.Search)
               & filterbuilder.Lte(x => x.Name, smQueryOptions.Search + "zzzz")
               ;

            var res = Table.Find(searchFilter, fo);


            return res;
        }*/
        protected override IFindFluent<DemoModels.Product, DemoModels.Product> GetFilteredQuery(SmQueryOptions? smQueryOptions)
        {
            if (smQueryOptions?.Search == null)
                return Table.Find(x => true);
            var res = Table.Find(x =>
                    (x.Name != null && x.Name.ToLowerInvariant().Contains(smQueryOptions.Search.ToLowerInvariant()))
                    || 
                    (x.Code != null && x.Code.ToLowerInvariant().StartsWith(smQueryOptions.Search.ToLowerInvariant()))
                );

            return res;
        }

        protected override DemoModels.ProductDto ProjectResultItem(DemoModels.Product x, SmQueryOptions smQueryOptions)
        {
            var res = new DemoModels.ProductDto();
            SmQueryOptionsNs.Mapper.CopyProperties(x, res, false, false, smQueryOptions.Select);
            if (smQueryOptions.Select?.Contains("StockSumQuantity".ToLower())??true)
                res.StockSumQuantity = x.Stocks?.Sum(x => x.Quantity);
            if (smQueryOptions.Select?.Contains("Description".ToLower()) ?? true)
                res.Description = x.Ext?.Description;
            return res;
        }


    }
}
