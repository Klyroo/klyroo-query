namespace Klyroo.Query;

/// <summary>What a data list component needs to know about a registered field of a <see cref="DataListSchema{T}"/>, without the expression machinery behind it. Read it from <see cref="DataListSchema{T}.FieldInfos"/>.</summary>
public sealed record DataListFieldInfo
{
    internal DataListFieldInfo(string key, bool sortable, bool isCollection, DataListFilter? filter, bool hasStructuredValue)
    {
        Key = key;
        Sortable = sortable;
        IsCollection = isCollection;
        Filter = filter;
        HasStructuredValue = hasStructuredValue;
    }

    /// <summary>The stable key of the field, as used in the URL and in <see cref="DataListSchema.SortKeys"/>.</summary>
    public string Key { get; init; }

    /// <summary>Whether the field can be sorted on.</summary>
    public bool Sortable { get; init; }

    /// <summary>Whether the field holds several values per row (a collection); such a field has no single value to show or sort on.</summary>
    public bool IsCollection { get; init; }

    /// <summary>The filter of the field, or <see langword="null"/> when it cannot be filtered.</summary>
    public DataListFilter? Filter { get; init; }

    /// <summary>Whether the values of the field are numbers, dates, times, GUIDs or booleans: the text of such a value is produced by the type, not typed by a person, so it cannot start a spreadsheet formula.</summary>
    public bool HasStructuredValue { get; init; }
}
