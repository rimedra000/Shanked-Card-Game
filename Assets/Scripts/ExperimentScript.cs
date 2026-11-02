using UnityEngine;

public class ExperimentScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // var card = new CardStruct(CardValue.Ace,CardSuit.Spades,CardDeck.One);
        var cs = UnorderedCards.AllCards();
        Debug.Log(string.Join("},{",cs.ToCards()));
        // Debug.Log(cs.Read(card));
        // cs.Clear(card);
        // Debug.Log(cs.Read(card));
        // cs.Set(card);
        // Debug.Log(cs.Read(card));

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
