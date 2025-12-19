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
    

    


   

    public void ReceiveCards(List<Card> cardsReceived)
    {
        
        foreach (Card card in cardsReceived)
        {
            if (cards.Contains(card)) continue;
            card.transform.SetParent(transform);
            card.transform.localPosition = Vector3.zero;
            cards.Add(card);
        }


    }
}
