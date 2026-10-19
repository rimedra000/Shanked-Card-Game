using System;
public interface IServerSimulator : IDisposable
{
    public bool ReceiveData(Data data,int id);
    public Data NewConnection(int id,byte[] bytes);
    public void PlayerDisconnect(int id);
    public bool GameStarted();
}
