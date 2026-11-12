using UnityEngine;
using TMPro;

public class RescueManager : MonoBehaviour
{
    public static RescueManager instance;

    [Header("Mission Status")]
    public bool isMissionStarted = false;
    public int totalSurvivors = 0;
    public int rescuedOnBoat = 0;
    public int deliveredToSafeZone = 0;

    [Header("Mission Start UI")]
    public GameObject missionStartBox;

    [Header("Flood Water")]
    public GameObject floodWaterObject; // DITO ILALAGAY ANG FLOODWATER OBJECT

    [Header("In-Game HUD References")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI countdownText;
    public GameObject winScreenUI;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        DrowningNPC[] npcs = Object.FindObjectsByType<DrowningNPC>(FindObjectsSortMode.None);
        totalSurvivors = npcs.Length;

        // 1. ITAGO ANG BAHA SA SIMULA PARA TUYO ANG KALSADA
        if (floodWaterObject != null)
        {
            floodWaterObject.SetActive(false);
        }

        if (missionStartBox != null) missionStartBox.SetActive(false);
        if (winScreenUI != null) winScreenUI.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (statusText != null) statusText.gameObject.SetActive(false);
    }

    // TATAWAGIN ITO KAPAG SUMAKAY SA BANGKA ANG PLAYER
    public void TriggerMissionStart()
    {
        if (isMissionStarted) return;

        // 2. PALITAWIN AGAD ANG BAHA!
        if (floodWaterObject != null)
        {
            floodWaterObject.SetActive(true);
        }

        if (missionStartBox != null)
        {
            missionStartBox.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnClickConfirmMission()
    {
        isMissionStarted = true;

        if (missionStartBox != null)
        {
            missionStartBox.SetActive(false);
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            UpdateUI();
        }

        Debug.Log("MISSION ACCEPTED! May baha na!");
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
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
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

        if (deliveredToSafeZone >= totalSurvivors && totalSurvivors > 0)
        {
            MissionComplete();
        }
    }

    void UpdateUI()
    {
        if (statusText != null && isMissionStarted)
        {
            if (deliveredToSafeZone >= totalSurvivors && totalSurvivors > 0)
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
        if (winScreenUI != null) winScreenUI.SetActive(true);
        StopRescueTimer();
        Debug.Log("MISSION SUCCESS!");
    }
}