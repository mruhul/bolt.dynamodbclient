using System.ComponentModel;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Text;
using Bolt.DynamoDbClient.Lock;

namespace Bolt.DynamoDbClient;

internal class DynamoDbWrapper : IDynamoDbWrapper
{
    private readonly IAmazonDynamoDB _db;
    private readonly DistributedLock _distributedLock;

    public DynamoDbWrapper(IAmazonDynamoDB db, DistributedLock distributedLock)
    {
        _db = db;
        _distributedLock = distributedLock;
    }

    public async Task<DbSearchResponse<T>> Query<T>(DbSearchRequest request, CancellationToken ct) where T : new()
    {
        var metaData = DynamoDbItemMetaDataReader.Get(typeof(T));

        var attributeNames = request.ExpressionAttributeNames;

        string? projectedExpression = request.ProjectionExpression;
        if (string.IsNullOrWhiteSpace(projectedExpression))
        {
            StringBuilder projectionExpressionSb = new StringBuilder();

            var firstItem = true;
            foreach (var prop in metaData.Properties)
            {
                if (prop.ProjectionColumnName != null)
                {
                    attributeNames ??= new Dictionary<string, string>();

                    attributeNames[prop.ProjectionColumnName] = prop.ColumnName;
                }

                if (firstItem)
                {
                    firstItem = false;

                    projectionExpressionSb.Append(prop.ProjectionColumnName ?? prop.ColumnName);
                }
                else
                {
                    projectionExpressionSb.Append(", ").Append(prop.ProjectionColumnName ?? prop.ColumnName);
                }
            }

            projectedExpression = projectionExpressionSb.ToString();
        }

        var rsp = await _db.QueryAsync(new QueryRequest
        {
            ProjectionExpression = projectedExpression,
            ExclusiveStartKey = request.ExclusiveStartKey,
            ConsistentRead = request.ConsistentRead ?? false,
            FilterExpression = request.FilterExpression,
            IndexName = request.IndexName,
            TableName = metaData.TableName,
            KeyConditionExpression = request.KeyConditionExpression,
            ExpressionAttributeNames = attributeNames,
            ExpressionAttributeValues = request.ExpressionAttributeValues,
            Limit = request.Limit ?? 50,
            ScanIndexForward = request.ScanIndexForward ?? false
        }, ct);

        return new DbSearchResponse<T>
        (
            Items: MapFromItems<T>(rsp.Items),
            Count: rsp.Count,
            LastEvaluatedKey: rsp.LastEvaluatedKey,
            ScannedCount: rsp.ScannedCount
        );
    }

    public async Task<T?> GetSingleItem<T>(GetSingleItemRequest getSingleItem, CancellationToken ct) where T : new()
    {
        var metaData = DynamoDbItemMetaDataReader.Get(typeof(T));
        var rsp = await _db.GetItemAsync(new GetItemRequest
        {
            TableName = metaData.TableName,
            Key = BuildKeyAttributeValues(metaData, getSingleItem.PartitionKey, getSingleItem.SortKey),
            ConsistentRead = getSingleItem.ConsistentRead
        }, ct);

        return rsp.Item.MapTo<T>();
    }

