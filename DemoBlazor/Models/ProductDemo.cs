using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModelsDemo
{
    public enum ProductTypeDemo
    {
        Product = 0,
        Service = 1,
        Information = 2,
    }
    public class ProductDemo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public bool Active { get; set; }
        public double? Price { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int Rating { get; set; }
        public ProductTypeDemo Type {  get; set; }
        public ProductExtDemo? Ext { get; set; }
        public List<InventoryStockDemo> Stocks { get; set; } = new List<InventoryStockDemo>();

    }


    //Products table Extension
    //special 1 to 1 (or 1 to 0) relationship. ProductExt can be empty to some Product
    //Primary key: ProductExt.ProductId
    //Special: Primary key value equals to Products.Id

    public class ProductExtDemo
    {
        [Key]
        public Guid ProductId { get; set; }
        public string? Description { get; set; }
        public double? MinimumStock { get; set; }
    }

    public class InventoryStockDemo
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid StoreId { get; set; }
        public double Quantity { get; set; }

    }


}
