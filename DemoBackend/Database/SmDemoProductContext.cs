using Controllers;
using Database;
using DemoModels;
using MemoryPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Reporitory;
using SharpCompress.Common;

namespace Database;

public class SmDemoProductContext : DbContext
{
    private string ConnectionString { get; set; }
    public DbSet<DemoModels.Product> Product { get; set; } = null!;
    

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

        //modelBuilder.Entity<Product>().Property(p => p.StockSumQuantity).UsePropertyAccessMode(PropertyAccessMode.Property).HasColumnName("stocksumquantity");
        modelBuilder.Entity<Product>()
            .Property<double?>("StockSumQuantity")
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("stocksumquantity");

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