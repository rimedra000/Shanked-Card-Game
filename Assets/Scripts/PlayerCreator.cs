using System;
using UnityEngine;

public class PlayerCreator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject NetworkedPlayerPrefab;
    private int a=-1;
    void Start()
    {
        GameManager.clientBehaviour.onDataReceived += ReceiveData;
    }

    private void ReceiveData(object sender, Data e)
    {
        if(a==-1){a=e.dataHeader.player;return;}
        if(a==e.dataHeader.player){return;}
        if (e.dataHeader.gameEvent!=GameEvent.AddPlayer)
        {
            return;
        }

        NetworkedPlayer networkedPlayer = Instantiate(NetworkedPlayerPrefab, transform).GetComponent<NetworkedPlayer>();
        networkedPlayer.playerID=e.dataHeader.player;
    }


}
