using Egzotopia.Data;
using Egzotopia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Egzotopia.Pages
{
    // 400 Hatasýný engellemek için güvenlik kontrolünü devre dýþý býrakýyoruz
    [IgnoreAntiforgeryToken]
    public class LoginModel : PageModel
    {
        private readonly EgZotopiaDbContext _context;

        public LoginModel(EgZotopiaDbContext context)
        {
            _context = context;
        }

        // --- Verileri Burada Karþýlýyoruz ---
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string? ErrorMessage { get; set; } // Ekranda görünecek mesaj

        public void OnGet()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                Response.Redirect("/Index");
            }
        }

        public IActionResult OnPost()
        {
            // 1. Veriler Geldi mi Kontrolü
            // Eðer Email veya Þifre boþsa, hatanýn ne olduðunu deðiþkene yazalým.
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                // Hangi kutu boþsa onu bulup yazalým
                List<string> bosAlanlar = new List<string>();
                if (string.IsNullOrEmpty(Email)) bosAlanlar.Add("E-posta");
                if (string.IsNullOrEmpty(Password)) bosAlanlar.Add("Þifre");

                ErrorMessage = "Veri gelmedi! Boþ alanlar: " + string.Join(", ", bosAlanlar);
                return Page();
            }

            // 2. Veritabanýndan Kullanýcýyý Bul
            var user = _context.Users.FirstOrDefault(u => u.Email == Email && u.Password == Password);

            if (user == null)
            {
                ErrorMessage = "E-posta veya þifre hatalý!";
                return Page();
            }

            // --- GÝRÝÞ BAÞARILI ---
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserFullName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role.ToString());

            // Yönlendirme (Admin ise Panele, Deðilse Anasayfaya)
            if (user.Role == 1 || user.Role == 2)
            {
                return RedirectToPage("/Admin/Dashboard");
            }

            return RedirectToPage("/Index");
        }
    }
}