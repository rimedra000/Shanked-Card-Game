[System.Serializable]
public readonly struct CardStruct  
{
    private readonly byte id;

    public CardStruct(CardValue value,CardSuit cardSuit,CardDeck cardDeck)
    {
        id = (byte)((byte)value + (byte)cardSuit + (byte)cardDeck);
    }

    public byte ToByte()
    {
        return id;
    }

    public readonly CardValue value => (CardValue)(id & 0b1_0_00_1111);
    public readonly CardSuit suit => (CardSuit)(id & 0b0_0_11_0000);

    public readonly CardDeck deck => (CardDeck)(id & 0b0_1_00_0000);

    public readonly override string ToString()
    {
        return $"Deck {deck}, {value} of {suit}";
    }

    public CardStruct(byte data)
    {
        id=data;
    }

    public CardStruct Censored()
    {
        byte temp = id;
        
        temp &= 0b_01110000;
        temp |= 0b_10000000;

        return new CardStruct(temp);

    }

    
    public static bool operator ==(CardStruct card1,CardStruct card2)
    {
        return card1.id==card2.id;
    }

    public static bool operator !=(CardStruct card1,CardStruct card2)
    {
        return card1.id!=card2.id;
    }

    public override bool Equals(object obj)
    {
        return obj is CardStruct && this == (CardStruct)obj;
    }

    public override int GetHashCode()
    {
        return id;
    }

    public static implicit operator CardValue(CardStruct card)
    {
        return card.value;
    }

    
    
}

public static class CardExtensions
{
    public static bool IsGreaterThanOrEqualTo(this CardValue card1,CardValue card2)
    {
        if(card1==CardValue.Ace) return true;
        if(card2==CardValue.Ace) return false;
        return (byte)card1>=(byte)card2;

    }

    public static byte[] ToByteArray(this CardStruct[] cards)
    {
        byte[] output=new byte[cards.Length];
        for (int i = 0; i < cards.Length; i++)
        {
            output[i] = cards[i].ToByte();
        }
        return output;
    }

    public static CardStruct[] ToCardStructArray(this byte[] cards)
    {
        CardStruct[] output=new CardStruct[cards.Length];
        for (int i = 0; i < cards.Length; i++)
        {
            output[i] = new CardStruct(cards[i]);
        }
        return output;
    }

}


public enum CardValue : byte
{
    Ace=1,
    Two=2,
    Three=3,
    Four=4,
    Five=5,
    Six=6,
    Seven=7,
    Eight=8,
    Nine=9,
    Ten=10,//a
    Jack=11,//b
    Queen=12,//c
    King=13,//d
    Joker=14,//e
    Blank=0x80

}

public enum CardSuit : byte
{
    Spades=0<<4,
    Diamonds=1<<4,
    Clubs=2<<4,
    Hearts=3<<4

}

public enum CardDeck : byte
{
    One=0<<6,
    Two=1<<6
}