using System.ComponentModel.DataAnnotations.Schema;

namespace Egzotopia.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string ProductName { get; set; } // Ürün adı değişirse diye saklayalım

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Sizin kodunuzda UnitPrice idi, bunu kullanacağız
    }
}