using System;
public interface BotHost
{
    public abstract void ReceiveData(Data data);
    // public abstract void SetSendData(Action<Data> SendData);
    public abstract string GetUsername();
}
