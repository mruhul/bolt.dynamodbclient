using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace Bolt.DynamoDbClient;

internal record DynamoDbItemMetaData
{
    public string TableName { get; init; } = string.Empty;
    public string PartitionKeyColumnName { get; init; } = string.Empty;
    public string SortKeyColumnName { get; init; } = string.Empty;
    public PropertyInfo? PartitionKeyProperty { get; init; }
    public PropertyInfo? SortKeyProperty { get; init; }
    public DynamoDbItemMetaProperty[] Properties { get; init; } = [];
    public TypeInfoMetaData? TypeInfoMetaData { get; init; }
};

internal record DynamoDbItemMetaProperty
{
    public DynamoDbItemMetaProperty(PropertyInfo property,
        DynamoDbColumnType? columnType,
        string? columnName,
        DynamoDbOperationIgnoreInstructionType? ignore)
    {
        ColumnType = columnType ?? DynamoDbColumnType.Default;
        PropertyInfo = property;
        IgnoreInstruction = ignore ?? DynamoDbOperationIgnoreInstructionType.Never;
        ColumnName = columnName ?? PropertyInfo.Name;
        ProjectionColumnName = DynamoDbReservedWords.IsReserveWord(ColumnName)
            ? $"#{ColumnName}"
            : null;
    }

    public string ColumnName { get; init; }
    public string? ProjectionColumnName { get; init; }
    public DynamoDbColumnType ColumnType { get; init; }
    public DynamoDbOperationIgnoreInstructionType IgnoreInstruction { get; init; }
    public PropertyInfo PropertyInfo { get; init; }
    public TypeInfoMetaData TypeMetaData { get; init; }
}

internal static class DynamoDbItemMetaDataReader
{
    private static readonly ConcurrentDictionary<Type, DynamoDbItemMetaData> Store = new();

    public static DynamoDbItemMetaData Get(Type type)
    {
        return Store.GetOrAdd(type, LoadMetaData);
    }

    private const string DefaultPartitionKeyColumnName = "PK";
    private const string DefaultSortKeyColumnName = "SK";

    private static DynamoDbItemMetaData LoadMetaData(Type type)
    {
        var typeName = type.Name;
        var propertiesAvailable = type.GetProperties().Where(x => x.CanRead && x.CanWrite).ToArray();
        var tableAtt = type.GetCustomAttribute<DynamoDbTableAttribute>();
        var properties = new List<DynamoDbItemMetaProperty>(propertiesAvailable.Length);
        PropertyInfo? partitionKeyProp = null;
        PropertyInfo? sortKeyProp = null;
        string? partitionKeyColumnName = null;
        string? sortKeyColumnName = null;

        foreach (var prop in propertiesAvailable)
        {
            var pkAttr = prop.GetCustomAttribute<DynamoDbPartitionKeyAttribute>();
            var skAttr = prop.GetCustomAttribute<DynamoDbSortKeyAttribute>();
            var ignoreAttr = prop.GetCustomAttribute<DynamoDbOperationIgnoreAttribute>();
            var columnAttr = prop.GetCustomAttribute<DynamoDbColumnAttribute>();

            DynamoDbColumnType? columnType = null;

            if (pkAttr != null)
            {
                columnType = DynamoDbColumnType.PartitionKey;
                partitionKeyProp = prop;
                partitionKeyColumnName = string.IsNullOrWhiteSpace(pkAttr.Name) ? prop.Name : pkAttr.Name;
            }

            if (partitionKeyProp == null)
            {
                if (string.Equals(prop.Name, DefaultPartitionKeyColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    columnType = DynamoDbColumnType.PartitionKey;
                    partitionKeyProp = prop;
                }
            }


            if (skAttr != null)
            {
                columnType = DynamoDbColumnType.SortKey;
                sortKeyProp = prop;
                sortKeyColumnName = string.IsNullOrWhiteSpace(skAttr.Name) ? prop.Name : skAttr.Name;
            }

            if (sortKeyColumnName == null)
            {
                if (string.Equals(prop.Name, DefaultSortKeyColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    columnType = DynamoDbColumnType.SortKey;
                    sortKeyProp = prop;
                }
            }

            if (ignoreAttr?.Ignore is DynamoDbOperationIgnoreInstructionType.Always) continue;

            var isSimpleType = IsSimpleType(prop.PropertyType);
            var isCollection = isSimpleType ? false : typeof(IEnumerable).IsAssignableFrom(prop.PropertyType);

            properties.Add(new DynamoDbItemMetaProperty(prop, 
                columnType, 
                columnAttr?.Name, 
                ignoreAttr?.Ignore)
            {
                TypeMetaData = prop.PropertyType.GetTypeInfo()
            });
        }

        return new DynamoDbItemMetaData
        {
            TableName = tableAtt?.Name ?? type.Name,
            PartitionKeyProperty = partitionKeyProp,
            PartitionKeyColumnName = partitionKeyColumnName ?? DefaultPartitionKeyColumnName,
            SortKeyProperty = sortKeyProp,
            SortKeyColumnName = sortKeyColumnName ?? DefaultSortKeyColumnName,
            Properties = properties.ToArray(),
            TypeInfoMetaData = type.GetTypeInfo()
        };
    }

    private static Type GetCollectionItemType(Type type)
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

        return typeof(string);
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
               || type == typeof(bool)
               || type == typeof(TimeSpan)
               || type == typeof(Guid)
               || type == typeof(int?)
               || type == typeof(decimal?)
               || type == typeof(double?)
               || type == typeof(float?)
               || type == typeof(long?)
               || type == typeof(DateTime?)
               || type == typeof(DateTimeOffset?)
               || type == typeof(bool?)
               || type == typeof(TimeSpan?)
               || type == typeof(Guid?);
    }
}

public class DynamoDbTableAttribute : Attribute
{
    public DynamoDbTableAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; private set; }
}

public class DynamoDbColumnAttribute : Attribute
{
    public DynamoDbColumnAttribute(string name)
    {
        Name = name;
    }

    public string? Name { get; private set; }
}

public class DynamoDbPartitionKeyAttribute : Attribute
{
    public DynamoDbPartitionKeyAttribute()
    {
    }

    public DynamoDbPartitionKeyAttribute(string name)
    {
        Name = name;
    }

    public string? Name { get; private set; }
}

public class DynamoDbSortKeyAttribute : Attribute
{
    public DynamoDbSortKeyAttribute()
    {
    }

    public DynamoDbSortKeyAttribute(string name)
    {
        Name = name;
    }

    public string? Name { get; private set; }
}

public class DynamoDbOperationIgnoreAttribute : Attribute
{
    public DynamoDbOperationIgnoreAttribute(DynamoDbOperationIgnoreInstructionType ignore)
    {
        Ignore = ignore;
    }

    public DynamoDbOperationIgnoreAttribute() : this(DynamoDbOperationIgnoreInstructionType.Always)
    {
    }

    public DynamoDbOperationIgnoreInstructionType Ignore { get; private set; }
}

public enum DynamoDbOperationIgnoreInstructionType
{
    Never,
    Always,
    Upsert,
    Create,
    Update
}

public enum DynamoDbColumnType
{
    Default,
    PartitionKey,
    SortKey
}