using RPCRabbitMQ.Client.Services.Interfaces;
using RPCRabbitMQ.DataContracts.RPC;

namespace RPCRabbitMQ.Client.Services;

public class RemoteNode : IRemoteNode, IDisposable
{
    private readonly RpcClient _rpcClient = new();
    
    public Task<IRemoteMethodHandler> ExecuteAsync<T>(ReadOnlyMemory<byte> input, CancellationToken cancellationToken) where T : IRemoteMethod
    {
        return _rpcClient.CallAsync(input, cancellationToken);
    }

    public void Dispose()
    {
        _rpcClient.Dispose();
    }
}
