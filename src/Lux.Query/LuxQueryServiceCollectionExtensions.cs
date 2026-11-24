using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blux.UI.Tools;

public static class LuxQueryServiceCollectionExtensions
{
    public static IServiceCollection AddLuxQuery(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<QueryState>();
        return services;
    }
}
