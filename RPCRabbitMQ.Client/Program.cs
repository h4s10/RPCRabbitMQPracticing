using System.Text;
using System.Text.Json;
using RPCRabbitMQ.Client.Services;
using RPCRabbitMQ.Client.Services.Interfaces;
using RPCRabbitMQ.DataContracts.Messages;
using RPCRabbitMQ.DataContracts.RPC.Implementations;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    Console.WriteLine("Canceling...");
    cts.Cancel();
    e.Cancel = true;
};

var cancellationToken = cts.Token;

/*
 *** Не понятен смысл этого блока кода
var nodeId = ""; // get a target node ID
IRemoteNode node = await nodeProvider.GetNodeAsync(nodeId, cancellationToken);
*/

IRemoteNode node = new RemoteNode();
var input = JsonSerializer.SerializeToUtf8Bytes(new MyInput("hi there!"));
IRemoteMethodHandler remoteMethod = await node.ExecuteAsync<RemoteMethod>(input, cancellationToken);

await Task.Delay(1_000, cancellationToken);
if (!remoteMethod.IsRunning) 
{
    await remoteMethod.CancelAsync(cancellationToken);
}

var output = await remoteMethod.GetOutputAsync(cancellationToken);
Console.WriteLine(Encoding.UTF8.GetString(output));