    public async Task<DbBatchReadResponse> GetItemsInBatch(IEnumerable<DbBatchReadRequest> requests,
        CancellationToken ct)
    {
        var requestItems = new Dictionary<string, KeysAndAttributes>();
        var tableKeyNames = new Dictionary<string, (string PartitionKeyColumnName,
            Type? ParitionKeyPropertyType,
            string SortKeyColumnName,
            Type? SortKeyPropertyType)>();
        var itemTypes = new Dictionary<string, Type>();
        var columnsToRead = new Dictionary<string, Dictionary<string, string>>();

        foreach (var req in requests)
        {
            var metaData = DynamoDbItemMetaDataReader.Get(req.ItemType);

            tableKeyNames[metaData.TableName] = (metaData.PartitionKeyColumnName,
                metaData.PartitionKeyProperty?.PropertyType,
                metaData.SortKeyColumnName,
                metaData.SortKeyProperty?.PropertyType);

            if (requestItems.TryGetValue(metaData.TableName, out var keyAndAttributes) == false)
            {
                keyAndAttributes = new KeysAndAttributes();
                requestItems[metaData.TableName] = keyAndAttributes;
                columnsToRead[metaData.TableName] = new Dictionary<string, string>();
            }

            foreach (var prop in metaData.Properties)
            {
                columnsToRead[metaData.TableName][prop.ColumnName] = prop.ColumnName;
            }

            foreach (var keys in req.Keys)
            {
                if (metaData.PartitionKeyProperty != null && metaData.SortKeyProperty != null)
                {
                    var partionKeyAttValue =
                        BuildAttributeValue(metaData.PartitionKeyProperty.PropertyType, keys.PartitionKey);
                    var sortKeyAttValue = BuildAttributeValue(metaData.SortKeyProperty.PropertyType, keys.SortKey);

                    if (partionKeyAttValue != null && sortKeyAttValue != null)
                    {
                        itemTypes[$"{metaData.TableName}:{keys.PartitionKey}:{keys.SortKey}"] = req.ItemType;

                        keyAndAttributes.Keys.Add(new Dictionary<string, AttributeValue>
                        {
                            [metaData.PartitionKeyColumnName] = partionKeyAttValue,
                            [metaData.SortKeyColumnName] = sortKeyAttValue
                        });
                    }
                }
            }
        }

        foreach (var item in requestItems)
        {
            item.Value.ProjectionExpression = string.Join(", ", columnsToRead[item.Key].Keys.ToArray());
        }

        var rsp = await _db.BatchGetItemAsync(requestItems, ct);

        var result = new Dictionary<Type, Dictionary<string, Dictionary<string, AttributeValue>>>();

        foreach (var table in rsp.Responses)
        {
            var (pkName, pkType, skName, skType) = tableKeyNames[table.Key];

            foreach (var row in table.Value)
            {
                var pkValue = GetKeyValue(row, pkName, pkType);
                var skValue = GetKeyValue(row, skName, skType);

                var rowKey = $"{table.Key}:{pkValue}:{skValue}";

                if (itemTypes.TryGetValue(rowKey, out var type))
                {
                    if (result.TryGetValue(type, out var tableRows) == false)
                    {
                        tableRows = new Dictionary<string, Dictionary<string, AttributeValue>>();
                        result[type] = tableRows;
                    }

                    tableRows[$"{pkValue}|{skValue}"] = row;
                }
            }
        }

        return new DbBatchReadResponse(result);
    }

    public async Task Upsert(object item, bool skipNullValues, CancellationToken ct)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        var metaData = DynamoDbItemMetaDataReader.Get(item.GetType());

