using CacheHub.EndPoints.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace CacheHub.EndPoints.Extensions
{
    public static class EndPointExtension
    {
        public static IServiceCollection AddApplicationEndPoints(this IServiceCollection services)
        {
            services.AddApplicationEndPoints(Assembly.GetEntryAssembly()!);

            return services;
        }

        public static IServiceCollection AddApplicationEndPoints(this IServiceCollection services, Assembly assembly)
        {
            ServiceDescriptor[] serviceDescriptors = [.. assembly
                .DefinedTypes
                .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                             type.IsAssignableTo(typeof(IEndPoint)))
                .Select(type => ServiceDescriptor.Transient(typeof(IEndPoint), type))
            ];

            services.TryAddEnumerable(serviceDescriptors);

            return services;
        }

        public static IApplicationBuilder MapEndPoints(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null)
        {
            IEnumerable<IEndPoint> endPoints = app.Services.GetRequiredService<IEnumerable<IEndPoint>>();

            IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

            foreach (IEndPoint endPoint in endPoints)
            {
                endPoint.MapEndPoints(builder);
            }

            return app;
        }
    }
}
