namespace Bolt.DynamoDbClient.Fluent.Query;

internal sealed partial class FluentQuery : IHaveDynamoDbSortKeyValue
{
    public IHaveDynamoDbSortKeyValue Between(string from, string to)
    {
        return this.ForStringBetweenSortKeyValue(from, to);
    }
    public IHaveDynamoDbSortKeyValue Between(DateTime from, DateTime to)
    {
        return this.ForStringBetweenSortKeyValue(from.ToUtcString(), to.ToUtcString());
    }
    public IHaveDynamoDbSortKeyValue Between(int from, int to)
    {
        return this.ForNumberBetweenSortKeyValue(from.ToString(), to.ToString());
    }
    public IHaveDynamoDbSortKeyValue Between(double from, double to)
    {
        return this.ForNumberBetweenSortKeyValue(from.ToString(), to.ToString());
    }
    public IHaveDynamoDbSortKeyValue Between(decimal from, decimal to)
    {
        return this.ForNumberBetweenSortKeyValue(from.ToString(), to.ToString());
    }
    public IHaveDynamoDbSortKeyValue Between(long from, long to)
    {
        return this.ForNumberBetweenSortKeyValue(from.ToString(), to.ToString());
    }
    public IHaveDynamoDbSortKeyValue Between(float from, float to)
    {
        return this.ForNumberBetweenSortKeyValue(from.ToString(), to.ToString());
    }
}

public interface ICollectDynamoDbSortKeyBetweenValue
{
    IHaveDynamoDbSortKeyValue Between(string from, string to);
    IHaveDynamoDbSortKeyValue Between(DateTime from, DateTime to);
    IHaveDynamoDbSortKeyValue Between(int from, int to);
    IHaveDynamoDbSortKeyValue Between(double from, double to);
    IHaveDynamoDbSortKeyValue Between(decimal from, decimal to);
    IHaveDynamoDbSortKeyValue Between(long from, long to);
    IHaveDynamoDbSortKeyValue Between(float from, float to);
}