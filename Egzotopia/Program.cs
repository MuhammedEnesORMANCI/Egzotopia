using Egzotopia.Services.Abstract;
using Egzotopia.Services.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Egzotopia.Data;
using Npgsql; // Npgsql kütüphanesi gerekli

namespace Egzotopia
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Razor Pages ve Servisler
            builder.Services.AddRazorPages();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IAnimalService, AnimalApiService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddTransient<Egzotopia.Services.EmailService>();

            // HttpClient Ayarları
            builder.Services.AddHttpClient("ApiNinjasClient", client =>
            {
                var apiKey = builder.Configuration["ApiKeys:ApiNinjasKey"];
                client.BaseAddress = new Uri("https://api.api-ninjas.com/");
                if (!string.IsNullOrEmpty(apiKey)) client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
            });

            builder.Services.AddHttpClient("PexelsClient", client =>
            {
                var apiKey = builder.Configuration["ApiKeys:PexelsKey"];
                client.BaseAddress = new Uri("https://api.pexels.com/v1/");
                if (!string.IsNullOrEmpty(apiKey)) client.DefaultRequestHeaders.Add("Authorization", apiKey);
            });

            // --------------------------------------------------
            // 🔴 KRİTİK DÜZELTME: BAĞLANTI ADRESİ DÖNÜŞTÜRME
            // --------------------------------------------------

            string connectionString = "";
            var renderConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

            if (!string.IsNullOrEmpty(renderConnectionString))
            {
                // Render'dayız, URL'yi parçalayıp Npgsql formatına çeviriyoruz
                connectionString = BuildConnectionString(renderConnectionString);
            }
            else
            {
                // Localdeyiz (Bilgisayarın)
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            }

            // PostgreSQL Bağlantısı
            builder.Services.AddDbContext<EgZotopiaDbContext>(options =>
                options.UseNpgsql(connectionString));

            // --------------------------------------------------

            // Logging ve Session
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // --- TABLO OLUŞTURMA VE VERİ YÜKLEME ---
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<EgZotopiaDbContext>();

                    // 1. Veritabanını oluştur
                    context.Database.Migrate();

                    // 2. Seed SQL dosyasını yükle (Sadece boşsa)
                    if (!context.Users.Any())
                    {
                        var sqlFile = Path.Combine(AppContext.BaseDirectory, "seed.sql");
                        if (File.Exists(sqlFile))
                        {
                            var sqlScript = File.ReadAllText(sqlFile);
                            context.Database.ExecuteSqlRaw(sqlScript);
                            Console.WriteLine("✅ seed.sql başarıyla yüklendi.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Hatayı logla ama uygulamayı durdurma
                    Console.WriteLine($"❌ Veritabanı başlatma hatası: {ex.Message}");
                }
            }

            app.UseDeveloperExceptionPage();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseSession();
            app.MapRazorPages();

            app.Run();
        }

        // --- YARDIMCI METOT: URL PARÇALAYICI ---
        // Render'ın verdiği "postgres://user:pass@host/db" formatını
        // C#'ın istediği "Host=...;Username=..." formatına çevirir.
        private static string BuildConnectionString(string databaseUrl)
        {
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true // SSL sertifika hatasını önler
            };

            return builder.ToString();
        }
    }
}