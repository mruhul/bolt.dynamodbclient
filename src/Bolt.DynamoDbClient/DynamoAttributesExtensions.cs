using Amazon.DynamoDBv2.Model;
using System.Globalization;

namespace Bolt.DynamoDbClient;

internal static class DynamoAttributesExtensions
{
    public static T? MapTo<T>(this Dictionary<string, AttributeValue> attributeValues) where T : new()
    {
        if (attributeValues == null || attributeValues.Count == 0) return default;
        var type = typeof(T);
        var metaData = DynamoDbItemMetaDataReader.Get(type);

        var result = new T();

        foreach(var col in metaData.Properties)
        {
            if(attributeValues.TryGetValue(col.ColumnName, out var att))
            {
                col.PropertyInfo.SetValue(result, FromDynamoDbValue(col, att));
            }
        }

        return result;
    }

    private static object? MapTo(Type type, Dictionary<string, AttributeValue> attributeValues)
    {
        if (attributeValues == null || attributeValues.Count == 0) return default;
        
        var metaData = DynamoDbItemMetaDataReader.Get(type);

        var result = Activator.CreateInstance(type);

        foreach (var col in metaData.Properties)
        {
            if (attributeValues.TryGetValue(col.ColumnName, out var att))
            {
                col.PropertyInfo.SetValue(result, FromDynamoDbValue(col, att));
            }
        }

        return result;
    }

    private static object? FromDynamoDbValue(DynamoDbItemMetaProperty metaData, AttributeValue value)
    {
        var type = metaData.PropertyInfo.PropertyType;

        if (value == null) return null;

        if (metaData.TypeMetaData.IsCollection)
        {
            return GetCollectionValue(metaData, value);
        }

        if (metaData.TypeMetaData.IsSimpleType == false)
        {
            return GetSubItemValue(metaData, value);
        }
        if (type == typeof(string)) return value.S;
        if (type == typeof(int)) return int.TryParse(value.N, out var parsedValue) ? parsedValue : null;
        if (type == typeof(int?)) return int.TryParse(value.N, out var parsedValue) ? parsedValue : null;
        if (type == typeof(double)) return double.TryParse(value.N, out var parsedValue) ? parsedValue : null;
        if (type == typeof(double?)) return double.TryParse(value.N, out var parsedValue) ? parsedValue : null;
        if (type == typeof(decimal)) return decimal.TryParse(value.N, out var parsedValue) ? parsedValue : null;
        if (type == typeof(decimal?)) return decimal.TryParse(value.N, out var parsedValue) ? parsedValue : null;
        if (type == typeof(float)) return float.TryParse(value.N, out var parsedValue) ? parsedValue : null;
        if (type == typeof(float?)) return float.TryParse(value.N, out var parsedValue) ? parsedValue : null;
        if (type == typeof(long)) return long.TryParse(value.N, out var parsedValue) ? parsedValue : null;
        if (type == typeof(long?)) return long.TryParse(value.N, out var parsedValue) ? parsedValue : null;
        if (type == typeof(bool)) return value.BOOL;
        if (type == typeof(bool?)) return value.BOOL;
        if (type == typeof(Guid)) return Guid.TryParse(value.S, out var parsedValue) ? parsedValue : null;
        if (type == typeof(Guid?)) return Guid.TryParse(value.S, out var parsedValue) ? parsedValue : null;
        if (type == typeof(DateTime) || type == typeof(DateTime?)) return ToUtcDateTime(value.S);

        return value.S;
    }

    private static object? GetSubItemValue(DynamoDbItemMetaProperty metaData, AttributeValue value)
    {
        return MapTo(metaData.PropertyInfo.PropertyType, value.M);
    }

    private static object GetCollectionValue(DynamoDbItemMetaProperty metaData, AttributeValue value)
    {
        var isArray = metaData.PropertyInfo.PropertyType.IsArray;

        var collectionItemType = metaData.TypeMetaData.CollectionItemType;

        if (collectionItemType == typeof(string)) return ToCollection(value.SS.Select(x => x), isArray);
        if (collectionItemType == typeof(Guid)) return ToCollection(value.SS.Select(x => Guid.Parse(x)),isArray);
            

        if (collectionItemType == typeof(int)) return ToCollection( value.NS.Select(x => int.Parse(x)),isArray);
        if (collectionItemType == typeof(double)) return ToCollection(value.NS.Select(x => double.Parse(x)), isArray);
        if (collectionItemType == typeof(long)) return ToCollection(value.NS.Select(x => long.Parse(x)), isArray);
        if (collectionItemType == typeof(decimal)) return ToCollection(value.NS.Select(x => decimal.Parse(x)), isArray);
        if (collectionItemType == typeof(float)) return ToCollection(value.NS.Select(x => float.Parse(x)), isArray);

        return ToCollection(value.SS.Select(x => x), isArray);
    }

    private static object ToCollection<T>(IEnumerable<T> source, bool isArray)
    {
        return isArray ? source.ToArray() : source.ToList();
    }

    private static DateTime? ToUtcDateTime(string source)
    {
        if (DateTime.TryParse(source, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
        {
            if (result.Kind == DateTimeKind.Utc) return result;

            return result.ToUniversalTime();
        }

        return null;
    }
}
