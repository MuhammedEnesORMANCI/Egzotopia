using Egzotopia.Services.Abstract;
using Egzotopia.Services.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Egzotopia.Data;

namespace Egzotopia
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ✅ Razor Pages (MVC değil!)
            builder.Services.AddRazorPages();
            builder.Services.AddHttpContextAccessor();

            // Servis kayıtları
            builder.Services.AddScoped<IAnimalService, AnimalApiService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddTransient<Egzotopia.Services.EmailService>();

            // 1. API Ninjas HttpClient Konfigürasyonu
            // Program.cs dosyasındaki mevcut yerini bulun ve bu kodla değiştirin:

            // 1. API Ninjas HttpClient Konfigürasyonu
            builder.Services.AddHttpClient("ApiNinjasClient", client =>
            {
                var apiKey = builder.Configuration["ApiKeys:ApiNinjasKey"]; // Anahtarı çek
                client.BaseAddress = new Uri("https://api.api-ninjas.com/");
                client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
            });

            // 2. Pexels HttpClient Konfigürasyonu
            builder.Services.AddHttpClient("PexelsClient", client =>
            {
                var apiKey = builder.Configuration["ApiKeys:PexelsKey"]; // Anahtarı çek
                client.BaseAddress = new Uri("https://api.pexels.com/v1/");
                client.DefaultRequestHeaders.Add("Authorization", apiKey);
            });

            // Entity Framework Core SQL Server DbContext konfigurasyonu
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<EgZotopiaDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Logging ekle (debug için)
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // --- ESKİ HALİNİ YORUMA ALIN VEYA SİLİN ---
            // if (!app.Environment.IsDevelopment())
            // {
            //     app.UseExceptionHandler("/Error");
            //     app.UseHsts();
            // }

            // --- YERİNE SADECE BUNU YAZIN ---
            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseSession();

            app.MapRazorPages();

            app.Run();
        }
    }
}