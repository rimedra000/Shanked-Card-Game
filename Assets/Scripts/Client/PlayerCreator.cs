using UnityEngine;

public class PlayerCreator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject NetworkedPlayerPrefab;
    private int selfPlayerId=-1;
    void Start()
    {
        ClientBehaviour.instance.onDataReceived += ReceiveData;
    }

    private void ReceiveData(object _, Data data)
    {
        if(!data.isOtherEventData()) return;
        OtherEventData otherEventData=data.GetOtherEventData();
        if(selfPlayerId==-1){selfPlayerId=otherEventData.miscData;return;}
        if(selfPlayerId==otherEventData.miscData) return;
        if(otherEventData.otherEvent!=OtherEvent.AddPlayer) return;
        
        NetworkedPlayer networkedPlayer = Instantiate(NetworkedPlayerPrefab, transform).GetComponent<NetworkedPlayer>();
        //TODO: sort players by turn order
        //TODO: show self's place in turn order
        // networkedPlayer.transform.SetSiblingIndex
        networkedPlayer.playerID=otherEventData.miscData;
        networkedPlayer.SetName(System.Text.Encoding.UTF8.GetString(otherEventData.miscBytes));
    }


}