using UnityEngine;
using TMPro;

public class RescueRadarArrow : MonoBehaviour
{
    [Header("Target Tracking")]
    public Transform playerOrBoat;       // Ang iyong Bangka o Player
    public float heightAboveBoat = 2.5f; // Taas ng palaso sa ibabaw ng bangka
    public float rotationSpeed = 10f;    // Bilis ng pagpihit ng palaso

    [Header("Visual References")]
    public GameObject arrowVisual;       // Ang 3D model o hugis ng palaso
    public TextMeshPro distanceText;     // (Optional) 3D text para sa distansya (hal. "35m")

    [Header("Colors (Optional)")]
    public Renderer arrowRenderer;       // Material renderer ng palaso
    public Color npcTargetColor = Color.yellow; // Kulay kapag nagtuturo sa tao
    public Color safeZoneColor = Color.green;   // Kulay kapag nagtuturo sa SafeZone

    private Transform currentTarget;

    void Start()
    {
        // Kung walang nakalagay, hanapin ang bangka
        if (playerOrBoat == null)
        {
            BoatController boat = Object.FindAnyObjectByType<BoatController>();
            if (boat != null) playerOrBoat = boat.transform;
        }
    }

    void Update()
    {
        // 1. Huwag magpakita kung hindi pa nagsisimula ang rescue mission
        if (RescueManager.instance != null && !RescueManager.instance.isMissionStarted)
        {
            if (arrowVisual != null) arrowVisual.SetActive(false);
            return;
        }

        // 2. Hanapin ang pinakamalapit na dapat puntahan
        FindBestTarget();

        if (currentTarget == null)
        {
            if (arrowVisual != null) arrowVisual.SetActive(false);
            return;
        }

        if (arrowVisual != null) arrowVisual.SetActive(true);

        // 3. I-pwesto ang palaso sa ibabaw ng bangka
        if (playerOrBoat != null)
        {
            transform.position = playerOrBoat.position + (Vector3.up * heightAboveBoat);
        }

        // 4. Iikot ang palaso papunta sa target (Patag na Y-rotation lang)
        Vector3 targetDir = currentTarget.position - transform.position;
        targetDir.y = 0f; // Bawal tumingala o tumungo para steady ang palaso

        if (targetDir.sqrMagnitude > 0.05f)
        {
            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // 5. I-update ang distansya sa metro
        if (distanceText != null)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.position);
            distanceText.text = $"{Mathf.RoundToInt(dist)}m";

            // Iharap ang numero sa camera para laging mabasa
            if (Camera.main != null)
            {
                distanceText.transform.forward = Camera.main.transform.forward;
            }
        }
    }

    void FindBestTarget()
    {
        // Hanapin ang lahat ng nalulunod sa mapa
        DrowningNPC[] allNPCs = Object.FindObjectsByType<DrowningNPC>(FindObjectsSortMode.None);
        DrowningNPC nearestNPC = null;
        float shortestDistance = Mathf.Infinity;

        Vector3 myPos = playerOrBoat != null ? playerOrBoat.position : transform.position;

        foreach (var npc in allNPCs)
        {
            // Tanging ang mga nasa tubig pa ang ituturo (huwag ang mga nakasakay na)
            if (npc != null && npc.currentState != DrowningNPC.NPCState.Rescued)
            {
                float dist = Vector3.Distance(myPos, npc.transform.position);
                if (dist < shortestDistance)
                {
                    shortestDistance = dist;
                    nearestNPC = npc;
                }
            }
        }

        // KUNG MAY NALULUNOD PA SA TUBIG:
        if (nearestNPC != null)
        {
            currentTarget = nearestNPC.transform;
            if (arrowRenderer != null) arrowRenderer.material.color = npcTargetColor;
        }
        else
        {
            // KUNG NAISAKAY NA ANG LAHAT: Ituro naman ang SafeZone!
            SafeZone safeZone = Object.FindAnyObjectByType<SafeZone>();
            if (safeZone != null)
            {
                currentTarget = safeZone.transform;
                if (arrowRenderer != null) arrowRenderer.material.color = safeZoneColor;
            }
            else
            {
                currentTarget = null;
            }
        }
    }
}