namespace cache_me_if_you_can.Services;

/// <summary>
/// TCP Server
/// </summary>
public class TerribleConnectionProblemServer : IDisposable
{
    private readonly ILogger<TerribleConnectionProblemServer> _logger;
    private readonly Socket _serverSocket;
    private readonly int _messageBufferSize;
    private bool _disposed;
    
    public TerribleConnectionProblemServer(
        ILogger<TerribleConnectionProblemServer> logger,
        IOptionsMonitor<ConfigFile> options)
    {
        _logger = logger;
        var configFile = options.CurrentValue;

        _messageBufferSize = configFile.TcpServer.MessageBufferSize;
        
        _serverSocket = new Socket(
            configFile.TcpServer.AddressFamily,
            configFile.TcpServer.SocketType,
            configFile.TcpServer.ProtocolType);
        
        var bindIpAddress = IPAddress.Parse(configFile.TcpServer.BindEndpoint);
        var bindPort = configFile.TcpServer.BindPort;

        var localEndPoint = new IPEndPoint(
            bindIpAddress,
            bindPort);
        
        _serverSocket.Bind(localEndPoint);
    }
    
    public async Task StartAsync(CancellationToken ct = default)
    {
        _serverSocket.Listen(100);

        while (!ct.IsCancellationRequested)
        {
            while (true)
            {
                var client = await _serverSocket.AcceptAsync(ct);
                _ = ProcessClientSafeAsync(client, ct);
            }
        }
    }

    private async Task ProcessClientSafeAsync(
        Socket client,
        CancellationToken ct = default)
    {
        try
        {
            await ProcessClientAsync(client, ct);
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Ошибка клиента: {ExMessage}", ex.Message);
        }
        finally
        {
            
            client.TryShutDown();
        }
    }
    
    private async Task ProcessClientAsync(
        Socket clientSocket,
        CancellationToken ct = default)
    {
        var rentBuffer = ArrayPool<byte>.Shared.Rent(_messageBufferSize);

        try
        {
            while (true)
            {
                var byteReceived = await clientSocket.ReceiveAsync(
                    new Memory<byte>(rentBuffer, 0, rentBuffer.Length), 
                    SocketFlags.None, 
                    ct);
                
                if (byteReceived == 0)
                {
                    _logger.LogInformation("Client disconnected");
                    break;
                }
                
                var charCount = Encoding.UTF8.GetCharCount(rentBuffer);
                var rentedArray = ArrayPool<char>.Shared.Rent(charCount);
                try
                {
                    Encoding.UTF8.GetChars(rentBuffer, rentedArray);
                    ReadOnlyMemory<char> charMemory = rentedArray.AsMemory(0, charCount);
                    var multiSpan = ComandanteParser.Parse(charMemory);

                    _logger.LogInformation($"{multiSpan.Command} {multiSpan.Key} {multiSpan.Value}");
                }
                finally
                {
                    ArrayPool<char>.Shared.Return(rentedArray, true);
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentBuffer, true);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); 
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _serverSocket.TryShutDown();
        }
            
        _disposed = true;
    }
}