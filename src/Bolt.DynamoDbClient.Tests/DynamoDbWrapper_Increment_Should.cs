using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Bolt.DynamoDbClient.Tests.Builders;
using Bolt.DynamoDbClient.Tests.TestHelpers;

namespace Bolt.DynamoDbClient.Tests
{
    public class DynamoDbWrapper_Increment_Should
    {
        private IDynamoDbWrapper sut;
        private IAmazonDynamoDB fake;

        public DynamoDbWrapper_Increment_Should()
        {
            fake = Substitute.For<IAmazonDynamoDB>();
            sut = DynamoDbWrapperBuilder.Build(fake);
        }

        [Fact]
        public async void call_dynamo_with_correct_request()
        {
            UpdateItemRequest gotRequest = null;

            fake.UpdateItemAsync(Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>())
                .Returns(new UpdateItemResponse())
                .AndDoes(c =>
                {
                    gotRequest = c.Arg<UpdateItemRequest>();
                });


            await sut.Increment<ComplexRecord>(new IncrementRequest
            { 
                PartitionKey = "pk", 
                SortKey = "sk", 
                PropertyValues = [
                    new IncrementPropertyValue
                    {
                        PropertyName = "IntValue",
                        IncrementBy = 2
                    },
                    new IncrementPropertyValue
                    {
                        PropertyName = "Int2Value",
                        IncrementBy = -1
                    },    
                ]
            }, CancellationToken.None);

            await fake.Received().UpdateItemAsync(Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>());

            new
            {
                GotRequest = gotRequest
            }.ShouldMatchApproved();
        }
    }
}
