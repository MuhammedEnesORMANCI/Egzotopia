using Egzotopia.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Egzotopia.Pages.Profile
{
    [IgnoreAntiforgeryToken]
    public class EditModel : PageModel
    {
        private readonly EgZotopiaDbContext _context;

        public EditModel(EgZotopiaDbContext context)
        {
            _context = context;
        }

        public class SimpleModel
        {
            public string FullName { get; set; }
            public string Email { get; set; }
        }

        // Sayfada verileri göstermek için kullanýyoruz
        public SimpleModel EditData { get; set; }

        public IActionResult OnGet()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");

            var user = _context.Users.Find(int.Parse(userIdString));
            if (user == null) return RedirectToPage("/Account/Login");

            EditData = new SimpleModel
            {
                FullName = user.FullName,
                Email = user.Email
            };

            return Page();
        }

        public IActionResult OnPost()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");

            // --- DEÐÝÞÝKLÝK BURADA: MANUEL VERÝ OKUMA ---
            // Formdan "GelenIsim" adýyla gönderilen veriyi direkt alýyoruz.
            string formdanGelenIsim = Request.Form["GelenIsim"];

            // Kontrol edelim: Gelen veri boþ mu?
            if (string.IsNullOrEmpty(formdanGelenIsim))
            {
                TempData["Hata"] = "HATA: Ýsim kutusu boþ geldi veya okunamadý.";
                return RedirectToPage(); // Hata varsa sayfayý yenile
            }

            // Veritabanýndaki kullanýcýyý bul
            var userInDb = _context.Users.Find(int.Parse(userIdString));

            if (userInDb != null)
            {
                // Yakaladýðýmýz veriyi veritabanýna yazýyoruz
                userInDb.FullName = formdanGelenIsim;

                _context.SaveChanges();

                // Session güncelle
                HttpContext.Session.SetString("UserFullName", userInDb.FullName);

                return RedirectToPage("/Profile/Index");
            }

            return RedirectToPage("/Profile/Index");
        }
    }
}