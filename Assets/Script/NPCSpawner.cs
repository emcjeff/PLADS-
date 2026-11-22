using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public static NPCSpawner instance;

    [Header("NPC Prefab")]
    public GameObject drowningNpcPrefab;  // Ang DrowningNPC_Fixed prefab mo

    [Header("Spawn Count")]
    public int totalNpcToSpawn = 3;       // Bilang ng tao (hal. 3 sa Level 1, 5 sa Level 2)

    [Header("Spawn Boundaries (Lawak ng Sakop)")]
    public float minX = -650f;
    public float maxX = -470f;
    public float minZ = 27f;
    public float maxZ = 190f;
    public float spawnHeightY = 1.6f;     // Taas sa ibabaw ng baha

    [Header("Obstacle Avoidance")]
    [Tooltip("Inirekomenda: 0.6 hanggang 0.8 para kasing-sukat lang ng tao at hindi humarang sa buong kalsada.")]
    public float checkRadius = 0.7f;      // Pinaliliit para makapasok sa mga kalsada
    public LayerMask obstacleLayer;       // Layer ng mga Bahay/Obstacles

    private bool hasSpawned = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // Walang spawn sa simula; mag-i-spawn kapag sumakay na sa bangka
    }

    // TATAWAGIN ITO NG RESCUEMANAGER KAPAG SUMAKAY SA BANGKA
    public void SpawnRandomNPCs()
    {
        if (hasSpawned) return;
        hasSpawned = true;

        if (drowningNpcPrefab == null)
        {
            Debug.LogError("NPCSpawner: Walang nakalagay na Drowning NPC Prefab sa Inspector!");
            return;
        }

        int successfullySpawned = 0;
        int maxAttempts = totalNpcToSpawn * 60; // 300 attempts para sigurado ang paghahanap
        int currentAttempt = 0;

        while (successfullySpawned < totalNpcToSpawn && currentAttempt < maxAttempts)
        {
            currentAttempt++;

            // 1. Kumuha ng random coordinate sa loob ng boundary
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            Vector3 randomPosition = new Vector3(randomX, spawnHeightY, randomZ);

            // 2. Suriin kung may nakaharang na bahay o pader
            bool hitObstacle = false;
            if (obstacleLayer != 0)
            {
                hitObstacle = Physics.CheckSphere(randomPosition, checkRadius, obstacleLayer);
            }

            // 3. Kung malinis at walang bahay, i-spawn ang NPC
            if (!hitObstacle)
            {
                Instantiate(drowningNpcPrefab, randomPosition, drowningNpcPrefab.transform.rotation);
                successfullySpawned++;
            }
        }

        if (successfullySpawned < totalNpcToSpawn)
        {
            Debug.LogWarning($"NPCSpawner: Nakapag-spawn lamang ng {successfullySpawned} sa {totalNpcToSpawn}. Subukang gawing 0.6 ang checkRadius o palawakin ang Min/Max boundary.");
        }
        else
        {
            Debug.Log($"SUCCESS: Naka-spawn ng kabuuang {successfullySpawned} drowning NPCs!");
        }

        // Tiyaking ma-update agad ang stats sa RescueManager
        if (RescueManager.instance != null)
        {
            RescueManager.instance.InitializeMissionStats();
        }
    }

    // Para makita ang dilaw na kahon sa Scene view kapag naka-click ang Spawner
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
        Vector3 center = new Vector3((minX + maxX) / 2f, spawnHeightY, (minZ + maxZ) / 2f);
        Vector3 size = new Vector3(Mathf.Abs(maxX - minX), 1f, Mathf.Abs(maxZ - minZ));
        Gizmos.DrawCube(center, size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);
    }
}