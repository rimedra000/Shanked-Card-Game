using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using System.Linq;
using System;


public class ClientBehaviour : MonoBehaviour
{
    NetworkDriver m_Driver;
    NetworkConnection m_Connection;

    NetworkPipeline m_Pipeline;

    public event EventHandler<Data> onDataReceived;

    private void Awake() {
        GameManager.clientBehaviour= this;
    }

    private void Start()
    {
        Debug.Log("Client start.");
        m_Driver = NetworkDriver.Create(new WebSocketNetworkInterface());
        m_Pipeline = m_Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        var endpoint = GameManager.networkEndpoint;
        m_Connection = m_Driver.Connect(endpoint);
    }

    void OnDestroy()
    {
        m_Driver.Dispose();
    }

    void Update()
    {
        
        
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server.");
                // CardValue[] cards = {CardValue.Ace,CardValue.Two,CardValue.Three,CardValue.Four,CardValue.Five,CardValue.Six,CardValue.Seven,CardValue.Eight,CardValue.Nine,CardValue.Ten,CardValue.Jack,CardValue.Queen,CardValue.King,CardValue.Joker};
                // var bytes =new NativeArray<byte>(cards.Select(c=>(byte)c).ToArray(),Allocator.Temp);
                // m_Driver.BeginSend(m_Pipeline,m_Connection, out var writer);
                // writer.WriteBytes(bytes);
                // m_Driver.EndSend(writer);
                // bytes.Dispose();
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                // while (stream.Length>stream.GetBytesRead())
                // {
                //     Debug.Log($"Got the value {(CardStruct)stream.ReadByte()} back from the server.");    
                // }

                var nativebytes = new NativeArray<byte>(stream.Length,Allocator.Temp);
                stream.ReadBytes(nativebytes);
                var bytes = nativebytes.ToArray();
                nativebytes.Dispose();
                var data = new Data(bytes);
                onDataReceived.Invoke(this,data);
                // Debug.Log(data);
                //m_Connection.Disconnect(m_Driver);
                //m_Connection = default;
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server.");
                m_Connection = default;
            }



        }
    }


    public void sendData(Data data)
    {
        var bytes =new NativeArray<byte>(data.ToBytes(),Allocator.Temp);
        m_Driver.BeginSend(m_Pipeline,m_Connection, out var writer);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
        bytes.Dispose();
    }
}
