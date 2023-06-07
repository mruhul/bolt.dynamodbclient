using Amazon.DynamoDBv2.Model;

namespace Bolt.DynamoDbClient.Fluent.Query;

internal sealed partial class FluentQuery : IHaveDynamoDbSortKeyValue
{
    public IHaveDynamoDbSortKeyValue LessThan(string value)
    {
        return this.ForStringSortKeyValue(QueryOperatorType.LessThan, value);
    }

    public IHaveDynamoDbSortKeyValue LessThan(Guid value)
    {
        return this.ForStringSortKeyValue(QueryOperatorType.LessThan, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue LessThan(int value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.LessThan, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue LessThan(long value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.LessThan, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue LessThan(double value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.LessThan, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue LessThan(decimal value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.LessThan, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue LessThan(float value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.LessThan, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue LessThan(bool value)
    {
        return this.ForBoolSortKeyValue(QueryOperatorType.LessThan, value);
    }

    public IHaveDynamoDbSortKeyValue LessThanOrEqual(string value)
    {
        return this.ForStringSortKeyValue(QueryOperatorType.LessThanOrEquals, value);
    }

    public IHaveDynamoDbSortKeyValue LessThanOrEqual(Guid value)
    {
        return this.ForStringSortKeyValue(QueryOperatorType.LessThanOrEquals, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue LessThanOrEqual(int value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.LessThanOrEquals, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue LessThanOrEqual(long value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.LessThanOrEquals, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue LessThanOrEqual(double value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.LessThanOrEquals, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue LessThanOrEqual(decimal value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.LessThanOrEquals, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue LessThanOrEqual(float value)
    {
        return this.ForNumberSortKeyValue(QueryOperatorType.LessThanOrEquals, value.ToString());
    }

    public IHaveDynamoDbSortKeyValue LessThanOrEqual(bool value)
    {
        return this.ForBoolSortKeyValue(QueryOperatorType.LessThanOrEquals, value);
    }
}

public interface ICollectDynamoDbSortKeyLessThanValue
{
    IHaveDynamoDbSortKeyValue LessThan(string value);
    IHaveDynamoDbSortKeyValue LessThan(Guid value);
    IHaveDynamoDbSortKeyValue LessThan(int value);
    IHaveDynamoDbSortKeyValue LessThan(long value);
    IHaveDynamoDbSortKeyValue LessThan(double value);
    IHaveDynamoDbSortKeyValue LessThan(decimal value);
    IHaveDynamoDbSortKeyValue LessThan(float value);
    IHaveDynamoDbSortKeyValue LessThan(bool value);


    IHaveDynamoDbSortKeyValue LessThanOrEqual(string value);
    IHaveDynamoDbSortKeyValue LessThanOrEqual(Guid value);
    IHaveDynamoDbSortKeyValue LessThanOrEqual(int value);
    IHaveDynamoDbSortKeyValue LessThanOrEqual(long value);
    IHaveDynamoDbSortKeyValue LessThanOrEqual(double value);
    IHaveDynamoDbSortKeyValue LessThanOrEqual(decimal value);
    IHaveDynamoDbSortKeyValue LessThanOrEqual(float value);
    IHaveDynamoDbSortKeyValue LessThanOrEqual(bool value);
}