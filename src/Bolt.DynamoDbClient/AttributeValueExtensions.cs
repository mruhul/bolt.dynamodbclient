using Amazon.DynamoDBv2.Model;

namespace Bolt.DynamoDbClient;

internal static class AttributeValueExtensions
{
    public static AttributeValue ToAttributeValue(this IDictionary<string, string> source)
    {
        var result = new Dictionary<string, AttributeValue>(source.Count);

        foreach (var key in source)
        {
            result[key.Key] = new AttributeValue { S = key.Value };
        }

        return new AttributeValue
        {
            M = result
        };
    }

    public static AttributeValue ToAttributeValue(this IDictionary<string, Guid> source)
    {
        var result = new Dictionary<string, AttributeValue>(source.Count);

        foreach (var key in source)
        {
            result[key.Key] = new AttributeValue { S = key.Value.ToString() };
        }

        return new AttributeValue
        {
            M = result
        };
    }

    public static AttributeValue ToAttributeValue(this IDictionary<string, int> source)
    {
        var result = new Dictionary<string, AttributeValue>(source.Count);

        foreach (var key in source)
        {
            result[key.Key] = new AttributeValue { N = key.Value.ToString() };
        }

        return new AttributeValue
        {
            M = result
        };
    }

    public static AttributeValue ToAttributeValue(this IDictionary<string, long> source)
    {
        var result = new Dictionary<string, AttributeValue>(source.Count);

        foreach (var key in source)
        {
            result[key.Key] = new AttributeValue { N = key.Value.ToString() };
        }

        return new AttributeValue
        {
            M = result
        };
    }

    public static AttributeValue ToAttributeValue(this IDictionary<string, double> source)
    {
        var result = new Dictionary<string, AttributeValue>(source.Count);

        foreach (var key in source)
        {
            result[key.Key] = new AttributeValue { N = key.Value.ToString() };
        }

        return new AttributeValue
        {
            M = result
        };
    }

    public static AttributeValue ToAttributeValue(this IDictionary<string, decimal> source)
    {
        var result = new Dictionary<string, AttributeValue>(source.Count);

        foreach (var key in source)
        {
            result[key.Key] = new AttributeValue { N = key.Value.ToString() };
        }

        return new AttributeValue
        {
            M = result
        };
    }
}
