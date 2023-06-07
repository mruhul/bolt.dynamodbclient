namespace Bolt.DynamoDbClient;

internal static class DateTimeExtensions
{
    public static string ToUtcString(this DateTime source)
        => source.Kind == DateTimeKind.Utc ? source.ToString("o") : source.ToUniversalTime().ToString("o");
}
