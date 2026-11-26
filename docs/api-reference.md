# Klyroo.Query API reference

> Generated from the compiled assemblies and their XML documentation. Do not edit by hand.

## Klyroo.Query.Core

Static classes and extensions (4) · Classes (12) · Records (27) · Interfaces (1) · Enums (8) · Delegates (3)

### Static classes and extensions

#### `DataListStateQuery`

`Klyroo.Query` · service

Converts a `DataListState` to and from URL query keys, as plain functions (no Blazor, no `NavigationManager`), under an optional prefix joined with a dot (`users.q`). Values equal to the defaults of the schema are left out, and bad input is dropped, never thrown.

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static FromQuery(IEnumerable<KeyValuePair<string, string?>> query, DataListSchema schema, string? prefix = null)` | `DataListState` | Reads a state from query keys. Starts from the schema defaults, so an empty query gives `DataListSchema.DefaultState`. Never throws on bad input: unknown filter keys, operators the filter does not allow, values that do not parse, unknown or repeated sort keys and an unknown layout are dropped (the default applies); the page is clamped to 1..`DataListState.MaxPage` and the page size to 1..`DataListSchema.MaxPageSize`. |
| `static FromQuery(IEnumerable<KeyValuePair<string, string?>> query, DataListSchema schema, string? prefix, string? timeZone)` | `DataListState` | `DataListStateQuery.FromQuery` for a request that has a time zone: a date-time in a filter value without an offset (`2026-11-05T14:00`, what the `datetime-local` field of a static form posts) is the wall clock of that zone, not UTC. A date (`2026-11-05`) and a value with an offset do not depend on it. |
| `static ToQuery(DataListState state, DataListSchema schema, string? prefix = null)` | `IReadOnlyDictionary<string, string>` | Writes the keys of a state that differ from the defaults. Rules the schema does not accept are left out, so the result always reads back. |

#### `InMemoryDataList`

`Klyroo.Query` · service

Runs a `DataListRequest` against an in-memory list: search, filter tree, stable multi-key sort, offset or cursor paging, total count and facets. The predicate is built as an expression (see the internal `FilterExpressions`) and compiled, so the IQueryable applier can reuse the same definition of every operator.

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static ExecuteInMemory<T>(DataListSchema<T> schema, IEnumerable<T> items, DataListRequest request)` | `DataListResult<T>` | Executes a request. The request is first passed through `DataListSchema.Sanitize`. Query-only filters are skipped. Rows with a null along a field path (`e.Department.Name` with no department) are read as empty rather than throwing. |
| `static InMemoryProvider<T>(DataListSchema<T> schema, IEnumerable<T> items)` | `DataListProvider<T>` | A `DataListProvider` over a list, for a data list whose rows are in memory. |

#### `QueryableDataList`

`Klyroo.Query` · service

Applies a `DataListRequest` to an `IQueryable` of any provider (EF Core, for one) without referencing it: the filter tree, the search, the sort and the paging become `Queryable` operators the provider translates to SQL. The caller runs the queries (`CountAsync`, `ToListAsync`), so the package needs no EF reference.

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static Apply<T>(DataListSchema<T> schema, IQueryable<T> source, DataListRequest request)` | `DataListQuery<T>` | Builds the queries of a request. The request is first passed through `DataListSchema.Sanitize`, and query-only filters are skipped (read them with `DataListSchema.QueryFilterRules` and apply them to `source` before this call). Rows are ordered by the sort (the schema default when the request has none), nulls last in both directions, then by the row key when the schema has one, so that pages never repeat or skip a row. Cursor paging is a keyset `WHERE` over the sort values and the row key. |
| `static FacetQueries<T>(DataListSchema<T> schema, IQueryable<T> source, DataListRequest request)` | `IReadOnlyList<DataListFacetQuery<T>>` | The count queries of the requested facets (the sanitized ones: option and boolean fields), one per facet, for the caller to run. A facet counts the rows that match the search and the filters, leaving out the facet's own filter in the default `FacetMode.ExcludeSelf` mode. |
| `static ToResult<T>(DataListSchema<T> schema, DataListRequest request, IReadOnlyList<T> pageRows, int? totalCount = null, IReadOnlyList<DataListFacetResult>? facets = null)` | `DataListResult<T>` | The `DataListResult` of an offset or unpaged request from what the caller loaded: the rows of `DataListQuery.Page`, the count of `DataListQuery.Matching` and the facets (`DataListFacetQuery.ToResult`). |
| `static ToResult<T>(DataListSchema<T> schema, DataListRequest request, IReadOnlyList<DataListRow<T>> rows, int? totalCount = null, IReadOnlyList<DataListFacetResult>? facets = null)` | `DataListResult<T>` | The `DataListResult` of any request from the rows of `DataListQuery.Rows`. With cursor paging the extra row is removed and `DataListResult.NextCursor` is made from the sort values and the key that the database returned for the last row of the page. |

#### `QueryCodecs`

`Klyroo.Query` · service

The built-in `IQueryCodec`s. Numbers and dates use the invariant culture, so a URL means the same thing on every machine. A codec never throws on input: text that is not a value makes `IQueryCodec.TryParse` return false.

| Property | Type | Description |
| :--- | :--- | :--- |
| `static Boolean` | `IQueryCodec<bool>` | "true" or "false", in any case. |
| `static DateOnly` | `IQueryCodec<DateOnly>` | A date as `yyyy-MM-dd`. |
| `static DateTimeOffset` | `IQueryCodec<DateTimeOffset>` | A moment in the round-trip format ("O", for example `2026-01-02T03:04:05.0000000+01:00`). A URL that carries the offset unescaped ("+01:00") reaches the server with a space where the plus sign was: that space is read as the plus sign. |
| `static Decimal` | `IQueryCodec<decimal>` | A decimal with a dot as the separator ("12.50"); no exponent and no thousands separator. |
| `static Double` | `IQueryCodec<double>` | A finite double with a dot as the separator; "NaN" and "Infinity" are not values (`IQueryCodec.Format` leaves them out). |
| `static Int32` | `IQueryCodec<int>` | A 32-bit integer: an optional sign and digits, nothing else. |
| `static Int64` | `IQueryCodec<long>` | A 64-bit integer: an optional sign and digits, nothing else. |
| `static String` | `IQueryCodec<string>` | Text as is. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static Array<T>(IQueryCodec<T> item, char separator = ,)` | `IQueryCodec<T[]>` | A list: the items joined by `separator` (a comma). The characters that would be mistaken for structure are written as percent escapes inside an item: the separator, `~` and `%`; an empty item is a lone `~`. An empty list is left out of the URL. |
| `static Enum<TEnum>(bool lowercase = false)` | `IQueryCodec<TEnum>` | An enum by member name, case-insensitive on input. Numbers and names that are not members are not values. A `[Flags]` enum is written as its names separated by commas (`Read,Write`) and read back the same way; a combination with a bit that has no name is left out of the URL. |
| `static Optional<T>(IQueryCodec<T> item)` | `IQueryCodec<T?>` | Makes the codec of a struct a codec of its nullable: a null value is left out of the URL. |
| `static Range<T>(IQueryCodec<T> item)` | `IQueryCodec<QueryRange<T>>` | A range written `from..to`; either end can be missing ("5..", "..9"). A range with no end is left out of the URL. |

### Classes

#### `DataListCollectionFieldBuilder<T, TProp>`

`Klyroo.Query` · class

