using Bolt.Common.Extensions;
using Shouldly;

namespace Bolt.DynamoDbClient.Tests.TestHelpers
{
    public static class ShouldlyExtensions
    {
        public static void ShouldMatchApproved<T>(this T source, string? discriminator = null, string? scenario = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            source.SerializeToPrettyJson().ShouldMatchApproved((builder) =>
            {
                builder.SubFolder("approved");
                builder.UseCallerLocation();
                builder.WithStringCompareOptions(StringCompareShould.IgnoreLineEndings);
                if (!string.IsNullOrWhiteSpace(discriminator))
                {
                    builder.WithDiscriminator(discriminator);
                }
            }, scenario);
        }
    }
}