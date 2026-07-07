using UnityEngine;

public class CardObjectTester : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private CardObject cardObject;
    public CardValue cardValue;
    public CardSuit cardSuit;
    public CardDeck cardDeck;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        cardObject.setCardStruct(new CardStruct(cardValue,cardSuit,cardDeck));
    }
}