The options of a collection field, passed to `DataListSchemaBuilder.Collection``. There is no `Sortable`: a collection has no single value to sort by.

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Filter(DataListFilter filter)` | `DataListCollectionFieldBuilder<T, TProp>` | Gives the field a filter; the kind must suit the element value type. |
| `Key(string key)` | `DataListCollectionFieldBuilder<T, TProp>` | Overrides the stable key (`tags.name` by default). |
| `StoresLocalTime(bool localTime = true)` | `DataListCollectionFieldBuilder<T, TProp>` | The `DateTime` values read from the elements are wall-clock times in the request's time zone, not UTC. See `DataListFieldBuilder.StoresLocalTime`. |

#### `DataListFacetBucket`

`Klyroo.Query` · class

One option of a facet as read from the database: the value in its own type, and the number of rows that have it.

| Property | Type | Description |
| :--- | :--- | :--- |
| `Count` (get, set) | `int` | The number of rows that have the value (a row counts once per distinct value, whatever the number of elements that carry it). |
| `Value` (get, set) | `object?` | The value of the field, boxed, never null or an empty string. |

#### `DataListFacetQuery<T>`

`Klyroo.Query` · class

The queries that count one facet, for the caller to run, and the code that turns their rows into a `DataListFacetResult`.

| Property | Type | Description |
| :--- | :--- | :--- |
| `Buckets` | `IQueryable<DataListFacetBucket>` | A group-by query: one row per distinct non-empty value and its count, the most counted first (ties by value), at most `DataListSchema.MaxFacetBuckets` plus one (the extra row tells `DataListFacetQuery.ToResult` that the facet is truncated). Load it with `ToListAsync()`; the search among the options and the window are applied by `DataListFacetQuery.ToResult`. |
| `Key` | `string` | The key of the facet. |
| `Missing` | `IQueryable<T>?` | The rows where the field is empty, or `null` when the request did not ask for `DataListFacetRequest.IncludeMissing`: count them with `CountAsync()`. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `ToResult(IEnumerable<DataListFacetBucket> buckets, int? missing = null)` | `DataListFacetResult` | The facet as the in-memory executor would answer it: options as text, searched, ordered by count then value (ordinal), then cut by `Skip` and `Limit`. |

#### `DataListFieldBuilder<T, TProp>`

`Klyroo.Query` · class

The options of one field, passed to `DataListSchemaBuilder.Field``.

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Filter(DataListFilter filter)` | `DataListFieldBuilder<T, TProp>` | Gives the field a filter. The filter kind must suit the type of the field (text on a string, number on a numeric type, and so on). |
| `Key(string key)` | `DataListFieldBuilder<T, TProp>` | Overrides the stable key. Needed for fields that are not a plain member path. |
| `Sortable(bool sortable = true)` | `DataListFieldBuilder<T, TProp>` | Lets requests sort by this field. |
| `StoresLocalTime(bool localTime = true)` | `DataListFieldBuilder<T, TProp>` | Declares that the `DateTime` values of this field are wall-clock times in the time zone of the request, not UTC. Only for `DateTime` fields. |

#### `DataListFilter`

`Klyroo.Query` · class

The definition of a filter: its kind, the operators it allows, its default operator and default value, and the limits an editor shows. Immutable; the `With...` methods return a changed copy. Create one with `DataListFilter.Text`, `DataListFilter.Option`, `DataListFilter.Boolean`, `DataListFilter.Number` or `DataListFilter.Date` and attach it to a field (`DataListFieldBuilder.Filter`) or register it as a query-only filter (`DataListSchemaBuilder.QueryFilter`).

