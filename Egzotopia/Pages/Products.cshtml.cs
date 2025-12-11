using Egzotopia.Models;
using Egzotopia.Services.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Egzotopia.Pages
{
    public class ProductsModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsModel> _logger;

        public ProductsModel(IProductService productService, ILogger<ProductsModel> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public List<Product>? Products { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Tüm ürünleri çek
                Products = await _productService.GetAllProductsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürünler listelenirken hata oluþtu.");
                Products = new List<Product>();
            }
        }

        // --- AJAX SEPETE EKLEME METODU ---
        public JsonResult OnPostAddToCart(int productId, string name, decimal price)
        {
            try
            {
                // 1. Sepeti Session'dan Çek
                string? cartJson = HttpContext.Session.GetString("Cart");
                List<CartItem> cart = cartJson == null
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

                // 2. Ürün sepette var mý kontrol et
                var existingItem = cart.FirstOrDefault(i => i.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = productId,
                        Name = name,
                        Price = price,
                        Quantity = 1
                    });
                }

                // 3. Sepeti Kaydet
                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

                // 4. JSON Cevabý Dön (Sayfa yenilenmez)
                return new JsonResult(new { success = true, message = $"{name} sepete eklendi!" });
            }
            catch (Exception)
            {
                return new JsonResult(new { success = false, message = "Ürün sepete eklenirken hata oluþtu." });
            }
        }
    }
}