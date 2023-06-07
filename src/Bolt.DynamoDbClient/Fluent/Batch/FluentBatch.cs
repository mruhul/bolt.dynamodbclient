namespace Bolt.DynamoDbClient.Fluent.Batch;

public class FluentBatch
{
    private readonly IDynamoDbWrapper _db;
    private List<DbBatchReadRequest> _requests = new List<DbBatchReadRequest>();

    internal FluentBatch(IDynamoDbWrapper db)
    {
        _db = db;
    }

    public FluentBatch KeyPairs<T>(params DynamoDbKeyPair[] keys)
    {
        if (keys == null || keys.Length == 0) return this;

        _requests.Add(new DbBatchReadRequest<T>(keys));

        return this;
    }
    public FluentBatch KeyPair<T>(object partitionKey, object sortKey)
    {
        _requests.Add(new DbBatchReadRequest<T>(new DynamoDbKeyPair(partitionKey, sortKey)));
        return this;
    }

    public Task<DbBatchReadResponse> Execute(CancellationToken ct = default)
    {
        return _db.GetItemsInBatch(_requests, ct);
    }
}
