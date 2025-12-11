using Egzotopia.Models;
using Egzotopia.Services.Abstract;
using System.Text.Json;

namespace Egzotopia.Services.Concrete
{
    public class AnimalApiService : IAnimalService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AnimalApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // 1. Ninja API (Bilgi)
        public async Task<List<NinjaAnimal>> GetAnimalsByNameAsync(string name)
        {
            var client = _httpClientFactory.CreateClient("ApiNinjasClient");
            try
            {
                var response = await client.GetAsync($"v1/animals?name={name}");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var animals = await response.Content.ReadFromJsonAsync<List<NinjaAnimal>>(options);
                    return animals ?? new List<NinjaAnimal>();
                }
            }
            catch (Exception) { /* Loglama yapılabilir */ }
            return new List<NinjaAnimal>();
        }

        // 2. Pexels API (Resim) - YENİ METOT
        public async Task<string> GetPexelsImageUrl(string query)
        {
            var client = _httpClientFactory.CreateClient("PexelsClient");
            try
            {
                // Sadece 1 tane (per_page=1) yatay (landscape) resim istiyoruz
                var response = await client.GetAsync($"search?query={query}&per_page=1&orientation=landscape");

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var pexelsData = await response.Content.ReadFromJsonAsync<PexelsResponse>(options);

                    // Eğer fotoğraf varsa ilkini, yoksa boş döndür
                    if (pexelsData != null && pexelsData.photos.Count > 0)
                    {
                        return pexelsData.photos[0].src.landscape;
                    }
                }
            }
            catch (Exception) { /* Hata olursa varsayılan resim döneriz */ }

            // Resim bulunamazsa varsayılan bir doğa resmi dönsün
            return "https://images.pexels.com/photos/1108099/pexels-photo-1108099.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1";
        }
    }
}