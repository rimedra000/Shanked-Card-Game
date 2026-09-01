using System;
using System.Collections.Generic;
using TMPro;

// using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private Image background;

    [NonSerialized] public int playerID=0;
    private bool turn=false;

    private void Awake()
    {
        ClientBehaviour.instance.onDataReceived += ReceiveData;
        var c = background.color;
        c.a=0;
        background.color=c;
    }

    private void ReceiveData(object _, Data data)
    {
        if(data.isGameEventData())
        {
            GameEventData gameEventData= data.GetGameEventData();
            if (gameEventData.header.gameEvent==GameEvent.StartTurn)
            {
                turn=gameEventData.header.player==playerID;
                if (turn)
                {
                    var c = background.color;
                    c.a=0;
                    background.color=c;
                }
                else
                {
                    var c = background.color;
                    c.a=1;
                    background.color=c;
                }
                return;
            }

            if (gameEventData.header.player!=playerID) return;
            CardStruct[] cards = gameEventData.cards;
            switch (gameEventData.header.gameEvent)
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
                case GameEvent.Ready:
                    Ready();
                    break;
            }
        }
        else
        {
            OtherEventData otherEventData= data.GetOtherEventData();
            OtherEventDataHeader otherEventDataHeader = otherEventData.header;
            if(otherEventDataHeader.otherEvent==OtherEvent.RemovePlayer&&otherEventDataHeader.miscData==playerID)
            {
                RemovePlayer();
            }
        }
    }

    private void Ready()
    {
        var c = background.color;
        c.a=c.a==0?1:0;
        background.color=c;
    }

    private void RemovePlayer()
    {
        ClientBehaviour.instance.onDataReceived -= ReceiveData;
        
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

    public void SetName(string name)
    {
        usernameText.text=name;
    }

    
}