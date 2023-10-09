using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Bolt.DynamoDbClient.DistributedLock;

internal sealed class DistributedLockImpl : IDistributedLock
{
    private readonly IAmazonDynamoDB _db;
    private readonly LocksRepository _repository;

    public DistributedLockImpl(IAmazonDynamoDB db, LocksRepository repository)
    {
        _db = db;
        _repository = repository;
    }    
    
    public async Task<bool> Acquire(string key, string token, TimeSpan duration)
    {
        try
        {
            return await _repository.Create(new()
            {
                Key = key,
                Token = token,
                CurrentTimeInUnixTimeSeconds = DateTime.UtcNow.ToUnixTimeSeconds(),
                ExpiredAtInUnixTimeSeconds = DateTime.UtcNow.Add(duration).ToUnixTimeSeconds()
            });
        }
        catch (ConditionalCheckFailedException e)
        {
            return false;
        }
    }

    public async Task<bool> Release(string key, string token)
    {
        try
        {
            await _repository.Delete(key, token);
            return true;
        }
        catch (ConditionalCheckFailedException)
        {
            return false;
        }
    }
}