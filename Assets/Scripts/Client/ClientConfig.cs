using UnityEngine;
using Unity.Networking.Transport;

public static class ClientConfig
{
    private static string _username = "";
    public static string username { get => _username; set{_username = value;PlayerPrefs.SetString(usernameKey,_username);} }
    private const string usernameKey="Username";
    public static CardValue[] sortOrder = new []{
        CardValue.Joker,
        CardValue.Ace,
        CardValue.King,
        CardValue.Queen,
        CardValue.Jack,
        CardValue.Ten,
        CardValue.Nine,
        CardValue.Eight,
        CardValue.Seven,
        CardValue.Six,
        CardValue.Five,
        CardValue.Four,
        CardValue.Three,
        CardValue.Two
    };
    // public static DrawPile drawPile;
    // public static DiscardPile discardPile;
    // public static long connectionID =0;

    public static Sprite getSpriteFromCardStruct(CardStruct cardStruct)
    {
        if(cardStruct.value==CardValue.Blank) return Resources.Load<Sprite>($"Card Art/Back/0");
        return Resources.Load<Sprite>($"Card Art/{cardStruct.value}/0");
    }

    // public static ushort port =31090;
    public static NetworkEndpoint networkEndpoint = NetworkEndpoint.LoopbackIpv4.WithPort(31090);

    

    static ClientConfig()
    {
        if(PlayerPrefs.HasKey(usernameKey))_username=PlayerPrefs.GetString(usernameKey);
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 1; i < args.Length; i++)
        {
            string arg = args[i];
            if(arg.StartsWith("-"))arg = arg[1..];
            arg=arg.ToLower();
            if (arg.StartsWith("ip"))
            {
                var sub = arg[3..];
                if (NetworkEndpoint.TryParse(sub,networkEndpoint.Port,out var networkEndpoint2))
                {
                    networkEndpoint=networkEndpoint2;
                }

            }
            if (arg.StartsWith("port"))
            {
                var sub = arg[5..];
                if(ushort.TryParse(sub,out var num))networkEndpoint=networkEndpoint.WithPort(num);
            }
        }

    }
}