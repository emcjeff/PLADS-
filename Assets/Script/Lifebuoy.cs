using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Lifebuoy : MonoBehaviour
{
    public float lifetime = 15f;
    public float waterLevel = 1.6f;
    public float floatStrength = 8f;

    [HideInInspector] public DrowningNPC hookedNPC; // Kung sino ang na-hook
    [HideInInspector] public bool isHooked = false;

    private Rigidbody rb;
    private float lifeTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Kusang mawawala lang kung HINDI nakakabit sa NPC
        if (!isHooked)
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }

    void FixedUpdate()
    {
        if (isHooked) return;

        if (transform.position.y < waterLevel)
        {
            float depth = waterLevel - transform.position.y;
            rb.AddForce(Vector3.up * depth * floatStrength, ForceMode.Acceleration);
            rb.linearDamping = 2f;
        }
    }

    // Tinatawag ng DrowningNPC kapag kumapit na ang hook
    public void AttachToNPC(DrowningNPC npc)
    {
        isHooked = true;
        hookedNPC = npc;

        // Patayin ang physics at collider ng salbabida para smooth ang paghatak papasok sa bangka
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
}