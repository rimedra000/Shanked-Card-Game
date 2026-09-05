#region GameEvent
public struct GameEventData
{
    private readonly byte header;
    public readonly GameEvent gameEvent => (GameEvent)(header & 0x0F);
    public readonly byte player => (byte)((header & 0b_0111_0000) >> 4);
    public readonly CardStruct[] cards;

    public byte[] ToBytes()
    {
        var result = new byte[cards.Length+1];
        result[0]=header;
        cards.ToByteArray().CopyTo(result,1);
        return result;
    }
    public GameEventData(byte player, GameEvent gameEvent, CardStruct[] cards):this(player,gameEvent)
    {
        this.cards = cards;
    }
    public GameEventData(byte player, GameEvent gameEvent)
    {
        player &= 0b_00000111;
        player <<= 4;
        byte gameEventByte = (byte)gameEvent;
        gameEventByte &=0x_0F;
        header = (byte)(gameEventByte | player);
        cards = new CardStruct[0];
    }
    public GameEventData(byte[] data)
    {
        header=data[0];
        cards = data[1..].ToCardStructArray();
    }
    public override readonly string ToString()
    {
        return $"player:{player} Game Event:{gameEvent} cards:[{{{string.Join("} , {", cards)}}}]";        
    }
}

public enum GameEvent : byte
{
    //                        shown to other players | sent from ___ | description
    //0x00 0 //reserved
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

#endregion
#region OtherEvent

public struct OtherEventData
{
    private readonly byte header;
    public readonly OtherEvent otherEvent => (OtherEvent)(header & 0b_0000_1111);
    public readonly byte miscData => (byte)((header & 0b_0111_0000) >> 4);
    public readonly byte[] miscBytes;

    public byte[] ToBytes()
    {
        var result = new byte[miscBytes.Length+1];
        result[0]=header;
        miscBytes.CopyTo(result,1);
        return result;
    }
    public OtherEventData(byte miscData, OtherEvent otherEvent, byte[] bytes):this(miscData,otherEvent)
    {
        miscBytes=bytes;
    }
    public OtherEventData(byte miscData, OtherEvent otherEvent)
    {
        miscData &= 0b_00000111;
        miscData <<= 4;
        byte otherEventByte = (byte)otherEvent;
        otherEventByte &=0x_0F;
        header = (byte)(otherEventByte | miscData | 0x80);
        miscBytes=new byte[0];
    }
    public OtherEventData(byte[] data)
    {
        header=data[0];
        miscBytes=data[1..];
    }
    public override readonly string ToString()
    {
        return $"miscData:{miscData} Other Event:{otherEvent} cards:[{{{string.Join("} , {", miscBytes)}}}]";        
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
#endregion
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