using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThrowLifebuoy : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    public GameObject lifebuoyPrefab;

    [Header("Throw Settings")]
    public float throwForce = 22f;          // Tinaasan natin para talagang lumipad paharap
    public float throwUpwardForce = 4f;     // Arc ng bato pataas
    public float lifebuoyDuration = 5f;     // Ilang segundo bago mawala ang salbabida

    [Header("Rotation Fix (Nakahiga)")]
    public Vector3 modelRotationOffset = new Vector3(90f, 0f, 0f);

    [Header("Cooldown Settings")]
    public float cooldownTime = 2f;
    private float cooldownTimer = 0f;
    private bool isReady = true;

    [Header("UI Cooldown")]
    public Image cooldownFillImage;
    public TextMeshProUGUI cooldownText;

    void Start()
    {
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        if (cooldownText != null) cooldownText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        if (!isReady)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownFillImage != null)
            {
                cooldownFillImage.fillAmount = 1f - (cooldownTimer / cooldownTime);
            }

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

        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F)) && isReady)
        {
            Throw();
        }
    }

    void Throw()
    {
        if (lifebuoyPrefab == null) return;

        isReady = false;
        cooldownTimer = cooldownTime;

        if (cooldownFillImage != null) cooldownFillImage.fillAmount = 0f;
        if (cooldownText != null) cooldownText.gameObject.SetActive(true);

        if (TutorialTaskManager.instance != null)
        {
            TutorialTaskManager.instance.OnPlayerThrewBuoy();
        }

        // 1. Mangagaling ang spawn sa posisyon ng mismong nagbato (Player) sa bandang dibdib
        // 2. Ilalabas nang 1.5 meters sa direksyon kung saan nakaharap ang Camera
        Vector3 spawnPos = transform.position + (Vector3.up * 1.2f) + (cam.forward * 1.5f);

        // Patag na ikot
        Quaternion spawnRotation = Quaternion.Euler(modelRotationOffset.x, cam.eulerAngles.y + modelRotationOffset.y, modelRotationOffset.z);
        GameObject buoy = Instantiate(lifebuoyPrefab, spawnPos, spawnRotation);

        // Kusang mawawala ang salbabida pagkatapos ng 'lifebuoyDuration' seconds
        if (lifebuoyDuration > 0f)
        {
            Destroy(buoy, lifebuoyDuration);
        }

        // I-ignore ang colliders para hindi maipit sa Player o Bangka
        Collider buoyCol = buoy.GetComponent<Collider>();
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

        // I-apply ang malakas na hagis paharap
        Rigidbody buoyRb = buoy.GetComponent<Rigidbody>();
        if (buoyRb != null)
        {
            // Siguraduhing walang tira-tirang puwersa
            buoyRb.linearVelocity = Vector3.zero;

            // Bawal tumaob
            buoyRb.constraints = RigidbodyConstraints.FreezeRotation;

            // Malakas na hagis sa direksyon ng Camera
            Vector3 forceDirection = (cam.forward * throwForce) + (Vector3.up * throwUpwardForce);
            buoyRb.AddForce(forceDirection, ForceMode.Impulse);
        }
    }
}