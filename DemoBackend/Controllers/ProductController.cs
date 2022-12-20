using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmQueryOptionsNs;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Controllers;

/// <summary>
/// CRUD for inmem Product Dictionary
/// </summary>

[Route("api/[controller]")]
[ApiController]
public class ProductController : DictionaryCrudBaseController<DemoModels.Product>
{
    public ProductController()
    {
        Database.DictionaryDatabase.InitRandomData();
        Table = Database.DictionaryDatabase.Products;
    }

    private class PatchProductExample : IExamplesProvider<Newtonsoft.Json.Linq.JObject>
    {
        public Newtonsoft.Json.Linq.JObject GetExamples()
        {
            var example = new
            {
                Name = "newName",
                Rating = 3
            };

            var res = Newtonsoft.Json.Linq.JObject.FromObject(example);


            return res;
        }
    }

    [HttpPatch("{id:Guid}")]
    [SwaggerRequestExample(typeof(Newtonsoft.Json.Linq.JObject), typeof(PatchProductExample))]
    public override IActionResult Patch(Guid id, [FromBody] Newtonsoft.Json.Linq.JObject patch)
    {
        var res = base.Patch(id, patch);
        return res;
    }


}
