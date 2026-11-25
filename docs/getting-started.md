# Getting Started with Klyroo.Query

Strongly typed, 100% NativeAOT query engine: composable filter AST (DataListSchema, DataListRequest, DataListFilterGroup), reversible URL query codecs, InMemory & IQueryable evaluators, Entity Framework Core async SQL executor (EfAsyncQueryExecutor), and reactive QueryState for Blazor and .NET MAUI Hybrid.

## 1. Choose Your Package

| Use Case | Package to Install |
| :--- | :--- |
| Headless C#, Minimal APIs, Workers, CLI, or native C# .NET MAUI ViewModels | `Klyroo.Query.Core` |
| Blazor Web App (Server, WebAssembly, Auto, Static SSR) or `.NET MAUI Blazor Hybrid` | `Klyroo.Query` |
| Entity Framework Core async SQL query execution | `Klyroo.Query.EntityFrameworkCore` |

```bash
dotnet add package Klyroo.Query.Core
dotnet add package Klyroo.Query
dotnet add package Klyroo.Query.EntityFrameworkCore   # Entity Framework Core async SQL executor
```

## 2. Register Services

Call `builder.Services.AddKlyrooQuery();` in `Program.cs` (both Server and Client projects when using Blazor Interactive Auto) or `MauiProgram.cs` (after `AddMauiBlazorWebView()`):

```csharp
builder.Services.AddKlyrooQuery();
```

## 3. Add Namespace Imports

In `_Imports.razor`:

```razor
@using Klyroo.UI
@using Klyroo.Query
@using Klyroo.Query
```

## 4. First Example

### Headless C# (`Klyroo.Query.Core`)

```csharp
using Klyroo.Query;

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

### Blazor / MAUI Hybrid (`Klyroo.Query`)

```razor
@inject QueryState Query

@code {
    // Two-way synchronized URL parameter (?page=3&sort=price:desc)
    private void GoToPage(int page) => Query.Set("page", page.ToString(), QueryHistory.Push);
    private string CurrentPage => Query.Get("page") ?? "1";
}
```

## Next Steps

- [Architecture & Two-Layer Design](./architecture.md)
- [Complete API Reference](./api-reference.md)
- [.NET MAUI & NativeAOT Guide](./maui-and-aot.md)
- [Recipes & Testing](./recipes.md)
