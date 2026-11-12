using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;          // Ang Player
    public float mouseSensitivity = 3f;
    public float distance = 5f;       // Distansya mula sa likod ng player
    public float height = 2f;         // Taas ng camera mula sa player

    private float currentX = 0f;
    private float currentY = 20f;

    void Start()
    {
        // I-lock ang cursor sa gitna ng screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // HUWAG pagalawin ang camera kapag naka-unlock ang cursor (naglalaro sa UI)
        if (Cursor.lockState != CursorLockMode.Locked) return;

        currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
        currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        currentY = Mathf.Clamp(currentY, -10f, 60f);
    }

    // LateUpdate para smooth ang pagsunod pagkatapos gumalaw ng player
    void LateUpdate()
    {
        if (target == null) return;

        // I-calculate ang rotation at posisyon ng camera sa paligid ng player
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 targetPosition = target.position + Vector3.up * height;
        Vector3 position = targetPosition - (rotation * Vector3.forward * distance);

        transform.position = position;
        transform.LookAt(targetPosition);
    }
}