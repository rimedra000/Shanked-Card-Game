using System;
// using System.Linq;

public readonly struct GameEventDataHeader
{
    public readonly byte data;
    public GameEventDataHeader(byte player, GameEvent gameEvent)
    {
        player &= 0b_00000111;
        player <<= 4;
        byte gameEventByte = (byte)gameEvent;
        gameEventByte &=0x_0F;
        data = (byte)(gameEventByte | player);
    }

    public GameEventDataHeader(byte data)
    {
        this.data = data;
    }

    public readonly GameEvent gameEvent => (GameEvent)(data & 0x0F);
    public readonly byte player => (byte)((data & 0b_0111_0000) >> 4);

    public override readonly string ToString()
    {
        return $"player:{player} Game Event:{gameEvent}";
    }


}


public enum GameEvent : byte
{
    
    
    //                        shown to other players | sent from ___ | description
    //0x00 0

    
    PlayCards = 0x1,       // public  | client | play cards from main hand 
    MoveShownCard = 0x2,   // public  | client | place shown card into main hand 
    MoveHiddenCard = 0x3,  // private | client | place hidden card into main hand 
    SwapHand = 0x4,        // private | server | swap hand 
    DrawCards = 0x5,       // private | server | draw cards 
    StartTurn = 0x6,       // public  | server | turn 
    ClearPile = 0x7,       // public  | server | move play pile to discard pile 
    DealHiddenCards = 0x8, // private | server | hidden cards 
    DealShownCards = 0x9,  // public  | server | shown cards 
    DealHandCards = 0xA,   // private | server | hand cards 
    SwapCards = 0xB,       // public  | client | swap card from shown hand (byte 2) with card from main hand (byte 3) 
    Ready = 0xC,           // public  | client | ready 

    //0x0D ? 
    
    //0x0E
    Shanked = 0x0F, //      // public  | server | shanked
}

public struct GameEventData
{
    public readonly GameEventDataHeader header;
    public readonly CardStruct[] cards;// => otherBytes.ToCardStructArray();

    // public readonly byte[] otherBytes;

    public byte[] ToBytes()
    {
        var result = new byte[cards.Length+1];
        result[0]=header.data;
        for(int i=0;i<cards.Length;i++)
        {
            result[i+1]=cards[i].ToByte();
        }
        return result;
    }

    public GameEventData(GameEventDataHeader dataHeader, CardStruct[] cards)
    {
        this.header = dataHeader;
        this.cards = cards;
    }


    // public Data(DataHeader dataHeader, byte[] bytes)
    // {
    //     this.dataHeader = dataHeader;
    //     this.otherBytes = bytes;
    // }

    public GameEventData(GameEventDataHeader dataHeader)
    {
        header = dataHeader;
        cards = Array.Empty<CardStruct>();
    }

    public GameEventData(byte[] data)
    {
        header=new GameEventDataHeader(data[0]);
        cards = data[1..].ToCardStructArray();
        // for(int i=1;i<data.Length;i++)
        // {
        //     cards[i-1]=data[i];
        // }
    }

    public override readonly string ToString()
    {
        return $"{header} cards:[{{{string.Join("} , {", cards)}}}]";        
    }

}

public enum OtherEvent : byte
{
    //0x0
    AddPlayer =0x1,  //name and join public
    RemovePlayer = 0x2 // publicvar result = new byte[cards.Length+1];
    //0x3
    //0x4
    //0x5
    //0x6
    //0x7
    //0x8
    //0x9
    //0xa
    //0xb
    //0xc
    //0xd
    //0xe
    //0xf misc
}

public struct OtherEventData
{
    public readonly OtherEventDataHeader header;
    public readonly byte[] miscBytes;


    public byte[] ToBytes()
    {
        var result = new byte[miscBytes.Length+1];
        result[0]=header.data;
        miscBytes.CopyTo(result,1);
        
        return result;
    }


    public OtherEventData(OtherEventDataHeader dataHeader, byte[] bytes)
    {
        header = dataHeader;
        miscBytes = bytes;
    }

    public OtherEventData(OtherEventDataHeader dataHeader)
    {
        header = dataHeader;
        miscBytes=Array.Empty<byte>();
    }

    public OtherEventData(byte[] data)
    {
        header=new OtherEventDataHeader(data[0]);
        miscBytes=data[1..];
    }

    

    // public override readonly string ToString()
    // {
    //     return $"{header} cards:[{{{string.Join("} , {", cards)}}}]";        
    // }

}

public readonly struct OtherEventDataHeader
{
    public readonly byte data;
    public OtherEventDataHeader(byte miscData, OtherEvent otherEvent)
    {
        miscData &= 0b_00000111;
        miscData <<= 4;
        byte otherEventByte = (byte)otherEvent;
        otherEventByte &=0x_0F;
        data = (byte)(otherEventByte | miscData | 0x80);
    }

    public OtherEventDataHeader(byte data)
    {
        this.data = data;
    }

    public readonly OtherEvent otherEvent => (OtherEvent)(data & 0b_0000_1111);
    public readonly byte miscData => (byte)((data & 0b_0111_0000) >> 4);

    // public override readonly string ToString()
    // {
    //     return $"player:{player} Other Event:{gameEvent}";
    // }
}

public struct Data
{
    public readonly byte[] bytes;
    public GameEventData GetGameEventData() => new(bytes);
    public bool isGameEventData()=>(bytes[0]&0x80)==0x00;

    public OtherEventData GetOtherEventData() => new(bytes);
    public bool isOtherEventData()=> (bytes[0]&0x80)==0x80;
    

    public static implicit operator Data(GameEventData gameEventData)=> new (gameEventData.ToBytes());
    public static implicit operator Data(OtherEventData otherEventData)=> new (otherEventData.ToBytes());
    public Data(byte[] bytes)
    {
        this.bytes=bytes;
    }
}

