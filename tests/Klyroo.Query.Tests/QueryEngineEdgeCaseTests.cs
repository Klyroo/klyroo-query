using System.Text.Json;
using Bunit;
using Klyroo.Query;
using Klyroo.Query.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Klyroo.Query.Tests;

public sealed class QueryEngineEdgeCaseTests : BunitContext
{
    public enum DealStage { Lead, Qualified, Won, Lost }

    public sealed class DealEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Region { get; set; } = "";
        public double Amount { get; set; }
        public bool Enterprise { get; set; }
        public DateOnly CloseDate { get; set; }
        public string? Owner { get; set; }
        public DealStage Stage { get; set; }
    }

    private sealed class DealDbContext(DbContextOptions<DealDbContext> options) : DbContext(options)
    {
        public DbSet<DealEntity> Deals => Set<DealEntity>();
    }

    private static readonly DataListSchema<DealEntity> Schema = DataListSchema.For<DealEntity>(s => s
        .RowKey(d => d.Id)
        .RowLabel(d => d.Title)
        .Search(d => d.Title, d => d.Region, d => d.Owner)
        .Field(d => d.Title, f => f.Key("title").Sortable().Filter(DataListFilter.Text()))
        .Field(d => d.Region, f => f.Key("region").Sortable().Filter(DataListFilter.Option().WithFacets()))
        .Field(d => d.Amount, f => f.Key("amount").Sortable().Filter(DataListFilter.Number(min: 0)))
        .Field(d => d.Enterprise, f => f.Key("enterprise").Filter(DataListFilter.Boolean()))
        .Field(d => d.CloseDate, f => f.Key("closeDate").Sortable().Filter(DataListFilter.Date()))
        .Field(d => d.Owner, f => f.Key("owner").Sortable().Filter(DataListFilter.Text().WithOperators(
            FilterOperator.Contains, FilterOperator.Is, FilterOperator.IsNot, FilterOperator.IsEmpty, FilterOperator.IsNotEmpty)))
        .Field(d => d.Stage, f => f.Key("stage").Sortable().Filter(DataListFilter.Option().WithFacets()))
        .DefaultSort(d => d.Id)
        .DefaultPageSize(3)
        .MaxPageSize(25));

    private static DealEntity[] SampleDeals() =>
    [
        new() { Id = 1, Title = "Étoile Cloud", Region = "EU", Amount = 120_000, Enterprise = true, CloseDate = new(2026, 1, 15), Owner = "Alice", Stage = DealStage.Won },
        new() { Id = 2, Title = "Apex Analytics", Region = "US", Amount = 45_000, Enterprise = false, CloseDate = new(2026, 2, 10), Owner = "Bob", Stage = DealStage.Qualified },
        new() { Id = 3, Title = "Nordic Retail", Region = "EU", Amount = 85_000, Enterprise = true, CloseDate = new(2026, 3, 20), Owner = null, Stage = DealStage.Won },
        new() { Id = 4, Title = "Pacific Logistics", Region = "APAC", Amount = 85_000, Enterprise = true, CloseDate = new(2026, 4, 5), Owner = "", Stage = DealStage.Lead },
        new() { Id = 5, Title = "Solaris Bank", Region = "EU", Amount = 210_000, Enterprise = true, CloseDate = new(2026, 5, 1), Owner = "Chloé", Stage = DealStage.Lost },
        new() { Id = 6, Title = "Café Express", Region = "US", Amount = 15_000, Enterprise = false, CloseDate = new(2026, 6, 30), Owner = "David", Stage = DealStage.Qualified }
    ];

    [Fact]
    public void InMemory_Contains_and_Search_are_case_and_accent_insensitive()
    {
        var byContains = Schema.ExecuteInMemory(SampleDeals(), new DataListRequest(
            DataListPaging.None.Instance, [],
            DataListFilterGroup.And(new FilterRule("title", FilterOperator.Contains, new FilterValue.Text("etoile")))));
        Assert.Single(byContains.Items);
        Assert.Equal(1, byContains.Items[0].Id);

        var bySearch = Schema.ExecuteInMemory(SampleDeals(), new DataListRequest(
            DataListPaging.None.Instance, [],
            DataListFilterGroup.Empty,
            new DataListSearch("chloe")));
        Assert.Single(bySearch.Items);
        Assert.Equal(5, bySearch.Items[0].Id);

        var byCafe = Schema.ExecuteInMemory(SampleDeals(), new DataListRequest(
            DataListPaging.None.Instance, [],
            DataListFilterGroup.Empty,
            new DataListSearch("CAFE")));
        Assert.Single(byCafe.Items);
        Assert.Equal(6, byCafe.Items[0].Id);
    }

    [Theory]
    [InlineData(FilterOperator.Gt, 85_000, new[] { 1, 5 })]
    [InlineData(FilterOperator.Gte, 85_000, new[] { 1, 3, 4, 5 })]
    [InlineData(FilterOperator.Lt, 45_000, new[] { 6 })]
    [InlineData(FilterOperator.Lte, 45_000, new[] { 2, 6 })]
    [InlineData(FilterOperator.Is, 85_000, new[] { 3, 4 })]
    public void InMemory_numeric_operators_match_exact_boundaries(FilterOperator op, double value, int[] expectedIds)
    {
        var res = Schema.ExecuteInMemory(SampleDeals(), new DataListRequest(
            DataListPaging.None.Instance,
            [new DataListSort("id", Descending: false)],
            DataListFilterGroup.And(new FilterRule("amount", op, new FilterValue.Number((decimal)value)))));
        Assert.Equal(expectedIds, res.Items.Select(x => x.Id).ToArray());
    }

    [Fact]
    public void InMemory_Between_supports_open_lower_and_open_upper_bounds()
    {
        var minOnly = Schema.ExecuteInMemory(SampleDeals(), new DataListRequest(
            DataListPaging.None.Instance, [new DataListSort("id", Descending: false)],
            DataListFilterGroup.And(new FilterRule("amount", FilterOperator.Between,
                new FilterValue.Range(new FilterValue.Number(120_000m), null)))));
        Assert.Equal([1, 5], minOnly.Items.Select(x => x.Id).ToArray());

        var maxOnly = Schema.ExecuteInMemory(SampleDeals(), new DataListRequest(
            DataListPaging.None.Instance, [new DataListSort("id", Descending: false)],
            DataListFilterGroup.And(new FilterRule("amount", FilterOperator.Between,
                new FilterValue.Range(null, new FilterValue.Number(45_000m))))));
        Assert.Equal([2, 6], maxOnly.Items.Select(x => x.Id).ToArray());
    }

    [Fact]
    public void InMemory_IsNot_IsEmpty_IsNotEmpty_and_Date_Before_After_work_deterministically()
    {
        var notEu = Schema.ExecuteInMemory(SampleDeals(), new DataListRequest(
            DataListPaging.None.Instance, [new DataListSort("id", Descending: false)],
            DataListFilterGroup.And(new FilterRule("region", FilterOperator.IsNot, new FilterValue.List(["EU"])))));
        Assert.Equal([2, 4, 6], notEu.Items.Select(x => x.Id).ToArray());

        var emptyOwners = Schema.ExecuteInMemory(SampleDeals(), new DataListRequest(
            DataListPaging.None.Instance, [new DataListSort("id", Descending: false)],
            DataListFilterGroup.And(new FilterRule("owner", FilterOperator.IsEmpty))));
        Assert.Equal([3, 4], emptyOwners.Items.Select(x => x.Id).ToArray());

        var filledOwners = Schema.ExecuteInMemory(SampleDeals(), new DataListRequest(
            DataListPaging.None.Instance, [new DataListSort("id", Descending: false)],
            DataListFilterGroup.And(new FilterRule("owner", FilterOperator.IsNotEmpty))));
        Assert.Equal([1, 2, 5, 6], filledOwners.Items.Select(x => x.Id).ToArray());

        var beforeMarch = Schema.ExecuteInMemory(SampleDeals(), new DataListRequest(
            DataListPaging.None.Instance, [new DataListSort("id", Descending: false)],
            DataListFilterGroup.And(new FilterRule("closeDate", FilterOperator.Before, new FilterValue.Date(new DateOnly(2026, 3, 1))))));
        Assert.Equal([1, 2], beforeMarch.Items.Select(x => x.Id).ToArray());

        var afterApril = Schema.ExecuteInMemory(SampleDeals(), new DataListRequest(
            DataListPaging.None.Instance, [new DataListSort("id", Descending: false)],
            DataListFilterGroup.And(new FilterRule("closeDate", FilterOperator.After, new FilterValue.Date(new DateOnly(2026, 4, 30))))));
        Assert.Equal([5, 6], afterApril.Items.Select(x => x.Id).ToArray());
    }

    [Fact]
    public void InMemory_multi_sort_tiebreaks_deterministically_on_equal_primary_values()
    {
        var res = Schema.ExecuteInMemory(SampleDeals(), new DataListRequest(
            DataListPaging.None.Instance,
            [new DataListSort("amount", Descending: true), new DataListSort("title", Descending: false)],
            DataListFilterGroup.Empty));

        // Amount 85_000 is shared by Nordic Retail (3) and Pacific Logistics (4) -> Nordic before Pacific
        Assert.Equal([5, 1, 3, 4, 2, 6], res.Items.Select(x => x.Id).ToArray());
    }

    [Fact]
    public void InMemory_offset_paging_slices_pages_and_handles_beyond_end()
    {
        var page1 = Schema.ExecuteInMemory(SampleDeals(), new DataListRequest(
            DataListPaging.Offset.ForPage(1, 2), [new DataListSort("id", Descending: false)], DataListFilterGroup.Empty));
        Assert.Equal([1, 2], page1.Items.Select(x => x.Id).ToArray());
        Assert.Equal(6, page1.TotalCount);

        var page3 = Schema.ExecuteInMemory(SampleDeals(), new DataListRequest(
            DataListPaging.Offset.ForPage(3, 2), [new DataListSort("id", Descending: false)], DataListFilterGroup.Empty));
        Assert.Equal([5, 6], page3.Items.Select(x => x.Id).ToArray());
        Assert.Equal(6, page3.TotalCount);

        var beyondEnd = Schema.ExecuteInMemory(SampleDeals(), new DataListRequest(
            DataListPaging.Offset.ForPage(99, 2), [new DataListSort("id", Descending: false)], DataListFilterGroup.Empty));
        Assert.Empty(beyondEnd.Items);
        Assert.Equal(6, beyondEnd.TotalCount);
    }

    [Fact]
    public void QueryCodecs_cover_all_primitives_enums_arrays_ranges_and_reject_invalid_values()
    {
        Assert.True(QueryCodecs.Int64.TryParse("9876543210", out var l) && l == 9876543210L);
        Assert.True(QueryCodecs.Decimal.TryParse("1234.56", out var dec) && dec == 1234.56m);
        Assert.True(QueryCodecs.Double.TryParse("3.14", out var dbl) && Math.Abs(dbl - 3.14) < 0.0001);
        Assert.False(QueryCodecs.Double.TryParse("NaN", out _));
        Assert.False(QueryCodecs.Double.TryParse("Infinity", out _));
        Assert.False(QueryCodecs.Int32.TryParse("not-an-int", out _));

        var dto = new DateTimeOffset(2026, 6, 15, 14, 30, 0, TimeSpan.Zero);
        var formattedDto = QueryCodecs.DateTimeOffset.Format(dto);
        Assert.NotNull(formattedDto);
        Assert.True(QueryCodecs.DateTimeOffset.TryParse(formattedDto!, out var parsedDto) && parsedDto == dto);

        var enumCodec = QueryCodecs.Enum<DealStage>(lowercase: true);
        Assert.Equal("qualified", enumCodec.Format(DealStage.Qualified));
        Assert.True(enumCodec.TryParse("won", out var stage) && stage == DealStage.Won);
        Assert.False(enumCodec.TryParse("999", out _));

        var arrayCodec = QueryCodecs.Array(QueryCodecs.Int32);
        Assert.True(arrayCodec.TryParse("1,2,3", out var arr) && arr.SequenceEqual([1, 2, 3]));
        Assert.Equal("1,2,3", arrayCodec.Format([1, 2, 3]));
    }

    [Fact]
    public void DataListJsonContext_rejects_numeric_wire_enums_and_roundtrips_cursor_requests()
    {
        var req = new DataListRequest(
            new DataListPaging.Cursor("abc-token", Take: 10, Count: true),
            [new DataListSort("amount", Descending: true)],
            DataListFilterGroup.Or(
                new FilterRule("region", FilterOperator.IsNot, new FilterValue.List(["APAC"])),
                new FilterRule("closeDate", FilterOperator.Between, new FilterValue.Range(
                    new FilterValue.Date(new DateOnly(2026, 1, 1)),
                    new FilterValue.Date(new DateOnly(2026, 12, 31))))),
            new DataListSearch("cloud"),
            facets: [new DataListFacetRequest("region", Mode: FacetMode.IncludeSelf)]);

        var json = JsonSerializer.Serialize(req, DataListJsonContext.Default.DataListRequest);
        Assert.Contains("\"isNot\"", json);
        Assert.Contains("\"includeSelf\"", json);
        var roundtripped = JsonSerializer.Deserialize(json, DataListJsonContext.Default.DataListRequest);
        Assert.Equal(req, roundtripped);

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<FilterOperator>("0", DataListJsonContext.Default.FilterOperator));
    }

    [Fact]
    public void QueryState_synchronizes_URL_parameters_notifications_HrefWith_and_Claim_isolation()
    {
        Services.AddKlyrooQuery();
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("http://localhost/deals?q=initial&page=2");

        var qs = Services.GetRequiredService<QueryState>();
        qs.UseRenderer(new RendererInfo("Server", true));

        Assert.Equal("initial", qs.Get("q"));
        Assert.Equal("2", qs.Get("page"));

        var notified = new List<string>();
        using var sub = qs.Subscribe("deals", keys => notified.AddRange(keys));

        qs.Set("deals.sort", "amount:desc");
        Assert.Equal("amount:desc", qs.Get("deals.sort"));
        Assert.Contains("deals.sort", notified);

        var href = qs.HrefWith(("page", "3"), ("q", null));
        Assert.Contains("page=3", href);
        Assert.DoesNotContain("q=", href);

        using var claim1 = qs.Claim("deals", "TableA");
        claim1.ThrowIfConflicted();
        Assert.Throws<InvalidOperationException>(() => qs.Claim("deals", "TableB"));
    }

    [Fact]
    public void QueryState_Snapshot_lists_the_keys_of_a_prefix_with_the_pending_writes_on_top()
    {
        Services.AddKlyrooQuery();
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("http://localhost/deals?deals.q=abc&deals.p=2&other=1");
        var qs = Services.GetRequiredService<QueryState>();
        qs.UseRenderer(new RendererInfo("Server", true));

        Assert.Equal(["deals.p", "deals.q"], qs.Snapshot("deals").Select(p => p.Key).Order().ToArray());
        Assert.Equal(3, qs.Snapshot("").Count);

        qs.Set("deals.q", "xyz");
        qs.Set("deals.p", null);
        var snapshot = qs.Snapshot("deals");
        Assert.Equal("xyz", Assert.Single(snapshot).Value);
        Assert.Throws<ArgumentNullException>(() => qs.Snapshot(null!));
    }

    [Fact]
    public void Schema_describes_its_fields_for_a_UI_and_reads_a_value_null_safe()
    {
        var schema = DataListSchema.For<Person>(s => s
            .Field(p => p.Name, f => f.Sortable())
            .Field(p => p.Boss!.Name, f => f.Key("bossName")));
        Assert.Equal(["name", "bossName"], schema.FieldInfos.Select(f => f.Key).ToArray());
        Assert.True(schema.FieldInfos[0].Sortable);
        Assert.Equal("ann", schema.ReadField(new Person("ann", null), "name"));
        Assert.Null(schema.ReadField(new Person("ann", null), "bossName"));
        Assert.Equal("bo", schema.ReadField(new Person("ann", new Person("bo", null)), "bossName"));
        Assert.Null(schema.ReadField(new Person("ann", null), "nope"));
        Assert.Equal("boss.name", DataListSchema.DefaultFieldKey((System.Linq.Expressions.Expression<Func<Person, string?>>)(p => p.Boss!.Name)));
    }

    [Fact]
    public void DataListStateQuery_names_the_parameter_of_a_filter_and_escapes_values_like_the_reader_unescapes_them()
    {
        var filter = DataListFilter.Text();
        Assert.Equal("f.name", DataListStateQuery.FilterParameterName("name", filter.DefaultOperator, filter));
        Assert.Equal("f.name.isNot", DataListStateQuery.FilterParameterName("name", FilterOperator.IsNot, filter));
        Assert.Equal("f.name.contains", DataListStateQuery.FilterParameterName("name", FilterOperator.Contains));
        Assert.Equal("isAnyOf", DataListStateQuery.OperatorName(FilterOperator.IsAnyOf));
        Assert.Equal("~", DataListStateQuery.IncludeEmptyToken);
        Assert.Equal("a%2Cb%7E%25", DataListStateQuery.EscapeValue("a,b~%"));
    }

    private sealed record Person(string Name, Person? Boss);

    [Fact]
    public async Task EfCore_Sqlite_executes_keyset_cursor_paging_multi_sort_and_complex_filters_in_SQL()
    {
        var ct = Xunit.TestContext.Current.CancellationToken;
        await using var conn = new SqliteConnection("DataSource=:memory:");
        await conn.OpenAsync(ct);
        var opts = new DbContextOptionsBuilder<DealDbContext>().UseSqlite(conn).Options;

        await using var db = new DealDbContext(opts);
        await db.Database.EnsureCreatedAsync(ct);
        db.Deals.AddRange(SampleDeals());
        await db.SaveChangesAsync(ct);

        // Walk ALL enterprise deals via keyset cursor pagination with ties on Amount (85_000)
        var collected = new List<int>();
        string? cursor = null;
        do
        {
            var req = new DataListRequest(
                new DataListPaging.Cursor(cursor, Take: 2, Count: true),
                [new DataListSort("amount", Descending: true), new DataListSort("title", Descending: false)],
                DataListFilterGroup.And(
                    new FilterRule("enterprise", FilterOperator.Is, new FilterValue.Boolean(true)),
                    new FilterRule("region", FilterOperator.IsAnyOf, new FilterValue.List(["EU", "APAC"]))));

            var source = db.Deals.AsNoTracking();
            var q = Schema.Apply(source, req);
            var total = await EfAsyncQueryExecutor.CountAsync(q.Matching, ct);
            var rows = await EfAsyncQueryExecutor.ToListAsync(q.Rows, ct);
            var result = Schema.ToResult(req, rows, total);

            Assert.Equal(4, result.TotalCount);
            collected.AddRange(result.Items.Select(x => x.Id));
            cursor = result.NextCursor;
        } while (cursor is not null);

        Assert.Equal([5, 1, 3, 4], collected);

        // Verify IsEmpty + Between date filter translates to SQLite cleanly
        var emptyOwnerReq = new DataListRequest(
            DataListPaging.None.Instance,
            [new DataListSort("id", Descending: false)],
            DataListFilterGroup.And(
                new FilterRule("owner", FilterOperator.IsEmpty),
                new FilterRule("closeDate", FilterOperator.Between,
                    new FilterValue.Range(new FilterValue.Date(new DateOnly(2026, 3, 1)), new FilterValue.Date(new DateOnly(2026, 4, 30))))));
        var emptyQ = Schema.Apply(db.Deals.AsNoTracking(), emptyOwnerReq);
        var emptyRows = await EfAsyncQueryExecutor.ToListAsync(emptyQ.Page, ct);
        Assert.Equal([3, 4], emptyRows.Select(x => x.Id).ToArray());
    }
}
