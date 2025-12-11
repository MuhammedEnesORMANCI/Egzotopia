namespace Egzotopia.Models
{
    // API Ninjas'tan gelen ham veri (JSON)
    public class NinjaAnimal
    {
        public string name { get; set; }
        public Taxonomy taxonomy { get; set; }
        public List<string> locations { get; set; }
        public Characteristics characteristics { get; set; }
    }

    public class Taxonomy
    {
        public string kingdom { get; set; }
        public string phylum { get; set; }
        public string @class { get; set; }
        public string order { get; set; }
        public string family { get; set; }
        public string genus { get; set; }
        public string scientific_name { get; set; }
    }

    public class Characteristics
    {
        public string lifespan { get; set; }
        public string weight { get; set; }      // ✅ EKLE
        public string habitat { get; set; }
        public string diet { get; set; }
        public string color { get; set; }       // ✅ EKLE
        public string type { get; set; }        // ✅ EKLE
    }
}
