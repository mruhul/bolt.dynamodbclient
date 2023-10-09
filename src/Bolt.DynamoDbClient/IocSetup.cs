using Bolt.DynamoDbClient.DistributedLock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bolt.DynamoDbClient;

public static class IocSetup
{
    public static IServiceCollection AddBoltDynamoDbClient(this IServiceCollection services, IocSetupOptions? options = null)
    {
        options ??= new IocSetupOptions();
        
        services.TryAddTransient<LocksRepository>();
        services.TryAddTransient<IDistributedLock,DistributedLockImpl>();
        services.TryAddTransient<LocksRepository>();
        services.TryAddSingleton(options.LockTableSettings);
        services.TryAddTransient<IDynamoDbWrapper, DynamoDbWrapper>();

        return services;
    }
}


public record IocSetupOptions
{
    public LockTableSettings LockTableSettings { get; init; } = new()
    {
        TableName = "table-distributed-locks"
    };
}