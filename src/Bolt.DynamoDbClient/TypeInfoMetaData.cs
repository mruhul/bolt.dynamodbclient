using System.Collections;

namespace Bolt.DynamoDbClient;

internal struct TypeInfoMetaData
{
    public Type Type { get; init; }
    public bool IsSimpleType { get; init; }
    public bool IsCollection { get; init; }
    public Type? CollectionItemType { get; init; }
    public CollectionType? CollectionType { get; init; }
}

internal enum CollectionType
{
    Enumerable,
    Collection,
    Array
}

internal static class TypeInfoExtensions
{
    internal static TypeInfoMetaData GetTypeInfo(this Type type)
    {
        var isSimpleType = IsSimpleType(type);
        var isCollection = isSimpleType ? false : typeof(IEnumerable).IsAssignableFrom(type);
        var collectionItemType = isCollection ? (GetCollectionItemType(type) ?? typeof(string)) : null;

        return new TypeInfoMetaData
        {
            Type = type,
            IsSimpleType = IsSimpleType(type),
            IsCollection = isCollection,
            CollectionItemType = collectionItemType,
            CollectionType = isCollection ? GetCollectionType(type) : null
        };
    }


    internal static CollectionType GetCollectionType(Type type)
    {
        if (type.IsArray) return CollectionType.Array;
        if (typeof(ICollection).IsAssignableFrom(type)) return CollectionType.Collection;
        return CollectionType.Enumerable;
    }

    internal static Type? GetCollectionItemType(Type type)
    {
        if (typeof(IEnumerable<string>).IsAssignableFrom(type)) return typeof(string);
        if (typeof(IEnumerable<int>).IsAssignableFrom(type)) return typeof(int);
        if (typeof(IEnumerable<double>).IsAssignableFrom(type)) return typeof(double);
        if (typeof(IEnumerable<decimal>).IsAssignableFrom(type)) return typeof(decimal);
        if (typeof(IEnumerable<long>).IsAssignableFrom(type)) return typeof(long);
        if (typeof(IEnumerable<float>).IsAssignableFrom(type)) return typeof(float);
        if (typeof(IEnumerable<Guid>).IsAssignableFrom(type)) return typeof(Guid);
        if (typeof(IEnumerable<DateTime>).IsAssignableFrom(type)) return typeof(DateTime);
        if (typeof(IEnumerable<DateTimeOffset>).IsAssignableFrom(type)) return typeof(DateTimeOffset);

        return null;
    }

    private static bool IsSimpleType(Type type)
    {
        return type == typeof(string)
            || type == typeof(int)
            || type == typeof(decimal)
            || type == typeof(double)
            || type == typeof(float)
            || type == typeof(long)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(TimeSpan)
            || type == typeof(Guid)
            || type == typeof(bool)
            || type == typeof(int?)
            || type == typeof(decimal?)
            || type == typeof(double?)
            || type == typeof(float?)
            || type == typeof(long?)
            || type == typeof(DateTime?)
            || type == typeof(DateTimeOffset?)
            || type == typeof(TimeSpan?)
            || type == typeof(Guid?)
            || type == typeof(bool?);
    }
}
