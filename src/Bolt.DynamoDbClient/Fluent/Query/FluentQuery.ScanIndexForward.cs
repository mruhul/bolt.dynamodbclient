namespace Bolt.DynamoDbClient.Fluent.Query;

internal sealed partial class FluentQuery : IHaveDynamoDbScanIndexForward
{
    private bool? _scanIndexForward;

    IHaveDynamoDbScanIndexForward ICollectDynamoDbScanIndexForward.ScanIndexForward(bool scanIndexForward)
    {
        _scanIndexForward = scanIndexForward;
        return this;
    }
}

public interface IHaveDynamoDbScanIndexForward : IDynamoDbFetchData { }
public interface ICollectDynamoDbScanIndexForward
{
    IHaveDynamoDbScanIndexForward ScanIndexForward(bool scanIndexForward);
}