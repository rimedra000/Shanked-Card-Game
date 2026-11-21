using UnityEngine;
using Unity.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Server;

public class WebSocketServerBehaviour : MonoBehaviour, IServerSender
{
    WebSocketServer wsServer;
    ServerSimulation serverSimulation;
    List<string> idList = new();

#if CLIENT
    private void Awake()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
    }

#endif

    private void Start()
    {
        serverSimulation = new(this);
        Debug.Log("Server start.");
        wsServer = new WebSocketServer(ServerConfig.HostUrl.GetLeftPart(System.UriPartial.Authority));
        wsServer.AddWebSocketService<CardGameService>(ServerConfig.HostUrl.AbsolutePath, s => { s.idList = idList; s.serverSimulation = serverSimulation; });
        wsServer.Start();
    }


    void OnDestroy()
    {
        wsServer.Stop();
    }

    void Update()
    {
        serverSimulation.Compute();
    }

    public void SendData(Data data, int id)
    {
        // Debug.Log($"sending({data}) to {id}({idList[id]})");
        wsServer.WebSocketServices[ServerConfig.HostUrl.AbsolutePath].Sessions.SendTo(data.bytes, idList[id]);
    }

    public void EndGame()
    {
        SceneManager.LoadScene(0);
    }
}

class CardGameService : WebSocketBehavior
{
    public IServerSimulator serverSimulation;
    public List<string> idList;

    private bool simConnected;

    protected override void OnMessage(MessageEventArgs e)
    {;
        if (simConnected)
        {
            serverSimulation.ReceiveData(new Data(e.RawData), (byte)idList.IndexOf(ID));
        }
        else
        {
            simConnected = true;
            Data? data = serverSimulation.NewConnection((byte)idList.IndexOf(ID), e.RawData);
            if (data.HasValue)
            {
                Send(data.Value.bytes);    
            }
            else
            {
                Close();
            }
            
        }
    }
    protected override void OnClose(CloseEventArgs e)
    {
        if (simConnected)
        {
            serverSimulation.PlayerDisconnect((byte)idList.IndexOf(ID));
        }
        idList.RemoveSwapBack(ID);
    }

    protected override void OnOpen()
    {
        idList.Add(ID);
    }
}