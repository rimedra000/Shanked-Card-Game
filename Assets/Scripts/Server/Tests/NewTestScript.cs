using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
// using UnityEngine.TestTools;

public class NewTestScript
{
    // A Test behaves as an ordinary method
    [Test]
    public void NewTestScriptSimplePasses()
    {
        // Use the Assert class to test conditions
        List<Data>[] a={new()};
        var sender = new testServerSender(()=>{},(d,i)=>{a[i].Add(d);});
        var sim = new ServerSimulation(sender,new System.Random(0));
        sim.NewConnection(0,new byte[]{65});
        sim.ReceiveData(new OtherEventData(0,OtherEvent.AddPlayer,new byte[]{65}),0);
        Debug.Log("{"+string.Join("},{",a[0])+"}");
    }   

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    // [UnityTest]
    // public IEnumerator NewTestScriptWithEnumeratorPasses()
    // {
    //     // Use the Assert class to test conditions.
    //     // Use yield to skip a frame.
    //     yield return null;
    // }
    class testServerSender : ServerSender
    {
        private Action endGame;
        private Action<Data,int> sendData;
        public testServerSender(Action endGame,Action<Data,int> sendData)
        {
            this.endGame=endGame;
            this.sendData=sendData;
        }
        public void EndGame()
        {
            endGame();
        }

        public void SendData(Data data, int id)
        {
            sendData(data,id);
        }
    }
}
