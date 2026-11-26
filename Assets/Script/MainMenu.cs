using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names (Tiyaking Eksakto ang Spelling)")]
    public string firstLevelSceneName = "level 1";
    public string tutorialSceneName = "Tutorial";

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject optionsPanel;
    public GameObject howToPlayPanel; // I-drag ang "OptionsPanel (1)" dito!

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

        // Main menu lang ang nakabukas sa simula
        ShowMainPanel();
    }

    // --- NAVIGATION ---

    public void PlayGame()
    {
        SceneManager.LoadScene(firstLevelSceneName);
    }

    public void OpenHowToPlay()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
    }

    public void PlayTutorial()
    {
        SceneManager.LoadScene(tutorialSceneName);
    }

    public void OpenOptions()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void ShowMainPanel()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        ShowMainPanel();
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game!");
        Application.Quit();
    }

    // --- SETTINGS ---
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