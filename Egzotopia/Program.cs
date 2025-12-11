using Egzotopia.Services.Abstract;
using Egzotopia.Services.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Egzotopia.Data;
using Npgsql;

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
            // VERİTABANI BAĞLANTISI
            // --------------------------------------------------

            string connectionString = "";
            var renderConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

            if (!string.IsNullOrEmpty(renderConnectionString))
            {
                // Render'dayız, URL'yi dönüştür
                connectionString = BuildConnectionString(renderConnectionString);
            }
            else
            {
                // Localdeyiz
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            }

            builder.Services.AddDbContext<EgZotopiaDbContext>(options =>
                options.UseNpgsql(connectionString));

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

            // --- TABLO VE VERİ YÜKLEME ---
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<EgZotopiaDbContext>();

                    // 1. Tabloları oluştur
                    context.Database.Migrate();

                    // 2. seed.sql dosyasını yükle
                    if (!context.Users.Any())
                    {
                        var sqlFile = Path.Combine(AppContext.BaseDirectory, "seed.sql");
                        if (File.Exists(sqlFile))
                        {
                            var sqlScript = File.ReadAllText(sqlFile);
                            context.Database.ExecuteSqlRaw(sqlScript);
                            Console.WriteLine("✅ seed.sql başarıyla yüklendi.");
                        }
                        else
                        {
                            Console.WriteLine("⚠️ seed.sql dosyası bulunamadı. (Properties ayarını kontrol et)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Veritabanı hatası: {ex.Message}");
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

        // --- URL PARÇALAYICI (HATA BURADAYDI, DÜZELTİLDİ) ---
        private static string BuildConnectionString(string databaseUrl)
        {
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                // DÜZELTME BURADA: Eğer Port -1 gelirse, varsayılan 5432 kullan.
                Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };

            return builder.ToString();
        }
    }
}