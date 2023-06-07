using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Bolt.DynamoDbClient.Tests.Builders;
using Bolt.DynamoDbClient.Tests.TestHelpers;

namespace Bolt.DynamoDbClient.Tests;

public class DynamoDbWrapper_GetSingle_Should
{
    private IDynamoDbWrapper sut;
    private IAmazonDynamoDB fake;

    public DynamoDbWrapper_GetSingle_Should()
    {
        fake = Substitute.For<IAmazonDynamoDB>();
        sut = new DynamoDbWrapper(fake);
    }

    [Fact]
    public async void call_dynamo_with_correct_request_and_response()
    {
        GetItemRequest? gotRequest = null;
        fake.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new GetItemResponse
            {
                Item = new Dictionary<string, AttributeValue>
                {
                    ["IntNullableValue"] = new () { N = "2" },
                    ["DoubleCollectionValue"] = new() { NS = new List<string>(){ "1.2","3.4" } },
                    ["IntValue"] = new() { N = "3" },
                    ["LongValue"] = new() { N = "3000" },
                    ["FloatValue"] = new() { N = "2" },
                    ["FloatNullableValue"] = new() { N = "3" },
                    ["LongNullableValue"] = new() { N = "2000" },
                    ["LongValue"] = new() { N = "3000" },
                    ["GuidValue"] = new() { S = "01626357-e80b-484b-a653-91f6d52589b8" },
                    ["DecimalNullableValue"] = new() { N = "200" },
                    ["SubRecordValue"] = new() { 
                        M = new Dictionary<string, AttributeValue>
                        {
                            ["Title"] = new() { S = "hello" },
                            ["Age"] = new() { N = "3" }
                        }
                    },
                    ["NullableDateTimeValue"] = new() { S = "2023-03-24T05:46:17.5148201Z" },
                    ["NullableStringValue"] = new() { S = "world!" },
                    ["StringValue"] = new() { S = "hello"},
                    ["ArrayValue"] = new() { SS = new () { "value1", "value2" } },
                    ["DecimalValue"] = new() { N = "300"},
                    ["DoubleNullableValue"] = new() { N = "200"},
                    ["DoubleValue"] = new() { N = "300"},
                    ["SK"] = new() { S = "sk1" },
                    ["PK"] = new() { S = "pk1" },
                    ["DateTimeValue"] = new() { S = "2023-02-24T05:46:17.5148201Z" },
                    ["GuildNullableValue"] = new() { S = "051c1572-eb64-45b4-bb1e-b624b5b10136" },
                    ["InCollectionValue"] = new() { NS = new() { "3","2","1"} },
                    ["GuidCollectionValue"] = new() { SS = new() { "01626357-e80b-484b-a653-91f6d52589b8", "051c1572-eb64-45b4-bb1e-b624b5b10136" } }
                }
            })
            .AndDoes(c =>
            {
                gotRequest = c.Arg<GetItemRequest>();
            });

        var gotResponse = await sut.GetSingleItem<ComplexRecord>(new GetSingleItemRequest("pk1","sk1"));

        await fake.Received().GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>());

        new
        {
            gotRequest,
            gotResponse
        }.ShouldMatchApproved();
    }
}
