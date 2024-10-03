using System.Diagnostics.CodeAnalysis;
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

        return AttributeValueMapper.MapTo(type, value);
    }
}
