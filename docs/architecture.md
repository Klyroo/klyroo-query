# Architecture — Lux.Query

Lux.Query follows the strict TanStack two-layer architecture shared across every repository in **LuxStack**:

```text
+-------------------------------------------------------------------+
|  Lux.Query                                                       |
|  Blazor Web (SSR / Server / WASM / Auto) & .NET MAUI Hybrid       |
|  SupportedPlatform: browser, ios, android, maccatalyst, windows   |
+-------------------------------------------------------------------+
                                |
                                v
+-------------------------------------------------------------------+
|  Lux.Query.Core                                                  |
|  100% Headless C# · Zero UI Dependency · IsAotCompatible=true     |
|  Works in APIs, Workers, CLI, NativeAOT & Native C# MAUI          |
+-------------------------------------------------------------------+
```

## Design Principles

1. **Zero UI Coupling in `Lux.Query.Core`** — State machines, validation rules, ASTs, formatters, and parsers live in `Lux.Query.Core` so they can be unit tested in milliseconds with plain `xUnit` without spinning up a renderer.
2. **NativeAOT & Trimming Verified** — `Lux.Query.Core` sets `<IsAotCompatible>true</IsAotCompatible>` and `<IsTrimmable>true</IsTrimmable>` with `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
3. **Every Render Mode & Native Hybrid** — `Lux.Query` avoids browser-only assumptions during prerendering and declares `<SupportedPlatform>` for `browser`, `ios`, `android`, `maccatalyst`, and `windows`.
4. **Composable with `Lux.Stack`** — Install `Lux.Query` standalone or aggregate the entire ecosystem with `dotnet add package Lux.Stack` and `services.AddLuxStack()`.

## Supported Platforms

- Headless C# / NativeAOT
- .NET MAUI Native C#
- Blazor Server / WASM / Auto
- .NET MAUI Blazor Hybrid
- Entity Framework Core
