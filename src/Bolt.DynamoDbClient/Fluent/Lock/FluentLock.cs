using Bolt.DynamoDbClient.DistributedLock;

namespace Bolt.DynamoDbClient.Fluent.Lock;

public class FluentLock : IHaveFluentLockKey, IHaveFluentLockTimeout, IHaveFluentLockRetry
{
    private readonly IDistributedLock _distributedLock;
    private readonly string _key;
    private readonly string? _token;
    private TimeSpan? _duration;
    private int? _retry;
    private int _delayInMs = 200;

    public FluentLock(IDistributedLock distributedLock, string key, string token)
    {
        _distributedLock = distributedLock;
        _key = key;
        _token = token;
    }


    public async Task<(bool, T?)> Execute<T>(Func<CancellationToken, Task<T>> func, CancellationToken ct)
    {
        var token = string.IsNullOrWhiteSpace(_token) ? Guid.NewGuid().ToString() : string.Empty;
        var duration = _duration ?? TimeSpan.FromMinutes(1);
        var retry = _retry ?? 0;
        
        var succeedToAcquireLock = false;
        
        try
        {
            for (int i = 0; i <= retry; i++)
            {
                if (i > 0 && _delayInMs > 0)
                    await Task.Delay(TimeSpan.FromMilliseconds(i * _delayInMs), ct);
                
                if (await _distributedLock.Acquire(_key, token, duration, ct))
                {
                    succeedToAcquireLock = true;

                    var result = await func.Invoke(ct);

                    return (true, result);
                }
            }
        }
        finally
        {
            if (succeedToAcquireLock)
                await _distributedLock.Release(_key, token, ct);
        }

        return (false, default);
    }

    public async Task<bool> Execute(Func<CancellationToken, Task> func, CancellationToken ct)
    {
        var token = string.IsNullOrWhiteSpace(_token) ? Guid.NewGuid().ToString() : string.Empty;
        var duration = _duration ?? TimeSpan.FromMinutes(1);
        var retry = _retry ?? 0;
        
        var succeedToAcquireLock = false;
        
        try
        {
            for (var i = 0; i <= retry; i++)
            {
                if (i > 0 && _delayInMs > 0)
                    await Task.Delay(TimeSpan.FromMilliseconds(i * _delayInMs), ct);
                
                if (await _distributedLock.Acquire(_key, token, duration, ct))
                {
                    succeedToAcquireLock = true;

                    await func.Invoke(ct);

                    return true;
                }
            }
        }
        finally
        {
            if (succeedToAcquireLock)
                await _distributedLock.Release(_key, token, ct);
        }

        return false;
    }


    public IHaveFluentLockTimeout Timeout(TimeSpan timeSpan)
    {
        _duration = timeSpan;
        return this;
    }

    public IHaveFluentLockTimeout Timeout(int timeoutInMs)
    {
        _duration = TimeSpan.FromMilliseconds(timeoutInMs);
        return this;
    }

    public IHaveFluentLockRetry Retry(int retryCount)
    {
        _retry = retryCount;
        return this;
    }

    public IHaveFluentLockRetry Retry(int retryCount, int delayInMs)
    {
        _retry = retryCount;
        _delayInMs = delayInMs;
        return this;
    }
}

public interface IExecuteInFluentLock
{
    /// <summary>
    /// Execute a func only if succeed to acquire lock and release the lock after task finished
    /// </summary>
    /// <param name="fetch"></param>
    /// <param name="ct"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<(bool,T?)> Execute<T>(Func<CancellationToken, Task<T>> fetch, CancellationToken ct);
    
    /// <summary>
    /// Execute a func only if succeed to acquire lock and release the lock after task finished
    /// </summary>
    /// <param name="fetch"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> Execute(Func<CancellationToken, Task> fetch, CancellationToken ct);
}

public interface IHaveFluentLockKey : IExecuteInFluentLock, 
    ICollectFluentLockTimeout, 
    ICollectFluentLockRetry
{
    
}


public interface ICollectFluentLockTimeout
{
    /// <summary>
    /// Expiry of Lock if not released. default is 1 minute
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    IHaveFluentLockTimeout Timeout(TimeSpan timeSpan);
    
    /// <summary>
    /// Expiry of Lock if not released. default is 1 minute
    /// </summary>
    /// <param name="timeoutInMs"></param>
    /// <returns></returns>
    IHaveFluentLockTimeout Timeout(int timeoutInMs);
}

public interface IHaveFluentLockTimeout : IExecuteInFluentLock, ICollectFluentLockRetry
{
    
}

public interface ICollectFluentLockRetry
{
    /// <summary>
    /// number of times retry when lock failed to acquire. Default delay in ms is 200ms for retry
    /// </summary>
    /// <param name="retryCount"></param>
    /// <returns></returns>
    IHaveFluentLockRetry Retry(int retryCount);
    
    /// <summary>
    /// number of times retry when lock failed to acquire.
    /// </summary>
    /// <param name="retryCount"></param>
    /// <param name="delayInMs"></param>
    /// <returns></returns>
    IHaveFluentLockRetry Retry(int retryCount, int delayInMs);
}

public interface IHaveFluentLockRetry : IExecuteInFluentLock
{
}