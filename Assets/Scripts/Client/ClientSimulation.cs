using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//TODO: fix a lot
public class ClientSimulation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private ClientBehaviour clientBehaviour;
    public List<CardStruct> playPile {get; private set;} = new();
    public Stack<CardStruct> drawPile {get; private set;} = new();
    public Stack<CardStruct> discardPile {get; private set;} = new();

    public byte turn {get; private set;} =0;

    public bool isSetupTime {get; private set;} = true;

    public Player[] players {get; private set;} = new Player[8];

    public class Player
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
        if(data.isGameEventData())
        {
            GameEventData gameEventData = data.GetGameEventData();
            
            byte id = gameEventData.header.player;
            Player player = players[id];
            CardStruct[] cards = gameEventData.cards;
            switch (gameEventData.header.gameEvent)
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
                    isSetupTime=false;
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
        //TODO: Handle otherEvents

    }
}