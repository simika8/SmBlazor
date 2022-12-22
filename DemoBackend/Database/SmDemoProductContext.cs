using Database;
using DemoModels;
using MemoryPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Database;

public class SmDemoProductContext : DbContext
{
    private string ConnectionString { get; set; }
    public static bool Initialized { get; set; }

    public DbSet<DemoModels.Product> Product { get; set; } = null!;

    private static void RemoveTables()
    {
        using var db = new SmDemoProductContext();

        var tableNames = db.Database.SqlQueryRaw<string>("SELECT table_name FROM information_schema.tables where information_schema.tables.table_schema = 'public'").ToList();
        foreach (var tableName in tableNames)
        {
            db.Database.ExecuteSqlRaw($"drop table if exists \"{tableName}\" cascade;");
        }
        ;
    }
    public static void InitRandomData()
    {
        if (Initialized)
            return;
        Initialized = true;
        using var db = new SmDemoProductContext();
        var prodCount = 100000;
        try
        {
            db.Database.Migrate();
            var oldcount = db.Product.Count();
            if (oldcount == prodCount)
                return;

        } catch (Exception ex)
        {
            RemoveTables();
            db.Database.Migrate();
        }

        RemoveTables();
        db.Database.Migrate();

        /*while (db.Product.Count() > 0)
        {
            using var db3 = new SmDemoProductContext();
            var toremove = db3.Product.Take(1000);
            db3.Product.RemoveRange(toremove);
            db3.SaveChanges();
        }*/



        var inmem = DictionaryDatabase.GetProductDict(prodCount).Select(x => x.Value).ToList();
        var inmemchunked = inmem.ChunkBy(1000);
        foreach (var chunk in inmemchunked)
        {
            using var db2 = new SmDemoProductContext();
            db2.Product.AddRange(chunk);
            db2.SaveChanges();
        }
    }


    public SmDemoProductContext()
    {

        ConnectionString = Environment.GetEnvironmentVariable("SmBlazorPgConnectionString") ?? "";
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new Exception("Connection string is empty");
        //var contextName = this.GetType().Name;
        //var databaseName = contextName.Remove(contextName.Length - "Context".Length);
        //ConnectionString += ";Database=" + databaseName;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options
            .UseNpgsql(ConnectionString);
        options
           .UseLowerCaseNamingConvention();


    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().Property(p => p.Stocks).HasDeepValueComparer();
        modelBuilder.Entity<Product>().Property(p => p.Ext).HasDeepValueComparer();
    }
}

public static class ValueConversionExtensions
{
    public static PropertyBuilder<T> HasDeepValueComparer<T>(this PropertyBuilder<T> propertyBuilder)
    {
        //example from: https://stackoverflow.com/a/53051419
        //instead of manually set IsModified:
        //db.Entry(entity).Property(b => b.xProperty).IsModified = true;

        ValueComparer<T> comparer = new ValueComparer<T>(
            (l, r) => MemoryPackSerializer.Serialize(l, null).SequenceEqual(MemoryPackSerializer.Serialize(r, null)),
            v => v == null ? 0 : MemoryPackSerializer.Serialize(v, null).GetHashCode(),
            v => MemoryPackSerializer.Deserialize<T>(MemoryPackSerializer.Serialize(v, null), null)!
            );
        propertyBuilder.Metadata.SetValueComparer(comparer);



        return propertyBuilder;

    }
}