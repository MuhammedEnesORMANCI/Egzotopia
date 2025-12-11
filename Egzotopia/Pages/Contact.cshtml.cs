using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Egzotopia.Pages
{
    public class ContactModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnPost(string name, string email, string subject, string message)
        {
            return RedirectToPage("/Index");
        }
    }
}