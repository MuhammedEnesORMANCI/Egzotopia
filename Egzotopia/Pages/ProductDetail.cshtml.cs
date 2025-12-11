using Egzotopia.Data;
using Egzotopia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Egzotopia.Pages
{
    public class ProductDetailModel : PageModel
    {
        private readonly EgZotopiaDbContext _context;

        public ProductDetailModel(EgZotopiaDbContext context)
        {
            _context = context;
        }

        public Product Product { get; set; }

        // Sayfa açýldýðýnda ürünü bul
        public IActionResult OnGet(int id)
        {
            Product = _context.Products.Find(id);

            if (Product == null)
            {
                return RedirectToPage("/Products"); // Ürün yoksa listeye dön
            }
            return Page();
        }

        // AJAX ÝLE SEPETE EKLEME (Sayfa Yenilenmez)
        public JsonResult OnPostAddToCart(int productId, string name, decimal price)
        {
            try
            {
                string? cartJson = HttpContext.Session.GetString("Cart");
                List<CartItem> cart = cartJson == null
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

                var existingItem = cart.FirstOrDefault(i => i.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    cart.Add(new CartItem { ProductId = productId, Name = name, Price = price, Quantity = 1 });
                }

                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
                return new JsonResult(new { success = true, message = $"{name} sepete eklendi!" });
            }
            catch (Exception)
            {
                return new JsonResult(new { success = false, message = "Hata oluþtu." });
            }
        }
    }
}