using Egzotopia.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq; // Distinct ve Select için gerekli

namespace Egzotopia.Pages.Admin
{
    public class AdminDashboardModel : AdminBasePage
    {
        private readonly EgZotopiaDbContext _context;

        public AdminDashboardModel(EgZotopiaDbContext context)
        {
            _context = context;
        }

        public int ActiveCategories { get; set; } // Hayvan Türü Sayýsý
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        public void OnGet()
        {
            // 1. Hayvan tablosu yok ama Ürünlerdeki "AnimalType"larý sayabiliriz.
            // Örn: Hem "lion" hem "parrot" varsa sonuç 2 çýkar.
            ActiveCategories = _context.Products
                                       .Select(p => p.AnimalType)
                                       .Distinct()
                                       .Count();

            // 2. Toplam Ürün Sayýsý
            TotalProducts = _context.Products.Count();

            // 3. Toplam Sipariþ Sayýsý
            TotalOrders = _context.Orders.Count();

            // 4. Toplam Gelir (Tablo boþsa hata vermesin diye Any kontrolü)
            TotalRevenue = _context.Orders.Any() ? _context.Orders.Sum(o => o.TotalAmount) : 0;
        }
    }
}