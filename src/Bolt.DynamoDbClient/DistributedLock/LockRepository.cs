using System.Net;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Bolt.DynamoDbClient.DistributedLock;

internal class CreateLockRecordRequest
{
    public string Key { get; init; }
    public string Token { get; init; }
    
    public long ExpiredAtInUnixTimeSeconds { get; init; }
    
    public long CurrentTimeInUnixTimeSeconds { get; init; }
}

internal sealed class LocksRepository
{
    private readonly IAmazonDynamoDB _db;
    private readonly LockTableSettings _settings;

    public LocksRepository(IAmazonDynamoDB db, LockTableSettings settings)
    {
        _db = db;
        _settings = settings;
    }

    public async Task<bool> Create(CreateLockRecordRequest request)
    {
        var putRequest = new PutItemRequest
        {
            TableName = _settings.TableName,
            Item = new Dictionary<string, AttributeValue>
            {
                [_settings.PartitionKeyColumnName] = new(request.Key),
                [_settings.SortKeyColumnName] = new("locks"),
                ["Token"] = new(request.Token),
                ["ExpiredAt"] = new()
                {
                    N = Convert.ToString(request.ExpiredAtInUnixTimeSeconds)
                },
                [_settings.TimeToLiveColumnName] = new()
                {
                    N = Convert.ToString(request.ExpiredAtInUnixTimeSeconds + 60)
                }
            }, 
            ConditionExpression = $"attribute_not_exists(#{_settings.PartitionKeyColumnName}) OR (ExpiredAt < :expired)",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                [$"#{_settings.PartitionKeyColumnName}"] = _settings.PartitionKeyColumnName
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":expired"] = new()
                {
                    N = Convert.ToString(request.CurrentTimeInUnixTimeSeconds + 1)
                }
            }
        };

        var rsp = await _db.PutItemAsync(putRequest);

        return rsp.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task Delete(string key, string token)
    {
        var request = new DeleteItemRequest
        {
            TableName = _settings.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                [_settings.PartitionKeyColumnName] = new(key),
                [_settings.SortKeyColumnName] = new("locks")
            },
            ConditionExpression = "#token = :token",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                ["#token"] = "Token"
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":token"] = new(token)
            }
        };

        await _db.DeleteItemAsync(request);
    }
}