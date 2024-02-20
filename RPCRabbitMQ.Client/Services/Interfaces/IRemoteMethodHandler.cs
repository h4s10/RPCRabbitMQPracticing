namespace RPCRabbitMQ.Client.Services.Interfaces;

public interface IRemoteMethodHandler
{
    Task CancelAsync(CancellationToken cancellationToken);
    /// <summary>
    /// Get output
    /// </summary>
    /// <exception cref="InvalidOperationException">CancelAsync was invoked</exception>
    /// <returns>Message</returns>
    Task<byte[]> GetOutputAsync(CancellationToken cancellationToken);
    bool IsRunning { get; }
}