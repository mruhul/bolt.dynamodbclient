namespace Bolt.DynamoDbClient.Fluent.Transaction;

public class FluentTransaction
{
    private readonly IDynamoDbWrapper _db;
    private List<WriteItemRequest> _requests;

    internal FluentTransaction(IDynamoDbWrapper db)
    {
        _db = db;
        _requests = new List<WriteItemRequest>();
    }

    public FluentTransaction Create<T>(T item)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));

        _requests.Add(new TransactCreateItemRequest(item));

        return this;
    }

    public FluentTransaction Update<T>(T item, bool skipNullValue = false)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));

        _requests.Add(new TransactUpdateItemRequest(item, skipNullValue));

        return this;
    }

    public FluentTransaction Upsert<T>(T item)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));

        _requests.Add(new TransactUpsertItemRequest(item));

        return this;
    }

    public FluentTransaction Delete<T>(object pk, object sk)
    {
        if (pk is null) throw new ArgumentNullException(nameof(pk));
        if (sk is null) throw new ArgumentNullException(nameof(sk));

        _requests.Add(new TransactDeleteItemRequest<T>(pk, sk));

        return this;
    }

    public FluentTransaction Increment<T>(string propertyName, object pk, object sk, int incrementBy = 1)
    {
        if (pk is null) throw new ArgumentNullException(nameof(pk));
        if (sk is null) throw new ArgumentNullException(nameof(sk));

        _requests.Add(new TransactIncrementRequest 
        { 
            ItemType = typeof(T),
            PartitionKey = pk,
            SortKey = sk,
            IncrementBy = incrementBy,
            PropertyName = propertyName,
        });

        return this;
    }

    public Task Execute(CancellationToken ct = default)
    {
        return _db.WriteItemsInTransaction(_requests, ct);
    }
}
