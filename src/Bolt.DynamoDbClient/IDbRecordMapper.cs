using Amazon.DynamoDBv2.Model;

namespace Bolt.DynamoDbClient;

public interface IDbRecordMapper
{
    T Map<T>(Dictionary<string, AttributeValue> attributeValues) where T : new();
}

internal sealed class DbRecordMapper : IDbRecordMapper
{
    public T Map<T>(Dictionary<string, AttributeValue> attributeValues) where T : new()
    {
        return attributeValues.MapTo<T>()!;
    }
}