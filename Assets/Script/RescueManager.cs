using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RescueManager : MonoBehaviour
{
    public static RescueManager instance;

    [Header("Mission Status")]
    public bool isMissionStarted = false;
    public int totalSurvivors = 3;           // Default bilang kung ilan ang i-spawn
    public int rescuedOnBoat = 0;
    public int deliveredToSafeZone = 0;

    [Header("Mission Elements")]
    public GameObject floodWaterObject;
    public GameObject safeZoneObject;
    public GameObject missionStartBox;

    [Header("In-Game HUD")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI countdownText;

    [Header("Win UI Screen")]
    public GameObject winScreenUI;
    public TextMeshProUGUI winSummaryText;
    public string nextLevelSceneName = "";

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        if (floodWaterObject != null) floodWaterObject.SetActive(false);
        if (safeZoneObject != null) safeZoneObject.SetActive(false);
        if (missionStartBox != null) missionStartBox.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (statusText != null) statusText.gameObject.SetActive(false);
        if (winScreenUI != null) winScreenUI.SetActive(false);

        // Kuhanin ang totoong bilang kung ilang NPC ang nakaset sa spawner
        if (NPCSpawner.instance != null)
        {
            totalSurvivors = NPCSpawner.instance.totalNpcToSpawn;
        }
    }

    public void TriggerMissionStart()
    {
        if (isMissionStarted) return;

        if (floodWaterObject != null) floodWaterObject.SetActive(true);
        if (safeZoneObject != null) safeZoneObject.SetActive(true);

        // 1. I-spawn ang mga tao
        if (NPCSpawner.instance != null)
        {
            NPCSpawner.instance.SpawnRandomNPCs();
            totalSurvivors = NPCSpawner.instance.totalNpcToSpawn;
        }

        // 2. Ipakita ang Mission Box
        if (missionStartBox != null)
        {
            missionStartBox.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Tatawagin pagkatapos mag-spawn
    public void InitializeMissionStats()
    {
        DrowningNPC[] npcs = Object.FindObjectsByType<DrowningNPC>(FindObjectsSortMode.None);
        if (npcs.Length > 0)
        {
            totalSurvivors = npcs.Length;
        }
        UpdateUI();
    }

    public void OnClickConfirmMission()
    {
        isMissionStarted = true;

        if (missionStartBox != null) missionStartBox.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            UpdateUI();
        }
    }

    public void UpdateRescueTimer(float timeRemaining)
    {
        if (!isMissionStarted) return;

        if (countdownText != null)
        {
            if (!countdownText.gameObject.activeSelf) countdownText.gameObject.SetActive(true);

            int seconds = Mathf.CeilToInt(timeRemaining);
            if (timeRemaining <= 5f)
            {
                countdownText.text = $"RESCUE QUICK: {seconds}s";
                countdownText.color = Color.red;
            }
            else
            {
                countdownText.text = $"Rescue Time: {seconds}s";
                countdownText.color = Color.yellow;
            }
        }
    }

    public void StopRescueTimer()
    {
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    public void SurvivorSaved()
    {
        rescuedOnBoat++;
        StopRescueTimer();
        UpdateUI();
    }

    public void SurvivorDelivered()
    {
        deliveredToSafeZone++;
        UpdateUI();

        // PANALO LANG KAPAG: May na-rescue (> 0) AT pantay o lumagpas sa totalSurvivors!
        if (totalSurvivors > 0 && deliveredToSafeZone >= totalSurvivors)
        {
            MissionComplete();
        }
    }

    void UpdateUI()
    {
        if (statusText != null && isMissionStarted)
        {
            if (totalSurvivors > 0 && deliveredToSafeZone >= totalSurvivors)
            {
                statusText.text = "All Survivors are Safe!";
            }
            else if (rescuedOnBoat > deliveredToSafeZone)
            {
                statusText.text = $"Survivors on Board: {rescuedOnBoat - deliveredToSafeZone}\nDeliver them to the Safe Zone!";
            }
            else
            {
                statusText.text = $"Delivered: {deliveredToSafeZone} / {totalSurvivors} Survivors";
            }
        }
    }

    void MissionComplete()
    {
        StopRescueTimer();

        // 1. Ipakita ang Win Screen Panel
        if (winScreenUI != null)
        {
            winScreenUI.SetActive(true);
        }

        // 2. I-set ang text
        if (winSummaryText != null)
        {
            winSummaryText.text = $"Nailigtas mo ang lahat ng {totalSurvivors} survivors!";
        }

        // 3. I-update ang tutorial task sa tagumpay
        if (TutorialTaskManager.instance != null)
        {
            TutorialTaskManager.instance.OnSurvivorsDelivered(false);
        }

        // 4. Freeze laro at ilabas ang mouse
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("TALAGANG PANALO NA!");
    }

    // --- BUTTONS ---
    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(nextLevelSceneName))
        {
            SceneManager.LoadScene(nextLevelSceneName);
        }
        else
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}