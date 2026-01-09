using CacheHub.Model;
using CacheHub.Services.Interfaces;

namespace CacheHub.EndPoints
{
    public static class WeatherForecastEndPoints
    {
        public static void MapWeatherForecastEndPoints(this IEndpointRouteBuilder app)
        {
            // Map the endpoint to get weather forecasts
            app.MapGet("/weatherforecast",
                async (IDistributedCacheService cacheService, CancellationToken cancellationToken) =>
                  await GetWeatherForecast(cacheService, true, cancellationToken))
            .WithName("getWeatherForecast");

            app.MapGet("/removeWeatherforecastCache",
               async (IDistributedCacheService cacheService, CancellationToken cancellationToken) =>
               {
                   var isCacheRemoved = await cacheService.RemoveAsync("weatherforecast", cancellationToken);

                   return Results.Ok(new
                   {
                       success = isCacheRemoved,
                       message = isCacheRemoved
                          ? "Weather forecast cache removed successfully"
                          : "Cache key not found"
                   });
               })
           .WithName("removeWeatherforecastCache");
        }

        /// <summary>
        /// Generates a weather forecast result, optionally including a summary, and returns it as an HTTP response.
        /// </summary>
        /// <param name="cacheService">The cache service used to retrieve or store weather forecast data.</param>
        /// <param name="includeSummary">Specifies whether to include a summary in the weather forecast. Set to <see langword="true"/> to include the
        /// summary; otherwise, <see langword="false"/>.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
        /// <returns>An <see cref="IResult"/> containing the weather forecast data as an HTTP response.</returns>
        private static async Task<IResult> GetWeatherForecast(IDistributedCacheService cacheService, bool includeSummary, CancellationToken cancellationToken)
        {
            return Results.Ok(await CreateForecast(cacheService, includeSummary, cancellationToken));
        }

        /// <summary>
        /// Creates an array of weather forecasts, optionally including summary descriptions, and caches the result for
        /// future retrieval.
        /// </summary>
        /// <remarks>If cached weather forecast data exists, it is returned immediately. Otherwise, new
        /// forecasts are generated and stored in the cache for subsequent calls. The method is thread-safe if the
        /// provided cache service is thread-safe.</remarks>
        /// <param name="cacheService">The cache service used to retrieve and store weather forecast data. Cannot be null.</param>
        /// <param name="includeSummary">Specifies whether to include summary descriptions in each weather forecast. If <see langword="true"/>,
        /// summaries are included; otherwise, they are omitted.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>An array of <see cref="WeatherForecast"/> objects representing the generated weather forecasts. If cached
        /// data is available, the cached array is returned; otherwise, a new array is generated and cached.</returns>
        private static async Task<WeatherForecast[]> CreateForecast(IDistributedCacheService cacheService, bool includeSummary, CancellationToken cancellationToken)
        {

            // Attempt to retrieve cached weather forecasts
            WeatherForecast[]? weatherForecasts = await cacheService.GetAsync<WeatherForecast[]>(
                "weatherforecast",
                cancellationToken);

            // If cached data exists, return it
            if (weatherForecasts is not null)
            {
                // Mark each forecast as cached
                weatherForecasts.ToList().ForEach(wf => wf.IsCached = true);

                return weatherForecasts;
            }

            // Generate new weather forecasts
            string[] summaries = [
               "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            ];

            // Create weather forecasts with or without summaries based on the includeSummary parameter
            IReadOnlyList<WeatherForecast> weathers = [.. Enumerable.Range(1, 5)
                .Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = includeSummary
                        ? summaries[Random.Shared.Next(summaries.Length)]
                        : null,
                    IsCached = false
                })];

            // Cache the newly generated weather forecasts
            weatherForecasts = [.. weathers];

            // Store the generated forecasts in the cache for future retrieval
            await cacheService.SetAsync("weatherforecast", weatherForecasts, cancellationToken);

            // Return the generated weather forecasts
            return weatherForecasts;
        }
    }
}
