# FAQ

Troubleshooting, known limitations and frequent questions. Each troubleshooting entry gives the symptom, the cause in the code and the fix.

## Troubleshooting

### `QueryState does not know whether the renderer is interactive`

**Symptom.** `Set` or `SetMany` throws `InvalidOperationException`: "QueryState does not know whether the renderer is interactive, and writing from a prerender or a static render would redirect the request."

**Cause.** Only an interactive renderer can navigate, and the raw `Set` and `SetMany` have not been told whether this one is. They assume nothing except in the browser (WebAssembly), on Android, iOS and Mac Catalyst.

**Fix.** Call `Query.UseRenderer(RendererInfo)` from `OnInitialized` before writing, or write through `Bind` and `Group`, which take the `RendererInfo` of the component. See [Platforms](platforms.md#blazor-render-modes).

### A write does nothing

**Symptom.** `Query.Set(...)` returns `false`, or setting `QueryValue<T>.Value` does not change the URL.

**Cause.** The renderer is not interactive: the component is prerendering or rendering statically, where a navigation would be a redirect. `QueryState` ignores writes there on purpose.

**Fix.** Render the component in an interactive render mode, or offer the change as a link with `HrefWith`. See [Link to a state in static server rendering](guides.md#link-to-a-state-in-static-server-rendering).

### A write is lost when the handler also navigates

**Symptom.** A handler calls `Query.Set(...)` and then `Navigation.NavigateTo("/details/5")`, and the query key never appears.

**Cause.** Writes wait until the handler yields, then go out as one navigation. A navigation to another page started in the meantime drops the pending writes, because they belonged to the page being left; flushing them afterwards would send the user back.

**Fix.** Put the query in the URL you navigate to (`Navigation.NavigateTo("/details/5?tab=notes")`) instead of writing and then navigating. A write is for the page that is showing.

### `The prefix '...' of the query string is already claimed by '...'`

**Symptom.** Creating a group or claiming a prefix throws `InvalidOperationException` naming two owners, or, in a component that survived a page change, `ThrowIfConflicted` throws on first use.

**Cause.** Two components use the same query key or prefix on one page. A `Claim` owns a prefix (and every key under `prefix.`); a `QueryGroup` owns the keys you `Add`. Two claims conflict when they overlap. A claim that meets one made before the last change of page is judged after the render and the disposal of the previous page, so a layout or a page reused for another route parameter does not throw while the old page is still disposing.

**Fix.** Give one of the components another prefix or key. If you hold the claim in a component, dispose it with the component so the next page can reuse the prefix.

### The URL contains `f.archived=` or another empty filter key

**Symptom.** A state written with `DataListStateQuery.ToQuery` carries `f.<key>=` with an empty value.

**Cause.** The schema declares a default rule for that filter (`DataListFilter.Boolean(false)`, `WithDefault`), and the state you wrote has no rule for the key. An absent default rule means the user removed it, and that is written as an empty value so it reads back as removed.

**Fix.** Start from `schema.DefaultState`, not `DataListState.Initial`, and keep `schema.DefaultState.Filters` when you replace the list of filters.

### A filter in the URL is ignored

**Symptom.** `FromQuery` returns a state without a rule that is in the URL.

**Cause.** Reading drops what the schema does not accept, without throwing: an unknown key, an operator the filter does not allow (`f.status.gt=3` on an option filter), a value that does not parse for the kind, or a value of the wrong shape (an empty text, a range with neither end). `Sanitize` applies the same rules to a request.

**Fix.** Check the key against `schema.FilterKeys`, the operator against `filter.AllowedOperators` (`schema.TryGetFilter`), and the value format against [Read and write the list state as a URL](guides.md#read-and-write-the-list-state-as-a-url). To accept `isEmpty` on an option filter, declare it with `WithOperators`.

### Dates are off by one day

**Symptom.** A date filter returns the rows of the previous or next day for users away from UTC.

**Cause.** `DataListRequest.TimeZone` is `null`, so a day means a UTC day.

**Fix.** Send the IANA id of the user's zone with each request (`state.ToRequest(timeZone: "Europe/Paris")`), and pass it to `DataListStateQuery.FromQuery` when reading a URL on the server. See [Filter dates in the user's time zone](guides.md#filter-dates-in-the-users-time-zone). A zone id the server does not know is cleared and means UTC.

### `Apply and FacetQueries are for a database provider`

**Symptom.** `Apply` or `FacetQueries` throws `InvalidOperationException` on a list you turned into a query with `AsQueryable()`.

**Cause.** The expressions `Apply` builds are written for a database: no null guards and text comparison that follows the database. Run over LINQ to Objects they would throw on a null along a path and compare case-sensitively. The check recognizes the LINQ to Objects provider only; a test double that wraps one is not caught.

**Fix.** Use `schema.ExecuteInMemory(items, request)` for collections.

### `An offset result carries the total count`

**Symptom.** `ToResult` throws for an offset request.

**Cause.** An offset result has a total, and `ToResult` does not run queries: you count `DataListQuery.Matching` and pass the count.

**Fix.** Count `query.Matching` (when `query.WantsTotal`, which is always true for offset and unpaged requests) and pass it as `totalCount`.

### `A cursor is made from the sort values of the last row`

**Symptom.** `ToResult` throws for a cursor request when you pass entities.

**Cause.** The cursor holds the sort values of the last row as the database computed them. A loaded entity may not have them (a sort on `Department.Name` needs no `Include`), so the cursor is built from `DataListQuery.Rows`.

**Fix.** Load `query.Rows` and pass those rows to the `ToResult` overload that takes `DataListRow<T>`. See [Run a request on EF Core](guides.md#run-a-request-on-ef-core).

### Cursor paging throws about a row key or a `Guid`

**Symptom.** `InvalidOperationException`: "Cursor paging needs a row key", or "The row key is a Guid", or "The sort field '...' is a Guid".

**Cause.** A cursor ends every sort with the row key to make the order total, so the schema needs `RowKey`. Databases order Guids in their own way, so a keyset condition on a `Guid` key or sort field cannot be built. A field or key of a type a cursor cannot carry is refused too.

**Fix.** Declare `RowKey(e => e.Id)` with an integer or string key, and sort by another field than a `Guid`.

### The same cursor returns an empty page

**Symptom.** A page loaded with a stored cursor is empty and has no `NextCursor`.

**Cause.** A cursor that does not decode, or that was made for another sort, ends the walk. Restarting at the first page instead would make an infinite scroll loop on the same rows forever. A cursor encodes the sort it was made for, so it cannot be reused after the sort changes.

**Fix.** Drop the cursor when the sort, filter or search changes, and start again from `null`.

### SQLite throws `SQLite does not support expressions of type 'DateTimeOffset' in ORDER BY clauses`

**Symptom.** `Apply` over an EF Core SQLite context fails at run time when a `DateTimeOffset` field is sorted or compared.

**Cause.** The SQLite provider of EF Core cannot order or compare `DateTimeOffset` values. Nothing is evaluated on the client, so the query throws when it runs.

**Fix.** Store the values as UTC ticks with a value converter (see [Run a request on EF Core](guides.md#run-a-request-on-ef-core)). Other providers: check the types of your fields against what the provider can compare.

### A text search finds less on a database than in memory

**Symptom.** Searching `ALICE` finds `Alice` with `ExecuteInMemory` but not through `Apply` on SQLite.

**Cause.** In memory, `contains` and the search ignore case and accents. On a database the applier emits a plain `Contains` and the database decides: the collation of the column, and what the provider translates `Contains` to. On EF Core 10.0.15 with SQLite it is case sensitive.

**Fix.** Use a column type or collation your provider honors for case-insensitive search, or accept that a search is as case-sensitive as your database. Do not compare `ExecuteInMemory` results and database results for text and expect them to match.

### `The key '...' is used by two fields` or `The field is not a plain member access`

**Symptom.** `DataListSchema.For` throws `InvalidOperationException` when the schema is declared.

**Cause.** Keys are the member path in camel case, so `c.Country!.Name` and a second field with the key `country.name` collide, and a field that is not a plain member access (`c => c.Age + 1`, or `t => t` in a collection) has no default key.

**Fix.** Give the field a distinct key with `f.Key("...")`. A key must be non-empty, contain no whitespace, `|`, `=` or `&`, and not start with `-`.

### `The facet '...' is on a collection`

**Symptom.** `FacetQueries` throws for a facet on a collection field.

**Cause.** A facet counts a row once per value, even when several elements carry it, and that needs a row key to count distinct rows.

**Fix.** Declare `RowKey(e => e.Id)` in the schema.

## Known limitations

- **State filters combine with AND.** The URL form (`DataListState.Filters`) holds one rule per key and operator. OR groups exist in a `DataListRequest`, not in the URL.
- **Collection fields cannot be sorted**, and cannot be the default sort.
- **A filter kind needs a matching field type**: text on `string`, boolean on `bool`, number on a numeric type, date on `DateOnly`, `DateTime` or `DateTimeOffset`. Option filters accept strings, enums, numbers, booleans, Guids and dates.
- **Results differ from memory where the database decides.** Text case and accents, the text sort order and some provider limits follow the database. See [Run a request on EF Core](guides.md#run-a-request-on-ef-core).
- **Sorting a nullable or reference-typed column adds a null flag** to the `ORDER BY` so that nulls stay last in both directions. A provider may not use an index on that column for the ordering.
- **A facet keeps at most `MaxFacetBuckets` values** (1000 unless set). The result says `Truncated` when more existed. At the cut, values with equal counts are kept in the order of the database, which can differ from the ordinal order of memory.
- **Offset paging is capped at page 1,000,000** in a URL (`DataListState.MaxPage`), and a page size above `MaxPageSize` is clamped.
- **`QueryState` is for Blazor.** It needs a `NavigationManager`. The schema, requests, codecs and executors work anywhere.
- **The `IQueryable` applier is not tested under NativeAOT.** See [Platforms](platforms.md#nativeaot).
- **No MAUI project is built or tested in this repository.**

## Frequent questions

### Which package do I install?

`Klyroo.Query.Core` for the schema, requests, URL codecs and executors in any .NET 10 project. `Klyroo.Query` when a Blazor app needs `QueryState`; it brings the core. `Klyroo.Query.EntityFrameworkCore` only if you want `EfAsyncQueryExecutor` instead of calling the EF Core operators yourself.

### Is there a stylesheet or a JavaScript file?

No. The packages ship no CSS and no JavaScript. Components that display a list, such as those of Klyroo.Table, are in other packages.

### Do I need EF Core to use `Apply`?

No. `Apply` returns `IQueryable` operators and references no provider. You need a provider to run them, and `Klyroo.Query.EntityFrameworkCore` is a convenience around `CountAsync` and `ToListAsync`.

### Can a client send any filter it likes?

It can send it, and `Sanitize` drops what the schema does not allow: unknown keys, disallowed operators, malformed values, oversized pages and lists, and deep nesting. The client can narrow what you pass in, never widen it, so restrict the rows (authorization, tenant) before applying the request.

### Is the cursor safe to expose?

It is opaque, not signed, and carries no secret or permission. A user who edits one can only move the position inside their own result, since filters and authorization apply again to every request.

### Why does the URL not carry my default filter?

Everything equal to the schema default is left out, so the URL of an untouched list is empty and links stay short. `FromQuery` starts from the defaults, so a missing key means the default applies.

### How do I show several lists on one page?

Give each a key prefix (`DataListStateQuery.ToQuery(state, schema, prefix: "orders")`, and the same prefix in `QueryState.Group`). Keys become `orders.q`, `orders.p` and so on, and a second owner of a prefix throws.
