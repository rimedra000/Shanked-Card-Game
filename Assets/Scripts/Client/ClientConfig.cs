using UnityEngine;
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
    public static string ConnectionUrl="ws://localhost/31070/";
    // public static string HostUrl="ws://0.0.0.0:31070";
    
    static ClientConfig()
    {
        if(PlayerPrefs.HasKey(usernameKey))_username=PlayerPrefs.GetString(usernameKey);
        string[] args = System.Environment.GetCommandLineArgs();
        
        // ifArg("hosturl", s =>
        // {
        //     HostUrl=s;
        //     // ConnectionUrl=HostUrl;
        // });

        ifArg("url", s=>
        {
            ConnectionUrl=s;
        });

        // for (int i = 1; i < args.Length; i++)
        // {
        //     string arg = args[i];
        //     if(arg.StartsWith("-"))arg = arg[1..];
        //     arg=arg.ToLower();
        //     if (arg.StartsWith("url"))
        //     {
        //         var sub = arg[3..];    
        //         ConnectionUrl=arg;
        //     }
            
        // }

        

        void ifArg(string cond, System.Action<string> action)
        {
            for (int argIndex = 1; argIndex < args.Length; argIndex++)
            {
                string arg=args[argIndex];
                if(arg.StartsWith("-"))arg = arg[1..];
                string argLower=arg.ToLower();
                if (argLower.StartsWith(cond))
                {
                    if (arg.Length>=cond.Length+2&&(arg[cond.Length] == ':' || arg[cond.Length] == '='))
                    {
                        action(arg[(cond.Length+1)..]);
                    }
                    else if (arg.Length == cond.Length)
                    {
                        argIndex++;
                        if(argIndex<args.Length)
                            action(args[argIndex]);
                    }
                    break;
                }    
            }   
        }

    }
}