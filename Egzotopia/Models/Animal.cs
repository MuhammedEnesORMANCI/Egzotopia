using System.ComponentModel.DataAnnotations.Schema; // Bunu eklemeyi unutma

namespace Egzotopia.Models
{
    // BU ETÝKET ÇOK ÖNEMLÝ: Veritabanýnda tablo oluþmasýný engeller.
    [NotMapped]
    public class Animal
    {
        // ID veritabaný olmadýðý için gereksiz ama API'den geliyorsa tutabilirsin.
        public int Id { get; set; }
        public string Name { get; set; } // Örn: "Lion"
        public string ScientificName { get; set; }
        public string Description { get; set; }
        public string Habitat { get; set; }
        public string ImageUrl { get; set; }

        // Product listesi tutmaya gerek yok çünkü veritabaný baðý yok.
    }
}