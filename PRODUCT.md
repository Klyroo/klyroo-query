# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

- **Primary**: .NET 10 backend and full-stack developers, Blazor engineers, and MAUI developers building searchable, filterable, paginated data views backed by in-memory collections, `IQueryable`, or Entity Framework Core.

## Product Purpose

`klyroo-query` provides a strongly typed, 100% NativeAOT query engine (`Klyroo.Query.Core`), an optional EF Core async SQL adapter (`Klyroo.Query.EntityFrameworkCore`), and reactive URL-synchronized query state (`Klyroo.Query`) for Blazor Web and `.NET MAUI Blazor Hybrid`.

Success means a team declares a `DataListSchema<T>` once and gets compile-time-safe filtering, sorting, faceting, keyset/offset paging, reversible URL codecs, and identical execution across in-memory datasets, LINQ, and SQL without rewriting query plumbing.

## Positioning

Unlike UI-coupled grid libraries that bury filter parsing inside Razor components or rely on reflection-heavy dynamic LINQ, `Klyroo.Query.Core` is a standalone, zero-UI, NativeAOT-safe typed query AST and URL codec engine that runs identically in Minimal APIs, CLI workers, MAUI ViewModels, EF Core SQL queries, and Blazor UIs.

## Operating Context

- **Packages**: `Klyroo.Query.Core` (headless NativeAOT engine), `Klyroo.Query.EntityFrameworkCore` (async SQL execution via `EfAsyncQueryExecutor`), and `Klyroo.Query` (Blazor & MAUI Hybrid `QueryState` + URL navigation integration).
- **Upstream/Downstream**: Foundation for `Klyroo.Table` (`KlyDataList`) and shareable across API servers and web/MAUI clients.

## Capabilities and Constraints

- `<IsAotCompatible>true</IsAotCompatible>` and `<IsTrimmable>true</IsTrimmable>` in `Klyroo.Query.Core` with zero UI dependencies.
- Supports composable filter AST (`DataListFilterGroup`, `FilterRule`, `FilterOperator`), sorting, facets, offset and keyset cursor paging, and reversible URL query serialization (`DataListStateQuery`).
- Works across Blazor Static SSR (link-based URL state), Interactive Server, WebAssembly, Auto, and `.NET MAUI Blazor Hybrid`.
- 100% MIT open-source license; strict `Klyroo.Query*` naming.

## Brand Commitments

- **Name**: `Klyroo.Query` (`klyroo-query`), part of the **Klyroo** ecosystem.
- **Voice**: Technical, exact, and architecture-first.

## Evidence on Hand

- Passing unit tests (`tests/Klyroo.Query.Tests`) covering schema evaluation, filter AST, URL codecs, keyset paging, and EF Core SQLite execution, plus NativeAOT publish verification in CI.
- Measured `.NET 10.0` Release benchmark (in `klyroo-stack/docs/benchmarks.md`): 100k rows with filter + sort + 5 facets executes in `16.6 ms` median via `ExecuteInMemory`.
- Live URL query synchronization demonstrated in `Klyroo.Stack.Site` at `/showcase`.
- **Explicit absences (do not fabricate)**: No NuGet download stats, customer logos, or third-party production case studies exist yet.

## Product Principles

1. **Declare schema once, run everywhere**: One `DataListSchema<T>` drives URL codecs, in-memory execution, `IQueryable`, and EF Core SQL.
2. **NativeAOT without compromise**: Zero reflection hacks in `Klyroo.Query.Core`; full trimming and AOT compatibility verified in CI.
3. **URL as durable state**: Filter, sort, search, and page state serialize cleanly and reversibly into query strings for deep linking and Static SSR.
4. **Decoupled from presentation**: Query evaluation has no dependency on tables, grids, or any UI framework.

## Accessibility & Inclusion

- Durable standard: **WCAG 2.1 AA** for any query-driven UI controls and URL-link pagination/filter surfaces built on `Klyroo.Query` (predictable focus and keyboard-accessible filter/paging links in Static SSR and interactive modes).
