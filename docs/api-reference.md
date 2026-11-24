# API Reference — Lux.Query

Complete public API reference extracted directly from the source code of `Lux.Query.Core` and `Lux.Query`.

## C# Types, Controllers, Models & Enums

### `interface IQueryCodec<T>`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Codecs/IQueryCodec.cs`

Converts a value to and from the text of a URL query parameter. The same codecs serve the components (URL-synced state) and an API server that parses the URLs they produce, which is why they live in this Blazor-free package. The type of the value.

### `class QueryCodecs`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Codecs/QueryCodecs.cs`

The built-in `IQueryCodec{T}`s. Numbers and dates use the invariant culture, so a URL means the same thing on every machine. A codec never throws on input: text that is not a value makes `TryParse` return false.

| Member | Type | Description |
| :--- | :--- | :--- |
| `String` | `IQueryCodec<string>` | Text as is. |
| `Int32` | `IQueryCodec<int>` | A 32-bit integer: an optional sign and digits, nothing else. |
| `Int64` | `IQueryCodec<long>` | A 64-bit integer: an optional sign and digits, nothing else. |
| `Decimal` | `IQueryCodec<decimal>` | A decimal with a dot as the separator ("12.50"); no exponent and no thousands separator. |
| `Double` | `IQueryCodec<double>` | A finite double with a dot as the separator; "NaN" and "Infinity" are not values (`Format` leaves them out). |
| `Boolean` | `IQueryCodec<bool>` | "true" or "false", in any case. |
| `DateOnly` | `IQueryCodec<DateOnly>` | A date as `yyyy-MM-dd`. |
| `DateTimeOffset` | `IQueryCodec<DateTimeOffset>` | A moment in the round-trip format ("O", for example `2026-01-02T03:04:05.0000000+01:00`). A URL that carries the offset unescaped ("+01:00") reaches the server with a space where the plus sign was: that space is read as the plus sign. |
| `Enum<TEnum>(bool lowercase = false)` | `IQueryCodec<TEnum>` | An enum by member name, case-insensitive on input. Numbers and names that are not members are not values. A `[Flags]` enum is written as its names separated by commas (`Read,Write`) and read back the same way; a combination with a bit that has no name is left out of the URL. Writes the names in lower case ("desc"); input is case-insensitive either way. |
| `Array<T>(IQueryCodec<T> item, char separator = ',')` | `IQueryCodec<T[]>` | A list: the items joined by `separator` (a comma). The characters that would be mistaken for structure are written as percent escapes inside an item: the separator, `~` and `%`; an empty item is a lone `~`. An empty list is left out of the URL. The codec of an item. The separator, a character that is not `~` or `%`. |
| `Range<T>(IQueryCodec<T> item)` | `IQueryCodec<QueryRange<T>>` | A range written `from..to`; either end can be missing ("5..", "..9"). A range with no end is left out of the URL. The codec of an end. |
| `Optional<T>(IQueryCodec<T> item)` | `IQueryCodec<T?>` | Makes the codec of a struct a codec of its nullable: a null value is left out of the URL. The codec of the value. |
| `TryParse(string raw, [MaybeNullWhen(false)` | `bool` | — |
| `Format(string value)` | `string?` | — |
| `TryParse(string raw, [MaybeNullWhen(false)` | `bool` | — |
| `Format(T value)` | `string?` | — |
| `TryParse(string raw, out double value)` | `bool` | — |
| `Format(double value)` | `string?` | — |
| `TryParse(string raw, out bool value)` | `bool` | — |
| `Format(bool value)` | `string?` | — |
| `TryParse(string raw, out DateOnly value)` | `bool` | — |
| `Format(DateOnly value)` | `string?` | — |
| `TryParse(string raw, out DateTimeOffset value)` | `bool` | — |
| `Format(DateTimeOffset value)` | `string?` | — |
| `TryParse(string raw, out TEnum value)` | `bool` | — |
| `Format(TEnum value)` | `string?` | — |
| `TryParse(string raw, out T? value)` | `bool` | — |
| `Format(T? value)` | `string?` | — |
| `TryParse(string raw, [MaybeNullWhen(false)` | `bool` | — |
| `Format(T[] value)` | `string?` | — |
| `TryParse(string raw, [MaybeNullWhen(false)` | `bool` | — |
| `Format(QueryRange<T> value)` | `string?` | — |

### `record QueryRange<T>`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Codecs/QueryCodecs.cs`

A range of values with either end optional, as `Range{T}` reads and writes it. The lower end, or null for no lower bound. The upper end, or null for no upper bound.

### `class InMemoryDataList`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/InMemory/InMemoryDataList.cs`

Runs a `DataListRequest` against an in-memory list: search, filter tree, stable multi-key sort, offset or cursor paging, total count and facets. The predicate is built as an expression (see the internal `FilterExpressions`) and compiled, so the IQueryable applier can reuse the same definition of every operator.

| Member | Type | Description |
| :--- | :--- | :--- |
| `ExecuteInMemory<T>(this DataListSchema<T> schema, IEnumerable<T> items, DataListRequest request)` | `DataListResult<T>` | Executes a request. The request is first passed through `Sanitize(DataListRequest)`. Query-only filters are skipped. Rows with a null along a field path (`e.Department.Name` with no department) are read as empty rather than throwing. The row type. The schema of the rows. The rows; enumerated once. What to compute. Cursor paging on a schema without a row key, or a sort field of a type the cursor cannot carry. |
| `InMemoryProvider<T>(this DataListSchema<T> schema, IEnumerable<T> items)` | `DataListProvider<T>` | A `DataListProvider{T}` over a list, for a data list whose rows are in memory. |
| `Create(DataListFacetRequest facet, IEnumerable<KeyValuePair<string, int>> counts, int missing, int maxBuckets)` | `DataListFacetResult` | — |

### `class DataListJsonContext`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/DataListJsonContext.cs`

### `record DataListPaging`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/DataListRequest.cs`

### `record Offset`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/DataListRequest.cs`

Skip a number of rows, then take a page. Rows to skip. Page size.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Number` | `int` | — |
| `ForPage(int page, int pageSize)` | `Offset` | The offset of a 1-based page. |

### `record Cursor`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/DataListRequest.cs`

Keyset paging: the page after an opaque cursor. The cursor of the previous page (`NextCursor`), or `null` for the first page. Page size. Also compute `TotalCount`.

### `record None`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/DataListRequest.cs`

No paging: the whole result.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Instance` | `None` | The shared instance. |

### `record DataListSort`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/DataListRequest.cs`

One sort rule. The key of a sortable field. Sort from the largest to the smallest. Nulls always sort last.

### `record DataListSearch`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/DataListRequest.cs`

The search box: a substring searched, ignoring case, in every search field of the schema. The text; blank matches everything.

### `record DataListFacetRequest`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/DataListRequest.cs`

Asks for the option counts of one filter. The key of a field with an option or boolean filter. Whether the facet's own filter applies. Maximum options returned; `null` for all. Options to skip (for loading more). Keeps only the options whose value contains this text, ignoring case. Also count the rows where the field is empty. Counts only these values (as text, the way a rule carries them), whatever `MaxFacetBuckets` says: a remote option filter asks for the values of the pages it has loaded. A value that no row has is not in the result (a reader takes it as 0). `null` or empty counts every value.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Equals(DataListFacetRequest? other)` | `bool` | Value equality: the values are compared one by one, like the other lists of the model. |
| `GetHashCode()` | `int` | — |

