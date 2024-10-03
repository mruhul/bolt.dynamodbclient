namespace Bolt.DynamoDbClient.Lock;

public record LockTableSettings
{
    public string TableName { get; init; } = string.Empty;
    public string PartitionKeyColumnName { get; init; } = "PK";
    public string SortKeyColumnName { get; init; } = "SK";
    public string TimeToLiveColumnName { get; init; } = "PurgeAfter";
}