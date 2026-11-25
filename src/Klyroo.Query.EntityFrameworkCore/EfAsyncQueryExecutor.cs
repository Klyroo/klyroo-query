using Microsoft.EntityFrameworkCore;

namespace Klyroo.Query.EntityFrameworkCore;

/// <summary>Asynchronous Entity Framework Core query helpers for <c>Klyroo.Query</c>.</summary>
public static class EfAsyncQueryExecutor
{
    public static async ValueTask<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await query.CountAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
