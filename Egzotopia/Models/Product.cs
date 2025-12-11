using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Egzotopia.Models
{
    public class Product
    {
        public int Id { get; set; }

        // --- BAĞLANTI BURASI ---
        // Tablo olmadığı için ID kullanamayız. 
        // Ürünün hangi hayvana ait olduğunu ismen yazacağız.
        // Örn: "lion", "parrot", "snake"
        [Required]
        [StringLength(100)]
        public string AnimalType { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        [StringLength(1000)]
        public string ProductType { get; set; }

        [StringLength(1200)]
        public string ImageUrl { get; set; }
        [StringLength(6200)]
        public string? Description { get; set; }
    }
}