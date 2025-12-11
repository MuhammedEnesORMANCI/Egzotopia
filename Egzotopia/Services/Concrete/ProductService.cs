using Egzotopia.Data;
using Egzotopia.Models;
using Egzotopia.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Egzotopia.Services.Concrete
{
    public class ProductService : IProductService
    {
        private readonly EgZotopiaDbContext _context;

        public ProductService(EgZotopiaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            // Bu satırın DİREKT veritabanından çekiyor olması gerekir.
            return await _context.Products.ToListAsync();
        }

        // ... (Diğer metotlar aşağıda devam ediyor)
        public async Task<List<Product>> GetProductsByAnimalTypeAsync(string animalType)
        {
            return await _context.Products
                                 .Where(p => p.AnimalType.ToLower() == animalType.ToLower())
                                 .ToListAsync();
        }
    }
}