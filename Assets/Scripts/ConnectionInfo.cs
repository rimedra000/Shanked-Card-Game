using System;

public readonly struct DataHeader
{
    public readonly byte data;
    public DataHeader(byte player, GameEvent gameEvent)
    {
        player &= 0b_00000111;
        player <<= 4;
        byte gameEventByte = (byte)gameEvent;
        gameEventByte &=0b_10001111;
        data = (byte)(gameEventByte | player);
    }

    public DataHeader(byte data)
    {
        this.data = data;
    }

    public readonly GameEvent gameEvent => (GameEvent)(data & 0b_1000_1111);
    public readonly byte player => (byte)((data & 0b_0111_0000) >> 4);

    public override readonly string ToString()
    {
        return $"player:{player} Game Event:{gameEvent}";
    }


}


public enum GameEvent : byte
{
    //0x00 0
    
    PlayCards = 0x01, // play cards from main hand (client) 1 public
    MoveShownCard = 0x02, // place shown card into main hand (client) 2 public
    MoveHiddenCard = 0x03, // place hidden card into main hand (client) 3 public
    SwapHand = 0x04, // swap hand (server) 4 private
    DrawCards = 0x05, // draw cards (server) 5 private
    StartTurn = 0x06, // turn (server) 6 public

    ClearPile = 0x07,  // move play pile to discard pile (server) 7 public

    DealHiddenCards = 0x08, // hidden cards (server) 8 private
    DealShownCards = 0x09, // shown cards (server) 9 public
    DealHandCards = 0x0A, // hand cards (server) a private
    SwapCards = 0x0B, // swap card from shown hand (byte 2) with card from main hand (byte 3) (client) b public
    Ready = 0x0C, // ready (client) c public

    //0xD //? d
    
    Shanked = 0x0F, // shanked (server) f public

    //0x80
    AddPlayer =0x81,  //name and join public
    RemovePlayer = 0x82 // public
    //0x83
    //0x84
    //0x85
    //0x86
    //0x87
    //0x88
    //0x89
    //0x8a
    //0x8b
    //0x8c
    //0x8d
    //0x8e
    //0x8f misc

}

public struct Data
{
    public readonly DataHeader dataHeader;
    public CardStruct[] cards => otherBytes.ToCardStructArray();

    public readonly byte[] otherBytes;

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
        this.otherBytes = cards.ToByteArray();
    }


    public Data(DataHeader dataHeader, byte[] bytes)
    {
        this.dataHeader = dataHeader;
        this.otherBytes = bytes;
    }

    public Data(DataHeader dataHeader)
    {
        this.dataHeader = dataHeader;
        this.otherBytes = Array.Empty<byte>();
    }

    public Data(byte[] data)
    {
        dataHeader=new DataHeader(data[0]);
        otherBytes = new byte[data.Length-1];
        for(int i=1;i<data.Length;i++)
        {
            otherBytes[i-1]=data[i];
        }
    }

    public override readonly string ToString()
    {
        return $"{dataHeader} cards:[{{{string.Join("} , {", otherBytes.ToCardStructArray())}}}]";        
    }
}