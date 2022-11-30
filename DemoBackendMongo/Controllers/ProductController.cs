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
    public class ProductController : MongoSmController<DemoModels.Product>
    {
        public ProductController()
        {
            DemoModels.MongoDatabase.InitRandomData();
            Table = DemoModels.MongoDatabase.Products;
            int? r = 5;
            var iii1 = NullableTypeHelper.ParseNullable<int?>("3");
            var iii3 = NullableTypeHelper.ParseNullable<int?>(null);
            var iii4 = NullableTypeHelper.ParseNullable<int?>("");
            var iii2 = NullableTypeHelper.ParseNullable<int?>("null");
            ;

            string? ss = null;
            
            //int?.ParseNullableInt(r.ToString());
            var ty = r.GetType();
            //var a = typeof(int).Parse("3");

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
            if (smQueryOptions.Search == null)
                return Table.Find(x => true);
            var res = Table.Find(x => 
                    x.Name.ToLowerInvariant().Contains(smQueryOptions.Search.ToLowerInvariant()) 
                    || x.Code.ToLowerInvariant().StartsWith(smQueryOptions.Search.ToLowerInvariant())
                );


            return res;
        }

    }
}
