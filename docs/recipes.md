# Recipes & Testing — Lux.Query

## Recipe 1: Entity Framework Core Async SQL Translation

```csharp
// Program.cs (Server)
builder.Services.AddLuxQuery();
builder.Services.AddSingleton<IDataListQueryExecutor, EfAsyncQueryExecutor>();

// In your endpoint or service:
var filtered = schema.Apply(db.Products.AsNoTracking(), request);
```

## Recipe 2: Headless Unit Testing (`xUnit`)

Test all business logic in `Lux.Query.Core` without spinning up a browser or Blazor renderer:

```csharp
using Xunit;

public sealed class QueryUnitTests
{
    [Fact]
    public void Headless_core_runs_without_ui_host()
    {
        // Arrange & Act using Lux.Query.Core directly
        // Assert deterministic outputs with zero JSInterop setup
    }
}
```

## Recipe 3: Component Testing (`bUnit`)

```csharp
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class QueryComponentTests : BunitContext
{
    public QueryComponentTests()
    {
        Services.AddLuxQuery();
    }
}
```

> **Tip**: Always register LuxStack services on `Services` **before** calling `SetRendererInfo(...)` in bUnit tests.
