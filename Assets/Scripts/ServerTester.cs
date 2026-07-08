#if SERVER
using TMPro;
using UnityEngine;
using System;
using System.Globalization;

public class ServerTester : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // private ServerSimulation sim;
    [SerializeField] private TMP_InputField inputText;
    [SerializeField] private TMP_Text inputHint;
    // [SerializeField] private TMP_Text outputText;
    [SerializeField] private ClientBehaviour clientBehaviour;
    void Start()
    {
        // sim = new ServerSimulation(output);
        // sim.AddPlayer();
        clientBehaviour.onDataReceived += output;
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void send()
    {
        //int num = int.Parse(inputText.text, System.Globalization.NumberStyles.HexNumber);
        byte[] bytes = ConvertHexStringToByteArray(inputText.text);
        if (bytes.Length<=0||inputText.text.Length<=1) return;
        clientBehaviour.sendData(new Data(bytes));
        // sim.ReceiveData(new Data(bytes));
        // Debug.Log( new Data(bytes));
    }

    private void output(object _, Data data)
    {
        //if (id!=0) return;
        // outputText.text = data.ToString() + "\n" + outputText.text;
        Debug.Log(data);
    }

    public void updateHint(string hexString)
    {
        byte[] bytes = ConvertHexStringToByteArray(hexString);
        if (bytes.Length<=0||hexString.Length<=1) return;
        inputHint.text = new Data(bytes).ToString();
    }


    public static byte[] ConvertHexStringToByteArray(string hexString)
    {
        // if (hexString.Length % 2 != 0)
        // {
        //     throw new ArgumentException(string.Format("The binary key cannot have an odd number of digits: {0}", hexString));
        // }

        byte[] data = new byte[hexString.Length / 2];
        for (int index = 0; index < data.Length; index++)
        {
            string byteValue = hexString.Substring(index * 2, 2);
            data[index] = byte.Parse(byteValue, NumberStyles.HexNumber);
        }

        return data; 
    }




}
#endif