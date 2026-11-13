using UnityEngine;
using TMPro;

public class TutorialTaskManager : MonoBehaviour
{
    public static TutorialTaskManager instance;

    public enum TaskStep
    {
        MoveWASD,          // Task 1: Lakad
        ThrowPractice,     // Task 2: Praktis bato
        BoardBoat,         // Task 3: Sakay sa bangka
        SearchSurvivors,   // Task 4: Hanapin ang nalulunod
        ApproachSurvivor,  // Task 5: Lapitan ang nalulunod gamit ang bangka
        DeliverToSafeZone, // Task 6: Ihatid sa Safe Zone
        AllTasksComplete   // Mission Success
    }

    [Header("Current Progress")]
    public TaskStep currentStep = TaskStep.MoveWASD;

    [Header("UI Reference")]
    public TextMeshProUGUI taskText;      // Text para sa instruction
    public GameObject taskPanel;          // Panel box sa screen

    private float walkTimer = 0f;
    private float requiredWalkTime = 1.5f;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        UpdateTaskUI();
    }

    void Update()
    {
        if (currentStep == TaskStep.MoveWASD)
        {
            CheckMovementTask();
        }
    }

    void CheckMovementTask()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h != 0 || v != 0)
        {
            walkTimer += Time.deltaTime;
            if (walkTimer >= requiredWalkTime)
            {
                currentStep = TaskStep.ThrowPractice;
                UpdateTaskUI();
            }
        }
    }

    // 1. Kapag nagbato ng salbabida sa tutorial
    public void OnPlayerThrewBuoy()
    {
        if (currentStep == TaskStep.ThrowPractice)
        {
            currentStep = TaskStep.BoardBoat;
            UpdateTaskUI();
        }
    }

    // 2. Kapag sumakay na sa bangka
    public void OnPlayerBoardedBoat()
    {
        if (currentStep == TaskStep.BoardBoat)
        {
            currentStep = TaskStep.SearchSurvivors;
            UpdateTaskUI();

            // Simulan ang baha at mission popup
            if (RescueManager.instance != null)
            {
                RescueManager.instance.TriggerMissionStart();
            }
        }
    }

    // 3. KAPAG NATAMAAN NG SALBABIDA ANG NALULUNOD
    public void OnBuoyAttachedToNPC()
    {
        currentStep = TaskStep.ApproachSurvivor;
        UpdateTaskUI();
    }

    // 4. KAPAG BUMITAW ANG NPC DAHIL NAUBUSAN NG ORAS
    public void OnSurvivorLost()
    {
        currentStep = TaskStep.SearchSurvivors;
        if (taskText != null)
        {
            taskText.text = "Bumitaw ang survivor! Batuhan ulit ng salbabida!";
            taskText.color = Color.red;
        }
    }

    // 5. KAPAG NAIAHON / NAISAKAY NA SA BANGKA
    public void OnSurvivorBoardedBoat()
    {
        currentStep = TaskStep.DeliverToSafeZone;
        UpdateTaskUI();
    }

    // 6. KAPAG NAIBABA SA SAFE ZONE
    public void OnSurvivorsDelivered(bool hasMoreSurvivors)
    {
        if (hasMoreSurvivors)
        {
            currentStep = TaskStep.SearchSurvivors;
            if (taskText != null)
            {
                taskText.text = "Ligtas na sila! Hanapin pa ang ibang nalulunod sa mapa!";
                taskText.color = Color.white;
            }
        }
        else
        {
            currentStep = TaskStep.AllTasksComplete;
            UpdateTaskUI();
        }
    }

    void UpdateTaskUI()
    {
        if (taskText == null) return;
        taskText.color = Color.white;

        switch (currentStep)
        {
            case TaskStep.MoveWASD:
                taskText.text = "TASK 1: Subukang maglakad gamit ang [WASD]";
                break;

            case TaskStep.ThrowPractice:
                taskText.text = "TASK 2: Subukang magbato ng salbabida [Left Click / F]";
                break;

            case TaskStep.BoardBoat:
                taskText.text = "TASK 3: Pumunta sa bangka at sumakay [Pindutin ang E]";
                break;

            case TaskStep.SearchSurvivors:
                taskText.text = "TASK 4: I-drive ang bangka at hanapin ang mga nalulunod!";
                break;

            case TaskStep.ApproachSurvivor:
                taskText.text = "TASK 5: MABILIS! Lapitan ang nalulunod gamit ang bangka para maisakay!";
                taskText.color = Color.yellow;
                break;

            case TaskStep.DeliverToSafeZone:
                taskText.text = "TASK 6: I-drive ang bangka papunta sa Safe Zone para ibaba sila!";
                taskText.color = Color.cyan;
                break;

            case TaskStep.AllTasksComplete:
                taskText.text = "MISSION ACCOMPLISHED: Nailigtas mo ang lahat!";
                taskText.color = Color.green;
                break;
        }
    }
}