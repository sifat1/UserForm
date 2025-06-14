using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Users;

namespace UserForm;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews();
        var rawUrl = Environment.GetEnvironmentVariable("DBENV");


        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql("Host=localhost;Database=userform;Username=postgres;Password=strong_password;Port=5432", npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            }));

        builder.Services.AddDefaultIdentity<UserDetails>(options =>
        {
            options.Password.RequireDigit = false; 
            options.Password.RequiredLength = 1;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/AccountManagement/Login"; 
        });
        var app = builder.Build();
        
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }
        
        
        app.UseStaticFiles();
        
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=HomeController}/{action=index}/{id?}");

        app.Run();
    }
}