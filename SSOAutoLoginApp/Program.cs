using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using SSOAutoLoginApp.Helpers.DI;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        var environment = builder.Environment.EnvironmentName;
        Console.WriteLine("Environment Name:  " + environment);

        builder.Configuration
           .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
           .AddEnvironmentVariables();

        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

        builder.Services.AddControllersWithViews(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });
        builder.Services.AddRazorPages()
            .AddMicrosoftIdentityUI();

        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        builder.Services.AddRazorPages();
        builder.Services.AddHttpClient();

        builder.Services.InjectedServices();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsProduction())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

        app.UseStaticFiles();

        app.UseRouting();
        app.UseSession();

        app.UseAuthorization();

        app.MapStaticAssets();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.MapRazorPages();

        app.Run();
    }
}