        await _db.PutItemAsync(BuildUpsertRequest(metaData, item, skipNullValues), ct);
    }

    private IEnumerable<T> MapFromItems<T>(List<Dictionary<string, AttributeValue>> source) where T : new()
    {
        if (source == null || source.Count == 0) yield break;

        foreach (var item in source)
        {
            var resultItem = item.MapTo<T>();
            if (resultItem == null) continue;
            yield return resultItem;
        }
    }


    public async Task<bool> Create(object item, CancellationToken ct)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        var metaData = DynamoDbItemMetaDataReader.Get(item.GetType());

        try
        {
            await _db.PutItemAsync(BuildCreateRequest(metaData, item), ct);

            return true;
        }
        catch (ConditionalCheckFailedException)
        {
            return false;
        }
    }

    public async Task Update(object item, bool skipNullValues, CancellationToken ct)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        var metaData = DynamoDbItemMetaDataReader.Get(item.GetType());

        var request = BuildUpdateItemRequest(metaData, item, skipNullValues);

        if (request == null) return; // Nothing to update

        await _db.UpdateItemAsync(request, ct);
    }

    public async Task Increment<T>(IncrementRequest request, CancellationToken ct)
    {
        Ensure.ThrowIfNull(request, nameof(request));

        var metaData = DynamoDbItemMetaDataReader.Get(typeof(T));

        var expressionAttributeNames = new Dictionary<string, string>(request.PropertyValues.Length);

        var expressionAttributeValues = new Dictionary<string, AttributeValue>(request.PropertyValues.Length + 1);

        expressionAttributeValues[":start"] = new AttributeValue { N = "0" };

        var updateExpressions = new string[request.PropertyValues.Length];

        var index = 0;
        foreach (var prop in request.PropertyValues)
        {
            var propAlias = $"#prop{index}";
            var propIncrementAlias = $":incr{index}";

            expressionAttributeNames[propAlias] = prop.PropertyName;
            expressionAttributeValues[propIncrementAlias] = new AttributeValue
            {
                N = prop.IncrementBy.ToString()
            };
            updateExpressions[index] =
                $"{(index == 0 ? "SET " : string.Empty)}{propAlias} = if_not_exists({propAlias}, :start) + {propIncrementAlias}";
            index++;
        }


        await _db.UpdateItemAsync(new()
        {
            TableName = metaData.TableName,
            Key = BuildKeyAttributeValues(metaData, request.PartitionKey, request.SortKey),
            ExpressionAttributeNames = expressionAttributeNames,
            ExpressionAttributeValues = expressionAttributeValues,
            UpdateExpression = updateExpressions.Length == 1
                ? updateExpressions[0]
                : string.Join(", ", updateExpressions),
        }, ct);
    }

    public Task<bool> Acquire(string key, string token, TimeSpan duration, CancellationToken ct = default)
    {
        return _distributedLock.Acquire(key, token, duration, ct);
    }

    public Task<bool> Release(string key, string token, CancellationToken ct = default)
    {
        return _distributedLock.Release(key, token, ct);
    }

    public async Task Delete<T>(DeleteSingleItemRequest item, CancellationToken ct)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        var metaData = DynamoDbItemMetaDataReader.Get(typeof(T));

        await _db.DeleteItemAsync(new DeleteItemRequest
        {
            TableName = metaData.TableName,
            Key = BuildKeyAttributeValues(metaData, item.partitionKey, item.sortKey)
        }, ct);
    }

    public async Task WriteItemsInTransaction(IEnumerable<WriteItemRequest> requests, CancellationToken ct)
    {
        var transactItems = new List<TransactWriteItem>();

        foreach (var request in requests)
        {
            var req = BuildTransactWriteItem(request);

            if (req != null) transactItems.Add(req);
        }

        await _db.TransactWriteItemsAsync(new TransactWriteItemsRequest()
        {
            TransactItems = transactItems
        }, ct);
    }

    public async Task<IEnumerable<T>> Query<T>(QueryRequest request, CancellationToken ct) where T : new()
    {
        var rsp = await _db.QueryAsync(request, ct);

        return rsp.Items?.Select(x => x.MapTo<T>()!) ?? Enumerable.Empty<T>();
    }

    private TransactWriteItem? BuildTransactWriteItem(WriteItemRequest request)
    {
        if (request is TransactCreateItemRequest createRequest)
        {
            return BuildTransactionWriteItem(createRequest);
        }

        if (request is TransactUpsertItemRequest upsertRequest)
        {
            return BuildTransactionWriteItem(upsertRequest);
        }

        if (request is TransactUpdateItemRequest updateRequest)
        {
            return BuildTransactionWriteItem(updateRequest);
        }

        if (request is TransactIncrementRequest incrementRequest)
        {
            return BuildTransactionWriteItem(incrementRequest);
        }

        if (request is TransactDeleteItemRequest deleteRequest)
        {
            var metaData = DynamoDbItemMetaDataReader.Get(deleteRequest.ItemType);

            var partitionKeyAtt = metaData.PartitionKeyProperty != null
                ? BuildAttributeValue(metaData.PartitionKeyProperty.PropertyType, deleteRequest.PartitionKey)
                : null;
            var sortKeyAtt = metaData.SortKeyProperty != null
                ? BuildAttributeValue(metaData.SortKeyProperty.PropertyType, deleteRequest.SortKey)
                : null;

            if (partitionKeyAtt != null && sortKeyAtt != null)
            {
                return new TransactWriteItem()
                {
                    Delete = new()
                    {
                        TableName = metaData.TableName,
                        Key = new Dictionary<string, AttributeValue>
                        {
                            [metaData.PartitionKeyColumnName] = partitionKeyAtt,
                            [metaData.SortKeyColumnName] = sortKeyAtt
                        }
                    }
                };
            }
        }

        throw new Exception("write request type not supported");
    }

    private string? GetKeyValue(Dictionary<string, AttributeValue> row, string key, Type? type)
    {
        if (row.TryGetValue(key, out var value))
        {
            if (type is null) return value.S;

            if (type == typeof(int)
                || type == typeof(double)
                || type == typeof(decimal)
                || type == typeof(long))
            {
                return value.N;
            }

            return value.S;
        }

        return null;
    }


    private TransactWriteItem? BuildTransactionWriteItem(TransactIncrementRequest request)
    {
        Ensure.ThrowIfNull(request, nameof(request));

        var metaData = DynamoDbItemMetaDataReader.Get(request.ItemType);

        var partitionKeyAtt = metaData.PartitionKeyProperty != null
            ? BuildAttributeValue(metaData.PartitionKeyProperty.PropertyType, request.PartitionKey)
            : null;
        var sortKeyAtt = metaData.SortKeyProperty != null
            ? BuildAttributeValue(metaData.SortKeyProperty.PropertyType, request.SortKey)
            : null;

        if (partitionKeyAtt == null || sortKeyAtt == null) return null;

        var expressionAttributeNames = new Dictionary<string, string>(request.PropertyValues.Length);

        var expressionAttributeValues = new Dictionary<string, AttributeValue>(request.PropertyValues.Length + 1);

        expressionAttributeValues[":start"] = new AttributeValue { N = "0" };

        var updateExpressions = new string[request.PropertyValues.Length];

        var index = 0;
        foreach (var prop in request.PropertyValues)
        {
            var propAlias = $"#prop{index}";
            var propIncrementAlias = $":incr{index}";

            expressionAttributeNames[propAlias] = prop.PropertyName;
            expressionAttributeValues[propIncrementAlias] = new AttributeValue
            {
                N = prop.IncrementBy.ToString()
            };
            updateExpressions[index] =
                $"{(index == 0 ? "SET " : string.Empty)}{propAlias} = if_not_exists({propAlias}, :start) + {propIncrementAlias}";
            index++;
        }

        return new TransactWriteItem()
        {
            Update = new Update
            {
                TableName = metaData.TableName,
                Key = BuildKeyAttributeValues(metaData, request.PartitionKey, request.SortKey),
                UpdateExpression = updateExpressions.Length == 1
                    ? updateExpressions[0]
                    : string.Join(", ", updateExpressions),
                ExpressionAttributeNames = expressionAttributeNames,
                ExpressionAttributeValues = expressionAttributeValues
            }
        };
    }


    private TransactWriteItem BuildTransactionWriteItem(TransactCreateItemRequest createRequest)
    {
        var metaData = DynamoDbItemMetaDataReader.Get(createRequest.Item.GetType());
        var dbReq = BuildCreateRequest(metaData, createRequest.Item);
        return new TransactWriteItem()
        {
            Put = new Put
            {
                TableName = dbReq.TableName,
                Item = dbReq.Item,
                ConditionExpression = dbReq.ConditionExpression,
                ExpressionAttributeNames = dbReq.ExpressionAttributeNames
            }
        };
    }

    private TransactWriteItem BuildTransactionWriteItem(TransactUpsertItemRequest upsertRequest)
    {
        var metaData = DynamoDbItemMetaDataReader.Get(upsertRequest.Item.GetType());
        var dbReq = BuildUpsertRequest(metaData, upsertRequest.Item, upsertRequest.SkipNullValue);
        return new TransactWriteItem()
        {
            Put = new Put
            {
                TableName = dbReq.TableName,
                Item = dbReq.Item
            }
        };
    }


    private TransactWriteItem? BuildTransactionWriteItem(TransactUpdateItemRequest updateRequest)
    {
        var metaData = DynamoDbItemMetaDataReader.Get(updateRequest.Item.GetType());
        var updateDbReq = BuildUpdateItemRequest(metaData, updateRequest.Item, updateRequest.SkipNullValue);
        if (updateDbReq != null)
        {
            return new TransactWriteItem()
            {
                Update = new()
                {
                    TableName = updateDbReq.TableName,
                    Key = updateDbReq.Key,
                    ExpressionAttributeNames = updateDbReq.ExpressionAttributeNames,
                    ExpressionAttributeValues = updateDbReq.ExpressionAttributeValues,
                    UpdateExpression = updateDbReq.UpdateExpression
                }
            };
        }

        return null;
    }

    private PutItemRequest BuildCreateRequest(DynamoDbItemMetaData metaData, object item)
    {
        return new PutItemRequest
        {
            TableName = metaData.TableName,
            Item = BuildUpsertAttributes(metaData, item, true),
            ConditionExpression = "attribute_not_exists(#pk) and attribute_not_exists(#sk)",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                ["#pk"] = metaData.PartitionKeyColumnName,
                ["#sk"] = metaData.SortKeyColumnName,
            }
        };
    }

    private PutItemRequest BuildUpsertRequest(DynamoDbItemMetaData metaData, object item, bool skipNullValues)
    {
        return new PutItemRequest
        {
            TableName = metaData.TableName,
            Item = BuildUpsertAttributes(metaData, item, skipNullValues)
        };
    }

    private UpdateItemRequest? BuildUpdateItemRequest(DynamoDbItemMetaData metaData, object item, bool skipNullValues)
    {
        var setExpression = new StringBuilder();
        var removeExpression = skipNullValues ? null : new StringBuilder();
        var keys = new Dictionary<string, AttributeValue>();
        var updateAttributeValues = new Dictionary<string, AttributeValue>();
        var updateAttributeNames = new Dictionary<string, string>();

        var firstSetItem = true;
        var firstRemoveItem = true;
        bool hasRemoveItem = false;
        var hasSetItem = false;
        var isIncrementApplicable = false;

        foreach (var prop in metaData.Properties)
        {
            if (prop.ColumnType != DynamoDbColumnType.Default)
            {
                var value = BuildAttributeValue(prop, prop.PropertyInfo.GetValue(item));
                if (value != null)
                {
                    keys[prop.ColumnName] = value;
                }

                continue;
            }

            if (prop.IgnoreInstruction == DynamoDbOperationIgnoreInstructionType.Always
                || prop.IgnoreInstruction == DynamoDbOperationIgnoreInstructionType.Update) continue;

            var attValue = BuildAttributeValue(prop, prop.PropertyInfo.GetValue(item));

            if (attValue == null)
            {
                if (skipNullValues) continue;

                updateAttributeNames.Add($"#{prop.ColumnName}", prop.ColumnName);

                if (firstRemoveItem)
                {
                    firstRemoveItem = false;
                    hasRemoveItem = true;
                    removeExpression?.Append($"REMOVE #{prop.ColumnName}");
                }
                else
                {
                    removeExpression?.Append($", #{prop.ColumnName}");
                }
            }
            else
            {
                updateAttributeNames.Add($"#{prop.ColumnName}", prop.ColumnName);
                updateAttributeValues.Add($":{prop.ColumnName}", attValue);

                if (prop.UseValueToIncrement && isIncrementApplicable == false)
                {
                    isIncrementApplicable = true;
                    updateAttributeValues[":start"] = new AttributeValue { N = "0" };
                }


                if (firstSetItem)
                {
                    firstSetItem = false;
                    hasSetItem = true;

                    if (prop.UseValueToIncrement)
                    {
                        setExpression.Append($"SET #{prop.ColumnName} = if_not_exists(#{prop.ColumnName}, :start) + :{prop.ColumnName}");
                    }
                    else
                    {
                        setExpression.Append($"SET #{prop.ColumnName} = :{prop.ColumnName}");
                    }
                }
                else
                {
                    if (prop.UseValueToIncrement)
                    {
                        setExpression.Append($", #{prop.ColumnName} = if_not_exists(#{prop.ColumnName}, :start) + :{prop.ColumnName}");
                    }
                    else
                    {
                        setExpression.Append($", #{prop.ColumnName} = :{prop.ColumnName}");
                    }
                }
            }
        }

        string? updateExpression;

        if (hasSetItem && hasRemoveItem)
        {
            updateExpression = setExpression.Append(" ").Append(removeExpression).ToString();
        }
        else if (hasSetItem)
        {
            updateExpression = setExpression.ToString();
        }
        else if (hasRemoveItem)
        {
            updateExpression = removeExpression?.ToString() ?? string.Empty;
        }
        else
        {
            return null; // Nothing to update
        }

        return new UpdateItemRequest()
        {
            TableName = metaData.TableName,
            Key = keys,
            UpdateExpression = updateExpression,
            ExpressionAttributeNames = updateAttributeNames,
            ExpressionAttributeValues = updateAttributeValues,
        };
    }

    private Dictionary<string, AttributeValue> BuildUpsertAttributes(DynamoDbItemMetaData metaData, object item,
        bool skipNullValues)
    {
        var result = new Dictionary<string, AttributeValue>();

        foreach (var prop in metaData.Properties)
        {
            if (prop.IgnoreInstruction == DynamoDbOperationIgnoreInstructionType.Always
                || prop.IgnoreInstruction == DynamoDbOperationIgnoreInstructionType.Upsert)
            {
                continue;
            }

            var value = prop.PropertyInfo.GetValue(item, null);

            var attValue = BuildAttributeValue(prop, value);

            if (attValue == null)
            {
                if (skipNullValues) continue;

                attValue = new AttributeValue
                {
                    NULL = true
                };
            }

            result[prop.ColumnName] = attValue;
        }

        return result;
    }

    private Dictionary<string, AttributeValue> BuildKeyAttributeValues(DynamoDbItemMetaData metaData,
        object partitionKeyValue, object sortKeyValue)
    {
        if (metaData.PartitionKeyProperty == null || metaData.SortKeyProperty == null)
        {
            throw new ArgumentNullException("Partition key property and sort key property cannot be null in metadata");
        }

        var partitionAtt = BuildAttributeValue(metaData.PartitionKeyProperty.PropertyType, partitionKeyValue);
        var sortAtt = BuildAttributeValue(metaData.SortKeyProperty.PropertyType, sortKeyValue);

        if (partitionAtt == null || sortAtt == null)
            throw new ArgumentNullException("Partition key and sort key cannot be null");

        return new Dictionary<string, AttributeValue>
        {
            [metaData.PartitionKeyColumnName] = partitionAtt,
            [metaData.SortKeyColumnName] = sortAtt,
        };
    }

    private AttributeValue? BuildAttributeValue(Type propertyType, object? value)
    {
        return BuildAttributeValue(propertyType,
            new() { Type = propertyType, IsCollection = false, IsSimpleType = true }, value);
    }

    private AttributeValue? BuildAttributeValue(DynamoDbItemMetaProperty prop, object? value)
    {
        return BuildAttributeValue(prop.PropertyInfo.PropertyType, prop.TypeMetaData, value);
    }

    private AttributeValue? BuildAttributeValue(Type type, TypeInfoMetaData typeMetaData, object? value)
    {
        if (value is null) return null;

        return AttributeValueMapper.MapFrom(value);
    }

    private AttributeValue? BuildAttributeNsValueForArray<T>(object value)
    {
        var collection = value as IEnumerable<T>;

        if (collection != null)
        {
            var items = new List<string>();

            foreach (var item in collection)
            {
                if (item == null) continue;

                items.Add(item?.ToString() ?? string.Empty);
            }

            return new AttributeValue
            {
                NS = items
            };
        }

        return null;
    }

    private const string UtcFormat = "o";

    private static string FormatAsUtc(DateTime source)
        => source.Kind == DateTimeKind.Utc
            ? source.ToString(UtcFormat)
            : source.ToUniversalTime().ToString(UtcFormat);
}

