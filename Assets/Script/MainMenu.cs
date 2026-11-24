using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names (Tiyaking Eksakto ang Spelling!)")]
    public string firstLevelSceneName = "level 1";   // Scene ng Level 1
    public string tutorialSceneName = "Tutorial";    // Scene ng Interactive Tutorial

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject optionsPanel;

    [Header("UI Controls")]
    public Slider volumeSlider;
    public Toggle fullscreenToggle;

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (volumeSlider != null) volumeSlider.value = AudioListener.volume;
        if (fullscreenToggle != null) fullscreenToggle.isOn = Screen.fullScreen;

        CloseOptions();
    }

    // --- BUTTON FUNCTIONS ---

    // 1. PLAY GAME (Diretso sa Level 1)
    public void PlayGame()
    {
        SceneManager.LoadScene(firstLevelSceneName);
    }

    // 2. TUTORIAL BUTTON (Papasok sa Interactive Tutorial Scene)
    public void PlayTutorial()
    {
        SceneManager.LoadScene(tutorialSceneName);
    }

    // 3. OPTIONS
    public void OpenOptions()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    // 4. EXIT
    public void QuitGame()
    {
        Debug.Log("Quit Game!");
        Application.Quit();
    }

    // --- SETTINGS CONTROLS ---
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
}