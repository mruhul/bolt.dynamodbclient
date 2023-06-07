namespace Bolt.DynamoDbClient.Fluent.Query;

internal sealed partial class FluentQuery : IHaveDynamoDbLimit
{
    private int? _limit;

    IHaveDynamoDbLimit ICollectDynamoDbLimit.Limit(int limit)
    {
        _limit = limit;
        return this;
    }
}

public interface IHaveDynamoDbLimit : IDynamoDbFetchData, ICollectDynamoDbExclusiveStartKey, ICollectDynamoDbConsistentRead, ICollectDynamoDbScanIndexForward { }
public interface ICollectDynamoDbLimit 
{
    IHaveDynamoDbLimit Limit(int limit);
}