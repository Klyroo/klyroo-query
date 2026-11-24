using System.Diagnostics;
using Lux.Query;
using Xunit;

namespace Lux.Query.Tests;

public sealed class PerformanceAndStressTests
{
    private sealed record PerfItem(int Id, string Name, string Category, decimal Price, bool Active);

    private static readonly DataListSchema<PerfItem> Schema = DataListSchema.For<PerfItem>(s => s
        .RowKey(x => x.Id)
        .RowLabel(x => x.Name)
        .Search(x => x.Name, x => x.Category)
        .Field(x => x.Name, f => f.Key("name").Sortable())
        .Field(x => x.Category, f => f.Key("category").Sortable().Filter(DataListFilter.Option().WithFacets()))
        .Field(x => x.Price, f => f.Key("price").Sortable().Filter(DataListFilter.Number(min: 0)))
        .Field(x => x.Active, f => f.Key("active").Filter(DataListFilter.Boolean()))
        .DefaultSort(x => x.Id)
        .MaxPageSize(200));

    [Fact]
    public void InMemory_filters_sorts_facets_and_cursor_pages_10000_rows_fast()
    {
        string[] categories = ["Compute", "Storage", "Networking", "AI", "Analytics"];
        var dataset = Enumerable.Range(1, 10_000)
            .Select(i => new PerfItem(i, $"Resource-{i:D5}", categories[i % categories.Length], (i * 17 % 5000) + 10m, i % 4 != 0))
            .ToArray();

        var sw = Stopwatch.StartNew();
        var req = new DataListRequest(
            new DataListPaging.Cursor(null, Take: 50, Count: true),
            [new DataListSort("price", Descending: true)],
            DataListFilterGroup.And(
                new FilterRule("active", FilterOperator.Is, new FilterValue.Boolean(true)),
                new FilterRule("price", FilterOperator.Between, new FilterValue.Range(new FilterValue.Number(1000m), new FilterValue.Number(4000m)))),
            facets: [new DataListFacetRequest("category", Mode: FacetMode.ExcludeSelf)]);

        var page1 = Schema.ExecuteInMemory(dataset, req);
        var page2 = Schema.ExecuteInMemory(dataset, req with { Paging = new DataListPaging.Cursor(page1.NextCursor, Take: 50, Count: false) });
        sw.Stop();

        Assert.Equal(50, page1.Items.Count);
        Assert.Equal(50, page2.Items.Count);
        Assert.NotNull(page1.NextCursor);
        Assert.Equal(5, page1.Facets![0].Options.Count);
        Assert.Empty(page1.Items.Select(x => x.Id).Intersect(page2.Items.Select(x => x.Id)));
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(5), $"10k query took too long: {sw.Elapsed}");
    }
}
