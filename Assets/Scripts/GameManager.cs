using System;
using UnityEngine;

public static class GameManager
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static ClientBehaviour clientBehaviour;
    // public static Func<CardStruct,bool> ValidCard;
    public static PlayPile playPile;

    public static Unity.Networking.Transport.NetworkEndpoint networkEndpoint;
    // public static DrawPile drawPile;
    // public static DiscardPile discardPile;


    public static Sprite getSpriteFromCardStruct(CardStruct cardStruct)
    {
        if(cardStruct.value==CardValue.Blank) return Resources.Load<Sprite>($"Card Art/Back/0");
        return Resources.Load<Sprite>($"Card Art/{cardStruct.value}/0");
    }


}
