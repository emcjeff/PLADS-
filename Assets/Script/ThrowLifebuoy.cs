using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro para sa seconds text

public class ThrowLifebuoy : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    public Transform throwPoint;
    public GameObject lifebuoyPrefab;

    [Header("Throw Settings")]
    public float throwForce = 18f;
    public float throwUpwardForce = 3f;

    [Header("Cooldown Settings")]
    public float cooldownTime = 3f;      // Ilang segundo ang cooldown
    private float cooldownTimer = 0f;
    private bool isReady = true;

    [Header("UI Cooldown")]
    public Image cooldownFillImage;      // Ang CooldownIcon
    public TextMeshProUGUI cooldownText; // ANG BAGONG TEXT PARA SA SECONDS (hal. 2.5s)
    public GameObject notReadyPrompt;    // (Optional)

    [Header("Spin Effect")]
    public bool spinLikeFrisbee = true;
    public float spinSpeed = 12f;

    void Start()
    {
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        if (notReadyPrompt != null) notReadyPrompt.SetActive(false);

        // Nakatago ang countdown text sa simula dahil ready na agad
        if (cooldownText != null) cooldownText.gameObject.SetActive(false);
    }

    void Update()
    {
        // 1. Cooldown computation
        if (!isReady)
        {
            cooldownTimer -= Time.deltaTime;

            // I-update ang radial fill bar kung meron
            if (cooldownFillImage != null)
            {
                cooldownFillImage.fillAmount = 1f - (cooldownTimer / cooldownTime);
            }

            // I-UPDATE ANG SECONDS TEXT SA SCREEN
            if (cooldownText != null)
            {
                if (!cooldownText.gameObject.activeSelf) cooldownText.gameObject.SetActive(true);
                // "F1" ibig sabihin may 1 decimal place (halimbawa: 2.8s, 1.4s)
                cooldownText.text = cooldownTimer.ToString("F1") + "s";
            }

            // Kapag tapos na ang cooldown
            if (cooldownTimer <= 0f)
            {
                isReady = true;
                if (cooldownFillImage != null) cooldownFillImage.fillAmount = 1f;
                if (cooldownText != null) cooldownText.gameObject.SetActive(false); // Itago ang text kapag ready na
                if (notReadyPrompt != null) notReadyPrompt.SetActive(false);
            }
        }

        // 2. Pagbato
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F))
        {
            if (isReady)
            {
                Throw();
            }
            else
            {
                if (notReadyPrompt != null) notReadyPrompt.SetActive(true);
            }
        }
    }

    void Throw()
    {
        isReady = false;
        cooldownTimer = cooldownTime;

        if (cooldownFillImage != null)
        {
            cooldownFillImage.fillAmount = 0f;
        }

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(true);
        }

        Vector3 spawnPos = throwPoint != null ? throwPoint.position : cam.position + (cam.forward * 2f);
        Quaternion flatRotation = Quaternion.Euler(0f, cam.eulerAngles.y, 0f);
        GameObject buoy = Instantiate(lifebuoyPrefab, spawnPos, flatRotation);

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

        Rigidbody buoyRb = buoy.GetComponent<Rigidbody>();
        if (buoyRb != null)
        {
            buoyRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            Vector3 forceDirection = (cam.forward * throwForce) + (Vector3.up * throwUpwardForce);
            buoyRb.AddForce(forceDirection, ForceMode.Impulse);

            if (spinLikeFrisbee)
            {
                buoyRb.AddTorque(Vector3.up * spinSpeed, ForceMode.Impulse);
            }
        }
    }
}