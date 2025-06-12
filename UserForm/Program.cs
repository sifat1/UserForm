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
        var rawUrl = Environment.GetEnvironmentVariable("DATABASE_URL");


        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(rawUrl, npgsqlOptions =>
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

        app.UseRouting();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=HomeController}/{action=index}/{id?}");

        app.Run();
    }
}