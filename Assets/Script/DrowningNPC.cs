using UnityEngine;
using UnityEngine.UI;

public class DrowningNPC : MonoBehaviour
{
    public enum NPCState { Drowning, HoldingBuoy, Rescued }

    [Header("State")]
    public NPCState currentState = NPCState.Drowning;

    [Header("Stamina / Panic Timer")]
    public float maxStamina = 15f;
    public float currentStamina;
    public float pickupDistance = 4.5f;

    [Header("UI Bar References")]
    public GameObject staminaCanvas;
    public Slider staminaSlider;
    public Image fillImage;

    [Header("Struggle Animation")]
    public float struggleSpeed = 4f;
    public float struggleHeight = 0.25f;

    private Vector3 startPos;
    private Collider col;
    private GameObject attachedBuoy;
    private BoatController boat;
    private Camera mainCam;

    void Start()
    {
        startPos = transform.position;
        col = GetComponent<Collider>();
        mainCam = Camera.main;

        boat = Object.FindAnyObjectByType<BoatController>();

        if (staminaCanvas != null) staminaCanvas.SetActive(false);
    }

    void Update()
    {
        switch (currentState)
        {
            case NPCState.Drowning:
                HandleDrowning();
                break;

            case NPCState.HoldingBuoy:
                HandleHoldingBuoy();
                break;

            case NPCState.Rescued:
                break;
        }
    }

    void HandleDrowning()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * struggleSpeed) * struggleHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    void HandleHoldingBuoy()
    {
        if (staminaCanvas != null && mainCam != null)
        {
            staminaCanvas.transform.forward = mainCam.transform.forward;
        }

        currentStamina -= Time.deltaTime;

        // IPASA ANG ORAS SA SCREEN HUD TIMER
        if (RescueManager.instance != null)
        {
            RescueManager.instance.UpdateRescueTimer(currentStamina);
        }

        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina / maxStamina;
        }

        if (fillImage != null)
        {
            fillImage.color = Color.Lerp(Color.red, Color.green, currentStamina / maxStamina);
        }

        if (boat != null)
        {
            float distanceToBoat = Vector3.Distance(transform.position, boat.transform.position);
            if (distanceToBoat <= pickupDistance)
            {
                BoardBoat();
                return;
            }
        }

        if (currentStamina <= 0f)
        {
            LoseGrip();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentState == NPCState.Drowning && other.CompareTag("Lifebuoy"))
        {
            GrabBuoy(other.gameObject);
        }
    }

    void GrabBuoy(GameObject buoy)
    {
        currentState = NPCState.HoldingBuoy;
        attachedBuoy = buoy;

        attachedBuoy.transform.SetParent(transform);
        attachedBuoy.transform.localPosition = new Vector3(0f, -0.2f, 0f);
        attachedBuoy.transform.localRotation = Quaternion.identity;

        currentStamina = maxStamina;

        if (staminaCanvas != null) staminaCanvas.SetActive(true);

        if (TutorialTaskManager.instance != null)
        {
            TutorialTaskManager.instance.OnBuoyAttachedToNPC();
        }

    }

    void LoseGrip()
    {
        currentState = NPCState.Drowning;

        if (staminaCanvas != null) staminaCanvas.SetActive(false);

        // Itigil ang screen timer
        if (RescueManager.instance != null)
        {
            RescueManager.instance.StopRescueTimer();
        }

        if (TutorialTaskManager.instance != null)
        {
            TutorialTaskManager.instance.OnSurvivorLost();
        }

        if (attachedBuoy != null)
        {
            Destroy(attachedBuoy);
        }
    }

    void BoardBoat()
    {
        currentState = NPCState.Rescued;

        if (staminaCanvas != null) staminaCanvas.SetActive(false);
        if (col != null) col.enabled = false;

        // Itigil ang screen timer
        if (RescueManager.instance != null)
        {
            RescueManager.instance.StopRescueTimer();
        }

        if (attachedBuoy != null)
        {
            Destroy(attachedBuoy);
        }

        if (boat != null)
        {
            boat.AddPassenger(transform);
        }

        if (RescueManager.instance != null)
        {
            RescueManager.instance.SurvivorSaved();
        }

         if (TutorialTaskManager.instance != null)
        {
            TutorialTaskManager.instance.OnSurvivorBoardedBoat();
        }

    }
}