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

    private void ReceiveData(object sender, Data data)
    {
        if(a==-1){a=data.dataHeader.player;return;}
        if(a==data.dataHeader.player){return;}
        if (data.dataHeader.gameEvent!=GameEvent.AddPlayer)
        {
            return;
        }

        NetworkedPlayer networkedPlayer = Instantiate(NetworkedPlayerPrefab, transform).GetComponent<NetworkedPlayer>();
        networkedPlayer.playerID=data.dataHeader.player;
        networkedPlayer.SetName(System.Text.Encoding.UTF8.GetString(data.otherBytes));
    }


}
