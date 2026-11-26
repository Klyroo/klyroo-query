# Guides

Task-oriented recipes. Each one states a goal, shows complete code and explains the part that is not obvious. The samples use the `Customer` model from [Getting started](getting-started.md):

```csharp
public enum CustomerStatus { Active, Suspended, Closed }

public sealed record Country(string Name);

public sealed record Customer(
    int Id,
    string Name,
    string? Email,
    CustomerStatus Status,
    int Age,
    Country? Country,
    IReadOnlyList<string> Tags);
```

The samples that run a request use two names from the first guide: `schema`, the `CustomerSchema.Value` declared there, and `customers`, the four customers of [Getting started](getting-started.md#3-run-a-request-in-memory).

- [Describe a row type with a schema](#describe-a-row-type-with-a-schema)
- [Run a request in memory](#run-a-request-in-memory)
- [Answer requests from an API endpoint](#answer-requests-from-an-api-endpoint)
- [Run a request on EF Core](#run-a-request-on-ef-core)
- [Read and write the list state as a URL](#read-and-write-the-list-state-as-a-url)
- [Bind several query keys to one record](#bind-several-query-keys-to-one-record)
- [Link to a state in static server rendering](#link-to-a-state-in-static-server-rendering)
- [Write a codec for your own type](#write-a-codec-for-your-own-type)
- [Filter dates in the user's time zone](#filter-dates-in-the-users-time-zone)
- [Test a list](#test-a-list)

## Describe a row type with a schema

Goal: say which fields of a row type can be sorted, filtered and searched, and what the defaults and limits are.

```csharp
using Klyroo.Query;

public static class CustomerSchema
{
    public static readonly DataListSchema<Customer> Value = DataListSchema.For<Customer>(s => s
        .RowKey(c => c.Id)
        .RowLabel(c => c.Name)
        .Search(c => c.Name, c => c.Email)
        .SearchAny(c => c.Tags, t => t)
        .Field(c => c.Name, f => f.Sortable())
        .Field(c => c.Age, f => f.Sortable().Filter(DataListFilter.Number(min: 0, max: 120, step: 1)))
        .Field(c => c.Status, f => f.Sortable().Filter(DataListFilter.Option().WithFacets()))
        .Field(c => c.Country!.Name, f => f.Key("country").Filter(DataListFilter.Option()))
        .Collection(c => c.Tags, t => t, f => f.Key("tag").Filter(DataListFilter.Option()))
        .QueryFilter("archived", DataListFilter.Boolean(false))
        .DefaultSort(c => c.Name)
        .DefaultPageSize(25)
        .MaxPageSize(100));
}
```

What each line does:

- `RowKey` identifies a row. It is the tie-break of every sort, so pages never repeat or skip a row, and cursor paging needs it. `RowLabel` names a row for assistive technology (`GetRowLabel`).
- `Search` lists the string members the search box looks in. A row matches when any of them contains the text. `SearchAny` does the same for the elements of a collection.
- `Field` registers a member. Its key is the member path in camel case (`age`, `status`), so `c.Country!.Name` would be `country.name`; `f.Key("country")` replaces that. A field that is not a plain member access, or an element read as itself like `t => t`, has no default key and needs `Key`.
- `Collection` reads one value from each element of a collection. A row matches a rule when at least one element does, `isNot` means that no element is any of the values (a row without elements matches), and the key is `tag` here because of `Key("tag")`. A collection field cannot be sorted.
- `QueryFilter` declares a filter that is not a field of the row. Its rules reach your code through `schema.QueryFilterRules(request)`, and neither `ExecuteInMemory` nor `Apply` matches rows against it. Use it for switches such as `archived` that your own query handles. `Boolean(false)` makes `false` the default value.
- `DefaultSort` is the sort used when a request has none, and it is left out of the URL. It makes its field sortable if it was not.

A key must be non-empty, contain no whitespace, `|`, `=` or `&`, and not start with `-`, because these characters have a meaning in the URL form. `DataListSchema.For` throws an `InvalidOperationException` when two fields share a key, when a filter kind does not suit the field type (a number filter on a string), or when `DefaultSort("key")` names a field that is not sortable. These errors surface once, where you declare the schema.

Filters are definitions, not just switches. They carry the operators a rule may use, a default rule, number limits, option lists and labels:

```csharp
// Only some operators, plus the "is empty" pair that any kind may add.
var email = DataListFilter.Text().WithOperators(FilterOperator.Contains, FilterOperator.IsEmpty, FilterOperator.IsNotEmpty);

// A default rule: effective without a URL override, left out of the URL, not counted as an active filter.
var status = DataListFilter.Option()
    .WithDefault(new FilterValue.List(["Active"]), FilterOperator.IsAnyOf)
    .WithFacets();

// Declared options with labels, instead of the values found in the data.
var tier = DataListFilter.Option(
    new DataListFilterOption("Active", "Active customers"),
    new DataListFilterOption("Closed", "Closed accounts"));

// A date filter at hour precision, for ranges of at most 31 days.
var created = DataListFilter.Date(DateGranularity.Hour).WithMaxRangeDays(31);

// A filter whose last value a list remembers between visits.
var region = DataListFilter.Option().WithPersist();
```

The executors read the operators, the default and the value shape. The rest (labels, options, number display, `WithFacets`, `WithOptionLimit`, `WithRemoteOptions`, `WithPersist`) are hints for an editor or a list, such as those of Klyroo.Table; the executors ignore them. `KlyDataList` stores the value of a `WithPersist` filter in the browser's `localStorage`, in the URL form of `DataListStateQuery`, and restores it when the address has no value for that filter. `WithFacets` is the exception in practice: it asks the list to request counts, and the counts come from the facets of a request.

The schema also bounds what a request can ask for: `DefaultPageSize` (50 unless set), `MaxPageSize` (500), `MaxFilterRules` (100), `MaxListValues` (500) and `MaxFacetBuckets` (1000), each with a method on the builder.

## Run a request in memory

Goal: answer a request from a collection you already hold, with search, nested filters, a stable sort, paging and facet counts.

```csharp
using Klyroo.Query;

var request = new DataListRequest(
    new DataListPaging.Offset(Skip: 0, Take: 10),
    [new DataListSort("status"), new DataListSort("age", Descending: true)],
    DataListFilterGroup.And(
        new FilterRule("age", FilterOperator.Between, new FilterValue.Range(new FilterValue.Number(30), null)),
        DataListFilterGroup.Or(
            new FilterRule("status", FilterOperator.IsAnyOf, new FilterValue.List(["Active", "Closed"])),
            new FilterRule("tag", FilterOperator.Is, new FilterValue.Text("vip")))),
    new DataListSearch("a"),
    [new DataListFacetRequest("status")]);

DataListResult<Customer> page = schema.ExecuteInMemory(customers, request);

foreach (var option in page.Facets![0].Options)
{
    Console.WriteLine($"{option.Value}: {option.Count}");
}
```

The filter is a tree: an AND group holding an `age` range and an OR group. A range with a missing bound (`new FilterValue.Range(new FilterValue.Number(30), null)`) is open on that side.

What the in-memory executor does:

- It passes the request through `Sanitize` first, so a rule the schema does not accept is dropped rather than throwing.
- Text `contains` and the search ignore case and accents (invariant culture), and `is` and `isNot` are exact.
- Sorting is stable over several keys. Text uses a natural order that ignores case and accents and reads digit runs as numbers (`item2` before `item10`), nulls come last in both directions, and the row key breaks ties by its own type (2 before 10).
- A null along a path (`c.Country!.Name` with no country) is read as empty instead of throwing.
- `items` is enumerated once.

`InMemoryProvider` wraps the same call in a `DataListProvider<T>`, the delegate shape of "load a page for this request" that an in-memory list, an EF query and an HTTP call can all share:

```csharp
DataListProvider<Customer> provider = CustomerSchema.Value.InMemoryProvider(customers);

DataListResult<Customer> page = await provider(request, CancellationToken.None);
```

Cursor paging walks a sorted list without an offset. Each page carries an opaque `NextCursor`, and `null` means the last page:

```csharp
var all = new List<Customer>();
string? cursor = null;
do
{
    var request = new DataListRequest(
        new DataListPaging.Cursor(After: cursor, Take: 2, Count: true),
        [new DataListSort("age")],
        DataListFilterGroup.Empty);

    DataListResult<Customer> page = schema.ExecuteInMemory(customers, request);
    all.AddRange(page.Items);
    cursor = page.NextCursor;
}
while (cursor is not null);
```

The cursor encodes the sort and the last row's key and sort values, so it stays valid when rows are added or removed between pages. A cursor that does not decode, or that was made for another sort, ends the walk with an empty page and no next cursor, which stops an infinite scroll instead of looping on the first page. The cursor is opaque but not signed: it carries no permission, because filters and authorization are applied again on every request. Cursor paging needs `RowKey`, and it refuses a sort field or key that a cursor cannot carry.

## Answer requests from an API endpoint

Goal: expose a `DataListRequest` over HTTP, so a client sends the request and gets a `DataListResult<T>` back.

The request and result records are annotated for System.Text.Json and have a source-generated context, `DataListJsonContext`. The generic `DataListResult<T>` cannot be in that context, so declare the closed type in a context of your own:

```csharp
using System.Text.Json.Serialization;
using Klyroo.Query;

// DataListResult<T> is generic, so the app declares the closed type in a context of its own.
[JsonSerializable(typeof(DataListResult<Customer>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public sealed partial class AppJsonContext : JsonSerializerContext;
```

Register both contexts with the HTTP JSON options and map the endpoints:

```csharp
// The wire records of Klyroo.Query.Core serialize through their source-generated context, without reflection.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Insert(1, DataListJsonContext.Default);
});
```

```csharp
// The client posts the DataListRequest as JSON.
app.MapPost("/api/customers/query", (DataListRequest request) =>
{
    // Never trust a request: unknown keys, operators and sizes are dropped here.
    DataListRequest clean = schema.Sanitize(request);

    // A query-only filter is for you to apply. The in-memory executor and Apply ignore it.
    bool archived = schema.QueryFilterRules(clean)
        .Any(rule => rule is { Key: "archived", Value: FilterValue.Boolean { Value: true } });

    // Authorization and fixed conditions go before the request is applied.
    IEnumerable<Customer> visible = customers.Where(c => archived || c.Status != CustomerStatus.Closed);

    return schema.ExecuteInMemory(visible, clean);
});

// The URL the components write works as an API URL too.
app.MapGet("/api/customers", (HttpContext http, string? timeZone) =>
{
    var pairs = http.Request.Query.SelectMany(q => q.Value.Select(v => new KeyValuePair<string, string?>(q.Key, v)));

    DataListState state = DataListStateQuery.FromQuery(pairs, schema, prefix: null, timeZone);

    return schema.ExecuteInMemory(customers, state.ToRequest(timeZone: timeZone));
});
```

The POST endpoint reads the request from the body. A client sends this JSON; polymorphic nodes carry a `type` discriminator, enums are camel-case strings (`isAnyOf`, `excludeSelf`) and an integer in place of an enum name is rejected:

```json
{
  "paging": { "type": "offset", "skip": 0, "take": 10 },
  "sort": [{ "key": "age", "descending": true }],
  "filter": {
    "combinator": "and",
    "children": [
      { "type": "rule", "key": "status", "operator": "isAnyOf", "value": { "type": "list", "values": ["Active", "Closed"] } }
    ]
  },
  "facets": [{ "key": "status" }]
}
```

Three points matter here:

- Call `Sanitize` on every request you receive. It clamps the page size to `MaxPageSize`, drops unknown keys, unknown or repeated sort keys, operators the filter does not allow and values of the wrong shape, and bounds the number of rules, list values and nested groups. `ExecuteInMemory` and `Apply` call it too, but your own code reads the request before them (for the query-only filter above).
- Authorization is yours. Restrict the rows before the request is applied, as the endpoint does with `visible`. A request can only narrow what you pass in.
- The GET endpoint shows that the URL written by the components is also a valid API URL: the same `DataListStateQuery.FromQuery` reads it. The `timeZone` argument is the zone in which a date without an offset is read; see [Filter dates in the user's time zone](#filter-dates-in-the-users-time-zone).

## Run a request on EF Core

Goal: execute a request as SQL, with a count, a page and facet counts, and without loading the table.

`Klyroo.Query.Core` does not reference EF Core. `Apply` builds plain `IQueryable` operators and returns them without running anything; you run them. `Klyroo.Query.EntityFrameworkCore` adds `EfAsyncQueryExecutor.CountAsync` and `ToListAsync`, thin wrappers over the EF Core operators. Install it next to your EF Core provider:

```bash
dotnet add package Klyroo.Query.EntityFrameworkCore --prerelease
```

Start from a flat row type, the shape the list shows, and a schema over it:

```csharp
using Klyroo.Query;
using Microsoft.EntityFrameworkCore;

public enum OrderStatus { Draft, Paid, Shipped }

public sealed class Order
{
    public int Id { get; set; }
    public string Reference { get; set; } = "";
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public DateTimeOffset PlacedAt { get; set; }
    public OrderCustomer? Customer { get; set; }
}

public sealed class OrderCustomer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

// The shape the list shows: a flat row type is the leanest thing to query.
public sealed class OrderRow
{
    public int Id { get; set; }
    public string Reference { get; set; } = "";
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public DateTimeOffset PlacedAt { get; set; }
    public string? CustomerName { get; set; }
}
```

```csharp
public static class OrderSchema
{
    public static readonly DataListSchema<OrderRow> Value = DataListSchema.For<OrderRow>(s => s
        .RowKey(o => o.Id)
        .Search(o => o.Reference, o => o.CustomerName)
        .Field(o => o.Reference, f => f.Sortable())
        .Field(o => o.Status, f => f.Sortable().Filter(DataListFilter.Option().WithFacets()))
        .Field(o => o.Total, f => f.Sortable().Filter(DataListFilter.Number(min: 0)))
        .Field(o => o.PlacedAt, f => f.Sortable().Filter(DataListFilter.Date()))
        .Field(o => o.CustomerName, f => f.Sortable())
        .DefaultSort(o => o.PlacedAt, descending: true));
}
```

Then apply the request:

```csharp
using Klyroo.Query.EntityFrameworkCore;

public static async Task<DataListResult<OrderRow>> QueryOrdersAsync(
    DbContext db, DataListRequest request, CancellationToken cancellationToken)
{
    var schema = OrderSchema.Value;
    var clean = schema.Sanitize(request);

    // Authorization and fixed conditions go here, before the request is applied.
    IQueryable<OrderRow> rows = db.Set<Order>()
        .AsNoTracking()
        .Select(o => new OrderRow
        {
            Id = o.Id,
            Reference = o.Reference,
            Status = o.Status,
            Total = o.Total,
            PlacedAt = o.PlacedAt,
            CustomerName = o.Customer!.Name,
        });

    // Nothing has run yet: Apply builds IQueryable operators.
    DataListQuery<OrderRow> query = schema.Apply(rows, clean);

    // Rows carries each row's sort values, which a cursor is made from.
    List<DataListRow<OrderRow>> page = await EfAsyncQueryExecutor.ToListAsync(query.Rows, cancellationToken);
    int? total = query.WantsTotal ? await EfAsyncQueryExecutor.CountAsync(query.Matching, cancellationToken) : null;

    var facets = new List<DataListFacetResult>();
    foreach (DataListFacetQuery<OrderRow> facet in schema.FacetQueries(rows, clean))
    {
        List<DataListFacetBucket> buckets = await EfAsyncQueryExecutor.ToListAsync(facet.Buckets, cancellationToken);
        int? missing = facet.Missing is null ? null : await EfAsyncQueryExecutor.CountAsync(facet.Missing, cancellationToken);
        facets.Add(facet.ToResult(buckets, missing));
    }

    return schema.ToResult(clean, page, total, facets.Count == 0 ? null : facets);
}
```

How it fits together:

- `Apply` returns a `DataListQuery<T>`. `Matching` is the filtered query without order or paging, for the count. `Page` is the sorted and paged query. `Rows` is the same page with each row's sort values and key computed by the database, which a cursor is made from.
- `WantsTotal` is true for offset and unpaged requests, and for a cursor request only when it set `Count`. `ToResult` of an offset request throws `InvalidOperationException` without the total.
- A cursor request needs `Rows`, not `Page`. With cursor paging the query takes one row more than the page size; `ToResult` uses it to know that a next page exists and removes it. The overload that takes plain entities throws for a cursor request.
- `FacetQueries` returns one group-by query per requested facet. A facet counts the rows that match the search and the filters, leaving out its own filter unless it asks for `FacetMode.IncludeSelf`, and counts a row once per distinct value even when several elements of a collection carry it (which needs a `RowKey`). At most `MaxFacetBuckets` values are kept, the most counted first, and the result says `Truncated` when more existed.
- Values travel as parameters, never as literals, and nothing is evaluated on the client: a query the provider cannot translate throws when it runs. `Apply` and `FacetQueries` refuse an in-memory `AsQueryable()` with an `InvalidOperationException`; use `ExecuteInMemory` for those.

The results equal those of `ExecuteInMemory` over the same rows, except where the database decides:

- Text case and accents follow the database. The expressions contain no `ToLower`, so `is`, `isNot`, option lists, the sort and facet values follow the collation of the column, and `contains` and the search follow whatever your provider translates `Contains` to. On EF Core 10.0.15 with SQLite `Contains` is case sensitive: a search for `ALICE` finds nothing where the in-memory executor finds `Alice`. Check your provider.
- Text sorts in the database order, not the natural order of memory.
- Cursor paging refuses a `Guid` sort field or row key, because databases order Guids in their own way.
- Sorting a nullable or reference-typed column puts a `CASE WHEN x IS NULL THEN 1 ELSE 0 END` flag before the value, to keep nulls last in both directions. An expression like that is not an index column, so a provider may not use an index on that column to serve the `ORDER BY`. Sorting by a non-nullable value-type member of the row (an `int`, a `decimal`) adds no flag.

SQLite also cannot order or compare `DateTimeOffset` values. Sorting by one throws `NotSupportedException: SQLite does not support expressions of type 'DateTimeOffset' in ORDER BY clauses`. Declare a converter on the context, here to UTC ticks:

```csharp
public sealed class SqliteShopDb(DbContextOptions<SqliteShopDb> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    // SQLite cannot order or compare DateTimeOffset values. Store them as UTC ticks.
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<UtcTicksConverter>();
}

public sealed class UtcTicksConverter() : ValueConverter<DateTimeOffset, long>(
    value => value.UtcTicks,
    ticks => new DateTimeOffset(ticks, TimeSpan.Zero));
```

Other providers: check the types of your fields against what the provider can compare.

## Read and write the list state as a URL

Goal: turn the state of a list into query keys for a link or an address bar, and read it back, with no Blazor involved.

```csharp
using Klyroo.Query;

// Start from the schema defaults, not from DataListState.Initial, and keep its default filter rules
// when you replace the list of filters: a default rule that is missing is written as removed (f.archived=).
DataListState state = schema.DefaultState with
{
    Search = "alice",
    Sort = [new DataListSort("age", Descending: true)],
    Filters =
    [
        .. schema.DefaultState.Filters,
        new FilterRule("status", FilterOperator.IsAnyOf, new FilterValue.List(["Active", "Closed"])),
        new FilterRule("age", FilterOperator.Between, new FilterValue.Range(new FilterValue.Number(20), new FilterValue.Number(40))),
    ],
    Page = 2,
};

// Only what differs from the defaults is written: q=alice, p=2, s=-age, f.status=Active,Closed, f.age.between=20..40
IReadOnlyDictionary<string, string> query = DataListStateQuery.ToQuery(state, schema);

// Reading never throws. Anything the schema does not accept is dropped.
DataListState read = DataListStateQuery.FromQuery(
    query.Select(pair => new KeyValuePair<string, string?>(pair.Key, pair.Value)),
    schema);
```

The keys, under an optional prefix joined with a dot:

| Key | Meaning |
| :--- | :--- |
| `q` | The search text |
| `p` | The 1-based page, from 1 to `DataListState.MaxPage` |
| `ps` | The page size, from 1 to the schema's maximum |
| `s` | The sort, `name\|-age`; a leading `-` is descending. Present and empty means no sort |
| `f.<key>` | A filter rule with the default operator of its kind |
| `f.<key>.<operator>` | A filter rule with another operator: `contains`, `is`, `isAnyOf`, `isNot`, `gt`, `gte`, `lt`, `lte`, `between`, `before`, `after`, `isEmpty`, `isNotEmpty` |
| `c` | The cursor, in cursor mode |
| `l` | The layout, `table` or `grid` |

A value is a comma-separated list. A comma, `~` or `%` inside a value is written `%2C`, `%7E` or `%25`, a lone `~` means "include empty rows", and a range is `from..to` with either side optional. Numbers use the invariant culture and dates are ISO 8601.

Some behaviors follow from the rule that defaults are left out:

- The state of an untouched list produces no keys. Always start from `schema.DefaultState`, not `DataListState.Initial`: the default state includes the default filter rules, and a default rule that is missing from `Filters` is written as removed (`f.archived=`).
- State filters combine with AND, one rule per key and operator. Groups with OR exist in a request, not in the URL form.
- A filter key that repeats (`f.status=Paid&f.status=Draft`, what a GET form with a checkbox per option sends) is read as one value, and a range can arrive as `f.<key>.from` and `f.<key>.to`.

With several lists on one page, give each a prefix so their keys never collide:

```csharp
// Two lists on one page: each one owns a prefix, so their keys never collide.
IReadOnlyDictionary<string, string> query = DataListStateQuery.ToQuery(
    schema.DefaultState with { Search = "alice", Page = 3 },
    schema,
    prefix: "customers");
// customers.q=alice, customers.p=3
```

## Build your own list component on the query layer

Goal: write a component that owns a key prefix, like `KlyDataList` of Klyroo.Table does, using only public API (the packages share no internals).

```csharp
// Every key and value of the query under the prefix, with the writes of the current handler on top.
IReadOnlyList<KeyValuePair<string, string?>> pairs = query.Snapshot("customers");

DataListState state = DataListStateQuery.FromQuery(pairs, schema, prefix: "customers");

// Describe the fields to draw a header or a card without touching expressions.
foreach (DataListFieldInfo field in schema.FieldInfos)
{
    if (field.IsCollection) continue;
    object? value = schema.ReadField(row, field.Key); // null-safe along the path, collections joined by commas
}
```

`Snapshot` returns a copy, so a later write does not change it, and the empty prefix returns every key. `DataListSchema.DefaultFieldKey(e => e.Department.Name)` gives the key a field gets when the schema does not name one (`department.name`). `FilterRuleText` writes and reads the text of one rule (`OperatorName`, `Escape`, `Unescape`, `Format`, `Parse`), and `LocalInstants.At` gives the instant of a local day and time in a zone with the same rule for clock gaps and repeated hours that the filters use.

## Bind several query keys to one record

Goal: keep a search, a page and a status filter in the URL, read and written together as one record.

`QueryValue<T>` binds one key. A `QueryGroup<TRecord>` binds the properties of an immutable record: one write is one navigation and one history entry, and each property that equals its default is left out of the URL.

```razor
@namespace Samples
@implements IDisposable
@inject QueryState Query

<input type="search" value="@Filter.Search" @oninput="OnSearch" />

<select value="@Filter.Status?.ToString()" @onchange="OnStatus">
    <option value="">Any status</option>
    @foreach (var status in Enum.GetValues<CustomerStatus>())
    {
        <option value="@status">@status</option>
    }
</select>

<p>Page @Filter.Page</p>
<button @onclick="NextPage">Next page</button>

@code {
    public sealed record CustomerFilter(string Search, int Page, CustomerStatus? Status);

    private QueryGroup<CustomerFilter> _group = default!;

    private CustomerFilter Filter => _group.Value;

    protected override void OnInitialized()
    {
        _group = Query
            .Group(new CustomerFilter("", 1, null), owner: "customer list", renderer: RendererInfo, prefix: "customers")
            .Add("q", f => f.Search, (f, v) => f with { Search = v }, QueryCodecs.String)
            .Add("p", f => f.Page, (f, v) => f with { Page = v }, QueryCodecs.Int32)
            .Add("status", f => f.Status, (f, v) => f with { Status = v }, QueryCodecs.Optional(QueryCodecs.Enum<CustomerStatus>()));
        _group.Changed += OnChanged;
    }

    // Typing replaces the current history entry. A new filter or page adds one, so Back undoes it.
    private void OnSearch(ChangeEventArgs e) =>
        _group.Update(f => f with { Search = e.Value?.ToString() ?? "", Page = 1 });

    private void OnStatus(ChangeEventArgs e) =>
        _group.Update(f => f with { Status = Enum.TryParse<CustomerStatus>(e.Value?.ToString(), out var s) ? s : null, Page = 1 }, QueryHistory.Push);

    private void NextPage() =>
        _group.Update(f => f with { Page = f.Page + 1 }, QueryHistory.Push);

    private void OnChanged() => _ = InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        _group.Changed -= OnChanged;
        _group.Dispose();
    }
}
```

Points to notice:

- `Group` takes the default record, an owner name for error messages, the `RendererInfo` of the component and an optional key prefix. With the prefix `customers`, the keys are `customers.q`, `customers.p` and `customers.status`.
- `Add` takes the key, a getter, a function that returns the record with the property set, and a codec. `QueryCodecs.Optional` turns the codec of a struct into the codec of its nullable, so an absent status is `null`.
- `Update` reads the current record, applies your function and writes it. The history argument decides whether the Back button undoes the change: `Replace` (the default) rewrites the current entry, which suits typing, and `QueryHistory.Push` adds an entry, which suits a page or an applied filter.
- Writes made in one event handler, before it first yields to the renderer, merge into one navigation. If any write of the batch asks for `Push`, the whole batch pushes.
- A key that is absent or that the codec cannot read gives the default: `?customers.p=abc` shows page 1.
- A group claims its keys. A second group or `Claim` that uses one of the same keys throws an `InvalidOperationException` that names both owners, which catches two components that would fight over a key. Dispose the group with the component: that releases its keys.

A handler that writes and then navigates to another page (`Query.Set(...); Navigation.NavigateTo("/details/5")`) loses the write: the navigation wins, because the write belonged to the page being left.

## Link to a state in static server rendering

Goal: let a page change its state without interactivity, with plain links and GET forms.

In static server rendering and during prerendering nothing can navigate, so `QueryState` writes nothing there: `Set` returns `false`, and `QueryValue.Value`, `QueryGroup.Set` and `Update` change nothing. `Get` still reads the current URL, and `HrefWith` builds the URL of the current page with changes applied, so the page offers its changes as links.

```razor
@namespace Samples
@inject QueryState Query

@* Plain links: they work in static server rendering, where nothing can navigate. *@
@{
    var page = int.TryParse(Query.Get("p"), out var parsed) ? parsed : 1;
}
<nav>
    @if (page > 1)
    {
        <a href="@Query.HrefWith(("p", page == 2 ? null : (page - 1).ToString()))">Previous</a>
    }
    <a href="@Query.HrefWith(("p", (page + 1).ToString()))">Next</a>
</nav>
```

`HrefWith` keeps the path, the other parameters (`utm_source` stays) and drops a key whose value is `null`. On a page at `/customers?utm_source=mail&p=2`, the links above are `/customers?utm_source=mail` and `/customers?utm_source=mail&p=3`. `QueryValue<T>.HrefWith(value)` and `QueryGroup<TRecord>.HrefWith(record)` do the same for a typed value or record, and leave out a value that equals its default.

On the server side, `DataListStateQuery.FromQuery` reads the keys a GET form sends, including repeated checkbox keys and the `f.<key>.from` and `f.<key>.to` pair of a range, so a statically rendered filter form needs no JavaScript.

## Write a codec for your own type

Goal: put a type the built-in codecs do not cover into a query key.

`IQueryCodec<T>` has two methods. `TryParse` reads the decoded text and returns `false` for text that is not a value, in which case the caller uses its default. `Format` returns the text, or `null` to leave the key out of the URL. Neither throws.

```csharp
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Klyroo.Query;

public readonly record struct Money(decimal Amount, string Currency);

// "12.5EUR" in the URL. A codec never throws: text it cannot read makes TryParse return false.
public sealed class MoneyCodec : IQueryCodec<Money>
{
    public bool TryParse(string raw, [MaybeNullWhen(false)] out Money value)
    {
        value = default;
        if (raw.Length <= 3) return false;

        var amount = raw[..^3];
        if (!decimal.TryParse(amount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed)) return false;

        value = new Money(parsed, raw[^3..]);
        return true;
    }

    // Returning null leaves the key out of the URL.
    public string? Format(Money value) =>
        value.Amount.ToString(CultureInfo.InvariantCulture) + value.Currency;
}
```

```csharp
// The default is left out of the URL: /prices has no key, /prices?max=20EUR carries one.
QueryValue<Money> max = query.Bind("max", new MoneyCodec(), new Money(0m, "EUR"), rendererInfo);
```

The built-in codecs in `QueryCodecs` are `String`, `Int32`, `Int64`, `Decimal`, `Double`, `Boolean`, `DateOnly`, `DateTimeOffset`, `Enum<T>`, and the combinators `Array<T>`, `Range<T>` and `Optional<T>`. Numbers and dates use the invariant culture, so a URL means the same on every machine, and the codec also serves an API that parses the URLs the components write. Combinators compose: `QueryCodecs.Array(QueryCodecs.Int32)` writes `1,2,3`, and `QueryCodecs.Range(QueryCodecs.DateOnly)` writes `2026-01-01..` for a range with no upper end.

## Filter dates in the user's time zone

Goal: make "orders on March 2nd" mean the user's March 2nd, not UTC's.

A date filter on a `DateTimeOffset` or `DateTime` field takes a `FilterValue.Date`, a calendar day. The day is the half-open range from its first instant to the next day's, in the time zone of the request: `DataListRequest.TimeZone`, an IANA id such as `Europe/Paris`, UTC when it is absent.

```csharp
var schema = DataListSchema.For<Shipment>(s => s
    .RowKey(x => x.Id)
    .Field(x => x.ShippedAt, f => f.Filter(DataListFilter.Date())));
```

```csharp
// "Is 2026-03-02" means that calendar day in the user's time zone: [2026-03-02T00:00+01:00, 2026-03-03T00:00+01:00).
var request = new DataListRequest(
    DataListPaging.None.Instance,
    [],
    DataListFilterGroup.And(new FilterRule("shippedAt", FilterOperator.Is, new FilterValue.Date(new DateOnly(2026, 3, 2)))),
    timeZone: "Europe/Paris");
```

A shipment at `2026-03-01T23:30Z` is on March 2nd in Paris, so this request matches it and the same request without a time zone does not. A day on a daylight saving change is 23 or 25 hours long. `DateOnly` fields compare the day itself and do not depend on the zone.

The client sends the zone with each request, and it is never part of the URL. When you build the request from a state, pass it to `ToRequest`:

```csharp
// The client sends its IANA time zone id with each request. It is never part of the URL.
DataListRequest request = state.ToRequest(timeZone: "Europe/Paris");
```

`Sanitize` clears an id the server does not know, which means UTC. `DateTime` fields are read as UTC whatever their `Kind`, unless the field says `.StoresLocalTime()`: its values are then wall-clock times in the request's zone, and the bounds are local `DateTime` values of kind `Unspecified`, which is what a PostgreSQL `timestamp without time zone` column or a SQL Server `datetime2` that holds local times needs. Zone ids are resolved with `TimeZoneInfo`, so the machine needs the zone data for the ids your clients send.

For a finer precision, `DataListFilter.Date(DateGranularity.Hour)` (or `Minute`, `Second`) edits `FilterValue.Moment` values, and a rule matches the whole hour, minute or second that contains the instant.

## Test a list

Goal: test a schema, a URL round trip, a component bound to the URL and an EF Core query, in xUnit.

The test project references the three packages and, for these tests, bUnit and the SQLite provider of EF Core:

```xml
<ItemGroup>
  <PackageReference Include="xunit.v3" Version="4.0.3" />
  <PackageReference Include="bunit" Version="2.15.2" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.15" />
</ItemGroup>
```

Schemas, requests and URL forms are plain functions, so they test without a host:

```csharp
using Klyroo.Query;
using Xunit;

public sealed class CustomerSchemaTests
{
    [Fact]
    public void Search_is_case_insensitive_and_sorted_by_the_default_sort()
    {
        var request = new DataListRequest(
            new DataListPaging.Offset(Skip: 0, Take: 10),
            [],
            DataListFilterGroup.Empty,
            new DataListSearch("MARTIN"));

        var result = CustomerSchema.Value.ExecuteInMemory(SampleData.Customers, request);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Alice Martin", Assert.Single(result.Items).Name);
    }

    [Fact]
    public void A_state_survives_a_trip_through_the_url()
    {
        var schema = CustomerSchema.Value;
        var state = schema.DefaultState with
        {
            Search = "dan",
            Filters = [new FilterRule("status", FilterOperator.IsAnyOf, new FilterValue.List(["Closed"]))],
        };

        var query = DataListStateQuery.ToQuery(state, schema);
        var back = DataListStateQuery.FromQuery(query.Select(p => new KeyValuePair<string, string?>(p.Key, p.Value)), schema);

        Assert.Equal("dan", query["q"]);
        Assert.Equal(state, back);
    }

    [Fact]
    public void Sanitize_drops_what_the_schema_does_not_allow()
    {
        var hostile = new DataListRequest(
            new DataListPaging.Offset(Skip: 0, Take: 1_000_000),
            [new DataListSort("password")],
            DataListFilterGroup.And(new FilterRule("secret", FilterOperator.Is, new FilterValue.Text("x"))));

        var clean = CustomerSchema.Value.Sanitize(hostile);

        Assert.Equal(CustomerSchema.Value.MaxPageSize, ((DataListPaging.Offset)clean.Paging).Take);
        Assert.Empty(clean.Sort);
        Assert.Empty(clean.Filter.Children);
    }
}
```

A component that binds the URL tests with bUnit. Register the service, tell the context the renderer is interactive, and navigate before rendering. Writes reach the URL after the event handler yields, so assert with `WaitForAssertionAsync`:

```csharp
using Bunit;
using Klyroo.Query;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class CustomerSearchTests : BunitContext
{
    public CustomerSearchTests()
    {
        Services.AddKlyrooQuery();

        // The components bind with their RendererInfo. Without an interactive one, writes are ignored.
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
    }

    [Fact]
    public void Reads_its_initial_value_from_the_url()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("http://localhost/customers?q=dan");

        var cut = Render<CustomerSearch>();

        Assert.Equal("Dan Reyes", cut.Find("li").TextContent);
    }

    [Fact]
    public async Task Typing_writes_the_q_key_to_the_url()
    {
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("http://localhost/customers");

        var cut = Render<CustomerSearch>();
        cut.Find("input").Input("alice");

        // Writes reach the URL once the event handler has yielded, so wait for them.
        await cut.WaitForAssertionAsync(() => Assert.EndsWith("?q=alice", navigation.Uri));
        Assert.Single(cut.FindAll("li"));
    }
}
```

To test the SQL, run the request against SQLite. An in-memory SQLite database lives as long as its connection, so keep it open for the test:

```csharp
using Klyroo.Query;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed class OrderQueryTests
{
    [Fact]
    public async Task Cursor_paging_walks_every_row_once()
    {
        var ct = TestContext.Current.CancellationToken;

        // An in-memory SQLite database lives as long as its connection.
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync(ct);
        var options = new DbContextOptionsBuilder<SqliteShopDb>().UseSqlite(connection).Options;

        await using var db = new SqliteShopDb(options);
        await db.Database.EnsureCreatedAsync(ct);
        db.Orders.AddRange(Enumerable.Range(1, 5).Select(i => new Order
        {
            Id = i,
            Reference = $"A-{i}",
            PlacedAt = new DateTimeOffset(2026, 3, i, 0, 0, 0, TimeSpan.Zero),
        }));
        await db.SaveChangesAsync(ct);

        var seen = new List<int>();
        string? cursor = null;
        do
        {
            var request = new DataListRequest(
                new DataListPaging.Cursor(After: cursor, Take: 2),
                [],
                DataListFilterGroup.Empty);

            var page = await OrderQueries.QueryOrdersAsync(db, request, ct);

            seen.AddRange(page.Items.Select(o => o.Id));
            cursor = page.NextCursor;
        }
        while (cursor is not null);

        // The schema sorts by PlacedAt descending, then by the row key.
        Assert.Equal([5, 4, 3, 2, 1], seen);
    }
}
```

SQLite is a convenient provider for tests of `Apply`, but it is not your production database: the differences listed in [Run a request on EF Core](#run-a-request-on-ef-core), text case above all, are the ones that can make a SQLite test pass where another provider behaves differently.
