# Platforms

What runs where, and what each environment requires. Everything on this page was checked against the project files or by running code; where something is not covered by a test in this repository, the page says so.

## At a glance

| Package | Runs on | Trimming | NativeAOT |
| :--- | :--- | :--- | :--- |
| `Klyroo.Query.Core` | Any .NET 10 project | `IsTrimmable` | `IsAotCompatible`; the in-memory executor is verified (see below) |
| `Klyroo.Query` | Blazor Static SSR, Interactive Server, WebAssembly, Auto; .NET MAUI Blazor Hybrid | `IsTrimmable` | Not marked `IsAotCompatible` |
| `Klyroo.Query.EntityFrameworkCore` | Wherever EF Core runs | Not marked | Not marked |

All three packages target `net10.0`. `Klyroo.Query` declares `browser`, `ios`, `android`, `maccatalyst` and `windows` as supported platforms; the other two declare none.

## Blazor render modes

`QueryState` is a scoped service, registered by `AddKlyrooQuery()`: one per Blazor Server circuit, one per WebAssembly app. Reading the URL works in every mode. Writing depends on whether the renderer can navigate.

| Render mode | Reads | Writes |
| :--- | :--- | :--- |
| Static SSR and prerendering | Yes, from the request URL | Ignored: `Set` returns `false`. Offer links with `HrefWith`. |
| Interactive Server | Yes | Yes, once the component knows its renderer is interactive |
| WebAssembly | Yes | Yes |
| Auto | Yes | Yes, once the component is interactive. With prerendering, its first render is static. |

**Why static rendering ignores writes.** Nothing can navigate during prerendering or static rendering. A navigation manager configured not to throw (`BlazorDisableThrowNavigationException`) turns the attempt into a real HTTP redirect. So `QueryState` sends a write to the navigation manager only when it knows the renderer is interactive.

**How it knows.** `QueryState.Bind` and `QueryState.Group` take the `RendererInfo` of the component and call `UseRenderer` with it. Pass the `RendererInfo` property of the component, which is `IsInteractive` only after the component became interactive. A component that prerenders and then becomes interactive therefore writes nothing during prerender and writes normally afterwards.

**The raw `Set` and `SetMany`.** They need the same knowledge. Call `UseRenderer(RendererInfo)` first, from `OnInitialized`. Without it they throw `InvalidOperationException` everywhere except in the browser (WebAssembly), on Android, iOS and Mac Catalyst, where the renderer is always interactive. In particular, an Interactive Server circuit and a Windows app must call `UseRenderer`, or use `Bind` and `Group`, which do.

**Links in static rendering.** `QueryState.HrefWith`, `QueryValue<T>.HrefWith` and `QueryGroup<TRecord>.HrefWith` return the URL of the current page with changes applied, and keep parameters the service did not write. A statically rendered filter form can use a GET form, because `DataListStateQuery.FromQuery` reads repeated keys and the `from` and `to` keys of a range on the server. See [Link to a state in static server rendering](guides.md#link-to-a-state-in-static-server-rendering).

## .NET MAUI Blazor Hybrid

Register the service in `MauiProgram.cs` with the same call as in a web app, on the `IServiceCollection` of the builder:

```csharp
builder.Services.AddKlyrooQuery();
```

`Klyroo.Query` lists `browser`, `ios`, `android`, `maccatalyst` and `windows` as supported platforms, and `QueryState` treats the browser, Android, iOS and Mac Catalyst as always interactive. This repository builds and tests no MAUI project: the tests run `QueryState` in bUnit, so MAUI itself is not covered here. Check the app on each platform you ship.

A MAUI app that does not use `BlazorWebView` (XAML or C# Markup) can reference only `Klyroo.Query.Core`, which has no ASP.NET Core or Razor dependency, and use the schema, the requests, the in-memory executor and the URL codecs from its view models.

## Trimming

`Klyroo.Query.Core` and `Klyroo.Query` set `IsTrimmable`. In the core, schemas are built from lambda expressions and the wire records have a source-generated JSON context, `DataListJsonContext`, so nothing serializes by reflection. Declare `DataListResult<YourRow>` in a context of your own to serialize results the same way.

The CI of this repository builds `Klyroo.Query.Core` with the AOT and trimming analyzers on and warnings as errors.

## NativeAOT

`Klyroo.Query.Core` sets `IsAotCompatible`. To check a real image, a console app that referenced only `Klyroo.Query.Core` was published with `PublishAot` for linux-x64 (.NET 10) and run. It declared a schema with a collection field and a query-only filter, ran `ExecuteInMemory` with search, filters, facets and cursor paging, round-tripped a state through `DataListStateQuery`, and serialized a request with `DataListJsonContext`. It published without warnings and gave the same results as on CoreCLR. [`examples/AotQueryExample`](../examples/AotQueryExample/Program.cs) is a minimal version of that app; add `<PublishAot>true</PublishAot>` to its project to publish it:

```bash
dotnet publish -c Release -r linux-x64
```

What is not covered:

- **`Apply` and `FacetQueries`.** They build generic `Queryable` calls over the field types of your lambdas at run time. The code suppresses the AOT and trimming warnings for those calls on the grounds that the instantiations already exist for your own expressions, and it documents them as needing a runtime that can create generic instantiations (CoreCLR or Mono). No test of this repository runs them under NativeAOT, so treat the `IQueryable` applier as a CoreCLR and Mono feature.
- **`Klyroo.Query`.** The package is trimmable but not marked AOT compatible and has no AOT test.
- **`Klyroo.Query.EntityFrameworkCore`.** It is two wrappers over EF Core, and what EF Core supports under NativeAOT is a question for EF Core.
- **Other operating systems.** The NativeAOT run above was on linux-x64 only.
