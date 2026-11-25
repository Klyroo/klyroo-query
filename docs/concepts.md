# Concepts

This page explains how the pieces of Klyroo.Query fit together and why they are shaped the way they are. For steps, see [Getting started](getting-started.md) and [Guides](guides.md).

## Two packages and an optional adapter

The library is split along the line between what a list means and where it is shown.

```text
   Klyroo.Query.Core                               Klyroo.Query
   (no UI, no ASP.NET Core)                        (Blazor)

   DataListSchema<T>      what a row type allows   QueryState      the query string as state
   DataListRequest        what to load             QueryValue<T>   one key, typed
   DataListResult<T>      what came back           QueryGroup<T>   several keys, one record
   DataListState          what the user left
   DataListStateQuery     state <-> query keys     Klyroo.Query.EntityFrameworkCore
   QueryCodecs            value <-> text           EfAsyncQueryExecutor   runs the IQueryable
   ExecuteInMemory / Apply  run a request
```

`Klyroo.Query.Core` knows nothing about Blazor, `NavigationManager` or the DOM, so an API server can reference it to parse the same URLs and answer the same requests as the components. `Klyroo.Query` depends on it and adds the part that needs a router. `Klyroo.Query.EntityFrameworkCore` is two static methods around the async operators of EF Core; `Klyroo.Query.Core` itself builds `IQueryable` operators without referencing EF Core.

Klyroo.Table builds data lists on top of these types, which is why `Klyroo.Query` and `Klyroo.Query.Core` expose their internals to it.

## Schema, request, state and result

Four kinds of object describe a list, and they have different owners.

- **Schema** (`DataListSchema<T>`) is built once, in code, by the developer. It lists the fields of a row type, which are sortable, which have a filter and of what kind, the search fields, the defaults and the limits. It is immutable and shared. Everything else is judged against it.
- **Request** (`DataListRequest`) is what a client asks a provider for: paging, sort rules, a filter tree, a search and facets. It is plain data, so it serializes to JSON and can come from anywhere, which is why the schema can reject any part of it.
- **State** (`DataListState`) is what the user left on screen: search, filters, sort, page, page size, layout and a cursor. It is the URL-shaped view of a list. `ToRequest` turns it into a request; the time zone is added there because it comes from the client, not from the state.
- **Result** (`DataListResult<T>`) is a page of rows with the total count, the cursor of the next page and the facet counts.

The flow of a list looks like this:

```text
 address bar  <-- DataListStateQuery -->  DataListState --ToRequest--> DataListRequest
                                                                              |
                                                                  schema.Sanitize(request)
                                                                              |
                          ExecuteInMemory (collection)   or   Apply (IQueryable) + you run it
                                                                              |
                                                                       DataListResult<T>
```

A filter is a tree. A `FilterRule` holds a key, an operator, a `FilterValue` and an include-empty flag; a `DataListFilterGroup` combines children with AND or OR. A `FilterValue` is a closed hierarchy (`Text`, `Number`, `Boolean`, `Date`, `Moment`, `List`, `Range`), and a number is always a `decimal` so that every numeric field type compares exactly. Each `DataListFilter` of the schema belongs to one of five kinds and allows a subset of the 13 operators:

| Kind | Operators |
| :--- | :--- |
| Text | `contains` (default), `is`, `isNot` |
| Option | `isAnyOf` (default), `is`, `isNot` |
| Boolean | `is` (default), `isNot` |
| Number | `is` (default), `isNot`, `gt`, `gte`, `lt`, `lte`, `between` |
| Date | `is` (default), `isNot`, `before`, `after`, `between` |

`isEmpty` and `isNotEmpty` can be added to any kind with `WithOperators`.

A facet is the count of rows per option of a filter. By default a facet leaves out its own filter (`ExcludeSelf`), so choosing "Active" does not hide "Closed" from the list of options.

## One definition of every operator

The in-memory executor and the `IQueryable` applier build their predicates from the same definition of each operator, as expression trees. The in-memory flavor compiles the tree and adds null guards and case-and-accent-insensitive text comparison. The database flavor emits the expression as written (no `ToLower`, no null guards, values as parameters), so the provider translates it and the collation of the column decides about text.

