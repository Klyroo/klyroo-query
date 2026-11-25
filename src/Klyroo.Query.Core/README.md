# Klyroo.Query.Core

The engine of [Klyroo.Query](https://github.com/klyroo/klyroo-query): a typed query model for lists of data, with no UI and no ASP.NET Core dependency. You describe a row type with a schema, and a request (filter tree, sort, search, paging, facets) runs over an in-memory collection or an `IQueryable`, has a URL form, and serializes to JSON. The package is marked trimmable and AOT compatible.

## Install

```bash
dotnet add package Klyroo.Query.Core --prerelease
```

## Use

```csharp
using Klyroo.Query;

Customer[] customers =
[
    new(1, "Alice Martin", "alice@example.com", CustomerStatus.Active, 34),
    new(2, "Bruno Costa", "bruno@example.com", CustomerStatus.Inactive, 51),
    new(3, "Dana Lee", "dana@example.com", CustomerStatus.Active, 28),
    new(4, "Clara Diaz", "clara@example.com", CustomerStatus.Active, 42),
    new(5, "Erik Holm", "erik@example.com", CustomerStatus.Active, 39),
];

var schema = DataListSchema.For<Customer>(s => s
    .RowKey(c => c.Id)
    .Search(c => c.Name, c => c.Email)
    .Field(c => c.Name, f => f.Sortable())
    .Field(c => c.Age, f => f.Sortable().Filter(DataListFilter.Number(min: 0)))
    .Field(c => c.Status, f => f.Filter(DataListFilter.Option()))
    .DefaultSort(c => c.Name));

// Active customers whose name or email contains "ar", oldest first, 25 per page.
var request = new DataListRequest(
    new DataListPaging.Offset(Skip: 0, Take: 25),
    [new DataListSort("age", Descending: true)],
    DataListFilterGroup.And(new FilterRule("status", FilterOperator.Is, new FilterValue.Text("Active"))),
    new DataListSearch("ar"));

DataListResult<Customer> page = schema.ExecuteInMemory(customers, request);

Console.WriteLine($"{page.TotalCount} customers");
foreach (var customer in page.Items)
    Console.WriteLine($"{customer.Name}, {customer.Age}");

public enum CustomerStatus { Active, Inactive }

public sealed record Customer(int Id, string Name, string Email, CustomerStatus Status, int Age);
```

It prints:

```text
2 customers
Clara Diaz, 42
Alice Martin, 34
```

`schema.Apply(source, request)` builds the same request as `IQueryable` operators for EF Core, `DataListStateQuery` converts a list state to and from URL query keys, and `schema.Sanitize(request)` drops anything the schema does not allow.

See the [repository README](https://github.com/klyroo/klyroo-query#readme) for the other packages, and the [documentation](https://github.com/klyroo/klyroo-query/tree/main/docs) for guides, concepts and the API reference.
