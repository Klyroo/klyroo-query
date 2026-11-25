using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klyroo.Query;

/// <summary>Registers the services of <c>Klyroo.Query</c> in the dependency injection container.</summary>
public static class KlyrooQueryServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="QueryState"/> as a scoped service (one per Blazor Server circuit, one per WebAssembly app) unless one is already registered. Call it once at startup.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection, to chain.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddKlyrooQuery(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<QueryState>();
        return services;
    }
}