public record DeleteSingleItemRequest(object partitionKey, object sortKey);

public record GetSingleItemRequest(object PartitionKey, object SortKey)
{
    public bool ConsistentRead { get; init; } = false;
};

public record IncrementRequest
{
    public object PartitionKey { get; init; } = string.Empty;
    public object SortKey { get; init; } = string.Empty;
    public PropertyIncrementValue[] PropertyValues { get; init; } = [];
}

public record PropertyIncrementValue
{
    public PropertyIncrementValue()
    {
    }

    public PropertyIncrementValue(string propertyName, int incrementBy)
    {
        PropertyName = propertyName;
        IncrementBy = incrementBy;
    }

    public string PropertyName { get; init; } = string.Empty;
    public int IncrementBy { get; init; }
}

public abstract record WriteItemRequest
{
}

public record TransactCreateItemRequest(object Item) : WriteItemRequest;

public record TransactUpdateItemRequest(object Item, bool SkipNullValue) : WriteItemRequest;

public record TransactUpsertItemRequest(object Item, bool SkipNullValue) : WriteItemRequest;

public record TransactIncrementRequest : WriteItemRequest
{
    public Type ItemType { get; init; } = null!;
    public object PartitionKey { get; init; } = string.Empty;
    public object SortKey { get; init; } = string.Empty;

