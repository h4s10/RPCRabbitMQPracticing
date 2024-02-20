using System.Collections.Concurrent;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RPCRabbitMQ.Client.Services;
using RPCRabbitMQ.Client.Services.Interfaces;
using RPCRabbitMQ.DataContracts;

namespace RPCRabbitMQ.Client;

public class RpcClient : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _replyQueueName;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> _callbackMapper = new();

    public RpcClient()
    {
        var factory = new ConnectionFactory { HostName = Settings.Host, UserName = Settings.Login, Password = Settings.Password };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _replyQueueName = _channel.QueueDeclare().QueueName;
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (_, ea) =>
        {
            if (!_callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
                return;
            
            var body = ea.Body.ToArray();
            tcs.TrySetResult(body);
        };

        _channel.BasicConsume(consumer: consumer,
                             queue: _replyQueueName,
                             autoAck: true);
    }

    public Task<IRemoteMethodHandler> CallAsync(ReadOnlyMemory<byte> message, CancellationToken cancellationToken = default)
    {
        var props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var tcs = new TaskCompletionSource<byte[]>();
        IRemoteMethodHandler handler = new RemoteMethodHandler(tcs, correlationId, _channel);
        _callbackMapper.TryAdd(correlationId, tcs);

        _channel.BasicPublish(exchange: string.Empty,
                             routingKey: Settings.QueueName,
                             basicProperties: props,
                             body: message);

        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out _));
        return Task.FromResult(handler);
    }

    public void Dispose()
    {
        _connection.Close();
    }
}
