using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters; 
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Egzotopia.Pages
{
 
    public class AdminBasePage : PageModel
    {
      
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
           
            string role = context.HttpContext.Session.GetString("UserRole");

            
            if (string.IsNullOrEmpty(role) || role == "0")
            {
                // İzinsiz giriş! Login'e şutla.
                context.Result = new RedirectToPageResult("/Login");
            }

            // Eğer rol 1 veya 2 ise hiçbir şey yapma, sayfa normal çalışsın.
            base.OnPageHandlerExecuting(context);
        }
    }
}