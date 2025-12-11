using Egzotopia.Data;
using Egzotopia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Egzotopia.Pages.Profile
{
    public class IndexModel : PageModel
    {
        private readonly EgZotopiaDbContext _context;

        public IndexModel(EgZotopiaDbContext context)
        {
            _context = context;
        }

        public User CurrentUser { get; set; }
        public List<Order> UserOrders { get; set; }

        public IActionResult OnGet()
        {
            // 1. Oturum Kontrolü
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");

            int userId = int.Parse(userIdString);

            // 2. Kullanýcý Bilgilerini Çek
            CurrentUser = _context.Users.Find(userId);

            // 3. Kullanýcýnýn Sipariþlerini Çek (En yeniden eskiye)
            UserOrders = _context.Orders
                .Include(o => o.OrderItems) // Sipariþ detaylarýný da getir
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return Page();
        }
    }
}