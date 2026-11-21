using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public static NPCSpawner instance;

    [Header("NPC Prefab")]
    public GameObject drowningNpcPrefab;  // Ang bagong DrowningNPC_Fixed prefab mo

    [Header("Spawn Count")]
    public int totalNpcToSpawn = 3;       // Ilang tao ang gustong i-spawn

    [Header("Spawn Boundaries (Lawak ng Sakop)")]
    public float minX = -650f;
    public float maxX = -470f;
    public float minZ = 27f;
    public float maxZ = 190f;
    public float spawnHeightY = 1.6f;     // Taas sa ibabaw ng baha

    [Header("Obstacle Avoidance")]
    public float checkRadius = 2.5f;
    public LayerMask obstacleLayer;

    private bool hasSpawned = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // WALA NANG SPAWN SA SIMULA!
        // Hihintayin natin ang tawag mula sa RescueManager.
    }

    // TATAWAGIN ITO KAPAG SUMAKAY NA ANG PLAYER SA BANGKA MULA SA RESCUEMANAGER
    public void SpawnRandomNPCs()
    {
        if (hasSpawned) return; // Iwasan ang pag-doble ng spawn kung sakaling matawag nang doble
        hasSpawned = true;

        if (drowningNpcPrefab == null)
        {
            Debug.LogError("NPCSpawner: Walang nakalagay na Drowning NPC Prefab sa Inspector!");
            return;
        }

        int successfullySpawned = 0;
        int maxAttempts = 100; // Maximum attempts para hindi mag-infinite loop
        int currentAttempt = 0;

        while (successfullySpawned < totalNpcToSpawn && currentAttempt < maxAttempts)
        {
            currentAttempt++;

            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            Vector3 randomPosition = new Vector3(randomX, spawnHeightY, randomZ);

            // Suriin kung mayroong obstacle (gusali, puno, atbp.) sa posisyon na ito
            bool hitObstacle = Physics.CheckSphere(randomPosition, checkRadius, obstacleLayer);

            if (!hitObstacle)
            {
                // Gamitin ang tamang rotation ng nakatayong prefab
                Instantiate(drowningNpcPrefab, randomPosition, drowningNpcPrefab.transform.rotation);
                successfullySpawned++;
                Debug.Log($"NPCSpawner: Nag-spawn ng NPC sa {randomPosition} (Attempt {currentAttempt})");
            }
            else
            {
                // Opsyonal: Debug log kapag may natamaan na obstacle
                // Debug.Log($"NPCSpawner: May nakitang obstacle sa {randomPosition}, nire-retry.");
            }
        }

        if (successfullySpawned < totalNpcToSpawn)
        {
            Debug.LogWarning($"NPCSpawner: Nakapag-spawn lamang ng {successfullySpawned} sa {totalNpcToSpawn} na hiniling. Maaaring masyadongikid ang spawn area o masyadong mataas ang checkRadius.");
        }
        else
        {
            Debug.Log($"SUCCESS: Naka-spawn ng kabuuang {successfullySpawned} drowning NPCs matapos sumakay sa bangka!");
        }

        // --- INALIS NA ANG BAHAGI DITO ---
        // Ang RescueManager na ang bahalang tumawag ng sarili nitong InitializeMissionStats
        // pagkatapos nitong tawagin ang SpawnRandomNPCs().
        // ---------------------------------
    }

    // Para makita mo ang spawn area sa Scene view kapag naka-select ang spawner
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Yellow na may transparency
        Vector3 center = new Vector3((minX + maxX) / 2f, spawnHeightY, (minZ + maxZ) / 2f);
        Vector3 size = new Vector3(Mathf.Abs(maxX - minX), 1f, Mathf.Abs(maxZ - minZ));
        Gizmos.DrawCube(center, size); // Gamitin ang DrawCube para solid ang box
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size); // At outline
    }
}