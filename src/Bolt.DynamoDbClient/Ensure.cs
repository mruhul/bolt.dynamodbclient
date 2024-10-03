namespace Bolt.DynamoDbClient;

internal static class Ensure
{
    public static void ThrowIfNull(object? arg, string argName)
    {
        if (arg == null) throw new ArgumentException(argName);
    }
}