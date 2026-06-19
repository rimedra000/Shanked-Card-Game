using System;
using System.Collections.Generic;
using System.Linq;
using NetworkData;
using UnityEngine;

public class ClientSimulation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private ClientBehaviour clientBehaviour;
    private List<CardStruct> playPile = new();
    private Stack<CardStruct> drawPile = new();
    private Stack<CardStruct> discardPile = new();

    private byte turn=0;

    private bool isSetupTime=true;

    private Player[] players;

    private class Player
    {
        public List<CardStruct> mainHand;
        public List<CardStruct> shownCards;
        public List<CardStruct> hiddenCards;
        public bool ready;

        public void init()
        {
            mainHand = new();
            shownCards = new();
            hiddenCards = new();
            ready= false;
        }
    }


    
}
