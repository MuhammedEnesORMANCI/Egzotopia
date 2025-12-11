using Egzotopia.Models;
using Egzotopia.Services.Abstract; // Servis arayüzü
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Egzotopia.Pages.Admin
{
    // Güvenlik için AdminBasePage'den miras alýyoruz
    public class ManageAnimalsModel : AdminBasePage
    {
        private readonly IAnimalService _animalService;

        public ManageAnimalsModel(IAnimalService animalService)
        {
            _animalService = animalService;
        }

        // Arama Sonuçlarýný Tutacak Liste (API Modeli: NinjaAnimal)
        public List<NinjaAnimal> SearchResults { get; set; }

        // Arama Kutusu (URL'den gelir: ?SearchQuery=lion)
        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        public string ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            // Eðer arama kutusu doluysa API'ye git
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                try
                {
                    // Servisindeki isme göre arama metodunu çaðýrýyoruz.
                    // (Not: AnimalApiService içinde GetAnimalsByNameAsync metodu olmalý)
                    SearchResults = await _animalService.GetAnimalsByNameAsync(SearchQuery);

                    if (SearchResults == null || !SearchResults.Any())
                    {
                        ErrorMessage = "Bu isimde bir hayvan bulunamadý.";
                    }
                }
                catch
                {
                    ErrorMessage = "API ile iletiþim kurulamadý. Ýnternet baðlantýnýzý kontrol edin.";
                }
            }
        }
    }
}