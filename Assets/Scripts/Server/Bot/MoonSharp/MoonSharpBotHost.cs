using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger;
using System;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Loaders;

public class MoonSharpBotHost : SimpleBot
{
    private Script script;
    private static MoonSharpVsCodeDebugServer server;
    private string username;

    private object PickFromFilteredMainHand_MoonSharpFunc;
    private object PickFromShownCards_MoonSharpFunc;
    private object SetupShownCards_MoonSharpFunc;
    static MoonSharpBotHost()
    {
        // server=new MoonSharpVsCodeDebugServer().Start();
        Script.GlobalOptions.RethrowExceptionNested=true;
        Script.DefaultOptions.ScriptLoader = new FileSystemScriptLoader();

        RegisterType<CardValue>();
        RegisterType<CardSuit>();
        RegisterType<CardDeck>();
        var cardStructDescriptor = RegisterType<CardStruct>(true);
        cardStructDescriptor.RemoveMember(nameof(CardStruct.ToByte));
        cardStructDescriptor.RemoveMember(nameof(CardStruct.GetHashCode));
        // cardStructDescriptor.AddMember("__new",new MethodMemberDescriptor(typeof(CardStruct).GetConstructor(new Type[]{typeof(CardValue),typeof(CardSuit),typeof(CardDeck)})));
        RegisterType<PlayerState>(true);
        RegisterType<ClientState>(true);

        DispatchingUserDataDescriptor RegisterType<T>(bool removeConstrutors = false)
        {
            IUserDataDescriptor descriptor = UserData.RegisterType<T>();
            var dispatchingDescriptor = (DispatchingUserDataDescriptor)descriptor;
            if (removeConstrutors && dispatchingDescriptor.HasMember("__new")) dispatchingDescriptor.RemoveMember("__new");
            // PrintMembersOfType<T>();
            // _registeredTypes.Add(typeof(T));
            // script.Globals[typeof(T).Name] = typeof(T);
            return dispatchingDescriptor;
        }

        // void PrintMembersOfType<T>()
        // {
        //     DispatchingUserDataDescriptor dataDescriptor = (DispatchingUserDataDescriptor)UserData.GetDescriptorForType<T>(false);
        //     var s = string.Join(" },{ ",dataDescriptor.MemberNames);
        //     // Debug.Log("[{ "+s+" }]");
        // }
    }



    public MoonSharpBotHost(string path)
    {
        script = new Script(CoreModules.Preset_HardSandbox);
        RegisterGlobals();
        script.Options.DebugPrint=i=>BotUtils.DebugLog(i); 

        server?.AttachToScript(script,path);
        script.DoFile(path,codeFriendlyName:path);
        
        username = script.Globals["username"].ToString();
        RegisterLuaFunctions();
        // while(server.GetAttachedDebuggersByIdAndName().Count()==0);
        void RegisterGlobals()
        {    
            script.Globals[nameof(CardValue)] = typeof(CardValue);
            script.Globals[nameof(CardSuit)] = typeof(CardSuit);
            script.Globals[nameof(CardDeck)] = typeof(CardDeck);
            script.Globals["testfunc"]=(Action<CardStruct>) ((_)=>{while (false);});
            script.Globals["sort_cards"]=(Func<CardStruct[],CardValue[],CardStruct[]>)CardExtensions.SortCards;
        }

        void RegisterLuaFunctions()
        {
            PickFromFilteredMainHand_MoonSharpFunc = script.Globals["pick_from_filtered_main_hand"];
            PickFromShownCards_MoonSharpFunc = script.Globals["pick_from_shown_cards"];
            SetupShownCards_MoonSharpFunc = script.Globals["setup_shown_cards"];
        }

    }

    public string GetUsername()
    {
        return username;
    }

    public CardStruct[] PickFromFilteredMainHand(CardStruct[] filteredCards, ClientState clientState)
    {
        return script.Call(PickFromFilteredMainHand_MoonSharpFunc,filteredCards,clientState).ToObject<CardStruct[]>();
    }

    public CardStruct PickFromShownCards(CardStruct[] cards, ClientState clientState)
    {
        return script.Call(PickFromShownCards_MoonSharpFunc, cards, clientState).ToObject<CardStruct>();
    }

    public (CardStruct, CardStruct) SetupShownCards(CardStruct[] mainHand, CardStruct[] shownCards)
    {
        var r = script.Call(SetupShownCards_MoonSharpFunc,mainHand,shownCards).Tuple;
        return (r[0].ToObject<CardStruct>(),r[1].ToObject<CardStruct>());
    }

    public void Dispose()
    {
        server?.Detach(script);
    }
}
