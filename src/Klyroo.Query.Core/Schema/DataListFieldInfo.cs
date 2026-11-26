namespace Klyroo.Query;

/// <summary>What a data list component needs to know about a registered field of a <see cref="DataListSchema{T}"/>, without the expression machinery behind it.</summary>
/// <param name="Key">The stable key of the field, as used in the URL and in <see cref="DataListSchema.SortKeys"/>.</param>
/// <param name="Sortable">Whether the field can be sorted on.</param>
/// <param name="IsCollection">Whether the field holds several values per row (a collection); such a field has no single value to show or sort on.</param>
/// <param name="Filter">The filter of the field, or <see langword="null"/> when it cannot be filtered.</param>
public sealed record DataListFieldInfo(string Key, bool Sortable, bool IsCollection, DataListFilter? Filter);
