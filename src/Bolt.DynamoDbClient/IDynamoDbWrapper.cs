using Amazon.DynamoDBv2.Model;

namespace Bolt.DynamoDbClient;

public interface IDynamoDbWrapper
{
    /// <summary>
    /// Retrieve a single item from table based on provided partition key and sort key. Will return null when item doesn't found
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="getSingleItem"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<T?> GetSingleItem<T>(GetSingleItemRequest getSingleItem, CancellationToken ct = default) where T : new();
    /// <summary>
    /// Retrieve items of different type from one or more tables
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<DbBatchReadResponse> GetItemsInBatch(IEnumerable<DbBatchReadRequest> requests, CancellationToken ct = default);
    /// <summary>
    /// Write more that one item in a transaction. Either all succeed or none
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task WriteItemsInTransaction(IEnumerable<WriteItemRequest> requests, CancellationToken ct = default);
    /// <summary>
    /// Create a new item if doesn't exist, otherwise just update the item.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Upsert(object item, CancellationToken ct = default);
    /// <summary>
    /// Create a new item. If an item with same key already exists then ignore.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> Create(object item, CancellationToken ct = default);
    /// <summary>
    /// Update an existing item
    /// </summary>
    /// <param name="item"></param>
    /// <param name="skipNullValues"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Update(object item, bool skipNullValues, CancellationToken ct = default);
    /// <summary>
    /// Delete an item by partition key and sort key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Delete<T>(DeleteSingleItemRequest item, CancellationToken ct = default);

    Task<DbSearchResponse<T>> Query<T>(DbSearchRequest request, CancellationToken ct = default) where T : new();
    
    /// <summary>
    /// Inrement value of an item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Increment<T>(IncrementRequest request, CancellationToken ct);
}

public record DbSearchRequest
    (
        string KeyConditionExpression,
        string? IndexName = null,
        string? ProjectionExpression = null,
        Dictionary<string, AttributeValue>? ExclusiveStartKey = null,
        bool? ConsistentRead = null ,
        string? FilterExpression = null ,
        int? Limit = null ,
        bool? ScanIndexForward = null ,
        Dictionary<string, string>? ExpressionAttributeNames = null,
        Dictionary<string, AttributeValue>? ExpressionAttributeValues = null
    );

public record DbSearchResponse<T>
    (
        IEnumerable<T> Items,
        int Count,
        Dictionary<string, AttributeValue> LastEvaluatedKey,
        int ScannedCount
    );