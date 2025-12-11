using System.ComponentModel.DataAnnotations;

namespace Egzotopia.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad gereklidir.")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta gereklidir.")]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 0: Üye, 1: Yönetici, 2: Süper Admin
        public int Role { get; set; } = 0;

        // İLİŞKİ: Bir kullanıcının sipariş geçmişi olur
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}