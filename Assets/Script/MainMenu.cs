using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;      // Yung may Play, Options, Quit
    public GameObject optionsPanel;   // Yung may Volume, Graphics, Back

    [Header("UI Controls")]
    public Slider volumeSlider;
    public Toggle fullscreenToggle;

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // I-set ang UI base sa kasalukuyang settings ng PC
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }

        // Tiyaking nakasara ang options sa simula
        CloseOptions();
    }

    // --- NAVIGATION ---
    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene");
    }

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

    public void QuitGame()
    {
        Debug.Log("Quit Game!");
        Application.Quit();
    }

    // --- SETTINGS CONTROLS ---

    // 1. Audio Volume (0.0 hanggang 1.0)
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    // 2. Fullscreen Toggle (Check / Uncheck)
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // 3. Graphics Quality (0: Low, 1: Medium, 2: High)
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
}