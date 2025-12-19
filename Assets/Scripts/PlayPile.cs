using System.Collections.Generic;
using UnityEngine;
using Value=Card.Value;

public class PlayPile : MonoBehaviour
{

    [SerializeField] private List<Card> cards = new();
    private List<Card> selectedCards = new();

    [SerializeField] private Vector3 selectOffset;

    private DiscardPile discardPile;

    public static PlayPile instance;

    private CardHand[] cardHands;

    
    private int cardHandIndex=0;

    private void Awake() {
        instance=this;
        cardHands=FindObjectsByType<CardHand>(FindObjectsSortMode.None);
        discardPile=DiscardPile.instance;
    }

    private void Start()
    {
        cardHands[cardHandIndex].StartTurn();
    }
    public void ReceivePlay(List<Card> cardsReceived)
    {
        foreach (Card card in cardsReceived)
        {
            if (cards.Contains(card)) continue;
            card.transform.SetParent(transform);
            card.transform.localPosition = Vector3.zero;
            //card.transform.SetSiblingIndex(0);
            cards.Insert(0, card);
        }



        if (FourMatch())
        {
            Debug.Log("4 match Played");
            discardPile.ReceiveCards(Card.TransformCardsToData(cards));
            
            cards.Clear();
            cardHandIndex--;
        }
        
        if (CardValue() == Value.Ten)
        {
            Debug.Log("ten Played");
            discardPile.ReceiveCards(Card.TransformCardsToData(cards));
            cards.Clear();
            cardHandIndex--;
        }

        if (cardsReceived[0].cardValue==Value.Jack)
        {
            int jackCount =1;
            while (jackCount<cardsReceived.Count&&cardsReceived[jackCount].cardValue==Value.Jack)
            {
                jackCount++;
            }
            cardHandIndex+=jackCount; 
        }

        while (CardValue() == Value.Joker)
        {
            Debug.Log("joker Played");
            discardPile.ReceiveCards(new List<int>{cards[0].cardId});
            Destroy(cards[0].gameObject);
            cards.RemoveAt(0);
            
            //rotate hands
            List<Card> tempCards=cardHands[^1].SwapHand(new List<Card>());
            for (int i = 0; i < cardHands.Length; i++)
            {
                tempCards = cardHands[i].SwapHand(tempCards);
            }

        }
        cardHandIndex++;
        cardHandIndex%=cardHands.Length;
        cardHands[cardHandIndex].StartTurn();
        
    }

    private bool FourMatch()
    {
        int count =0;
        int index=0;
        Value value=CardValue(0);
        while (CardValue(index)!=Value.Blank)
        {
            if (CardValue(index)==value)
            {
                count++;
                index++;
            }
            else if(CardValue(index)==Value.Eight)
            {
                index++;
            }
            else
            {
                return false; 
            }

            if (count>=4)
            {
                return true;
            }
        }
        return false;
    }


    // public void CardToggle(Card card)
    // {
    //     if (!cards.Contains(card)) { Debug.Log("unknown card"); return; }
    //     if (selectedCards.Count > 0 && selectedCards[0].cardValue != card.cardValue) return;


    //     Debug.Log(card.cardId);
    //     if (selectedCards.Contains(card))
    //     {
    //         selectedCards.Remove(card);
    //         card.transform.Translate(-selectOffset);
    //     }
    //     else
    //     {
    //         selectedCards.Add(card);
    //         card.transform.Translate(selectOffset);
    //     }
    // }

    public void Shank(CardHand cardHand)
    {
        cardHand.ReceiveCards(cards);
        cards.Clear();
        cardHandIndex++;
        cardHandIndex%=cardHands.Length;
        cardHands[cardHandIndex].StartTurn();
        
    }


    public Value CardValue(int index=0)
    {
        if (cards.Count <= index) return Card.Value.Blank;
        return cards[index].cardValue;
    }



   
    
    public bool ValidPlay(Value value,int topCardIndex=0)
    {
        Value lastCard = CardValue(topCardIndex);
        if (lastCard==Value.Eight)
        {
            return ValidPlay(value, topCardIndex + 1);
        }
        if (value == lastCard)
        {
            return true;
        }
        if (lastCard == Value.Blank || lastCard == Value.Two || lastCard == Value.Three)
        {
            return true;
        }
        if (value == Value.Joker || value == Value.Two || value == Value.Eight || value == Value.Ten)
        {
            return true;
        }
        if (lastCard == Value.Seven)
        {
            if ((int)value <= (int)Value.Seven)
            {
                return true;
            }
            else return false;
        }
        else if ((int)value >= (int)lastCard)
        {
            return true;
        }
        else return false;
    }



}

