using Egzotopia.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Egzotopia.Pages.Profile
{
    // 400 Hatasýný (Güvenlik) engellemek için
    [IgnoreAntiforgeryToken]
    public class ChangePasswordModel : PageModel
    {
        private readonly EgZotopiaDbContext _context;

        public ChangePasswordModel(EgZotopiaDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            // Oturum kontrolü
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToPage("/Account/Login");
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            // 1. Oturum Kontrolü
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");

            // 2. HTML Formundan Verileri Manuel Alýyoruz (En Garanti Yol)
            string eskiSifre = Request.Form["EskiSifre"];
            string yeniSifre = Request.Form["YeniSifre"];
            string yeniSifreTekrar = Request.Form["YeniSifreTekrar"];

            // 3. Boþ Alan Kontrolü
            if (string.IsNullOrEmpty(eskiSifre) || string.IsNullOrEmpty(yeniSifre) || string.IsNullOrEmpty(yeniSifreTekrar))
            {
                TempData["Hata"] = "Lütfen tüm alanlarý doldurunuz.";
                return Page();
            }

            // 4. Yeni Þifreler Uyuþuyor mu?
            if (yeniSifre != yeniSifreTekrar)
            {
                TempData["Hata"] = "Yeni þifreler birbiriyle uyuþmuyor!";
                return Page();
            }

            // 5. Veritabanýndan Kullanýcýyý Bul
            var user = _context.Users.Find(int.Parse(userIdString));
            if (user == null) return RedirectToPage("/Account/Login");

            // 6. Eski Þifre Doðru mu?
            if (user.Password != eskiSifre)
            {
                TempData["Hata"] = "Mevcut þifrenizi yanlýþ girdiniz.";
                return Page();
            }

            // 7. ÞÝFREYÝ GÜNCELLE VE KAYDET
            user.Password = yeniSifre;
            _context.SaveChanges();

            // Baþarýlý mesajý verip profil sayfasýna gönder
            TempData["Basarili"] = "Þifreniz baþarýyla deðiþtirildi!";
            return RedirectToPage("/Profile/Index");
        }
    }
}