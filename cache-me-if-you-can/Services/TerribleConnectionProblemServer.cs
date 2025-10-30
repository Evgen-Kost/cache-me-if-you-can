namespace cache_me_if_you_can.Services;

/// <summary>
/// Represents a TCP server that handles multiple client connections and processes cache store commands.
/// </summary>
public class TerribleConnectionProblemServer : IDisposable
{
    /// <summary>
    /// Logger instance for recording server activities and errors.
    /// </summary>
    private readonly ILogger<TerribleConnectionProblemServer> _logger;
    
    /// <summary>
    /// The underlying cache store that handles all data operations.
    /// </summary>
    private readonly SimpleDimpleStore _simpleStore;
    
    /// <summary>
    /// The main server socket that accepts incoming client connections.
    /// </summary>
    private readonly Socket _serverSocket;
    
    /// <summary>
    /// Maximum size in bytes for message buffers used during client communication.
    /// </summary>
    private readonly int _messageBufferSize;
    
    /// <summary>
    /// Flag indicating whether the server has been disposed.
    /// </summary>
    private bool _disposed;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TerribleConnectionProblemServer"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for recording server activities.</param>
    /// <param name="options">Configuration options for the TCP server.</param>
    /// <param name="simpleStore">The cache store instance for data operations.</param>
    public TerribleConnectionProblemServer(
        ILogger<TerribleConnectionProblemServer> logger,
        IOptionsMonitor<ConfigFile> options,
        SimpleDimpleStore simpleStore)
    {
        _logger = logger;
        _simpleStore = simpleStore;
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
    
    /// <summary>
    /// Starts the server and begins accepting client connections asynchronously.
    /// </summary>
    /// <param name="ct">A cancellation token that can be used to stop the server.</param>
    /// <returns>A task that represents the asynchronous operation and completes when the server stops.</returns>
    /// <exception cref="SocketException">Thrown when socket operations fail.</exception>
    public async Task StartAsync(CancellationToken ct = default)
    {
        _serverSocket.Listen(100);

        while (!ct.IsCancellationRequested)
        {
            var client = await _serverSocket.AcceptAsync(ct);
            
            _ = ProcessClientSafeAsync(client, ct);
        }
    }

    /// <summary>
    /// Safely processes a client connection with exception handling and resource cleanup.
    /// </summary>
    /// <param name="client">The client socket to process.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous client processing operation.</returns>
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
    
    /// <summary>
    /// Processes commands from a connected client in a loop until disconnection or cancellation.
    /// </summary>
    /// <param name="clientSocket">The socket representing the client connection.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous client processing operation.</returns>
    /// <exception cref="SocketException">Thrown when socket communication fails. This exception is propagated to the caller.</exception>
    private async Task ProcessClientAsync(
        Socket clientSocket,
        CancellationToken ct = default)
    {
        // Rent buffer for receiving raw bytes from the network
        var rentBuffer = ArrayPool<byte>.Shared.Rent(_messageBufferSize);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var byteReceived = await clientSocket.ReceiveAsync(
                    new Memory<byte>(rentBuffer, 0, rentBuffer.Length), 
                    SocketFlags.None, 
                    ct);
                
                // Zero bytes indicates client disconnection
                if (byteReceived == 0)
                {
                    _logger.LogInformation("Client disconnected");
                    break;
                }

                var charCount = Encoding.UTF8.GetCharCount(rentBuffer, 0, byteReceived);
                var rentedArray = ArrayPool<char>.Shared.Rent(charCount);
                try
                {
                    Encoding.UTF8.GetChars(rentBuffer, 0, byteReceived, rentedArray, 0);
                    ReadOnlyMemory<char> charMemory = rentedArray.AsMemory(0, charCount);
                    var multiSpan = ComandanteParser.Parse(charMemory);

                    var command = multiSpan.Command;
                    var key = multiSpan.Key.ToString();
                    var value = multiSpan.Value.ToByteArray();
                    
                    _logger.LogInformation($"{command} {key} {value}");

                    await (multiSpan.Command switch
                    {
                        StoreCommands.Get => HandleGetCommandAsync(clientSocket, key),
                        StoreCommands.Set => HandleSetCommandAsync(clientSocket, key, value),
                        StoreCommands.Del => HandleDelCommandAsync(clientSocket, key),
                        _ => clientSocket.SendAsync(ClientResponse.ErrorUnknownCommandResponse)
                    });
                }
                catch (SocketException)
                {
                    // Propagate socket exceptions to the caller
                    throw;
                }
                catch (Exception ex)
                {
                    // Send error response for all other exceptions
                    await clientSocket.SendAsync(ClientResponse.ErrorCommonResponse(ex));
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
    
    /// <summary>
    /// Handles the GET command request by retrieving a value from the store.
    /// </summary>
    /// <param name="clientSocket">The client socket for sending responses.</param>
    /// <param name="key">The key of the value to retrieve from the store.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task HandleGetCommandAsync(Socket clientSocket, string key)
    {
        if (await clientSocket.SendErrorToClientIfKeyIsEmptyAsync(key)) return;
        
        var getData = _simpleStore.Get(key);
        await clientSocket.SendAsync(getData ?? ClientResponse.NullResponse);
    }
    
    /// <summary>
    /// Handles the SET command request by storing a key-value pair in the store.
    /// </summary>
    /// <param name="clientSocket">The client socket for sending responses.</param>
    /// <param name="key">The key to store in the cache.</param>
    /// <param name="value">The byte array value to associate with the key.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task HandleSetCommandAsync(Socket clientSocket, string key, byte[] value)
    {
        if (await clientSocket.SendErrorToClientIfKeyIsEmptyAsync(key)) return;
        if (await clientSocket.SendErrorToClientIfValueIsEmptyAsync(value)) return;

        _simpleStore.Set(key, value);
        await clientSocket.SendAsync(ClientResponse.OkResponse);
    }
    
    /// <summary>
    /// Handles the DEL command request by removing a key from the store.
    /// </summary>
    /// <param name="clientSocket">The client socket for sending responses.</param>
    /// <param name="key">The key to delete from the cache.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task HandleDelCommandAsync(Socket clientSocket, string key)
    {
        if (await clientSocket.SendErrorToClientIfKeyIsEmptyAsync(key)) return;
                            
        _simpleStore.Delete(key);
        await clientSocket.SendAsync(ClientResponse.OkResponse);
    }
    
    /// <summary>
    /// Releases all resources used by the <see cref="TerribleConnectionProblemServer"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); 
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="TerribleConnectionProblemServer"/> 
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources; 
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    private void Dispose(bool disposing)
    {
        // Prevent multiple disposal
        if (_disposed) return;

        if (disposing)
        {
            // Dispose managed resources: shutdown and dispose server socket
            _serverSocket.TryShutDown();
        }
            
        _disposed = true;
    }
}