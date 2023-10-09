using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Bolt.DynamoDbClient.Tests.Builders;
using Bolt.DynamoDbClient.Tests.TestHelpers;

namespace Bolt.DynamoDbClient.Tests
{
    public class DynamoDbWrapper_Create_Should
    {
        private IDynamoDbWrapper sut;
        private IAmazonDynamoDB fake;

        public DynamoDbWrapper_Create_Should()
        {
            fake = Substitute.For<IAmazonDynamoDB>();
            sut = DynamoDbWrapperBuilder.Build(fake);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void call_dynamo_with_correct_request(TestData<ComplexRecord> input)
        {
            PutItemRequest gotRequest = null;

            fake.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>())
                .Returns(new PutItemResponse())
                .AndDoes(c =>
                {
                    gotRequest = c.Arg<PutItemRequest>();
                });


            sut.Create(input.Value);

            fake.Received().PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>());

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
