namespace Bolt.DynamoDbClient;

internal static class DateTimeExtensions
{
    public static string ToUtcString(this DateTime source)
        => source.Kind == DateTimeKind.Utc ? source.ToString("o") : source.ToUniversalTime().ToString("o");
    
    private static readonly DateTime UnixBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static long ToUnixTimeSeconds(this DateTime source)
    {
        if (source.Kind == DateTimeKind.Utc)
            return (Int64)source.Subtract(UnixBaseTime).TotalSeconds;

        return (Int64)source.ToUniversalTime().Subtract(UnixBaseTime).TotalSeconds;
    }
}
