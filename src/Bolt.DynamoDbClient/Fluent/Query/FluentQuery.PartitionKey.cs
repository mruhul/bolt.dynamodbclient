using Amazon.DynamoDBv2.Model;

namespace Bolt.DynamoDbClient.Fluent.Query;

internal sealed partial class FluentQuery : IHaveDynamoDbPartitionKeyColumnName, IHaveDynamoDbPartitionKeyValue
{
    private string? _partitionKeyColumnName;
    private AttributeValue? _partitionKeyAttributeValue;

    IHaveDynamoDbPartitionKeyValue ICollectDynamoDbPartitionKeyValue.Equals(string value)
    {
        _partitionKeyAttributeValue = new()
        {
            S = value
        };

        return this;
    }

    IHaveDynamoDbPartitionKeyValue ICollectDynamoDbPartitionKeyValue.Equals(Guid value)
    {
        _partitionKeyAttributeValue = new()
        {
            S = value.ToString()
        };

        return this;
    }

    IHaveDynamoDbPartitionKeyValue ICollectDynamoDbPartitionKeyValue.Equals(int value)
    {
        _partitionKeyAttributeValue = new()
        {
            N = value.ToString()
        };

        return this;
    }

    IHaveDynamoDbPartitionKeyColumnName ICollectDynamoDbPartitionKeyColumnName.PartitionKey()
    {
        return this;
    }

    IHaveDynamoDbPartitionKeyColumnName ICollectDynamoDbPartitionKeyColumnName.PartitionKey(string columnName)
    {
        _partitionKeyColumnName = columnName;
        return this;
    }
}

public interface IHaveDynamoDbPartitionKeyColumnName : ICollectDynamoDbPartitionKeyValue { }
public interface ICollectDynamoDbPartitionKeyColumnName
{
    IHaveDynamoDbPartitionKeyColumnName PartitionKey();
    IHaveDynamoDbPartitionKeyColumnName PartitionKey(string columnName);
}

public interface IHaveDynamoDbPartitionKeyValue : ICollectDynamoDbSortKeyColumnName, IDynamoDbFetchData { }
public interface ICollectDynamoDbPartitionKeyValue
{
    IHaveDynamoDbPartitionKeyValue Equals(string value);
    IHaveDynamoDbPartitionKeyValue Equals(Guid value);
    IHaveDynamoDbPartitionKeyValue Equals(int value);
}