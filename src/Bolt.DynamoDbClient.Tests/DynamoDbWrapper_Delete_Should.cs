using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Bolt.DynamoDbClient.Tests.Builders;
using Bolt.DynamoDbClient.Tests.TestHelpers;

namespace Bolt.DynamoDbClient.Tests
{
    public class DynamoDbWrapper_Delete_Should
    {
        private IDynamoDbWrapper sut;
        private IAmazonDynamoDB fake;

        public DynamoDbWrapper_Delete_Should()
        {
            fake = Substitute.For<IAmazonDynamoDB>();
            sut = new DynamoDbWrapper(fake);
        }

        [Fact]
        public void call_dynamo_with_correct_request()
        {
            var givenRequest = new DeleteSingleItemRequest("pk1", "sk1");

            DeleteItemRequest? gotRequest = null;

            fake.DeleteItemAsync(Arg.Any<DeleteItemRequest>(), Arg.Any<CancellationToken>())
                .Returns(new DeleteItemResponse())
                .AndDoes(c =>
                {
                    gotRequest = c.Arg<DeleteItemRequest>();
                });


            sut.Delete<ComplexRecord>(givenRequest);

            fake.Received().DeleteItemAsync(Arg.Any<DeleteItemRequest>(), Arg.Any<CancellationToken>());

            gotRequest.ShouldMatchApproved();
        }

        public static IEnumerable<object[]> TestData = new TestData<DeleteSingleItemRequest>[]
        {
            new()
            {
                Key = "1",
                Scenario = "When all value provided",
                Value = new DeleteSingleItemRequest("pk-1","sk-1")
            }
        }.ToTestData();
    }
}
