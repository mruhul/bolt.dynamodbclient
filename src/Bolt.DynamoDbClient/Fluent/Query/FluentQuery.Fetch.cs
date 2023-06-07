using Amazon.DynamoDBv2.Model;

namespace Bolt.DynamoDbClient.Fluent.Query;

internal sealed partial class FluentQuery : IDynamoDbFetchData
{
    public Task<DbSearchResponse<T>> Fetch<T>(CancellationToken ct) where T : new()
    {
        if (_partitionKeyAttributeValue == null) throw new ArgumentNullException("PartitionKeyValue not provided");

        var metaData = DynamoDbItemMetaDataReader.Get(typeof(T));
        var partitionKeyColumnName = string.IsNullOrWhiteSpace(_partitionKeyColumnName) ? metaData.PartitionKeyColumnName : _partitionKeyColumnName;

        var expressionAttributeNames = new Dictionary<string, string>();

        string sortKeyColumnName = string.Empty;
        string sortKeyExpression = string.Empty;
        if (_sortKeyAttributeValue != null)
        {
            sortKeyColumnName = string.IsNullOrWhiteSpace(_sortKeyColumnName) ? metaData.SortKeyColumnName : _sortKeyColumnName;
            sortKeyExpression = BuildSortKeyExpression(_operatorType, sortKeyColumnName ?? string.Empty);
            expressionAttributeNames[$"#{sortKeyColumnName}"] = sortKeyColumnName;
        }
        var partitionKeyExpression = string.Format("#{0} = :{0}", partitionKeyColumnName);

        expressionAttributeNames[$"#{partitionKeyColumnName}"] = partitionKeyColumnName;

        var expressionAttributeValues = new Dictionary<string, AttributeValue>();

        expressionAttributeValues[$":{partitionKeyColumnName}"] = _partitionKeyAttributeValue;

        if (_sortKeyAttributeValue != null)
        {
            if (_sortKeyEndRangeAttributeValue != null)
            {
                expressionAttributeValues[$":{sortKeyColumnName}From"] = _sortKeyAttributeValue;
                expressionAttributeValues[$":{sortKeyColumnName}To"] = _sortKeyEndRangeAttributeValue;
            }
            else
            {
                expressionAttributeValues[$":{sortKeyColumnName}"] = _sortKeyAttributeValue;
            }
        }

        return _db.Query<T>(new DbSearchRequest
            (
                KeyConditionExpression: _sortKeyAttributeValue != null ? 
                    string.Format("{0} and {1}", partitionKeyExpression, sortKeyExpression)
                    : partitionKeyExpression,
                IndexName: _indexName,
                ProjectionExpression: null,
                ExclusiveStartKey: _exclusiveStartKey,
                ConsistentRead: _consistentRead,
                FilterExpression: null,
                Limit: _limit,
                ScanIndexForward: _scanIndexForward,                
                ExpressionAttributeNames: expressionAttributeNames,
                ExpressionAttributeValues: expressionAttributeValues
            ), ct);
    }


    private string BuildSortKeyExpression(QueryOperatorType operatorType, string sortKeyColumnName)
    {
        return operatorType switch
        {
            QueryOperatorType.Equals => $"#{sortKeyColumnName} = :{sortKeyColumnName}",
            QueryOperatorType.LessThan => $"#{sortKeyColumnName} < :{sortKeyColumnName}",
            QueryOperatorType.LessThanOrEquals => $"#{sortKeyColumnName} <= :{sortKeyColumnName}",
            QueryOperatorType.GreaterThan => $"#{sortKeyColumnName} > :{sortKeyColumnName}",
            QueryOperatorType.GreaterThanOrEquals => $"#{sortKeyColumnName} >= :{sortKeyColumnName}",
            QueryOperatorType.Between => $"#{sortKeyColumnName} BETWEEN :{sortKeyColumnName}From AND :{sortKeyColumnName}To",
            QueryOperatorType.BeginsWith => $"begins_with(#{sortKeyColumnName}, :{sortKeyColumnName})",
            _ => $"#{sortKeyColumnName} = :{sortKeyColumnName}"
        };
    }
}

public interface IDynamoDbFetchData
{
    Task<DbSearchResponse<T>> Fetch<T>(CancellationToken ct = default) where T:new();
}