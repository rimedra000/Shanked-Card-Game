using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private List<CardObject> handCards = new(3);
    private List<CardObject> hiddenCards = new (3);
    private List<CardObject> shownCards = new (3);

    private List<CardObject> selectedCards = new ();

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handCardsTransform;
    [SerializeField] private Transform shownCardsTransform;
    [SerializeField] private Transform hiddenCardsTransform;
    [SerializeField] private Button button;
    [SerializeField] private Button readyButton;
    [SerializeField] private PlayPile playPile;
    [SerializeField] private ClientBehaviour clientBehaviour;
    [SerializeField] private TextMeshProUGUI buttonText;

    private int playerID=-1;
    private bool turn=false;
    private bool isSetup=true;

    private bool isReady=false;
    private bool isShanked;



    private void Start()
    { 
        button.onClick.AddListener(PlayerConfirm);
        readyButton.onClick.AddListener(ReadyConfirm);
        clientBehaviour.onDataReceived += ReceiveData;
    }

    private void ReceiveData(object _, Data e)
    {
        if (playerID==-1)
        {
            Init(e);       
            return;
        }

        if (e.dataHeader.gameEvent==GameEvent.StartTurn)
        {

            if (turn&&e.dataHeader.player!=playerID)
            {
                turn=false;    //turn end
                EndTurn();
            }
       
            
            if(e.dataHeader.player!=playerID)
            {
                //others turns
            }


            if(isSetup)
            {
                SetupEnd();
                // return;
            }



        }

        if (e.dataHeader.player!=playerID) return;
        CardStruct[] cards = e.cards;
        switch (e.dataHeader.gameEvent)
        {
            case GameEvent.PlayCards:
                PlayCards(cards);
                break;
            case GameEvent.MoveShownCard:
                MoveShownCard(cards);
                break;
            case GameEvent.MoveHiddenCard:
                MoveHiddenCard(cards);
                break;
            case GameEvent.SwapHand:
                SwapHand(cards);
                break;
            case GameEvent.DrawCards:
                DrawCards(cards);
                break;
            case GameEvent.StartTurn:
                StartTurn();
                break;
            case GameEvent.DealHiddenCards:
                DealHiddenCards(cards);
                break;
            case GameEvent.DealShownCards:
                DealShownCards(cards);
                break;
            case GameEvent.DealHandCards:
                DealHandCards(cards);
                break;
            case GameEvent.SwapCards:
                SwapCards(cards);
                break;
            case GameEvent.Shanked:
                Shanked(cards);
                break;
            case GameEvent.RemovePlayer:
                RemovePlayer(cards);
                break;
        }
    }

    private void SetupEnd()
    {
        Destroy(readyButton.gameObject);
        isSetup = false;
        SetCardsInteract(shownCards,false);
        SetCardsInteract(handCards,false);
        button.interactable = false;
        DeSelectAll();
    }

    private void RemovePlayer(CardStruct[] cards)
    {
        for (int i = handCards.Count - 1; i >= 0; i--)
        {
            CardObject item = handCards[i];
            Destroy(item.gameObject);
        }
        for (int i = shownCards.Count - 1; i >= 0; i--)
        {
            CardObject item = shownCards[i];
            Destroy(item.gameObject);
        }
        for (int i = hiddenCards.Count - 1; i >= 0; i--)
        {
            CardObject item = hiddenCards[i];
            Destroy(item.gameObject);
        }
        handCards.Clear();
        shownCards.Clear();
        hiddenCards.Clear();
    }


    private void Init(Data e)
    {
        // Debug.Log(e.dataHeader.player);
        playerID=e.dataHeader.player;
        readyButton.interactable=true;
        SendData(new Data(new DataHeader((byte)playerID,GameEvent.AddPlayer),Array.Empty<CardStruct>()));
    }

    private void StartTurn()
    {
        turn=true;
        SetInteractablity();
    }

    private void EndTurn()
    {
        buttonText.text="";
        SetCardsInteract(handCards,false);
        SetCardsInteract(shownCards,false);
        SetCardsInteract(hiddenCards,false);
        button.interactable=false;
    }

    private void SetInteractablity()
    {
        bool emptyHandCards = handCards.Count <= 0;
        bool emptyShownCards = shownCards.Count <= 0;
        bool emptyHiddenCards = hiddenCards.Count <= 0;
        isShanked=false;
        button.interactable=false;
        
        if (!emptyHandCards)
        {
            buttonText.text="Play Cards";
            SetCardsInteract(hiddenCards,false);
            SetCardsInteract(shownCards,false);
            isShanked = true;
            foreach (CardObject card in handCards)
            {
                bool isValid = playPile.ValidCard(card.cardStruct.value);
                if (isValid) isShanked = false;
                card.button.interactable = isValid;
            }
            if(isShanked) 
            {   
                button.interactable=true;
                buttonText.text="Shanked";
            }
        }
        else if (!emptyShownCards)
        {
            buttonText.text="Move Cards";
            SetCardsInteract(hiddenCards,false);
            SetCardsInteract(shownCards,true);

        }
        else if (!emptyHiddenCards)
        {
            buttonText.text="Move Cards";
            SetCardsInteract(hiddenCards,true);
        }
    }

    private void Shanked(CardStruct[] cards)
    {
        foreach (CardStruct card in cards)
        {
            handCards.Add(MakeCard(card,handCardsTransform));
        }

    }

    private void SwapCards(CardStruct[] cards)
    {
        DeSelectAll();
        RemoveCard(cards[0],shownCards);
        shownCards.Add(MakeCard(cards[1],shownCardsTransform));
        RemoveCard(cards[1],handCards);
        handCards.Add(MakeCard(cards[0],handCardsTransform));
        SetCardsInteract(handCards,true);
        SetCardsInteract(shownCards,true);
        button.interactable=false;
    }

    private void DealHandCards(CardStruct[] cards)
    {
        foreach (CardStruct card in cards)
        {
            handCards.Add(MakeCard(card,handCardsTransform));
        }
    }

    private void DealShownCards(CardStruct[] cards)
    {
        foreach (CardStruct card in cards)
        {
            shownCards.Add(MakeCard(card,shownCardsTransform));
        }
    }

    private void DealHiddenCards(CardStruct[] cards)
    {
        foreach (CardStruct card in cards)
        {
            CardObject cardObject = MakeCard(card, hiddenCardsTransform);
            cardObject.setCardStructHidden(card);
            cardObject.button.interactable=false;
            hiddenCards.Add(cardObject);
        }

    }

    private void DrawCards(CardStruct[] cards)
    {
        button.interactable=false;
        
        foreach (CardStruct card in cards)
        {
            handCards.Add(MakeCard(card,handCardsTransform));
        }
    }

    private void SwapHand(CardStruct[] cards)
    {
        for (int i = handCardsTransform.childCount - 1; i >= 0 ; i--)
        {
            Destroy(handCardsTransform.GetChild(i).gameObject);
        }
        handCards.Clear();

        foreach (CardStruct card in cards)
        {
            handCards.Add(MakeCard(card,handCardsTransform));
        }
    }

    private void MoveHiddenCard(CardStruct[] cards)
    {
        DeSelectAll();
        CardStruct card = cards[0];
        RemoveCard(card,hiddenCards);
        handCards.Add(MakeCard(card,handCardsTransform));
        SetInteractablity();
    
    }

    private void MoveShownCard(CardStruct[] cards)
    {
        DeSelectAll();
        CardStruct card = cards[0];
        RemoveCard(card,shownCards);
        handCards.Add(MakeCard(card,handCardsTransform));
        SetInteractablity();
        
    }

    private void PlayCards(CardStruct[] cards)
    {
        DeSelectAll();
        foreach (CardStruct card in cards)
        {
            RemoveCard(card,handCards);
        }   
    }

    private CardObject MakeCard(CardStruct card,Transform parent)
    {
        GameObject gameObject = Instantiate(cardPrefab,parent);
        CardObject cardObject = gameObject.GetComponent<CardObject>();
        cardObject.setCardStruct(card);
        cardObject.onSelected +=ReceiveSelect;
        return cardObject;
    }

    private void ReceiveSelect(object sender, EventArgs e)
    {
        CardObject cardObject =  (CardObject)sender;
        if(isSetup)
        {
            if (!selectedCards.Contains(cardObject))
            {
                SelectCard(cardObject);
                if (handCards.Contains(cardObject))
                {
                    SetCardsInteract(handCards,false);
                }
                else if (shownCards.Contains(cardObject))
                {
                    SetCardsInteract(shownCards,false);

                }
                cardObject.button.interactable=true;
            }
            else
            {
                DeSelectCard(cardObject);
                if (handCards.Contains(cardObject))
                {
                    SetCardsInteract(handCards,true);
                }
                else if (shownCards.Contains(cardObject))
                {
                    SetCardsInteract(shownCards,true);
                }
                
            }

            button.interactable=selectedCards.Count==2;
        }
        else
        {
            
            if(handCards.Contains(cardObject))
            {
                
                if (selectedCards.Count<=0)
                {
                    SelectCard(cardObject);
                    foreach (CardObject card in handCards)
                    {
                        card.button.interactable=card.cardStruct.value==cardObject.cardStruct.value;
                    }
                    button.interactable=true;
                }
                else if(selectedCards.Contains(cardObject))
                {
                    if (selectedCards.Count==1)
                    {
                        foreach (CardObject card in handCards)
                        {
                            card.button.interactable=playPile.ValidCard(card.cardStruct.value);
                        }
                        button.interactable=false;
                    }
                    DeSelectCard(cardObject);
                    
                }
                else
                {
                    SelectCard(cardObject);
                }
            }
            else if (shownCards.Contains(cardObject))
            {
                if (selectedCards.Count<=0)
                {
                    SetCardsInteract(shownCards,false);
                    cardObject.button.interactable=true;
                    SelectCard(cardObject);
                    button.interactable=true;
                }
                else
                {
                    SetCardsInteract(shownCards,true);
                    DeSelectCard(cardObject);
                    button.interactable=false;
                }
            }
            else if (hiddenCards.Contains(cardObject))
            {
                if (selectedCards.Count<=0)
                {
                    SetCardsInteract(hiddenCards,false);
                    cardObject.button.interactable=true;
                    SelectCard(cardObject);
                    button.interactable=true;
                }
                else
                {
                    SetCardsInteract(hiddenCards,true);
                    DeSelectCard(cardObject);
                    button.interactable=false;
                }
            }
        }

    }

    private void DeSelectCard(CardObject card)
    {
        selectedCards.Remove(card);
        card.transform.Translate(Vector2.down * 5f);
    }

    private void SelectCard(CardObject card)
    {
        selectedCards.Add(card);
        card.transform.Translate(Vector2.up * 5f);
    }


    private void DeSelectAll()
    {
        for (int i = selectedCards.Count - 1; i >= 0 ; i--)
        {
            DeSelectCard(selectedCards[i]);
        }
    }

    private void RemoveCard(CardStruct card,List<CardObject> list)
    {
        var a = list.Find(c => c.cardStruct==card);
        Destroy(a.gameObject);
        list.Remove(a);
    }


    private void PlayerConfirm()
    {
        if(isSetup)
        {
            CardObject shownCard;
            CardObject handCard;
            if(shownCards.Contains(selectedCards[0]))
            {
                shownCard = selectedCards[0];
                handCard= selectedCards[1];
            }
            else
            {
                shownCard = selectedCards[1];
                handCard= selectedCards[0];
            }
            CardStruct[] cards = {shownCard.cardStruct,handCard.cardStruct};
            Data data = new Data(new DataHeader((byte)playerID,GameEvent.SwapCards),cards);
            SendData(data);
        }
        else
        {
            if (isShanked)
            {
                Data data = new Data(new DataHeader((byte)playerID,GameEvent.Shanked),Array.Empty<CardStruct>());
                SendData(data);
            }
            else if(handCards.Count>0)
            {
                CardStruct[] cards = selectedCards.Select(c=> c.cardStruct).ToArray();
                Data data = new Data(new DataHeader((byte)playerID,GameEvent.PlayCards),cards);
                SendData(data);
            }
            else if(shownCards.Count>0)
            {   
                Data data = new Data(new DataHeader((byte)playerID,GameEvent.MoveShownCard),new []{selectedCards[0].cardStruct});
                SendData(data);
            }
            else if(hiddenCards.Count>0)
            {
                Data data = new Data(new DataHeader((byte)playerID,GameEvent.MoveHiddenCard),new []{selectedCards[0].cardStruct});
                SendData(data);
            }
        }
    }

    private void ReadyConfirm()
    {
        if(!isReady)
        {
            isReady=true;
            DeSelectAll();
            SetCardsInteract(shownCards,false);
            SetCardsInteract(handCards,false);
        }
        else
        {
            isReady=false;
            SetCardsInteract(shownCards,true);
            SetCardsInteract(handCards,true);
        }

        Data data = new Data(new DataHeader((byte)playerID,GameEvent.Ready),Array.Empty<CardStruct>());
        SendData(data);
        

        
    }

    private void SendData(Data data)
    {
        clientBehaviour.sendData(data);
    }


    private void SetCardsInteract(List<CardObject> cards,bool interactable)
    {
        foreach (CardObject card in cards)
        {
            card.button.interactable = interactable;
        }
    }



}
