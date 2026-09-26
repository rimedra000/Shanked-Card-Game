// #define SHORT_CARDS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
public class ServerSimulation : ServerSimulator
{
    private ServerSender serverSender;

    private List<CardStruct> playPile = new();
    private Stack<CardStruct> drawPile = new();
    private Stack<CardStruct> discardPile = new();

    private byte turn=0;
    private byte turnOffset=0;
    private bool isSetupTime=true;
    private bool isWaitingForPlayers=true;
    private Player[] players =new Player[8];
    private class Player
    {
        public List<CardStruct> mainHand=new();
        public List<CardStruct> shownCards=new();
        public List<CardStruct> hiddenCards=new();
        public bool ready=false;
        public bool connected=false;
        public byte? connectionIndex;
        public string username="";
        // public int realId=-1;
        public BotHost botHost;
    }

    private List<Data> pastData =new();

    private Queue<(GameEventData,int)> BotSentDataQueue=new();

    public ServerSimulation(ServerSender serverSender,Random random=null)
    {
        this.serverSender=serverSender;
#if !SHORT_CARDS
        CardStruct[] tempCards=new CardStruct[108];
        {
            int i=0;
            
            // #if !SHORT_CARDS
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
        }   
#else
        CardStruct[] tempCards=new byte[]{1,2,3,4,5,6,7,8,9,10,11,12,13,14}.ToCardStructArray();
#endif
        //shuffle all cards
        Random r = random??new Random();
        for(int i=tempCards.Length - 1; i > 1; i--)
        {
            int randomIndex=r.Next(0,i);
            CardStruct tempCard = tempCards[randomIndex];
            tempCards[randomIndex] = tempCards[i];
            tempCards[i] = tempCard;
        }

        drawPile= new(tempCards);

        
    }

    private void BotSendDataIntercept(GameEventData data,int id)
    {
        bool success = BotReceiveData(data,id);
        if(!success)
        {
            RemovePlayer((byte)id);
        }
    }

    

    private bool AddPlayer(OtherEventData data,int connectionId)
    {
        byte id = data.miscData;
        foreach (Player p in players)if(p!=null&&p.connected&&p.connectionIndex==connectionId)return false;
        if (players[id] != null) //rejoining player
        {   
            Player reconnectedPlayer=players[id];
            if (reconnectedPlayer.connected == true) return false;
            else
            {
                reconnectedPlayer.connectionIndex= (byte)connectionId;
                reconnectedPlayer.connected=true;
                foreach (Data oldData in pastData)
                {
                    SendData(isPrivateData(oldData,id)?CensorData(oldData):oldData,id);
                }
                return true;
            }
        }
        if(!isSetupTime) return false;
        if(!trygetstringfrombytes(data.miscBytes,out var username))return false;
        Player player = new Player
        {
            username=username,
            connectionIndex = (byte)connectionId,
            connected=true
            
        };
        players[id] = player;

        foreach (Data oldData in pastData)
        {
            SendData(isPrivateData(oldData,id)?CensorData(oldData):oldData,id);
        }
        
        SendToAll(data);

        DealHandCards(id);
        DealShownCards(id);
        DealHiddenCards(id);
        if(isWaitingForPlayers)
        {
            isWaitingForPlayers=false;
            initBots();
        }
        return true;
    }

    private byte? GetNextEmptyPlayerSlot()
    {
        for (byte i = 0; i < players.Length; i++)if (players[i] == null) return i;
        return null;
    }

