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

            // --- RENDER İÇİN VERİTABANI BAĞLANTISI ---

            // 1. Render'daki bağlantı adresini al
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

            // 2. Eğer Render'da değilsek (Local), appsettings.json'dan al
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            }

            // 3. PostgreSQL Bağlantısını Yap
            builder.Services.AddDbContext<EgZotopiaDbContext>(options =>
                options.UseNpgsql(connectionString));

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

            // --- OTOMATİK TABLO VE VERİ YÜKLEME (RENDER İÇİN KRİTİK) ---
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<EgZotopiaDbContext>();

                    // 1. ADIM: Tabloları oluştur (Yoksa)
                    context.Database.Migrate();

                    // 2. ADIM: Veri var mı diye kontrol et
                    // Eğer kullanıcı tablosu boşsa, veritabanı boş demektir.
                    if (!context.Users.Any())
                    {
                        // seed.sql dosyasının yolunu bul
                        var sqlFile = Path.Combine(AppContext.BaseDirectory, "seed.sql");

                        // Dosyayı oku ve veritabanına bas
                        if (File.Exists(sqlFile))
                        {
                            var sqlScript = File.ReadAllText(sqlFile);
                            context.Database.ExecuteSqlRaw(sqlScript);
                            var logger = services.GetRequiredService<ILogger<Program>>();
                            logger.LogInformation("✅ seed.sql başarıyla çalıştırıldı ve veriler yüklendi.");
                        }
                        else
                        {
                            var logger = services.GetRequiredService<ILogger<Program>>();
                            logger.LogWarning("⚠️ seed.sql dosyası bulunamadı! (Properties -> Copy to Output Directory ayarını kontrol et)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "❌ Veritabanı başlatılırken hata oluştu.");
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