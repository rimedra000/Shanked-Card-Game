using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Random=UnityEngine.Random;

public class DrawPile : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private List<Card> cards = new();
    public int cardsRemaining => cards.Count;
    [SerializeField] private GameObject cardPrefab;
    

    public static DrawPile instance;

    private void Awake() {
        instance = this;
        SetupPile();
        
    }

    public bool IsEmpty()
    {
        return cards.Count <=0;
    }

    public Card DrawCard()
    {
        
        Card card = cards[0];
        cards.Remove(card);
        return card;        
    }

    public void DrawCards(int num,CardHand cardHand)
    {
        if(num<=0||IsEmpty())return;
        List<Card> drawCards =new();
        for (int i = 0; i < num&&!IsEmpty(); i++)
        {
            drawCards.Add(DrawCard());
        }
        cardHand.ReceiveCards(drawCards);
    }

    private void SetupPile()
    {
        var a =new int[108];
        for (int i = 0; i < a.Length-1; i++)
        {
            a[i]=i;
        }
        a = Shuffle(a);
        cards=Card.InstantiateCardsFromData(a.ToList(),transform,cardPrefab);
        
    }

    private int[] Shuffle(int[] a)
    {
        for (int i = a.Length-1; i > 1; i--)
        {
            int b = Random.Range(0,i);
            var temp = a[b];
            a[b] = a[i];
            a[i] = temp;
        }
        return a;
    }





    
}
