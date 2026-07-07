using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClientSimulation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private ClientBehaviour clientBehaviour;
    private List<CardStruct> playPile = new();
    private Stack<CardStruct> drawPile = new();
    private Stack<CardStruct> discardPile = new();

    private byte turn=0;

    private byte selfID=0; //TODO: figure out which player client is

    private bool isSetupTime=true;

    private Player[] players=new Player[8];

    private class Player
    {
        public List<CardStruct> mainHand=new();
        public List<CardStruct> shownCards=new();
        public List<CardStruct> hiddenCards=new();
        public bool ready=false;
    }

    private void Start()
    {
        clientBehaviour.onDataReceived += ReceiveData;
    }

    private void ReceiveData(object _, Data data)
    {
        byte id = data.dataHeader.player;
        Player player = players[id];
        CardStruct[] cards = data.cards;
        switch (data.dataHeader.gameEvent)
        {
            case GameEvent.PlayCards:
                player.mainHand = player.mainHand.Except(cards).ToList();
                break;
            case GameEvent.MoveShownCard:
                player.shownCards.Remove(cards[0]);
                player.mainHand.Add(cards[0]);
                break;
            case GameEvent.MoveHiddenCard:
                player.hiddenCards.Remove(cards[0]);
                player.mainHand.Add(cards[0]);
                break;
            case GameEvent.SwapHand:
                player.mainHand=cards.ToList();
                break;
            case GameEvent.DrawCards:
                player.mainHand = player.mainHand.Concat(cards).ToList();
                break;
            case GameEvent.StartTurn:
                turn=id;
                break;
            case GameEvent.ClearPile:
                discardPile = (Stack<CardStruct>)cards.Concat(discardPile);
                break;
            case GameEvent.DealHiddenCards:
                player.hiddenCards=cards.ToList();
                break;
            case GameEvent.DealShownCards:
                player.shownCards=cards.ToList();
                break;
            case GameEvent.DealHandCards:
                player.mainHand=cards.ToList();
                break;
            case GameEvent.SwapCards:
                player.shownCards.Remove(cards[0]);
                player.shownCards.Add(cards[1]);
                player.mainHand.Remove(cards[1]);
                player.mainHand.Add(cards[0]);
                break;
            case GameEvent.Ready:
                player.ready=!player.ready;
                break;
            case GameEvent.Shanked:
                player.mainHand = player.mainHand.Concat(cards).ToList();
                playPile.Clear();
                break;
        }

    }
}
