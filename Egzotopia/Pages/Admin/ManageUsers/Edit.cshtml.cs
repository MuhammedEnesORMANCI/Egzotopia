using Egzotopia.Data;
using Egzotopia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Egzotopia.Pages.Admin.ManageUsers
{
    public class EditModel : AdminBasePage
    {
        private readonly EgZotopiaDbContext _context;

        public EditModel(EgZotopiaDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public User EditUser { get; set; }

        // Þifre güncellemek için opsiyonel alan
        [BindProperty]
        public string? NewPassword { get; set; }

        public void OnGet(int id)
        {
            // Düzenlenecek kullanýcýyý bul
            EditUser = _context.Users.Find(id);
        }

        public IActionResult OnPost()
        {
            if (EditUser == null || EditUser.Id == 0) return Redirect("/Admin/ManageUsers");

            var userInDb = _context.Users.Find(EditUser.Id);
            if (userInDb != null)
            {
                userInDb.FullName = EditUser.FullName;
                userInDb.Email = EditUser.Email;
                userInDb.Role = EditUser.Role;

                // Eðer yeni þifre kutusu doluysa þifreyi de deðiþtir
                if (!string.IsNullOrEmpty(NewPassword))
                {
                    userInDb.Password = NewPassword;
                }

                _context.SaveChanges();
            }

            return Redirect("/Admin/ManageUsers");
        }
    }
}