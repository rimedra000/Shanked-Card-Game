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

    // [SerializeField] private TMP_InputField nameText;
    private void Awake() {
        #if !SERVER
        hostButton.gameObject.SetActive(false);
        #endif    
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    public void SetIP(string ip)
    {
        if(!NetworkEndpoint.TryParse(ip,7777,out NetworkEndpoint endpoint))
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
        
        GameManager.networkEndpoint=NetworkEndpoint.LoopbackIpv4.WithPort(7777);
        SceneManager.LoadScene(2);
    }
#endif

    public void Play()
    {
        SceneManager.LoadScene(1);
    }
}
