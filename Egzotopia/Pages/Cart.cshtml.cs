using Egzotopia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Egzotopia.Pages
{
    public class CartModel : PageModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; } = 50.00m;
        public decimal Total { get; set; }

        public void OnGet()
        {
            string? cartJson = HttpContext.Session.GetString("Cart");

            if (!string.IsNullOrEmpty(cartJson))
            {
                try
                {
                    CartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
                }
                catch
                {
                    CartItems = new List<CartItem>(); // JSON bozuksa sepeti sýfýrla
                }
            }

            Subtotal = CartItems.Sum(item => item.Total);
            // Eðer sepet boþsa kargo ücreti ekleme
            Total = CartItems.Any() ? Subtotal + ShippingCost : 0;
        }

        // Sepetten Ürün Silme
        public IActionResult OnPostRemove(int productId)
        {
            string? cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson)) return RedirectToPage();

            var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

            cart.RemoveAll(i => i.ProductId == productId);

            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
            return RedirectToPage();
        }

        // Sepetteki Adeti Güncelleme
        public IActionResult OnPostUpdateQuantity(int productId, int quantity)
        {
            // Eðer miktar 1'den küçükse silme fonksiyonunu çaðýr
            if (quantity < 1) return OnPostRemove(productId);

            string? cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson)) return RedirectToPage();

            var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                item.Quantity = quantity;
                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
            }
            return RedirectToPage();
        }
    }
}