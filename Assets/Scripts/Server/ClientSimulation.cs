using System.Collections.Generic;
public class ClientSimulation
{
    //TODO: fix a lot
    private List<CardStruct> playPile = new();
    private byte drawPileCount = 108;//((13*4)+2)*2
    private List<CardStruct> discardPile = new();
    private byte turn = 0;
    private byte clientId;
    private bool isSetupTime = true;
    private Player[] players=new Player[8];

    private class Player
    {
        public List<CardStruct> mainHand=new();
        public List<CardStruct> shownCards=new();
        public List<CardStruct> hiddenCards=new();
        public bool ready=false;
    }
    public ClientState ReceiveData(Data data)
    {
        if(clientId==0xff)clientId=data.GetOtherEventData().miscData;
        if(data.isGameEventData())
        {
            GameEventData gameEventData = data.GetGameEventData();
            
            byte id = gameEventData.player;
            Player player = players[id];
            CardStruct[] cards = gameEventData.cards;
            switch (gameEventData.gameEvent)
            {
                case GameEvent.PlayCards:
                    foreach (CardStruct card in cards)player.mainHand.Remove(card);
                    playPile.InsertRange(0,cards);
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
                    player.mainHand=new(cards);
                    break;
                case GameEvent.DrawCards:
                    player.mainHand.AddRange(cards);
                    drawPileCount-= (byte)cards.Length;
                    break;
                case GameEvent.StartTurn:
                    turn=id;
                    isSetupTime=false;
                    break;
                case GameEvent.ClearPile:
                    discardPile.InsertRange(0,cards);
                    if(cards.Length==playPile.Count)playPile.Clear();
                    else foreach (var card in cards)playPile.Remove(card);
                    break;
                case GameEvent.DealHiddenCards:
                    player.hiddenCards=new(cards);
                    break;
                case GameEvent.DealShownCards:
                    player.shownCards=new(cards);
                    break;
                case GameEvent.DealHandCards:
                    player.mainHand=new(cards);
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
                    player.mainHand.AddRange(cards);
                    playPile.Clear();
                    break;
            }
        }
        else if(data.isOtherEventData())
        {
            OtherEventData otherEventData = data.GetOtherEventData();

            switch (otherEventData.otherEvent)
            {
                case OtherEvent.AddPlayer:
                    players[otherEventData.miscData]=new();
                    break;
                case OtherEvent.RemovePlayer:
                    players[otherEventData.miscData]=null;
                    discardPile.InsertRange(0,otherEventData.miscBytes.ToCardStructArray());
                    break;
            }
        }
        return GenerateClientState();
    }
    public ClientSimulation(byte id)
    {
        clientId=id;
    }


    ClientState GenerateClientState()
    {
        PlayerState[] playerStates=new PlayerState[8];
        for (int i = 0; i < 8; i++)
        {
            Player p=players[i];
            if(p==null)continue;
            playerStates[i]=new(p.mainHand.ToArray(),p.shownCards.ToArray(),p.hiddenCards.ToArray(),p.ready);
        }
        return new ClientState(playerStates,!isSetupTime,playPile.ToArray(),discardPile.ToArray(),drawPileCount,turn,clientId);
    }
}


public readonly struct ClientState
{
    public readonly PlayerState[] players;
    public readonly bool isGameStarted;
    public readonly CardStruct[] playPile;
    public readonly CardStruct[] discardPile;
    public readonly byte drawCardsRemaining;
    public readonly byte turn;
    public readonly byte clientId;
    public readonly PlayerState selfPlayer => players[clientId];

    public ClientState(PlayerState[] players, bool isGameStarted, CardStruct[] playPile,
        CardStruct[] discardPile, byte drawCardsRemaining, byte turn, byte clientId)
    {
        this.players=players;
        this.isGameStarted=isGameStarted;
        this.playPile=playPile;
        this.discardPile=discardPile;
        this.drawCardsRemaining=drawCardsRemaining;
        this.turn=turn;
        this.clientId=clientId;
    }

    public bool ValidCard(CardValue card)
    {
        if(card==CardValue.Two||card==CardValue.Eight||card==CardValue.Ten||card==CardValue.Joker) return true;
        if(playPile.Length==0) return true;
        CardValue comparisonCard = playPile[0];
        for (int i = 0; comparisonCard==CardValue.Eight; i++)
        {
            if(i==playPile.Length) return true;
            comparisonCard = playPile[i];
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
public readonly struct PlayerState
{
    public readonly CardStruct[] mainHand;
    public readonly CardStruct[] shownCards;
    public readonly CardStruct[] hiddenCards;
    public readonly bool isReady;

    public PlayerState(CardStruct[] mainHand, CardStruct[] shownCards, CardStruct[] hiddenCards, bool isReady)
    {
        this.mainHand=mainHand;
        this.shownCards=shownCards;
        this.hiddenCards=hiddenCards;
        this.isReady=isReady;
    }

}