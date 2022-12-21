using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DemoModels;
using MemoryPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    public static int MinQueryMilliseconds { get; set; } = 100;
    public static int MaxQueryMilliseconds { get; set; } = 3000;
    public static DatabaseType DbType { get; set; } = DatabaseType.Dictionary;
    /// <summary>
    /// DB type
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>dict</summary>
        Dictionary,
        /// <summary>EfPg</summary>
        EfPg,
        /// <summary>Mongo</summary>
        Mongo,
    }

    [HttpPut(nameof(SetSearchRunTime))]
    public async Task<ActionResult> SetSearchRunTime(int minQueryMilliseconds, int maxQueryMilliseconds)
    {
        MinQueryMilliseconds = minQueryMilliseconds;
        MaxQueryMilliseconds = maxQueryMilliseconds;

        return Ok(( MinQueryMilliseconds, MaxQueryMilliseconds));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbType">0: In memory Dictionary, 1: EF+PgSql, 2: MongoDb</param>
    /// <returns></returns>
    [HttpPut(nameof(SetDataBase))]
    public async Task<ActionResult> SetDataBase(DatabaseType dbType)
    {
        DbType = dbType;
        return Ok(dbType);
    }


    [HttpPost(nameof(Proba))]
    public async Task<ActionResult<Product>> Proba()
    {
        //var list = ImmutableArray.Create(new InventoryStock() { Id = new Guid(), Quantity = 987 });
        


        Database.SmDemoProductContext.InitRandomData();
        using var db = new Database.SmDemoProductContext();
        var prod = db.Product.First(x => x.Code == "C0000001");

        //prod.Stocks = new();

        prod.Stocks?.Add(new() {Quantity = prod.Stocks.Count()+1 });
        //db.Entry(prod).Property(b => b.Stocks).IsModified = true;

        prod.Ext.Description = "desc"+ prod.Stocks.Count();
        //db.Entry(prod).Property(b => b.Ext).IsModified = true;
        //db.Entry(prod).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        db.SaveChanges();


        //var v = new Person { Age = 40, Name = "John" };
        
        
        var json2 = JsonConvert.SerializeObject(prod);
        var val2 = JsonConvert.DeserializeObject<Product>(json2);
        var bin2 = MemoryPackSerializer.Serialize(prod);
        var val2b = MemoryPackSerializer.Deserialize<Product>(bin2);

        /*;
        for (int i = 0; i < 100000; i++)
        {
            var json3 = JsonConvert.SerializeObject(prod);
            var val3 = JsonConvert.DeserializeObject<Product>(json3);
        }

        ;
        for (int i = 0; i < 100000; i++)
        {
            var bin3 = MemoryPackSerializer.Serialize(prod);
            var val3 = MemoryPackSerializer.Deserialize<Product>(bin3);
        }

        ;*/

        var eq = JsonConvert.SerializeObject(val2) == JsonConvert.SerializeObject(val2b);
        ;
        /*var val1 = new ProbaType() { Id = 1, Name = "asdf" };

        var bin = MemoryPackSerializer.Serialize(val1);
        var val = MemoryPackSerializer.Deserialize<Product>(bin);*/
        ;

        return Ok();
    }
    [HttpPost(nameof(Proba2))]
    public async Task<ActionResult<Person>> Proba2()
    {
        var v = new Person { Age = 40, Name = "John" };

        var bin = MemoryPackSerializer.Serialize(v);
        var val = MemoryPackSerializer.Deserialize<Person>(bin);

        return Ok(val);
    }



}

[MemoryPack.MemoryPackable]
public partial class ProbaType
{
    public int Id { get; set; }
    public string Name { get; set; }

}

[MemoryPackable]
public partial class Person
{
    public int Age { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
}

public class Dog
{
    public int Legs;
    public double GoodBoiBarksThisLoud;
}