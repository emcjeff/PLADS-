using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThrowLifebuoy : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    public Transform throwPoint;
    public GameObject lifebuoyPrefab;

    [Header("Throw Settings")]
    public float throwForce = 22f;
    public float throwUpwardForce = 4f;
    public Vector3 modelRotationOffset = new Vector3(90f, 0f, 0f);

    [Header("Hook & Rope Settings")]
    public float reelSpeed = 9f;             // Bilis ng paghatak ng lubid
    public float waterPullBackSpeed = 1.2f;  // Bahagyang atras sa agos kapag binitawan ang hatak
    public float ropeWidth = 0.08f;
    public Color normalRopeColor = new Color(0.95f, 0.85f, 0.5f); // Kulay lubid
    public Color dangerRopeColor = Color.red;                     // Kulay kapag malapit maputol

    [Header("Rope Tension System")]
    public float maxTension = 100f;
    public float tensionGainRate = 45f;      // Bilis ng pagtaas ng tension habang humahatak
    public float tensionCoolRate = 60f;      // Bilis ng paglamig kapag binitawan
    private float currentTension = 0f;

    [Header("Tension UI References")]
    public GameObject tensionPanel;
    public Slider tensionSlider;
    public Image tensionFillImage;
    public TextMeshProUGUI tensionStatusText;

    [Header("Cooldown Settings")]
    public float cooldownTime = 2f;
    private float cooldownTimer = 0f;
    private bool isReady = true;

    [Header("UI Cooldown")]
    public Image cooldownFillImage;
    public TextMeshProUGUI cooldownText;

    private LineRenderer ropeLine;
    private Lifebuoy activeBuoy;

    void Start()
    {
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        if (cooldownText != null) cooldownText.gameObject.SetActive(false);
        if (tensionPanel != null) tensionPanel.SetActive(false);

        ropeLine = GetComponent<LineRenderer>();
        if (ropeLine == null)
        {
            ropeLine = gameObject.AddComponent<LineRenderer>();
        }

        ropeLine.positionCount = 2;
        ropeLine.startWidth = ropeWidth;
        ropeLine.endWidth = ropeWidth;

        Shader spriteShader = Shader.Find("Sprites/Default");
        if (spriteShader != null)
        {
            ropeLine.material = new Material(spriteShader);
        }

        ropeLine.startColor = normalRopeColor;
        ropeLine.endColor = normalRopeColor;
        ropeLine.enabled = false;
    }

    void Update()
    {
        // 1. Cooldown calculation
        if (!isReady)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownFillImage != null) cooldownFillImage.fillAmount = 1f - (cooldownTimer / cooldownTime);
            if (cooldownText != null)
            {
                if (!cooldownText.gameObject.activeSelf) cooldownText.gameObject.SetActive(true);
                cooldownText.text = cooldownTimer.ToString("F1") + "s";
            }

            if (cooldownTimer <= 0f)
            {
                isReady = true;
                if (cooldownFillImage != null) cooldownFillImage.fillAmount = 1f;
                if (cooldownText != null) cooldownText.gameObject.SetActive(false);
            }
        }

        // 2. Lubid, Paghatak, at Rope Tension System
        HandleRopeAndTension();

        // 3. Pagbato (Huwag magbato kung may hinahatak ka pang survivor)
        if (Cursor.lockState != CursorLockMode.Locked) return;

        bool isAlreadyHooked = (activeBuoy != null && activeBuoy.isHooked);
        if (!isAlreadyHooked && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F)) && isReady)
        {
            Throw();
        }
    }

    void HandleRopeAndTension()
    {
        // Kung walang aktibong salbabida, itago ang lubid at tension UI
        if (activeBuoy == null)
        {
            if (ropeLine != null && ropeLine.enabled) ropeLine.enabled = false;
            if (tensionPanel != null && tensionPanel.activeSelf) tensionPanel.SetActive(false);
            currentTension = 0f;
            return;
        }

        // A. DITO LANG LALABAS ANG LUBID AT TENSION: Kapag tumama at kumapit sa NPC!
        if (activeBuoy.isHooked && activeBuoy.hookedNPC != null)
        {
            if (tensionPanel != null && !tensionPanel.activeSelf) tensionPanel.SetActive(true);

            Vector3 handOrigin = throwPoint != null ? throwPoint.position : transform.position + Vector3.up * 1.1f;
            bool holdingPullButton = Input.GetMouseButton(1) || Input.GetKey(KeyCode.R);

            Transform npcTransform = activeBuoy.hookedNPC.transform;
            Vector3 targetPos = new Vector3(transform.position.x, npcTransform.position.y, transform.position.z);

            if (holdingPullButton)
            {
                // HATAK + TAAS NG TENSION!
                currentTension += tensionGainRate * Time.deltaTime;

                npcTransform.position = Vector3.MoveTowards(npcTransform.position, targetPos, reelSpeed * Time.deltaTime);

                if (Vector3.Distance(npcTransform.position, targetPos) > 0.1f)
                {
                    npcTransform.LookAt(targetPos);
                }
                activeBuoy.hookedNPC.UpdateAnchorPosition(npcTransform.position);
            }
            else
            {
                // BITAW: LUMALAMIG ANG TENSION + KAUNTING ATRAS SA AGOS!
                currentTension -= tensionCoolRate * Time.deltaTime;

                if (waterPullBackSpeed > 0f)
                {
                    Vector3 pushAwayDir = (npcTransform.position - targetPos).normalized;
                    npcTransform.position += pushAwayDir * waterPullBackSpeed * Time.deltaTime;
                    activeBuoy.hookedNPC.UpdateAnchorPosition(npcTransform.position);
                }
            }

            currentTension = Mathf.Clamp(currentTension, 0f, maxTension);
            UpdateTensionVisuals(handOrigin);

            // KAPAG UMABOT SA 100% -> NAPUTOL ANG LUBID!
            if (currentTension >= maxTension)
            {
                SnapRope();
                return;
            }
        }
        // B. KUNG LUMILIPAD PA LANG O NAGMINTIS: WALANG LUBID NA NAKIKITA!
        else
        {
            if (ropeLine != null && ropeLine.enabled) ropeLine.enabled = false;
            if (tensionPanel != null && tensionPanel.activeSelf) tensionPanel.SetActive(false);
            currentTension = 0f;
        }
    }

    void UpdateTensionVisuals(Vector3 handOrigin)
    {
        float ratio = currentTension / maxTension;

        if (tensionSlider != null) tensionSlider.value = ratio;

        if (tensionFillImage != null)
        {
            if (ratio < 0.5f)
                tensionFillImage.color = Color.Lerp(Color.green, Color.yellow, ratio * 2f);
            else
                tensionFillImage.color = Color.Lerp(Color.yellow, Color.red, (ratio - 0.5f) * 2f);
        }

        if (tensionStatusText != null)
        {
            if (ratio >= 0.75f)
            {
                tensionStatusText.text = "DANGER! RELEASE!";
                tensionStatusText.color = Color.red;
            }
            else if (ratio >= 0.45f)
            {
                tensionStatusText.text = "TENSION HIGH...";
                tensionStatusText.color = Color.yellow;
            }
            else
            {
                tensionStatusText.text = "REELING (HOLD [R / RMB])";
                tensionStatusText.color = Color.white;
            }
        }

        Color currentRopeCol = Color.Lerp(normalRopeColor, dangerRopeColor, ratio);
        bool shouldShake = ratio >= 0.75f;
        DrawRope(handOrigin, activeBuoy.transform.position, currentRopeCol, shouldShake);
    }

    void DrawRope(Vector3 start, Vector3 end, Color col, bool shake)
    {
        if (ropeLine == null) return;

        ropeLine.enabled = true;
        ropeLine.startColor = col;
        ropeLine.endColor = col;

        if (shake)
        {
            end += Random.insideUnitSphere * 0.08f;
        }

        ropeLine.SetPosition(0, start);
        ropeLine.SetPosition(1, end);
    }

    void SnapRope()
    {
        Debug.Log("SNAP! Naputol ang lubid dahil sa sobrang tension!");

        DrowningNPC npc = activeBuoy.hookedNPC;

        Destroy(activeBuoy.gameObject);
        activeBuoy = null;
        currentTension = 0f;

        if (ropeLine != null) ropeLine.enabled = false;
        if (tensionPanel != null) tensionPanel.SetActive(false);

        if (npc != null)
        {
            npc.OnRopeSnapped();
        }
    }

    void Throw()
    {
        if (lifebuoyPrefab == null) return;

        // Kung may lumang nagmintis na salbabida, burahin ito bago magbato ng bago
        if (activeBuoy != null && !activeBuoy.isHooked)
        {
            Destroy(activeBuoy.gameObject);
            activeBuoy = null;
        }

        currentTension = 0f;
        isReady = false;
        cooldownTimer = cooldownTime;

        if (cooldownFillImage != null) cooldownFillImage.fillAmount = 0f;
        if (cooldownText != null) cooldownText.gameObject.SetActive(true);

        if (TutorialTaskManager.instance != null)
        {
            TutorialTaskManager.instance.OnPlayerThrewBuoy();
        }

        Vector3 spawnPos = throwPoint != null 
            ? throwPoint.position 
            : transform.position + (Vector3.up * 1.2f) + (cam.forward * 1.5f);

        Quaternion spawnRotation = Quaternion.Euler(modelRotationOffset.x, cam.eulerAngles.y + modelRotationOffset.y, modelRotationOffset.z);
        GameObject buoyObj = Instantiate(lifebuoyPrefab, spawnPos, spawnRotation);

        activeBuoy = buoyObj.GetComponentInChildren<Lifebuoy>();
        if (activeBuoy == null)
        {
            activeBuoy = buoyObj.AddComponent<Lifebuoy>();
        }

        Collider buoyCol = buoyObj.GetComponentInChildren<Collider>();
        if (buoyCol != null)
        {
            Collider[] myCols = GetComponentsInChildren<Collider>();
            foreach (var c in myCols) Physics.IgnoreCollision(buoyCol, c);

            BoatController boat = Object.FindAnyObjectByType<BoatController>();
            if (boat != null)
            {
                Collider[] boatCols = boat.GetComponentsInChildren<Collider>();
                foreach (var c in boatCols) Physics.IgnoreCollision(buoyCol, c);
            }
        }

        Rigidbody buoyRb = buoyObj.GetComponent<Rigidbody>();
        if (buoyRb != null)
        {
            buoyRb.linearVelocity = Vector3.zero;
            buoyRb.constraints = RigidbodyConstraints.FreezeRotation;

            Vector3 forceDirection = (cam.forward * throwForce) + (Vector3.up * throwUpwardForce);
            buoyRb.AddForce(forceDirection, ForceMode.Impulse);
        }
    }
}