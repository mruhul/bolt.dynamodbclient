using Bolt.DynamoDbClient.Lock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bolt.DynamoDbClient;

public static class IocSetup
{
    public static IServiceCollection AddBoltDynamoDbClient(this IServiceCollection services, IocSetupOptions? options = null)
    {
        options ??= new IocSetupOptions();
        
        services.TryAddSingleton<IDbRecordMapper,DbRecordMapper>();
        services.TryAddTransient<LocksRepository>();
        services.TryAddTransient<DistributedLock>();
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