### `record DataListRequest`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/DataListRequest.cs`

What a data list asks a provider for: a page of rows under a sort, a filter tree and a search, plus facet counts.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Paging` | `DataListPaging` | How to page. |
| `Sort` | `IReadOnlyList<DataListSort>` | The sort rules, most significant first; empty uses the schema default. |
| `Filter` | `DataListFilterGroup` | The filter tree (rules and query-only rules). |
| `Search` | `DataListSearch?` | The search, if any. |
| `Facets` | `IReadOnlyList<DataListFacetRequest>` | The facets to count. |
| `TimeZone` | `string?` | The IANA id of the user's time zone (`Europe/Paris`), sent by the client with each request; never part of the URL state. A `Date` on a `DateTimeOffset` or `DateTime` field means that day in this zone, and hour, minute and second units are cut in it. `null`, or an id the server does not know, means UTC (`Sanitize(DataListRequest)` clears unknown ids). |
| `Empty` | `DataListRequest` | A request for everything: no paging, sort, filter, search or facets. |
| `Equals(DataListRequest? other)` | `bool` | — |
| `GetHashCode()` | `int` | — |

### `record DataListFacetOption`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/DataListResult.cs`

One option of a facet and the number of rows that have it. The value as text: invariant culture, `true`/`false`, enum name, ISO dates. The row count.

### `record DataListFacetResult`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/DataListResult.cs`

The counts of one facet.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Key` | `string` | The key of the filter. |
| `Options` | `IReadOnlyList<DataListFacetOption>` | The options, by count descending then value (ordinal), after `Search`, `Skip` and `Limit`. |
| `Missing` | `int?` | Rows where the field is empty, when requested. |
| `Total` | `int` | The number of distinct options before `Skip` and `Limit` (after `Search`); more are available when `Skip + Options.Count &lt; Total`. |
| `Truncated` | `bool` | The facet has more distinct values than `MaxFacetBuckets`: the options are the ones with the highest counts, and the others are not listed or counted in `Total`. A UI can say "more values". |
| `Equals(DataListFacetResult? other)` | `bool` | — |
| `GetHashCode()` | `int` | — |

### `record DataListResult<T>`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/DataListResult.cs`

A page of rows and what goes with it. The row type. The rows of the page. Rows matching the filter and search across all pages; `null` when not computed. The cursor of the next page, or `null` after the last one (cursor paging only). The requested facet counts.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Equals(DataListResult<T>? other)` | `bool` | Value equality: the items are compared one by one with their own equality, like the other lists of the model. |
| `GetHashCode()` | `int` | — |
| `DataListProvider<T>(DataListRequest request, CancellationToken cancellationToken)` | `delegate ValueTask<DataListResult<T>>` | Loads a page of rows for a request. The same shape for an in-memory list, an EF query or an HTTP call. The row type. What to load. Cancels a stale request. |

### `record DataListState`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/DataListState.cs`

The state of a data list as the user left it: search, filters, sort, page, page size, layout and, for cursor paging, the cursor. Immutable. `DataListStateQuery` converts it to and from URL query keys.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Search` | `string` | The search text (`q`). |
| `Filters` | `IReadOnlyList<FilterRule>` | The filter rules, combined with AND (`f.&lt;key&gt;`). |
| `Sort` | `IReadOnlyList<DataListSort>` | The sort rules (`s`); the UI edits one. |
| `Page` | `int` | The 1-based page (`p`). |
| `PageSize` | `int` | The page size (`ps`). |
| `Layout` | `DataListLayout` | The layout (`l`). |
| `Cursor` | `string?` | The cursor of the current page in cursor mode (`c`), `null` on the first page. |
| `Initial` | `DataListState` | The state with nothing applied: empty search, no filters or sort, page 1, 50 rows, table layout. |
| `ToRequest(bool cursorPaging = false, IReadOnlyList<DataListFacetRequest>? facets = null, string? timeZone = null)` | `DataListRequest` | Builds the request for this state: offset paging at `Page`, or cursor paging with `Cursor` when `cursorPaging` is set. `timeZone` is the IANA id of the browser's time zone (`TimeZone`); it comes from the client at request time, not from the state. |
| `Equals(DataListState? other)` | `bool` | — |
| `GetHashCode()` | `int` | — |

### `enum FilterOperator`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/Enums.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `Contains` | `enum member` | — |
| `Is` | `enum member` | — |
| `IsAnyOf` | `enum member` | — |
| `IsNot` | `enum member` | — |
| `Gt` | `enum member` | — |
| `Gte` | `enum member` | — |
| `Lt` | `enum member` | — |
| `Lte` | `enum member` | — |
| `Between` | `enum member` | — |
| `Before` | `enum member` | — |
| `After` | `enum member` | — |
| `IsEmpty` | `enum member` | — |
| `IsNotEmpty` | `enum member` | — |

### `enum FilterKind`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/Enums.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `Text` | `enum member` | — |
| `Option` | `enum member` | — |
| `Boolean` | `enum member` | — |
| `Number` | `enum member` | — |
| `Date` | `enum member` | — |

### `enum FilterCombinator`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/Enums.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `And` | `enum member` | — |
| `Or` | `enum member` | — |

### `enum DateGranularity`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/Enums.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `Day` | `enum member` | — |
| `Hour` | `enum member` | — |
| `Minute` | `enum member` | — |
| `Second` | `enum member` | — |

### `enum DataListLayout`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/Enums.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `Table` | `enum member` | — |
| `Grid` | `enum member` | — |

### `enum FacetMode`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/Enums.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `ExcludeSelf` | `enum member` | — |
| `IncludeSelf` | `enum member` | — |

### `record DataListFilterNode`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/FilterNodes.cs`

### `record FilterRule`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/FilterNodes.cs`

One condition on a field (or on a query-only filter, see `QueryFilter`). The filter key: the stable key of a field, or the name of a query-only filter. The comparison. The operand; `null` for `IsEmpty` and `IsNotEmpty`. Also match rows where the field is empty (null or an empty string). Replaces the `~none` entry of the Nuxt UI table.

### `record DataListFilterGroup`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/FilterNodes.cs`

A group of filter nodes combined with AND or OR. An empty group matches every row.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Combinator` | `FilterCombinator` | How the children combine. |
| `Children` | `IReadOnlyList<DataListFilterNode>` | The children. |
| `Empty` | `DataListFilterGroup` | The group that matches everything. |
| `And(params DataListFilterNode[] children)` | `DataListFilterGroup` | An AND group of the given nodes. |
| `Or(params DataListFilterNode[] children)` | `DataListFilterGroup` | An OR group of the given nodes. |
| `Equals(DataListFilterGroup? other)` | `bool` | — |
| `GetHashCode()` | `int` | — |

### `record FilterValue`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/FilterValue.cs`

### `record Text`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/FilterValue.cs`

A string.

### `record Number`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/FilterValue.cs`

A number. Always a `decimal` so that every numeric field type compares exactly.

### `record Boolean`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/FilterValue.cs`

A boolean.

### `record Date`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/FilterValue.cs`

A calendar day. On `DateTimeOffset` and `DateTime` fields it is that day in the time zone of the request (`TimeZone`, UTC when absent), as the half-open range from its first instant to the next day's. `DateOnly` fields compare the day itself.

### `record Moment`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/FilterValue.cs`

