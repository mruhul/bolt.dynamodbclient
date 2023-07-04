namespace Bolt.DynamoDbClient.Tests.Builders;

public static class ComplexRecordBuilder
{
    public static ComplexRecord Build()
    {
        return new ComplexRecord
        {
            StringValue = "hello",
            NullableStringValue = "world!",
            ArrayValue = new string[] { "value1", "value2" },
            InCollectionValue = new List<int> { 1, 2, 3 },
            GuidCollectionValue = new Guid[] { new Guid("01626357-E80B-484B-A653-91F6D52589B8"), new Guid("051C1572-EB64-45B4-BB1E-B624B5B10136") },
            DoubleCollectionValue = new double[] { 1.2, 3.4 },
            DecimalNullableValue = 200m,
            DecimalValue = 300m,
            DoubleNullableValue = 200.0,
            DoubleValue = 300.0,
            FloatNullableValue = 2,
            FloatValue = 3,
            GuidValue = new Guid("01626357-E80B-484B-A653-91F6D52589B8"),
            GuildNullableValue = new Guid("051C1572-EB64-45B4-BB1E-B624B5B10136"),
            IntNullableValue = 2,
            IntValue = 3,
            LongNullableValue = 2000,
            LongValue = 3000,
            DateTimeValue = DateTime.Parse("2023-02-24T05:46:17.5148201Z"),
            NullableDateTimeValue = DateTime.Parse("2023-03-24T05:46:17.5148201Z"),
            PK = "complex-1",
            SK = "complex-sk-1",
            SubRecordValue = new SubRecord()
            {
                Age = 16,
                Title = "hello"
            },
            BoolNullableValue = true,
            BoolValue = true,
            SubRecords = new SubRecord[]
            {
                new SubRecord
                {
                    Age = 16,
                    Title = "hello"
                },
                new SubRecord
                {
                    Age = 18,
                    Title = "world"
                }
            },
            SubRecordsMap = new Dictionary<string, SubRecord>
            {
                ["hello"] = new SubRecord
                {
                    Age = 16,
                    Title = "hello"
                },
                ["world"] = new SubRecord
                {
                    Age = 18,
                    Title = "world"
                }
            }
        };
    }
}