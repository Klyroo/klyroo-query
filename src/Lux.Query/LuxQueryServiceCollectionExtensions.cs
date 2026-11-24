using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lux.Query;

public static class LuxQueryServiceCollectionExtensions
{
    public static IServiceCollection AddLuxQuery(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<QueryState>();
        return services;
    }
}
