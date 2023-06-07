namespace Bolt.DynamoDbClient.Tests.TestHelpers;

public class TestData<T>
{
    public string Key { get; init; }
    public string Scenario { get; init; }
    public T Value { get; init; }
}

public static class TestDataExtensions
{
    public static IEnumerable<object[]> ToTestData<T>(this IEnumerable<T> source)
    {
        var result = new List<object[]>();

        foreach(var item in source)
        {
            result.Add(new object[] { item });
        }

        return result;
    }
}