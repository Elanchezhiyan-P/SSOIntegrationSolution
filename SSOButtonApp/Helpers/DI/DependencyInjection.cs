using SSOButtonApp.Service.Interface;
using SSOButtonApp.Service.Repository;

namespace SSOButtonApp.Helpers.DI
{
    public static class DependencyInjection
    {
        public static void InjectedServices(this IServiceCollection services)
        {
            services.AddScoped<IAccountManager, AccountManager>();
        }
    }
}
