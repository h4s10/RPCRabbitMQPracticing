namespace RPCRabbitMQ.DataContracts.RPC;

public interface IRemoteMethod
{
    ValueTask<byte[]> ExecuteAsync(ReadOnlyMemory<byte> input, CancellationToken cancellationToken);
}