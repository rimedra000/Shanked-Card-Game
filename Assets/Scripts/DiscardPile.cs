using System.Collections.Generic;
using UnityEngine;


public class DiscardPile : MonoBehaviour
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    [SerializeField] private List<Card> cards = new();
    public static DiscardPile instance;
    [SerializeField] private GameObject cardPrefab;

    private void Awake() {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void SelectCard(Card card)
    {}
    public void ReceiveCards(List<int> cardsReceived)
    {
        
        cards.InsertRange(0,Card.InstantiateCardsFromData(cardsReceived,transform,cardPrefab));


    }
}
