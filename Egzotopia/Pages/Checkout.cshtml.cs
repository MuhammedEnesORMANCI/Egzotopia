using Egzotopia.Data;
using Egzotopia.Models;
using Egzotopia.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Egzotopia.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly EgZotopiaDbContext _context;
        private readonly EmailService _emailService;

        public CheckoutModel(EgZotopiaDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [BindProperty]
        public string UserEmail { get; set; }

        [BindProperty]
        public string CardName { get; set; } = "Ahmet Yýlmaz";

        [BindProperty]
        public string CardNumber { get; set; } = "4543 6000 1234 5678";

        [BindProperty]
        public string ExpiryMonth { get; set; } = "12";

        [BindProperty]
        public string ExpiryYear { get; set; } = "28";

        [BindProperty]
        public string CVV { get; set; } = "145";

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal Total { get; set; }

        public IActionResult OnGet()
        {
            // --- YENÝ EKLENDÝ: GÜVENLÝK KONTROLÜ (KAPI BEKÇÝSÝ) ---
            // Eðer Session'da UserId yoksa, kullanýcý giriþ yapmamýþ demektir.
            // Onu hemen Login sayfasýna postalýyoruz.
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToPage("/Login");
            }
            // -------------------------------------------------------

            LoadCart();
            if (CartItems.Count == 0) return RedirectToPage("/Products");

            // Kullanýcý artýk kesin giriþ yapmýþ olduðu için Session'daki maili alýyoruz.
            string sessionEmail = HttpContext.Session.GetString("UserEmail");
            string sessionUserFullName = HttpContext.Session.GetString("UserFullName");

            // Eðer session'dan mail gelmezse (zor ama) boþ býrakmayalým
            UserEmail = !string.IsNullOrEmpty(sessionEmail) ? sessionEmail : "";
            CardName = !string.IsNullOrEmpty(sessionUserFullName) ? sessionUserFullName : "";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // --- YENÝ EKLENDÝ: GÜVENLÝK KONTROLÜ (POST ÝÇÝN) ---
            // Biri URL'i kandýrýp post atmasýn diye burada da kontrol ediyoruz.
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToPage("/Login");
            }
            // ---------------------------------------------------

            LoadCart();
            if (CartItems.Count == 0) return RedirectToPage("/Products");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Kullanýcý ID'sini Session'dan alýyoruz (Kesin var, yukarýda kontrol ettik)
                int userId = int.Parse(HttpContext.Session.GetString("UserId"));

                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    TotalAmount = Total,
                    Status = "Hazýrlanýyor",
                    Email = UserEmail,
                    FullName = CardName,
                    Phone = "555 123 45 67",
                    Address = "Hýzlý Sipariþ (Prototip Adresi)",
                    City = "Ýstanbul",
                    PostalCode = "34000",
                    PaymentMethod = "Kredi Kartý (Sanal)",
                    UserId = userId // Kullanýcýyý sipariþe baðlýyoruz
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in CartItems)
                {
                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        ProductName = item.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    });

                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= item.Quantity;
                    }
                }
                await _context.SaveChangesAsync();

                try
                {
                    _emailService.SendOrderConfirmation(order.Email, order.Id, Total);
                }
                catch (Exception)
                {
                    throw new Exception("MailError");
                }

                await transaction.CommitAsync();

                HttpContext.Session.Remove("Cart");
                return RedirectToPage("/OrderSuccess", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                if (ex.Message == "MailError")
                {
                    ModelState.AddModelError("UserEmail", "E-posta gönderilemedi. Lütfen geçerli bir e-posta adresi giriniz.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Sipariþ oluþturulurken beklenmeyen bir hata oluþtu.");
                }

                return Page();
            }
        }

        private void LoadCart()
        {
            string? cartJson = HttpContext.Session.GetString("Cart");
            if (!string.IsNullOrEmpty(cartJson))
            {
                CartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
            }
            Total = CartItems.Sum(x => x.Total) + 50;
        }
    }
}