An instant. A date filter matches the whole hour, minute or second (the filter's `DateGranularity`) that contains it, cut in the request's time zone; against a `DateOnly` field it reduces to its local day.

### `record List`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/FilterValue.cs`

A list of option values. Values are strings and are converted to the type of the field (enum name, number, bool, Guid, ISO date).

| Member | Type | Description |
| :--- | :--- | :--- |
| `Values` | `IReadOnlyList<string>` | The values. |
| `Equals(List? other)` | `bool` | — |
| `GetHashCode()` | `int` | — |

### `record Range`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Model/FilterValue.cs`

A range with inclusive bounds, either of which may be missing. Bounds are `Number`, `Date` or `Moment`.

### `class DataListQuery<T>`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Queryable/QueryableDataList.cs`

The two queries of a request, for the caller to run: `Matching` counts the rows (`CountAsync()`) and `Page` loads the page (`ToListAsync()`). Neither runs anything: they are plain `IQueryable{T}` built from `Queryable` operators. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Matching` | `IQueryable<T>` | The rows that match the filter and the search, unordered and unpaged: count them to get the total. |
| `Page` | `IQueryable<T>` | The page: `Matching` sorted (nulls last in both directions, then by row key) and cut by the paging. With cursor paging it holds one row more than the page size, which `QueryableDataList`.ToResult uses to know that a next page exists and removes. |
| `Rows` | `IQueryable<DataListRow<T>>` | `Page` with each row's sort values and key read by the database next to it (`DataListRow{T}`). Load this one for cursor paging: the next cursor is made from these values, not from the loaded entity, so a sort on `e.Department.Name` needs no `Include` and a projected row type needs nothing special. For offset paging or no paging, `Page` is enough. |
| `WantsTotal` | `bool` | Whether the result carries a total: always for offset and no paging, for a cursor only when the request asked (`Count`). Count `Matching` only then. |
| `Deconstruct(out IQueryable<T> matching, out IQueryable<T> page)` | `void` | Splits the query into its two parts: `var (matching, page) = schema.Apply(...)`. |

### `class DataListRow<T>`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Queryable/QueryableDataList.cs`

A row of a page together with what the database computed for its cursor: the value of each sort field, in the order of the sort, and the row key. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Item` | `T` | The row. |
| `SortValues` | `object?[]` | The sort values, boxed, one per sort rule (a null is a null in the database). |
| `Key` | `object?` | The row key, boxed. |

### `class DataListFacetBucket`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Queryable/QueryableDataList.cs`

One option of a facet as read from the database: the value in its own type, and the number of rows that have it.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Value` | `object?` | The value of the field, boxed, never null or an empty string. |
| `Count` | `int` | The number of rows that have the value (a row counts once per distinct value, whatever the number of elements that carry it). |

### `class DataListFacetQuery<T>`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Queryable/QueryableDataList.cs`

The queries that count one facet, for the caller to run, and the code that turns their rows into a `DataListFacetResult`. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Key` | `string` | The key of the facet. |
| `Buckets` | `IQueryable<DataListFacetBucket>` | A group-by query: one row per distinct non-empty value and its count, the most counted first (ties by value), at most `MaxFacetBuckets` plus one (the extra row tells `ToResult` that the facet is truncated). Load it with `ToListAsync()`; the search among the options and the window are applied by `ToResult`. |
| `Missing` | `IQueryable<T>?` | The rows where the field is empty, or `null` when the request did not ask for `IncludeMissing`: count them with `CountAsync()`. |
| `ToResult(IEnumerable<DataListFacetBucket> buckets, int? missing = null)` | `DataListFacetResult` | The facet as the in-memory executor would answer it: options as text, searched, ordered by count then value (ordinal), then cut by `Skip` and `Limit`. The rows of `Buckets`. The count of `Missing`, when it was requested. |

### `class QueryableDataList`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Queryable/QueryableDataList.cs`

Applies a `DataListRequest` to an `IQueryable{T}` of any provider (EF Core, for one) without referencing it: the filter tree, the search, the sort and the paging become `Queryable` operators the provider translates to SQL. The caller runs the queries (`CountAsync`, `ToListAsync`), so the package needs no EF reference. The expressions are the raw flavour of the filter definitions: values travel as parameters, never as literals (an `IN` for an option list, plain `Contains` for text, no `ToLower`), nothing is client-evaluated, and a null along a path (`e.Department.Name`) is SQL null. The results are those of `ExecuteInMemory{T}`, with these differences: text case and accents follow the database (the collation of the column, and how the provider translates `Contains`: SQLite's is case sensitive whatever the collation), and sorting text uses the database's order instead of the natural order. The providers must be able to compare the types of the fields: SQLite needs a value converter for `DateTimeOffset` (UTC ticks) and `decimal` (double); see the package README.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Apply<T>(this DataListSchema<T> schema, IQueryable<T> source, DataListRequest request)` | `DataListQuery<T>` | Builds the queries of a request. The request is first passed through `Sanitize(DataListRequest)`, and query-only filters are skipped (read them with `QueryFilterRules` and apply them to `source` before this call). Rows are ordered by the sort (the schema default when the request has none), nulls last in both directions, then by the row key when the schema has one, so that pages never repeat or skip a row. Cursor paging is a keyset `WHERE` over the sort values and the row key. The row type. The schema of the rows. The rows: a provider query, not an in-memory sequence. What to compute. `source` is an in-memory `AsQueryable()` (the check recognizes the LINQ to Objects provider only: a test double that wraps one, or any other provider that evaluates in memory, is not caught and must use `ExecuteInMemory` too; use `ExecuteInMemory`: it compares text ignoring case and guards nulls, which a provider expression does not); cursor paging on a schema without a row key, on a key or sort field a cursor cannot carry, or on a `Guid` (databases order them in their own way). |
| `FacetQueries<T>(this DataListSchema<T> schema, IQueryable<T> source, DataListRequest request)` | `IReadOnlyList<DataListFacetQuery<T>>` | The count queries of the requested facets (the sanitized ones: option and boolean fields), one per facet, for the caller to run. A facet counts the rows that match the search and the filters, leaving out the facet's own filter in the default `ExcludeSelf` mode. A facet on a collection field in a schema without a row key (a row is counted once per value, which needs the key). |
| `ToResult<T>(
        this DataListSchema<T> schema,
        DataListRequest request,
        IReadOnlyList<T> pageRows,
        int? totalCount = null,
        IReadOnlyList<DataListFacetResult>? facets = null)` | `DataListResult<T>` | The `DataListResult{T}` of an offset or unpaged request from what the caller loaded: the rows of `Page`, the count of `Matching` and the facets (`ToResult`). The request pages by cursor: load `Rows` and use the overload that takes them, because the cursor needs the sort values the database computed. |
| `ToResult<T>(
        this DataListSchema<T> schema,
        DataListRequest request,
        IReadOnlyList<DataListRow<T>> rows,
        int? totalCount = null,
        IReadOnlyList<DataListFacetResult>? facets = null)` | `DataListResult<T>` | The `DataListResult{T}` of any request from the rows of `Rows`. With cursor paging the extra row is removed and `NextCursor` is made from the sort values and the key that the database returned for the last row of the page. The schema of the rows. The request that was applied. The rows loaded from `Rows`. The count of `Matching`, when `WantsTotal`. The facet results, or `null` when none were requested. |
| `Signature(IReadOnlyList<DataListSort> sort)` | `string` | — |
| `Validate<T>(List<DataListField<T>> sortFields, FieldAccessor key)` | `void` | — |
| `TryParse<T>(List<DataListField<T>> sortFields, string?[] texts, out object?[] values)` | `bool` | — |
| `Lambda(Expression body, params ParameterExpression[] parameters)` | `LambdaExpression` | — |
| `AddOrder<T>(IQueryable<T> source, LambdaExpression key, bool descending, bool first)` | `IQueryable<T>` | — |
| `Window<T>(IQueryable<T> source, MethodInfo definition, int count)` | `IQueryable<T>` | Skip or take with the count in a holder, so the provider sees a member access and emits a parameter (a literal would give each page its own query plan). |
| `After<T>(
        ParameterExpression p,
        List<(DataListField<T> Field, DataListSort Rule)` | `Expression` | The rows that sort strictly after the cursor: for some sort field i, all the fields before it equal the cursor's and field i comes after (nulls last, so a null is after every value and nothing is after a null), or all fields are equal and the row key is greater. |
| `Buckets<T>(DataListSchema<T> schema, DataListField<T> field, IQueryable<T> rows, IReadOnlyList<string>? values = null)` | `IQueryable<DataListFacetBucket>` | — |
| `Row` | `TRow?` | — |
| `Value` | `TValue?` | — |

### `enum DataListNumberDisplay`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Schema/DataListFilter.cs`

How a number filter editor shows its value.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Input` | `enum member` | — |
| `Slider` | `enum member` | — |
| `InputAndSlider` | `enum member` | — |

### `enum DataListOptionSort`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Schema/DataListFilter.cs`

How an option filter orders the options it lists.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Count` | `enum member` | — |
| `Alpha` | `enum member` | — |

### `class DataListFilter`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Schema/DataListFilter.cs`

The definition of a filter: its kind, the operators it allows, its default operator and default value, and the limits an editor shows. Immutable; the `With...` methods return a changed copy. Create one with `Text`, `Option()`, `Boolean`, `Number` or `Date` and attach it to a field (`Filter`) or register it as a query-only filter (`QueryFilter`).

| Member | Type | Description |
| :--- | :--- | :--- |
| `Kind` | `FilterKind` | The kind of filter. |
| `AllowedOperators` | `IReadOnlyList<FilterOperator>` | The operators a rule of this filter may use. The per-kind set of the spec, or the subset given to `WithOperators`. |
| `DefaultOperator` | `FilterOperator` | The operator a rule gets when none is given, and the one a URL key without an operator suffix means. |
| `DefaultValue` | `FilterValue?` | The value effective when the URL has no override; omitted from the URL and not counted as active. `null` for none. |
| `DefaultValueOperator` | `FilterOperator` | The operator of the default rule (`DefaultOperator` unless `WithDefault` set another). |
| `DefaultIncludeEmpty` | `bool` | Whether the default rule also includes empty rows. |
| `Min` | `decimal?` | The smallest value an editor offers (number filters). |
| `Max` | `decimal?` | The largest value an editor offers (number filters). |
| `Step` | `decimal?` | The step of a number editor. |
| `Granularity` | `DateGranularity` | The precision of a date editor; decides whether values are `Date` or `Moment`. |
| `NumberDisplay` | `DataListNumberDisplay` | How a number editor shows its value: a field, a slider or both. Only number filters; a slider needs `Min` and `Max`, without them the editor shows the field. |
| `NumberFormat` | `string?` | The .NET numeric format string the filter tag writes a number with (`N0`, `C2`, `0.0 'kg'`), in the culture of the user. It formats the tag only, not the field of the editor. `null` writes the number with its digits and the group separator of the culture. |
| `MaxRangeDays` | `int?` | The most days a range of a date editor may cover, both ends included. `null` for no limit. This limits the editor only (the calendar, the presets and the typed ends): a range in the URL, a bound state or a request is not checked against it, and neither is it on the server. |
| `Persist` | `bool` | Whether the user's last value is remembered between visits (browser storage), restored only when the URL has no value. |
| `Label` | `string?` | The name of the filter in the interface (the tag, the add filter menu). `null` falls back to the title of the column with the same key, then to the key written as words. |
| `Options` | `IReadOnlyList<DataListFilterOption>` | The options an option filter offers, in order; empty for the other kinds. Without them the editor lists the values of the facet counts (`Facets`) and the values already selected. |
| `Facets` | `bool` | Whether the data list asks for the counts of this filter (the rows that have each option, or each yes and no) and shows them in the editor. Only for option and boolean filters. |
| `FacetMode` | `FacetMode` | Whether the counts leave out this filter's own rule (`ExcludeSelf`, the default: the other options stay visible) or keep it (`IncludeSelf`). |
| `OptionSort` | `DataListOptionSort?` | How the editor orders the options. `null` keeps the declared options in their order and lists the options that come from counts by count (the order of the facet). Not used for remote options, which keep the order of their source. |
| `OptionLimit` | `int?` | How many options the editor lists before a "show more" button, or `null` to list them all. The selected options always show. A statically rendered editor lists them all. |
| `RemoteOptions` | `DataListRemoteOptions?` | Where the options come from when they are loaded from a remote source, page by page, with a search. `null` for declared options and for options taken from the counts. |
| `Text()` | `DataListFilter` | A text filter: `contains` (default), `is`, `isNot`. |
| `Option()` | `DataListFilter` | An option filter: `isAnyOf` (default), `is`, `isNot`. Works on strings, enums, numbers, booleans, Guids and dates. |
| `Option(params DataListFilterOption[] options)` | `DataListFilter` | An option filter with the options its editor lists (value and label). The options, in the order the editor shows them. |
| `Option(params string[] values)` | `DataListFilter` | An option filter that lists `values`, each labelled by itself. The values, in the order the editor shows them. |
| `Boolean(bool? defaultValue = null)` | `DataListFilter` | A boolean filter: `is` (default), `isNot`. The value effective without a URL override, or `null` for none. |
| `Number(decimal? min = null, decimal? step = null, decimal? max = null)` | `DataListFilter` | A number filter: `is` (default), `isNot`, `gt`, `gte`, `lt`, `lte`, `between`. The smallest value an editor offers. The step of the editor. The largest value an editor offers. |
| `Date(DateGranularity granularity = DateGranularity.Day)` | `DataListFilter` | A date filter: `is` (default), `isNot`, `before`, `after`, `between`. The precision; `Day` edits `Date` values. |
| `WithOperators(params FilterOperator[] operators)` | `DataListFilter` | Restricts the allowed operators. Each must belong to the kind's set, except `IsEmpty` and `IsNotEmpty`, which any kind may add. Resets the default operator to the first one when the old default is no longer allowed. |
| `WithDefaultOperator(FilterOperator op)` | `DataListFilter` | Sets the operator used when none is given. It must be allowed. |
| `WithDefault(FilterValue value, FilterOperator? op = null, bool includeEmpty = false)` | `DataListFilter` | Sets the default rule: effective without a URL override, omitted from the URL. The value. The operator of the default rule; `DefaultOperator` when `null`. Whether the default rule includes empty rows. |
| `WithPersist(bool persist = true)` | `DataListFilter` | Remembers the last value of this filter between visits. |
| `WithLabel(string label)` | `DataListFilter` | Names the filter in the interface. The label of the tag and of the add filter menu. |
| `WithOptions(params DataListFilterOption[] options)` | `DataListFilter` | Sets the options the editor lists. Only for option filters. The options, in order; their values must be distinct. |
| `WithFacets(bool facets = true)` | `DataListFilter` | Asks the data list for the counts of this filter and shows them next to the options (or next to Yes and No). By default the counts leave out this filter's own rule, so the other options stay visible (see `WithFacetMode`). The counts are asked for when the filters or the search change, and kept while the user pages or sorts. Whether to ask for the counts. |
| `WithFacetMode(FacetMode mode)` | `DataListFilter` | Chooses whether the counts of this filter keep its own rule (`IncludeSelf`) or leave it out (`ExcludeSelf`, the default). The mode. |
| `WithOptionSort(DataListOptionSort sort)` | `DataListFilter` | Orders the declared options and the counted options of an option filter by count or by label (`OptionSort`). Remote options keep the order of their source: the sort is ignored for them. The order. |
| `WithOptionLimit(int limit)` | `DataListFilter` | Lists only the first options of an option filter, with a "show more" button for the others. The selected options always show. How many options to list first; at least 1. |
| `WithRemoteOptions(DataListOptionsProvider provider, DataListOptionsResolver? resolveSelected = null, int pageSize = DataListRemoteOptions.DefaultPageSize, int minLength = 0)` | `DataListFilter` | Loads the options from a remote source instead of declaring them: the editor asks for a page when it is shown (a tag's popover that opens, a panel section that opens, the add filter menu when the filter is picked), searches while the user types (after a pause), and loads the next page as the list scrolls. The values of a rule that the loaded pages do not contain (restored from the address) get their labels from `resolveSelected`, asked for while the rows load. Only interactive renders load pages: a statically rendered editor lists the selected options and, when the filter has counts (`WithFacets`), the most counted ones (at most `OptionLimit`, else the page size), all labelled in one call to the resolver; a counted option the resolver does not know is left out. The counts of the options that are listed are asked for by value (`Values`), so every page has its counts whatever the number of distinct values. `OptionSort` does not apply: the options keep the order of the source. Loads one page of options for a search. Returns the options of values that no loaded page holds; values it leaves out show as they are. How many options to ask for per page. The shortest search that is sent; a shorter one (not empty) lists nothing. |
| `WithNumberDisplay(DataListNumberDisplay display)` | `DataListFilter` | Chooses how a number editor shows its value. A slider needs `Min` and `Max`; without them the editor shows the field. The display. |

### `record DataListFilterOption`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Schema/DataListFilter.cs`

One option of an option filter: the value that goes into the rule and the URL, and what the interface shows. The value as text, the way a `List` carries it (an enum name, a number, `true`). The text shown to the user.

### `record DataListOptionsRequest`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Schema/DataListRemoteOptions.cs`

What a `DataListOptionsProvider` is asked for: one page of options for a search. Both kinds of paging are carried: a source that pages by position reads `Skip` and `Take`, one that pages with an opaque token reads `Cursor`. The editor fills all of them. The text typed in the search field of the editor, trimmed. Empty when nothing is typed: list the options unfiltered. How many options the earlier pages of this search returned (0 for the first page). The number of options wanted. The `NextCursor` of the previous page; `null` for the first page and for a source that does not use cursors.

### `record DataListOptionsPage`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Schema/DataListRemoteOptions.cs`

One page of options loaded from a remote source. The options of this page, in the order to list them. A value that an earlier page of the same search already listed is skipped. Whether another page exists. A non-null `NextCursor` also means there is one. The cursor that asks for the page after this one; `null` when the source pages by `Skip` and `Take` or this is the last page.

| Member | Type | Description |
| :--- | :--- | :--- |
| `DataListOptionsProvider(DataListOptionsRequest request, CancellationToken cancellationToken)` | `delegate ValueTask<DataListOptionsPage>` | Loads the options of an option filter from a remote source, one page at a time. The token is cancelled when a newer search replaces the request (a late answer is ignored anyway). An exception shows an error with a retry button in the editor. The search and the page wanted. Cancelled when the request is stale. |
| `DataListOptionsResolver(IReadOnlyList<string> values, CancellationToken cancellationToken)` | `delegate ValueTask<IReadOnlyList<DataListFilterOption>>` | Returns the options of values that no loaded page holds (a value restored from the address, whose page was never loaded), so that a tag and an editor can show their labels. Values it cannot find may be left out: they show as they are. The values to label. Cancelled when the list goes away. |

### `class DataListRemoteOptions`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Schema/DataListRemoteOptions.cs`

The remote source of the options of an option filter. See `WithRemoteOptions`.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Provider` | `DataListOptionsProvider` | Loads a page of options. |
| `Resolver` | `DataListOptionsResolver?` | Labels the values that no loaded page holds, or `null` when values show as they are. |
| `PageSize` | `int` | How many options each page asks for. |
| `MinLength` | `int` | The shortest search that is sent. |

### `class DataListSchema`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Schema/DataListSchema.cs`

Describes what a data list can do with a row type: its fields, which are sortable or filterable, the search fields, the query-only filters, the defaults and the limits. Built in C# with expressions (no reflection over string paths, so it is trim-safe) through `For{T}`. The schema decides what a request may ask: `Sanitize(DataListRequest)` drops anything it does not know.

| Member | Type | Description |
| :--- | :--- | :--- |
| `For<T>(Action<DataListSchemaBuilder<T>> configure)` | `DataListSchema<T>` | Starts the schema of a row type. See `DataListSchemaBuilder{T}`. The row type. Describes the schema. |
| `FilterKeys` | `IReadOnlyList<string>` | The keys of every filter (fields and query-only filters), in declaration order. |
| `SortKeys` | `IReadOnlyList<string>` | The keys of the sortable fields. |
| `DefaultSort` | `IReadOnlyList<DataListSort>` | The sort applied when a request has none; omitted from the URL. |
| `DefaultPageSize` | `int` | The page size when none is given (50 unless set, never above `MaxPageSize`). |
| `MaxPageSize` | `int` | The largest page a request may ask for (500 unless set). |
| `DefaultLayout` | `DataListLayout` | The layout when none is given. |
| `DefaultState` | `DataListState` | The state with every default applied: the default filter values, sort, page 1, `DefaultPageSize` and `DefaultLayout`. The URL representation omits whatever equals it. |
| `MaxFilterRules` | `int` | The most filter rules `Sanitize(DataListRequest)` keeps (100 unless set); later ones are dropped. Bounds the work a hostile request can ask for. |
| `MaxListValues` | `int` | The most values `Sanitize(DataListRequest)` keeps in the list of one rule (500 unless set); later ones are dropped. |
| `MaxFacetBuckets` | `int` | The most distinct options a facet keeps (1000 unless set): the ones with the highest counts, ties by value. A facet over more values is cut and marked `Truncated`; the search among the options and the window (`Skip`, `Limit`) apply to the kept ones. In a database the cap is a `TAKE` on the group-by, so a column with millions of values never transfers them all. |
| `HasSearch` | `bool` | Whether the schema has search fields. |
| `TryGetFilter(string key, [NotNullWhen(true)` | `bool` | Gets the definition of a filter by key. |
| `IsQueryFilter(string key)` | `bool` | Whether `key` is a query-only filter: part of the request for the provider, never matched against rows by the in-memory executor. |
| `QueryFilterRules(DataListRequest request)` | `IReadOnlyList<FilterRule>` | The query-only rules of a request, for the provider to interpret. Looks through nested groups. |
| `Sanitize(DataListRequest request)` | `DataListRequest` | Returns a request a provider can trust: filter rules with an unknown key, a disallowed operator or a value of the wrong shape are dropped (empty nested groups with them), sort keys that are unknown, not sortable or repeated are dropped, facets that are not option or boolean field filters are dropped, the search is dropped when the schema has no search field, a page size is clamped to 1..`MaxPageSize` (zero or less means `DefaultPageSize`), a negative skip becomes 0 and a facet limit is clamped to 1..1000. Hostile input is bounded: rules without a key and null nodes are dropped, a list keeps at most `MaxListValues` distinct non-null values, a request keeps at most `MaxFilterRules` rules and groups nest at most `MaxFilterDepth` deep. A time zone id the server does not know is cleared (UTC). |

### `class DataListSchema<T>`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Schema/DataListSchema.cs`

The schema of a row type : a `DataListSchema` that can also read a row's key and label and run requests in memory. The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `HasRowKey` | `bool` | Whether the schema has a `RowKey`. Cursor paging needs one. |
| `GetRowKey(T item)` | `string` | The stable key of a row, as text (invariant culture). The schema has no row key. |
| `GetRowLabel(T item)` | `string?` | The label that names a row for accessibility ("Select Alice"), or `null`. |

### `class DataListSchemaBuilder<T>`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Schema/DataListSchemaBuilder.cs`

The fluent description of a `DataListSchema{T}`, passed to `For{T}`: DataListSchema.For&lt;Employee&gt;(s =&gt; s .RowKey(e =&gt; e.Id).RowLabel(e =&gt; e.Name) .Search(e =&gt; e.Name, e =&gt; e.Email) .Field(e =&gt; e.Age, f =&gt; f.Sortable().Filter(DataListFilter.Number(min: 0, step: 1))) .Field(e =&gt; e.Department.Name, f =&gt; f.Filter(DataListFilter.Option())) .QueryFilter("archived", DataListFilter.Boolean(false)) .DefaultSort(e =&gt; e.Name) .MaxPageSize(500)); The row type.

| Member | Type | Description |
| :--- | :--- | :--- |
| `RowKey<TKey>(Expression<Func<T, TKey>> selector)` | `DataListSchemaBuilder<T>` | The stable identity of a row, used for selection and as the cursor tie-break. Converted to text in the invariant culture. |
| `RowLabel(Func<T, string?> label)` | `DataListSchemaBuilder<T>` | The label that names a row for assistive technology ("Select Alice"). |
| `Search(params Expression<Func<T, string?>>[] fields)` | `DataListSchemaBuilder<T>` | The string fields the search box looks in (case-insensitive substring, any field matches). The expressions are compiled for in-memory evaluation and passed as written to a database provider. |
| `Field<TProp>(Expression<Func<T, TProp>> selector, Action<DataListFieldBuilder<T, TProp>>? configure = null)` | `DataListSchemaBuilder<T>` | Registers a field. Its key is the member path in camelCase (`department.name`) unless `Key` says otherwise. |
| `Collection<TElem, TProp>(
        Expression<Func<T, IEnumerable<TElem>?>> collection,
        Expression<Func<TElem, TProp>> element,
        Action<DataListCollectionFieldBuilder<T, TProp>>? configure = null)` | `DataListSchemaBuilder<T>` | Registers a field that reads one value from each element of a collection of the row: `.Collection(e =&gt; e.Tags, t =&gt; t.Name, f =&gt; f.Filter(DataListFilter.Option()))`. The key joins both paths in camelCase (`tags.name`). A row matches a rule when at least one element does; `isNot` is the negation of `is`/`isAnyOf` (no element is any of the values, so a row without elements matches); `isEmpty` means no non-empty element (an empty or null collection included). The predicate becomes `collection.Any(x =&gt; ...)`. Collection fields cannot be sorted. The element type. The type of the value read from an element. The collection of the row (a property of a type that is an `IEnumerable{T}` of ). The value read from an element. The key and the filter. |
| `SearchAny<TElem>(Expression<Func<T, IEnumerable<TElem>?>> collection, Expression<Func<TElem, string?>> element)` | `DataListSchemaBuilder<T>` | The search also looks at a string of each element of a collection (any element matches). |
| `QueryFilter(string key, DataListFilter filter)` | `DataListSchemaBuilder<T>` | A filter that is not a field of the row: its rules reach the provider (`QueryFilterRules`) but the in-memory executor never matches rows against them. Use it for server-side switches such as `archived`. |
| `DefaultSort<TProp>(Expression<Func<T, TProp>> selector, bool descending = false)` | `DataListSchemaBuilder<T>` | The sort used when a request has none. The field is made sortable (and registered when missing). |
| `DefaultSort(string key, bool descending = false)` | `DataListSchemaBuilder<T>` | The sort used when a request has none, by the key of a sortable field. |
| `MaxPageSize(int size)` | `DataListSchemaBuilder<T>` | The largest page a request may ask for (500 by default). |
| `MaxFilterRules(int count)` | `DataListSchemaBuilder<T>` | The most filter rules a sanitized request keeps (100 by default). |
| `MaxListValues(int count)` | `DataListSchemaBuilder<T>` | The most values a sanitized request keeps in the list of one rule (500 by default). |
| `MaxFacetBuckets(int count)` | `DataListSchemaBuilder<T>` | The most distinct options a facet keeps (1000 by default); more are cut and the facet is marked truncated. See `MaxFacetBuckets`. |
| `DefaultPageSize(int size)` | `DataListSchemaBuilder<T>` | The page size when none is given (50 by default, never above the maximum). |
| `DefaultLayout(DataListLayout layout)` | `DataListSchemaBuilder<T>` | The layout when none is given (table by default). |
| `Source` | `ValueSource` | — |
| `ExplicitKey` | `string?` | — |
| `IsSortable` | `bool` | — |
| `StoresLocalTime` | `bool` | — |
| `Filter` | `DataListFilter?` | — |
| `ResolveKey()` | `string` | — |

### `class DataListFieldBuilder<T, TProp>`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Schema/DataListSchemaBuilder.cs`

The options of one field, passed to `Field{TProp}`. The row type. The type of the field.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Key(string key)` | `DataListFieldBuilder<T, TProp>` | Overrides the stable key. Needed for fields that are not a plain member path. |
| `Sortable(bool sortable = true)` | `DataListFieldBuilder<T, TProp>` | Lets requests sort by this field. |
| `StoresLocalTime(bool localTime = true)` | `DataListFieldBuilder<T, TProp>` | The `DateTime` values of this field are wall-clock times in the time zone of the request (a column such as PostgreSQL `timestamp without time zone`, or a SQL Server `datetime2` that holds local times), not UTC. The bounds of a date filter are then local wall-clock times with kind `Unspecified` (a day is the local midnight to the next local midnight, an hour is the local hour) instead of UTC instants, so a provider that refuses a UTC `DateTime` for such a column accepts them. Without this option a `DateTime` field is read as UTC. Only for `DateTime` fields. |
| `Filter(DataListFilter filter)` | `DataListFieldBuilder<T, TProp>` | Gives the field a filter. The filter kind must suit the type of the field (text on a string, number on a numeric type, and so on). |

### `class DataListCollectionFieldBuilder<T, TProp>`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/Schema/DataListSchemaBuilder.cs`

The options of a collection field, passed to `Collection{TElem, TProp}`. There is no `Sortable`: a collection has no single value to sort by. The row type. The type of the value read from an element.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Key(string key)` | `DataListCollectionFieldBuilder<T, TProp>` | Overrides the stable key (`tags.name` by default). |
| `StoresLocalTime(bool localTime = true)` | `DataListCollectionFieldBuilder<T, TProp>` | The `DateTime` values read from the elements are wall-clock times in the request's time zone, not UTC. See `StoresLocalTime`. |
| `Filter(DataListFilter filter)` | `DataListCollectionFieldBuilder<T, TProp>` | Gives the field a filter; the kind must suit the element value type. |

### `class DataListStateQuery`
*Package*: `Lux.Query.Core` · *Source*: `src/Lux.Query.Core/State/DataListStateQuery.cs`

Converts a `DataListState` to and from URL query keys, as plain functions (no Blazor, no `NavigationManager`). Keys, under an optional prefix joined with a dot (`users.q`): `q`the search text `p`the 1-based page, at least 1 and at most `MaxPage` `ps`the page size, from 1 to the schema's maximum `s`the sort, `name|-age` (a leading `-` is descending); present and empty means "no sort" `f.&lt;key&gt;`a filter rule with the default operator of its kind `f.&lt;key&gt;.&lt;op&gt;`a filter rule with another operator (`contains is isAnyOf isNot gt gte lt lte between before after isEmpty isNotEmpty`) `c`the cursor `l``table` or `grid` A filter value is a comma-separated list; `,` `~` and `%` in a value are escaped as `%2C` `%7E` `%25`; a bare `~` token means "include empty rows"; a range is `from..to`. Values equal to the schema's `DefaultState` are omitted. A default filter rule that the user removed is written as its key with an empty value. The state keeps one rule per key; with two, the default rule of that key is not restored. A filter key that is repeated (`f.status=Paid&amp;f.status=Draft`, what a form with a checkbox per option sends) is read as one value: an option filter takes all of them, another kind keeps the first one that is not blank, plus a repeat that is exactly `~` (include empty rows); the address the app writes is always one comma list. A range may also be written as two keys, `f.&lt;key&gt;.from` and `f.&lt;key&gt;.to` (what a GET form can send): they read as `f.&lt;key&gt;.between`, and a state is always written back with the latter. Operators are matched ignoring case. A filter key may contain dots: the whole text after `f.` is tried as a key first, then the part before the last dot as a key and the part after as an operator.

| Member | Type | Description |
| :--- | :--- | :--- |
| `ToQuery(DataListState state, DataListSchema schema, string? prefix = null)` | `IReadOnlyDictionary<string, string>` | Writes the keys of a state that differ from the defaults. Rules the schema does not accept are left out, so the result always reads back. The state. The schema giving the defaults, the filter kinds and the sortable keys. An optional key prefix for pages with several data lists. Raw (not URL-encoded) keys and values, in a stable order. |
| `FromQuery(IEnumerable<KeyValuePair<string, string?>> query, DataListSchema schema, string? prefix = null)` | `DataListState` | Reads a state from query keys. Starts from the schema defaults, so an empty query gives `DefaultState`. Never throws on bad input: unknown filter keys, operators the filter does not allow, values that do not parse, unknown or repeated sort keys and an unknown layout are dropped (the default applies); the page is clamped to 1..`MaxPage` and the page size to 1..`MaxPageSize`. The decoded key and value pairs; when a key repeats, the first one counts, except that the repeats of a filter key are merged (see the remarks). Null values are ignored. The schema giving the defaults, the filter kinds and the sortable keys. The key prefix given to `ToQuery`. |
| `FromQuery(IEnumerable<KeyValuePair<string, string?>> query, DataListSchema schema, string? prefix, string? timeZone)` | `DataListState` | `FromQuery(IEnumerable{KeyValuePair{string, string?}}, DataListSchema, string?)` for a request that has a time zone: a date-time in a filter value without an offset (`2026-11-05T14:00`, what the `datetime-local` field of a static form posts) is the wall clock of that zone, not UTC. A date (`2026-11-05`) and a value with an offset do not depend on it. The decoded key and value pairs; when a key repeats, the first one counts, except that the repeats of a filter key are merged (see the remarks). Null values are ignored. The schema giving the defaults, the filter kinds and the sortable keys. The key prefix given to `ToQuery`. The IANA id of the zone of the user; null or an unknown id means UTC. |

### `class EfAsyncQueryExecutor`
*Package*: `Lux.Query.EntityFrameworkCore` · *Source*: `src/Lux.Query.EntityFrameworkCore/EfAsyncQueryExecutor.cs`

Asynchronous Entity Framework Core query helpers for `Lux.Query`.

| Member | Type | Description |
| :--- | :--- | :--- |
| `CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)` | `ValueTask<int>` | — |
| `ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)` | `ValueTask<List<T>>` | — |

### `class LuxQueryServiceCollectionExtensions`
*Package*: `Lux.Query` · *Source*: `src/Lux.Query/LuxQueryServiceCollectionExtensions.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `AddLuxQuery(this IServiceCollection services)` | `IServiceCollection` | — |

### `class QueryGroup<TRecord>`
*Package*: `Lux.Query` · *Source*: `src/Lux.Query/QueryState/QueryGroup.cs`

A record whose properties are query keys, read and written together: one write is one navigation and one history entry. Each property has a codec and, like `QueryValue{T}`, is left out of the URL when it equals the matching property of the default record. Make one with `Group{TRecord}`, add its properties with `Add{TValue}`, and dispose it with the component that owns it (which also releases its prefix). An immutable record; its properties are changed with `with` expressions.

| Member | Type | Description |
| :--- | :--- | :--- |
| `ThrowIfConflicted()` | `void` | Throws when a key of this group is also used by a claim that outlived the page change this group was created in (see `Claim`). Every use of the group calls it. The conflict, naming both owners. |
| `History` | `QueryHistory` | The history of changes made through `Set(TRecord)` and `Update` without a history of their own. |
| `Add<TValue>(string key, Func<TRecord, TValue> get, Func<TRecord, TValue, TRecord> with, IQueryCodec<TValue> codec)` | `QueryGroup<TRecord>` | Adds a property. The default of the property is the one of the default record. The query key of the property; with a prefix, `prefix.key`. Reads the property of a record. Returns the record with the property set, usually `(r, v) =&gt; r with { Page = v }`. Converts the property to and from text. The group, to chain. |
| `Value` | `TRecord` | The record the URL describes: the default with each property whose key is present and readable. |
| `Set(TRecord value)` | `bool` | Writes every property of `value` in one change, with the history of the group. A property equal to its default is removed. The new record. False when nothing was written because the renderer is not interactive. |
| `Set(TRecord value, QueryHistory history)` | `bool` | Writes every property of `value` in one change. The new record. Whether the change adds a history entry. False when nothing was written because the renderer is not interactive. |
| `Update(Func<TRecord, TRecord> change, QueryHistory? history = null)` | `bool` | Changes the current record with `change` and writes it, for example `g.Update(f =&gt; f with { Page = 1 }, QueryHistory.Push)`. Returns the new record from the current one. Whether the change adds a history entry; the history of the group when null. |
| `HrefWith(TRecord value)` | `string` | The URL of the current page with the properties of `value` written, for a link in static server rendering. The record the link carries. |
| `Dispose()` | `void` | Stops following the URL and releases the prefix. |

### `enum QueryHistory`
*Package*: `Lux.Query` · *Source*: `src/Lux.Query/QueryState/QueryState.cs`

Whether a change of the URL adds a history entry (the Back button undoes it) or replaces the current one.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Push` | `enum member` | — |
| `Replace` | `enum member` | — |

### `class QueryState`
*Package*: `Lux.Query` · *Source*: `src/Lux.Query/QueryState/QueryState.cs`

The query string of the current URL as state: read it, change it, follow it when it changes. The URL is the single source of truth, so a link, the Back button and a URL typed in the address bar all change what the components bound to it show, and a page can be shared with its state. One instance per circuit or per browser tab (it is scoped), registered by `AddBluxUiTools`. Writes made synchronously in one event handler, before it first yields to the renderer (an `await` that does not complete at once), merge and become a single navigation started right after; a write made after that point is another batch. If any write of a batch asks for `Push` the whole batch pushes. A value equal to what the URL already holds writes nothing. Parameters this service never wrote (`utm_source`, a parameter of another component) stay in the URL as they were. A handler that writes and then navigates to another page (`Query.Set(...); Navigation.NavigateTo("/details/5")`) loses the write: the navigation wins. Prerendering and static rendering. No navigation can happen there, and a navigation manager that is configured not to throw (`BlazorDisableThrowNavigationException`) turns the attempt into a real HTTP redirect. So a write is never sent to the navigation manager unless the renderer is known to be interactive: `Bind{T}` and `Group{TRecord}` take the `RendererInfo` of the component for this, and a write from a non-interactive renderer changes nothing and returns false. A page offers its changes as links instead (`HrefWith(IEnumerable{KeyValuePair{string, string?}})`). The raw `Set` and `SetMany` need the same knowledge, from `UseRenderer`; without it they throw `InvalidOperationException` on the server (in WebAssembly the renderer is always interactive). A navigation of this service that never comes back (a `NavigationLock` cancelled it) is rolled back after a few seconds to what the URL really holds. A key matches a prefix when it equals it or starts with the prefix and a dot (`f` matches `f.name`); the empty prefix matches every key.

| Member | Type | Description |
| :--- | :--- | :--- |
| `UseRenderer(RendererInfo renderer)` | `void` | Tells the service whether the renderer is interactive. Only an interactive renderer can navigate, so while this is false a write changes nothing (see the remarks of the class). `Bind{T}` and `Group{TRecord}` call it with the renderer you pass; call it yourself before the raw `Set` and `SetMany`, from `OnInitialized` with the `RendererInfo` of the component. |
| `Interactive` | `bool?` | True when writes can navigate, false when the renderer is known not to be interactive, null when nothing said so yet (`UseRenderer`). |
| `Get(string key)` | `string?` | The first value of a query key, including the writes of the current handler that have not reached the URL yet; null when the key is absent. |
| `Set(string key, string? value, QueryHistory history = QueryHistory.Replace)` | `bool` | Sets a query key; a null value removes it. Returns false when nothing was written because the renderer is not interactive. The key. The new text, or null to remove the key. Whether the change adds a history entry. The renderer is not known (`UseRenderer`) on the server. |
| `SetMany(IEnumerable<KeyValuePair<string, string?>> changes, QueryHistory history = QueryHistory.Replace)` | `bool` | Sets several keys in one change: one navigation, one history entry. A null value removes its key. The keys and their new text. Whether the change adds a history entry. False when nothing was written because the renderer is not interactive. The renderer is not known (`UseRenderer`) on the server. |
| `FlushAsync()` | `Task` | Sends the changes made so far to the URL now instead of after the current handler yields. Useful before leaving the page by another means; the changes of a handler normally need no call. |
| `Subscribe(string prefix, Action<IReadOnlyList<string>> handler)` | `IDisposable` | Calls `handler` with the keys that changed, when a key matching `prefix` changes: through a write of this service, or through the URL (Back and Forward, a link, a typed URL). The handler runs on the thread that raised the change: from a component, go through `InvokeAsync` before rendering. It is not called for a navigation that leaves the state as it already is. The prefix of the keys to follow, or the empty text for all of them. Receives the changed keys that match, in no particular order. Dispose it to stop. |
| `HrefWith(IEnumerable<KeyValuePair<string, string?>> changes)` | `string` | The URL (path and query) of the current page with the changes applied, for a link or a form that carries a state change without interactivity (static server rendering): the same keys as `Set` writes. A null value removes its key. The keys and their new text. |
| `HrefWith(params (string Key, string? Value)` | `string` | — |
| `Claim(string prefix, string owner)` | `QueryClaim` | Declares that a component owns a prefix of the query keys (`users` owns `users.q`, `users.p`), or all of them with the empty prefix. A second claim of the same prefix throws and names both owners. A claim that meets one made before the last change of page (a layout, a page reused for another route parameter) is judged once the render that is under way has finished and the previous page is disposed: `ThrowIfConflicted` then reports it (`QueryGroup{TRecord}` calls it on every use). The prefix, or the empty text. A name for the claiming component, used in the message. Dispose it to release the prefix. The prefix is already claimed on this page. |
| `Bind<T>(string key, IQueryCodec<T> codec, T defaultValue, RendererInfo renderer, QueryHistory history = QueryHistory.Replace)` | `QueryValue<T>` | Binds one query key to a typed value. See `QueryValue{T}`. The query key. Converts the value to and from text. The value when the key is absent or does not parse; a value equal to it is left out of the URL. The `RendererInfo` of the component: nothing is written unless it is interactive. The history of changes made through `Value`. |
| `Group<TRecord>(TRecord defaultValue, string owner, RendererInfo renderer, string? prefix = null, QueryHistory history = QueryHistory.Replace)` | `QueryGroup<TRecord>` | Binds a record whose properties are query keys. See `QueryGroup{TRecord}`. The value when the URL holds none of the keys; each property left out of the URL when equal to its value here. A name for the claiming component, used in the message when two groups use the same key. The `RendererInfo` of the component: nothing is written unless it is interactive. The prefix of the keys (`users` gives `users.q`), or null for none. The history of changes made through `Set(TRecord)`. |
| `Dispose()` | `void` | Stops following the URL. The scope does it at its end. |
| `Handler` | `Action<IReadOnlyList<string>>` | — |
| `Matches(string key)` | `bool` | — |
| `Dispose()` | `void` | — |

### `class QueryClaim`
*Package*: `Lux.Query` · *Source*: `src/Lux.Query/QueryState/QueryState.cs`

A claim on a prefix of the query keys, or on the keys of a group. Dispose it to release them.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Prefix` | `string` | The claimed prefix. |
| `Owner` | `string` | The name of the claiming component. |
| `ThrowIfConflicted()` | `void` | Throws when this claim turned out to overlap one that is still alive once the render that created it was over. The conflict, naming both owners. |
| `Dispose()` | `void` | Releases the claim. |

### `class QueryValue<T>`
*Package*: `Lux.Query` · *Source*: `src/Lux.Query/QueryState/QueryValue.cs`

One query key as a typed value. A key that is absent, or whose text the codec cannot read, gives the default; a value equal to the default is written as no key at all, so the URL of an untouched page stays clean. Make one with `Bind{T}`, and dispose it with the component that owns it. The type of the value.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Key` | `string` | The query key. |
| `Default` | `T` | The value of an absent or unreadable key. |
| `History` | `QueryHistory` | The history of changes made through `Value`. |
| `Value` | `T` | The value the URL holds, with the writes of the current handler on top. Setting it writes the URL after the handler yields (see `QueryState`); the getter shows the new value at once. |
| `Set(T value, QueryHistory history)` | `bool` | Sets the value with an explicit history. The new value. Whether the change adds a history entry. False when nothing was written because the renderer is not interactive. |
| `HrefWith(T value)` | `string` | The URL of the current page with this key set to `value` (see `HrefWith(IEnumerable{KeyValuePair{string, string?}})`). The value the link carries. |
| `Dispose()` | `void` | Stops following the URL. |
