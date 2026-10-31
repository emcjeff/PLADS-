using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio; 

public class SettingsMenuManager : MonoBehaviour
{
    public AudioMixer audioMixer; 

    public void OpenSettingScene()
    {
        SceneManager.LoadScene("SettingScene"); 
    }

    
    public void SetVolume(float volume)
    {
      
        Debug.Log("Volume set to: " + volume);
    }

    
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Main Menu Scene"); 
    }
}