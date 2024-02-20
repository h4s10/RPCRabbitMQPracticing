using RPCRabbitMQ.DataContracts.RPC;

namespace RPCRabbitMQ.Client.Services.Interfaces;

public interface IRemoteNode
{
    Task<IRemoteMethodHandler> ExecuteAsync<T>(ReadOnlyMemory<byte> input, CancellationToken cancellationToken) where T : IRemoteMethod;
}
