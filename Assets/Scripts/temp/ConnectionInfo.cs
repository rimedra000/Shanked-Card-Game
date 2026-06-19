using System;
using UnityEngine;

namespace NetworkData
{
    

public readonly struct DataHeader
{
    public readonly byte data;
    public DataHeader(byte player, GameEvent gameEvent)
    {
        player &= 0b_00000111;
        player <<= 4;
        byte gameEventByte = (byte)gameEvent;
        gameEventByte &=0b_00001111;
        data = (byte)(gameEventByte | player);
    }

    public DataHeader(byte data)
    {
        this.data = data;
    }

    public readonly GameEvent gameEvent => (GameEvent)(data & 0b_0000_1111);
    public readonly byte player => (byte)((data & 0b_0111_0000) >> 4);

    public override readonly string ToString()
    {
        return $"player:{player} Game Event:{gameEvent}";
    }


}


public enum GameEvent : byte
{
    //0b_0000 0
    
    PlayCards = 0b_0001, // play cards from main hand (client) 1
    MoveShownCard = 0b_0010, // place shown card into main hand (client) 2
    MoveHiddenCard = 0b_0011, // place hidden card into main hand (client) 3
    SwapHand = 0b_0100, // swap hand (server) 4
    DrawCards = 0b_0101, // draw cards (server) 5
    StartTurn = 0b_0110, // turn (server) 6

    ClearPile = 0b_0111,  // move play pile to discard pile (server) 7

    DealHiddenCards = 0b_1000, // hidden cards (server) 8
    DealShownCards = 0b_1001, // shown cards (server) 9 
    DealHandCards = 0b_1010, // hand cards (server) a
    SwapCards = 0b_1011, // swap card from shown hand (byte 2) with card from main hand (byte 3) (client) b
    Ready = 0b_1100, // ready (client) c

    //0b_1110 //? d
    
    Shanked = 0b_1111, // shanked (server) f
}

public struct Data
{
    public readonly DataHeader dataHeader;
    public readonly CardStruct[] cards;

    public byte[] ToBytes()
    {
        var result = new byte[cards.Length+1];
        result[0]=dataHeader.data;
        for(int i=0;i<cards.Length;i++)
        {
            result[i+1]=cards[i].ToByte();
        }
        return result;
    }

    public Data(DataHeader dataHeader, CardStruct[] cards)
    {
        this.dataHeader = dataHeader;
        this.cards = cards;
    }

    public Data(byte[] data)
    {
        dataHeader=new DataHeader(data[0]);
        cards = new CardStruct[data.Length-1];
        for(int i=1;i<data.Length;i++)
        {
            cards[i-1]=new CardStruct(data[i]);
        }
    }

    public override readonly string ToString()
    {
        return $"{dataHeader} cards:[{{{string.Join("} , {", cards)}}}]";        
    }
}

}
