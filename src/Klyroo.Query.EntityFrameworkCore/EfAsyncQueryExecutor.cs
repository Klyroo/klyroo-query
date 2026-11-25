using Microsoft.EntityFrameworkCore;

namespace Klyroo.Query.EntityFrameworkCore;

/// <summary>
/// Runs the queries that <c>Klyroo.Query.Core</c> builds (<see cref="DataListQuery{T}"/>, <see cref="DataListFacetQuery{T}"/>) with the asynchronous operators
/// of Entity Framework Core. They are plain static methods, so they work on any <see cref="IQueryable{T}"/> an EF Core provider can run.
/// </summary>
public static class EfAsyncQueryExecutor
{
    /// <summary>Counts the rows of a query in the database (<c>SELECT COUNT(*)</c>), for example <see cref="DataListQuery{T}.Matching"/>.</summary>
    /// <typeparam name="T">The row type.</typeparam>
    /// <param name="query">A query of an Entity Framework Core provider.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    /// <returns>The number of rows.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="query"/> is not backed by an Entity Framework Core query provider.</exception>
    public static async ValueTask<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await query.CountAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Loads the rows of a query into a list, for example <see cref="DataListQuery{T}.Page"/> or <see cref="DataListFacetQuery{T}.Buckets"/>.</summary>
    /// <typeparam name="T">The row type.</typeparam>
    /// <param name="query">A query of an Entity Framework Core provider.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    /// <returns>The rows, in the order of the query.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="query"/> is not backed by an Entity Framework Core query provider.</exception>
    public static async ValueTask<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
