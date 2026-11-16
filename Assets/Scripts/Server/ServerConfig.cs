using System;
using System.Collections.Generic;
using UnityEngine;
public class ServerConfig
{
    // public static ushort port =31090;
    // public static NetworkEndpoint networkEndpoint = NetworkEndpoint.AnyIpv4.WithPort(31090);
    public static Uri HostUrl=new("ws://0.0.0.0:31070");

    public static List<string> botPaths = new();

    public static Action<string> DebugLog = Debug.Log;

    public static int? rngSeed;

    static ServerConfig()
    {
        string[] args = Environment.GetCommandLineArgs();
#if UNITY_EDITOR
        botPaths.Add($"{Application.dataPath}/Scripts/Server/Bot/MoonSharp/BotScripts/TestMoonSharpScript.lua");
#endif
        ifArg("hosturl", s =>
        {
            if (Uri.TryCreate(s, UriKind.Absolute, out Uri result) && (result.Scheme == "ws" || result.Scheme == "wss"))
            {
                HostUrl=result;    
            }
        });
        
        
        // ifArg("port", s =>
        // {
        //     if (ushort.TryParse(s, out var num))
        //     {
        //         networkEndpoint = networkEndpoint.WithPort(num);
        //     }
        // });

        ifArg("bot", s =>
        {
            if(System.IO.File.Exists(s))
                botPaths.Add(s);
        });


        ifArg("seed", s =>
        {
            if (int.TryParse(s, out var num))
            {
                rngSeed=num;
            }
        });

        void ifArg(string cond, Action<string> action)
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