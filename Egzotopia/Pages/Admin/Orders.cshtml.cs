using Egzotopia.Data;
using Egzotopia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Egzotopia.Pages.Admin
{
    public class OrdersModel : PageModel // AdminBasePage kullanýyorsan onu yazabilirsin
    {
        private readonly EgZotopiaDbContext _context;

        public OrdersModel(EgZotopiaDbContext context)
        {
            _context = context;
        }

        public List<Order> Orders { get; set; } = new List<Order>();

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }

        public void OnGet()
        {
            // 1. Temel Sorgu
            var query = _context.Orders
                .Include(o => o.OrderItems) // Ürünleri getir
                .Include(o => o.User)       // Üye bilgisini getir
                .AsQueryable();

            // 2. Arama Filtresi
            if (!string.IsNullOrEmpty(SearchString))
            {
                string search = SearchString.ToLower().Trim();

                // ID aramasý için sayý kontrolü (SQL hatasý vermemesi için)
                bool isNumeric = int.TryParse(search, out int searchId);

                query = query.Where(o =>
                    (isNumeric && o.Id == searchId) ||            // ID eþleþmesi
                    o.FullName.ToLower().Contains(search) ||      // Ýsim aramasý
                    o.Email.ToLower().Contains(search)            // Email aramasý
                );
            }

            // 3. Durum Filtresi (Düzeltildi)
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                var filter = StatusFilter.Trim(); // Boþluklarý temizle
                query = query.Where(o => o.Status == filter);
            }

            // 4. Sýralama ve Listeleme (En yeni en üstte)
            Orders = query.OrderByDescending(o => o.OrderDate).ToList();
        }

        public IActionResult OnPostUpdateStatus(int orderId, string newStatus)
        {
            var order = _context.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = newStatus;
                _context.SaveChanges();
            }
            return RedirectToPage();
        }

        public IActionResult OnPostDeleteOrder(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == orderId);

            if (order != null)
            {
                _context.Orders.Remove(order);
                _context.SaveChanges();
            }
            return RedirectToPage();
        }
    }
}