using Egzotopia.Data;
using Egzotopia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Egzotopia.Pages.Admin.ManageProducts
{
    public class IndexModel : AdminBasePage
    {
        private readonly EgZotopiaDbContext _context;

        public IndexModel(EgZotopiaDbContext context)
        {
            _context = context;
        }

        public List<Product> Products { get; set; } = new List<Product>();

        [TempData]
        public string Message { get; set; }

        public void OnGet()
        {
            Products = _context.Products.OrderByDescending(p => p.Id).ToList();
        }

        // DEÐÝÞÝKLÝK BURADA: Metot ismi sadece "OnPost" oldu.
        public IActionResult OnPost(int id)
        {
            if (id <= 0) return RedirectToPage();

            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();

                // Mesajý dolduruyoruz
                Message = "Ürün baþarýyla silindi!";
            }
            else
            {
                Message = "Hata: Ürün bulunamadý.";
            }

            return RedirectToPage();
        }
    }
}