| Property | Type | Description |
| :--- | :--- | :--- |
| `AllowedOperators` | `IReadOnlyList<FilterOperator>` | The operators a rule of this filter may use. The per-kind set of the spec, or the subset given to `DataListFilter.WithOperators`. |
| `DefaultIncludeEmpty` | `bool` | Whether the default rule also includes empty rows. |
| `DefaultOperator` | `FilterOperator` | The operator a rule gets when none is given, and the one a URL key without an operator suffix means. |
| `DefaultValue` | `FilterValue?` | The value effective when the URL has no override; omitted from the URL and not counted as active. `null` for none. |
| `DefaultValueOperator` | `FilterOperator` | The operator of the default rule (`DataListFilter.DefaultOperator` unless `DataListFilter.WithDefault` set another). |
| `FacetMode` | `FacetMode` | Whether the counts leave out this filter's own rule (`FacetMode.ExcludeSelf`, the default: the other options stay visible) or keep it (`FacetMode.IncludeSelf`). |
| `Facets` | `bool` | Whether the data list asks for the counts of this filter (the rows that have each option, or each yes and no) and shows them in the editor. Only for option and boolean filters. |
| `Granularity` | `DateGranularity` | The precision of a date editor; decides whether values are `Date` or `Moment`. |
| `Kind` | `FilterKind` | The kind of filter. |
| `Label` | `string?` | The name of the filter in the interface (the tag, the add filter menu). `null` falls back to the title of the column with the same key, then to the key written as words. |
| `Max` | `decimal?` | The largest value an editor offers (number filters). |
| `MaxRangeDays` | `int?` | The most days a range of a date editor may cover, both ends included. `null` for no limit. This limits the editor only (the calendar, the presets and the typed ends): a range in the URL, a bound state or a request is not checked against it, and neither is it on the server. |
| `Min` | `decimal?` | The smallest value an editor offers (number filters). |
| `NumberDisplay` | `DataListNumberDisplay` | How a number editor shows its value: a field, a slider or both. Only number filters; a slider needs `DataListFilter.Min` and `DataListFilter.Max`, without them the editor shows the field. |
| `NumberFormat` | `string?` | The .NET numeric format string the filter tag writes a number with (`N0`, `C2`, `0.0 'kg'`), in the culture of the user. It formats the tag only, not the field of the editor. `null` writes the number with its digits and the group separator of the culture. |
| `OptionLimit` | `int?` | How many options the editor lists before a "show more" button, or `null` to list them all. The selected options always show. A statically rendered editor lists them all. |
| `Options` | `IReadOnlyList<DataListFilterOption>` | The options an option filter offers, in order; empty for the other kinds. Without them the editor lists the values of the facet counts (`DataListFilter.Facets`) and the values already selected. |
| `OptionSort` | `DataListOptionSort?` | How the editor orders the options. `null` keeps the declared options in their order and lists the options that come from counts by count (the order of the facet). Not used for remote options, which keep the order of their source. |
| `Persist` | `bool` | Whether the user's last value is remembered between visits, restored only when the URL has no value. A hint for a list component: the executors and the URL codecs ignore it. `KlyDataList` of Klyroo.Table keeps the value in the browser's `localStorage`, in the URL form of `DataListStateQuery`, and restores it on its first interactive render. |
| `RemoteOptions` | `DataListRemoteOptions?` | Where the options come from when they are loaded from a remote source, page by page, with a search. `null` for declared options and for options taken from the counts. |
| `Step` | `decimal?` | The step of a number editor. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static Boolean(bool? defaultValue = null)` | `DataListFilter` | A boolean filter: `is` (default), `isNot`. |
| `static Date(DateGranularity granularity = Day)` | `DataListFilter` | A date filter: `is` (default), `isNot`, `before`, `after`, `between`. |
| `DefaultRule(string key)` | `FilterRule?` | The default rule of this filter under `key`, or `null` when there is no default value. |
| `IsValid(FilterRule rule)` | `bool` | Whether `rule` is well formed for this filter: an allowed operator and a value of the right shape. Rules that fail are dropped by `DataListSchema.Sanitize`. |
| `static Number(decimal? min = null, decimal? step = null, decimal? max = null)` | `DataListFilter` | A number filter: `is` (default), `isNot`, `gt`, `gte`, `lt`, `lte`, `between`. |
| `static Option()` | `DataListFilter` | An option filter: `isAnyOf` (default), `is`, `isNot`. Works on strings, enums, numbers, booleans, Guids and dates. |
| `static Option(params DataListFilterOption[] options)` | `DataListFilter` | An option filter with the options its editor lists (value and label). |
| `static Option(params string[] values)` | `DataListFilter` | An option filter that lists `values`, each labelled by itself. |
| `static Text()` | `DataListFilter` | A text filter: `contains` (default), `is`, `isNot`. |
| `WithDefault(FilterValue value, FilterOperator? op = null, bool includeEmpty = false)` | `DataListFilter` | Sets the default rule: effective without a URL override, omitted from the URL. |
| `WithDefaultOperator(FilterOperator op)` | `DataListFilter` | Sets the operator used when none is given. It must be allowed. |
| `WithFacetMode(FacetMode mode)` | `DataListFilter` | Chooses whether the counts of this filter keep its own rule (`FacetMode.IncludeSelf`) or leave it out (`FacetMode.ExcludeSelf`, the default). |
| `WithFacets(bool facets = true)` | `DataListFilter` | Asks the data list for the counts of this filter and shows them next to the options (or next to Yes and No). By default the counts leave out this filter's own rule, so the other options stay visible (see `DataListFilter.WithFacetMode`). The counts are asked for when the filters or the search change, and kept while the user pages or sorts. |
| `WithLabel(string label)` | `DataListFilter` | Names the filter in the interface. |
| `WithMaxRangeDays(int days)` | `DataListFilter` | Limits the days a range of a date editor may cover (both ends included). Only date filters. An editor limit only: the URL and the server do not enforce it. |
| `WithNumberDisplay(DataListNumberDisplay display)` | `DataListFilter` | Chooses how a number editor shows its value. A slider needs `DataListFilter.Min` and `DataListFilter.Max`; without them the editor shows the field. |
| `WithNumberFormat(string format)` | `DataListFilter` | Sets the format the filter tag writes a number with (the tag only, not the editor). Only number filters. |
| `WithOperators(params FilterOperator[] operators)` | `DataListFilter` | Restricts the allowed operators. Each must belong to the kind's set, except `FilterOperator.IsEmpty` and `FilterOperator.IsNotEmpty`, which any kind may add. Resets the default operator to the first one when the old default is no longer allowed. |
| `WithOptionLimit(int limit)` | `DataListFilter` | Lists only the first options of an option filter, with a "show more" button for the others. The selected options always show. |
| `WithOptions(params DataListFilterOption[] options)` | `DataListFilter` | Sets the options the editor lists. Only for option filters. |
| `WithOptionSort(DataListOptionSort sort)` | `DataListFilter` | Orders the declared options and the counted options of an option filter by count or by label (`DataListFilter.OptionSort`). Remote options keep the order of their source: the sort is ignored for them. |
| `WithPersist(bool persist = true)` | `DataListFilter` | Remembers the last value of this filter between visits (see `DataListFilter.Persist`): a list component stores it and restores it when the URL has no value for the filter. |
| `WithRemoteOptions(DataListOptionsProvider provider, DataListOptionsResolver? resolveSelected = null, int pageSize = 25, int minLength = 0)` | `DataListFilter` | Loads the options from a remote source, page by page and with a search, instead of declaring them. The editor of a UI library asks for a page when it is shown, searches while the user types and loads the next page as the list scrolls. |

#### `DataListJsonContext`

`Klyroo.Query` · class · inherits `JsonSerializerContext`

System.Text.Json source-generated metadata for the wire records (request, filter tree, values, facets, state), so they serialize without reflection and survive trimming. Polymorphic types carry a `type` discriminator; enums are strings; names are camelCase. `DataListResult` is generic: declare `[JsonSerializable(typeof(DataListResult<MyRow>))]` in the context of your own app, which can reuse these types.

| Property | Type | Description |
| :--- | :--- | :--- |
| `Boolean` | `JsonTypeInfo<bool>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `Cursor` | `JsonTypeInfo<DataListPaging.Cursor>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `DataListFacetOption` | `JsonTypeInfo<DataListFacetOption>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `DataListFacetRequest` | `JsonTypeInfo<DataListFacetRequest>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `DataListFacetResult` | `JsonTypeInfo<DataListFacetResult>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `DataListFilterGroup` | `JsonTypeInfo<DataListFilterGroup>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `DataListFilterNode` | `JsonTypeInfo<DataListFilterNode>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `DataListLayout` | `JsonTypeInfo<DataListLayout>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `DataListPaging` | `JsonTypeInfo<DataListPaging>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `DataListRequest` | `JsonTypeInfo<DataListRequest>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `DataListSearch` | `JsonTypeInfo<DataListSearch>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `DataListSort` | `JsonTypeInfo<DataListSort>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `DataListState` | `JsonTypeInfo<DataListState>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `Date` | `JsonTypeInfo<FilterValue.Date>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `DateOnly` | `JsonTypeInfo<DateOnly>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `DateTimeOffset` | `JsonTypeInfo<DateTimeOffset>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `Decimal` | `JsonTypeInfo<decimal>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `static Default` | `DataListJsonContext` | The default `JsonSerializerContext` associated with a default `JsonSerializerOptions` instance. |
| `DictionaryStringDictionaryStringString` | `JsonTypeInfo<Dictionary<string, Dictionary<string, string>>>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `DictionaryStringString` | `JsonTypeInfo<Dictionary<string, string>>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `FacetMode` | `JsonTypeInfo<FacetMode>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `FilterCombinator` | `JsonTypeInfo<FilterCombinator>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `FilterOperator` | `JsonTypeInfo<FilterOperator>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `FilterRule` | `JsonTypeInfo<FilterRule>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `FilterValue` | `JsonTypeInfo<FilterValue>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `FilterValueBoolean` | `JsonTypeInfo<FilterValue.Boolean>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `Int32` | `JsonTypeInfo<int>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `IReadOnlyListDataListFacetOption` | `JsonTypeInfo<IReadOnlyList<DataListFacetOption>>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `IReadOnlyListDataListFacetRequest` | `JsonTypeInfo<IReadOnlyList<DataListFacetRequest>>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `IReadOnlyListDataListFilterNode` | `JsonTypeInfo<IReadOnlyList<DataListFilterNode>>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `IReadOnlyListDataListSort` | `JsonTypeInfo<IReadOnlyList<DataListSort>>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `IReadOnlyListFilterRule` | `JsonTypeInfo<IReadOnlyList<FilterRule>>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `IReadOnlyListString` | `JsonTypeInfo<IReadOnlyList<string>>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `List` | `JsonTypeInfo<FilterValue.List>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `Moment` | `JsonTypeInfo<FilterValue.Moment>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `None` | `JsonTypeInfo<DataListPaging.None>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `NullableInt32` | `JsonTypeInfo<int?>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `Number` | `JsonTypeInfo<FilterValue.Number>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `Offset` | `JsonTypeInfo<DataListPaging.Offset>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `Range` | `JsonTypeInfo<FilterValue.Range>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `String` | `JsonTypeInfo<string>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `StringArray` | `JsonTypeInfo<string[]>` | Defines the source generated JSON serialization contract metadata for a given type. |
| `Text` | `JsonTypeInfo<FilterValue.Text>` | Defines the source generated JSON serialization contract metadata for a given type. |

#### `DataListQuery<T>`

`Klyroo.Query` · class

The two queries of a request, for the caller to run: `DataListQuery.Matching` counts the rows (`CountAsync()`) and `DataListQuery.Page` loads the page (`ToListAsync()`). Neither runs anything: they are plain `IQueryable` built from `Queryable` operators.

| Property | Type | Description |
| :--- | :--- | :--- |
| `Matching` | `IQueryable<T>` | The rows that match the filter and the search, unordered and unpaged: count them to get the total. |
| `Page` | `IQueryable<T>` | The page: `DataListQuery.Matching` sorted (nulls last in both directions, then by row key) and cut by the paging. With cursor paging it holds one row more than the page size, which `QueryableDataList`.ToResult uses to know that a next page exists and removes. |
| `Rows` | `IQueryable<DataListRow<T>>` | `DataListQuery.Page` with each row's sort values and key read by the database next to it (`DataListRow`). Load this one for cursor paging: the next cursor is made from these values, not from the loaded entity, so a sort on `e.Department.Name` needs no `Include` and a projected row type needs nothing special. For offset paging or no paging, `DataListQuery.Page` is enough. |
| `WantsTotal` | `bool` | Whether the result carries a total: always for offset and no paging, for a cursor only when the request asked (`Count`). Count `DataListQuery.Matching` only then. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Deconstruct(out IQueryable<T> matching, out IQueryable<T> page)` | `void` | Splits the query into its two parts: `var (matching, page) = schema.Apply(...)`. |

