using CacheHub.Services;
using CacheHub.Services.Interfaces;

namespace CacheHub.Configuration
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            try  
            {
                // Register distributed cache service
                services.AddDistributedMemoryCache();

                // Register custom cache service
                services.AddSingleton<ICacheService, CacheService>();

                return services;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while configuring services.", ex);
            }
        }
    }
}
