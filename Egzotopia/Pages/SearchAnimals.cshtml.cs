using Egzotopia.Models;
using Egzotopia.Services.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Egzotopia.Pages
{
    public class SearchAnimalsModel : PageModel
    {
        private readonly IAnimalService _animalService;

        public SearchAnimalsModel(IAnimalService animalService)
        {
            _animalService = animalService;
        }

        public List<Animal> Results { get; set; } = new List<Animal>();

        [BindProperty(SupportsGet = true)]
        public string Query { get; set; }

        public string ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            if (string.IsNullOrEmpty(Query))
            {
                return; // Arama yapýlmadýysa boþ kalsýn
            }

            try
            {
                // 1. Ninja API'den arama sonuçlarýný çek
                var ninjaResults = await _animalService.GetAnimalsByNameAsync(Query);

                if (ninjaResults == null || !ninjaResults.Any())
                {
                    ErrorMessage = $"'{Query}' kriterine uygun hayvan bulunamadý.";
                    return;
                }

                // 2. Her sonuç için bilgi ve resmi PARALEL olarak çek
                var tasks = ninjaResults.Select(async ninja =>
                {
                    // Pexels'ten resmi ve Ninja'dan bilgiyi alýyoruz
                    var imageTask = _animalService.GetPexelsImageUrl(ninja.name);

                    var imageUrl = await imageTask;

                    // DTO'muza (Animal sýnýfýna) çeviriyoruz
                    return new Animal
                    {
                        Name = ninja.name,
                        ScientificName = ninja.taxonomy?.scientific_name ?? "Bilinmiyor",
                        Habitat = ninja.characteristics?.habitat ?? "Belirtilmemiþ",
                        ImageUrl = imageUrl
                    };
                });

                // Hepsini listeye ekle
                Results = (await Task.WhenAll(tasks.Where(t => t != null))).ToList();
            }
            catch (Exception)
            {
                ErrorMessage = "Arama sýrasýnda API baðlantý hatasý oluþtu.";
            }
        }
    }
}