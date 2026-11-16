using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Button playButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private TMP_InputField urlField;
    [SerializeField] private TMP_InputField nameField;

    private void Awake()
    {
        urlField.text=ClientConfig.ConnectionUrl;
        nameField.text=ClientConfig.username;
        #if !SERVER
        hostButton.gameObject.SetActive(false);
        #endif
    }


    public void SetUrl(string url)
    {
        ClientConfig.ConnectionUrl=url;
        // if(!NetworkEndpoint.TryParse(url,ClientConfig.networkEndpoint.Port,out NetworkEndpoint endpoint))
        // {
        //     playButton.interactable=false;
        // }
        // else
        // {
        //     ClientConfig.networkEndpoint=endpoint;
        //     playButton.interactable=true;
        // }
    }

    public void SetName(string name)
    {
        ClientConfig.username=name;
    }


#if SERVER
    public void Host()
    {
        // ClientConfig.networkEndpoint=NetworkEndpoint.LoopbackIpv4.WithPort(ClientConfig.networkEndpoint.Port);
        ClientConfig.ConnectionUrl="ws://localhost:31070";
        SceneManager.LoadScene(2);
    }
#endif

    public void Play()
    {
        SceneManager.LoadScene(1);
    }
}