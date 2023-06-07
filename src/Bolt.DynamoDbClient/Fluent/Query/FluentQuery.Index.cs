namespace Bolt.DynamoDbClient.Fluent.Query;

internal sealed partial class FluentQuery :
    IHaveDynamoDbIndexName
{
    private string? _indexName = null;

    IHaveDynamoDbIndexName ICollectDynamoDbIndexName.IndexName(string name)
    {
        _indexName = name;
        return this;
    }
}

public interface IHaveDynamoDbIndexName : ICollectDynamoDbPartitionKeyColumnName { }
public interface ICollectDynamoDbIndexName
{
    IHaveDynamoDbIndexName IndexName(string name);
}