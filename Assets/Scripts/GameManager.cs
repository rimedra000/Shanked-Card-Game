using UnityEngine;

public static class GameManager
{
    #if CLIENT
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static ClientBehaviour clientBehaviour;
    // public static Func<CardStruct,bool> ValidCard;
    public static PlayPile playPile;
    public static string username="";
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
    public static long connectionID =0;

    public static Sprite getSpriteFromCardStruct(CardStruct cardStruct)
    {
        if(cardStruct.value==CardValue.Blank) return Resources.Load<Sprite>($"Card Art/Back/0");
        return Resources.Load<Sprite>($"Card Art/{cardStruct.value}/0");
    }
    #endif
    public static Unity.Networking.Transport.NetworkEndpoint networkEndpoint = Unity.Networking.Transport.NetworkEndpoint.LoopbackIpv4.WithPort(port);

    
    public static ushort port =31090;
    //high to low
    


}
