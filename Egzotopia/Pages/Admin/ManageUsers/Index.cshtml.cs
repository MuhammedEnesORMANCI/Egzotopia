using Egzotopia.Data;
using Egzotopia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Egzotopia.Pages.Admin.ManageUsers
{
    public class IndexModel : AdminBasePage
    {
        private readonly EgZotopiaDbContext _context;

        public IndexModel(EgZotopiaDbContext context)
        {
            _context = context;
        }

        public List<User> Users { get; set; } = new List<User>();

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "2") return Redirect("/Admin/Dashboard");

            Users = _context.Users.OrderByDescending(u => u.Id).ToList();
            return Page();
        }

        // DEÐÝÞÝKLÝK BURADA: Metot ismi sadece "OnPost" oldu.
        // Bu sayede formda handler belirtmeye gerek kalmaz.
        public IActionResult OnPost(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            // Sayfayý zorla yenile
            return Redirect("/Admin/ManageUsers");
        }
    }
}