#### `DataListRemoteOptions`

`Klyroo.Query` · class

The remote source of the options of an option filter. See `DataListFilter.WithRemoteOptions`.

| Field | Type | Description |
| :--- | :--- | :--- |
| `const DefaultPageSize = 25` | `int` | The page size when none is given. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `MinLength` | `int` | The shortest search that is sent. |
| `PageSize` | `int` | How many options each page asks for. |
| `Provider` | `DataListOptionsProvider` | Loads a page of options. |
| `Resolver` | `DataListOptionsResolver?` | Labels the values that no loaded page holds, or `null` when values show as they are. |

#### `DataListRow<T>`

`Klyroo.Query` · class

A row of a page together with what the database computed for its cursor: the value of each sort field, in the order of the sort, and the row key.

| Property | Type | Description |
| :--- | :--- | :--- |
| `Item` (get, set) | `T` | The row. |
| `Key` (get, set) | `object?` | The row key, boxed. |
| `SortValues` (get, set) | `object?[]` | The sort values, boxed, one per sort rule (a null is a null in the database). |

#### `DataListSchema`

`Klyroo.Query` · class

Describes what a data list can do with a row type: its fields, which are sortable or filterable, the search fields, the query-only filters, the defaults and the limits. Built in C# with expressions (no reflection over string paths, so it is trim-safe) through `DataListSchema.For``. The schema decides what a request may ask: `DataListSchema.Sanitize` drops anything it does not know.

| Field | Type | Description |
| :--- | :--- | :--- |
| `const MaxFilterDepth = 16` | `int` | The deepest nesting of filter groups kept by `DataListSchema.Sanitize`; deeper groups are dropped. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `DefaultLayout` | `DataListLayout` | The layout when none is given. |
| `DefaultPageSize` | `int` | The page size when none is given (50 unless set, never above `DataListSchema.MaxPageSize`). |
| `DefaultSort` | `IReadOnlyList<DataListSort>` | The sort applied when a request has none; omitted from the URL. |
| `DefaultState` | `DataListState` | The state with every default applied: the default filter values, sort, page 1, `DataListSchema.DefaultPageSize` and `DataListSchema.DefaultLayout`. The URL representation omits whatever equals it. |
| `FilterKeys` | `IReadOnlyList<string>` | The keys of every filter (fields and query-only filters), in declaration order. |
| `HasSearch` | `bool` | Whether the schema has search fields. |
| `MaxFacetBuckets` | `int` | The most distinct options a facet keeps (1000 unless set): the ones with the highest counts, ties by value. A facet over more values is cut and marked `DataListFacetResult.Truncated`; the search among the options and the window (`Skip`, `Limit`) apply to the kept ones. In a database the cap is a `TAKE` on the group-by, so a column with millions of values never transfers them all. |
| `MaxFilterRules` | `int` | The most filter rules `DataListSchema.Sanitize` keeps (100 unless set); later ones are dropped. Bounds the work a hostile request can ask for. |
| `MaxListValues` | `int` | The most values `DataListSchema.Sanitize` keeps in the list of one rule (500 unless set); later ones are dropped. |
| `MaxPageSize` | `int` | The largest page a request may ask for (500 unless set). |
| `SortKeys` | `IReadOnlyList<string>` | The keys of the sortable fields. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static For<T>(Action<DataListSchemaBuilder<T>> configure)` | `DataListSchema<T>` | Starts the schema of a row type. See `DataListSchemaBuilder`. |
| `IsQueryFilter(string key)` | `bool` | Whether `key` is a query-only filter: part of the request for the provider, never matched against rows by the in-memory executor. |
| `QueryFilterRules(DataListRequest request)` | `IReadOnlyList<FilterRule>` | The query-only rules of a request, for the provider to interpret. Looks through nested groups. |
| `Sanitize(DataListRequest request)` | `DataListRequest` | Returns a request a provider can trust: what the schema does not allow is dropped, and sizes and counts are bounded. |
| `TryGetFilter(string key, out DataListFilter? filter)` | `bool` | Gets the definition of a filter by key. |

#### `DataListSchema<T>`

`Klyroo.Query` · class · inherits `DataListSchema`

The schema of a row type `T`: a `DataListSchema` that can also read a row's key and label and run requests in memory.

| Property | Type | Description |
| :--- | :--- | :--- |
| `HasRowKey` | `bool` | Whether the schema has a `RowKey`. Cursor paging needs one. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `GetRowKey(T item)` | `string` | The stable key of a row, as text (invariant culture). |
| `GetRowLabel(T item)` | `string?` | The label that names a row for accessibility ("Select Alice"), or `null`. |

#### `DataListSchemaBuilder<T>`

`Klyroo.Query` · class

The fluent description of a `DataListSchema`, passed to `DataListSchema.For``.

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Collection<TElem, TProp>(Expression<Func<T, IEnumerable<TElem>?>> collection, Expression<Func<TElem, TProp>> element, Action<DataListCollectionFieldBuilder<T, TProp>>? configure = null)` | `DataListSchemaBuilder<T>` | Registers a field that reads one value from each element of a collection of the row: `.Collection(e => e.Tags, t => t.Name, f => f.Filter(DataListFilter.Option()))`. The key joins both paths in camelCase (`tags.name`). |
| `DefaultLayout(DataListLayout layout)` | `DataListSchemaBuilder<T>` | The layout when none is given (table by default). |
| `DefaultPageSize(int size)` | `DataListSchemaBuilder<T>` | The page size when none is given (50 by default, never above the maximum). |
| `DefaultSort<TProp>(Expression<Func<T, TProp>> selector, bool descending = false)` | `DataListSchemaBuilder<T>` | The sort used when a request has none. The field is made sortable (and registered when missing). |
| `DefaultSort(string key, bool descending = false)` | `DataListSchemaBuilder<T>` | The sort used when a request has none, by the key of a sortable field. |
| `Field<TProp>(Expression<Func<T, TProp>> selector, Action<DataListFieldBuilder<T, TProp>>? configure = null)` | `DataListSchemaBuilder<T>` | Registers a field. Its key is the member path in camelCase (`department.name`) unless `DataListFieldBuilder.Key` says otherwise. |
| `MaxFacetBuckets(int count)` | `DataListSchemaBuilder<T>` | The most distinct options a facet keeps (1000 by default); more are cut and the facet is marked truncated. See `DataListSchema.MaxFacetBuckets`. |
| `MaxFilterRules(int count)` | `DataListSchemaBuilder<T>` | The most filter rules a sanitized request keeps (100 by default). |
| `MaxListValues(int count)` | `DataListSchemaBuilder<T>` | The most values a sanitized request keeps in the list of one rule (500 by default). |
| `MaxPageSize(int size)` | `DataListSchemaBuilder<T>` | The largest page a request may ask for (500 by default). |
| `QueryFilter(string key, DataListFilter filter)` | `DataListSchemaBuilder<T>` | A filter that is not a field of the row: its rules reach the provider (`DataListSchema.QueryFilterRules`) but the in-memory executor never matches rows against them. Use it for server-side switches such as `archived`. |
| `RowKey<TKey>(Expression<Func<T, TKey>> selector)` | `DataListSchemaBuilder<T>` | The stable identity of a row, used for selection and as the cursor tie-break. Converted to text in the invariant culture. |
| `RowLabel(Func<T, string?> label)` | `DataListSchemaBuilder<T>` | The label that names a row for assistive technology ("Select Alice"). |
| `Search(params Expression<Func<T, string?>>[] fields)` | `DataListSchemaBuilder<T>` | The string fields the search box looks in (case-insensitive substring, any field matches). The expressions are compiled for in-memory evaluation and passed as written to a database provider. |
| `SearchAny<TElem>(Expression<Func<T, IEnumerable<TElem>?>> collection, Expression<Func<TElem, string?>> element)` | `DataListSchemaBuilder<T>` | The search also looks at a string of each element of a collection (any element matches). |

### Records

#### `FilterValue.Boolean`

`Klyroo.Query` · record · inherits `FilterValue`

A boolean.

| Constructor | Description |
| :--- | :--- |
| `Boolean(bool Value)` | A boolean. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Value` (init) | `bool` | The flag. |

