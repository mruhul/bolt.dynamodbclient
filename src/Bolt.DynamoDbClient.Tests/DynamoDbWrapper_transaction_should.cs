using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Bolt.DynamoDbClient.Tests.Builders;
using Bolt.DynamoDbClient.Tests.TestHelpers;

namespace Bolt.DynamoDbClient.Tests;

public class DynamoDbWrapper_transaction_should
{
    private IDynamoDbWrapper sut;
    private IAmazonDynamoDB fake;

    public DynamoDbWrapper_transaction_should()
    {
        fake = Substitute.For<IAmazonDynamoDB>();
        sut = DynamoDbWrapperBuilder.Build(fake);
    }

    [Fact]
    public async Task call_dynamo_with_correct_request()
    {
        var transaction = sut.Transaction();

        transaction.Create(new SampleRecord
        {
            Pk = "sample-record#1",
            Sk = "sample-record#details",
            Name = "testing"
        });

        transaction.Upsert(new SampleRecord
        {
            Pk = "sample-record#2",
            Sk = "sample-record#details",
            Name = "testing 2"
        });

        transaction.Update(new SampleRecord
        {
            Pk = "sample-other-record#1",
            Sk = "sample-other-record#details",
            Name = "testing other record update 1"
        });

        transaction.Increment<SampleRecordIncrement>(
            "inc-pk-1",
            "inc-sk-1",
            [
                new("TotalCommits", 1),
                new("TotalLikes", -1),
            ]);

        TransactWriteItemsRequest? gotRequest = null;
        fake.TransactWriteItemsAsync(Arg.Any<TransactWriteItemsRequest>(), Arg.Any<CancellationToken>())
            .Returns(new TransactWriteItemsResponse())
            .AndDoes(c => { gotRequest = c.Arg<TransactWriteItemsRequest>(); });


        await transaction.Execute(CancellationToken.None);

        gotRequest.ShouldMatchApproved();
    }

    public record SampleRecord : SampleRecordBase
    {
        public string Name { get; set; }
    }

    public record SampleOtherRecord : SampleRecordBase
    {
        public string Age { get; set; }
        public string Token { get; set; }
    }

    public record SampleRecordIncrement : SampleRecordBase
    {
        public int TotalCommits { get; set; }
        public int TotalLikes { get; set; }
    }

    [DynamoDbTable("sample-store")]
    public record SampleRecordBase
    {
        [DynamoDbColumn("PK")] public string Pk { get; set; } = string.Empty;
        [DynamoDbColumn("SK")] public string Sk { get; set; } = string.Empty;
    }
}