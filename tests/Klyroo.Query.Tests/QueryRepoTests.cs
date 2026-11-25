using Klyroo.Query;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Klyroo.Query.Tests;

public sealed class QueryRepoTests : BunitContext
{
    private sealed record Product(int Id, string Name, decimal Price);

    [Fact]
    public void QueryCore_applies_filter_sort_and_pagination_in_memory()
    {
        var items = new[]
        {
            new Product(1, "Keyboard", 120m),
            new Product(2, "Mouse", 45m),
            new Product(3, "Monitor", 350m),
        };
        var schema = DataListSchema.For<Product>(s => s
            .RowKey(p => p.Id)
            .Search(p => p.Name)
            .Field(p => p.Price, f => f.Key("price").Sortable()));

        var req = new DataListRequest(
            new DataListPaging.Offset(0, 10),
            [new DataListSort("price", Descending: true)],
            DataListFilterGroup.Empty,
            new DataListSearch("M"));

        var res = schema.ExecuteInMemory(items, req);
        Assert.Equal(2, res.TotalCount);
        Assert.Equal("Monitor", res.Items[0].Name);
        Assert.Equal("Mouse", res.Items[1].Name);
    }

    [Fact]
    public void QueryBlazor_registers_and_updates_url_parameters()
    {
        Services.AddKlyrooQuery();
        var qs = Services.GetRequiredService<QueryState>();
        qs.UseRenderer(new Microsoft.AspNetCore.Components.RendererInfo("Server", isInteractive: true));
        Assert.Null(qs.Get("page"));
        Assert.True(qs.Set("page", "3"));
        Assert.Equal("3", qs.Get("page"));
    }
}
