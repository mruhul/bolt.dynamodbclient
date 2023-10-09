using Amazon.DynamoDBv2;
using Bolt.DynamoDbClient.Lock;

namespace Bolt.DynamoDbClient.Tests.Builders;

public static class DynamoDbWrapperBuilder
{
    public static IDynamoDbWrapper Build(IAmazonDynamoDB db)
    {
        return new DynamoDbWrapper(db, new DistributedLock(new LocksRepository(db, new LockTableSettings
        {
            TableName = "table-locks"
        })));
    }
}