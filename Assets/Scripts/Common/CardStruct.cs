// [System.Serializable]
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

    public static CardStruct[] SortCards(this CardStruct[] cards,CardValue[] sortOrder)
    {
        CardStruct[] output = new CardStruct[cards.Length];
        int i=0;
        foreach (CardValue value in sortOrder)
        {
            foreach (CardStruct card in cards)
            {
                if (card == value) output[i++]=card;
            }
        }
        return output;
    }

}


public enum CardValue : byte
{
    //0 reserved
    Ace=0x01,
    Two=0x02,
    Three=0x03,
    Four=0x04,
    Five=0x05,
    Six=0x06,
    Seven=0x07,
    Eight=0x08,
    Nine=0x09,
    Ten=0x0A,//10
    Jack=0x0B,//11
    Queen=0x0C,//12
    King=0x0D,//13
    Joker=0x0E,//14
    //15 f
    Blank=0x80

}

public enum CardSuit : byte
{
    Spades=0x00,
    Diamonds=0x10,
    Clubs=0x20,
    Hearts=0x30

}

public enum CardDeck : byte
{
    One=0x00,
    Two=0x40
}