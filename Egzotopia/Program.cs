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

            // ✅ Razor Pages
            builder.Services.AddRazorPages();
            builder.Services.AddHttpContextAccessor();

            // Servis kayıtları
            builder.Services.AddScoped<IAnimalService, AnimalApiService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddTransient<Egzotopia.Services.EmailService>();

            // 1. API Ninjas HttpClient Konfigürasyonu
            builder.Services.AddHttpClient("ApiNinjasClient", client =>
            {
                var apiKey = builder.Configuration["ApiKeys:ApiNinjasKey"];
                client.BaseAddress = new Uri("https://api.api-ninjas.com/");
                // Eğer key null gelirse hata vermemesi için kontrol
                if (!string.IsNullOrEmpty(apiKey))
                    client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
            });

            // 2. Pexels HttpClient Konfigürasyonu
            builder.Services.AddHttpClient("PexelsClient", client =>
            {
                var apiKey = builder.Configuration["ApiKeys:PexelsKey"];
                client.BaseAddress = new Uri("https://api.pexels.com/v1/");
                if (!string.IsNullOrEmpty(apiKey))
                    client.DefaultRequestHeaders.Add("Authorization", apiKey);
            });

            // --- DEĞİŞİKLİK BURADA BAŞLIYOR (RENDER İÇİN) ---

            // 1. Render'daki bağlantı adresini al
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

            // 2. Eğer Render'da değilsek (Local), appsettings.json'dan al
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            }

            // 3. PostgreSQL Bağlantısını Yap (UseSqlServer YERİNE UseNpgsql)
            builder.Services.AddDbContext<EgZotopiaDbContext>(options =>
                options.UseNpgsql(connectionString));

            // --------------------------------------------------

            // Logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // --- OTOMATİK TABLO OLUŞTURMA (RENDER İÇİN KRİTİK) ---
            // Bu kod site açılırken veritabanı tabloları yoksa oluşturur.
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<EgZotopiaDbContext>();
                    // Veritabanını ve tabloları oluşturur
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Veritabanı oluşturulurken hata meydana geldi.");
                }
            }
            // -----------------------------------------------------

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