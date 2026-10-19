// #define SHORT_CARDS
using System;
using System.Collections.Generic;
using System.Text;
public partial class ServerSimulation : IServerSimulator
{
    private IServerSender serverSender;
    private List<CardStruct> playPile = new();
    private Stack<CardStruct> drawPile = new();
    private Stack<CardStruct> discardPile = new();

    private byte turn = 0;
    private byte turnOffset = 0;
    private bool isSetupTime = true;
    private bool isWaitingForPlayers = true;
    private Player[] players = new Player[8];
    private class Player
    {
        public List<CardStruct> mainHand = new();
        public List<CardStruct> shownCards = new();
        public List<CardStruct> hiddenCards = new();
        public bool ready = false;
        public bool connected = false;
        public byte? connectionIndex;
        public string username = "";
        public BotHost botHost;

        public bool isPlaying()
        {
            // if (this == null) return false;
            if (!connected) return false;
            bool emptyMainHand = mainHand.Count <= 0;
            bool emptyShownCards = shownCards.Count <= 0;
            bool emptyHiddenCards = hiddenCards.Count <= 0;
            return !(emptyMainHand && emptyShownCards && emptyHiddenCards);
        }
    }

    private List<Data> pastData = new();

    private Queue<(GameEventData, int)> BotSentDataQueue = new();

    public ServerSimulation(IServerSender serverSender, Random random = null)
    {
        this.serverSender = serverSender;
        
#if !SHORT_CARDS
        CardStruct[] tempCards = new CardStruct[108];
        {
            int i = 0;

            // #if !SHORT_CARDS
            //create all cards
            foreach (CardDeck deck in Enum.GetValues(typeof(CardDeck)))
            {
                foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
                {
                    foreach (CardValue value in Enum.GetValues(typeof(CardValue)))
                    {
                        if (value == CardValue.Blank || (value == CardValue.Joker && (suit == CardSuit.Clubs || suit == CardSuit.Hearts))) continue;
                        tempCards[i] = new CardStruct(value, suit, deck);
                        i++;
                    }
                }
            }
        }
#else
        CardStruct[] tempCards=new byte[]{1,2,3,4,5,6,7,8,9,10,11,12,13,14}.ToCardStructArray();
#endif
        //shuffle all cards
        Random r = random ?? (ServerConfig.rngSeed.HasValue?new Random(ServerConfig.rngSeed.Value):new Random());
        for (int i = tempCards.Length - 1; i > 1; i--)
        {
            int randomIndex = r.Next(0, i);
            CardStruct tempCard = tempCards[randomIndex];
            tempCards[randomIndex] = tempCards[i];
            tempCards[i] = tempCard;
        }

        drawPile = new(tempCards);


    }

    private void BotSendDataIntercept(GameEventData data, int id)
    {
        bool success = BotReceiveData(data, id);
        if (!success)
        {
            RemovePlayer((byte)id);
        }
    }

    private bool AddPlayer(OtherEventData data, int connectionId)
    {
        byte id = data.miscData;
        foreach (Player p in players) if (p != null && p.connected && p.connectionIndex == connectionId) return false;
        if (players[id] != null) //rejoining player
        {
            Player reconnectedPlayer = players[id];
            if (reconnectedPlayer.connected == true) return false;
            else
            {
                reconnectedPlayer.connectionIndex = (byte)connectionId;
                reconnectedPlayer.connected = true;
                foreach (Data oldData in pastData)
                {
                    SendData(isPrivateData(oldData, id) ? CensorData(oldData) : oldData, id);
                }
                return true;
            }
        }
        if (!isSetupTime) return false;
        if (!trygetstringfrombytes(data.miscBytes, out var username)) return false;
        Player player = new Player
        {
            username = username,
            connectionIndex = (byte)connectionId,
            connected = true

        };
        players[id] = player;

        foreach (Data oldData in pastData)
        {
            SendData(isPrivateData(oldData, id) ? CensorData(oldData) : oldData, id);
        }

        SendToAll(data);

        DealHandCards(id);
        DealShownCards(id);
        DealHiddenCards(id);
        if (isWaitingForPlayers)
        {
            isWaitingForPlayers = false;
            initBots();
        }
        return true;
    }

