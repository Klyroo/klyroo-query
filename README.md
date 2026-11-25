# Klyroo.Query

A typed query model for lists of data: search, filter, sort, page and facet requests, their URL form, and their execution over in-memory collections and Entity Framework Core, for .NET developers who build data lists in Blazor, .NET MAUI Blazor Hybrid or an API.

[![CI](https://github.com/klyroo/klyroo-query/actions/workflows/ci.yml/badge.svg)](https://github.com/klyroo/klyroo-query/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/vpre/Klyroo.Query.svg)](https://www.nuget.org/packages/Klyroo.Query)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

You describe a row type once, with a schema built from lambda expressions. A request (filter tree, sort, search, paging, facets) is plain data that serializes to JSON, runs against an in-memory list or an `IQueryable` with the same operators, and has a URL form that survives a round trip. The engine, `Klyroo.Query.Core`, has no UI or ASP.NET Core dependency and runs wherever .NET 10 runs; `Klyroo.Query` adds `QueryState`, which keeps Blazor components in sync with the query string.

## Features

- **Typed schema**: declare sortable fields, filters, search fields, collection fields and defaults with expressions such as `c => c.Country!.Name`, so there are no field names to mistype and nothing reflects over strings.
- **Filter tree**: five filter kinds (text, option, boolean, number, date), 13 operators, nested AND/OR groups, and an include-empty flag on a rule.
- **One request, several backends**: `ExecuteInMemory` runs a request over a collection, and `Apply` turns the same request into `IQueryable` operators that EF Core translates to SQL.
- **Paging and facets**: offset paging, keyset cursor paging with a stable sort (nulls last, row key as the tie-break), and option counts per filter.
- **Request validation**: `Sanitize` drops unknown keys, operators the filter does not allow, malformed values and oversized pages before a request reaches your data.
- **URL codecs**: `DataListStateQuery` writes a list state as query keys (`q`, `p`, `s`, `f.status`) and reads it back without throwing, and `QueryCodecs` converts single values.
- **URL state for Blazor**: `QueryState` binds query keys to typed values and records, with history control and links that work in static server rendering.
- **Trim-safe core**: `Klyroo.Query.Core` is marked trimmable and AOT compatible, and ships a source-generated JSON context for the wire records.

## Packages

| Package | Description |
| :--- | :--- |
| [`Klyroo.Query.Core`](https://www.nuget.org/packages/Klyroo.Query.Core) | Schema, request, filter and result model, URL codecs, in-memory executor and `IQueryable` applier. No UI dependency. |
| [`Klyroo.Query`](https://www.nuget.org/packages/Klyroo.Query) | `QueryState`, `QueryValue<T>` and `QueryGroup<TRecord>` for Blazor, and the `AddKlyrooQuery` registration. Depends on `Klyroo.Query.Core`. |
| [`Klyroo.Query.EntityFrameworkCore`](https://www.nuget.org/packages/Klyroo.Query.EntityFrameworkCore) | `EfAsyncQueryExecutor`, which runs the queries built by `Klyroo.Query.Core` with the async operators of EF Core. |

## Installation

```bash
dotnet add package Klyroo.Query.Core --prerelease                  # the engine, for any .NET 10 project
dotnet add package Klyroo.Query --prerelease                       # adds QueryState for Blazor (brings Klyroo.Query.Core)
dotnet add package Klyroo.Query.EntityFrameworkCore --prerelease   # optional, for EF Core
```

In a Blazor app, register the service and import the namespace. There is no stylesheet to add.

```csharp
using Klyroo.Query;

builder.Services.AddKlyrooQuery();
```

```razor
@using Klyroo.Query
```

## Quick start

Headless first. This runs in a console app, a worker or an API, with only `Klyroo.Query.Core` installed.

```csharp
using Klyroo.Query;

var schema = DataListSchema.For<Customer>(s => s
    .RowKey(c => c.Id)
    .Search(c => c.Name, c => c.Email)
    .Field(c => c.Name, f => f.Sortable())
    .Field(c => c.Age, f => f.Sortable().Filter(DataListFilter.Number(min: 0)))
    .Field(c => c.Status, f => f.Filter(DataListFilter.Option()))
    .DefaultSort(c => c.Name));

var request = new DataListRequest(
    new DataListPaging.Offset(Skip: 0, Take: 25),
    [new DataListSort("age", Descending: true)],
    DataListFilterGroup.And(new FilterRule("status", FilterOperator.Is, new FilterValue.Text("Active"))),
    new DataListSearch("a"));

DataListResult<Customer> page = schema.ExecuteInMemory(customers, request);
// page.TotalCount == 2, page.Items[0].Name == "Alice Martin"
```

`customers` is any `IReadOnlyList<Customer>`, where `Customer` is a record with `Id`, `Name`, `Email`, `Status` (an enum) and `Age`.

Then Blazor. This component binds the `q` key of the URL to a search box. A link to `/customers?q=dan`, the Back button and the address bar all change what it shows, and an empty search leaves no key in the URL. It needs an interactive render mode.

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

## Documentation

| | |
| :--- | :--- |
| [Getting started](docs/getting-started.md) | Step-by-step first integration |
| [Guides](docs/guides.md) | Task-oriented recipes |
| [Concepts](docs/concepts.md) | How the library works |
| [Platforms](docs/platforms.md) | Render modes, .NET MAUI, NativeAOT |
| [API reference](docs/api-reference.md) | Every public type and member |
| [FAQ](docs/faq.md) | Troubleshooting and limitations |

## Requirements

- .NET 10 SDK. All three packages target `net10.0`.
- `Klyroo.Query.Core` has no platform restriction. It is marked `IsAotCompatible` and `IsTrimmable`, and a console app using the in-memory executor, the URL codecs and the JSON context was published and run with NativeAOT on linux-x64.
- `Klyroo.Query` works in Blazor Static SSR, Interactive Server, WebAssembly and Auto. Static SSR cannot navigate, so writes are ignored there and links (`HrefWith`) carry the state instead. It declares `browser`, `ios`, `android`, `maccatalyst` and `windows` as supported platforms for .NET MAUI Blazor Hybrid; this repository does not build or test a MAUI app.
- `Klyroo.Query.EntityFrameworkCore` depends on `Microsoft.EntityFrameworkCore` 10.0.15 or later. The tests of this repository run the `IQueryable` applier on SQLite.

## Klyroo ecosystem

Klyroo is a family of independent .NET libraries that compose. Each one has its own repository, versions and releases.

| Library | Description |
| :--- | :--- |
| [Klyroo.UI](https://github.com/klyroo/klyroo-ui) | Accessible Blazor and .NET MAUI Hybrid components styled with Tailwind CSS v4 |
| [**Klyroo.Query**](https://github.com/klyroo/klyroo-query) | Typed query model, URL codecs and IQueryable / EF Core execution |
| [Klyroo.Table](https://github.com/klyroo/klyroo-table) | Data lists and trees built on Klyroo.Query |
| [Klyroo.Form](https://github.com/klyroo/klyroo-form) | Schema-driven forms, validation and wizards |
| [Klyroo.Dashboard](https://github.com/klyroo/klyroo-dashboard) | Analytical dashboards with filters, comparisons and KPI blocks |
| [Klyroo.Display](https://github.com/klyroo/klyroo-display) | Read-only display blocks, media players and file preview |
| [Klyroo.Spreadsheet](https://github.com/klyroo/klyroo-spreadsheet) | CSV and TSV import with column mapping and validation |
| [Klyroo.Agent](https://github.com/klyroo/klyroo-agent) | Bridge that lets AI agents read and act on a Blazor UI |
| [Klyroo.Stack](https://github.com/klyroo/klyroo-stack) | Meta-package, MCP server and `dotnet new` templates |

## Contributing

Contributions are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request, and follow the
[code of conduct](CODE_OF_CONDUCT.md). Report security issues privately as described in [SECURITY.md](SECURITY.md).

## License

[MIT](LICENSE)
