using Egzotopia.Data;
using Egzotopia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Egzotopia.Pages
{
    // 1. Güvenlik hatasýný (400) engellemek için bunu ekliyoruz
    [IgnoreAntiforgeryToken]
    public class ConfirmRegisterModel : PageModel
    {
        private readonly EgZotopiaDbContext _context;

        public ConfirmRegisterModel(EgZotopiaDbContext context)
        {
            _context = context;
        }

        public string Message { get; set; }
        public string SentEmail { get; set; }

        // 2. Formdan gelen kodu burada yakalýyoruz
        [BindProperty]
        public string Code { get; set; }

        public IActionResult OnGet()
        {
            SentEmail = HttpContext.Session.GetString("TempEmail");
            if (string.IsNullOrEmpty(SentEmail))
            {
                return RedirectToPage("/Register");
            }
            return Page();
        }

        // 3. Parantez içi BOÞ. Veriyi yukarýdaki 'Code' deðiþkeninden alacak.
        public IActionResult OnPost()
        {
            // Session'daki doðru kodu al
            string correctCode = HttpContext.Session.GetString("VerificationCode");

            // Kullanýcýnýn girdiði kod (Code) ile karþýlaþtýr
            // (Boþ mu diye de kontrol edelim)
            if (string.IsNullOrEmpty(Code) || Code != correctCode)
            {
                Message = "Hatalý kod girdiniz! Lütfen tekrar deneyin.";
                SentEmail = HttpContext.Session.GetString("TempEmail");
                return Page();
            }

            // --- KOD DOÐRU ---
            var newUser = new User
            {
                FullName = HttpContext.Session.GetString("TempFullName"),
                Email = HttpContext.Session.GetString("TempEmail"),
                Password = HttpContext.Session.GetString("TempPassword"),
                Role = 0 // Varsayýlan üye
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            // Session Temizliði
            HttpContext.Session.Remove("TempFullName");
            HttpContext.Session.Remove("TempPassword");
            HttpContext.Session.Remove("VerificationCode");

            // Giriþ Yap
            HttpContext.Session.SetString("UserId", newUser.Id.ToString());
            HttpContext.Session.SetString("UserEmail", newUser.Email);
            HttpContext.Session.SetString("UserFullName", newUser.FullName);
            HttpContext.Session.SetString("UserRole", newUser.Role.ToString());

            return RedirectToPage("/Index");
        }
    }
}