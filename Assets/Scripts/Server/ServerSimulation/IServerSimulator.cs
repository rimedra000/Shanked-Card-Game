using System;
public interface IServerSimulator : IDisposable
{
    public void ReceiveData(Data data,byte id);
    public Data NewConnection(byte id,byte[] bytes);
    public void PlayerDisconnect(byte id);

    public void Compute();
    public bool GameStarted();
}
