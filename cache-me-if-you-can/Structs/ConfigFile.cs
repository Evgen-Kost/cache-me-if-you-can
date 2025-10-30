namespace cache_me_if_you_can.Structs;

public class ConfigFile
{
    public required ConfigTcpServer TcpServer { get; set; }
}

public class ConfigTcpServer
{
    public AddressFamily AddressFamily { get; set; } = AddressFamily.InterNetwork;
    public SocketType SocketType { get; set; } = SocketType.Stream;
    public ProtocolType ProtocolType { get; set; } = ProtocolType.Tcp;
    public string BindEndpoint { get; set; } = "127.0.0.1";
    public int BindPort { get; set; } = 8080;

    public int MessageBufferSize { get; set; } = 1024;
}