using Common;
using Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace ProductApi
{

    [Route("[controller]")]
    public class ProductOdataController : DictionaryODataController<DemoModels.Product>
    {
        public ProductOdataController()
        {
            Database.DictionaryDatabase.InitRandomData();
            Table = Database.DictionaryDatabase.Products;
        }

        protected override IQueryable<DemoModels.Product> ApplySearchFilter(IQueryable<DemoModels.Product> q1, string keywords)
        {
            var q2 = q1
                .Where(x => 
                x.Name.StartsWith(keywords, StringComparison.InvariantCultureIgnoreCase) 
                || (x.Ext != null && x.Ext.Description != null && x.Ext.Description.StartsWith(keywords, StringComparison.InvariantCultureIgnoreCase))

                );
            return q2;
        }
    }

}
