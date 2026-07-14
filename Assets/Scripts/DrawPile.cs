#if CLIENT
using UnityEngine;

public class DrawPile : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public int cardsRemaining = 108;
    [SerializeField] private GameObject cardPrefab;
    
    private void Start()
    {
        GameManager.clientBehaviour.onDataReceived += ReceiveData;
        for (int i = 0; i < 108; i++)
        {
            Instantiate(cardPrefab,transform);
        }
    }

    private void ReceiveData(object sender, Data data)
    {
        GameEvent gameEvent = data.dataHeader.gameEvent;
        if (gameEvent != GameEvent.DrawCards &&
            gameEvent != GameEvent.DealHandCards &&
            gameEvent != GameEvent.DealHiddenCards &&
            gameEvent != GameEvent.DealShownCards)
        {
            return;
        }

        cardsRemaining -= data.cards.Length;
        for (int i = transform.childCount-1; i >= cardsRemaining; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

}
#endif