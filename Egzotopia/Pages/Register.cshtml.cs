using Egzotopia.Data;
using Egzotopia.Models;
using Egzotopia.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Egzotopia.Pages
{
    // Güvenlik kontrolünü geçici olarak kapatýyoruz (400 hatasýný engellemek için)
    [IgnoreAntiforgeryToken]
    public class RegisterModel : PageModel
    {
        private readonly EgZotopiaDbContext _context;
        private readonly EmailService _emailService;

        public RegisterModel(EgZotopiaDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // --- HTML'den Gelen Verileri Yakalayan Deðiþkenler ---

        [BindProperty]
        public string FullName { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        [BindProperty]
        public bool AgreeTerms { get; set; }

        // Ekrana hata yazdýrmak için
        public string DebugError { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            // 1. Manuel Kontroller (En garantisi budur)
            if (string.IsNullOrEmpty(FullName))
            {
                DebugError = "Lütfen Ad Soyad giriniz."; return Page();
            }
            if (string.IsNullOrEmpty(Email))
            {
                DebugError = "Lütfen E-posta giriniz."; return Page();
            }
            if (string.IsNullOrEmpty(Password))
            {
                DebugError = "Lütfen þifre giriniz."; return Page();
            }

            // Sözleþme Kontrolü
            if (AgreeTerms == false)
            {
                DebugError = "Lütfen sözleþmeyi kabul edin."; return Page();
            }

            // Þifre Eþleþme
            if (Password != ConfirmPassword)
            {
                DebugError = "Þifreler uyuþmuyor!"; return Page();
            }

            // Mail Var mý?
            if (_context.Users.Any(u => u.Email == Email))
            {
                DebugError = "Bu mail zaten sistemde kayýtlý."; return Page();
            }

            // --- HER ÞEY YOLUNDA, ÝÞLEMÝ YAP ---
            Random rnd = new Random();
            string code = rnd.Next(100000, 999999).ToString();

            bool mailSent = _emailService.SendEmail(Email, code);

            if (!mailSent)
            {
                DebugError = "Mail gönderilemedi. Lütfen mail adresini kontrol et.";
                return Page();
            }

            // Session'a at
            HttpContext.Session.SetString("TempFullName", FullName);
            HttpContext.Session.SetString("TempEmail", Email);
            HttpContext.Session.SetString("TempPassword", Password);
            HttpContext.Session.SetString("VerificationCode", code);

            return RedirectToPage("/ConfirmRegister");
        }
    }
}