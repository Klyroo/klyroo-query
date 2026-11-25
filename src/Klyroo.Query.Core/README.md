# Klyroo.Query.Core

The engine of [Klyroo.Query](https://github.com/klyroo/klyroo-query): a typed query model for lists of data, with no UI and no ASP.NET Core dependency. You describe a row type with a schema, and a request (filter tree, sort, search, paging, facets) runs over an in-memory collection or an `IQueryable`, has a URL form, and serializes to JSON. The package is marked trimmable and AOT compatible.

## Install

```bash
dotnet add package Klyroo.Query.Core --prerelease
```

## Use

```csharp
using Klyroo.Query;

var schema = DataListSchema.For<Customer>(s => s
    .RowKey(c => c.Id)
    .Search(c => c.Name, c => c.Email)
    .Field(c => c.Age, f => f.Sortable().Filter(DataListFilter.Number(min: 0)))
    .DefaultSort(c => c.Name));

var request = new DataListRequest(
    new DataListPaging.Offset(Skip: 0, Take: 25),
    [new DataListSort("age", Descending: true)],
    DataListFilterGroup.Empty,
    new DataListSearch("martin"));

DataListResult<Customer> page = schema.ExecuteInMemory(customers, request);
```

`schema.Apply(source, request)` builds the same request as `IQueryable` operators for EF Core, `DataListStateQuery` converts a list state to and from URL query keys, and `schema.Sanitize(request)` drops anything the schema does not allow.

See the [repository README](https://github.com/klyroo/klyroo-query#readme) for the other packages, and the [documentation](https://github.com/klyroo/klyroo-query/tree/main/docs) for guides, concepts and the API reference.
