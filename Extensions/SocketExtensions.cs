namespace cache_me_if_you_can.Extensions;

/// <summary>
/// Provides extension methods for <see cref="Socket"/> operations.
/// </summary>
public static class SocketExtensions
{
    /// <summary>
    /// Attempts to gracefully shutdown a socket and optionally closes it, ignoring errors if the socket is already closed.
    /// </summary>
    /// <param name="socket">The socket to shutdown and close.</param>
    /// <param name="style">
    /// Specifies the type of shutdown operation to perform. 
    /// Default is <see cref="SocketShutdown.Both"/> which disables both sending and receiving.
    /// </param>
    /// <param name="close">
    /// Indicates whether to close the socket after shutdown. 
    /// Default is <see langword="true"/>.
    /// </param>
    public static void TryShutDown(
        this Socket socket, 
        SocketShutdown style = SocketShutdown.Both, 
        bool close = true)
    {
        try
        {
            socket.Shutdown(style);
        }
        catch (SocketException)
        {
            // Socket is already closed - ignore the exception
        }
        finally
        {
            if (close)
            {
                socket.Close();
            }
        }
    }

    /// <summary>
    /// Validates that the key is not null or empty, and sends an error response to the client if validation fails.
    /// </summary>
    /// <param name="clientSocket">The client socket to send the error response to.</param>
    /// <param name="key">The key string to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the key is null or empty and an error response was sent; 
    /// <see langword="false"/> if the key is valid.
    /// </returns>
    public static async Task<bool> SendErrorToClientIfKeyIsEmptyAsync(this Socket clientSocket, string key)
    {
        if (!string.IsNullOrEmpty(key)) return false;
        
        await clientSocket.SendAsync(ClientResponse.ErrorKeyEmptyResponse);
        return true;
    }
    
    /// <summary>
    /// Validates that the value byte array is not empty, and sends an error response to the client if validation fails.
    /// </summary>
    /// <param name="clientSocket">The client socket to send the error response to.</param>
    /// <param name="value">The byte array value to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the value is empty and an error response was sent; 
    /// <see langword="false"/> if the value is valid (contains at least one byte).
    /// </returns>
    public static async Task<bool> SendErrorToClientIfValueIsEmptyAsync(this Socket clientSocket, byte[]? value)
    {
        if (value is not null && value.Length != 0) return false;
        
        await clientSocket.SendAsync(ClientResponse.ErrorValueEmptyResponse);
        return true;
    }
}