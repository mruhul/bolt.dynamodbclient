using Amazon.DynamoDBv2.Model;

namespace Bolt.DynamoDbClient.Fluent.Query;

internal sealed partial class FluentQuery : IHaveDynamoDbExclusiveStartKey
{
    private Dictionary<string,AttributeValue>? _exclusiveStartKey;

    IHaveDynamoDbExclusiveStartKey ICollectDynamoDbExclusiveStartKey.ExclusiveStartKey(Dictionary<string, AttributeValue> exclusiveStartKey)
    {
        _exclusiveStartKey = exclusiveStartKey;
        return this;
    }
}

public interface IHaveDynamoDbExclusiveStartKey : IDynamoDbFetchData, ICollectDynamoDbConsistentRead, ICollectDynamoDbScanIndexForward { }
public interface ICollectDynamoDbExclusiveStartKey
{
    IHaveDynamoDbExclusiveStartKey ExclusiveStartKey(Dictionary<string, AttributeValue> exclusiveStartKey);
}