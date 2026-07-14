#if CLIENT
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Button playButton;
    [SerializeField] private Button hostButton;

    private void Awake()
    {
        Screen.autorotateToPortrait =false;
    }


    public void SetIP(string ip)
    {
        if(!NetworkEndpoint.TryParse(ip,GameManager.port,out NetworkEndpoint endpoint))
        {
            playButton.interactable=false;
        }
        else
        {
            GameManager.networkEndpoint=endpoint;
            playButton.interactable=true;
        }
    }

    public void SetName(string name)
    {
        GameManager.username=name;
    }


#if SERVER
    public void Host()
    {
        
        GameManager.networkEndpoint=NetworkEndpoint.LoopbackIpv4.WithPort(GameManager.port);
        SceneManager.LoadScene(2);
    }
#endif

    public void Play()
    {
        SceneManager.LoadScene(1);
    }
}
#endif