#### `DataListPaging.Cursor`

`Klyroo.Query` · record · inherits `DataListPaging`

Keyset paging: the page after an opaque cursor.

| Constructor | Description |
| :--- | :--- |
| `Cursor(string? After, int Take, bool Count = false)` | Keyset paging: the page after an opaque cursor. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `After` (init) | `string?` | The cursor of the previous page (`DataListResult.NextCursor`), or `null` for the first page. |
| `Count` (init) | `bool` | Also compute `DataListResult.TotalCount`. |
| `Take` (init) | `int` | Page size. |

#### `DataListFacetOption`

`Klyroo.Query` · record

One option of a facet and the number of rows that have it.

| Constructor | Description |
| :--- | :--- |
| `DataListFacetOption(string Value, int Count)` | One option of a facet and the number of rows that have it. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Count` (init) | `int` | The row count. |
| `Value` (init) | `string` | The value as text: invariant culture, `true`/`false`, enum name, ISO dates. |

#### `DataListFacetRequest`

`Klyroo.Query` · record

Asks for the option counts of one filter.

| Constructor | Description |
| :--- | :--- |
| `DataListFacetRequest(string Key, FacetMode Mode = ExcludeSelf, int? Limit = null, int Skip = 0, string? Search = null, bool IncludeMissing = false, IReadOnlyList<string>? Values = null)` | Asks for the option counts of one filter. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `IncludeMissing` (init) | `bool` | Also count the rows where the field is empty. |
| `Key` (init) | `string` | The key of a field with an option or boolean filter. |
| `Limit` (init) | `int?` | Maximum options returned; `null` for all. |
| `Mode` (init) | `FacetMode` | Whether the facet's own filter applies. |
| `Search` (init) | `string?` | Keeps only the options whose value contains this text, ignoring case. |
| `Skip` (init) | `int` | Options to skip (for loading more). |
| `Values` (init) | `IReadOnlyList<string>?` | Counts only these values (as text, the way a rule carries them), whatever `DataListSchema.MaxFacetBuckets` says: a remote option filter asks for the values of the pages it has loaded. A value that no row has is not in the result (a reader takes it as 0). `null` or empty counts every value. |

#### `DataListFacetResult`

`Klyroo.Query` · record

The counts of one facet.

| Constructor | Description |
| :--- | :--- |
| `DataListFacetResult(string key, IReadOnlyList<DataListFacetOption> options, int? missing = null, int total = 0)` | Creates a facet result. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Key` (init) | `string` | The key of the filter. |
| `Missing` (init) | `int?` | Rows where the field is empty, when requested. |
| `Options` (init) | `IReadOnlyList<DataListFacetOption>` | The options, by count descending then value (ordinal), after `Search`, `Skip` and `Limit`. |
| `Total` (init) | `int` | The number of distinct options before `Skip` and `Limit` (after `Search`); more are available when `Skip + Options.Count < Total`. |
| `Truncated` (init) | `bool` | The facet has more distinct values than `DataListSchema.MaxFacetBuckets`: the options are the ones with the highest counts, and the others are not listed or counted in `DataListFacetResult.Total`. A UI can say "more values". |

#### `DataListFilterGroup`

`Klyroo.Query` · record · inherits `DataListFilterNode`

A group of filter nodes combined with AND or OR. An empty group matches every row.

| Constructor | Description |
| :--- | :--- |
| `DataListFilterGroup(FilterCombinator combinator, IReadOnlyList<DataListFilterNode> children)` | Creates a group. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Children` (init) | `IReadOnlyList<DataListFilterNode>` | The children. |
| `Combinator` (init) | `FilterCombinator` | How the children combine. |
| `static Empty` | `DataListFilterGroup` | The group that matches everything. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static And(params DataListFilterNode[] children)` | `DataListFilterGroup` | An AND group of the given nodes. |
| `static Or(params DataListFilterNode[] children)` | `DataListFilterGroup` | An OR group of the given nodes. |

#### `DataListFilterNode`

`Klyroo.Query` · record

A node of the filter tree: a `FilterRule` or a `DataListFilterGroup`.

#### `DataListFilterOption`

`Klyroo.Query` · record

One option of an option filter: the value that goes into the rule and the URL, and what the interface shows.

| Constructor | Description |
| :--- | :--- |
| `DataListFilterOption(string Value, string Label)` | One option of an option filter: the value that goes into the rule and the URL, and what the interface shows. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Label` (init) | `string` | The text shown to the user. |
| `Value` (init) | `string` | The value as text, the way a `List` carries it (an enum name, a number, `true`). |

#### `DataListOptionsPage`

`Klyroo.Query` · record

One page of options loaded from a remote source.

| Constructor | Description |
| :--- | :--- |
| `DataListOptionsPage(IReadOnlyList<DataListFilterOption> Items, bool HasMore = false, string? NextCursor = null)` | One page of options loaded from a remote source. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `HasMore` (init) | `bool` | Whether another page exists. A non-null `DataListOptionsPage.NextCursor` also means there is one. |
| `Items` (init) | `IReadOnlyList<DataListFilterOption>` | The options of this page, in the order to list them. A value that an earlier page of the same search already listed is skipped. |
| `NextCursor` (init) | `string?` | The cursor that asks for the page after this one; `null` when the source pages by `Skip` and `Take` or this is the last page. |

#### `DataListOptionsRequest`

`Klyroo.Query` · record

What a `DataListOptionsProvider` is asked for: one page of options for a search. Both kinds of paging are carried: a source that pages by position reads `DataListOptionsRequest.Skip` and `DataListOptionsRequest.Take`, one that pages with an opaque token reads `DataListOptionsRequest.Cursor`. The editor fills all of them.

| Constructor | Description |
| :--- | :--- |
| `DataListOptionsRequest(string Search, int Skip, int Take, string? Cursor)` | What a `DataListOptionsProvider` is asked for: one page of options for a search. Both kinds of paging are carried: a source that pages by position reads `DataListOptionsRequest.Skip` and `DataListOptionsRequest.Take`, one that pages with an opaque token reads `DataListOptionsRequest.Cursor`. The editor fills all of them. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Cursor` (init) | `string?` | The `DataListOptionsPage.NextCursor` of the previous page; `null` for the first page and for a source that does not use cursors. |
| `Search` (init) | `string` | The text typed in the search field of the editor, trimmed. Empty when nothing is typed: list the options unfiltered. |
| `Skip` (init) | `int` | How many options the earlier pages of this search returned (0 for the first page). |
| `Take` (init) | `int` | The number of options wanted. |

