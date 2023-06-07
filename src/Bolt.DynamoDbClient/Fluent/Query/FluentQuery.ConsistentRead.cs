using Amazon.DynamoDBv2.Model;

namespace Bolt.DynamoDbClient.Fluent.Query;

internal sealed partial class FluentQuery : IHaveDynamoDbConsistentRead
{
    private bool? _consistentRead;

    IHaveDynamoDbConsistentRead ICollectDynamoDbConsistentRead.ConsistentRead(bool consistentRead)
    {
        _consistentRead = consistentRead;
        return this;
    }
}

public interface IHaveDynamoDbConsistentRead : IDynamoDbFetchData, ICollectDynamoDbScanIndexForward { }
public interface ICollectDynamoDbConsistentRead
{
    IHaveDynamoDbConsistentRead ConsistentRead(bool consistentRead);
}