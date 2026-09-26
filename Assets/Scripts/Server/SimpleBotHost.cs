using System;
using System.Linq;
using UnityEngine;

public class SimpleBotHost : BotHost
{
    ClientSimulation clientSimulation;
    Action<GameEventData> SendData;
    ClientState clientState;
    SimpleBot simpleBot;
    byte id;

    public SimpleBotHost(SimpleBot simpleBot,Action<GameEventData> SendData,byte id)
    {
        this.simpleBot=simpleBot;
        this.SendData=SendData;
        this.id=id;
        clientSimulation=new(id);
    }


    public void ReceiveData(Data data)
    {
        clientState = clientSimulation.ReceiveData(data);
        if(data.isGameEventData())
        {
            GameEventData gameEventData=data.GetGameEventData();
            if(gameEventData.player!=clientState.clientId)return;
            switch (gameEventData.gameEvent)
            {
                case GameEvent.StartTurn:
                    HandleStartTurn();
                    break;
                case GameEvent.MoveShownCard:
                case GameEvent.MoveHiddenCard:
                    HandleStartTurn();
                    break;
                case GameEvent.DealHiddenCards:
                    HandleSwapCards();
                    break;
                case GameEvent.SwapCards:
                    HandleSwapCards();
                    break;
            }
        }
        else if(data.isOtherEventData())
        {
            OtherEventData otherEventData=data.GetOtherEventData();
            if(otherEventData.miscData!=clientState.clientId)return;
        }

        void HandleStartTurn()
        {
            PlayerState selfPlayer = clientState.selfPlayer;
            if (selfPlayer.mainHand.Length>0)
            {
                CardStruct[] filteredCards = selfPlayer.mainHand.Where(c=>clientState.ValidCard(c)).ToArray();
                if (filteredCards.Length==0)
                {
                    SendData(new GameEventData(clientState.clientId,GameEvent.Shanked));
                }
                else if(filteredCards.Length==1)
                {
                    SendData(new GameEventData(clientState.clientId,GameEvent.PlayCards,filteredCards));
                }
                else
                {
                    CardStruct[] cards = simpleBot.PickFromFilteredMainHand(filteredCards, clientState);
                    SendData(new GameEventData(clientState.clientId,GameEvent.PlayCards,cards));
                }

            }
            else if(selfPlayer.shownCards.Length>0)
            {
                CardStruct card = simpleBot.PickFromShownCards(selfPlayer.shownCards, clientState);
                SendData(new GameEventData(clientState.clientId,GameEvent.MoveShownCard,new []{card}));
            }
            else if(selfPlayer.hiddenCards.Length>0)
            {
                CardStruct card = selfPlayer.hiddenCards[0];
                SendData(new GameEventData(clientState.clientId,GameEvent.MoveHiddenCard,new []{card}));
            }
        }


        void HandleSwapCards()
        {
            PlayerState selfPlayer = clientState.selfPlayer;
            var (mainHandCard,shownCard)=simpleBot.SetupShownCards(selfPlayer.mainHand,selfPlayer.shownCards);
            if(selfPlayer.shownCards.Contains(mainHandCard)&&selfPlayer.mainHand.Contains(shownCard))
            {
                SendData(new GameEventData(clientState.clientId,GameEvent.SwapCards,new []{mainHandCard,shownCard}));
            }
            else
            {
                SendData(new GameEventData(clientState.clientId,GameEvent.Ready));
            }
        }
    }

    public string GetUsername()
    {
        return simpleBot.GetUsername()+id;
    }
}


public interface SimpleBot
{
    /// <summary>
    /// If toMainHand is in shownCards and toShownCards is in mainHand,
    /// swap the two cards,
    /// otherwise set ready
    /// </summary>
    /// <returns>(CardStruct toMainHand, CardStruct toShownCards)</returns>
    public abstract (CardStruct,CardStruct) SetupShownCards(CardStruct[] mainHand, CardStruct[] shownCards);
    public abstract CardStruct[] PickFromFilteredMainHand(CardStruct[] filteredCards,ClientState clientState);
    public abstract CardStruct PickFromShownCards(CardStruct[] cards,ClientState clientState);
    public abstract string GetUsername();
}
