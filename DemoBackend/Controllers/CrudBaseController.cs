﻿using System;
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
public abstract class CrudBaseController<T> : ControllerBase where T : class
{
    protected RepositoryCrud<T, Guid> RepositoryCrud { get; set; } = new();

    [HttpPost]
    public IActionResult Post([FromBody] T entity)
    {
        var id = Others.GetGuidKey(entity);

        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse(1, "Invalid modelstate", ModelState));
        }

        if (RepositoryCrud.Read(id, out var dbentity))
        {
            return Conflict(new ErrorResponse(2, "Entity already exists", dbentity));
        }
        RepositoryCrud.Create(id, entity);
        //Table[id] = entity;

        return CreatedAtAction(nameof(Post), id, entity);

    }

    [HttpGet("{id:Guid}")]
    public async Task<ActionResult<T>> Get(Guid id)
    {
        await Task.Delay(0);
        if (RepositoryCrud.Read(id, out var entity))
        {
            return entity;
        }
        else
            return NotFound(new ErrorResponse(3, "Not found", id));
    }


    [HttpPut("{id:Guid}")]
    public IActionResult Put(Guid id, [FromBody] T modifiedEntity)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse(4, "Invalid modelstate", ModelState));
        }

        if (RepositoryCrud.Read(id, out var entity))
        {
            RepositoryCrud.Update(id, modifiedEntity);
            return Ok(modifiedEntity);
        }
        else
            return BadRequest(new ErrorResponse(5, "Not found", id));

    }

    /// <summary>
    /// json merge patch
    /// </summary>
    [HttpPatch("{id:Guid}")]
    public virtual IActionResult Patch(Guid id, [FromBody] Newtonsoft.Json.Linq.JObject patch)
    {

        if (RepositoryCrud.Read(id, out var entity))
        {
            var sourceObject = Newtonsoft.Json.Linq.JObject.FromObject(entity);
            sourceObject.Merge(patch, new Newtonsoft.Json.Linq.JsonMergeSettings() { MergeArrayHandling = Newtonsoft.Json.Linq.MergeArrayHandling.Replace});
            entity = sourceObject.ToObject<T>();
            if (entity == null)
                return BadRequest(new ErrorResponse(6, "entity == null", entity));

            var isValid = TryValidateModel(entity);
            if (!isValid)
            {
                return BadRequest(new ErrorResponse(7, "Invalid modelstate", ModelState));
            }

            RepositoryCrud.Update(id, entity);
            return Ok(entity);
        }
        else
            return BadRequest(new ErrorResponse(8, "Not found", id));

    }


    [HttpDelete("{id:Guid}")]
    public async Task<ActionResult<T>> Delete(Guid id)
    {
        if (RepositoryCrud.Delete(id))
        {
            return NoContent();
        }
        else
            return BadRequest(new ErrorResponse(9, "Not found", id));
    }





    protected void AddExpressionIfNotNull(List<Expression> list, Expression? item)
    {
        if (item != null)
            list.Add(item);
    }
}