    private void initBots()
    {
        foreach (string botPath in ServerConfig.botPaths)
        {
            var botIndexn = GetNextEmptyPlayerSlot();
            if (!botIndexn.HasValue) return;
            var botIndex = botIndexn.Value;
            try
            {
                AddBot(new SimpleBotHost(new MoonSharpBotHost(botPath), d => BotSentDataQueue.Enqueue((d, botIndex)), botIndex), botIndex);
            }
            catch (Exception e)
            {
                ServerConfig.DebugLog(e.ToString());
            }
        }

        byte? GetNextEmptyPlayerSlot()
        {
            for (byte i = 0; i < players.Length; i++) if (players[i] == null) return i;
            return null;
        }
    }
    private bool AddBot(BotHost botHost, byte id)
    {
        string username;
        try
        {
            username = botHost.GetUsername();
        }
        catch (Exception e)
        {
            ServerConfig.DebugLog(e.ToString());
            return false;
        }

        Player player = new Player
        {
            username = username,
            connected = true,
            botHost = botHost
        };
        players[id] = player;

        foreach (Data oldData in pastData)
        {
            SendData(isPrivateData(oldData, id) ? CensorData(oldData) : oldData, id);
        }
        var usernameBytes = Encoding.UTF8.GetBytes(username);
        SendToAll(new OtherEventData(id, OtherEvent.AddPlayer, usernameBytes));

        DealHandCards(id);
        DealShownCards(id);
        DealHiddenCards(id);
        return true;
    }

    bool trygetstringfrombytes(byte[] bytes, out string output)
    {
        var encoding = new UTF8Encoding(false, true);
        try
        {
            output = encoding.GetString(bytes);
        }
        catch (ArgumentException)
        {
            output = null;
            return false;
        }
        return true;
    }

    private bool isPrivateData(Data data, byte player)
    {
        if (data.isGameEventData())
        {
            GameEventData gameEventData = data.GetGameEventData();
            if (gameEventData.player == player) return false;
            return gameEventData.isPrivate();
        }
        else if (data.isOtherEventData())
        {
            OtherEventData otherEventData = data.GetOtherEventData();
            return otherEventData.isPrivate();
        }
        return false;
    }

