namespace Bolt.DynamoDbClient.DistributedLock;

public interface IDistributedLock
{
    public Task<bool> Acquire(string key, string token, TimeSpan duration, CancellationToken ct = default);
    public Task<bool> Release(string key, string token, CancellationToken ct = default);
}