That is a trade-off. A single definition keeps the two executors from drifting, and you can test a schema in memory and trust it on a database, with the differences documented in [Run a request on EF Core](guides.md#run-a-request-on-ef-core): text case, text sort order and a few provider limits. The alternative, lower-casing in SQL to make the database mimic memory, would defeat indexes and still depend on the provider.

## Validation by dropping

Requests and URLs arrive from outside, so the library treats them as untrusted and drops what it does not understand instead of throwing.

- `Sanitize` removes rules with an unknown key, a disallowed operator or a value of the wrong shape, sort keys that are unknown, not sortable or repeated, and facets on fields that have no option or boolean filter. It clamps the page size and bounds the work a hostile request can ask for: at most `MaxFilterRules` rules, `MaxListValues` values per list and 16 levels of groups.
- `DataListStateQuery.FromQuery` starts from the schema defaults and keeps whatever parses. A hand-edited URL degrades to a default instead of an error page.
- Codecs return `false` from `TryParse` for text that is not a value, and `Format` returns `null` to leave a key out.

Misconfiguration is the opposite case, and it throws `InvalidOperationException`: an invalid schema, cursor paging without a row key, `Apply` on an in-memory `IQueryable`. Those are bugs in your code, found once and early.

## Cursors

Offset paging skips rows, so a row added between two page loads shifts the next page. Cursor paging is keyset paging: the cursor encodes the sort, the key of the last row and its sort values, and the next page is every row that sorts strictly after that tuple. The sort puts nulls last in both directions and ends with the row key, which makes the order total, so pages never repeat or skip a row. The cursor is opaque and not signed. It carries no permission, since every request is filtered and authorized again, and editing one only moves the position inside the caller's own result.

## `QueryState`: the URL as the source of truth

`QueryState` reads and writes the query string of the current URL, and the URL stays the single source of truth. A link, the Back button and a URL typed in the address bar all change what the bound components show, and a page can be shared with its state.

```text
 component --Value = x--> QueryState --batch, after the handler yields--> NavigationManager.NavigateTo
     ^                         |                                                    |
     |                         +--Subscribe(prefix)-- Changed event <-- LocationChanged
     +------------------------------ re-render (InvokeAsync) ----------------------+
```

How it behaves:

- **Writes are batched.** Writes made in one event handler, before it first yields to the renderer, merge into one navigation that starts right after, so a handler that changes the page and the sort produces one history entry. If any write asks for `Push`, the batch pushes; otherwise it replaces the current entry. A value equal to what the URL already holds writes nothing, and parameters this service never wrote (`utm_source`) stay as they were.
- **Reads include pending writes.** `Get` and `QueryValue<T>.Value` return what the URL will hold, so a component sees its own write immediately.
- **Own navigations are recognized.** `QueryState` remembers what it sent, so the `LocationChanged` that comes back is not reported as an external change. A URL reached any other way (Back, a link, the address bar) wins and notifies subscribers of the keys that changed.
- **A cancelled navigation rolls back.** If a `NavigationLock` cancels a navigation of this service, no event comes back; after a few seconds the state shown returns to what the URL holds.
- **Ownership is explicit.** A component declares the keys it owns (`Claim`, or implicitly through `Group`), and a second owner of the same keys throws with both names. This turns two components fighting over `?p=` into an error at startup.
- **Defaults stay out of the URL.** `QueryValue<T>` and `QueryGroup<TRecord>` write nothing for a value equal to its default, so the URL of an untouched page is clean.

## Render modes and where writes can happen

Only an interactive renderer can navigate. During prerendering and in static server rendering a navigation attempt either cannot happen or, when the app disables the navigation exception, becomes a real HTTP redirect. So `QueryState` never sends a write to the navigation manager unless it knows the renderer is interactive: `Bind` and `Group` take the `RendererInfo` of the component for that, a write from a non-interactive renderer changes nothing and returns `false`, and a page offers its changes as links with `HrefWith`. See [Platforms](platforms.md) for what each mode requires.

## Threading

`QueryState` guards its mutable state with a lock. `Changed` events and `Subscribe` handlers run on the thread that raised the change, which is not necessarily the renderer's thread, so a component goes through `InvokeAsync` before it renders. The schema and the executors hold no per-call state: a schema is safe to share, and compiled predicates are cached on it (at most 64, the cache is emptied when full).

## Design decisions

- **Expressions, not strings.** A schema is `c => c.Country!.Name`, so a rename breaks the build and nothing resolves a member from a string at run time. This is what lets the core be trim-safe and, for the in-memory path, AOT compatible.
- **Requests are data.** A request has no behavior, only a source-generated JSON shape with a `type` discriminator, so the same object travels between a component, an API and a test.
- **Keys are stable names, not paths.** A request refers to `country`, not to `Country.Name`, so the wire format and the URL do not break when a model changes.
- **No EF Core in the engine.** `Apply` returns `IQueryable` and you run it, which keeps the core free of a database dependency and lets any provider that translates the operators work. The cost is that you write a few lines to count, load and assemble the result, as in the [EF Core guide](guides.md#run-a-request-on-ef-core).
- **Defaults are omitted.** Everything equal to the schema default stays out of the URL. The cost is that a default filter the user removes must be written explicitly, as a key with an empty value.
- **State filters are AND-only.** The URL form carries one rule per key and operator. OR groups exist in a request, which is the right place for them.