    private void SendToAll(GameEventData data)
    {
        int id = data.player;
        SendData(data, id);

        GameEventData CensoredData = data.isPrivate() ? CensorData(data) : data;
        for (int i = 0; i < players.Length; i++)
        {
            if (i == id) continue;
            SendData(CensoredData, i);
        }
        pastData.Add(data);
    }
    private void SendToAll(OtherEventData data)
    {
        //TODO: check other event for who to send to
        for (int i = 0; i < players.Length; i++)
        {
            SendData(data, i);
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
        CardStruct[] censoredCards = new CardStruct[data.cards.Length];
        for (int i = 0; i < data.cards.Length; i++)
            censoredCards[i] = data.cards[i].Censored();
        return new GameEventData(data.player, data.gameEvent, censoredCards);
    }

    public bool ReceiveData(Data data, int connectionId)
    {
        bool result = false;
        if (data.isGameEventData())
        {
            GameEventData gameEventData = data.GetGameEventData();
            if (players[gameEventData.player].connectionIndex != connectionId) return false;
            result = ReceiveGameEventData(gameEventData);


            for (int i = 0; i < 1000 && BotSentDataQueue.Count > 0; i++)
            {
                var (a, b) = BotSentDataQueue.Dequeue();
                BotSendDataIntercept(a, b);
            }
            return result;
        }
        else if (data.isOtherEventData())
        {
            OtherEventData otherEventData = data.GetOtherEventData();
            OtherEvent otherEvent = otherEventData.otherEvent;
            if (otherEvent == OtherEvent.AddPlayer)
                return AddPlayer(otherEventData, connectionId);
            return false;
        }
        return false;
    }

    private bool BotReceiveData(GameEventData gameEventData, int playerIndex)
    {
        if (gameEventData.player == playerIndex) return ReceiveGameEventData(gameEventData);
        return false;
    }

    private bool ReceiveGameEventData(GameEventData gameEventData)
    {
        byte id = gameEventData.player;
        GameEvent gameEvent = gameEventData.gameEvent;

        if (isSetupTime)
        {
            switch (gameEvent)
            {
                case GameEvent.SwapCards:
                    return SwapCards(id, gameEventData.cards);
                case GameEvent.Ready:
                    return Ready(id);
                default:
                    return false;
            }
        }
        else if (id == turn)
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
        players[id] = null;
        CardStruct[] playerCards = new CardStruct[player.mainHand.Count + player.shownCards.Count + player.hiddenCards.Count];
        player.mainHand.CopyTo(playerCards);
        player.shownCards.CopyTo(playerCards, player.mainHand.Count);
        player.hiddenCards.CopyTo(playerCards, player.mainHand.Count + player.shownCards.Count);
        OtherEventData data = new OtherEventData(id, OtherEvent.RemovePlayer, playerCards.ToByteArray());
        SendToAll(data);
        return;
    }

    private bool ValidCard(CardValue card)
    {
        if (card == CardValue.Two || card == CardValue.Eight || card == CardValue.Ten || card == CardValue.Joker) return true;
        if (playPile.Count == 0) return true;
        CardValue comparisonCard = playPile[0].value;
        for (int i = 0; comparisonCard == CardValue.Eight; i++)
        {
            if (i == playPile.Count) return true;
            comparisonCard = playPile[i].value;
        }
        if (comparisonCard == CardValue.Two || comparisonCard == CardValue.Three) return true;
        if (comparisonCard == CardValue.Seven)
        {
            return comparisonCard.IsGreaterThanOrEqualTo(card);
        }
        else
        {
            return card.IsGreaterThanOrEqualTo(comparisonCard);
        }
    }

    private static bool isPlaying(Player player)
    {
        return player?.isPlaying() ?? false;
    }



    public Data NewConnection(int connectionId, byte[] bytes)
    {
        if (trygetstringfrombytes(bytes, out var username))
        {
            for (byte i = 0; i < players.Length; i++)
            {
                if (players[i] != null && !players[i].connected)
                {
                    if (players[i].username == username)
                        return new OtherEventData(i, OtherEvent.AddPlayer);
                }
            }
        }
        for (byte i = 0; i < players.Length; i++)
        {
            if (players[i] == null) return new OtherEventData(i, OtherEvent.AddPlayer);
        }
        for (byte i = 0; i < players.Length; i++)
        {
            if (players[i] != null && !players[i].connected)
            {
                RemovePlayer(i);
                return new OtherEventData(i, OtherEvent.AddPlayer);
            }
        }

        return new OtherEventData((byte)connectionId, OtherEvent.AddPlayer);
    }

    public void PlayerDisconnect(int id)
    {

        // if (players[id]==null) return;
        int playerIndex = -1;
        for (int i = 0; i < players.Length; i++)
        {
            Player p = players[i];
            if (p == null || !p.connected) continue;
            if (p.connectionIndex == id) { playerIndex = i; break; }
        }
        // if(playerIndex<0)return;
        Player player = players[playerIndex];

        Player maxConnectionPlayer = null;
        foreach (Player p in players)
        {
            if (p == null || !p.connected) continue;
            if (p.connectionIndex >= (maxConnectionPlayer?.connectionIndex ?? 0x00)) maxConnectionPlayer = p;
        }
        // if(maxConnectionPlayer==null)
        // {
        //     serverSender.EndGame();
        //     return;
        // }
        maxConnectionPlayer.connectionIndex = player.connectionIndex;
        player.connected = false;
        player.connectionIndex = null;
        if (turn == playerIndex && !isSetupTime) StartTurn();
        return;
    }

    public bool GameStarted() => !isSetupTime;

    public void SendData(Data data, int id)
    {
        Player player = players[id];
        if (player == null) return;
        if (!player.connected) return;
        if (player.connectionIndex.HasValue)
        {
            serverSender.SendData(data, player.connectionIndex.Value);
        }
        else if (player.botHost != null)
        {
            try
            {
                player.botHost.ReceiveData(data);
            }
            catch (Exception e)
            {
                // RemovePlayer(player);
                // if (e is MoonSharp.Interpreter.InterpreterException)
                // {
                //     MoonSharp.Interpreter.InterpreterException ie = (MoonSharp.Interpreter.InterpreterException)e;
                //     LogError(ie.DecoratedMessage);
                // }
                // else
                ServerConfig.DebugLog(e.ToString());


                RemovePlayer((byte)id);
            }

        }
    }

    // void Log(string s)
    // {
    //     serverSender.Log(s);
    // }

    public void Dispose()
    {
        for (int i = 0; i < players.Length; i++)
        {
            Player p = players[i];
            if(p==null)return;
            p.botHost?.Dispose();
        }
    }
}