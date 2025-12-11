using Egzotopia.Models;
using Egzotopia.Services.Abstract;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

// HATA ÇÖZÜMÜ: Namespace eklendi
namespace Egzotopia.Pages
{
    public class AnimalsModel : PageModel
    {
        private readonly IAnimalService _animalService;
        private readonly ILogger<AnimalsModel> _logger;

        public AnimalsModel(IAnimalService animalService, ILogger<AnimalsModel> logger)
        {
            _animalService = animalService;
            _logger = logger;
        }

        public List<Animal> Animals { get; set; } = new List<Animal>();
        public bool IsLoading { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedCategory { get; set; }

        // ?? Hayvan kategorileri
        private static readonly Dictionary<string, List<string>> AnimalCategories =
            new Dictionary<string, List<string>>
            {
                ["BigCats"] = new()
                {
                    "lion","tiger","leopard","cheetah","jaguar","snow leopard",
                    "cougar","serval","ocelot","caracal","siberian tiger"
                },
                ["Canines"] = new()
                {
                    "wolf","fennec fox","maned wolf","dhole","tasmanian devil"
                },
                ["Savana"] = new()
                {
                    "elephant","zebra","giraffe","rhinoceros","hippopotamus",
                    "gazelle","wildebeest","springbok","kudu","oryx",
                    "baboon","mandrill","warthog","okapi","aardvark","bushbaby"
                },
                ["Asia_Oceania"] = new()
                {
                    "panda","red panda","sloth","koala","anteater","tapir",
                    "alpaca","llama","vicuna","capybara","armadillo",
                    "chinchilla","kangaroo","wallaby","platypus","echidna",
                    "binturong"
                },
                ["Primates"] = new()
                {
                    "monkey","gorilla","chimpanzee","orangutan","lemur",
                    "aye-aye","marmoset","tarsier","gibbon"
                },
                ["Marine"] = new()
                {
                    "penguin","polar bear","walrus","seal","dolphin","whale",
                    "narwhal","manatee","sea otter","puffin","beluga"
                },
                ["Reptiles"] = new()
                {
                    "komodo dragon","python","anaconda","king cobra",
                    "alligator","crocodile","chameleon","gecko","iguana",
                    "axolotl","poison dart frog","gila monster","snapping turtle"
                },
                ["Birds"] = new()
                {
                    "toucan","flamingo","macaw","cockatoo","quetzal",
                    "cassowary","emu","ostrich","peacock","condor","vulture",
                    "hornbill","kiwi"
                },
                ["Others"] = new()
                {
                    "vampire bat","tarantula","scorpion","salamander",
                    "giant otter","fossa","marten"
                }
            };

        // Animals.cshtml.cs içindeki OnGetAsync metodu

        public async Task OnGetAsync()
        {
            try
            {
                IsLoading = true;
                var random = new Random();
                List<string> pool;

                // Kategori Seçimi
                if (!string.IsNullOrEmpty(SelectedCategory) && AnimalCategories.ContainsKey(SelectedCategory))
                {
                    pool = AnimalCategories[SelectedCategory];
                }
                else
                {
                    pool = AnimalCategories.SelectMany(x => x.Value).ToList();
                }

                // DÜZELTME 1: 12 yerine 15 aday seçiyoruz (Yedekli olsun diye)
                var showcase = pool
                    .OrderBy(_ => random.Next())
                    .Take(15)
                    .ToArray();

                var tasks = showcase.Select(async name =>
                {
                    var infoTask = _animalService.GetAnimalsByNameAsync(name);
                    var imageTask = _animalService.GetPexelsImageUrl(name);

                    await Task.WhenAll(infoTask, imageTask);

                    var data = infoTask.Result.FirstOrDefault();
                    var img = imageTask.Result;

                    if (data != null)
                    {
                        return new Animal
                        {
                            Name = data.name,
                            ScientificName = data.taxonomy?.scientific_name ?? "Unknown",
                            Habitat = data.characteristics?.habitat ?? "Unknown",
                            ImageUrl = img
                        };
                    }
                    return null;
                });

                // Sonuçlarý al, boþlarý filtrele
                var validAnimals = (await Task.WhenAll(tasks)).Where(a => a != null);

                // DÜZELTME 2: Saðlam olanlardan sadece ilk 12 tanesini listeye ekle
                Animals.AddRange(validAnimals.Take(12));

                IsLoading = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hata oluþtu.");
                ErrorMessage = "Veri yüklenirken hata oluþtu.";
                IsLoading = false;
            }
        }
    }
}