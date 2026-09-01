#if SERVER
//#define SHORT_CARDS
using System;
using System.Collections.Generic;
using System.Linq;
public class ServerSimulation : ServerSimulator
{
    // private Action<Data,int> SendData;
    // private Action GameEnd;

    private ServerSender serverSender;

    private List<CardStruct> playPile = new();
    private Stack<CardStruct> drawPile = new();
    private Stack<CardStruct> discardPile = new();

    private byte turn=0;

    private byte turnOffset=0;

    private bool isSetupTime=true;

    private Player[] players =new Player[8];

    private List<Data> pastData =new();

    public ServerSimulation(ServerSender serverSender)
    {
        this.serverSender=serverSender;
        // this.SendData=SendData;
        // this.GameEnd=GameEnd;

        CardStruct[] tempCards=new CardStruct[
            #if !SHORT_CARDS
            108
            #else
            14
            #endif
            ];
        {
            int i=0;
            
            #if !SHORT_CARDS
            //create all cards
            foreach (CardDeck deck in Enum.GetValues(typeof(CardDeck)))
            {
                foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
                {
                    foreach (CardValue value in Enum.GetValues(typeof(CardValue)))
                    {
                        if(value == CardValue.Blank||(value==CardValue.Joker&&(suit==CardSuit.Clubs||suit==CardSuit.Hearts))) continue;
                        tempCards[i] = new CardStruct(value,suit,deck);
                        i++;
                    }
                }
            }
            #else
            foreach (CardValue value in Enum.GetValues(typeof(CardValue)))
            {
                if(value == CardValue.Blank) continue;
                tempCards[i] = new CardStruct(value,CardSuit.Spades,CardDeck.One);
                i++;
            }
            #endif
        }
        //shuffle all cards
        Random r = new Random();
        for(int i=tempCards.Length - 1; i > 1; i--)
        {
            int randomIndex=r.Next(0,i);
            CardStruct tempCard = tempCards[randomIndex];
            tempCards[randomIndex] = tempCards[i];
            tempCards[i] = tempCard;
        }

        foreach (CardStruct card in tempCards)
        {
            drawPile.Push(card);
        }
    }

    private class Player
    {
        public List<CardStruct> mainHand=new();
        public List<CardStruct> shownCards=new();
        public List<CardStruct> hiddenCards=new();
        public bool ready=false;
        public bool connected=true;

        public long connectionID =0;

        public int realId=-1;

    }

    private bool AddPlayer(OtherEventData data)
    {
        if(!isSetupTime) return false;
        
        byte id = data.header.miscData;

        if (players[id]!=null) return false;

        Player player = new Player();

        players[id] = player;



        foreach (Data oldData in pastData)
        {
            serverSender.SendData(isPrivateData(oldData)?CensorData(oldData):oldData,id);
        }
        // GameEventData newData=data;
        
        
        SendToAll(data);

        DealHandCards(id);
        DealShownCards(id);
        DealHiddenCards(id);
        return true;
    }

    private bool isPrivateData(Data data)
    {
        // dataEvent&=0x8F;
        if (data.isGameEventData())
        {
            GameEvent gameEvent= data.GetGameEventData().header.gameEvent;
            return isPrivateData(gameEvent);
        }
        else if(data.isOtherEventData())
        {
            OtherEvent otherEvent = data.GetOtherEventData().header.otherEvent;
            return isPrivateData(otherEvent);
        }
        return false;
    }

    private bool isPrivateData(GameEvent gameEvent)
    {
        switch (gameEvent)
            {
                case GameEvent.PlayCards:
                    return false;
                case GameEvent.MoveShownCard:
                    return false;
                case GameEvent.MoveHiddenCard:
                    return true;
                case GameEvent.SwapHand:
                    return true;
                case GameEvent.DrawCards:
                    return true;
                case GameEvent.StartTurn:
                    return false;
                case GameEvent.ClearPile:
                    return false;
                case GameEvent.DealHiddenCards:
                    return true;
                case GameEvent.DealShownCards:
                    return false;
                case GameEvent.DealHandCards:
                    return true;
                case GameEvent.SwapCards:
                    return false;
                case GameEvent.Ready:
                    return false;
                case GameEvent.Shanked:
                    return true;
                default:
                    return false;
            }
    }

    private bool isPrivateData(OtherEvent otherEvent)
    {
        switch (otherEvent)
        {
            case OtherEvent.AddPlayer:
                return false;
            case OtherEvent.RemovePlayer:
                return false;
            default:
                return false;
        }
    }

