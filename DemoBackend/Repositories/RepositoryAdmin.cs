using Common;
using Controllers;
using Database;
using DemoModels;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using SharpCompress.Common;
using SmQueryOptionsNs;
using System.Data;
using static Controllers.AdminController;

namespace Reporitory;

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


public static class RepositoryAdmin
{
    public static int MinQueryMilliseconds { get; set; } = 100;
    public static int MaxQueryMilliseconds { get; set; } = 300;

    public static DatabaseType DbType { get; set; } = DatabaseType.Dictionary;
    public static HashSet<DatabaseType> Initialized = new();
    public static void InitRandomData()
    {
        if (Initialized.Contains(DbType))
            return;
        Initialized.Add(DbType);

        switch (RepositoryAdmin.DbType)
        {
            case DatabaseType.Dictionary:
                InitRandomDataDict();
                break;
            case DatabaseType.EfPg:
                InitRandomDataEf();
                break;
            case DatabaseType.Mongo:
                InitRandomDataMongo();
                break;
        }

    }

    public static void InitRandomDataDict()
    {
        var newRecords = GetProductDict(100000);

        var table = Database.DictionaryDatabase.GetTable<Product, Guid>();
        foreach (var item in newRecords)
            table.Add(item.Key, item.Value);
    }
    public static Dictionary<Guid, Product> GetProductDict(int prodCount)
    {
        var products = new Dictionary<Guid, Product>();
        for (int i = 1; i <= prodCount; i++)
        {
            var p = RandomProduct.GenerateProduct(i, prodCount);
            if (i == 1)
                p.Name = "Product, with spec chars:(', &?) in it's name, asdf. fdsafasdf sadfasd .";
            if (i == 2)
                p.Code = "Prod C0000002";
            products.Add(p.Id, p);

        }
        return products;

    }

    public static void InitRandomDataEf()
    {
        using var db = new SmDemoProductContext();
        var prodCount = 100000;
        try
        {
            db.Database.Migrate();
            var oldcount = db.Product.Count();
            if (oldcount == prodCount)
                return;

        }
        catch (Exception ex)
        {
            RemoveTablesEfPg();
            db.Database.Migrate();
        }

        RemoveTablesEfPg();
        db.Database.Migrate();


        var inmem = RepositoryAdmin.GetProductDict(prodCount).Select(x => x.Value).ToList();
        var inmemchunked = inmem.ChunkBy(1000);
        foreach (var chunk in inmemchunked)
        {
            using var db2 = new SmDemoProductContext();
            db2.Product.AddRange(chunk);
            db2.SaveChanges();
        }
    }


    private static void RemoveTablesEfPg()
    {
        using var db = new SmDemoProductContext();

        var tableNames = db.Database.SqlQueryRaw<string>("SELECT table_name FROM information_schema.tables where information_schema.tables.table_schema = 'public'").ToList();
        foreach (var tableName in tableNames)
        {
            db.Database.ExecuteSqlRaw($"drop table if exists \"{tableName}\" cascade;");
        }
    ;
    }

    public static void InitRandomDataMongo()
    {
        var db = SmDemoProductMongoDatabase.GetDb();
        var product = SmDemoProductMongoDatabase.GetCollection<Product>(db);

        var prodCount = 100000;

        var oldcount = product?.CountDocuments(new MongoDB.Bson.BsonDocument()) ?? 0;
        if (oldcount == prodCount)
            return;

        db.DropCollection("Product");

        product = SmDemoProductMongoDatabase.GetCollection<Product>(db);



        var inmem = RepositoryAdmin.GetProductDict(prodCount).Select(x => x.Value).ToList();
        var inmemchunked = inmem.ChunkBy(1000);
        foreach (var chunk in inmemchunked)
        {
            product.InsertMany(chunk);
        }
        #region index
        var indexKeysDefinition = Builders<Product>.IndexKeys.Ascending(x => x.Name);
        var cio = new CreateIndexOptions<Product>()
        {
            Collation = new Collation("hu", strength: CollationStrength.Primary)
        };
        var cim = new CreateIndexModel<Product>(indexKeysDefinition, cio);
        product.Indexes.CreateOne(cim);

        #endregion
    }
}