#### `DataListPaging`

`Klyroo.Query` · record

How a request pages: `Offset`, `Cursor` or `None`.

#### `DataListRequest`

`Klyroo.Query` · record

What a data list asks a provider for: a page of rows under a sort, a filter tree and a search, plus facet counts.

| Constructor | Description |
| :--- | :--- |
| `DataListRequest(DataListPaging paging, IReadOnlyList<DataListSort> sort, DataListFilterGroup filter, DataListSearch? search = null, IReadOnlyList<DataListFacetRequest>? facets = null, string? timeZone = null)` | Creates a request. Null lists are read as empty and a null `paging` as `None`. `timeZone` is the IANA id of the user's time zone, see `DataListRequest.TimeZone`. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `static Empty` | `DataListRequest` | A request for everything: no paging, sort, filter, search or facets. |
| `Facets` (init) | `IReadOnlyList<DataListFacetRequest>` | The facets to count. |
| `Filter` (init) | `DataListFilterGroup` | The filter tree (rules and query-only rules). |
| `Paging` (init) | `DataListPaging` | How to page. |
| `Search` (init) | `DataListSearch?` | The search, if any. |
| `Sort` (init) | `IReadOnlyList<DataListSort>` | The sort rules, most significant first; empty uses the schema default. |
| `TimeZone` (init) | `string?` | The IANA id of the user's time zone (`Europe/Paris`), sent by the client with each request; never part of the URL state. A `Date` on a `DateTimeOffset` or `DateTime` field means that day in this zone, and hour, minute and second units are cut in it. `null`, or an id the server does not know, means UTC (`DataListSchema.Sanitize` clears unknown ids). |

#### `DataListResult<T>`

`Klyroo.Query` · record

A page of rows and what goes with it.

| Constructor | Description |
| :--- | :--- |
| `DataListResult<T>(IReadOnlyList<T> Items, int? TotalCount = null, string? NextCursor = null, IReadOnlyList<DataListFacetResult>? Facets = null)` | A page of rows and what goes with it. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Facets` (init) | `IReadOnlyList<DataListFacetResult>?` | The requested facet counts. |
| `Items` (init) | `IReadOnlyList<T>` | The rows of the page. |
| `NextCursor` (init) | `string?` | The cursor of the next page, or `null` after the last one (cursor paging only). |
| `TotalCount` (init) | `int?` | Rows matching the filter and search across all pages; `null` when not computed. |

#### `DataListSearch`

`Klyroo.Query` · record

The search box: a substring searched, ignoring case, in every search field of the schema.

| Constructor | Description |
| :--- | :--- |
| `DataListSearch(string Term)` | The search box: a substring searched, ignoring case, in every search field of the schema. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Term` (init) | `string` | The text; blank matches everything. |

#### `DataListSort`

`Klyroo.Query` · record

One sort rule.

| Constructor | Description |
| :--- | :--- |
| `DataListSort(string Key, bool Descending = false)` | One sort rule. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Descending` (init) | `bool` | Sort from the largest to the smallest. Nulls always sort last. |
| `Key` (init) | `string` | The key of a sortable field. |

#### `DataListState`

`Klyroo.Query` · record

The state of a data list as the user left it: search, filters, sort, page, page size, layout and, for cursor paging, the cursor. Immutable. `DataListStateQuery` converts it to and from URL query keys.

| Constructor | Description |
| :--- | :--- |
| `DataListState(string search, IReadOnlyList<FilterRule> filters, IReadOnlyList<DataListSort> sort, int page, int pageSize, DataListLayout layout, string? cursor = null)` | Creates a state. Null lists are read as empty. |

| Field | Type | Description |
| :--- | :--- | :--- |
| `const MaxPage = 1000000` | `int` | The highest page number read from a URL. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Cursor` (init) | `string?` | The cursor of the current page in cursor mode (`c`), `null` on the first page. |
| `Filters` (init) | `IReadOnlyList<FilterRule>` | The filter rules, combined with AND (`f.<key>`). |
| `static Initial` | `DataListState` | The state with nothing applied: empty search, no filters or sort, page 1, 50 rows, table layout. |
| `Layout` (init) | `DataListLayout` | The layout (`l`). |
| `Page` (init) | `int` | The 1-based page (`p`). |
| `PageSize` (init) | `int` | The page size (`ps`). |
| `Search` (init) | `string` | The search text (`q`). |
| `Sort` (init) | `IReadOnlyList<DataListSort>` | The sort rules (`s`); the UI edits one. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `ToRequest(bool cursorPaging = false, IReadOnlyList<DataListFacetRequest>? facets = null, string? timeZone = null)` | `DataListRequest` | Builds the request for this state: offset paging at `DataListState.Page`, or cursor paging with `DataListState.Cursor` when `cursorPaging` is set. `timeZone` is the IANA id of the browser's time zone (`DataListRequest.TimeZone`); it comes from the client at request time, not from the state. |

#### `FilterValue.Date`

`Klyroo.Query` · record · inherits `FilterValue`

A calendar day. On `DateTimeOffset` and `DateTime` fields it is that day in the time zone of the request (`DataListRequest.TimeZone`, UTC when absent), as the half-open range from its first instant to the next day's. `DateOnly` fields compare the day itself.

| Constructor | Description |
| :--- | :--- |
| `Date(DateOnly Value)` | A calendar day. On `DateTimeOffset` and `DateTime` fields it is that day in the time zone of the request (`DataListRequest.TimeZone`, UTC when absent), as the half-open range from its first instant to the next day's. `DateOnly` fields compare the day itself. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Value` (init) | `DateOnly` | The day. |

#### `FilterRule`

`Klyroo.Query` · record · inherits `DataListFilterNode`

One condition on a field (or on a query-only filter, see `DataListSchemaBuilder.QueryFilter`).

| Constructor | Description |
| :--- | :--- |
| `FilterRule(string Key, FilterOperator Operator, FilterValue? Value = null, bool IncludeEmpty = false)` | One condition on a field (or on a query-only filter, see `DataListSchemaBuilder.QueryFilter`). |

| Property | Type | Description |
| :--- | :--- | :--- |
| `IncludeEmpty` (init) | `bool` | Also match rows where the field is empty (null or an empty string). |
| `Key` (init) | `string` | The filter key: the stable key of a field, or the name of a query-only filter. |
| `Operator` (init) | `FilterOperator` | The comparison. |
| `Value` (init) | `FilterValue?` | The operand; `null` for `FilterOperator.IsEmpty` and `FilterOperator.IsNotEmpty`. |

#### `FilterValue`

`Klyroo.Query` · record

The value of a `FilterRule`. A closed hierarchy: `Text`, `Number`, `Boolean`, `Date` (a calendar day), `Moment` (an instant, for hour, minute and second granularity), `List` and `Range`.

#### `FilterValue.List`

`Klyroo.Query` · record · inherits `FilterValue`

A list of option values. Values are strings and are converted to the type of the field (enum name, number, bool, Guid, ISO date).

| Constructor | Description |
| :--- | :--- |
| `List(IReadOnlyList<string> values)` | Creates a list. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Values` (init) | `IReadOnlyList<string>` | The values. |

#### `FilterValue.Moment`

`Klyroo.Query` · record · inherits `FilterValue`

An instant. A date filter matches the whole hour, minute or second (the filter's `DateGranularity`) that contains it, cut in the request's time zone; against a `DateOnly` field it reduces to its local day.

| Constructor | Description |
| :--- | :--- |
| `Moment(DateTimeOffset Value)` | An instant. A date filter matches the whole hour, minute or second (the filter's `DateGranularity`) that contains it, cut in the request's time zone; against a `DateOnly` field it reduces to its local day. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Value` (init) | `DateTimeOffset` | The instant, with its offset. |

