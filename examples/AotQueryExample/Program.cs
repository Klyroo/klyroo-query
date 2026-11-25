using Klyroo.Query;

var data = new[] { new Item(1, "Alpha", 10), new Item(2, "Beta", 30), new Item(3, "Gamma", 20) };
var schema = DataListSchema.For<Item>(s => s
    .RowKey(i => i.Id)
    .Field(i => i.Score, f => f.Key("score").Sortable()));
var request = new DataListRequest(
    new DataListPaging.Offset(0, 10),
    [new DataListSort("score", Descending: true)],
    DataListFilterGroup.Empty);
var result = schema.ExecuteInMemory(data, request);
Console.WriteLine($"Total: {result.TotalCount}, Top: {result.Items[0].Name}");

public sealed record Item(int Id, string Name, int Score);
