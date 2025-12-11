namespace Egzotopia.Models
{
    // Sepetteki tek bir ürünü temsil eder
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;

        // Sepette toplam fiyatı hesaplar
        public decimal Total => Price * Quantity;
    }
}