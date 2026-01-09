using CacheHub.EndPoints;

namespace CacheHub.Configuration
{
    public static class ConfigureApplicationEndPoints
    {
        public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapWeatherForecastEndPoints();

            return app;
        }
    }
}
