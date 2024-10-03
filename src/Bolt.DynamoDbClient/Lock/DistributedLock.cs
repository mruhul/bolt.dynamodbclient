using Amazon.DynamoDBv2.Model;

namespace Bolt.DynamoDbClient.Lock;

internal sealed class DistributedLock
{
    private readonly LocksRepository _repository;

    public DistributedLock(LocksRepository repository)
    {
        _repository = repository;
    }    
    
    public async Task<bool> Acquire(string key, string token, TimeSpan duration, CancellationToken ct)
    {
        try
        {
            return await _repository.Create(new()
            {
                Key = key,
                Token = token,
                CurrentTimeInUnixTimeSeconds = DateTime.UtcNow.ToUnixTimeSeconds(),
                ExpiredAtInUnixTimeSeconds = DateTime.UtcNow.Add(duration).ToUnixTimeSeconds()
            }, ct);
        }
        catch (ConditionalCheckFailedException)
        {
            return false;
        }
    }

    public async Task<bool> Release(string key, string token, CancellationToken ct)
    {
        try
        {
            await _repository.Delete(key, token, ct);
            return true;
        }
        catch (ConditionalCheckFailedException)
        {
            return false;
        }
    }
}