namespace SSOAutoLoginApp.Helpers.DI
{
    public static class DependencyInjection
    {
        public static void InjectedServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
        }
    }
}
