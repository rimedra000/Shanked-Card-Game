public class ServerConfig
{
    public static ushort port =31090;
    public static Unity.Networking.Transport.NetworkEndpoint networkEndpoint = Unity.Networking.Transport.NetworkEndpoint.AnyIpv4.WithPort(port);
}