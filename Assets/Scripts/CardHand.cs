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

    private bool hasEmptyHand=> hand.Count<=0;
    private bool hiddenCardsStage => shownCards[0]==null&&shownCards[1]==null&&shownCards[2]==null;

    private bool inSetup=true;
    private Card setupCardSwapCard;

    [SerializeField] private Transform handParent;
    [SerializeField] private Transform shownCardsParent;
    [SerializeField] private Transform hiddenCardsParent;


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
        
        drawPile.DrawCards(3,this);
        

        SortCards();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartTurn()
    {
        
        
        isTurn=true;
        //bool canPlay=false;
        if (!hasEmptyHand)//if has cards in hand
        {
            foreach (var card in hand)
            {
                if (playPile.ValidPlay(card.cardValue))
                {
                    return;
                }
            }
            isTurn=false;
            playPile.Shank(this);
            return;
        }
        else
        {
            if (hiddenCardsStage) return;
            
            foreach (var card in shownCards)
            {
                if (card == null) continue;
                if (playPile.ValidPlay(card.cardValue))
                {
                    return;
                }
                isTurn = false;
                playPile.Shank(this);
                return;
            }

        }
    }

    public void SelectCard(Card card)
    {
        bool fromMainHand=hand.Contains(card);
        bool fromHiddenCards=hiddenCards.Contains(card);
        bool fromShownCards=shownCards.Contains(card);
        
        if (inSetup&&(fromMainHand||fromShownCards))
        {
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
                var temp =setupCardSwapCard.cardId;
                setupCardSwapCard.cardId=card.cardId;
                card.cardId=temp;
                card.UpdateCardVisuals();
                setupCardSwapCard.UpdateCardVisuals();
                setupCardSwapCard.transform.Translate(0,-1,0);
                setupCardSwapCard=null;
                SortCards();
            }


            
            return;
        }
        if (!isTurn)return;
        if (!playPile.ValidPlay(card.cardValue)&&!fromHiddenCards) return;
        int currentSection;
        if (hand.Count>0&&fromMainHand)
        {
            currentSection=0;
        }
        
        else if(hand.Count==0&&!hiddenCardsStage&&fromShownCards)
        {
            currentSection=1;
        }
        else if(hand.Count==0&&hiddenCardsStage&&fromHiddenCards)
        {
            currentSection=2;
        }
        else
        {
            return;
        }

        
        if (selectedCards.Contains(card))
        {
            selectedCards.Remove(card);
            card.transform.Translate(0, -1, 0);
        }
        else if (currentSection!=0&&selectedCards.Count==0)//if main hand empty and no cards selected
        {            
            if (currentSection==1)//if in shown cards
            {
                

                selectedCards.Add(card);
                card.transform.Translate(0, 1, 0);
            }
            else if (currentSection==2)
            {
                //if (!hiddenCardsStage)return;
                
                hiddenCards[Array.IndexOf(hiddenCards,card)]=null;
                card.faceUp=true;
                card.UpdateCardVisuals();
                isTurn=false;

                if (playPile.ValidPlay(card.cardValue))
                {
                    playPile.Play(new List<Card>{card});    
                }
                else//unreachable path
                {
                    card.transform.SetParent(handParent,false);
                    hand.Add(card);
                    playPile.Shank(this);
                }
                
                
            }
            
        }
        else if(selectedCards.Count == 0||selectedCards[0].cardValue == card.cardValue)//if main hand
        {
            selectedCards.Add(card);
            card.transform.Translate(0, 1, 0);
        }
    }



    public void ReceiveCards(List<Card> cardsReceived,bool sortCards=true)
    {
        foreach (Card card in cardsReceived)
        {
            card.transform.SetParent(handParent,false);
            card.faceUp=true;
            card.UpdateCardVisuals();
            if (hand.Contains(card)) continue;
            hand.Add(card);
        }
        if (sortCards) SortCards(); 
    }

   

    public void PlaySelectedCards()
    {
        if (inSetup)
        {
            inSetup=false;
            return;
        }
        if (!isTurn) return;
        if (selectedCards.Count<=0)return;
        drawPile.DrawCards(3-hand.Count+selectedCards.Count,this);
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
        playPile.Play(selectedCards);
        selectedCards.Clear();
        SortCards();
        
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
    
    
}
