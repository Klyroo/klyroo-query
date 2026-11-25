using System.Text.Json;
using Klyroo.Query;
using Klyroo.Query.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Klyroo.Query.Tests;

public sealed class ComprehensiveQueryTests
{
    public sealed class EmpEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Dept { get; set; } = "";
        public double Salary { get; set; }
        public bool Active { get; set; }
        public DateOnly HiredOn { get; set; }
        public string? Note { get; set; }
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<EmpEntity> Employees => Set<EmpEntity>();
    }

    private static readonly DataListSchema<EmpEntity> EmpSchema = DataListSchema.For<EmpEntity>(s => s
        .RowKey(e => e.Id)
        .RowLabel(e => e.Name)
        .Search(e => e.Name, e => e.Dept)
        .Field(e => e.Name, f => f.Key("name").Sortable().Filter(DataListFilter.Text()))
        .Field(e => e.Dept, f => f.Key("dept").Sortable().Filter(DataListFilter.Option().WithFacets()))
        .Field(e => e.Salary, f => f.Key("salary").Sortable().Filter(DataListFilter.Number(min: 0)))
        .Field(e => e.Active, f => f.Key("active").Filter(DataListFilter.Boolean()))
        .Field(e => e.HiredOn, f => f.Key("hired").Sortable().Filter(DataListFilter.Date()))
        .Field(e => e.Note, f => f.Key("note").Filter(DataListFilter.Text()))
        .QueryFilter("archived", DataListFilter.Boolean())
        .DefaultSort(e => e.Name)
        .DefaultPageSize(2)
        .MaxPageSize(50));

    private static EmpEntity[] SampleData() =>
    [
        new() { Id = 1, Name = "Alice Martin", Dept = "Engineering", Salary = 120_000, Active = true, HiredOn = new(2024, 3, 15), Note = "Lead" },
        new() { Id = 2, Name = "Bob Chen", Dept = "Engineering", Salary = 95_000, Active = true, HiredOn = new(2025, 1, 10), Note = null },
        new() { Id = 3, Name = "Claire Petit", Dept = "Product", Salary = 110_000, Active = false, HiredOn = new(2023, 6, 1), Note = "" },
        new() { Id = 4, Name = "David Kim", Dept = "Design", Salary = 88_000, Active = true, HiredOn = new(2025, 4, 20), Note = "Remote" },
        new() { Id = 5, Name = "Elena Rossi", Dept = "Product", Salary = 135_000, Active = true, HiredOn = new(2022, 11, 5), Note = null }
    ];

    [Fact]
    public void InMemory_evaluates_nested_And_Or_filters_ranges_and_IncludeEmpty()
    {
        var filter = DataListFilterGroup.And(
            new FilterRule("active", FilterOperator.Is, new FilterValue.Boolean(true)),
            new FilterRule("salary", FilterOperator.Between, new FilterValue.Range(new FilterValue.Number(90_000m), new FilterValue.Number(130_000m))),
            DataListFilterGroup.Or(
                new FilterRule("dept", FilterOperator.IsAnyOf, new FilterValue.List(["Engineering"])),
                new FilterRule("note", FilterOperator.IsEmpty)));

        var res = EmpSchema.ExecuteInMemory(SampleData(), new DataListRequest(
            DataListPaging.None.Instance,
            [new DataListSort("salary", Descending: true)],
            filter));

        Assert.Equal(2, res.TotalCount);
        Assert.Equal(["Alice Martin", "Bob Chen"], res.Items.Select(x => x.Name).ToArray());
    }

    [Fact]
    public void InMemory_facets_support_ExcludeSelf_and_IncludeSelf_with_IncludeMissing()
    {
        var filter = DataListFilterGroup.And(new FilterRule("dept", FilterOperator.Is, new FilterValue.Text("Engineering")));

        var exclude = EmpSchema.ExecuteInMemory(SampleData(), new DataListRequest(
            DataListPaging.None.Instance, [], filter, null,
            facets: [new DataListFacetRequest("dept", Mode: FacetMode.ExcludeSelf)]));
        Assert.Equal(3, exclude.Facets![0].Options.Count);

        var include = EmpSchema.ExecuteInMemory(SampleData(), new DataListRequest(
            DataListPaging.None.Instance, [], filter, null,
            facets: [new DataListFacetRequest("dept", Mode: FacetMode.IncludeSelf)]));
        Assert.Single(include.Facets![0].Options);
        Assert.Equal("Engineering", include.Facets[0].Options[0].Value);
    }

    [Fact]
    public void InMemory_keyset_cursor_paging_walks_every_row_without_skips_or_duplicates()
    {
        var collected = new List<int>();
        string? cursor = null;
        do
        {
            var page = EmpSchema.ExecuteInMemory(SampleData(), new DataListRequest(
                new DataListPaging.Cursor(cursor, Take: 2, Count: true),
                [new DataListSort("salary", Descending: true)],
                DataListFilterGroup.Empty));
            Assert.Equal(5, page.TotalCount);
            collected.AddRange(page.Items.Select(x => x.Id));
            cursor = page.NextCursor;
        } while (cursor is not null);

        Assert.Equal([5, 1, 3, 2, 4], collected);
    }

    [Fact]
    public void DataListStateQuery_roundtrips_cleanly_and_ignores_tampered_inputs()
    {
        var state = new DataListState(
            search: "alice",
            filters:
            [
                new FilterRule("dept", FilterOperator.IsAnyOf, new FilterValue.List(["Engineering", "Product"])),
                new FilterRule("salary", FilterOperator.Between, new FilterValue.Range(new FilterValue.Number(80_000m), new FilterValue.Number(140_000m)))
            ],
            sort: [new DataListSort("salary", Descending: true)],
            page: 3,
            pageSize: 10,
            layout: DataListLayout.Grid);

        var pairs = DataListStateQuery.ToQuery(state, EmpSchema, prefix: "emp");
        var restored = DataListStateQuery.FromQuery(
            pairs.Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value)),
            EmpSchema,
            prefix: "emp");

        Assert.Equal(state, restored);

        // Tampered / hostile query never throws and clamps safely
        var hostile = DataListStateQuery.FromQuery([
            new("emp.p", "999999999"),
            new("emp.ps", "50000"),
            new("emp.f.unknownField", "hack"),
            new("emp.s", "nonexistent")
        ], EmpSchema, prefix: "emp");
        Assert.Equal(DataListState.MaxPage, hostile.Page);
        Assert.Equal(50, hostile.PageSize);
        Assert.Empty(hostile.Filters);
    }

    [Fact]
    public void QueryCodecs_and_DataListJsonContext_roundtrip_without_reflection()
    {
        Assert.True(QueryCodecs.Int32.TryParse("42", out var i) && i == 42);
        Assert.True(QueryCodecs.Boolean.TryParse("true", out var b) && b);
        Assert.True(QueryCodecs.DateOnly.TryParse("2026-05-10", out var d) && d == new DateOnly(2026, 5, 10));
        var rangeCodec = QueryCodecs.Range(QueryCodecs.Int32);
        Assert.True(rangeCodec.TryParse("10..50", out var r) && r.From == 10 && r.To == 50);

        var req = new DataListRequest(
            DataListPaging.Offset.ForPage(2, 10),
            [new DataListSort("salary", Descending: true)],
            DataListFilterGroup.And(new FilterRule("dept", FilterOperator.IsAnyOf, new FilterValue.List(["Engineering"]))),
            new DataListSearch("alice"));

        var json = JsonSerializer.Serialize(req, DataListJsonContext.Default.DataListRequest);
        var parsed = JsonSerializer.Deserialize(json, DataListJsonContext.Default.DataListRequest);
        Assert.Equal(req, parsed);
    }

    [Fact]
    public async Task EfCore_Sqlite_translates_Apply_FacetQueries_and_Cursor_paging_to_SQL()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var conn = new SqliteConnection("DataSource=:memory:");
        await conn.OpenAsync(ct);
        var opts = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(conn).Options;

        await using var db = new TestDbContext(opts);
        await db.Database.EnsureCreatedAsync(ct);
        db.Employees.AddRange(SampleData());
        await db.SaveChangesAsync(ct);

        var req = new DataListRequest(
            DataListPaging.Offset.ForPage(1, 2),
            [new DataListSort("salary", Descending: true)],
            DataListFilterGroup.And(new FilterRule("active", FilterOperator.Is, new FilterValue.Boolean(true))),
            search: null,
            facets: [new DataListFacetRequest("dept", Mode: FacetMode.ExcludeSelf)]);

        var source = db.Employees.AsNoTracking();
        var query = EmpSchema.Apply(source, req);
        var total = await EfAsyncQueryExecutor.CountAsync(query.Matching, ct);
        var page = await EfAsyncQueryExecutor.ToListAsync(query.Page, ct);
        var facets = new List<DataListFacetResult>();
        foreach (var fq in EmpSchema.FacetQueries(source, req))
            facets.Add(fq.ToResult(await EfAsyncQueryExecutor.ToListAsync(fq.Buckets, ct)));

        var result = EmpSchema.ToResult(req, page, total, facets);
        Assert.Equal(4, result.TotalCount);
        Assert.Equal(["Elena Rossi", "Alice Martin"], result.Items.Select(x => x.Name).ToArray());
        Assert.True(result.Facets![0].Options.Count >= 2);
    }
}
