using Bolt.DynamoDbClient;

namespace Bolt.DynamoDbClient.Tests.Builders;

[DynamoDbTable("test-store")]
public record ComplexRecord
{
    public string PK { get; init; }
    public string SK { get; init; }
    public string StringValue { get; init; }
    public string? NullableStringValue { get; init; }
    public int IntValue { get; init; }
    public int? IntNullableValue { get; init; }
    public long LongValue { get; init; }
    public long? LongNullableValue { get; init; }
    public decimal DecimalValue { get; init; }
    public decimal? DecimalNullableValue { get; init; }
    public double DoubleValue { get; init; }
    public double? DoubleNullableValue { get; init; }
    public float FloatValue { get; init; }
    public float? FloatNullableValue { get; init; }
    public Guid GuidValue { get; init; }
    public Guid? GuildNullableValue { get; init; }

    public DateTime? DateTimeValue { get; init; }
    public DateTime? NullableDateTimeValue { get; init; }

    public string[] ArrayValue { get; init; }
    public ICollection<int> InCollectionValue { get; init; }
    public ICollection<Guid> GuidCollectionValue { get; init; }
    public IEnumerable<double> DoubleCollectionValue { get; init; }
    public SubRecord SubRecordValue { get; init; }
    public bool BoolValue { get; init; }
    public bool? BoolNullableValue { get; init; }

    public SubRecord[] SubRecords { get; init; }
    public Dictionary<string, SubRecord> SubRecordsMap { get; init; }
}

public record SubRecord
{
    public string Title { get; init; }
    public int Age { get; init; }
}