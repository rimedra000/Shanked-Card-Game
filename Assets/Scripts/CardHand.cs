using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardHand : MonoBehaviour
{
    private List<Card> hand = new();
    private Card[] hiddenCards = new Card[3];
    private Card[] shownCards = new Card[3];

    private List<Card> selectedCards = new();

    private PlayPile playPile;
    private DrawPile drawPile;
    private bool isTurn=false;

    //private bool hasEmptyHand=> hand.Count<=0;
    //private bool hiddenCardsStage => shownCards[0]==null&&shownCards[1]==null&&shownCards[2]==null;

    //private bool inSetup=true;
    private Card setupCardSwapCard;
    [SerializeField] private GameObject cardPrefab;

    [SerializeField] private Transform handParent;
    [SerializeField] private Transform shownCardsParent;
    [SerializeField] private Transform hiddenCardsParent;

    private HandStage handStage=HandStage.Setup;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playPile = PlayPile.instance;
        drawPile = DrawPile.instance;
        
        
        for (int i = 0; i < hiddenCards.Length; i++)
        {
            Card card = drawPile.DrawCard();
            hiddenCards[i]=card;
            card.faceUp=false;
            card.transform.SetParent(hiddenCardsParent,false);
            card.transform.localPosition=Vector3.zero;
        }
        for (int i = 0; i < shownCards.Length; i++)
        {
            Card card = drawPile.DrawCard();
            shownCards[i]=card;
            card.faceUp=true;
            card.UpdateCardVisuals();
            card.transform.SetParent(shownCardsParent,false);
            card.transform.localPosition=Vector3.zero;
        }
        
        ReceiveCards(drawPile.DrawCards(3));
        
        

        SortCards();

    }


    private enum HandStage
    {
        Setup,
        MainHand,
        ShownCards,
        HiddenCards,
        Won,
    }

    public void StartTurn()
    {
        isTurn=true;
        //bool canPlay=false;
        //TODO: Fix bug at start where hand is not checked to be playable
        if (handStage==HandStage.MainHand)//if has cards in hand
        {
            foreach (Card card in hand)
            {
                if (playPile.ValidPlay(card.cardValue))
                {
                    return;
                }
            }
            isTurn=false;
            ReceiveCards(playPile.Shank());
            return;
        }
        else if (handStage==HandStage.ShownCards)
        {
            foreach (Card card in shownCards)
            {
                if (card == null) continue;
                if (playPile.ValidPlay(card.cardValue))
                {
                    return;
                }
                isTurn = false;
                ReceiveCards(playPile.Shank());
                return;
            }

        }
    }


    private void UpdateHandStage()
    {
        if (handStage==HandStage.Setup)
        {
            return;
        }
        if(hand.Count>0)
        {
            handStage=HandStage.MainHand;
        }
        else if (!IsEmpty(shownCards))
        {
            handStage= HandStage.ShownCards;
        }
        else if (!IsEmpty(hiddenCards))
        {
            handStage=HandStage.HiddenCards;
        }
        else
        {
            handStage=HandStage.Won;
            Debug.Log("win");
        }
        
    }

    public void SelectCard(Card card)
    {
        bool fromMainHand=hand.Contains(card);
        bool fromHiddenCards=hiddenCards.Contains(card);
        bool fromShownCards=shownCards.Contains(card);
        
        if (handStage==HandStage.Setup&&(fromMainHand||fromShownCards))
        {//setup
            if (setupCardSwapCard==null)
            {
                setupCardSwapCard=card;
                card.transform.Translate(0, 1, 0);
                
            }
            else if (setupCardSwapCard==card)
            {    
                setupCardSwapCard=null;
                card.transform.Translate(0, -1, 0);
            }
            else
            {
                int tempId =setupCardSwapCard.cardId;
                setupCardSwapCard.SetCardId(card.cardId);
                card.SetCardId(tempId);
                setupCardSwapCard.transform.Translate(0,-1,0);
                setupCardSwapCard=null;
                SortCards();
            }


            
            return;
        }
        if (!isTurn)return;
        if (!playPile.ValidPlay(card.cardValue)&&!fromHiddenCards) return;
        if ((handStage != HandStage.MainHand || !fromMainHand) && (handStage != HandStage.ShownCards || !fromShownCards) && (handStage != HandStage.HiddenCards || !fromHiddenCards))
        {//if card not valid for current stage return
            return;
        }


        if (selectedCards.Contains(card))//unselect card
        {
            selectedCards.Remove(card);
            card.transform.Translate(0, -1, 0);
        }
        else if (handStage!=HandStage.MainHand&&selectedCards.Count==0)//if main hand empty and no cards selected
        {            
            if (handStage==HandStage.ShownCards)//if in shown cards
            {
                selectedCards.Add(card);
                card.transform.Translate(0, 1, 0);
            }
            else if (handStage==HandStage.HiddenCards)
            {
                //if (!hiddenCardsStage)return;
                
                hiddenCards[Array.IndexOf(hiddenCards,card)]=null;
                card.faceUp=true;
                card.UpdateCardVisuals();
                isTurn=false;

                if (playPile.ValidPlay(card.cardValue))
                {
                    playPile.ReceivePlay(Card.TransformCardsToData(new List<Card>{card}));    
                    UpdateHandStage();
                }
                else
                {
                    card.transform.SetParent(handParent,false);
                    hand.Add(card);
                    ReceiveCards(playPile.Shank());
                }
                
                
            }
            
        }
        else if(selectedCards.Count == 0||selectedCards[0].cardValue == card.cardValue)//if main hand
        {
            selectedCards.Add(card);
            card.transform.Translate(0, 1, 0);
        }
    }



    // public void ReceiveCards(List<Card> cardsReceived)
    // {
    //     foreach (Card card in cardsReceived)
    //     {
    //         card.transform.SetParent(handParent,false);
    //         card.faceUp=true;
    //         card.UpdateCardVisuals();
    //         if (hand.Contains(card)) continue;
    //         hand.Add(card);
    //     }
    //     SortCards(); 
    // }


    public void ReceiveCards(List<int> cardsReceived)
    {
        hand.AddRange(Card.InstantiateCardsFromData(cardsReceived,handParent,cardPrefab));
        SortCards();
        UpdateHandStage();
    }

   

    public void PlaySelectedCards()
    {
        if (handStage==HandStage.Setup)
        {
            handStage=HandStage.MainHand;
            return;
        }
        if (!isTurn) return;
        if (selectedCards.Count<=0)return;
        ReceiveCards(drawPile.DrawCards(3-hand.Count+selectedCards.Count));
        isTurn=false;
        foreach (Card card in selectedCards)
        {
            if (hand.Contains(card))
            {
                hand.Remove(card);
            }
            else if(shownCards.Contains(card))
            {
                shownCards[Array.IndexOf(shownCards,card)]=null;
            }
            else if (hiddenCards.Contains(card))
            {
                hiddenCards[Array.IndexOf(hiddenCards,card)]=null;
            }
            
            
        }
        playPile.ReceivePlay(Card.TransformCardsToData(selectedCards));
        selectedCards.Clear();
        SortCards();
        UpdateHandStage();
    }

    private void SortCards()
    {
        hand = hand.OrderBy(card => (int)card.cardValue).ToList();
        for (int i = 0; i < hand.Count; i++) hand[i].transform.SetSiblingIndex(i);

    }

    public List<Card> SwapHand(List<Card> otherHand)
    {
        var temp = hand;
        hand= otherHand;

        foreach (Card card in hand)
        {
            //if (hand.Contains(card)) continue;
            card.transform.SetParent(handParent);
            card.faceUp=true;
            card.UpdateCardVisuals();
            //hand.Add(card);
        }
        SortCards();

        return temp;
    }
    
    private bool IsEmpty(Array array)
    {
        foreach (var item in array)
        {
            if (item!=null) return true;
        }
        return false;
    }
    
}
