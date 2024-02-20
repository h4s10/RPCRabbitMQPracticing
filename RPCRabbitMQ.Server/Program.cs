using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RPCRabbitMQ.DataContracts;
using RPCRabbitMQ.DataContracts.Messages;
using RPCRabbitMQ.DataContracts.RPC.Implementations;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    Console.WriteLine("Canceling...");
    cts.Cancel();
    e.Cancel = true;
};

var factory = new ConnectionFactory { HostName = Settings.Host, UserName = Settings.Login, Password = Settings.Password };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
var launchedTasks = new ConcurrentDictionary<string, CancellationTokenSource>();

channel.QueueDeclare(queue: Settings.QueueName,
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null);
var consumer = new EventingBasicConsumer(channel);
channel.BasicConsume(queue: Settings.QueueName,
    autoAck: false,
    consumer: consumer);
channel.QueueDeclare(queue: Settings.CancelQueueName,
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null);
var consumerCancel = new EventingBasicConsumer(channel);
channel.BasicConsume(queue: Settings.CancelQueueName,
    autoAck: false,
    consumer: consumerCancel);
Console.WriteLine(" [x] Awaiting RPC requests");

consumerCancel.Received += (model, ea) =>
{
    var id = Encoding.UTF8.GetString(ea.Body.ToArray());
    Console.WriteLine($"Cancel task received with id {id}");
    if (launchedTasks.TryGetValue(id, out var tokenSource))
        tokenSource.Cancel();
};
consumer.Received += async (model, ea) =>
{
    var response = string.Empty;
    var body = ea.Body.ToArray();
    var props = ea.BasicProperties;
    var replyProps = channel.CreateBasicProperties();
    replyProps.CorrelationId = props.CorrelationId;
    var cancellationTokenSource = new CancellationTokenSource();
    launchedTasks.TryAdd(props.CorrelationId, cancellationTokenSource);
    
    try
    {
        Console.WriteLine($"Task received with id {props.CorrelationId}");
        var methodImplementation = new RemoteMethod();
        var output = await methodImplementation.ExecuteAsync(body, cancellationTokenSource.Token);
        response = JsonSerializer.Deserialize<MyInput>(output)?.Message ?? string.Empty;
    }
    catch (Exception e)
    {
        Console.WriteLine($" [.] {e.Message}");
        response = string.Empty;
    }
    finally
    {
        var responseBytes = Encoding.UTF8.GetBytes(response);
        launchedTasks.TryRemove(props.CorrelationId, out _);
        channel.BasicPublish(exchange: string.Empty,
            routingKey: props.ReplyTo,
            basicProperties: replyProps,
            body: responseBytes);
        Console.WriteLine($"Callback has sent {props.CorrelationId}");
        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
    }
};

while (!cts.IsCancellationRequested)
{
    // ignore for docker
}