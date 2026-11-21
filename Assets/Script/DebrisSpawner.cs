using UnityEngine;

public class DebrisSpawner : MonoBehaviour
{
    public static DebrisSpawner instance;

    [Header("Debris Prefabs")]
    [Tooltip("I-drag dito ang lahat ng ginawa mong Debris Prefabs (basag_bote, barrel, kahoy, etc.)")]
    public GameObject[] debrisPrefabs;

    [Header("Spawn Settings")]
    public int totalDebrisToSpawn = 15;      // Ilang pirasong kalat/debris ang lilitaw
    public float spawnHeightY = 1.6f;        // Taas ng tubig-baha
    public float checkRadius = 1.2f;         // Iwasan ang mga pader ng bahay
    public LayerMask obstacleLayer;          // Layer ng mga gusali

    [Header("Boundaries (Parehas sa NPC Spawner)")]
    public float minX = -650f;
    public float maxX = -470f;
    public float minZ = 27f;
    public float maxZ = 190f;

    private bool hasSpawned = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    // TATAWAGIN ITO KAPAG SUMAKAY NA SA BANGKA
    public void SpawnAllDebris()
    {
        if (hasSpawned) return;
        hasSpawned = true;

        if (debrisPrefabs == null || debrisPrefabs.Length == 0)
        {
            Debug.LogWarning("DebrisSpawner: Walang nakalagay na Debris Prefabs!");
            return;
        }

        int successfullySpawned = 0;
        int maxAttempts = totalDebrisToSpawn * 10;
        int currentAttempt = 0;

        while (successfullySpawned < totalDebrisToSpawn && currentAttempt < maxAttempts)
        {
            currentAttempt++;

            // 1. Pumili ng random coordinate sa loob ng binahang kalsada
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            Vector3 spawnPosition = new Vector3(randomX, spawnHeightY, randomZ);

            // 2. Tiyaking hindi sa loob ng bahay mag-i-spawn
            bool hitObstacle = Physics.CheckSphere(spawnPosition, checkRadius, obstacleLayer);

            if (!hitObstacle)
            {
                // 3. Random na pumili kung anong basura ang lilitaw (bote, barrel, kahoy, etc.)
                GameObject chosenPrefab = debrisPrefabs[Random.Range(0, debrisPrefabs.Length)];

                // Random na ikot (Y-rotation) para natural at hindi pare-pareho ang itsura
                Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                Instantiate(chosenPrefab, spawnPosition, randomRotation);
                successfullySpawned++;
            }
        }

        Debug.Log($"Naka-spawn ng {successfullySpawned} random debris sa baha!");
    }

    // Makikita ang cyan na kahon sa Scene view kapag naka-select
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minX + maxX) / 2f, spawnHeightY, (minZ + maxZ) / 2f);
        Vector3 size = new Vector3(Mathf.Abs(maxX - minX), 1f, Mathf.Abs(maxZ - minZ));
        Gizmos.DrawWireCube(center, size);
    }
}