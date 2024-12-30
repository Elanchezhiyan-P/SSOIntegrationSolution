using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SSOButtonApp.AppDbContext;
using SSOButtonApp.Helpers.DI;
using SSOButtonApp.Models;
using System.Text.Json.Serialization;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        string envName = builder.Environment.EnvironmentName;

        Console.WriteLine("Environment Name:  " + envName);

        builder.Configuration
            .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

        string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        .EnableDetailedErrors());

        // Add services to the container.
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
        //.AddRazorRuntimeCompilation();

        var tenantid = builder.Configuration["AzureAd:TenantId"];

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = "Microsoft";
        }).AddMicrosoftAccount(options =>
        {
            options.ClientId = builder.Configuration["AzureAd:ClientId"];
            options.ClientSecret = builder.Configuration["AzureAd:ClientSecret"];
            options.AuthorizationEndpoint = $"https://login.microsoftonline.com/{tenantid}/oauth2/v2.0/authorize";
            options.TokenEndpoint = $"https://login.microsoftonline.com/{tenantid}/oauth2/v2.0/token";
            options.AccessDeniedPath = "/Account/AccessDenied";
        }).AddCookie(options =>
        {
            options.Cookie.Name = "SSOButtonApp";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.None;
        });

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = true;
        });

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
            options.User.RequireUniqueEmail = true;
        })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        builder.Services.AddRazorPages();
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.InjectedServices();

        builder.Services.AddHttpContextAccessor();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsProduction())
        {
            app.UseExceptionHandler("/Account/Error");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseStatusCodePagesWithReExecute("/Account/Error", "?statusCode={0}");

        app.UseHttpsRedirection();
        app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

        app.UseStaticFiles();
        app.UseRouting();

        app.UseSession();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Account}/{action=Login}")
            .WithStaticAssets();

        app.MapRazorPages();
        app.Run();
    }
}