using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayPile : MonoBehaviour
{

    private List<CardObject> cards = new();
    [SerializeField] private GameObject cardPrefab;
    //private List<Card> selectedCards = new();

    // [SerializeField] private Vector3 selectOffset;

    // private DiscardPile discardPile;

    // public static PlayPile instance;

    // private Player[] cardHands;
    // private bool[] a;

    
    // private int cardHandIndex=0;

    private void Awake() {
        // instance=this;
        // cardHands=FindObjectsByType<Player>(FindObjectsSortMode.None);
        // GameManager.playPile=this;
        
    }

    private void ReceiveData(object _, Data data)
    {
        if (!data.isGameEventData()) return;
        GameEventData gameEventData=data.GetGameEventData();
        switch (gameEventData.header.gameEvent)
        {
            case GameEvent.PlayCards:
                // cards = e.cards.Concat(cards).ToList();
                foreach (CardStruct card in gameEventData.cards.Reverse())
                {
                    cards.Insert(0,MakeCard(card));
                }
                break;
            case GameEvent.ClearPile:
                // cards = cards.Except(e.cards).ToList(); 
                foreach (CardStruct card in gameEventData.cards)
                {
                    CardObject cardObject = cards.Find(c=>c.cardStruct==card);
                    cards.Remove(cardObject);
                    Destroy(cardObject.gameObject);
                }
                break;
            case GameEvent.Shanked:
                foreach (CardObject card in cards)
                {
                    Destroy(card.gameObject);
                }
                cards.Clear();
                break;
            default:
                return;
        }
    }

    private CardObject MakeCard(CardStruct card)
    {
        GameObject gameObject = Instantiate(cardPrefab,transform);
        gameObject.transform.SetSiblingIndex(transform.childCount-1);
        CardObject cardObject = gameObject.GetComponent<CardObject>();
        cardObject.setCardStruct(card);
        return cardObject;
    }

    private void Start()
    {
        ClientBehaviour.instance.onDataReceived += ReceiveData;
        // cardHands[cardHandIndex].StartTurn();
    }


    public bool ValidCard(CardValue card)
    {
        if(card==CardValue.Two||card==CardValue.Eight||card==CardValue.Ten||card==CardValue.Joker) return true;
        if(!cards.Any()) return true;
        CardValue comparisonCard = cards[0].cardStruct.value;
        for (int i = 0; comparisonCard==CardValue.Eight; i++)
        {
            if(i==cards.Count) return true;
            comparisonCard = cards[i].cardStruct.value;
        }
        if(comparisonCard==CardValue.Two||comparisonCard==CardValue.Three) return true;
        if(comparisonCard==CardValue.Seven)
        {
            return comparisonCard.IsGreaterThanOrEqualTo(card);
        }
        else
        {
            return card.IsGreaterThanOrEqualTo(comparisonCard);
        }
    }
   


}