using Database;
using DemoModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Database;

public class SmDemoProductContext : DbContext
{
    private string ConnectionString { get; set; }
    public static bool Initialized { get; set; }

    public DbSet<DemoModels.Product> Product { get; set; } = null!;

    public static void InitRandomData()
    {
        if (Initialized)
            return;
        Initialized = true;
        using var db = new SmDemoProductContext();
        var prodCount = 100000;
        db.Database.Migrate();
        var oldcount = db.Product.Count();
        if (oldcount == prodCount)
            return;

        while (db.Product.Count() > 0)
        {
            using var db3 = new SmDemoProductContext();
            var toremove = db3.Product.Take(1000);
            db3.Product.RemoveRange(toremove);
            db3.SaveChanges();
        }
        


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
}