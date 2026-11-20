using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DrowningNPC : MonoBehaviour
{
    public enum NPCState { Drowning, HoldingBuoy, Panicking, Rescued }

    [Header("State")]
    public NPCState currentState = NPCState.Drowning;

    [Header("Stamina / Grip Settings")]
    public float maxStamina = 12f;
    public float currentStamina;
    public float pickupDistance = 4.5f;

    [Header("Panic Meter Settings")]
    public int hitsToCalm = 2;               // Ilang bato bago mapawi ang panic
    private int currentHits = 0;             // Ilang tama na
    public float panicSwimSpeed = 3f;        // Bilis ng paglangoy habang nagwawala
    public float panicStruggleSpeed = 10f;
    private Vector2 randomWanderDirection;
    private float changeDirectionTimer = 0f;

    [Header("UI Overhead Meter")]
    public GameObject staminaCanvas;         // Ang World Space Canvas sa ulo
    public Slider staminaSlider;             // Ang Slider bar
    public Image fillImage;                  // Kulay ng bar
    public TextMeshProUGUI meterStatusText;  // (Optional) Text tulad ng "PANIC!" o "CALM"

    [Header("Normal Struggle")]
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

            case NPCState.Panicking:
                HandlePanicking();
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

    // 1. STAMINA / GRIP MODE (Green -> Yellow -> Red)
    void HandleHoldingBuoy()
    {
        BillboardToCamera();

        currentStamina -= Time.deltaTime;

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

        if (meterStatusText != null)
        {
            meterStatusText.text = "HOLDING ON!";
            meterStatusText.color = Color.white;
        }

        // Lapitan ng bangka
        if (boat != null)
        {
            float distanceToBoat = Vector3.Distance(transform.position, boat.transform.position);
            if (distanceToBoat <= pickupDistance)
            {
                BoardBoat();
                return;
            }
        }

        // Kapag naubos ang oras -> TRIGGER PANIC METER!
        if (currentStamina <= 0f)
        {
            TriggerPanicState();
        }
    }

    // 2. PANIC METER MODE (Red Pulsing Bar)
    void HandlePanicking()
    {
        BillboardToCamera();

        // Mabilis na pag-alon at paggalaw
        float newY = startPos.y + Mathf.Sin(Time.time * panicStruggleSpeed) * (struggleHeight * 1.5f);

        changeDirectionTimer -= Time.deltaTime;
        if (changeDirectionTimer <= 0f)
        {
            randomWanderDirection = Random.insideUnitCircle.normalized;
            changeDirectionTimer = Random.Range(1f, 2f);
        }

        Vector3 moveOffset = new Vector3(randomWanderDirection.x, 0f, randomWanderDirection.y) * panicSwimSpeed * Time.deltaTime;
        transform.position += moveOffset;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // PULSING EFFECT SA PANIC METER (Kumikislap na Pula/Orange)
        if (fillImage != null)
        {
            float pulse = Mathf.PingPong(Time.time * 6f, 1f);
            fillImage.color = Color.Lerp(Color.red, new Color(1f, 0.4f, 0f), pulse);
        }
    }

    void TriggerPanicState()
    {
        currentState = NPCState.Panicking;
        currentHits = 0;

        // Tanggalin ang salbabida
        if (attachedBuoy != null) Destroy(attachedBuoy);

        // ILITAW ANG PANIC METER (100% Full Panic)
        if (staminaCanvas != null) staminaCanvas.SetActive(true);
        if (staminaSlider != null) staminaSlider.value = 1f;

        if (meterStatusText != null)
        {
            meterStatusText.text = "⚠️ PANICKING!";
            meterStatusText.color = Color.red;
        }

        if (TutorialTaskManager.instance != null)
        {
            TutorialTaskManager.instance.taskText.text = $"⚠️ NAG-PANIC ANG SURVIVOR! Batuhin ng {hitsToCalm} beses para ma-ubos ang Panic Meter!";
            TutorialTaskManager.instance.taskText.color = Color.red;
        }

        Debug.Log("PUMASOK SA PANIC STATE ANG NPC!");
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isBuoy = other.CompareTag("Lifebuoy") || other.GetComponentInParent<Lifebuoy>() != null;
        if (!isBuoy) return;

        GameObject buoyObj = other.GetComponentInParent<Lifebuoy>() != null 
            ? other.GetComponentInParent<Lifebuoy>().gameObject 
            : other.gameObject;

        if (currentState == NPCState.Drowning)
        {
            GrabBuoy(buoyObj);
        }
        else if (currentState == NPCState.Panicking)
        {
            HandlePanicHit(buoyObj);
        }
    }

    // BAWAS SA PANIC METER BAWAT TAMA NG SALBABIDA
    void HandlePanicHit(GameObject buoyObj)
    {
        currentHits++;
        Destroy(buoyObj);

        // Calculate bawas sa Panic Meter (hal. 2 hits = bawas 50% kada bato)
        float panicRemaining = 1f - ((float)currentHits / hitsToCalm);

        if (staminaSlider != null)
        {
            staminaSlider.value = panicRemaining;
        }

        int remainingHits = hitsToCalm - currentHits;

        if (remainingHits > 0)
        {
            if (meterStatusText != null)
            {
                meterStatusText.text = $"PANIC: {remainingHits} HIT LEFT!";
            }

            if (TutorialTaskManager.instance != null)
            {
                TutorialTaskManager.instance.taskText.text = $"⚠️ TUMAMA! {remainingHits} bato na lang bago kumalma!";
                TutorialTaskManager.instance.taskText.color = Color.yellow;
            }
        }
        else
        {
            // UBOS NA ANG PANIC METER -> KUMALMA NA!
            CalmDownFromPanic();
        }
    }

    void CalmDownFromPanic()
    {
        currentState = NPCState.HoldingBuoy;
        startPos = transform.position;
        currentStamina = maxStamina;

        if (meterStatusText != null)
        {
            meterStatusText.text = "CALMED DOWN!";
            meterStatusText.color = Color.green;
        }

        if (TutorialTaskManager.instance != null)
        {
            TutorialTaskManager.instance.OnBuoyAttachedToNPC();
        }

        Debug.Log("UBOS NA ANG PANIC! Kumalma na siya!");
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

    void BoardBoat()
    {
        currentState = NPCState.Rescued;

        if (staminaCanvas != null) staminaCanvas.SetActive(false);
        if (col != null) col.enabled = false;

        if (RescueManager.instance != null) RescueManager.instance.StopRescueTimer();
        if (attachedBuoy != null) Destroy(attachedBuoy);
        if (boat != null) boat.AddPassenger(transform);
        if (RescueManager.instance != null) RescueManager.instance.SurvivorSaved();
        if (TutorialTaskManager.instance != null) TutorialTaskManager.instance.OnSurvivorBoardedBoat();
    }

    void BillboardToCamera()
    {
        if (staminaCanvas != null && mainCam != null)
        {
            staminaCanvas.transform.forward = mainCam.transform.forward;
        }
    }
}