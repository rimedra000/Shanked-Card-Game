using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using System;


public class ClientBehaviour : MonoBehaviour
{
    public static ClientBehaviour instance;
    NetworkDriver m_Driver;
    NetworkConnection m_Connection;

    NetworkPipeline m_Pipeline;

    public event EventHandler<Data> onDataReceived;

    private void Awake() {
        // GameManager.clientBehaviour= this;
        instance=this;
    }

    private void Start()
    {
        Debug.Log("Client start.");
        m_Driver = NetworkDriver.Create(new WebSocketNetworkInterface());
        m_Pipeline = m_Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        var endpoint = ClientConfig.networkEndpoint;
        byte[] name = System.Text.Encoding.UTF8.GetBytes(ClientConfig.username);
        var bytes= new NativeArray<byte>(name,Allocator.Temp);
        m_Connection = m_Driver.Connect(endpoint,bytes);
        bytes.Dispose();
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
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                var nativebytes = new NativeArray<byte>(stream.Length,Allocator.Temp);
                stream.ReadBytes(nativebytes);
                var bytes = nativebytes.ToArray();
                nativebytes.Dispose();
                onDataReceived.Invoke(this,new Data(bytes));
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server.");
                m_Connection = default;
                m_Driver.Dispose();
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
    }


    public void sendData(Data data)
    {
        var bytes =new NativeArray<byte>(data.bytes,Allocator.Temp);
        m_Driver.BeginSend(m_Pipeline,m_Connection, out var writer);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
        bytes.Dispose();
    }
}