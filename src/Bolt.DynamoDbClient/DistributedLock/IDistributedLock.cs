namespace Bolt.DynamoDbClient.DistributedLock;

public interface IDistributedLock
{
    public Task<bool> Acquire(string key, string token, TimeSpan duration);
    public Task<bool> Release(string key, string token);
}