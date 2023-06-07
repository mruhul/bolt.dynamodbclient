using Amazon.DynamoDBv2.Model;

namespace Bolt.DynamoDbClient.Fluent.Query;

internal sealed partial class FluentQuery : IHaveDynamoDbSortKeyValue
{
    public IHaveDynamoDbSortKeyValue GreaterThan(string value)
    {
        return this.ForStringSortKeyValue(QueryOperatorType.GreaterThan, value);
    }

    public IHaveDynamoDbSortKeyValue GreaterThan(Guid value)
    {
        return this.ForStringSortKeyValue(QueryOperatorType.GreaterThan, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue GreaterThan(int value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.GreaterThan, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue GreaterThan(long value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.GreaterThan, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue GreaterThan(double value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.GreaterThan, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue GreaterThan(decimal value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.GreaterThan, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue GreaterThan(float value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.GreaterThan, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue GreaterThan(bool value)
    {
        return this.ForBoolSortKeyValue(QueryOperatorType.GreaterThan, value);
    }

    public IHaveDynamoDbSortKeyValue GreaterThanOrEqual(string value)
    {
        return this.ForStringSortKeyValue(QueryOperatorType.GreaterThanOrEquals, value);
    }

    public IHaveDynamoDbSortKeyValue GreaterThanOrEqual(Guid value)
    {
        return this.ForStringSortKeyValue(QueryOperatorType.GreaterThanOrEquals, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue GreaterThanOrEqual(int value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.GreaterThanOrEquals, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue GreaterThanOrEqual(long value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.GreaterThanOrEquals, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue GreaterThanOrEqual(double value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.GreaterThanOrEquals, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue GreaterThanOrEqual(decimal value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.GreaterThanOrEquals, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue GreaterThanOrEqual(float value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.GreaterThanOrEquals, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue GreaterThanOrEqual(bool value)
    {
        return this.ForBoolSortKeyValue(QueryOperatorType.GreaterThanOrEquals, value);
    }
}

public interface ICollectDynamoDbSortKeyGreaterThanValue
{
    IHaveDynamoDbSortKeyValue GreaterThan(string value);
    IHaveDynamoDbSortKeyValue GreaterThan(Guid value);
    IHaveDynamoDbSortKeyValue GreaterThan(int value);
    IHaveDynamoDbSortKeyValue GreaterThan(long value);
    IHaveDynamoDbSortKeyValue GreaterThan(double value);
    IHaveDynamoDbSortKeyValue GreaterThan(decimal value);
    IHaveDynamoDbSortKeyValue GreaterThan(float value);
    IHaveDynamoDbSortKeyValue GreaterThan(bool value);


    IHaveDynamoDbSortKeyValue GreaterThanOrEqual(string value);
    IHaveDynamoDbSortKeyValue GreaterThanOrEqual(Guid value);
    IHaveDynamoDbSortKeyValue GreaterThanOrEqual(int value);
    IHaveDynamoDbSortKeyValue GreaterThanOrEqual(long value);
    IHaveDynamoDbSortKeyValue GreaterThanOrEqual(double value);
    IHaveDynamoDbSortKeyValue GreaterThanOrEqual(decimal value);
    IHaveDynamoDbSortKeyValue GreaterThanOrEqual(float value);
    IHaveDynamoDbSortKeyValue GreaterThanOrEqual(bool value);
}