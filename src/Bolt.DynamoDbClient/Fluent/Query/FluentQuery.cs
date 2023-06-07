namespace Bolt.DynamoDbClient.Fluent.Query;

internal sealed partial class FluentQuery : IDynamoDbFluentQuery
{
    private readonly IDynamoDbWrapper _db;

    public FluentQuery(IDynamoDbWrapper db)
    {
        _db = db;
    }
}

public interface IDynamoDbFluentQuery
    : ICollectDynamoDbIndexName,
    ICollectDynamoDbPartitionKeyColumnName
{
}