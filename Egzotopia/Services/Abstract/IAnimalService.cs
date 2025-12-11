using Egzotopia.Models;

namespace Egzotopia.Services.Abstract
{
    // Services/Abstract/IAnimalService.cs
    public interface IAnimalService
    {
        Task<List<NinjaAnimal>> GetAnimalsByNameAsync(string name);

        Task<string> GetPexelsImageUrl(string query);

    }
}
