using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RescueManager : MonoBehaviour
{
    public static RescueManager instance;

    [Header("Mission Status")]
    public bool isMissionStarted = false;
    public int totalSurvivors = 3;           // Bilang ng mga ililigtas
    public int rescuedOnBoat = 0;           // Ilan na ang nasa bangka
    public int deliveredToSafeZone = 0;     // Ilan na ang naibaba sa Safe Zone

    [Header("Mission Elements (Lilitaw sa Pagsakay)")]
    public GameObject floodWaterObject;      // Ang Tubig-Baha
    public GameObject safeZoneObject;        // Ang Safe Zone / Pantalan
    public GameObject missionStartBox;       // Emergency Briefing Box

    [Header("In-Game HUD References")]
    public TextMeshProUGUI statusText;       // "Delivered: 0 / 3 Survivors"
    public TextMeshProUGUI countdownText;    // Overhead / Screen Timer

    [Header("Win UI Screen")]
    public GameObject winScreenUI;           // Win Screen Panel
    public TextMeshProUGUI winSummaryText;   // Mensahe sa Win Screen
    public string nextLevelSceneName = "";   // Pangalan ng Level 2 (hal. "Level2")

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // 1. Itago ang lahat ng mission elements sa simula (tuyo at malinis ang kalsada)
        if (floodWaterObject != null) floodWaterObject.SetActive(false);
        if (safeZoneObject != null) safeZoneObject.SetActive(false);
        if (missionStartBox != null) missionStartBox.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (statusText != null) statusText.gameObject.SetActive(false);
        if (winScreenUI != null) winScreenUI.SetActive(false);

        // Kuhanin ang bilang ng tao mula sa NPCSpawner kung meron
        if (NPCSpawner.instance != null)
        {
            totalSurvivors = NPCSpawner.instance.totalNpcToSpawn;
        }
    }

    // TATAWAGIN ITO KAPAG SUMAKAY NA SA BANGKA ANG PLAYER
    public void TriggerMissionStart()
    {
        if (isMissionStarted) return;

        // 1. Palitawin ang baha at ang Safe Zone
        if (floodWaterObject != null) floodWaterObject.SetActive(true);
        if (safeZoneObject != null) safeZoneObject.SetActive(true);

        // 2. I-spawn ang mga nalulunod (NPCs)
        if (NPCSpawner.instance != null)
        {
            NPCSpawner.instance.SpawnRandomNPCs();
            totalSurvivors = NPCSpawner.instance.totalNpcToSpawn;
        }

        // 3. I-spawn ang mga lumulutang na basura at hazards (Debris)
        if (DebrisSpawner.instance != null)
        {
            DebrisSpawner.instance.SpawnAllDebris();
        }

        // 4. Ipakita ang Emergency Briefing Box
        if (missionStartBox != null)
        {
            missionStartBox.SetActive(true);
        }

        // I-pause ang laro at ilabas ang mouse para mapindot ang "OK"
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Tinatawag ng NPCSpawner kapag tapos na mag-spawn para tama ang bilang
    public void InitializeMissionStats()
    {
        DrowningNPC[] npcs = Object.FindObjectsByType<DrowningNPC>(FindObjectsSortMode.None);
        if (npcs.Length > 0)
        {
            totalSurvivors = npcs.Length;
        }
        UpdateUI();
    }

    // TATAWAGIN NG "OK" BUTTON SA BRIEFING BOX
    public void OnClickConfirmMission()
    {
        isMissionStarted = true;

        if (missionStartBox != null)
        {
            missionStartBox.SetActive(false);
        }

        // Ibalik ang takbo ng laro at i-lock ulit ang cursor
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ipakita na ang top status HUD
        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            UpdateUI();
        }

        Debug.Log("RESCUE MISSION OFFICIALLY STARTED!");
    }

    // UPDATE SA COUNTDOWN TIMER
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
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }

    // KAPAG NAIAHON ANG TAO SA BANGKA
    public void SurvivorSaved()
    {
        rescuedOnBoat++;
        StopRescueTimer();
        UpdateUI();
    }

    // KAPAG NAIBABA ANG TAO SA SAFEZONE
    public void SurvivorDelivered()
    {
        deliveredToSafeZone++;
        UpdateUI();

        // Panalo lang kapag naihatid na LAHAT sa Safe Zone!
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

        // 2. I-set ang buod ng tagumpay
        if (winSummaryText != null)
        {
            winSummaryText.text = $"Nailigtas mo ang lahat ng {totalSurvivors} survivors!";
        }

        // 3. I-update ang task text sa tagumpay
        if (TutorialTaskManager.instance != null)
        {
            TutorialTaskManager.instance.OnSurvivorsDelivered(false);
        }

        // 4. I-freeze ang laro at ilabas ang mouse para makapindot sa buttons
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("MISSION SUCCESS: Lahat ay ligtas na!");
    }

    // --- WIN SCREEN BUTTON FUNCTIONS ---

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;

        // Kung may inilagay na pangalan ng next scene (hal. "Level2")
        if (!string.IsNullOrEmpty(nextLevelSceneName))
        {
            SceneManager.LoadScene(nextLevelSceneName);
        }
        else
        {
            // O lilipat sa susunod na index sa Build Settings
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