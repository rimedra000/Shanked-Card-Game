public class UnorderedCards
{
    public UnorderedCards(ulong[] ulongs)
    {
        a[0]=ulongs[0];
        a[1]=ulongs[1];
    }

    public UnorderedCards(ulong a,ulong b)
    {
        this.a[0]= a;
        this.a[1]= b;
    }

    public UnorderedCards(CardStruct[] cards)
    {
        foreach (CardStruct card in cards) Set(card);
    }



    private ulong[] a=new ulong[2];
    public bool Set(CardStruct card)
    {
        if(!isValidCard(card)) return false;
        if(Read(card))return false;
        byte b = card.ToByte();
        a[(b>>6)&1]|=1ul<<(b&0x3f);
        return true;
    }

    public bool Read(CardStruct card)
    {
        if(!isValidCard(card)) return false;
        byte b = card.ToByte();
        return ((a[(b>>6)&1]>>(b&0x3f))&1)==1;
    }

    public bool Clear(CardStruct card)
    {
        if(!isValidCard(card)) return false;
        if(!Read(card))return false;
        byte b = card.ToByte();
        a[(b>>6)&1]&=~(1ul<<(b&0x3f));
        return true;
    }

    public bool Toggle(CardStruct card)
    {
        if(!isValidCard(card)) return false;
        if(Read(card))return false;
        byte b = card.ToByte();
        a[(b>>6)&1]^=1ul<<(b&0x3f);
        return true;
    }

    public CardStruct[] ToCards()
    {
        CardStruct[] cards=new CardStruct[108];
        int index=0;
        for (int j = 0; j < 64; j++)
        {
            ulong b=1ul<<j;
            if ((a[0]&b)==b)
            {
                cards[index++] = new CardStruct((CardValue)(j&0xf),(CardSuit)(j&0x30),CardDeck.One);
            }
        }
        for (int j = 0; j < 64; j++)
        {
            ulong b=1ul<<j;
            if ((a[1]&b)==b)
            {
                cards[index++] = new CardStruct((CardValue)(j&0xf),(CardSuit)(j&0x30),CardDeck.Two);
            }
        }
        var result = new CardStruct[index];
        for (int j = 0; j < index; j++)
        {
            result[j]=cards[j];
        }
        return result;

    }

    public ulong[] ToUInt64Array()
    {
        return (ulong[])a.Clone();
    }
    public (ulong,ulong) ToUInt64s()
    {
        return (a[0],a[1]);
    }

    private bool isValidCard(CardStruct card)
    {
        if(card.value==CardValue.Blank) return false;
        if(card.value==CardValue.Joker)
        {
            switch (card.suit)
            {
                case CardSuit.Spades:
                case CardSuit.Diamonds:
                    return true;
                case CardSuit.Clubs:
                case CardSuit.Hearts:
                    return false;
            }
        }
        return true;
    }

    public static UnorderedCards AllCards()
    {
        return new UnorderedCards(0x3ffe_3ffe_7ffe_7ffe,0x3ffe_3ffe_7ffe_7ffe);
    }

    public static UnorderedCards NoCards()
    {
        return new UnorderedCards(0,0);
    }

}
