# Architecture — Klyroo.Query

Klyroo.Query follows the strict headless two-layer architecture shared across every repository in **Klyroo**:

```text
+-------------------------------------------------------------------+
|  Klyroo.Query                                                       |
|  Blazor Web (SSR / Server / WASM / Auto) & .NET MAUI Hybrid       |
|  SupportedPlatform: browser, ios, android, maccatalyst, windows   |
+-------------------------------------------------------------------+
                                |
                                v
+-------------------------------------------------------------------+
|  Klyroo.Query.Core                                                  |
|  100% Headless C# · Zero UI Dependency · IsAotCompatible=true     |
|  Works in APIs, Workers, CLI, NativeAOT & Native C# MAUI          |
+-------------------------------------------------------------------+
```

## Design Principles

1. **Zero UI Coupling in `Klyroo.Query.Core`** — State machines, validation rules, ASTs, formatters, and parsers live in `Klyroo.Query.Core` so they can be unit tested in milliseconds with plain `xUnit` without spinning up a renderer.
2. **NativeAOT & Trimming Verified** — `Klyroo.Query.Core` sets `<IsAotCompatible>true</IsAotCompatible>` and `<IsTrimmable>true</IsTrimmable>` with `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
3. **Every Render Mode & Native Hybrid** — `Klyroo.Query` avoids browser-only assumptions during prerendering and declares `<SupportedPlatform>` for `browser`, `ios`, `android`, `maccatalyst`, and `windows`.
4. **Composable with `Klyroo.Stack`** — Install `Klyroo.Query` standalone or aggregate the entire ecosystem with `dotnet add package Klyroo.Stack` and `services.AddKlyrooStack()`.

## Supported Platforms

- Headless C# / NativeAOT
- .NET MAUI Native C#
- Blazor Server / WASM / Auto
- .NET MAUI Blazor Hybrid
- Entity Framework Core
