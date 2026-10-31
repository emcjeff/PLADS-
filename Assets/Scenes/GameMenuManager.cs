using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuManager : MonoBehaviour
{
    
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Main Menu Scene");
    }
}