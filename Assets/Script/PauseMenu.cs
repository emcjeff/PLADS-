using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance;

    [Header("UI Panels")]
    public GameObject pauseMenuUI;   // Ang Pause Menu Panel
    public GameObject optionsPanel;  // (Optional) Kung may options sa loob

    public static bool isGamePaused = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // Siguraduhing tuloy ang laro at nakatago ang menu sa simula
        Resume();
    }

    void Update()
    {
        // Pindutin ang Escape (ESC) para mag-Pause o mag-Resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // 1. RESUME
    public void Resume()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        Time.timeScale = 1f; // Tuloy ang oras
        isGamePaused = false;

        // I-lock at itago ulit ang mouse cursor sa laro
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 2. PAUSE
    public void Pause()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);

        Time.timeScale = 0f; // Freeze ang laro!
        isGamePaused = true;

        // Palitawin ang mouse cursor para makapag-click sa menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 3. RESTART MISSION
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 4. BUMALIK SA MAIN MENU
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // 5. QUIT DIRECTLY
    public void QuitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }
}