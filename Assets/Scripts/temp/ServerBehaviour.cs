using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Linq;
using System.Collections.Generic;
using NetworkData;

public class ServerBehaviour : MonoBehaviour
{

    NetworkDriver m_Driver;
    NativeList<NetworkConnection> m_Connections;
    NetworkPipeline m_Pipeline;

    ServerSimulation serverSimulation;

    List<Data> dataHistory=new();

    void Start()
    {
        Debug.Log("Server start.");
        m_Driver = NetworkDriver.Create(new WebSocketNetworkInterface());
        m_Pipeline = m_Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        m_Connections = new NativeList<NetworkConnection>(8, Allocator.Persistent);

        var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(7777);
        if (m_Driver.Bind(endpoint) != 0)
        {
            Debug.LogError("Failed to bind to port 7777.");
            return;
        }
        m_Driver.Listen();

        serverSimulation=new(sendData);
    }

    void OnDestroy()
    {
        if (m_Driver.IsCreated)
        {
            m_Driver.Dispose();
            m_Connections.Dispose();
        }
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();
        // Clean up connections.
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                i--;
            }
        }
        // Accept new connections.
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default)
        {
            m_Connections.Add(c);
            serverSimulation.AddPlayer();
            Debug.Log("Accepted a connection.");
        }

        for (int i = 0; i < m_Connections.Length; i++)
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    var nativebytes = new NativeArray<byte>(stream.Length,Allocator.Temp);
                    stream.ReadBytes(nativebytes);
                    var bytes = nativebytes.ToArray();
                    nativebytes.Dispose();
                    var data = new Data(bytes);
                    serverSimulation.ReceiveData(data);
                    // Debug.Log(data);
//                   var values =bytes.Select(e=>(CardValue)e).ToArray();
                    
// //                    Debug.Log($"Got {value} from a client, converting to card");

//                     //CardStruct card = new CardStruct(value, CardSuit.Hearts,CardDeck.One);
//                     var cards = values.Select(v=>new CardStruct(v,CardSuit.Hearts,CardDeck.One));
//                     var bytes2 = new NativeArray<byte>(cards.Select(c=>c.ToByte()).ToArray(),Allocator.Temp);
//                     m_Driver.BeginSend(m_Pipeline, m_Connections[i], out var writer);
//                     writer.WriteBytes(bytes2);
//                     m_Driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from the server.");
                    m_Connections[i] = default;
                    break;
                }
            }
        }

    }

    private void sendData(Data data,int id)
    {
        // Debug.Log(data);
        m_Driver.BeginSend(m_Pipeline, m_Connections[id], out var writer);
        writer.WriteBytes(data.ToBytes());
        m_Driver.EndSend(writer);

    }

    
}
