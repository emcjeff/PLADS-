using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DrowningNPC : MonoBehaviour
{
    public enum NPCState { Drowning, HoldingBuoy, Panicking, Rescued }

    [Header("State")]
    public NPCState currentState = NPCState.Drowning;

    [Header("Stamina / Grip Settings")]
    public float maxStamina = 15f;           // Oras bago mag-panic kung hindi agad nahatak
    public float currentStamina;
    public float pickupDistance = 4.5f;      // Layo sa bangka bago kusa sumampa

    [Header("Panic Settings")]
    public int hitsToCalm = 2;               // Ilang bato ng salbabida bago kumalma
    private int currentHits = 0;
    public float panicSwimSpeed = 2.5f;      // Bilis ng pag-iwas habang nagpapanic
    public float panicStruggleSpeed = 10f;

    [Header("Panic Boundary (Limitasyon ng Layas)")]
    [Tooltip("Hanggang ilang metro lang ang pwede niyang layuin mula sa pwesto niya")]
    public float panicRadius = 3.5f;
    private Vector3 panicAnchorPos;
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

    // 1. HABANG NALULUNOD SA TUBIG
    void HandleDrowning()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * struggleSpeed) * struggleHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    // 2. HABANG NAKA-HOOK AT HINAHATAK NG SALBABIDA
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
            meterStatusText.text = "HOOKED! REEL IN!";
            meterStatusText.color = Color.white;
        }

        // Kapag nahatak na palapit sa bangka -> SUMAMPA NA SA BANGKA!
        if (boat != null)
        {
            float distanceToBoat = Vector3.Distance(transform.position, boat.transform.position);
            if (distanceToBoat <= pickupDistance)
            {
                BoardBoat();
                return;
            }
        }

        // Kapag naubos ang oras bago mahatak -> MAG-PANIC!
        if (currentStamina <= 0f)
        {
            TriggerPanicState();
        }
    }

    // 3. HABANG NAG-PAPANIC (Limitado sa loob ng panicRadius)
    void HandlePanicking()
    {
        BillboardToCamera();

        float newY = startPos.y + Mathf.Sin(Time.time * panicStruggleSpeed) * (struggleHeight * 1.5f);

        changeDirectionTimer -= Time.deltaTime;
        if (changeDirectionTimer <= 0f)
        {
            float distFromAnchor = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.z),
                new Vector2(panicAnchorPos.x, panicAnchorPos.z)
            );

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

        Vector3 moveOffset = new Vector3(randomWanderDirection.x, 0f, randomWanderDirection.y) * panicSwimSpeed * Time.deltaTime;
        Vector3 targetPos = transform.position + moveOffset;

        // Hard Clamp: Bawal lumagpas sa radius
        Vector3 offset = targetPos - panicAnchorPos;
        offset.y = 0f;
        if (offset.magnitude > panicRadius)
        {
            offset = offset.normalized * panicRadius;
        }

        transform.position = new Vector3(panicAnchorPos.x + offset.x, newY, panicAnchorPos.z + offset.z);

        // Kumikislap na Pulang Panic Meter
        if (fillImage != null)
        {
            float pulse = Mathf.PingPong(Time.time * 6f, 1f);
            fillImage.color = Color.Lerp(Color.red, new Color(1f, 0.4f, 0f), pulse);
        }
    }

    // TATAWAGIN NG THROWLIFEBUOY KAPAG NAPUTOL ANG LUBID DAHIL SA SOBRANG TENSION
    public void OnRopeSnapped()
    {
        TriggerPanicState();

        if (TutorialTaskManager.instance != null && TutorialTaskManager.instance.taskText != null)
        {
            TutorialTaskManager.instance.taskText.text = $"NAPUTOL ANG LUBID! Nag-panic ang survivor! Batuhin ng {hitsToCalm} beses!";
            TutorialTaskManager.instance.taskText.color = Color.red;
        }
    }

    void TriggerPanicState()
    {
        currentState = NPCState.Panicking;
        currentHits = 0;

        panicAnchorPos = new Vector3(transform.position.x, startPos.y, transform.position.z);

        // Itigil ang screen timer dahil pumasok na sa Panic mode
        if (RescueManager.instance != null)
        {
            RescueManager.instance.StopRescueTimer();
        }

        if (attachedBuoy != null)
        {
            Destroy(attachedBuoy);
            attachedBuoy = null;
        }

        if (staminaCanvas != null) staminaCanvas.SetActive(true);
        if (staminaSlider != null) staminaSlider.value = 1f;

        if (meterStatusText != null)
        {
            meterStatusText.text = "PANICKING!";
            meterStatusText.color = Color.red;
        }

        if (TutorialTaskManager.instance != null && TutorialTaskManager.instance.taskText != null)
        {
            TutorialTaskManager.instance.taskText.text = $"NAG-PANIC ANG SURVIVOR! Batuhin ng {hitsToCalm} beses para kumalma!";
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

        float panicRemaining = 1f - ((float)currentHits / hitsToCalm);
        if (staminaSlider != null) staminaSlider.value = panicRemaining;

        int remainingHits = hitsToCalm - currentHits;

        if (remainingHits > 0)
        {
            Destroy(buoyObj);

            if (meterStatusText != null) meterStatusText.text = $"PANIC: {remainingHits} HIT LEFT!";
            if (TutorialTaskManager.instance != null && TutorialTaskManager.instance.taskText != null)
            {
                TutorialTaskManager.instance.taskText.text = $"TUMAMA! {remainingHits} bato na lang bago kumalma!";
                TutorialTaskManager.instance.taskText.color = Color.yellow;
            }
        }
        else
        {
            // SA HULING TAMA: Kumalma na siya at I-HOOK NA AGAD ang salbabidang tumama!
            GrabBuoy(buoyObj);
        }
    }

    void GrabBuoy(GameObject buoy)
    {
        currentState = NPCState.HoldingBuoy;
        attachedBuoy = buoy;
        startPos = transform.position;

        Lifebuoy buoyScript = attachedBuoy.GetComponent<Lifebuoy>();
        if (buoyScript != null)
        {
            buoyScript.AttachToNPC(this);
        }

        // Ikabit sa bewang ng NPC nang hindi binabaligtad ang patag nitong rotation
        attachedBuoy.transform.SetParent(transform, true);
        attachedBuoy.transform.localPosition = new Vector3(0f, 0.8f, 0f);

        currentStamina = maxStamina;

        if (staminaCanvas != null) staminaCanvas.SetActive(true);

        if (TutorialTaskManager.instance != null)
        {
            TutorialTaskManager.instance.OnBuoyAttachedToNPC();
        }
    }

    // Tinatawag ng ThrowLifebuoy habang hinahatak para hindi mag-snap back ang NPC
    public void UpdateAnchorPosition(Vector3 newPos)
    {
        startPos = newPos;
        panicAnchorPos = newPos;
    }

    void BoardBoat()
    {
        currentState = NPCState.Rescued;

        if (staminaCanvas != null) staminaCanvas.SetActive(false);
        if (col != null) col.enabled = false;

        if (RescueManager.instance != null) RescueManager.instance.StopRescueTimer();

        if (attachedBuoy != null)
        {
            Destroy(attachedBuoy);
            attachedBuoy = null;
        }

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Application.isPlaying ? panicAnchorPos : transform.position, panicRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }
}