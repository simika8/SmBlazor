using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmQueryOptionsNs;
using Swashbuckle.AspNetCore.SwaggerGen;
using Reporitory;

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public abstract class CrudBaseController<T> : ControllerBase where T : class, IHasId<Guid>
{
    protected RepositoryCrud<T, Guid> RepositoryCrud { get; set; } = new();

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] T entity)
    {
        var id = Others.GetGuidKey(entity);

        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse(1, "Invalid modelstate", ModelState));
        }

        var dbentity = await RepositoryCrud.Read(id);
        if (dbentity != null)
            return Conflict(new ErrorResponse(2, "Entity already exists", dbentity));

        await RepositoryCrud.Create(id, entity);
        //Table[id] = entity;

        return CreatedAtAction(nameof(Post), id, entity);

    }

    [HttpGet("{id:Guid}")]
    public async Task<ActionResult<T>> Get(Guid id)
    {
        await Task.Delay(0);
        var dbentity = await RepositoryCrud.Read(id);
        if (dbentity == null)
            return NotFound(new ErrorResponse(3, "Not found", id));
        
        return dbentity;
            
    }


    [HttpPut("{id:Guid}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] T modifiedEntity)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse(4, "Invalid modelstate", ModelState));
        }

        var dbentity = await RepositoryCrud.Read(id);
        if (dbentity == null)
            return NotFound(new ErrorResponse(5, "Not found", id));

        await RepositoryCrud.Update(id, modifiedEntity);
        return Ok(modifiedEntity);

    }

    /// <summary>
    /// json merge patch
    /// </summary>
    [HttpPatch("{id:Guid}")]
    public virtual async Task<IActionResult> Patch(Guid id, [FromBody] Newtonsoft.Json.Linq.JObject patch)
    {
        var dbentity = await RepositoryCrud.Read(id);
        if (dbentity == null)
            return NotFound(new ErrorResponse(8, "Not found", id));

        var sourceObject = Newtonsoft.Json.Linq.JObject.FromObject(dbentity);
        sourceObject.Merge(patch, new Newtonsoft.Json.Linq.JsonMergeSettings() { MergeArrayHandling = Newtonsoft.Json.Linq.MergeArrayHandling.Replace});
        dbentity = sourceObject.ToObject<T>();
        if (dbentity == null)
            return BadRequest(new ErrorResponse(6, "entity == null", dbentity));

        var isValid = TryValidateModel(dbentity);
        if (!isValid)
            return BadRequest(new ErrorResponse(7, "Invalid modelstate", ModelState));

        await RepositoryCrud.Update(id, dbentity);
        return Ok(dbentity);
            

    }


    [HttpDelete("{id:Guid}")]
    public async Task<ActionResult<T>> Delete(Guid id)
    {
        if (await RepositoryCrud.Delete(id))
        {
            return NoContent();
        }
        else
            return NotFound(new ErrorResponse(9, "Not found", id));
    }

}
