# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
