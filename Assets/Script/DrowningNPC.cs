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

    [Header("Panic Settings")]
    public int hitsToCalm = 2;               // Ilang bato ng salbabida bago kumalma
    private int currentHits = 0;
    public float panicSwimSpeed = 2.5f;      // Bilis ng paglangoy habang nagpapanic
    public float panicStruggleSpeed = 10f;
    
    [Header("Panic Boundary (Limitasyon ng Layas)")]
    [Tooltip("Hanggang ilang metro lang ang pwede niyang layuin mula sa pwesto niya")]
    public float panicRadius = 3.5f;         // 3.5 METERS LANG ANG RADIUS!
    private Vector3 panicAnchorPos;          // Dito siya nakatali
    private Vector2 randomWanderDirection;
    private float changeDirectionTimer = 0f;

    [Header("UI Overhead Meter")]
    public GameObject staminaCanvas;
    public Slider staminaSlider;
    public Image fillImage;
    public TextMeshProUGUI meterStatusText;

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
        panicAnchorPos = transform.position;
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

    void HandleHoldingBuoy()
    {
        BillboardToCamera();

        currentStamina -= Time.deltaTime;

        if (RescueManager.instance != null)
        {
            RescueManager.instance.UpdateRescueTimer(currentStamina);
        }

        if (staminaSlider != null) staminaSlider.value = currentStamina / maxStamina;
        if (fillImage != null) fillImage.color = Color.Lerp(Color.red, Color.green, currentStamina / maxStamina);

        if (meterStatusText != null)
        {
            meterStatusText.text = "HOLDING ON!";
            meterStatusText.color = Color.white;
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
            TriggerPanicState();
        }
    }

    // --- HARD-CLAMPED PANIC MOVEMENT ---
    void HandlePanicking()
    {
        BillboardToCamera();

        // 1. Magulong pag-alon
        float newY = startPos.y + Mathf.Sin(Time.time * panicStruggleSpeed) * (struggleHeight * 1.5f);

        // 2. Pagpili ng direksyon
        changeDirectionTimer -= Time.deltaTime;
        if (changeDirectionTimer <= 0f)
        {
            float distFromAnchor = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(panicAnchorPos.x, panicAnchorPos.z));
            
            // Kapag malapit na sa dulo ng 3.5m radius, pabalikin sa gitna
            if (distFromAnchor >= panicRadius * 0.8f)
            {
                Vector2 backToCenter = (new Vector2(panicAnchorPos.x, panicAnchorPos.z) - new Vector2(transform.position.x, transform.position.z)).normalized;
                randomWanderDirection = backToCenter;
            }
            else
            {
                randomWanderDirection = Random.insideUnitCircle.normalized;
            }

            changeDirectionTimer = Random.Range(0.8f, 1.5f);
        }

        // 3. I-apply ang galaw
        Vector3 moveOffset = new Vector3(randomWanderDirection.x, 0f, randomWanderDirection.y) * panicSwimSpeed * Time.deltaTime;
        Vector3 targetPos = transform.position + moveOffset;

        // HARD CLAMP: Bawal lumagpas sa 3.5 meters kailanman!
        Vector3 offset = targetPos - panicAnchorPos;
        offset.y = 0f;
        if (offset.magnitude > panicRadius)
        {
            offset = offset.normalized * panicRadius;
        }

        transform.position = new Vector3(panicAnchorPos.x + offset.x, newY, panicAnchorPos.z + offset.z);

        // 4. Kislap ng Panic Meter
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

        // I-lock ang pwesto kung saan siya nag-panic para hindi lumayo rito
        panicAnchorPos = new Vector3(transform.position.x, startPos.y, transform.position.z);

        if (attachedBuoy != null) Destroy(attachedBuoy);

        if (staminaCanvas != null) staminaCanvas.SetActive(true);
        if (staminaSlider != null) staminaSlider.value = 1f;

        if (meterStatusText != null)
        {
            meterStatusText.text = "⚠️ PANICKING!";
            meterStatusText.color = Color.red;
        }

        if (TutorialTaskManager.instance != null)
        {
            TutorialTaskManager.instance.taskText.text = $"⚠️ NAG-PANIC ANG SURVIVOR! Batuhin ng {hitsToCalm} beses para kumalma!";
            TutorialTaskManager.instance.taskText.color = Color.red;
        }
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

    void HandlePanicHit(GameObject buoyObj)
    {
        currentHits++;
        Destroy(buoyObj);

        float panicRemaining = 1f - ((float)currentHits / hitsToCalm);
        if (staminaSlider != null) staminaSlider.value = panicRemaining;

        int remainingHits = hitsToCalm - currentHits;

        if (remainingHits > 0)
        {
            if (meterStatusText != null) meterStatusText.text = $"PANIC: {remainingHits} HIT LEFT!";
            if (TutorialTaskManager.instance != null)
            {
                TutorialTaskManager.instance.taskText.text = $"TUMAMA! {remainingHits} bato na lang bago kumalma!";
                TutorialTaskManager.instance.taskText.color = Color.yellow;
            }
        }
        else
        {
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

    // Makikita mo ang pulang bilog na hangganan sa Scene view kapag naka-click ang NPC!
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Application.isPlaying ? panicAnchorPos : transform.position, panicRadius);
    }
}