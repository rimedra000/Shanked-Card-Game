using System;
using System.Collections.Generic;
// using System.Linq;
using UnityEngine;

public class NetworkedPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private List<CardObject> handCards = new(3);
    private List<CardObject> hiddenCards = new (3);
    private List<CardObject> shownCards = new (3);

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handCardsTransform;
    [SerializeField] private Transform shownCardsTransform;
    [SerializeField] private Transform hiddenCardsTransform;

    public int playerID=0;
    private bool turn=false;

    private void Awake()
    {
        GameManager.clientBehaviour.onDataReceived += ReceiveData;
    }

    private void ReceiveData(object _, Data e)
    {
        if (e.dataHeader.gameEvent==GameEvent.StartTurn)
        {
            turn=e.dataHeader.player==playerID;
            return;
        }

        if (e.dataHeader.player!=playerID) return;
        CardStruct[] cards = e.cards;
        switch (e.dataHeader.gameEvent)
        {
            case GameEvent.PlayCards:
                PlayCards(cards);
                break;
            case GameEvent.MoveShownCard:
                MoveShownCard(cards);
                break;
            case GameEvent.MoveHiddenCard:
                MoveHiddenCard(cards);
                break;
            case GameEvent.SwapHand:
                SwapHand(cards);
                break;
            case GameEvent.DrawCards:
                DrawCards(cards);
                break;
            case GameEvent.StartTurn:
                break;
            case GameEvent.DealHiddenCards:
                DealHiddenCards(cards);
                break;
            case GameEvent.DealShownCards:
                DealShownCards(cards);
                break;
            case GameEvent.DealHandCards:
                DealHandCards(cards);
                break;
            case GameEvent.SwapCards:
                SwapCards(cards);
                break;
            case GameEvent.Shanked:
                Shanked(cards);
                break;
            case GameEvent.RemovePlayer:
                RemovePlayer();
                break;
        }
    }

    private void RemovePlayer()
    {
        GameManager.clientBehaviour.onDataReceived -= ReceiveData;
        
        Destroy(gameObject);
    }

    private void Shanked(CardStruct[] cards)
    {
        foreach (CardStruct card in cards)
        {
            handCards.Add(MakeCard(card,handCardsTransform));
        }
    }

    private void SwapCards(CardStruct[] cards)
    {
        RemoveCard(cards[0],shownCards);
        shownCards.Add(MakeCard(cards[1],shownCardsTransform));
        RemoveCard(cards[1].Censored(),handCards);
        handCards.Add(MakeCard(cards[0].Censored(),handCardsTransform));
    }

    private void DealHandCards(CardStruct[] cards)
    {
        foreach (CardStruct card in cards)
        {
            handCards.Add(MakeCard(card,handCardsTransform));
        }
    }

    private void DealShownCards(CardStruct[] cards)
    {
        foreach (CardStruct card in cards)
        {
            shownCards.Add(MakeCard(card,shownCardsTransform));
        }
    }

    private void DealHiddenCards(CardStruct[] cards)
    {
        foreach (CardStruct card in cards)
        {
            hiddenCards.Add(MakeCard(card,hiddenCardsTransform));
        }
    }

    private void DrawCards(CardStruct[] cards)
    {
        foreach (CardStruct card in cards)
        {
            handCards.Add(MakeCard(card,handCardsTransform));
        }
    }

    private void SwapHand(CardStruct[] cards)
    {
        for (int i = handCardsTransform.childCount - 1; i >= 0 ; i--)
        {
            Destroy(handCardsTransform.GetChild(i).gameObject);
        }
        handCards.Clear();

        foreach (CardStruct card in cards)
        {
            handCards.Add(MakeCard(card,handCardsTransform));
        }
    }

    private void MoveHiddenCard(CardStruct[] cards)
    {
        CardStruct card = cards[0];
        RemoveCard(card,hiddenCards);
        handCards.Add(MakeCard(card,handCardsTransform));
    }

    private void MoveShownCard(CardStruct[] cards)
    {
        CardStruct card = cards[0];
        RemoveCard(card,shownCards);
        handCards.Add(MakeCard(card.Censored(),handCardsTransform));
        
    }

    private void PlayCards(CardStruct[] cards)
    {
        foreach (CardStruct card in cards)
        {
            RemoveCard(card.Censored(),handCards);
        }   
    }

    private CardObject MakeCard(CardStruct card,Transform parent)
    {
        GameObject gameObject = Instantiate(cardPrefab,parent);
        CardObject cardObject = gameObject.GetComponent<CardObject>();
        cardObject.setCardStruct(card);
        return cardObject;
    }

    private void RemoveCard(CardStruct card,List<CardObject> list)
    {
        var a = list.Find(c => c.cardStruct==card);
        Destroy(a.gameObject);
        list.Remove(a);
    }

    
}
