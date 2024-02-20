using System.Text;
using RabbitMQ.Client;
using RPCRabbitMQ.Client.Services.Interfaces;
using RPCRabbitMQ.DataContracts;

namespace RPCRabbitMQ.Client.Services;

public class RemoteMethodHandler : IRemoteMethodHandler
{
    private readonly TaskCompletionSource<byte[]> _taskCompletionSource;
    private readonly string _correlationId;
    private readonly IModel _channel;

    public RemoteMethodHandler(TaskCompletionSource<byte[]> taskCompletionSource, string correlationId, IModel channel)
    {
        _taskCompletionSource = taskCompletionSource;
        _correlationId = correlationId;
        _channel = channel;
    }

    public bool IsRunning => !_taskCompletionSource.Task.IsCompleted;

    public Task CancelAsync(CancellationToken cancellationToken)
    {
        if (IsRunning)
            _taskCompletionSource.SetCanceled(cancellationToken);
        
        var props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;

        _channel.BasicPublish(exchange: string.Empty,
            routingKey: Settings.CancelQueueName,
            basicProperties: props,
            body: Encoding.UTF8.GetBytes(_correlationId));

        return Task.CompletedTask;
    }

    public async Task<byte[]> GetOutputAsync(CancellationToken cancellationToken)
    {
        return await _taskCompletionSource.Task.WaitAsync(cancellationToken);
    }
}
