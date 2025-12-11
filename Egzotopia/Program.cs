using Egzotopia.Services.Abstract;
using Egzotopia.Services.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Egzotopia.Data;
using Npgsql; // Bunu eklediğinden emin ol

namespace Egzotopia
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Razor Pages
            builder.Services.AddRazorPages();
            builder.Services.AddHttpContextAccessor();

            // Servisler
            builder.Services.AddScoped<IAnimalService, AnimalApiService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddTransient<Egzotopia.Services.EmailService>();

            // HttpClient Ayarları
            builder.Services.AddHttpClient("ApiNinjasClient", client =>
            {
                var apiKey = builder.Configuration["ApiKeys:ApiNinjasKey"];
                client.BaseAddress = new Uri("https://api.api-ninjas.com/");
                if (!string.IsNullOrEmpty(apiKey))
                    client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
            });

            builder.Services.AddHttpClient("PexelsClient", client =>
            {
                var apiKey = builder.Configuration["ApiKeys:PexelsKey"];
                client.BaseAddress = new Uri("https://api.pexels.com/v1/");
                if (!string.IsNullOrEmpty(apiKey))
                    client.DefaultRequestHeaders.Add("Authorization", apiKey);
            });

            // --- VERİTABANI BAĞLANTISI (DÜZELTİLEN KISIM) ---

            var connectionString = "";

            // 1. Önce Render'dan gelen adresi al
            var databaseUrl = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

            // 2. Eğer Render'daysak (URL doluysa), bunu Npgsql formatına çevir
            if (!string.IsNullOrEmpty(databaseUrl))
            {
                try
                {
                    // Render URL'si: postgres://user:password@host:port/database
                    var databaseUri = new Uri(databaseUrl);
                    var userInfo = databaseUri.UserInfo.Split(':');

                    var builderDb = new NpgsqlConnectionStringBuilder
                    {
                        Host = databaseUri.Host,
                        Port = databaseUri.Port,
                        Username = userInfo[0],
                        Password = userInfo[1],
                        Database = databaseUri.LocalPath.TrimStart('/'),
                        SslMode = SslMode.Require, // Render SSL ister
                        TrustServerCertificate = true
                    };

                    connectionString = builderDb.ToString();
                    Console.WriteLine("✅ Render veritabanı adresi başarıyla dönüştürüldü.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ URL dönüştürme hatası: " + ex.Message);
                    // Hata olursa ham haliyle deneyelim
                    connectionString = databaseUrl;
                }
            }
            else
            {
                // 3. Render'da değilsek yerel ayarı kullan
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            }

            // 4. PostgreSQL'i başlat
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

            // --- TABLO OLUŞTURMA VE SEED ---
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<EgZotopiaDbContext>();
                    context.Database.Migrate(); // Tabloları oluştur

                    // Seed SQL'i çalıştır
                    if (!context.Users.Any())
                    {
                        var sqlFile = Path.Combine(AppContext.BaseDirectory, "seed.sql");
                        if (File.Exists(sqlFile))
                        {
                            var sqlScript = File.ReadAllText(sqlFile);
                            context.Database.ExecuteSqlRaw(sqlScript);
                            Console.WriteLine("✅ seed.sql yüklendi.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "❌ Veritabanı başlatma hatası.");
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
    }
}