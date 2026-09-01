using TMPro;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Button playButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private TMP_InputField ipField;
    [SerializeField] private TMP_InputField nameField;

    private void Awake()
    {
        ipField.text=ClientConfig.networkEndpoint.Address.Split(":")[0];
        nameField.text=ClientConfig.username;
        #if !SERVER
        hostButton.gameObject.SetActive(false);
        #endif
    }


    public void SetIP(string ip)
    {
        if(!NetworkEndpoint.TryParse(ip,ClientConfig.networkEndpoint.Port,out NetworkEndpoint endpoint))
        {
            playButton.interactable=false;
        }
        else
        {
            ClientConfig.networkEndpoint=endpoint;
            playButton.interactable=true;
        }
    }

    public void SetName(string name)
    {
        ClientConfig.username=name;
    }


#if SERVER
    public void Host()
    {
        ClientConfig.networkEndpoint=NetworkEndpoint.LoopbackIpv4.WithPort(ClientConfig.networkEndpoint.Port);
        SceneManager.LoadScene(2);
    }
#endif

    public void Play()
    {
        SceneManager.LoadScene(1);
    }
}