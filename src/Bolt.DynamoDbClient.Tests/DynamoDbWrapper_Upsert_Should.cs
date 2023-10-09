using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Bolt.DynamoDbClient.Tests.Builders;
using Bolt.DynamoDbClient.Tests.TestHelpers;

namespace Bolt.DynamoDbClient.Tests
{
    public class DynamoDbWrapper_Upsert_Should
    {
        private readonly IDynamoDbWrapper _sut;
        private readonly IAmazonDynamoDB _fake;

        public DynamoDbWrapper_Upsert_Should()
        {
            _fake = Substitute.For<IAmazonDynamoDB>();
            _sut = DynamoDbWrapperBuilder.Build(_fake);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void call_dynamo_with_correct_request(TestData<ComplexRecord> input)
        {
            PutItemRequest gotRequest = null;

            _fake.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>())
                .Returns(new PutItemResponse())
                .AndDoes(c =>
                {
                    gotRequest = c.Arg<PutItemRequest>();
                });


            _sut.Upsert(input.Value);

            _fake.Received().PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>());

            new
            {
                Key = input.Key,
                Scenario = input.Scenario,
                GotRequest = gotRequest
            }.ShouldMatchApproved(input.Key, input.Scenario);
        }

        public static IEnumerable<object[]> TestData = new TestData<ComplexRecord>[]
        {
            new()
            {
                Key = "1",
                Scenario = "When all value provided",
                Value = ComplexRecordBuilder.Build()
            },
            new()
            {
                Key = "2",
                Scenario = "When nullable are empty",
                Value = ComplexRecordBuilder.Build() with
                {
                    DecimalNullableValue = null,
                    FloatNullableValue = null,
                    GuildNullableValue = null,
                    IntNullableValue= null,
                    NullableStringValue = null,
                }
            },
        }.ToTestData();
    }
}
