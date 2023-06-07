using Amazon.DynamoDBv2.Model;

namespace Bolt.DynamoDbClient.Fluent.Query;

internal sealed partial class FluentQuery : IHaveDynamoDbSortKeyValue
{
    IHaveDynamoDbSortKeyValue ICollectDynamoDbSortKeyValue.Equals(string value)
    {
        return this.ForStringSortKeyValue(QueryOperatorType.Equals, value);
    }

    IHaveDynamoDbSortKeyValue ICollectDynamoDbSortKeyValue.Equals(Guid value)
    {
        return this.ForStringSortKeyValue(QueryOperatorType.Equals, value.ToString());
    }

    IHaveDynamoDbSortKeyValue ICollectDynamoDbSortKeyValue.Equals(DateTime value)
    {
        return this.ForStringSortKeyValue(QueryOperatorType.Equals, value.ToUtcString());
    }

    IHaveDynamoDbSortKeyValue ICollectDynamoDbSortKeyValue.Equals(int value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.Equals, value.ToString());
    }

    IHaveDynamoDbSortKeyValue ICollectDynamoDbSortKeyValue.Equals(long value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.Equals, value.ToString());
    }

    IHaveDynamoDbSortKeyValue ICollectDynamoDbSortKeyValue.Equals(double value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.Equals, value.ToString());
    }

    IHaveDynamoDbSortKeyValue ICollectDynamoDbSortKeyValue.Equals(decimal value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.Equals, value.ToString());
    }

    IHaveDynamoDbSortKeyValue ICollectDynamoDbSortKeyValue.Equals(float value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.Equals, value.ToString());
    }

    IHaveDynamoDbSortKeyValue ICollectDynamoDbSortKeyValue.Equals(bool value)
    {
        return this.ForBoolSortKeyValue(QueryOperatorType.Equals, value);
    }
}

public interface IHaveDynamoDbSortKeyValue : IDynamoDbFetchData, 
    ICollectDynamoDbLimit, 
    ICollectDynamoDbExclusiveStartKey, 
    ICollectDynamoDbConsistentRead,
    ICollectDynamoDbScanIndexForward { }
public interface ICollectDynamoDbSortKeyValue
{
    IHaveDynamoDbSortKeyValue Equals(string value);
    IHaveDynamoDbSortKeyValue Equals(Guid value);
    IHaveDynamoDbSortKeyValue Equals(DateTime value);
    IHaveDynamoDbSortKeyValue Equals(int value);
    IHaveDynamoDbSortKeyValue Equals(long value);
    IHaveDynamoDbSortKeyValue Equals(double value);
    IHaveDynamoDbSortKeyValue Equals(decimal value);
    IHaveDynamoDbSortKeyValue Equals(float value);
    IHaveDynamoDbSortKeyValue Equals(bool value);
}