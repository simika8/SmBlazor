using System.ComponentModel.DataAnnotations;

namespace DemoModels
{
    public enum ProductTypeDto
    {
        Product = 0,
        Service = 1,
        Information = 2,
    }
    public class ProductDto
    {
        public Guid? Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? Active { get; set; }
        public double? Price { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? Rating { get; set; }
        public ProductTypeDto? Type {  get; set; }
        public string? Description { get; set; }
        public double? StockSumQuantity { get; set; }

    }

}
