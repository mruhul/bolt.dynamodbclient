using Bolt.DynamoDbClient.DistributedLock;
using Bolt.DynamoDbClient.Fluent.Batch;
using Bolt.DynamoDbClient.Fluent.Lock;
using Bolt.DynamoDbClient.Fluent.Query;
using Bolt.DynamoDbClient.Fluent.Transaction;

namespace Bolt.DynamoDbClient
{
    public static class DynamoDbWrapperExtensions
    {
        /// <summary>
        /// Retrieve an item by partition key and sort key. return null when doesn't exist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="partitionKey"></param>
        /// <param name="sortKey"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<T?> GetSingleItem<T>(this IDynamoDbWrapper db, object partitionKey, object sortKey,
            CancellationToken ct) where T : new()
            => db.GetSingleItem<T>(new GetSingleItemRequest(partitionKey, sortKey), ct);

        /// <summary>
        /// Retrieve an item by partition key of type string and sort key of type string. return null when doesn't exist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="partitionKey"></param>
        /// <param name="sortKey"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<T?> GetSingleItem<T>(this IDynamoDbWrapper db, string partitionKey, string sortKey,
            CancellationToken ct) where T : new()
            => db.GetSingleItem<T>(new GetSingleItemRequest(partitionKey, sortKey), ct);

        /// <summary>
        /// Delete an item by partition key and sortkey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="partitionKey"></param>
        /// <param name="sortKey"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task Delete<T>(this IDynamoDbWrapper db, string partitionKey, string sortKey,
            CancellationToken ct) where T : new()
            => db.Delete<T>(new DeleteSingleItemRequest(partitionKey, sortKey), ct);

        /// <summary>
        /// Delete an item by partition key and sortkey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="partitionKey"></param>
        /// <param name="sortKey"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task Delete<T>(this IDynamoDbWrapper db, object partitionKey, object sortKey,
            CancellationToken ct) where T : new()
            => db.Delete<T>(new DeleteSingleItemRequest(partitionKey, sortKey), ct);

        /// <summary>
        /// Provide a fluent option to query items
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IDynamoDbFluentQuery Query(this IDynamoDbWrapper db)
            => new FluentQuery(db);

        /// <summary>
        /// Provide a fluent option to read items in batch
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static FluentBatch Batch(this IDynamoDbWrapper db) => new FluentBatch(db);
        /// <summary>
        /// Provide a fluent option to write items in transaction
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static FluentTransaction Transaction(this IDynamoDbWrapper db) => new FluentTransaction(db);

        /// <summary>
        /// Create instance of distributed lock based on supplied key, token and provide fluent interface
        /// to execute a task inside a lock
        /// </summary>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IHaveFluentLockKey LockBy(this IDistributedLock source, string key, string token) =>
            new FluentLock(source, key, token);
        
        /// <summary>
        /// Create instance of distributed lock based on supplied key and provide fluent interface
        /// to execute a task inside a lock
        /// </summary>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IHaveFluentLockKey LockBy(this IDistributedLock source, string key) =>
            new FluentLock(source, key, Guid.NewGuid().ToString());
    }
}
