using Unity.Networking.Transport;
public class ServerConfig
{
    // public static ushort port =31090;
    public static NetworkEndpoint networkEndpoint = NetworkEndpoint.AnyIpv4.WithPort(31090);

    static ServerConfig()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 1; i < args.Length; i++)
        {
            string arg = args[i];
            if(arg.StartsWith("-"))arg = arg[1..];
            arg=arg.ToLower();
            // if (arg.StartsWith("ip"))
            // {
            //     var sub = arg[3..];
            //     if (NetworkEndpoint.TryParse(sub,networkEndpoint.Port,out var networkEndpoint2))
            //     {
            //         networkEndpoint=networkEndpoint2;
            //     }
            //
            // }
            if (arg.StartsWith("port"))
            {
                var sub = arg[5..];
                if(ushort.TryParse(sub,out var num))networkEndpoint=networkEndpoint.WithPort(num);
            }
        }

    }
}