    private void initBots()
    {
        var botIndexa =GetNextEmptyPlayerSlot();
        if(!botIndexa.HasValue)return;
        var botIndex=botIndexa.Value;
        AddBot(new SimpleBotHost(new ExampleSimpleBot(),d=>BotSentDataQueue.Enqueue((d,botIndex)), botIndex), botIndex);

        // var botIndexa2 =GetNextEmptyPlayerSlot();
        // if(!botIndexa2.HasValue)return;
        // var botIndex2=botIndexa2.Value;
        // AddBot(new SimpleBotHost(new ExampleSimpleBot(),d=>BotSentDataQueue.Enqueue((d,botIndex2)), botIndex2), botIndex2);


        
    }
    private bool AddBot(BotHost botHost,byte id)
    {
        string username=botHost.GetUsername();
        Player player = new Player
        {
            username=username,
            connected=true,
            botHost=botHost
        };
        players[id] = player;

        foreach (Data oldData in pastData)
        {
            SendData(isPrivateData(oldData,id)?CensorData(oldData):oldData,id);
        }
        var usernameBytes= Encoding.UTF8.GetBytes(username);
        SendToAll(new OtherEventData(id,OtherEvent.AddPlayer,usernameBytes));

        DealHandCards(id);
        DealShownCards(id);
        DealHiddenCards(id);
        return true;
    }

    bool trygetstringfrombytes(byte[] bytes,out string output)
    {
        var encoding =new UTF8Encoding(false,true);
        try
        {
            output = encoding.GetString(bytes);
        }
        catch (ArgumentException)
        {
            output=null;
            return false;
        }
        return true;
    }

    private bool isPrivateData(Data data,byte player)
    {
        if (data.isGameEventData())
        {
            GameEventData gameEventData = data.GetGameEventData();
            if(gameEventData.player==player)return false;
            return isPrivateData(gameEventData.gameEvent);
        }
        else if(data.isOtherEventData())
        {
            OtherEvent otherEvent = data.GetOtherEventData().otherEvent;
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
        int id = data.player;
        // if(players[id]?.connected??false)
        SendData(data,id);

        GameEventData CensoredData = isPrivateData(data.gameEvent) ?CensorData(data):data;
        for (int i = 0; i < players.Length; i++)
        {
            if(i==id) continue;
            // Player player = players[i];
            // if (player==null) continue;
            // if(player.connected==false)continue;
            SendData(CensoredData,i);
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
            // Player player = players[i];
            // if (player==null) continue;
            // if(player.connected==false)continue;
            SendData(data,i);
        }
        pastData.Add(data);
    }

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
        return new GameEventData(data.player,data.gameEvent,data.cards.Select(c=>c.Censored()).ToArray());
    }

