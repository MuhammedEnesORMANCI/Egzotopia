using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Egzotopia.Models
{
    public class Order
    {
        public int Id { get; set; }

        // --- GÜNCELLEME 1: Misafir alışverişi için UserId 'int?' (nullable) yapıldı ---
        public int? UserId { get; set; }
        public User? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Hazırlanıyor";

        // --- GÜNCELLEME 2: Checkout Formundaki Veriler İçin Alanlar Eklendi ---
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }    

        [Required]
        public string PostalCode { get; set; }

        public string PaymentMethod { get; set; } // Kredi Kartı / Havale

        // Detaylar
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}