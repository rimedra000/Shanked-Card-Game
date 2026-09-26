using UnityEngine;

public class ExampleSimpleBot : SimpleBot
{
    public string GetUsername()
    {
        return nameof(ExampleSimpleBot);
    }

    public CardStruct[] PickFromFilteredMainHand(CardStruct[] filteredCards, ClientState clientState)
    {
        return new[]{filteredCards[0]};
    }

    public CardStruct PickFromShownCards(CardStruct[] cards, ClientState clientState)
    {
        return cards[0];
    }

    public (CardStruct, CardStruct) SetupShownCards(CardStruct[] mainHand, CardStruct[] shownCards)
    {
        return (mainHand[0],shownCards[0]);
    }
}
