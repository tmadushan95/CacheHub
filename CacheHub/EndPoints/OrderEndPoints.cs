using CacheHub.EndPoints.Interfaces;
using CacheHub.Services.Interfaces;

namespace CacheHub.EndPoints
{
    public class OrderEndPoints : IEndPoint
    {
        public void MapEndPoints(IEndpointRouteBuilder app)
        {
            app.MapGet("/allOrders",
                async (ICacheService cacheService, CancellationToken cancellationToken) =>
                {
                    return Results.Ok(new[]
                     {
                        new 
                        {
                            Id =1,
                            Name= "Order from CacheHub" 
                        },
                        new 
                        {
                            Id =2, 
                            Name="Order from Redis"
                        },
                        new 
                        { 
                            Id =3, 
                            Name="Order from Database"
                        },
                    });
                })
            .WithName("createProduct");
        }
    }
}
