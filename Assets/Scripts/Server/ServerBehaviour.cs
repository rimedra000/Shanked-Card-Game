using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport.Error;

public class ServerBehaviour : MonoBehaviour, ServerSender
{
    NetworkDriver m_Driver;
    NativeList<NetworkConnection> m_Connections;
    NetworkPipeline m_Pipeline;
    ServerSimulator serverSimulation;

#if CLIENT
    private void Awake()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
    }

#endif

    void Start()
    {
        Debug.Log("Server start.");
        m_Driver = NetworkDriver.Create(new WebSocketNetworkInterface());
        m_Pipeline = m_Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        m_Connections = new NativeList<NetworkConnection>(8, Allocator.Persistent);

        var endpoint = ServerConfig.networkEndpoint;
        if (m_Driver.Bind(endpoint) != 0)
        {
            Debug.LogError("Failed to bind to port.");
            return;
        }
        m_Driver.Listen();

        serverSimulation = new ServerSimulation(this);
    }

    public void EndGame()
    {
        StartCoroutine(nameof(GameEnd2));
        Debug.Log("game over");
    }
    private IEnumerator GameEnd2()
    {
        yield return new WaitForSeconds(3);
#if CLIENT
        SceneManager.LoadScene(0);
#else
            // Application.Quit();
            SceneManager.LoadScene(0);
#endif
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
                m_Connections.RemoveAtSwapBack(i);//TODO: fix possible desync
                dataQueueList.RemoveAtSwapBack(i);
                i--;
            }
        }
        // Accept new connections.
        NetworkConnection c;
        bool gameNotStarted = !serverSimulation.GameStarted();
        while (m_Connections.Length < 8 && (c = m_Driver.Accept(out NativeArray<byte> nativebytes)) != default)
        {
            // if(serverSimulation.GameStarted()) break;
            // if(m_Connections.Length>=8) break;

            m_Connections.Add(c);
            dataQueueList.Add(new Queue<Data>());

            int id = m_Connections.Length - 1;
            Debug.Log($"client {id} connected");

            byte[] bytes = nativebytes.ToArray();
            nativebytes.Dispose();

            // OtherEventData data = new OtherEventData((byte)id,OtherEvent.AddPlayer);
            Data data = serverSimulation.NewConnection(id, bytes);
            SendData(data, id);


            // Debug.Log("Accepted a connection.");
        }

        for (int i = 0; i < m_Connections.Length; i++)
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    var nativebytes = new NativeArray<byte>(stream.Length, Allocator.Temp);
                    stream.ReadBytes(nativebytes);
                    var data = nativebytes.ToArray();
                    nativebytes.Dispose();
                    serverSimulation.ReceiveData(new Data(data), i);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from the server.");
                    // var data = new OtherEventData(new OtherEventDataHeader((byte)i,OtherEvent.RemovePlayer));
                    serverSimulation.PlayerDisconnect(i);
                    m_Connections[i] = default;
                    break;
                }
            }
            sendDelayedData();
        }

    }
    List<Queue<Data>> dataQueueList = new(){};

    public void SendData(Data data, int id)
    {
        // Debug.Log(data);
        m_Driver.BeginSend(m_Pipeline, m_Connections[id], out var writer);
        writer.WriteBytes(data.bytes);
        var status = m_Driver.EndSend(writer);
        if (status == (int)StatusCode.NetworkSendQueueFull)
        {
            dataQueueList[id].Enqueue(data);
        }


    }
    private void sendDelayedData()
    {
        for (int id = 0; id < dataQueueList.Count; id++)
        {
            int status = 0;
            while (status >= 0&&dataQueueList[id].TryPeek(out var data))
            {
                m_Driver.BeginSend(m_Pipeline, m_Connections[id], out var writer);
                writer.WriteBytes(data.bytes);
                status = m_Driver.EndSend(writer);
                if(status>=0)dataQueueList[id].Dequeue();
            }
        }

    }
}

public interface ServerSender
{
    public void SendData(Data data, int id);
    public void EndGame();

}