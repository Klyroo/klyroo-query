# Lux.Query (LuxStack)

> **Headless NativeAOT Query AST, URL codecs, IQueryable applier, EF Core & QueryState**

Strongly typed, 100% NativeAOT query engine: composable filter AST (DataListSchema, DataListRequest, DataListFilterGroup), reversible URL query codecs, InMemory & IQueryable evaluators, Entity Framework Core async SQL executor (EfAsyncQueryExecutor), and reactive QueryState for Blazor and .NET MAUI Hybrid.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](./LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![NativeAOT](https://img.shields.io/badge/Core-NativeAOT%20Ready-10b981.svg)](./docs/maui-and-aot.md)
[![MAUI Hybrid](https://img.shields.io/badge/.NET%20MAUI-Blazor%20Hybrid-8b5cf6.svg)](./docs/maui-and-aot.md)

---

## Architecture (TanStack Two-Layer Split)

- **`Lux.Query.Core`** — 100% headless C#, zero UI dependencies, `<IsAotCompatible>true</IsAotCompatible>`, `<IsTrimmable>true</IsTrimmable>`. Runs in Minimal APIs, background workers, CLI apps, and native C# .NET MAUI ViewModels.
- **`Lux.Query`** — Multiplatform Blazor Web (Static SSR, Interactive Server, WebAssembly, Auto) & `.NET MAUI Blazor Hybrid` (`browser`, `ios`, `android`, `maccatalyst`, `windows`).
- **`Lux.Query.EntityFrameworkCore`** — Optional Entity Framework Core async SQL execution adapter.

## Highlights

- **Typed Schema & Filter AST** (`Typed AST`) — Declare searchable, sortable, and filterable fields with DataListSchema.For<T>() with zero magic strings and full compile-time safety.
- **InMemory, IQueryable & EF Core** (`Any Backend`) — Run the exact same DataListRequest in memory (ExecuteInMemory), over LINQ IQueryable, or translated to async SQL via EfAsyncQueryExecutor.
- **URL Query Codecs & QueryState** (`URL Sync`) — Reversible serialization of filters, sorts, and paging into the query string with browser history control and mobile WebView support.
- **100% Headless & Zero UI in .Core** (`NativeAOT`) — Lux.Query.Core has zero UI dependencies: compile it in NativeAOT Minimal APIs, background workers, or native .NET MAUI ViewModels.

## Installation

```bash
dotnet add package Lux.Query.Core   # Headless C# / NativeAOT / MAUI C#
dotnet add package Lux.Query        # Blazor Web & .NET MAUI Blazor Hybrid
dotnet add package Lux.Query.EntityFrameworkCore   # Entity Framework Core async SQL executor
```

Register services in `Program.cs` (Blazor Web App) or `MauiProgram.cs` (.NET MAUI Blazor Hybrid):

```csharp
builder.Services.AddLuxQuery();
```

## Quick Start

### Headless Core (`Lux.Query.Core`)

```csharp
using Blux.UI.Tools.Query;

var schema = DataListSchema.For<Product>(s => s
    .RowKey(p => p.Id)
    .Search(p => p.Name)
    .Field(p => p.Price, f => f.Key("price").Sortable().Filterable()));

var request = new DataListRequest(
    new DataListPaging.Offset(0, 25),
    [new DataListSort("price", Descending: true)],
    DataListFilterGroup.Empty,
    new DataListSearch("Monitor"));

var result = schema.ExecuteInMemory(products, request);
```

### Blazor Web & .NET MAUI Hybrid (`Lux.Query`)

```razor
@inject QueryState Query

@code {
    // Two-way synchronized URL parameter (?page=3&sort=price:desc)
    private void GoToPage(int page) => Query.Set("page", page.ToString(), QueryHistory.Push);
    private string CurrentPage => Query.Get("page") ?? "1";
}
```

## Documentation

| Guide | Description |
| :--- | :--- |
| [Getting Started](./docs/getting-started.md) | Installation, DI registration, stylesheets, and first working example |
| [Architecture](./docs/architecture.md) | Headless `.Core` vs UI split, NativeAOT guarantees, and platform matrix |
| [API Reference](./docs/api-reference.md) | Exhaustive reference of every public class, enum, property, method, and Razor parameter |
| [MAUI & NativeAOT](./docs/maui-and-aot.md) | `.NET MAUI Blazor Hybrid` (`iOS`, `Android`, `macOS`, `Windows`) and NativeAOT deployment |
| [Recipes & Testing](./docs/recipes.md) | End-to-end production patterns and `xUnit` / `bUnit` testing recipes |
| [AI Agent Skill](./skills/lux-query/SKILL.md) | Machine-readable guidance for coding agents |

## Key Public Types

| Type | Kind | Package | Summary |
| :--- | :--- | :--- | :--- |
| `IQueryCodec<T>` | `interface` | `Lux.Query.Core` | Converts a value to and from the text of a URL query parameter. The same codecs serve the components (URL-synced state) and an API server that parses the URLs they produce, which is why they live in this Blazor-free package. The type of the value. |
| `QueryCodecs` | `class` | `Lux.Query.Core` | The built-in `IQueryCodec{T}`s. Numbers and dates use the invariant culture, so a URL means the same thing on every machine. A codec never throws on input: text that is not a value makes `TryParse` return false. |
| `QueryRange<T>` | `record` | `Lux.Query.Core` | A range of values with either end optional, as `Range{T}` reads and writes it. The lower end, or null for no lower bound. The upper end, or null for no upper bound. |
| `InMemoryDataList` | `class` | `Lux.Query.Core` | Runs a `DataListRequest` against an in-memory list: search, filter tree, stable multi-key sort, offset or cursor paging, total count and facets. The predicate is built as an expression (see the internal `FilterExpressions`) and compiled, so the IQueryable applier can reuse the same definition of every operator. |
| `DataListJsonContext` | `class` | `Lux.Query.Core` | Public API type |
| `DataListPaging` | `record` | `Lux.Query.Core` | Public API type |
| `Offset` | `record` | `Lux.Query.Core` | Skip a number of rows, then take a page. Rows to skip. Page size. |
| `Cursor` | `record` | `Lux.Query.Core` | Keyset paging: the page after an opaque cursor. The cursor of the previous page (`NextCursor`), or `null` for the first page. Page size. Also compute `TotalCount`. |
| `None` | `record` | `Lux.Query.Core` | No paging: the whole result. |
| `DataListSort` | `record` | `Lux.Query.Core` | One sort rule. The key of a sortable field. Sort from the largest to the smallest. Nulls always sort last. |
| `DataListSearch` | `record` | `Lux.Query.Core` | The search box: a substring searched, ignoring case, in every search field of the schema. The text; blank matches everything. |
| `DataListFacetRequest` | `record` | `Lux.Query.Core` | Asks for the option counts of one filter. The key of a field with an option or boolean filter. Whether the facet's own filter applies. Maximum options returned; `null` for all. Options to skip (for loading more). Keeps only the options whose value contains this text, ignoring case. Also count the rows where the field is empty. Counts only these values (as text, the way a rule carries them), whatever `MaxFacetBuckets` says: a remote option filter asks for the values of the pages it has loaded. A value that no row has is not in the result (a reader takes it as 0). `null` or empty counts every value. |
| `DataListRequest` | `record` | `Lux.Query.Core` | What a data list asks a provider for: a page of rows under a sort, a filter tree and a search, plus facet counts. |
| `DataListFacetOption` | `record` | `Lux.Query.Core` | One option of a facet and the number of rows that have it. The value as text: invariant culture, `true`/`false`, enum name, ISO dates. The row count. |
| `DataListFacetResult` | `record` | `Lux.Query.Core` | The counts of one facet. |
| `DataListResult<T>` | `record` | `Lux.Query.Core` | A page of rows and what goes with it. The row type. The rows of the page. Rows matching the filter and search across all pages; `null` when not computed. The cursor of the next page, or `null` after the last one (cursor paging only). The requested facet counts. |
| `DataListState` | `record` | `Lux.Query.Core` | The state of a data list as the user left it: search, filters, sort, page, page size, layout and, for cursor paging, the cursor. Immutable. `DataListStateQuery` converts it to and from URL query keys. |
| `FilterOperator` | `enum` | `Lux.Query.Core` | Public API type |

## Building & Testing

```bash
dotnet build
dotnet test
dotnet pack -c Release
```

## License

MIT — see [LICENSE](./LICENSE). Part of the **[LuxStack](../lux.stack/README.md)** ecosystem.
