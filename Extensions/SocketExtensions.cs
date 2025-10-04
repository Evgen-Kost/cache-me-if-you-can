namespace cache_me_if_you_can.Extensions;

public static class SocketExtensions
{
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
            // Сокет уже закрыт - игнорируем
        }
        finally
        {
            if (close)
            {
                socket.Close();
            }
        }
    }
}