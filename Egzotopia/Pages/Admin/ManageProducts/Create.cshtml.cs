using Egzotopia.Data;
using Egzotopia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Egzotopia.Pages.Admin.ManageProducts
{
    // AdminBasePage ile güvenliði saðlýyoruz
    public class CreateModel : AdminBasePage
    {
        private readonly EgZotopiaDbContext _context;

        public CreateModel(EgZotopiaDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Product { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            // Validasyon kontrolü
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Veritabanýna Ekle
            _context.Products.Add(Product);
            _context.SaveChanges();

            // Listeye geri dön (Bir üst klasöre çýkmak için ../ kullanabiliriz veya direkt yolu yazarýz)
            return RedirectToPage("/Admin/ManageProducts");
        }
    }
}