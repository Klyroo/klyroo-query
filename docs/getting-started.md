# Getting started

This tutorial builds one small list end to end: a `Customer` schema, a request run against an in-memory list, the URL form of the list state, and a Blazor component that keeps a search box in sync with the address bar. The first three steps need only `Klyroo.Query.Core`, so they work in a console app, a worker or an API. The last two need Blazor.

## 1. Install the packages

```bash
dotnet add package Klyroo.Query.Core --prerelease
dotnet add package Klyroo.Query --prerelease        # only for the Blazor steps
```

`Klyroo.Query` depends on `Klyroo.Query.Core`, so a Blazor project needs only the second command. The packages target `net10.0`.

## 2. Describe the row type

A schema says what a request is allowed to do with a row type: which fields can be sorted, which can be filtered and how, which fields the search box looks in, and which field identifies a row.

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

```csharp
using Klyroo.Query;

// A schema says what a request may do with a Customer. Declare it once and share it.
var schema = DataListSchema.For<Customer>(s => s
    .RowKey(c => c.Id)
    .Search(c => c.Name, c => c.Email)
    .Field(c => c.Name, f => f.Sortable())
    .Field(c => c.Age, f => f.Sortable().Filter(DataListFilter.Number(min: 0)))
    .Field(c => c.Status, f => f.Filter(DataListFilter.Option().WithFacets()))
    .DefaultSort(c => c.Name));
```

Fields are lambda expressions, so a renamed property breaks the build instead of a string at run time. The key of a field is its member path in camel case (`age`, `status`); `f.Key("...")` overrides it. A request refers to fields by key, which is why `"age"` appears in the next step.

## 3. Run a request in memory

A `DataListRequest` is plain data: paging, sort rules, a filter tree, an optional search and the facets to count. `ExecuteInMemory` applies it to any collection. This tutorial uses four customers:

```csharp
IReadOnlyList<Customer> customers =
[
    new(1, "Alice Martin", "alice@example.com", CustomerStatus.Active, 34, new Country("France"), ["vip", "newsletter"]),
    new(2, "Bob Stone", "bob@example.com", CustomerStatus.Suspended, 51, new Country("Canada"), []),
    new(3, "Chloe Durand", null, CustomerStatus.Active, 28, null, ["newsletter"]),
    new(4, "Dan Reyes", "dan@example.com", CustomerStatus.Closed, 45, new Country("France"), ["vip"]),
];
```

```csharp
var request = new DataListRequest(
    new DataListPaging.Offset(Skip: 0, Take: 2),
    [new DataListSort("age", Descending: true)],
    DataListFilterGroup.And(
        new FilterRule("status", FilterOperator.IsAnyOf, new FilterValue.List(["Active", "Closed"])),
        new FilterRule("age", FilterOperator.Gte, new FilterValue.Number(30))),
    new DataListSearch("a"),
    [new DataListFacetRequest("status")]);

DataListResult<Customer> result = schema.ExecuteInMemory(customers, request);

Console.WriteLine($"{result.TotalCount} matching rows, showing {result.Items.Count}");
foreach (var customer in result.Items)
{
    Console.WriteLine($"{customer.Name} ({customer.Age})");
}
```

With those customers it prints:

```text
2 matching rows, showing 2
Dan Reyes (45)
Alice Martin (34)
```

`TotalCount` counts the rows that match across all pages, and `result.Facets` holds the count of each `status` value among the rows that match everything except the status filter itself. The same request runs on a database with the `Apply` method of `Klyroo.Query.Core`; see [Run a request on EF Core](guides.md#run-a-request-on-ef-core).

## 4. Read and write the list state as a URL

The state of a list as a user left it (search text, filters, sort, page, page size, layout) converts to query keys and back. Only what differs from the schema defaults is written, so the URL of an untouched list is empty.

```csharp
DataListState state = schema.DefaultState with { Search = "alice", Page = 2 };

IReadOnlyDictionary<string, string> query = DataListStateQuery.ToQuery(state, schema);
// q=alice, p=2

DataListState back = DataListStateQuery.FromQuery(
    query.Select(pair => new KeyValuePair<string, string?>(pair.Key, pair.Value)),
    schema);

Console.WriteLine(back == state); // True
```

Reading never throws. A key the schema does not know, an operator the filter does not allow and a value that does not parse are dropped, and the default applies. That makes it safe to read a URL a user edited by hand.

## 5. Keep a Blazor component in sync with the URL

Register `QueryState` once. It is a scoped service: one instance per Blazor Server circuit, one per WebAssembly app.

```csharp
using Klyroo.Query;

builder.Services.AddKlyrooQuery();
```

Import the namespace in `_Imports.razor`. The package ships no stylesheet and no JavaScript.

```razor
@using Klyroo.Query
```

Then bind a query key to a typed value. `Bind` takes the key, a codec that converts the value to and from text, the value to use when the key is absent, and the `RendererInfo` of the component.

```razor
@namespace Samples
@implements IDisposable
@inject QueryState Query

<input type="search" placeholder="Search customers" value="@_search.Value" @oninput="OnInput" />

<ul>
    @foreach (var customer in Matches())
    {
        <li>@customer.Name</li>
    }
</ul>

@code {
    private QueryValue<string> _search = default!;

    protected override void OnInitialized()
    {
        // The default value ("") is left out of the URL. RendererInfo says whether this render can navigate.
        _search = Query.Bind("q", QueryCodecs.String, "", RendererInfo);
        _search.Changed += OnSearchChanged;
    }

    private void OnInput(ChangeEventArgs e) => _search.Value = e.Value?.ToString() ?? "";

    // Changed is raised on the thread that changed the URL, so go through InvokeAsync before rendering.
    private void OnSearchChanged() => _ = InvokeAsync(StateHasChanged);

    private IEnumerable<Customer> Matches() =>
        SampleData.Customers.Where(c => c.Name.Contains(_search.Value, StringComparison.OrdinalIgnoreCase));

    public void Dispose()
    {
        _search.Changed -= OnSearchChanged;
        _search.Dispose();
    }
}
```

Render it from a page in an interactive render mode:

```razor
@page "/customers"
@rendermode InteractiveServer

<PageTitle>Customers</PageTitle>

<CustomerSearch />
```

Open `/customers?q=dan` and the list shows one customer. Type in the box and the address becomes `/customers?q=alice` after the event handler yields; clear the box and the key disappears, because an empty string is the default. Follow a link to `/customers?q=bob` and the list updates in place, because the component follows the address and not only its own writes.

Three details of that component are worth knowing:

- `RendererInfo` matters because prerendering and static server rendering cannot navigate. If it says the renderer is not interactive, writes change nothing. See [Platforms](platforms.md).
- `Changed` is raised on the thread that changed the URL, so the component calls `InvokeAsync(StateHasChanged)` before rendering.
- The component disposes the binding. A `QueryValue` holds a subscription to the URL until it is disposed.

## Next steps

- [Guides](guides.md): schemas in depth, an API endpoint, EF Core, cursor paging, custom codecs, time zones and tests.
- [Concepts](concepts.md): how schema, request, state and result relate, and how `QueryState` follows the URL.
- [Platforms](platforms.md): render modes, .NET MAUI and NativeAOT.
- [API reference](api-reference.md): every public type and member.
