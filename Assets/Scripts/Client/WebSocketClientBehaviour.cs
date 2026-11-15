#if UNITY_WEBGL && !UNITY_EDITOR
#define WEB
#endif
#if !WEB
using UnityEngine;
using System;
using System.Collections.Generic;
using WebSocketSharp;


public class WebSocketClientBehaviour : MonoBehaviour
{
    public static WebSocketClientBehaviour instance;

    WebSocket wsClient;

    public event EventHandler<Data> onDataReceived;

    Queue<Data> dataQueue=new();

    private void Awake() {
        instance=this;
    }

    private void Start()
    {
        Debug.Log("client start");
        wsClient = new("ws://localhost:31070");
        wsClient.OnMessage += (_,e) => {dataQueue.Enqueue(new Data(e.RawData)); };
        wsClient.OnClose += (_,_) => {UnityEngine.SceneManagement.SceneManager.LoadScene(0);};
        byte[] name = System.Text.Encoding.UTF8.GetBytes(ClientConfig.username);
        wsClient.Connect();
        wsClient.Send(name);
    }

    void OnDestroy()
    {
        wsClient.Close();
    }

    void Update()
    {
        while(dataQueue.Count>0)
        {
            onDataReceived.Invoke(this,dataQueue.Dequeue());
        }
    }


    public void sendData(Data data)
    {
        wsClient.Send(data.bytes);
    }
}

#else
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using AOT;

public class WebSocketClientBehaviour : MonoBehaviour
{
    public static WebSocketClientBehaviour instance;
    public event EventHandler<Data> onDataReceived;

    [DllImport("__Internal")]
    private static extern void OpenWebSocket(string str,Action<byte[], int> messageCallback,Action openCallback);

    [DllImport("__Internal")]
    private static extern void CloseWebSocket();

    [DllImport("__Internal")]
    private static extern void WebSocketSend(byte[] array, int size);

    void Start() {
        OpenWebSocket("ws://localhost:31070",JSMessageCallBack,JSOpenCallback);
    }

    void OnDestroy()
    {
        CloseWebSocket();
    }


    private void Awake() {
        instance=this;
    }
    [MonoPInvokeCallback(typeof(Action<byte[],int>))]
    public static void JSMessageCallBack([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 1)] byte[] bytes, int bytesLength)
    {
        // Debug.Log(str);
        instance.onDataReceived.Invoke(instance,new Data(bytes));
    }
    [MonoPInvokeCallback(typeof(Action))]
    public static void JSOpenCallback()
    {
        byte[] name = System.Text.Encoding.UTF8.GetBytes(ClientConfig.username);
        WebSocketSend(name,name.Length);
    }

    public void sendData(Data data)
    {
        WebSocketSend(data.bytes,data.bytes.Length);
        // wsClient.Send(data.bytes);
    }
}
#endif