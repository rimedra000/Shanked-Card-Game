using System;
using UnityEngine;

[System.Serializable]
public readonly struct CardStruct  
{
    private readonly byte id;

    public CardStruct(CardValue value,CardSuit cardSuit,CardDeck cardDeck)
    {
        id = (byte)((byte)value + (byte)cardSuit + (byte)cardDeck);
    }

    public readonly CardValue value => (CardValue)(id & 0b0_0_00_1111);
    public readonly CardSuit suit => (CardSuit)(id & 0b0_0_11_0000);

    public readonly CardDeck deck => (CardDeck)(id & 0b0_1_00_0000);

    public readonly override string ToString()
    {
        return $"Deck {deck}, {value} of {suit}";
    }

    public static explicit operator byte(CardStruct cardStruct)
    {
        return cardStruct.id;
    }

    public static explicit operator CardStruct(byte data)
    {
        return new CardStruct(data);
        
    }

    private CardStruct(byte data)
    {
        id=data;
    }
}

public static class CardExtensions
{
    // public static string SuitToString(this CardSuit suit)
    // {
    //     return suit switch
    //     {
    //         CardSuit.Clubs => "Clubs",
    //         CardSuit.Diamonds => "Diamonds",
    //         CardSuit.Hearts => "Hearts",
    //         CardSuit.Spades => "Spades",
    //         _ => throw new NotImplementedException()
    //     };
    // }

    // public static string ValueToString(this CardValue value)
    // {
    //     return value switch
    //     {
    //         CardValue.Ace => "Ace",
    //         CardValue.Two => "2",
    //         CardValue.Three => "3",
    //         CardValue.Four => "4",
    //         CardValue.Five => "5",
    //         CardValue.Six => "6",
    //         CardValue.Seven => "7",
    //         CardValue.Eight => "8",
    //         CardValue.Nine => "9",
    //         CardValue.Ten => "10",
    //         CardValue.Jack => "Jack",
    //         CardValue.Queen => "Queen",
    //         CardValue.King => "King",
    //         CardValue.Joker => "Joker",
    //         _ => throw new NotImplementedException()
    //     };
    // }

    // public static string DeckToString(this CardDeck deck)
    // {
    //     return deck switch
    //     {
    //         CardDeck.One => "One",
    //         CardDeck.Two => "Two",
    //         _ => throw new NotImplementedException()
    //     };
    // }

    // public static string CardToString(this CardStruct cardStruct)
    // {
    //     return $"Deck {cardStruct.deck.DeckToString()} {cardStruct.value.ValueToString()} Of {cardStruct.suit.SuitToString()}";
    // }
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
    Ten=10,
    Jack=11,
    Queen=12,
    King=13,
    Joker=14

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