    private void SendToAll(GameEventData data)
    {
        int id = data.header.player;
        if(players[id]!=null)serverSender.SendData(data,id);

        GameEventData CensoredData = isPrivateData(data.header.gameEvent) ?CensorData(data):data;
        for (int i = 0; i < players.Length; i++)
        {
            if(i==id) continue;
            if(players[i]==null) continue;
            serverSender.SendData(CensoredData,i);
        }
        pastData.Add(data);
    }
    private void SendToAll(OtherEventData data)
    {
        //TODO: check other event for who to send to
        // int id = data.header.miscData;
        // if(players[id]!=null)serverSender.SendData(data.ToBytes(),id);

        // GameEventData CensoredData = isPrivateData(data.header.gameEvent) ?CensorData(data):data;
        for (int i = 0; i < players.Length; i++)
        {
            // if(i==id) continue;
            if(players[i]==null) continue;
            serverSender.SendData(data,i);
        }
        pastData.Add(data);
    }
    // private void SendToAll(byte[] data)
    // {
    //     if ((data[0]&0x80)==0) SendToAll(new GameEventData(data));
    //     else SendToAll(new OtherEventData(data));
    // }


    private Data CensorData(Data data)
    {
        if (data.isGameEventData())
        {
            return CensorData(data.GetGameEventData());
        }
        else
        {
            return data;
        }
    }
    private GameEventData CensorData(GameEventData data)
    {
        return new GameEventData(data.header,data.cards.Select(c=>c.Censored()).ToArray());
    }

