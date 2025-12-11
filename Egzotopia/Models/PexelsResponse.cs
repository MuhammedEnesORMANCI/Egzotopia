namespace Egzotopia.Models
{
    // Pexels API'den dönen ana yapı
    public class PexelsResponse
    {
        public List<PexelsPhoto> photos { get; set; } = new List<PexelsPhoto>();
    }

    public class PexelsPhoto
    {
        public PexelsSrc src { get; set; }
        public string alt { get; set; } // Resim açıklaması
    }

    public class PexelsSrc
    {
        public string original { get; set; }
        public string large2x { get; set; }
        public string large { get; set; } // Biz bunu kullanacağız
        public string medium { get; set; }
        public string landscape { get; set; }
    }
}