using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Bolt.DynamoDbClient.Fluent.Query;

internal sealed partial class FluentQuery
{
    private AttributeValue? _sortKeyAttributeValue;
    private AttributeValue? _sortKeyEndRangeAttributeValue;
    private QueryOperatorType _operatorType;

    private FluentQuery ForStringSortKeyValue(QueryOperatorType operatorType, string value) 
    {
        _sortKeyAttributeValue = new() { S = value };

        _operatorType = operatorType;

        return this;
    }

    private FluentQuery ForNumberSortKeyValue(QueryOperatorType operatorType, string value)
    {
        _sortKeyAttributeValue = new() { N = value };

        _operatorType = operatorType;

        return this;
    }

    private FluentQuery ForBoolSortKeyValue(QueryOperatorType operatorType, bool value)
    {
        _sortKeyAttributeValue = new() { BOOL = value };

        _operatorType = operatorType;

        return this;
    }

    private FluentQuery ForStringBetweenSortKeyValue(string valueFrom, string valueTo)
    {
        _sortKeyAttributeValue = new() { S = valueFrom };
        _sortKeyEndRangeAttributeValue = new() { S = valueTo };

        _operatorType = QueryOperatorType.Between;

        return this;
    }

    private FluentQuery ForNumberBetweenSortKeyValue(string valueFrom, string valueTo)
    {
        _sortKeyAttributeValue = new() { N = valueFrom };
        _sortKeyEndRangeAttributeValue = new() { N = valueTo };

        _operatorType = QueryOperatorType.Between;

        return this;
    }
}

internal enum QueryOperatorType
{
    Equals,
    LessThan,
    LessThanOrEquals,
    GreaterThan,
    GreaterThanOrEquals,
    BeginsWith,
    Between
}