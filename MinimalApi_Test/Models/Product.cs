using System.ComponentModel.DataAnnotations;

namespace MinimalApi_Test.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Range(0.1, 10000)]
        public decimal Price { get; set; }
    }
}