    public PropertyIncrementValue[] PropertyValues { get; init; } = [];
}

public abstract record TransactDeleteItemRequest(Type ItemType, object PartitionKey, object SortKey) : WriteItemRequest;

public record TransactDeleteItemRequest<T>(object PartitionKey, object SortKey)
    : TransactDeleteItemRequest(typeof(T), PartitionKey, SortKey);

public enum WriteOperationType
{
    Create,
    Update,
    Upsert,
    Delete
}

public record DbBatchReadRequest
{
    public DbBatchReadRequest(Type itemType, params DynamoDbKeyPair[] keys)
    {
        ItemType = itemType;
        Keys = keys;
    }

    public Type ItemType { get; init; }
    public DynamoDbKeyPair[] Keys { get; init; }
}

public record DbBatchReadRequest<T> : DbBatchReadRequest
{
    public DbBatchReadRequest(params DynamoDbKeyPair[] keys) : base(typeof(T), keys)
    {
    }
}

public record DbBatchReadResponse
{
    private readonly Dictionary<Type, Dictionary<string, Dictionary<string, AttributeValue>>> _responses;

    public DbBatchReadResponse(Dictionary<Type, Dictionary<string, Dictionary<string, AttributeValue>>> responses)
    {
        _responses = responses;
    }

    public T? GetSingle<T>(string partitionKey, string sortKey) where T : new()
    {
        return GetSingle<T, string, string>(partitionKey, sortKey);
    }

    public T? GetSingle<T, Pk, Sk>(Pk partitionKey, Sk sortKey) where T : new()
    {
        var type = typeof(T);

        if (_responses.TryGetValue(type, out var rows))
        {
            if (rows.TryGetValue($"{partitionKey}|{sortKey}", out var row))
            {
                return row.MapTo<T>();
            }
        }

        return default;
    }

    public IEnumerable<T> GetAll<T>() where T : new()
    {
        var type = typeof(T);

        if (_responses.TryGetValue(type, out var rows))
        {
            foreach (var row in rows)
            {
                var item = row.Value.MapTo<T>();

                if (item != null) yield return item;
            }
        }
    }
}

public record DynamoDbKeyPair(object PartitionKey, object SortKey);