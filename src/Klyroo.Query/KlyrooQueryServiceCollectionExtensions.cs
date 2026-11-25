using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klyroo.Query;

public static class KlyrooQueryServiceCollectionExtensions
{
    public static IServiceCollection AddKlyrooQuery(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<QueryState>();
        return services;
    }
}
