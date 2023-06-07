namespace Bolt.DynamoDbClient.Fluent.Query;

internal sealed partial class FluentQuery : IHaveDynamoDbSortKeyColumnName
{
    private string? _sortKeyColumnName;
    
    public IHaveDynamoDbSortKeyColumnName SortKey()
    {
        return this;
    }

    public IHaveDynamoDbSortKeyColumnName SortKey(string columnName)
    {
        _sortKeyColumnName = columnName;
        return this;
    }
}

public interface IHaveDynamoDbSortKeyColumnName : ICollectDynamoDbSortKeyValue, 
    ICollectDynamoDbSortKeyLessThanValue, 
    ICollectDynamoDbSortKeyGreaterThanValue,
    ICollectDynamoDbSortKeyBeginsWithValue,
    ICollectDynamoDbSortKeyBetweenValue
{ }
public interface ICollectDynamoDbSortKeyColumnName
{
    IHaveDynamoDbSortKeyColumnName SortKey();
    IHaveDynamoDbSortKeyColumnName SortKey(string columnName);
}