#### `DataListPaging.None`

`Klyroo.Query` · record · inherits `DataListPaging`

No paging: the whole result.

| Property | Type | Description |
| :--- | :--- | :--- |
| `static Instance` | `DataListPaging.None` | The shared instance. |

#### `FilterValue.Number`

`Klyroo.Query` · record · inherits `FilterValue`

A number. Always a `Decimal` so that every numeric field type compares exactly.

| Constructor | Description |
| :--- | :--- |
| `Number(decimal Value)` | A number. Always a `Decimal` so that every numeric field type compares exactly. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Value` (init) | `decimal` | The number. |

#### `DataListPaging.Offset`

`Klyroo.Query` · record · inherits `DataListPaging`

Skip a number of rows, then take a page.

| Constructor | Description |
| :--- | :--- |
| `Offset(int Skip, int Take)` | Skip a number of rows, then take a page. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Number` | `int` | The 1-based page number: `Skip / Take + 1`. |
| `Skip` (init) | `int` | Rows to skip. |
| `Take` (init) | `int` | Page size. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static ForPage(int page, int pageSize)` | `DataListPaging.Offset` | The offset of a 1-based page. |

#### `QueryRange<T>`

`Klyroo.Query` · record

A range of values with either end optional, as `QueryCodecs.Range`` reads and writes it.

| Constructor | Description |
| :--- | :--- |
| `QueryRange<T>(T? From, T? To)` | A range of values with either end optional, as `QueryCodecs.Range`` reads and writes it. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `From` (init) | `T?` | The lower end, or null for no lower bound. |
| `To` (init) | `T?` | The upper end, or null for no upper bound. |

#### `FilterValue.Range`

`Klyroo.Query` · record · inherits `FilterValue`

A range with inclusive bounds, either of which may be missing. Bounds are `Number`, `Date` or `Moment`.

| Constructor | Description |
| :--- | :--- |
| `Range(FilterValue? From, FilterValue? To)` | A range with inclusive bounds, either of which may be missing. Bounds are `Number`, `Date` or `Moment`. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `From` (init) | `FilterValue?` | The lower bound, or `null` for none. |
| `To` (init) | `FilterValue?` | The upper bound, or `null` for none. |

#### `FilterValue.Text`

`Klyroo.Query` · record · inherits `FilterValue`

A string.

| Constructor | Description |
| :--- | :--- |
| `Text(string Value)` | A string. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Value` (init) | `string` | The text. |

### Interfaces

#### `IQueryCodec<T>`

`Klyroo.Query` · interface

Converts a value to and from the text of a URL query parameter. The same codecs serve the components (URL-synced state) and an API server that parses the URLs they produce, which is why they live in this Blazor-free package.

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Format(T value)` | `string?` | The text of a value, or null to leave the parameter out of the URL (an empty list, no value). |
| `TryParse(string raw, out T value)` | `bool` | Parses the decoded text of a parameter. Returns false for text that is not a value of this codec: the caller then uses its default. |

### Enums

#### `DataListLayout`

`Klyroo.Query` · enum

How a data list shows its rows.

| Value | Description |
| :--- | :--- |
| `Table` | One row per record (URL value `table`). |
| `Grid` | A grid of cards (URL value `grid`). |

#### `DataListNumberDisplay`

`Klyroo.Query` · enum

How a number filter editor shows its value.

| Value | Description |
| :--- | :--- |
| `Input` | A number field (the default). |
| `Slider` | A slider between `DataListFilter.Min` and `DataListFilter.Max`. |
| `InputAndSlider` | A number field and a slider. |

#### `DataListOptionSort`

`Klyroo.Query` · enum

How an option filter orders the options it lists.

| Value | Description |
| :--- | :--- |
| `Count` | By count, the most counted first, then by label. Needs the counts (`DataListFilter.WithFacets`); an option without a count comes last. |
| `Alpha` | By label, ignoring case and accents. |

#### `DateGranularity`

`Klyroo.Query` · enum

The precision a date filter edits; a day is carried as a `Date`, anything finer as a `Moment`.

| Value | Description |
| :--- | :--- |
| `Day` | A calendar day. |
| `Hour` | An hour. |
| `Minute` | A minute. |
| `Second` | A second. |

#### `FacetMode`

`Klyroo.Query` · enum

Whether a facet counts with its own filter applied.

| Value | Description |
| :--- | :--- |
| `ExcludeSelf` | The facet's own filter is removed before counting, so the other options stay visible (default). |
| `IncludeSelf` | The facet's own filter stays applied. |

#### `FilterCombinator`

`Klyroo.Query` · enum

How the children of a filter group combine.

| Value | Description |
| :--- | :--- |
| `And` | Every child must match. |
| `Or` | At least one child must match. |

#### `FilterKind`

`Klyroo.Query` · enum

The five kinds of filter a field can have.

| Value | Description |
| :--- | :--- |
| `Text` | Free text on a string field. |
| `Option` | A pick from a set of values (strings, enums, numbers, booleans, ids). |
| `Boolean` | True or false. |
| `Number` | A number or a range of numbers. |
| `Date` | A date or a range of dates. |

#### `FilterOperator`

`Klyroo.Query` · enum

The 13 comparison operators of a filter rule. Each filter kind allows a subset (see `DataListFilter`).

| Value | Description |
| :--- | :--- |
| `Contains` | Text contains the value. In memory it ignores case and accents (invariant culture); on a database it follows the collation of the column. |
| `Is` | The field equals the value. |
| `IsAnyOf` | The field equals any value of a list. |
| `IsNot` | Negation of `FilterOperator.IsAnyOf`: the field equals none of the values. |
| `Gt` | Greater than. |
| `Gte` | Greater than or equal. |
| `Lt` | Less than. |
| `Lte` | Less than or equal. |
| `Between` | Between two bounds, both inclusive; either bound may be missing. |
| `Before` | Strictly before a date: before the start of the day, hour, minute or second of the value. |
| `After` | Strictly after a date: at or after the end of the day, hour, minute or second of the value. |
| `IsEmpty` | The field is null or an empty string. |
| `IsNotEmpty` | The field is neither null nor an empty string. |

### Delegates

#### `DataListOptionsProvider`

`Klyroo.Query` · delegate

Loads the options of an option filter from a remote source, one page at a time. The token is cancelled when a newer search replaces the request (a late answer is ignored anyway). An exception shows an error with a retry button in the editor.

#### `DataListOptionsResolver`

`Klyroo.Query` · delegate

Returns the options of values that no loaded page holds (a value restored from the address, whose page was never loaded), so that a tag and an editor can show their labels. Values it cannot find may be left out: they show as they are.

#### `DataListProvider<T>`

`Klyroo.Query` · delegate

Loads a page of rows for a request. The same shape for an in-memory list, an EF query or an HTTP call.

## Klyroo.Query

Static classes and extensions (1) · Classes (4) · Enums (1)

### Static classes and extensions

#### `KlyrooQueryServiceCollectionExtensions`

`Klyroo.Query` · service

Registers the services of `Klyroo.Query` in the dependency injection container.

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static AddKlyrooQuery(IServiceCollection services)` | `IServiceCollection` | Registers `QueryState` as a scoped service (one per Blazor Server circuit, one per WebAssembly app) unless one is already registered. Call it once at startup. |

### Classes

#### `QueryClaim`

`Klyroo.Query` · class

A claim on a prefix of the query keys, or on the keys of a group. Dispose it to release them.

