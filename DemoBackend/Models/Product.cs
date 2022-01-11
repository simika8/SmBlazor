using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models
{
    public enum ProductType
    {
        Product = 0,
        Service = 1,
        Information = 2,
    }
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public bool Active { get; set; }
        public double? Price { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int Rating { get; set; }
        public ProductType Type {  get; set; }
        public ProductExt? Ext { get; set; }
        public List<InventoryStock>? Stocks { get; set; }

    }


    //Products table Extension
    //special 1 to 1 (or 1 to 0) relationship. ProductExt can be empty to some Product
    //Primary key: ProductExt.ProductId
    //Special: Primary key value equals to Products.Id

    public class ProductExt
    {
        [Key]
        public Guid ProductId { get; set; }
        public string? Description { get; set; }
        public double? MinimumStock { get; set; }
    }

    public class InventoryStock
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid StoreId { get; set; }
        public double Quantity { get; set; }

    }


}
