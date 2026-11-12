using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public static NPCSpawner instance;

    [Header("NPC Prefab")]
    public GameObject drowningNpcPrefab;  // Ang Prefab ng taong nalulunod

    [Header("Spawn Count")]
    public int totalNpcToSpawn = 3;       // Ilang tao ang gustong i-spawn

    [Header("Spawn Boundaries (Lawak ng Sakop)")]
    public float minX = -600f;            // Pinaka-kaliwa
    public float maxX = -500f;            // Pinaka-kanan
    public float minZ = 20f;              // Pinaka-ibaba
    public float maxZ = 120f;             // Pinaka-itaas
    public float spawnHeightY = 0.8f;     // Taas kung saan lilitaw (pantay sa tubig)

    [Header("Obstacle Avoidance")]
    public float checkRadius = 1.2f;      // Layo ng titingnan para makasigurong walang pader o bahay
    public LayerMask obstacleLayer;       // Layer ng mga bahay at safezone

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        SpawnRandomNPCs();
    }

    public void SpawnRandomNPCs()
    {
        if (drowningNpcPrefab == null)
        {
            Debug.LogWarning("NPCSpawner: Walang nakalagay na Drowning NPC Prefab!");
            return;
        }

        int successfullySpawned = 0;
        int maxAttempts = 100; // Proteksyon para hindi mag-loop nang walang hanggan
        int currentAttempt = 0;

        while (successfullySpawned < totalNpcToSpawn && currentAttempt < maxAttempts)
        {
            currentAttempt++;

            // 1. Kumuha ng Random coordinate sa loob ng Range
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            Vector3 randomPosition = new Vector3(randomX, spawnHeightY, randomZ);

            // 2. I-check kung may nakabara (Bahay, Pader, atbp.) gamit ang CheckSphere
            bool hitObstacle = Physics.CheckSphere(randomPosition, checkRadius, obstacleLayer);

            // Kung malinis ang pwesto at walang bahay:
            if (!hitObstacle)
            {
                Instantiate(drowningNpcPrefab, randomPosition, Quaternion.identity);
                successfullySpawned++;
            }
        }

        Debug.Log($"Matagumpay na naka-spawn ng {successfullySpawned} random NPCs sa loob ng Range!");
    }

    // GIZMOS: Para makita mo ang kulay dilaw na kahon sa Scene view habang inaayos mo ang range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minX + maxX) / 2f, spawnHeightY, (minZ + maxZ) / 2f);
        Vector3 size = new Vector3(Mathf.Abs(maxX - minX), 1f, Mathf.Abs(maxZ - minZ));
        Gizmos.DrawWireCube(center, size);
    }
}