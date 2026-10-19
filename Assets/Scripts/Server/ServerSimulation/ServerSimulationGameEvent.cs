using System;
using System.Collections.Generic;
public partial class ServerSimulation
{
    private bool PlayCards(CardStruct[] cards)
    {
        Player player = players[turn];
        if (cards.Length == 0) return false;
        CardValue cardValue = cards[0].value;
        if (!player.mainHand.Contains(cards[0])) return false;
        for (int i = 1; i < cards.Length; i++)
        {
            CardStruct card = cards[i];
            if (card.value != cardValue) return false;
            if (!player.mainHand.Contains(card)) return false;
        }
        //TODO: check if cards are duplicates
        if (!ValidCard(cardValue)) return false;

        playPile.InsertRange(0, cards);

        foreach (CardStruct card in cards)
        {
            player.mainHand.Remove(card);
        }

        GameEventData data = new GameEventData(turn, GameEvent.PlayCards, cards);
        SendToAll(data);

        DrawCards();

        if (FourMatch() || cardValue == CardValue.Ten)
        {
            turnOffset = 0;
            ClearCards(playPile.ToArray());
        }
        else if (cardValue == CardValue.Jack)
        {
            turnOffset += (byte)cards.Length;
        }
        else if (cardValue == CardValue.Joker)
        {
            SwapHand(cards.Length);
            ClearCards(cards);
        }

        StartTurn();
        return true;

        bool FourMatch()
        {
            int count = 0;
            int index = 0;
            CardValue value = playPile[0].value;
            while (index < playPile.Count)
            {
                CardValue value1 = playPile[index].value;
                if (value1 == value)
                {
                    count++;
                    index++;
                }
                else if (value1 == CardValue.Eight)
                {
                    index++;
                }
                else
                {
                    return false;
                }

                if (count >= 4)
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
        if (player.mainHand.Count > 0) return false;
        if (!player.shownCards.Contains(card)) return false;

        GameEventData data = new GameEventData(turn, GameEvent.MoveShownCard, new[] { card });
        SendToAll(data);

        player.mainHand.Add(card);
        player.shownCards.Remove(card);
        return true;
    }

    private bool MoveHiddenCard(CardStruct card)
    {
        Player player = players[turn];
        if (player.mainHand.Count > 0) return false;
        if (player.shownCards.Count > 0) return false;
        if (!player.hiddenCards.Contains(card)) return false;

        GameEventData data = new GameEventData(turn, GameEvent.MoveHiddenCard, new[] { card });
        SendToAll(data);

        player.mainHand.Add(card);
        player.hiddenCards.Remove(card);
        return true;
    }

    private void SwapHand(int offset)
    {
        List<CardStruct>[] handsCopy =new List<CardStruct>[8];
        for (int i = 0; i < players.Length; i++)
            handsCopy[i] = players[i]?.mainHand;

        for (int i = 0; i < players.Length; i++)
        {
            Player player = players[i];
            if (!isPlaying(player)) continue;
            int otherIdx = i;

            int j = 0;
            while (j < offset)
            {
                otherIdx--;
                otherIdx %= (byte)players.Length;
                if (otherIdx < 0) otherIdx += players.Length;
                if (isPlaying(players[otherIdx]))
                {
                    j++;
                }
            }
            // Log($"i:{i} j:{j} offset:{offset} otheridx:{otherIdx}");
            players[i].mainHand = handsCopy[otherIdx];
            GameEventData data = new GameEventData((byte)i, GameEvent.SwapHand, players[i].mainHand.ToArray());
            SendToAll(data);
        }
    }

    private void DrawCards()
    {
        Player player = players[turn];
        int cardsNeeded = Math.Min(3 - player.mainHand.Count, drawPile.Count);
        if (cardsNeeded <= 0) return;

        CardStruct[] cards = drawPile.PopMany(cardsNeeded);
        GameEventData data = new GameEventData(turn, GameEvent.DrawCards, cards);
        SendToAll(data);
        player.mainHand.AddRange(cards);
    }

    private void StartTurn()
    {
        if (isGameDone())
        {
            serverSender.EndGame();
            return;
            //game done
        }

        while (turnOffset > 0)
        {
            turn++;
            turn %= (byte)players.Length;
            if (isPlaying(players[turn]))
            {
                turnOffset--;
            }
        }

        turnOffset = 1;

        SendToAll(new GameEventData(turn, GameEvent.StartTurn));

        bool isGameDone()
        {
            foreach (Player player in players)
            {
                if(isPlaying(player))return false;
            }
            return true;
        }
    }

    private void ClearCards(CardStruct[] cards)
    {
        GameEventData data = new GameEventData(turn, GameEvent.ClearCards, cards);
        SendToAll(data);
        discardPile.PushMany(cards);
        playPile.RemoveRange(0, cards.Length);
    }

    private void DealHiddenCards(byte id)
    {
        Player player = players[id];
        CardStruct[] cards = drawPile.PopMany(3);
        GameEventData data = new GameEventData(id, GameEvent.DealHiddenCards, cards);
        SendToAll(data);
        player.hiddenCards.AddRange(cards);

    }
    private void DealShownCards(byte id)
    {
        Player player = players[id];
        CardStruct[] cards = drawPile.PopMany(3);
        GameEventData data = new GameEventData(id, GameEvent.DealShownCards, cards);
        SendToAll(data);
        player.shownCards.AddRange(cards);

    }
    private void DealHandCards(byte id)
    {
        Player player = players[id];
        CardStruct[] cards = drawPile.PopMany(3);
        GameEventData data = new GameEventData(id, GameEvent.DealHandCards, cards);
        SendToAll(data);
        player.mainHand.AddRange(cards);
    }

    private bool SwapCards(byte id, CardStruct[] cards)
    {
        if (!isSetupTime) return false;
        Player player = players[id];

        if (!player.shownCards.Contains(cards[0]) || !player.mainHand.Contains(cards[1])) return false;

        GameEventData data = new GameEventData(id, GameEvent.SwapCards, cards);
        SendToAll(data);
        player.shownCards[player.shownCards.IndexOf(cards[0])] = cards[1];
        player.mainHand[player.mainHand.IndexOf(cards[1])] = cards[0];
        return true;
    }

    private bool Ready(byte id)
    {
        if (!isSetupTime) return false;

        GameEventData data = new GameEventData(id, GameEvent.Ready);
        SendToAll(data);

        Player player = players[id];
        player.ready = !player.ready;
        foreach (Player p in players)
        {
            if (p == null) continue;
            if (!p.connected) continue;
            if (!p.ready) return true;
        }
        isSetupTime = false;
        StartTurn();
        return true;
    }

    private bool Shanked()
    {
        Player player = players[turn];
        if (player.mainHand.Count == 0) return false;
        foreach (CardStruct card in player.mainHand)
        {
            if (ValidCard(card.value)) return false;
        }

        GameEventData data = new GameEventData(turn, GameEvent.Shanked, playPile.ToArray());
        SendToAll(data);

        player.mainHand.AddRange(playPile);
        playPile.Clear();
        StartTurn();
        return true;//for now
    }
}
