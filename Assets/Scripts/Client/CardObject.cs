using System;
using UnityEngine;
using UnityEngine.UI;

public class CardObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    
    [SerializeField] private Image image;
    [SerializeField] public Button button;
    public event EventHandler onSelected;

    public CardStruct cardStruct {get;private set;}


    void Awake()
    {
        if(button) button.onClick.AddListener(OnClick);
        if (!image&&!TryGetComponent(out image))
        {
            Debug.Log("Card has no image",this);
        }
    }

    private void OnClick()
    {   
        // throw new NotImplementedException();
        onSelected.Invoke(this,null);
        
    }

    public void setCardStruct(CardStruct card)
    {
        image.sprite=ClientConfig.getSpriteFromCardStruct(card);
        cardStruct=card;
    }

    public void setCardStructHidden(CardStruct card)
    {
        image.sprite=ClientConfig.getSpriteFromCardStruct(card.Censored());
        cardStruct=card;
    }


}