    public bool ReceiveData(Data data,int connectionId)
    {
        bool result = false; 
        if (data.isGameEventData())
        {
            GameEventData gameEventData=data.GetGameEventData();
            byte id = gameEventData.player;
            if(players[id].connectionIndex!=connectionId) return false;
            GameEvent gameEvent = gameEventData.gameEvent;
            
            if (isSetupTime)
            {        
                switch (gameEvent)
                {
                    case GameEvent.SwapCards:
                        result = SwapCards(id,gameEventData.cards);
                        break;
                    case GameEvent.Ready:
                        result = Ready(id);
                        break;
                    default:
                        return false;
                }
            }
            else if(id==turn)
            {
                switch (gameEvent)
                {
                    case GameEvent.Shanked:
                        result = Shanked();
                        break;
                    case GameEvent.PlayCards:
                        result = PlayCards(gameEventData.cards);
                        break;
                    case GameEvent.MoveShownCard:
                        result = MoveShownCard(gameEventData.cards[0]);
                        break;
                    case GameEvent.MoveHiddenCard:
                        result = MoveHiddenCard(gameEventData.cards[0]);
                        break;
                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }  
            for (int i=0;i<1000&&BotSentDataQueue.Count>0;i++)
            {
                var (a,b) = BotSentDataQueue.Dequeue();
                BotSendDataIntercept(a,b);
            }
            return result;
        }
        else if(data.isOtherEventData())
        {
            OtherEventData otherEventData= data.GetOtherEventData();
            OtherEvent otherEvent = otherEventData.otherEvent;
            if(otherEvent==OtherEvent.AddPlayer)
                return AddPlayer(otherEventData,connectionId);
            return false;
        }
        return false;
    }

    private bool BotReceiveData(GameEventData gameEventData,int playerIndex)
    {
        // GameEventData gameEventData=data.GetGameEventData();
        byte id = gameEventData.player;
        if(id!=playerIndex) return false;
        GameEvent gameEvent = gameEventData.gameEvent;
        
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

    private void RemovePlayer(byte id)
    {
        Player player = players[id];
        players[id]=null;
        CardStruct[] playerCards = player.mainHand.Concat(player.shownCards).Concat(player.hiddenCards).ToArray();
        OtherEventData data = new OtherEventData(id,OtherEvent.RemovePlayer,playerCards.ToByteArray());
        SendToAll(data);
        return;
    }

    private bool ValidCard(CardValue card)
    {
        if(card==CardValue.Two||card==CardValue.Eight||card==CardValue.Ten||card==CardValue.Joker) return true;
        if(playPile.Count==0) return true;
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
        if(player.mainHand.Count==0) return false;
        foreach (CardStruct card in player.mainHand)
        {
            if(ValidCard(card.value))return false;
        }

        GameEventData data = new GameEventData(turn,GameEvent.Shanked,playPile.ToArray());
        SendToAll(data);

        player.mainHand.AddRange(playPile);
        playPile.Clear();
        StartTurn();
        return true;//for now
    }

    private bool PlayCards(CardStruct[] cards)
    {
        Player player = players[turn];
        if(cards.Length==0)return false;
        CardValue cardValue = cards[0].value;
        if(cards.Any(card=> card.value!=cardValue||!player.mainHand.Contains(card)))return false;
        if(!ValidCard(cardValue))return false;

        playPile.InsertRange(0,cards);

        foreach (CardStruct card in cards)
        {
            player.mainHand.Remove(card);
        }

        GameEventData data = new GameEventData(turn,GameEvent.PlayCards,cards);
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

        GameEventData data = new GameEventData(turn,GameEvent.MoveShownCard,new []{card});
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

        GameEventData data = new GameEventData(turn,GameEvent.MoveHiddenCard,new []{card});
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
            // UnityEngine.Debug.Log($"i:{i} j:{j} offset:{offset} otheridx:{otherIdx}");
            players[i].mainHand=handsCopy[otherIdx];
            GameEventData data = new GameEventData((byte)i,GameEvent.SwapHand,players[i].mainHand.ToArray());
            SendToAll(data);
        }
    }

    private void DrawCards()
    {
        Player player = players[turn];
        int cardsNeeded = Math.Min(3-player.mainHand.Count,drawPile.Count);
        if (cardsNeeded<=0) return;

        CardStruct[] cards = drawPile.PopMany(cardsNeeded);
        GameEventData data = new GameEventData(turn,GameEvent.DrawCards,cards);
        SendToAll(data);
        player.mainHand.AddRange(cards);
    }

    private void StartTurn()
    {
        if(!players.Any(player => HasCards(player)))
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
        // if(!players[turn].connected)
        // {
        //     players[turn]=null;
        //     StartTurn();
        //     return;
        // }

        SendToAll(new GameEventData(turn,GameEvent.StartTurn));
    }

    private static bool HasCards(Player player)
    {
        if(player==null) return false;
        if(!player.connected) return false; 
        bool emptyMainHand = player.mainHand.Count <= 0;
        bool emptyShownCards = player.shownCards.Count <= 0;
        bool emptyHiddenCards = player.hiddenCards.Count <= 0;
        return !(emptyMainHand && emptyShownCards && emptyHiddenCards);
    }

    private void ClearPile()
    {
        GameEventData data = new GameEventData(turn,GameEvent.ClearPile,playPile.ToArray());        
        SendToAll(data);

        foreach (CardStruct card in playPile)
        {
            discardPile.Push(card);
        }

        playPile.Clear();
    }

    private void ClearCards(CardStruct[] cards)
    {
        GameEventData data = new GameEventData(turn,GameEvent.ClearPile,cards);        
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
        CardStruct[] cards = drawPile.PopMany(3);
        GameEventData data = new GameEventData(id,GameEvent.DealHiddenCards,cards);
        SendToAll(data);
        player.hiddenCards.AddRange(cards);
        
    }
    private void DealShownCards(byte id)
    {
        Player player = players[id];
        CardStruct[] cards = drawPile.PopMany(3);
        GameEventData data = new GameEventData(id,GameEvent.DealShownCards,cards);
        SendToAll(data);
        player.shownCards.AddRange(cards);
        
    }
    private void DealHandCards(byte id)
    {
        Player player = players[id];
        CardStruct[] cards = drawPile.PopMany(3);
        GameEventData data = new GameEventData(id,GameEvent.DealHandCards,cards);
        SendToAll(data);
        player.mainHand.AddRange(cards);
    }

    private bool SwapCards(byte id,CardStruct[] cards)
    {
        if (!isSetupTime) return false;
        Player player = players[id];

        if (!player.shownCards.Contains(cards[0]) || !player.mainHand.Contains(cards[1])) return false;

        GameEventData data = new GameEventData(id,GameEvent.SwapCards,cards);        
        SendToAll(data);
        player.shownCards.SwapItem(cards[0],cards[1]);
        player.mainHand.SwapItem(cards[1],cards[0]);
        return true;
    }

    private bool Ready(byte id)
    {
        if(!isSetupTime) return false;

        GameEventData data = new GameEventData(id,GameEvent.Ready);        
        SendToAll(data);

        Player player = players[id];
        player.ready=!player.ready;
        foreach (Player p in players)
        {
            if(p==null) continue;
            if(!p.connected)continue;
            if(!p.ready) return true;
        }
        isSetupTime=false;
        StartTurn();
        return true;
    }

    public Data NewConnection(int connectionId,byte[] bytes)
    {
        if(trygetstringfrombytes(bytes,out var username))
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i]!=null&&!players[i].connected)
                {
                    if (players[i].username == username)
                        return new OtherEventData((byte)i, OtherEvent.AddPlayer);
                }
            }
        }
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i]==null)return new OtherEventData((byte)i,OtherEvent.AddPlayer);
        }
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i]!=null&&!players[i].connected)
            {
                RemovePlayer((byte)i);
                return new OtherEventData((byte)i,OtherEvent.AddPlayer);
            }
        }

        return new OtherEventData((byte)connectionId,OtherEvent.AddPlayer);
    }

    public void PlayerDisconnect(int id)
    {
        
        // if (players[id]==null) return;
        int playerIndex=-1;
        for (int i = 0; i < players.Length; i++)
        {
            Player p = players[i];
            if(p==null||!p.connected) continue;
            if (p.connectionIndex==id){playerIndex=i;break;}
        }
        // if(playerIndex<0)return;
        Player player = players[playerIndex];
        
        Player maxConnectionPlayer=null;
        foreach (Player p in players)
        {
            if(p==null||!p.connected) continue;
            if(p.connectionIndex>=(maxConnectionPlayer?.connectionIndex??0x00))maxConnectionPlayer=p;
        }
        // if(maxConnectionPlayer==null)
        // {
        //     serverSender.EndGame();
        //     return;
        // }
        maxConnectionPlayer.connectionIndex=player.connectionIndex;
        player.connected=false;
        player.connectionIndex=null;
        if(turn==playerIndex&&!isSetupTime) StartTurn();
        return;
    }

    public bool GameStarted()
    {
        return !isSetupTime;
    }

    public void SendData(Data data,int id)
    {
        Player player = players[id];
        if(player==null) return;
        if(!player.connected) return;
        if(player.connectionIndex.HasValue)
        {
            serverSender.SendData(data, player.connectionIndex.Value);
        }
        else if(player.botHost!=null)
        {
            player.botHost.ReceiveData(data);
        }
    }
}

public interface ServerSimulator
{
    public bool ReceiveData(Data data,int id);
    public Data NewConnection(int id,byte[] bytes);
    public void PlayerDisconnect(int id);
    public bool GameStarted();
}

public static class MiscExtensions
{
    public static T[] PopMany<T>(this Stack<T> stack,int count)
    {
        T[] result = new T[count];
        for(int i=0;i<count;i++)result[i]=stack.Pop();
        return result;
    }

    public static void SwapItem<T>(this List<T> list,T item1,T item2)
    {
        list[list.IndexOf(item1)]=item2;
    }
}