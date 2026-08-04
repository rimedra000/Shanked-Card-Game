#if SERVER
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

public class ServerBehaviour : MonoBehaviour , ServerSender
{
    NetworkDriver m_Driver;
    NativeList<NetworkConnection> m_Connections;
    NetworkPipeline m_Pipeline;
    ServerSimulation serverSimulation;

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

        var endpoint = NetworkEndpoint.AnyIpv4.WithPort(GameManager.port);
        if (m_Driver.Bind(endpoint) != 0)
        {
            Debug.LogError("Failed to bind to port.");
            return;
        }
        m_Driver.Listen();

        serverSimulation=new(this);
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
            SceneManager.LoadScene(2);
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
                m_Connections.RemoveAtSwapBack(i);
                i--;
            }
        }
        // Accept new connections.
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default)
        {
            if(m_Connections.Length>=8) break;
            m_Connections.Add(c);
            Debug.Log($"client {m_Connections.Length-1} connected (s)");

            Data data = new Data(new DataHeader((byte)(m_Connections.Length-1),GameEvent.AddPlayer));

            SendData(data,m_Connections.Length-1);
            // serverSimulation.AddPlayer();

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
                    var data = new Data(nativebytes.ToArray());
                    nativebytes.Dispose();
                    serverSimulation.ReceiveData(data,i);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from the server.");
                    var data = new Data(new DataHeader((byte)i,GameEvent.RemovePlayer));
                    serverSimulation.ReceiveData(data,i);
                    m_Connections[i] = default;
                    break;
                }
            }
        }

    }

    public void SendData(Data data,int id)
    {
        // Debug.Log(data);
        m_Driver.BeginSend(m_Pipeline, m_Connections[id], out var writer);
        writer.WriteBytes(data.ToBytes());
        m_Driver.EndSend(writer);

    }
}

public interface ServerSender
{
    public void SendData(Data data,int id);
    public void EndGame();
    
}
#endif
