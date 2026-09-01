using UnityEngine;

public class PlayerCreator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject NetworkedPlayerPrefab;
    private int a=-1;
    void Start()
    {
        ClientBehaviour.instance.onDataReceived += ReceiveData;
    }

    private void ReceiveData(object _, Data data)
    {
        if(!data.isOtherEventData()) return;
        OtherEventData otherEventData=data.GetOtherEventData();
        if(a==-1){a=otherEventData.header.miscData;return;}
        if(a==otherEventData.header.miscData){return;}
        if (otherEventData.header.otherEvent!=OtherEvent.AddPlayer)
        {
            return;
        }

        NetworkedPlayer networkedPlayer = Instantiate(NetworkedPlayerPrefab, transform).GetComponent<NetworkedPlayer>();
        networkedPlayer.playerID=otherEventData.header.miscData;
        networkedPlayer.SetName(System.Text.Encoding.UTF8.GetString(otherEventData.miscBytes));
    }


}