    public bool ReceiveData(Data data,int trueId)
    {
        if (data.isGameEventData())
        {
            GameEventData gameEventData=data.GetGameEventData();
            byte id = gameEventData.header.player;
            GameEvent gameEvent = gameEventData.header.gameEvent;
            
            if (isSetupTime)
            {        
                switch (gameEvent)
                {
                    case GameEvent.SwapCards:
                        return SwapCards(id,gameEventData.cards);
                    case GameEvent.Ready:
                        return Ready(id);
                    default:
                        return false;
                }
            }
            else if(id==turn)
            {
                switch (gameEvent)
                {
                    case GameEvent.Shanked:
                        return Shanked();
                    case GameEvent.PlayCards:
                        return PlayCards(gameEventData.cards);
                    case GameEvent.MoveShownCard:
                        return MoveShownCard(gameEventData.cards[0]);
                    case GameEvent.MoveHiddenCard:
                        return MoveHiddenCard(gameEventData.cards[0]);
                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }  
        }
        else if(data.isOtherEventData())
        {
            OtherEventData otherEventData= data.GetOtherEventData();
            OtherEventDataHeader otherDataHeader=otherEventData.header;
            OtherEvent otherEvent = otherDataHeader.otherEvent;
            if(otherEvent==OtherEvent.RemovePlayer)
                return RemovePlayer(otherDataHeader.miscData);
            if(isSetupTime&&otherEvent==OtherEvent.AddPlayer)
                return AddPlayer(otherEventData);
            return false;
        }
        return false;
        
    }

    private bool RemovePlayer(byte id)
    {
        if (players[id]==null) return false;

        Player player = players[id];
        players[id]=null;
        CardStruct[] playerCards = player.mainHand.Concat(player.shownCards).Concat(player.hiddenCards).ToArray();
        OtherEventData data = new OtherEventData(new OtherEventDataHeader(id,OtherEvent.RemovePlayer),playerCards.ToByteArray());
        SendToAll(data);


        if(turn==id&&!isSetupTime) StartTurn();
        return true;
    }

    private bool ValidCard(CardValue card)
    {
        if(card==CardValue.Two||card==CardValue.Eight||card==CardValue.Ten||card==CardValue.Joker) return true;
        if(!playPile.Any()) return true;
        CardValue comparisonCard = playPile[0].value;
        for (int i = 0; comparisonCard==CardValue.Eight; i++)
        {
            if(i==playPile.Count) return true;
            comparisonCard = playPile[i].value;
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

    private bool Shanked()
    {
        Player player = players[turn];
        foreach (CardStruct card in player.mainHand)
        {
            if(ValidCard(card.value))return false;
        }

        GameEventData data = new GameEventData(new GameEventDataHeader(turn,GameEvent.Shanked),playPile.ToArray());
        SendToAll(data);

        player.mainHand = player.mainHand.Concat(playPile).ToList();
        playPile.Clear();
        StartTurn();
        return true;//for now
    }

    private bool PlayCards(CardStruct[] cards)
    {
        Player player = players[turn];
        CardValue cardValue = cards[0].value;
        foreach (CardStruct card in cards)
        {
            if(card.value!=cardValue) return false;
            if(!player.mainHand.Contains(card)) return false;
        }

        if(!ValidCard(cardValue))return false;

        playPile = cards.Concat(playPile).ToList();

        foreach (CardStruct card in cards)
        {
            player.mainHand.Remove(card);
        }


        GameEventData data = new GameEventData(new GameEventDataHeader(turn,GameEvent.PlayCards),cards);
        SendToAll(data);

        DrawCards();

        if(FourMatch()||cardValue==CardValue.Ten)
        {
            turnOffset=0;
            ClearPile();
        }
        else if(cardValue==CardValue.Jack)
        {
            turnOffset+=(byte)cards.Length;
        }
        else if (cardValue ==CardValue.Joker)
        {
            SwapHand(cards.Length);
            ClearCards(cards);
        }

        StartTurn();
        return true;

        bool FourMatch()
        {
            int count=0;
            int index=0;
            CardValue value=playPile[0].value;
            while (index<playPile.Count)
            {
                CardValue value1 = playPile[index].value;
                if (value1==value)
                {
                    count++;
                    index++;
                }
                else if(value1==CardValue.Eight)
                {
                    index++;
                }
                else
                {
                    return false;
                }

                if (count>=4)
                {
                    return true;
                }
            }
            return false;
        }
    }

    private bool MoveShownCard(CardStruct card)
    {
        Player player = players[turn];
        if (player.mainHand.Count>0) return false;
        if (!player.shownCards.Contains(card)) return false;

        GameEventData data = new GameEventData(new GameEventDataHeader(turn,GameEvent.MoveShownCard),new []{card});
        SendToAll(data);

        player.mainHand.Add(card);
        player.shownCards.Remove(card);
        return true;
    }

    private bool MoveHiddenCard(CardStruct card)
    {
        Player player = players[turn];
        if (player.mainHand.Count>0) return false;
        if (player.shownCards.Count>0) return false;
        if (!player.hiddenCards.Contains(card)) return false;

        GameEventData data = new GameEventData(new GameEventDataHeader(turn,GameEvent.MoveHiddenCard),new []{card});
        SendToAll(data);

        player.mainHand.Add(card);
        player.hiddenCards.Remove(card);
        return true;
    }

    private void SwapHand(int offset)
    {

        List<CardStruct>[] handsCopy = players.Select(p=>p?.mainHand).ToArray();

        for(int i=0;i<players.Length;i++)
        {
            Player player = players[i];
            if(!HasCards(player))continue;
            int otherIdx=i;

            int j=0;
            while(j<offset)
            {
                otherIdx--;
                otherIdx%=(byte)players.Length;
                if(otherIdx<0)otherIdx+=players.Length;
                if (HasCards(players[otherIdx]))
                {
                    j++;
                }
            }

            UnityEngine.Debug.Log($"i:{i} j:{j} offset:{offset} otheridx:{otherIdx}");

            players[i].mainHand=handsCopy[otherIdx];
            GameEventData data = new GameEventData(new GameEventDataHeader((byte)i,GameEvent.SwapHand),players[i].mainHand.ToArray());
            SendToAll(data);

        }


    }

    private void DrawCards()
    {
        Player player = players[turn];
        int cardsNeeded = Math.Min(3-player.mainHand.Count,drawPile.Count);
        if (cardsNeeded<=0) return;

        //TODO: send data
        CardStruct[] cards = new CardStruct[cardsNeeded];

        for(int i = 0;i<cardsNeeded;i++) cards[i] = drawPile.Pop();
        GameEventData data = new GameEventData(new GameEventDataHeader(turn,GameEvent.DrawCards),cards);
        SendToAll(data);
        for(int i = 0;i<cardsNeeded;i++) player.mainHand.Add(cards[i]);
        
    }

    private void StartTurn()
    {
        //TODO: account for finished players

        if(!players.Any(player => HasCards(player)&&(player?.connected??false)))
        {
            serverSender.EndGame();

            
            return;        
            //game done
        }
        
        while (turnOffset>0)
        {
            turn++;
            turn%=(byte)players.Length;
            if (HasCards(players[turn]))
            {
                turnOffset--;
            }
        }

        turnOffset=1;
        if(!players[turn].connected)
        {
            players[turn]=null;
            StartTurn();
            return;
        }


        SendToAll(new GameEventData(new GameEventDataHeader(turn,GameEvent.StartTurn)));

        

    }

    private static bool HasCards(Player player)
    {
        if(player==null) return false;
        bool emptyMainHand = player.mainHand.Count <= 0;
        bool emptyShownCards = player.shownCards.Count <= 0;
        bool emptyHiddenCards = player.hiddenCards.Count <= 0;
        return !(emptyMainHand && emptyShownCards && emptyHiddenCards);
    }

    private void ClearPile()
    {
        GameEventData data = new GameEventData(new GameEventDataHeader(turn,GameEvent.ClearPile),playPile.ToArray());        
        SendToAll(data);

        foreach (CardStruct card in playPile)
        {
            discardPile.Push(card);
        }

        playPile.Clear();
    }

    private void ClearCards(CardStruct[] cards)
    {
        GameEventData data = new GameEventData(new GameEventDataHeader(turn,GameEvent.ClearPile),cards);        
        SendToAll(data);

        foreach (CardStruct card in cards)
        {
            discardPile.Push(card);
            playPile.Remove(card);
        }
    }

    private void DealHiddenCards(byte id)
    {
        Player player = players[id];
        CardStruct[] cards = new CardStruct[3];
        for (int i = 0; i < 3; i++) cards[i] = drawPile.Pop();
        GameEventData data = new GameEventData(new GameEventDataHeader(id,GameEvent.DealHiddenCards),cards);
        SendToAll(data);
        for (int i = 0; i < 3; i++) player.hiddenCards.Add(cards[i]);
        
    }
    private void DealShownCards(byte id)
    {
        Player player = players[id];
        CardStruct[] cards = new CardStruct[3];
        for (int i = 0; i < 3; i++) cards[i] = drawPile.Pop();
        GameEventData data = new GameEventData(new GameEventDataHeader(id,GameEvent.DealShownCards),cards);
        SendToAll(data);
        for (int i = 0; i < 3; i++) player.shownCards.Add(cards[i]);
        
    }
    private void DealHandCards(byte id)
    {
        Player player = players[id];
        CardStruct[] cards = new CardStruct[3];
        for (int i = 0; i < 3; i++) cards[i] = drawPile.Pop();
        GameEventData data = new GameEventData(new GameEventDataHeader(id,GameEvent.DealHandCards),cards);
        SendToAll(data);
        for (int i = 0; i < 3; i++) player.mainHand.Add(cards[i]);
    }

    private bool SwapCards(byte id,CardStruct[] cards)
    {
        if (!isSetupTime) return false;
        Player player = players[id];

        if (!player.shownCards.Contains(cards[0]) || !player.mainHand.Contains(cards[1])) return false;

        GameEventData data = new GameEventData(new GameEventDataHeader(id,GameEvent.SwapCards),cards);        
        SendToAll(data);
        
        player.shownCards.Remove(cards[0]);
        player.shownCards.Add(cards[1]);
        player.mainHand.Remove(cards[1]);
        player.mainHand.Add(cards[0]);
        return true;
    }

    private bool Ready(byte id)
    {
        if(!isSetupTime) return false;

        GameEventData data = new GameEventData(new GameEventDataHeader(id,GameEvent.Ready));        
        SendToAll(data);

        Player player = players[id];
        player.ready=!player.ready;
        foreach (Player p in players)
        {
            if(p==null) continue;
            if(!p.ready) return true;
        }
        isSetupTime=false;
        StartTurn();
        return true;
    }

    public Data NewConnection(int id)
    {
        return new OtherEventData(new OtherEventDataHeader((byte)id,OtherEvent.AddPlayer));
        // throw new NotImplementedException();
    }

    public void PlayerDisconnect(int id)
    {
        // throw new NotImplementedException();
        RemovePlayer((byte)id);
    }

    public bool GameStarted()
    {
        return !isSetupTime;
    }
}

public interface ServerSimulator
{
    // public ServerSimulation(ServerSender serverSender);
    
    public bool ReceiveData(Data data,int id);

    public Data NewConnection(int id);

    public void PlayerDisconnect(int id);
    public bool GameStarted();

}

#endif
