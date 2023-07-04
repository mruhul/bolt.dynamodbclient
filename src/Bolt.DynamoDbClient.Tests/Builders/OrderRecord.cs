using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bolt.DynamoDbClient.Tests.Builders;

[DynamoDbTable("online-orders")]
public record OrderRecord
{
    [DynamoDbPartitionKey]
    public Guid StockId { get; init; }
    [DynamoDbSortKey]
    public string OrderId { get; init; }
    public string Email { get; init; }
}

[DynamoDbTable("test-store")]
public record BookDbRecord
{
    public string PK { get; init; }
    public string SK { get; init; }
    public string Title { get; init; }
    public DateTime? PublishedAt { get; init; }
    public decimal Price { get; init; }
    public Guid? Identifier { get; init; }
}
