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
    public class ProductController : MongoSmController<DemoModels.Product, DemoModels.ProductDto>
    {
        public ProductController()
        {
            DemoModels.MongoDatabase.InitRandomData();
            Table = DemoModels.MongoDatabase.Products;
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

        /*protected override IFindFluent<DemoModels.Product, MongoDB.Bson.BsonDocument>? GetFieldSelectQuery(IFindFluent<DemoModels.Product, DemoModels.Product>? query, SmQueryOptions? smQueryOptions)
        {
            var fieldList = new List<string>() { "Id", "Name", "Code" };
            var project = Builders<DemoModels.Product>.Projection.Combine(fieldList.Select(x => Builders<DemoModels.Product>.Projection.Include(x)).ToList());

            //var query2 = query.Project<DemoModels.Product>(x => new { x.Name, x.Code });
            IFindFluent<DemoModels.Product, MongoDB.Bson.BsonDocument>? query2 = query.Project(project);
            var res21312 = query2.ToList();


            return query2;
        }*/
        protected override DemoModels.ProductDto ProjectResultItem(DemoModels.Product x, SmQueryOptions? smQueryOptions)
        {
            var res = new DemoModels.ProductDto();
            SmQueryOptionsNs.Mapper.CopyProperties(x, res, false, false, smQueryOptions.Select);
            res.StockSumQuantity = x.Stocks?.Sum(x => x.Quantity);
            return res;
        }


    }
}
