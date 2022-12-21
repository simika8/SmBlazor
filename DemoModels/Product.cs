﻿using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DemoModels2
{
    public enum ProductType
    {
        Product = 0,
        Service = 1,
        Information = 2,
    }
    public partial class Product2
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? Active { get; set; }
        public double? Price { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? Rating { get; set; }
        public ProductType? Type { get; set; }
    }


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

    public partial record ProductExt
    {
        [Key]
        [BsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid ProductId { get; set; }
        public string? Description { get; set; }
        public double? MinimumStock { get; set; }
    }

    public partial class InventoryStock
    {
        [BsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid Id { get; set; }
        [BsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid ProductId { get; set; }
        public Guid StoreId { get; set; }
        public double Quantity { get; set; }

    }


}
