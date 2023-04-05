using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmQueryOptionsNs;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Reporitory;

namespace Controllers;

/// <summary>
/// CRUD for inmem Product Dictionary
/// </summary>

[Route("api/[controller]")]
[ApiController]
public class ProductController : CrudBaseController<DemoModels.Product>
{
    public ProductController()
    {
        RepositoryAdmin.InitRandomData();


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
    public override async Task<IActionResult> Patch(Guid id, [FromBody] Newtonsoft.Json.Linq.JObject patch)
    {
        var res = await base.Patch(id, patch);
        return res;
    }


}
