using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Card : MonoBehaviour
{
    [Range(0,107)]public int cardId{get; private set;}
    public Value cardValue => IdToEnums(cardId).Item1;
    public Image cardImage;
    public bool faceUp=true;
    const int CardsInDeck = 52;
    const int CardsInSuit = 13;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateCardVisuals();
    }

    // Update is called once per frame
 
    void OnMouseDown()
    {
        SendMessageUpwards(nameof(CardHand.SelectCard), this);
    }

    public void SendCardToggle()
    {
        SendMessageUpwards(nameof(CardHand.SelectCard),this);
    }

    public void UpdateCardVisuals()
    {
        if (faceUp||cardValue==Value.Blank)
        {
            string name = ValueToString(cardValue);
            gameObject.name = name;

            Sprite cardSprite = Resources.Load<Sprite>("Card Art/Front/Hearts/" + name);
            cardImage.sprite = cardSprite;    
        }
        else
        {
            cardImage.sprite=null;
        }
        
        
        
    }

    public void SetCardId(int id)
    {
        cardId=id;
        UpdateCardVisuals();
    }

    public static List<int> TransformCardsToData(List<Card> cards)
    {
        List<int> result=new();
        result.Capacity=cards.Count;
        for (int i = 0; i < cards.Count; i++)
        {
            result.Add(cards[i].cardId);
            Destroy(cards[i].gameObject);
        }
        return result;
    }

    public static List<Card> InstantiateCardsFromData(List<int> data,Transform parentTransform,GameObject cardPrefab)
    {
        List<Card> result=new();
        result.Capacity=data.Count;
        
        foreach (int cardValue in data)
        {
            //if (cards.Contains(card)) continue;
            Card card =Instantiate(cardPrefab,parentTransform).GetComponent<Card>();
            card.SetCardId(cardValue);
            //card.UpdateCardVisuals();
            //card.transform.SetParent(transform);
            //card.transform.localPosition = Vector3.zero;
            result.Add(card);
        }
        return result;
        
    }



    public static (Value, Suit, Deck) IdToEnums(int id)
    {
        if (id>=108||id<0)
        {
            throw new ArgumentOutOfRangeException();
        }
        if (id >= 104)
        {
            id -= 104;
            return (Value.Joker, (Suit)(id % 2), (Deck)(id / 2));
        }
        var val = id % CardsInSuit;
        var suit = id / CardsInSuit % 4;
        var deck = id / CardsInDeck % 2;
        return ((Value)val, (Suit)suit, (Deck)deck);
    }

    public static Value IdToValue(int id)
    {
        if (id>=108||id<0)
        {
            throw new ArgumentOutOfRangeException();
        }
        if (id >= 104)
        {
            return Value.Joker;
        }
        int val = id % CardsInSuit;
        return (Value)val;
    }

    public static int EnumsToId(Value value, Suit suit, Deck deck)
    {
        if (value == Value.Joker)
        {
            return 104 + (int)deck * 2 + (int)suit;
        }

        return (int)deck * CardsInDeck + (int)suit * CardsInSuit + (int)value;
    }

    

    public static string ValueToString(Value value)
    {
        return value switch
        {
            Value.Ace => "Ace",
            Value.Two => "2",
            Value.Three => "3",
            Value.Four => "4",
            Value.Five => "5",
            Value.Six => "6",
            Value.Seven => "7",
            Value.Eight => "8",
            Value.Nine => "9",
            Value.Ten => "10",
            Value.Jack => "Jack",
            Value.Queen => "Queen",
            Value.King => "King",
            Value.Joker => "Joker",
            Value.Blank => "Blank",
            _ => throw new NotImplementedException()
        };
    }

    public enum Value
    {
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace,
        Joker,

        Blank

    }

    public enum Suit
    {
        Spades,
        Diamonds,
        Clubs,
        Hearts

    }

    public static string SuitToString(Suit suit)
    {
        return suit switch
        {
            Suit.Clubs => "Clubs",
            Suit.Diamonds => "Diamonds",
            Suit.Hearts => "Hearts",
            Suit.Spades => "Spades",
            _ => throw new NotImplementedException()
        };
    }
    public enum Deck
    {
        One,
        Two
    }

    void OnValidate()
    {
        UpdateCardVisuals();
    }

    


}
