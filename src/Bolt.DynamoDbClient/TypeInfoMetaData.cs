using System.Collections;

namespace Bolt.DynamoDbClient;

internal struct TypeInfoMetaData
{
    public Type Type { get; init; }
    public bool IsSimpleType { get; init; }
    public bool IsCollection { get; init; }
    public Type? CollectionItemType { get; init; }
    public CollectionType? CollectionType { get; init; }
    public bool IsDictionary { get; init; }
    public Type? DictionaryValueType { get; init; }
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
        var isDictionary = typeof(IDictionary).IsAssignableFrom(type);
        var dictValueType = isDictionary ? type.GetGenericArguments().LastOrDefault() : null;

        return new TypeInfoMetaData
        {
            Type = type,
            IsSimpleType = IsSimpleType(type),
            IsCollection = isCollection,
            CollectionItemType = collectionItemType,
            CollectionType = isCollection ? GetCollectionType(type) : null,
            IsDictionary = isDictionary,
            DictionaryValueType = dictValueType
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
        var argTypes = type.GetGenericArguments();
        if(argTypes.Length != 1) return null;
        return argTypes[0];
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
