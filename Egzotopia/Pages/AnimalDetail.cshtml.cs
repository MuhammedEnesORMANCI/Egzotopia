using Egzotopia.Data; // DbContext için
using Egzotopia.Models;
using Egzotopia.Services.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Egzotopia.Pages
{
    public class AnimalDetailModel : PageModel
    {
        private readonly IAnimalService _animalService;
        private readonly EgZotopiaDbContext _context; // Veritabaný
        private readonly ILogger<AnimalDetailModel> _logger;

        public AnimalDetailModel(IAnimalService animalService, EgZotopiaDbContext context, ILogger<AnimalDetailModel> logger)
        {
            _animalService = animalService;
            _context = context;
            _logger = logger;
        }

        public Animal? Animal { get; set; }
        public List<Product>? RelatedProducts { get; set; }

        // ARTIK ID DEÐÝL, NAME (ÝSÝM) ALIYORUZ
        public async Task<IActionResult> OnGetAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return RedirectToPage("/Animals");
            }

            try
            {
                // 1. API'den Hayvan Bilgisini Çek
                var apiResults = await _animalService.GetAnimalsByNameAsync(name);
                var apiAnimal = apiResults.FirstOrDefault();

                if (apiAnimal != null)
                {
                    string pexelsImage = await _animalService.GetPexelsImageUrl(apiAnimal.name);

                    Animal = new Animal
                    {
                        Name = apiAnimal.name,
                        ScientificName = apiAnimal.taxonomy?.scientific_name,
                        Habitat = apiAnimal.characteristics?.habitat,
                        Description = $"Beslenme: {apiAnimal.characteristics?.diet}...",

                        ImageUrl = pexelsImage // ARTIK PEXELS KULLANIYORUZ
                    };

                    // 2. Veritabanýndan Bu Hayvana Ait Ürünleri Çek (AnimalType ile)
                    // Örn: AnimalType == "lion" olan ürünleri getir.
                    RelatedProducts = await _context.Products
                                          .Where(p => p.AnimalType.ToLower() == name.ToLower())
                                          .ToListAsync();
                }
                else
                {
                    // Hayvan bulunamazsa listeye dön
                    return RedirectToPage("/Animals");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Hayvan detayý yüklenirken hata: {ex.Message}");
            }

            return Page();
        }
        // AnimalDetail.cshtml.cs dosyasýnýn içine eklenecek:

        public JsonResult OnPostAddToCart(int productId, string name, decimal price)
        {
            try
            {
                string? cartJson = HttpContext.Session.GetString("Cart");
                List<CartItem> cart = cartJson == null
                    ? new List<CartItem>()
                    : System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

                var existingItem = cart.FirstOrDefault(i => i.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    cart.Add(new CartItem { ProductId = productId, Name = name, Price = price, Quantity = 1 });
                }

                HttpContext.Session.SetString("Cart", System.Text.Json.JsonSerializer.Serialize(cart));
                return new JsonResult(new { success = true, message = $"{name} sepete eklendi!" });
            }
            catch (Exception)
            {
                return new JsonResult(new { success = false, message = "Hata oluþtu." });
            }
        }
    }
}