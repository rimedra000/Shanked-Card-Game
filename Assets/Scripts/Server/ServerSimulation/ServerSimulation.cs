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
    private bool botsInitialized = false;
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

    private Queue<GameEventData> SentGameEventDataQueue = new();
    const int MAX_DATA_PROCESS_LOOP_COUNT = 1000;

    public ServerSimulation(IServerSender serverSender, Random random = null)
    {
        this.serverSender = serverSender;

#if !SHORT_CARDS
        CardStruct[] tempCards = new CardStruct[108];
        {
            int i = 0;
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
        CardStruct[] tempCards =new byte[]
        {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E,
            0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E,
            0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 
            0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 
            0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
            0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E,
            0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 
            0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 
        }.ToCardStructArray();
#endif
        //shuffle all cards
        Random r = random ?? (ServerConfig.rngSeed.HasValue ? new Random(ServerConfig.rngSeed.Value) : new Random());
        for (int i = tempCards.Length - 1; i > 1; i--)
        {
            int randomIndex = r.Next(0, i);
            CardStruct tempCard = tempCards[randomIndex];
            tempCards[randomIndex] = tempCards[i];
            tempCards[i] = tempCard;
        }
        drawPile = new(tempCards);
    }

    private bool AddPlayer(OtherEventData data, byte connectionId)
    {
        byte id = data.miscData;
        foreach (Player p in players) if (p != null && p.connected && p.connectionIndex == connectionId) return false;
        if (players[id] != null) //rejoining player
        {
            Player reconnectedPlayer = players[id];
            if (reconnectedPlayer.connected == true) return false;
            else
            {
                reconnectedPlayer.connectionIndex = connectionId;
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
        AddPlayer(id, username, connectionId: connectionId);
        return true;
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
        AddPlayer(id, username, botHost: botHost);
        return true;
    }

    private void AddPlayer(byte id, string username, byte? connectionId = null, BotHost botHost = null)
    {
        Player player = new Player
        {
            username = username,
            connectionIndex = connectionId,
            connected = true,
            botHost = botHost,

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
        if (!botsInitialized)
        {
            botsInitialized = true;
            initBots();
        }
        return;
    }

    private void initBots()
    {
        Action<GameEventData,byte> SendDataCallback = (data, id) =>
        {
            if (data.player == id)
            {
                SentGameEventDataQueue.Enqueue(data);
            }
            else
            {
                RemovePlayer(id);
            }
        };

        foreach (string botPath in ServerConfig.botPaths)
        {
            byte? botIndexn = GetNextEmptyPlayerSlot();
            if (!botIndexn.HasValue) return;
            byte botIndex = botIndexn.Value;
            try
            {
                AddBot(new SimpleBotHost(new MoonSharpBotHost(botPath), d => SendDataCallback(d, botIndex), botIndex), botIndex);
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
        byte id = data.player;
        SendData(data, id);

        GameEventData CensoredData = data.isPrivate() ? CensorData(data) : data;
        for (byte i = 0; i < players.Length; i++)
        {
            if (i == id) continue;
            SendData(CensoredData, i);
        }
        pastData.Add(data);
    }
    private void SendToAll(OtherEventData data)
    {
        //TODO: check other event for who to send to
        for (byte i = 0; i < players.Length; i++)
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

    public void ReceiveData(Data data, byte connectionId)
    {
        if (data.isGameEventData())
        {
            GameEventData gameEventData = data.GetGameEventData();
            if (players[gameEventData.player].connectionIndex == connectionId)
            {
                SentGameEventDataQueue.Enqueue(gameEventData);
            }
        }
        else if (data.isOtherEventData())
        {
            OtherEventData otherEventData = data.GetOtherEventData();
            OtherEvent otherEvent = otherEventData.otherEvent;
            if (otherEvent == OtherEvent.AddPlayer)
                AddPlayer(otherEventData, connectionId);
        }
    }

    private bool ProcessGameEventData(GameEventData gameEventData)
    {
        byte id = gameEventData.player;
        GameEvent gameEvent = gameEventData.gameEvent;

        if (isSetupTime)
        {
            return gameEvent switch
            {
                GameEvent.SwapCards => SwapCards(id, gameEventData.cards),
                GameEvent.Ready => Ready(id),
                _ => false,
            };
        }
        else if (id == turn)
        {
            return gameEvent switch
            {
                GameEvent.Shanked => Shanked(),
                GameEvent.PlayCards => PlayCards(gameEventData.cards),
                GameEvent.MoveShownCard => MoveShownCard(gameEventData.cards[0]),
                GameEvent.MoveHiddenCard => MoveHiddenCard(gameEventData.cards[0]),
                _ => false,
            };
        }
        return false;
        
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



    public Data NewConnection(byte connectionId, byte[] bytes)
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

        return new OtherEventData(connectionId, OtherEvent.AddPlayer);
    }

    public void PlayerDisconnect(byte id)
    {
        int playerIndex = -1;
        for (int i = 0; i < players.Length; i++)
        {
            Player p = players[i];
            if (p == null || !p.connected) continue;
            if (p.connectionIndex == id) { playerIndex = i; break; }
        }
        Player player = players[playerIndex];

        Player maxConnectionPlayer = null;
        foreach (Player p in players)
        {
            if (p == null || !p.connected) continue;
            if (p.connectionIndex >= (maxConnectionPlayer?.connectionIndex ?? 0x00)) maxConnectionPlayer = p;
        }
        maxConnectionPlayer.connectionIndex = player.connectionIndex;
        player.connected = false;
        player.connectionIndex = null;
        if (turn == playerIndex && !isSetupTime) StartTurn();
        return;
    }

    public bool GameStarted() => !isSetupTime;

    public void SendData(Data data, byte id)
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
                ServerConfig.DebugLog(e.ToString());
                RemovePlayer(id);
            }

        }
    }


    public void Dispose()
    {
        foreach (Player p in players)
            p?.botHost?.Dispose();

    }

    public void Compute()
    {
        for (int i = 0; i < MAX_DATA_PROCESS_LOOP_COUNT && SentGameEventDataQueue.Count > 0; i++)
        {
            GameEventData data = SentGameEventDataQueue.Dequeue();
            bool result = ProcessGameEventData(data);
            if(result) continue;
            Player player = players[data.player];
            if (!result && player.botHost != null)
            {
                RemovePlayer(data.player);
            }

            
        }
    }
}