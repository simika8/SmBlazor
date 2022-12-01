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
            Database.SmDemoProductMongoDatabase.InitRandomData();
            Table = Database.SmDemoProductMongoDatabase.Product;
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
            var query = Table.Find(x =>
                    (x.Name != null && x.Name.ToLower().Contains(smQueryOptions.Search.ToLower()))
                    ||
                    (x.Code != null && x.Code.ToLower().StartsWith(smQueryOptions.Search.ToLower()))
                );

            return query;
        }

        protected override DemoModels.ProductDto ProjectResultItem(DemoModels.Product x, SmQueryOptions smQueryOptions)
        {
            var res = new DemoModels.ProductDto();
            SmQueryOptionsNs.Mapper.CopyProperties(x, res, false, false, smQueryOptions.Select);
            if (smQueryOptions.Select?.Contains("StockSumQuantity".ToLower()) ?? true)
                res.StockSumQuantity = (x.Stocks?.Count() > 0) ? x.Stocks?.Sum(x => x.Quantity) : null;
            if (smQueryOptions.Select?.Contains("Description".ToLower()) ?? true)
                res.Description = x.Ext?.Description;
            return res;
        }


    }
}
