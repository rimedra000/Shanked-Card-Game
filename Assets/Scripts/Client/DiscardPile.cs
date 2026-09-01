#if CLIENT
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class DiscardPile : MonoBehaviour
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    private List<CardObject> cards = new();
    // public static DiscardPile instance;
    [SerializeField] private GameObject cardPrefab;

    private void Start() {
        // instance = this;
        GameManager.clientBehaviour.onDataReceived += ReceiveData;
    }

    private void ReceiveData(object _, Data data)
    {
        bool isClearPile=data.isGameEventData()&&data.GetGameEventData().header.gameEvent==GameEvent.ClearPile;
        bool isRemovePlayer=data.isOtherEventData()&&data.GetOtherEventData().header.otherEvent==OtherEvent.RemovePlayer;
        if (!(isClearPile||isRemovePlayer)) return;
        foreach (CardStruct card in data.bytes[1..].ToCardStructArray().Reverse())
        {
            cards.Insert(0,MakeCard(card));
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


    
    
    
}
#endif