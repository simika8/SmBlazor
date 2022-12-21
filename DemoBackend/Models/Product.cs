using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

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
    public partial class Product
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? Active { get; set; }
        public double? Price { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? Rating { get; set; }
        public ProductType? Type {  get; set; }
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "jsonb")]
        public ProductExt? Ext { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "jsonb")]
        public List<InventoryStock>? Stocks { get; set; }

    }


    //Products table Extension
    //special 1 to 1 (or 1 to 0) relationship. ProductExt can be empty to some Product
    //Primary key: ProductExt.ProductId
    //Special: Primary key value equals to Products.Id
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
