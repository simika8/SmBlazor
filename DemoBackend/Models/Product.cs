using MemoryPack;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoModels
{
    /// <summary>
    /// Product Type
    /// </summary>
    public enum ProductType
    {
        /// <summary>Product = 0</summary>
        Product = 0,
        /// <summary>Service = 1</summary>
        Service = 1,
        /// <summary>Information = 2</summary>
        Information = 2,
    }

    [MemoryPackable]
    public partial class Product : Reporitory.IHasId<Guid>
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? Active { get; set; }
        public double? Price { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? Rating { get; set; }
        public ProductType? Type { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "jsonb")]
        public ProductExt? Ext { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "jsonb")]
        public List<InventoryStock>? Stocks { get; set; }

        //helper data field only for EF core
        internal double? StockSumQuantity { get => Stocks?.Sum(x =>x.Quantity); set { } }

    }

    [MemoryPackable]
    public partial record ProductExt
    {
        public string? Description { get; set; }
        public double? MinimumStock { get; set; }
    }
    [MemoryPackable]
    public partial class InventoryStock
    {
        public Guid StoreId { get; set; }
        public double Quantity { get; set; }

    }


}
