using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Image backgroundImage;
    void Awake()
    {
        backgroundImage=GetComponent<Image>();
    }

    // private void OnEnable() {
        
    // }

    private void Start()
    {
        
    }
    // Update is called once per frame
    // void Update()
    // {
        
    // }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
}
