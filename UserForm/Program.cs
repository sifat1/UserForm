using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Users;
using UserForm.Resources;

namespace UserForm;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Services: Register dependencies
        builder.Services.AddScoped<UserService>();

        // 2. Localization setup - Enhanced configuration
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

        // Configure MVC with proper localization
        builder.Services.AddControllersWithViews()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (type, factory) => 
                    factory.Create(typeof(SharedResource));
            });

        // 3. Culture config (use full codes like "en-US")
        var supportedCultures = new[] { "en-US", "pl-PL" };
        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("en-US");
            options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
            options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();

            // Culture providers in priority order
            options.RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider()
            };
        });

        // 4. Database (PostgreSQL) - unchanged
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection") ??
                "Host=localhost;Database=userform;Username=postgres;Password=strong_password;Port=5432",
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                }));

        // 5. Identity - unchanged
        builder.Services.AddIdentity<UserDetails, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 1;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // 6. Cookie setup - unchanged
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/AccountManagement/Login";
            options.AccessDeniedPath = "/AccountManagement/AccessDenied";
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.SlidingExpiration = true;
        });

        // 7. Build app
        var app = builder.Build();

        // 8. Apply localization middleware early - Enhanced
        var locOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
        app.UseRequestLocalization(locOptions?.Value ?? new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("en-US"),
            SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList(),
            SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList()
        });

        // 9. Auto-migrate database (optional) - unchanged
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        // 10. Middleware pipeline - unchanged
        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        // 11. Endpoint routing - unchanged
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=FormManage}/{action=List}/{id?}");

        app.Run();
    }
}