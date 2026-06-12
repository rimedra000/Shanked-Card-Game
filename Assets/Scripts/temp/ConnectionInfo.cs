using UnityEngine;


public struct DataHeader
{
    byte data;
    public DataHeader(byte player, GameEvent gameEvent)
    {
        player &= 0b_00000111;
        player <<= 4;
        var gameEventByte = (byte)gameEvent;
        gameEventByte &=0b_00001111;
        data = (byte)(gameEventByte | player);
    }
    
}


public enum GameEvent : byte
{

    Shanked = 0b_0000, // shanked (server)
    PlayCards = 0b_0001, // play cards from main hand (client)
    MoveShownCard = 0b_0010, // place shown card into main hand (client)
    MoveHiddenCard = 0b_0011, // place hidden card into main hand (client)
    SwapHand = 0b_0100, // swap hand (server)
    DrawCards = 0b_0101, // draw cards (server)
    StartTurn = 0b_0110, // turn (server)

    //0b_0111 

    DealHiddenCards = 0b_1000, // hidden cards (server)
    DealShownCards = 0b_1001, // shown cards (server)
    DealHandCards = 0b_1010, // hand cards (server)
    SwapCards = 0b_1011, // swap card (byte 2) with card (byte 3) (client)
    Ready = 0b_1100, // ready (client)

    //0b_1110
    //0b_1111
}


