using Egzotopia.Data;
using Egzotopia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Egzotopia.Pages.Admin.ManageProducts
{
    public class EditModel : AdminBasePage
    {
        private readonly EgZotopiaDbContext _context;

        public EditModel(EgZotopiaDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Product { get; set; }

        public void OnGet(int id)
        {
            // Sayfa açýlýrken veriyi veritabanýndan çekiyoruz
            Product = _context.Products.Find(id);
        }

        public IActionResult OnPost()
        {
            if (Product == null || Product.Id == 0)
            {
                return RedirectToPage("./Index");
            }

            // Güncellenecek asýl veriyi bul
            var productToUpdate = _context.Products.Find(Product.Id);

            if (productToUpdate != null)
            {
                // --- TÜM BÝLGÝLERÝ GÜNCELLE ---
                productToUpdate.Name = Product.Name;
                productToUpdate.Price = Product.Price;
                productToUpdate.StockQuantity = Product.StockQuantity;
                productToUpdate.AnimalType = Product.AnimalType;
                productToUpdate.ProductType = Product.ProductType;
                productToUpdate.ImageUrl = Product.ImageUrl;
                productToUpdate.Description = Product.Description; // Açýklama da eklendi

                _context.SaveChanges();
            }

            return RedirectToPage("./Index");
        }
    }
}