| Property | Type | Description |
| :--- | :--- | :--- |
| `Owner` | `string` | The name of the claiming component. |
| `Prefix` | `string` | The claimed prefix. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Dispose()` | `void` | Releases the claim. |
| `ThrowIfConflicted()` | `void` | Throws when this claim turned out to overlap one that is still alive once the render that created it was over. |

#### `QueryGroup<TRecord>`

`Klyroo.Query` · class

A record whose properties are query keys, read and written together: one write is one navigation and one history entry. Each property has a codec and, like `QueryValue`, is left out of the URL when it equals the matching property of the default record. Make one with `QueryState.Group``, add its properties with `QueryGroup.Add``, and dispose it with the component that owns it (which also releases its prefix).

| Property | Type | Description |
| :--- | :--- | :--- |
| `History` | `QueryHistory` | The history of changes made through `QueryGroup.Set` and `QueryGroup.Update` without a history of their own. |
| `Value` | `TRecord` | The record the URL describes: the default with each property whose key is present and readable. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Add<TValue>(string key, Func<TRecord, TValue> get, Func<TRecord, TValue, TRecord> with, IQueryCodec<TValue> codec)` | `QueryGroup<TRecord>` | Adds a property. The default of the property is the one of the default record. |
| `Dispose()` | `void` | Stops following the URL and releases the prefix. |
| `HrefWith(TRecord value)` | `string` | The URL of the current page with the properties of `value` written, for a link in static server rendering. |
| `Set(TRecord value)` | `bool` | Writes every property of `value` in one change, with the history of the group. A property equal to its default is removed. |
| `Set(TRecord value, QueryHistory history)` | `bool` | Writes every property of `value` in one change. |
| `ThrowIfConflicted()` | `void` | Throws when a key of this group is also used by a claim that outlived the page change this group was created in (see `QueryState.Claim`). Every use of the group calls it. |
| `Update(Func<TRecord, TRecord> change, QueryHistory? history = null)` | `bool` | Changes the current record with `change` and writes it, for example `g.Update(f => f with { Page = 1 }, QueryHistory.Push)`. |

#### `QueryState`

`Klyroo.Query` · class

The query string of the current URL as state: read it, change it, follow it when it changes. The URL is the single source of truth, so a link, the Back button and a URL typed in the address bar all change what the components bound to it show, and a page can be shared with its state. One instance per circuit or per browser tab (it is scoped), registered by `AddKlyrooQuery`.

| Constructor | Description |
| :--- | :--- |
| `QueryState(NavigationManager navigation)` | Creates the service. `AddKlyrooQuery` registers it scoped; create it yourself only in a test. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Interactive` | `bool?` | True when writes can navigate, false when the renderer is known not to be interactive, null when nothing said so yet (`QueryState.UseRenderer`). |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Bind<T>(string key, IQueryCodec<T> codec, T defaultValue, RendererInfo renderer, QueryHistory history = Replace)` | `QueryValue<T>` | Binds one query key to a typed value. See `QueryValue`. |
| `Claim(string prefix, string owner)` | `QueryClaim` | Declares that a component owns a prefix of the query keys (`users` owns `users.q`, `users.p`), or all of them with the empty prefix. A second claim of the same prefix throws and names both owners. A claim that meets one made before the last change of page (a layout, a page reused for another route parameter) is judged once the render that is under way has finished and the previous page is disposed: `QueryClaim.ThrowIfConflicted` then reports it (`QueryGroup` calls it on every use). |
| `Dispose()` | `void` | Stops following the URL. The scope does it at its end. |
| `FlushAsync()` | `Task` | Sends the changes made so far to the URL now instead of after the current handler yields. Useful before leaving the page by another means; the changes of a handler normally need no call. |
| `Get(string key)` | `string?` | The first value of a query key, including the writes of the current handler that have not reached the URL yet; null when the key is absent. |
| `Group<TRecord>(TRecord defaultValue, string owner, RendererInfo renderer, string? prefix = null, QueryHistory history = Replace)` | `QueryGroup<TRecord>` | Binds a record whose properties are query keys. See `QueryGroup`. |
| `HrefWith(IEnumerable<KeyValuePair<string, string?>> changes)` | `string` | The URL (path and query) of the current page with the changes applied, for a link or a form that carries a state change without interactivity (static server rendering): the same keys as `QueryState.Set` writes. A null value removes its key. |
| `HrefWith(params (string, string?)[] changes)` | `string` | The same as `QueryState.HrefWith`, with the changes as (key, value) tuples: `HrefWith(("p", "2"), ("q", null))`. |
| `Set(string key, string? value, QueryHistory history = Replace)` | `bool` | Sets a query key; a null value removes it. Returns false when nothing was written because the renderer is not interactive. |
| `SetMany(IEnumerable<KeyValuePair<string, string?>> changes, QueryHistory history = Replace)` | `bool` | Sets several keys in one change: one navigation, one history entry. A null value removes its key. |
| `Subscribe(string prefix, Action<IReadOnlyList<string>> handler)` | `IDisposable` | Calls `handler` with the keys that changed, when a key matching `prefix` changes: through a write of this service, or through the URL (Back and Forward, a link, a typed URL). The handler runs on the thread that raised the change: from a component, go through `InvokeAsync` before rendering. It is not called for a navigation that leaves the state as it already is. |
| `UseRenderer(RendererInfo renderer)` | `void` | Tells the service whether the renderer is interactive. Only an interactive renderer can navigate, so while this is false a write changes nothing (see the remarks of the class). `QueryState.Bind`` and `QueryState.Group`` call it with the renderer you pass; call it yourself before the raw `QueryState.Set` and `QueryState.SetMany`, from `OnInitialized` with the `RendererInfo` of the component. |

#### `QueryValue<T>`

`Klyroo.Query` · class

One query key as a typed value. A key that is absent, or whose text the codec cannot read, gives the default; a value equal to the default is written as no key at all, so the URL of an untouched page stays clean. Make one with `QueryState.Bind``, and dispose it with the component that owns it.

| Property | Type | Description |
| :--- | :--- | :--- |
| `Default` | `T` | The value of an absent or unreadable key. |
| `History` | `QueryHistory` | The history of changes made through `QueryValue.Value`. |
| `Key` | `string` | The query key. |
| `Value` (get, set) | `T` | The value the URL holds, with the writes of the current handler on top. Setting it writes the URL after the handler yields (see `QueryState`); the getter shows the new value at once. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Dispose()` | `void` | Stops following the URL. |
| `HrefWith(T value)` | `string` | The URL of the current page with this key set to `value` (see `QueryState.HrefWith`). |
| `Set(T value, QueryHistory history)` | `bool` | Sets the value with an explicit history. |

### Enums

#### `QueryHistory`

`Klyroo.Query` · enum

Whether a change of the URL adds a history entry (the Back button undoes it) or replaces the current one.

| Value | Description |
| :--- | :--- |
| `Push` | A new history entry: a page, a sort, an applied filter. |
| `Replace` | The current entry is rewritten: typing in a search box, a normalized value. |

## Klyroo.Query.EntityFrameworkCore

Static classes and extensions (1)

### Static classes and extensions

#### `EfAsyncQueryExecutor`

`Klyroo.Query.EntityFrameworkCore` · service

Runs the queries that `Klyroo.Query.Core` builds (`DataListQuery`, `DataListFacetQuery`) with the asynchronous operators of Entity Framework Core. They are plain static methods, so they work on any `IQueryable` an EF Core provider can run.

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = null)` | `ValueTask<int>` | Counts the rows of a query in the database (`SELECT COUNT(*)`), for example `DataListQuery.Matching`. |
| `static ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = null)` | `ValueTask<List<T>>` | Loads the rows of a query into a list, for example `DataListQuery.Page` or `DataListFacetQuery.Buckets`. |
