using UnityEngine;

public class TutorialBeacon : MonoBehaviour
{
    public float bobSpeed = 3f;
    public float bobHeight = 0.4f;
    public float rotateSpeed = 90f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        // Tataas-baba at iikot-ikot sa ibabaw ng bangka para kitang-kita!
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }
}