using Egzotopia.Models;

namespace Egzotopia.Services.Abstract
{
    public interface IProductService
    {
        // Tüm ürünleri getir
        Task<List<Product>> GetAllProductsAsync();

        // DEĞİŞEN KISIM: ID yerine string animalType (Örn: "lion") alıyoruz
        Task<List<Product>> GetProductsByAnimalTypeAsync(string animalType);
    }
}