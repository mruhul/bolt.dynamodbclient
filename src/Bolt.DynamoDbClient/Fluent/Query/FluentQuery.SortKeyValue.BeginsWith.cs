namespace Bolt.DynamoDbClient.Fluent.Query;

internal sealed partial class FluentQuery : IHaveDynamoDbSortKeyValue
{
    public IHaveDynamoDbSortKeyValue BeginsWith(string value)
    {
        return this.ForStringSortKeyValue(QueryOperatorType.BeginsWith, value);
    }
}

public interface ICollectDynamoDbSortKeyBeginsWithValue
{
    IHaveDynamoDbSortKeyValue BeginsWith(string value);
}