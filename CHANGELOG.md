# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- A documentation site at https://klyroo.github.io/klyroo-query/, built from `README.md` and `docs/` and published by `.github/workflows/docs.yml` (the build is `docs-site/` of klyroo-stack). `docs/site.yml` sets the title, tagline, install command, NuGet packages, order of the pages and accent color; the Markdown files themselves are read as they are.
- `QueryState.Snapshot(prefix)`, `DataListSchema<T>.FieldInfos` (with the `DataListFieldInfo` class, read-only and built by the schema), `DataListSchema.DefaultFieldKey`, and `DataListStateQuery.FilterParameterName`, `OperatorName`, `EscapeValue` and `IncludeEmptyToken`: the API Klyroo.Table needs, documented, so a list component of your own can be built on the query layer. `DataListSchema<T>.FormatField`, `TryReadNumber`, `CompareField` and `LocalInstants` are public for the Klyroo packages only and hidden from IntelliSense. `FilterRuleText` stays internal.

### Changed

- **Breaking:** `DataListSchema<T>.ReadField`, which returned a boxed `object?`, is replaced by `FormatField(item, key, format, provider)` (the text of a field, null-safe, collections joined by commas), `TryReadNumber(item, key, out decimal)` (a numeric field for totals and averages) and `CompareField(first, second, key)` (the sort order of two rows by a field). `DataListFieldInfo.HasStructuredValue` tells whether a field holds numbers, dates, times, GUIDs or booleans, and `HasStructuredValueFor(item, key)` answers for one row (a field declared `object` is decided by the type of its value).
- **Breaking:** `DataListRow<T>.SortValues` and `Key`, and `DataListFacetBucket.Value`, are internal: only `ToResult` reads them to build the cursor and the facet, so the boxed values no longer appear in the public API. Load the rows and buckets as before and pass them to `ToResult`.

- The packages no longer grant `InternalsVisibleTo` to Klyroo.Table, Klyroo.Table.Core or their tests: a sibling package uses only public API.

- The documentation of `DataListFilter.Persist` and `WithPersist` says what reads them: a list component such as `KlyDataList` of Klyroo.Table, which keeps the value in `localStorage` in the URL form of `DataListStateQuery`. The executors and the URL codecs still ignore the flag.

## [0.1.0-preview.1] - 2026-11-24

### Added

- A typed `DataListSchema<T>` built from lambda expressions: sortable fields, filters of five kinds (text, option, boolean, number, date), search fields, collection fields, query-only filters, defaults and limits.
- A request model, `DataListRequest`, with a filter tree of rules and AND/OR groups over 13 operators, sorting, offset or keyset cursor paging, and facet counts, plus `DataListResult<T>` and a source-generated JSON context for the wire records.
- `ExecuteInMemory` and `InMemoryProvider` to run a request over a collection, with a stable multi-key sort, search, paging, totals and facets.
- `Apply`, `FacetQueries` and `ToResult` to turn a request into `IQueryable` operators for EF Core or another provider, and `EfAsyncQueryExecutor` in `Klyroo.Query.EntityFrameworkCore` to run them.
- `DataListSchema.Sanitize`, which drops unknown keys, disallowed operators, malformed values and oversized pages from a request before it is used.
- `DataListStateQuery` and `QueryCodecs` to write a list state as URL query keys and read it back without throwing, with codecs for numbers, dates, enums, lists and ranges.
- `QueryState`, `QueryValue<T>` and `QueryGroup<TRecord>` for Blazor, registered with `AddKlyrooQuery`: typed query keys with history control, batched writes, key ownership and links for static server rendering.
- `Klyroo.Query.Core` is marked trimmable and AOT compatible and has no UI dependency.

[Unreleased]: https://github.com/klyroo/klyroo-query/compare/v0.1.0-preview.1...HEAD
[0.1.0-preview.1]: https://github.com/klyroo/klyroo-query/releases/tag/v0.1.0-preview.1
