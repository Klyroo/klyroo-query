# Lux.Query (LuxStack)

TanStack Query & Router Search Params equivalent for .NET, Blazor, and .NET MAUI:
- **`Lux.Query.Core`**: 100% Headless C# / NativeAOT query AST, URL codecs, `IQueryable` applier, and in-memory executor (zero UI dependency).
- **`Lux.Query`**: Scoped Blazor & MAUI Hybrid URL `QueryState` synchronized with `NavigationManager` (`AddLuxQuery()`).
- **`Lux.Query.EntityFrameworkCore`**: Async Entity Framework Core executor for `Lux.Query`.
