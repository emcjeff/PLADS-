using UnityEngine;
using TMPro;

public class TutorialTaskManager : MonoBehaviour
{
    public static TutorialTaskManager instance;

    public enum TaskStep
    {
        MoveWASD,          // Task 1
        ThrowPractice,     // Task 2
        BoardBoat,         // Task 3
        SearchSurvivors,   // Task 4
        ApproachSurvivor,  // Task 5
        DeliverToSafeZone, // Task 6
        AllTasksComplete   // Win
    }

    [Header("Level Configuration")]
    [Tooltip("I-check ito sa Level 2 para lampasan ang Tasks 1-3!")]
    public bool skipTutorial = false;

    [Header("Current Progress")]
    public TaskStep currentStep = TaskStep.MoveWASD;

    [Header("UI Reference")]
    public TextMeshProUGUI taskText;
    public GameObject taskPanel;

    private float walkTimer = 0f;
    private float requiredWalkTime = 1.5f;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // KUNG LEVEL 2 (Skip Tutorial): Diretso agad sa Task 4
        if (skipTutorial)
        {
            currentStep = TaskStep.SearchSurvivors;
        }
        else
        {
            currentStep = TaskStep.MoveWASD;
        }

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

    public void OnPlayerThrewBuoy()
    {
        if (currentStep == TaskStep.ThrowPractice)
        {
            currentStep = TaskStep.BoardBoat;
            UpdateTaskUI();
        }
    }

    // INAYOS DITO: GAGANA NA PAREHO SA LEVEL 1 AT LEVEL 2
    public void OnPlayerBoardedBoat()
    {
        if (currentStep == TaskStep.BoardBoat || skipTutorial)
        {
            currentStep = TaskStep.SearchSurvivors;
            UpdateTaskUI();

            // SIMULAN ANG BAHA, NPC SPAWN, AT MISSION TRACKER!
            if (RescueManager.instance != null)
            {
                RescueManager.instance.TriggerMissionStart();
            }
        }
    }

    public void OnBuoyAttachedToNPC()
    {
        currentStep = TaskStep.ApproachSurvivor;
        UpdateTaskUI();
    }

    public void OnSurvivorLost()
    {
        currentStep = TaskStep.SearchSurvivors;
        if (taskText != null)
        {
            taskText.text = "⚠️ Bumitaw ang survivor! Batuhan ulit ng salbabida!";
            taskText.color = Color.red;
        }
    }

    public void OnSurvivorBoardedBoat()
    {
        currentStep = TaskStep.DeliverToSafeZone;
        UpdateTaskUI();
    }

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
                taskText.text = "🎉 MISSION ACCOMPLISHED: Nailigtas mo ang lahat!";
                taskText.color = Color.green;
                break;
        }
    }
}