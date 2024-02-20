namespace RPCRabbitMQ.DataContracts.RPC.Implementations;

public class RemoteMethod : IRemoteMethod
{
    public async ValueTask<byte[]> ExecuteAsync(ReadOnlyMemory<byte> input, CancellationToken cancellationToken)
    {
        await Task.Delay(5_000, cancellationToken);
        return input.ToArray();
    }
}