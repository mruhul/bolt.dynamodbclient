using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Bolt.DynamoDbClient.Tests.Builders;
using Bolt.DynamoDbClient.Tests.TestHelpers;

namespace Bolt.DynamoDbClient.Tests
{
    public class DynamoDbWrapper_Update_Should
    {
        private IDynamoDbWrapper sut;
        private IAmazonDynamoDB fake;

        public DynamoDbWrapper_Update_Should()
        {
            fake = Substitute.For<IAmazonDynamoDB>();
            sut = DynamoDbWrapperBuilder.Build(fake);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void call_dynamo_with_correct_request_when_doesnt_skip_null_value(TestData<ComplexRecord> input)
        {
            UpdateItemRequest gotRequest = null;

            fake.UpdateItemAsync(Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>())
                .Returns(new UpdateItemResponse())
                .AndDoes(c =>
                {
                    gotRequest = c.Arg<UpdateItemRequest>();
                });


            sut.Update(input.Value, false);

            fake.Received().UpdateItemAsync(Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>());

            new
            {
                Key = input.Key,
                Scenario = input.Scenario,
                GotRequest = gotRequest
            }.ShouldMatchApproved(input.Key, input.Scenario);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void call_dynamo_with_correct_request_when_skip_null_value(TestData<ComplexRecord> input)
        {
            UpdateItemRequest? gotRequest = null;

            fake.UpdateItemAsync(Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>())
                .Returns(new UpdateItemResponse())
                .AndDoes(c =>
                {
                    gotRequest = c.Arg<UpdateItemRequest>();
                });


            sut.Update(input.Value, true);

            fake.Received().UpdateItemAsync(Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>());

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
