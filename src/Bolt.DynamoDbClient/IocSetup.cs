using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bolt.DynamoDbClient;

public static class IocSetup
{
    public static IServiceCollection AddBoltDynamoDbClient(this IServiceCollection services)
    {
        services.TryAddTransient<IDynamoDbWrapper, DynamoDbWrapper>();

        return services;
    }
}
