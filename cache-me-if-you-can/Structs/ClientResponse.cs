using System.Text.Unicode;

namespace cache_me_if_you_can.Structs;

/// <summary>
/// Provides standardized response messages for client communication in the cache protocol.
/// </summary>
public static class ClientResponse
{
    /// <summary>
    /// Successful operation response.
    /// </summary>
    /// <value>Returns "OK\r\n" as a UTF-8 encoded byte array.</value>
    public static readonly byte[] OkResponse = "OK\r\n"u8.ToArray();

    /// <summary>
    /// Response indicating that the requested key was not found or has no value.
    /// </summary>
    /// <value>Returns "(null)\r\n" as a UTF-8 encoded byte array.</value>
    public static readonly byte[] NullResponse = "(null)\r\n"u8.ToArray();

    /// <summary>
    /// Error response for unrecognized commands or commands with missing required parameters.
    /// </summary>
    /// <value>Returns "ERR Unknown command or not enough parameters\r\n" as a UTF-8 encoded byte array.</value>
    public static readonly byte[] ErrorUnknownCommandResponse =
        "ERR Unknown command or not enough parameters\r\n"u8.ToArray();

    /// <summary>
    /// Error response for operations that require a key but received an empty one.
    /// </summary>
    /// <value>Returns "ERR Key is Empty\r\n" as a UTF-8 encoded byte array.</value>
    public static readonly byte[] ErrorKeyEmptyResponse = "ERR Key is Empty\r\n"u8.ToArray();

    /// <summary>
    /// Error response for SET operations that require a value but received an empty one.
    /// </summary>
    /// <value>Returns "ERR Value is Empty\r\n" as a UTF-8 encoded byte array.</value>
    public static readonly byte[] ErrorValueEmptyResponse = "ERR Value is Empty\r\n"u8.ToArray();

    /// <summary>
    /// Generates a custom error response based on an exception's message
    /// </summary>
    /// <param name="ex">The exception whose message will be included in the response.</param>
    /// <returns>A UTF-8 encoded byte array containing "ERR {exception message}\r\n".</returns>
    public static byte[] ErrorCommonResponse(Exception ex)
    {
        const int maxBufferSize = 1024;
        var buffer = ArrayPool<byte>.Shared.Rent(maxBufferSize);

        try
        {
            if (!Utf8.TryWrite(buffer, $"ERR {ex.Message}\r\n", out var bytesWritten))
                return "ERR Internal server error\r\n"u8.ToArray();

            var result = new byte[bytesWritten];
            buffer.AsSpan(0, bytesWritten).CopyTo(result);
            return result;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, true);
        }
    }
}