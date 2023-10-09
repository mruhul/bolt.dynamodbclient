namespace Bolt.DynamoDbClient.DistributedLock;

public record LockTableSettings
{
    public required string TableName { get; init; }
    public string PartitionKeyColumnName { get; init; } = "PK";
    public string SortKeyColumnName { get; init; } = "SK";
    public string TimeToLiveColumnName { get; init; } = "PurgeAfter";
}